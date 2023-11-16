using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Pregen", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/PickSubtype", UICanvasHost = 1)]
public class QudPregenModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudPregenModule, HorizontalScroller>
{
	public override void RandomSelection()
	{
		base.prefabComponent.scrollContext.GetContextAt(Stat.Random(0, base.prefabComponent.choices.Count - 1)).Activate();
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		string genotype = GetModule<QudGenotypeModule>().data.Genotype;
		IEnumerable<QudPregenModule.QudPregenData> enumerable = base.module.pregens.Values.Where((QudPregenModule.QudPregenData p) => p.Genotype == genotype);
		foreach (QudPregenModule.QudPregenData item in enumerable)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = item.Name,
				Title = item.Name,
				IconPath = item.Tile,
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Detail[0]],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[item.Foreground[0]],
				Description = item.Description
			};
		}
	}

	public void onSelectSubtype(FrameworkDataElement choice)
	{
		base.module.SelectPregen(choice.Id);
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		base.prefabComponent.autoHotkey = true;
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(onSelectSubtype);
		base.prefabComponent.BeforeShow(descriptor, GetSelections());
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
		UIBreadcrumb obj = new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Pregens"
		};
		object iconPath;
		if (base.module?.data?.Pregen != null)
		{
			QudPregenModule qudPregenModule = base.module;
			if (qudPregenModule != null && qudPregenModule.pregens.ContainsKey(base.module?.data?.Pregen))
			{
				iconPath = base.module?.pregens[base.module?.data?.Pregen]?.Tile;
				goto IL_00ca;
			}
		}
		iconPath = UIBreadcrumb.DEFAULT_ICON ?? UIBreadcrumb.DEFAULT_ICON;
		goto IL_00ca;
		IL_00ca:
		obj.IconPath = (string)iconPath;
		obj.IconDetailColor = Color.clear;
		obj.IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y'];
		return obj;
	}
}
