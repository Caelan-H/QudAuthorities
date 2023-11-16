using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:PickMutations", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/PickMutations", UICanvasHost = 1)]
public class QudMutationsModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudMutationsModule, CategoryMenusScroller>
{
	[Serializable]
	public class MLNode
	{
		public MutationCategory Category;

		public MutationEntry Entry;

		public bool bExpand;

		public MLNode ParentNode;

		public int Selected;

		public int Variant;

		public List<MLNode> nodes;

		public bool Valid()
		{
			if (Entry == null)
			{
				return true;
			}
			foreach (MLNode node in nodes)
			{
				if (node.Entry != null && node.Selected > 0 && !Entry.OkWith(node.Entry))
				{
					return false;
				}
			}
			return true;
		}
	}

	private int nSelected;

	public EmbarkBuilderModuleWindowDescriptor windowDescriptor;

	public bool highlightedHasVariant;

	private MLNode selectedNode;

	protected const string EMPTY_CHECK = "[ ]";

	protected const string CHECKED = "[■]";

	private StringBuilder sb = new StringBuilder();

	private List<CategoryMenuData> categoryMenus = new List<CategoryMenuData>();

	public static readonly string EID_GET_BASE_MP = "GetBaseMP";

	public static readonly string EID_GET_CATEGORIES = "GetMutationCategories";

	private List<MLNode> mutationNodes = new List<MLNode>();

	private const string VARIANT_SELECT = "Variant";

	public const string SHOW_POINTS = "ShowPoints";

	public override void ResetSelection()
	{
		QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
		qudMutationsModuleData.mp = (int)base.module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
		if (qudMutationsModuleData.mp == 0)
		{
			base.module.setData(null);
			return;
		}
		base.module.setData(qudMutationsModuleData);
		UpdateNodesFromData();
		UpdateControls();
	}

	public void UpdateDataFromNodes()
	{
		QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
		qudMutationsModuleData.mp = (int)base.module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
		foreach (MLNode mutationNode in mutationNodes)
		{
			if (mutationNode.Selected > 0)
			{
				qudMutationsModuleData.mp -= mutationNode.Selected * mutationNode.Entry.Cost;
				QudMutationModuleDataRow qudMutationModuleDataRow = new QudMutationModuleDataRow();
				qudMutationModuleDataRow.Mutation = mutationNode.Entry.DisplayName;
				qudMutationModuleDataRow.Count = mutationNode.Selected;
				qudMutationModuleDataRow.Variant = mutationNode.Variant;
				qudMutationsModuleData.selections.Add(qudMutationModuleDataRow);
			}
		}
		base.module.setData(qudMutationsModuleData);
	}

	public void UpdateNodesFromData()
	{
		ClearNodes();
		int num = 0;
		foreach (QudMutationModuleDataRow row in base.module.data.selections)
		{
			MLNode mLNode = mutationNodes.Find((MLNode n) => n.Entry != null && n.Entry.DisplayName == row.Mutation);
			mLNode.Selected = row.Count;
			mLNode.Variant = row.Variant;
			num += mLNode.Selected * mLNode.Entry.Cost;
		}
	}

	public void UpdateControls()
	{
		foreach (PrefixMenuOption menu in categoryMenus.SelectMany((CategoryMenuData category) => category.menuOptions))
		{
			MLNode mLNode = mutationNodes?.Find((MLNode n) => n?.Entry?.DisplayName == menu.Id);
			menu.Prefix = FormatNodePrefix(mLNode);
			menu.Description = FormatNodeDescription(mLNode, mLNode.Entry);
		}
		base.prefabComponent.BeforeShow(windowDescriptor, categoryMenus);
		base.prefabComponent.onHighlight.AddListener(HighlightMutation);
		GetOverlayWindow().UpdateMenuBars(windowDescriptor);
	}

