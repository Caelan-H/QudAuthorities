using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Mutations", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/PickMutations", UICanvasHost = 1)]
public class QudGenotypeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudGenotypeModule, HorizontalScroller>
{
	public override void RandomSelection()
	{
		int index = Stat.Random(0, base.prefabComponent.choices.Count - 1);
		base.prefabComponent.scrollContext.GetContextAt(index).Activate();
		base.module.SelectGenotype(base.prefabComponent.choices[index].Id);
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (GenotypeEntry genotype in base.module.genotypes)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = genotype.Name,
				Title = genotype.DisplayName,
				IconPath = genotype.Tile,
				IconDetailColor = (genotype.DetailColor.IsNullOrEmpty() ? ConsoleLib.Console.ColorUtility.ColorMap['w'] : ConsoleLib.Console.ColorUtility.ColorMap[genotype.DetailColor[0]]),
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y'],
				Description = genotype.GetFlatChargenInfo(),
				Chosen = IsChoiceSelected
			};
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return base.module.getSelected() == choice?.Id;
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = GetType().FullName,
			Title = "Choose Genotype",
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
		};
		foreach (GenotypeEntry genotype in base.module.genotypes)
		{
			if (genotype.Name == base.module.getSelected())
			{
				uIBreadcrumb.Title = genotype.DisplayName;
				uIBreadcrumb.IconPath = genotype.Tile;
				uIBreadcrumb.IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap['b'];
				uIBreadcrumb.IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y'];
				return uIBreadcrumb;
			}
		}
		return uIBreadcrumb;
	}

	public void onSelectGenotype(FrameworkDataElement choice)
	{
		base.module.SelectGenotype(choice.Id);
		base.module.builder.advance();
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		base.prefabComponent.autoHotkey = true;
		base.prefabComponent.scrollContext.wraps = false;
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(onSelectGenotype);
		base.prefabComponent.BeforeShow(descriptor, GetSelections());
		base.BeforeShow(descriptor);
	}
}
