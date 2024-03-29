using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Subtype", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/PickSubtype", UICanvasHost = 1)]
public class QudSubtypeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudSubtypeModule, HorizontalScroller>
{
	public override bool enabled => !base.module.hasCategories();

	public override void RandomSelection()
	{
		int index = Stat.Random(0, base.prefabComponent.choices.Count - 1);
		base.prefabComponent.scrollContext.GetContextAt(index).Activate();
		base.module.SelectSubtype(base.prefabComponent.choices[index].Id);
	}

	public void onSelectSubtype(FrameworkDataElement choice)
	{
		base.module.SelectSubtype(choice.Id);
		base.module.builder.advance();
	}

	public string getSubtypeTitle()
	{
		return base.module.getAvailableSelections().FirstOrDefault()?.SubtypeClass.ChargenTitle ?? "choose subtype";
	}

	public string getSubtypeSingularTitle()
	{
		return base.module.getAvailableSelections().FirstOrDefault()?.SubtypeClass.SingluarTitle ?? "subtype";
	}

	public virtual IEnumerable<FrameworkDataElement> GetSelections()
	{
		return base.module.GetSelections();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		base.prefabComponent.autoHotkey = true;
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(onSelectSubtype);
		HorizontalScrollerScroller scrollscroller = base.prefabComponent as HorizontalScrollerScroller;
		if (scrollscroller != null)
		{
			scrollscroller.onHighlight.RemoveAllListeners();
			scrollscroller.onHighlight.AddListener(delegate
			{
				scrollscroller.UpdateHighlightText();
			});
		}
		base.prefabComponent.BeforeShow(descriptor, GetSelections());
		base.prefabComponent.titleText.SetText(":" + getSubtypeTitle() + ":");
		base.BeforeShow(descriptor);
	}

	public override IEnumerable<MenuOption> GetKeyLegend()
	{
		foreach (MenuOption item in base.GetKeyLegend())
		{
			yield return item;
		}
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = Grammar.InitCapWithFormatting(getSubtypeSingularTitle()),
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
		};
		foreach (SubtypeEntry subtype in base.module.subtypes)
		{
			if (subtype.Name == base.module.getSelected())
			{
				uIBreadcrumb.Title = subtype.DisplayName;
				uIBreadcrumb.IconPath = subtype.Tile;
				uIBreadcrumb.IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap['b'];
				uIBreadcrumb.IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y'];
				return uIBreadcrumb;
			}
		}
		return uIBreadcrumb;
	}
}