	public override void RandomSelection()
	{
		while (base.module.data.mp > 0)
		{
			mutationNodes.Where((MLNode m) => m.Entry != null && m.Entry.Cost <= base.module.data.mp && m.Selected < m.Entry.Maximum && m.Valid()).GetRandomElement().Selected++;
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	public void HighlightMutation(FrameworkDataElement dataElement)
	{
		MLNode mLNode = mutationNodes.Find((MLNode e) => e?.Entry?.DisplayName == dataElement?.Id);
		if (mLNode != null)
		{
			selectedNode = mLNode;
			highlightedHasVariant = mLNode.Entry?.HasVariants() ?? false;
			GetOverlayWindow().UpdateMenuBars(GetOverlayWindow().currentWindowDescriptor);
		}
	}

	public void SelectMutation(FrameworkDataElement dataElement)
	{
		MLNode mLNode = mutationNodes.Find((MLNode e) => e?.Entry?.DisplayName == dataElement?.Id);
		if (mLNode != null)
		{
			if (mLNode.Selected < mLNode.Entry.Maximum && mLNode.Entry.Cost <= base.module.data.mp && mLNode.Valid())
			{
				mLNode.Selected++;
			}
			else if (mLNode.Selected > 0)
			{
				mLNode.Selected = 0;
			}
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	private string FormatNodeDescription(MLNode node, MutationEntry entry)
	{
		sb.Clear();
		sb.Append(entry.DisplayName);
		sb.Append(entry.HasVariants() ? " [{{W|V}}]" : "");
		if (node.Variant > 0)
		{
			sb.Append(" (");
			sb.Append(entry?.Mutation?.GetVariants()?[node.Variant] ?? ("(unknown #" + node.Variant + ")"));
			sb.Append(")");
		}
		return sb.ToString();
	}

	private string FormatNodePrefix(MLNode node)
	{
		bool flag = node.Selected > 0;
		bool num = (!node.Valid() || node.Entry.Cost > base.module.data.mp) && !flag;
		sb.Clear();
		if (num)
		{
			sb.Append("{{K|");
		}
		if (node.Selected == 0)
		{
			sb.Append("[ ]");
		}
		else if (node.Selected == 1 && node.Entry.Maximum == 1)
		{
			sb.Append("[■]");
		}
		else
		{
			sb.Append("[").Append(node.Selected).Append("]");
		}
		sb.Append("[{{");
		sb.Append((node.Entry.Cost > 0) ? "G" : "R");
		sb.Append("|").Append(node.Entry.Cost).Append("}}]");
		if (num)
		{
			sb.Append("}}");
		}
		return sb.ToString();
	}

	private PrefixMenuOption makeMenuOption(MutationEntry entry)
	{
		BaseMutation baseMutation = entry.CreateInstance();
		MLNode node = mutationNodes.Find((MLNode n) => n?.Entry?.DisplayName == entry?.DisplayName);
		return new PrefixMenuOption
		{
			Id = entry.DisplayName,
			Prefix = FormatNodePrefix(node),
			Description = FormatNodeDescription(node, entry),
			LongDescription = ((baseMutation != null) ? (baseMutation.GetDescription() + "\n\n" + baseMutation.GetLevelText(1)) : "???"),
			Renderable = entry?.GetRenderable()
		};
	}

	public void ClearNodes()
	{
		mutationNodes = new List<MLNode>();
		categoryMenus = new List<CategoryMenuData>();
		List<string> list = base.module.builder.handleUIEvent(EID_GET_CATEGORIES) as List<string>;
		foreach (MutationCategory category in MutationFactory.GetCategories())
		{
			if (list != null && !list.Contains(category.Name))
			{
				continue;
			}
			CategoryMenuData categoryMenuData = new CategoryMenuData();
			categoryMenus.Add(categoryMenuData);
			categoryMenuData.Title = category.DisplayName;
			categoryMenuData.menuOptions = new List<PrefixMenuOption>();
			MLNode mLNode = new MLNode();
			mLNode.bExpand = false;
			mLNode.Category = category;
			mLNode.nodes = mutationNodes;
			mutationNodes.Add(mLNode);
			foreach (MutationEntry entry in category.Entries)
			{
				MLNode mLNode2 = new MLNode();
				mLNode2.ParentNode = mLNode;
				mLNode2.Entry = entry;
				mLNode2.nodes = mutationNodes;
				mutationNodes.Add(mLNode2);
			}
			categoryMenuData.menuOptions.AddRange(category.Entries.Select(makeMenuOption));
		}
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		if (descriptor != null)
		{
			windowDescriptor = descriptor;
		}
		if (base.module.data == null || (base.module.data.mp == 0 && base.module.data.selections.Count == 0))
		{
			QudMutationsModuleData qudMutationsModuleData = new QudMutationsModuleData();
			qudMutationsModuleData.mp = (int)base.module.builder.handleUIEvent(EID_GET_BASE_MP, 0);
			base.module.setData(qudMutationsModuleData);
		}
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(SelectMutation);
		UpdateNodesFromData();
		UpdateControls();
	}

	public override IEnumerable<MenuOption> GetKeyLegend()
	{
		foreach (MenuOption item in base.GetKeyLegend())
		{
			yield return item;
		}
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		yield return new MenuOption
		{
			Id = "ShowPoints",
			InputCommand = "",
			KeyDescription = null,
			Description = ((base.module.data.mp < 0) ? ("{{R|Points Remaining: " + base.module.data.mp + "}}") : ("{{y|Points Remaining: " + base.module.data.mp + "}}"))
		};
		if (highlightedHasVariant)
		{
			yield return new MenuOption
			{
				Id = "Variant",
				InputCommand = "CmdChargenMutationVariant",
				KeyDescription = ControlManager.getCommandInputDescription("CmdChargenMutationVariant"),
				Description = "Choose Variant"
			};
		}
		foreach (MenuOption item in base.GetKeyMenuBar())
		{
			yield return item;
		}
	}

	public override void HandleMenuOption(MenuOption menuOption)
	{
		if (menuOption.Id == "ShowPoints")
		{
			string text = base.module.GetSummaryBlock()?.Description;
			if (!string.IsNullOrEmpty(text))
			{
				Popup.NewPopupMessageAsync(text, PopupMessage.AcceptButton, null, "Points Remaining: " + base.module.data.mp).Start();
			}
		}
		else if (menuOption.Id == "Variant")
		{
			SelectVariant();
		}
		else
		{
			base.HandleMenuOption(menuOption);
		}
	}

	public async void SelectVariant()
	{
		int num = await Popup.ShowOptionListAsync("Customize this mutation", selectedNode.Entry.CreateInstance().GetVariants().ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
		if (num >= 0)
		{
			selectedNode.Variant = num;
			UpdateDataFromNodes();
			UpdateControls();
		}
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		return new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Mutations",
			IconPath = "Items/sw_horns.bmp",
			IconDetailColor = Color.clear,
			IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
		};
	}
}
