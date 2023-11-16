using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.CharacterBuilds.UI;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds.Qud.UI;

[UIView("CharacterCreation:Modes", false, false, false, null, null, false, 0, false, NavCategory = "Menu", UICanvas = "Chargen/Modes", UICanvasHost = 1)]
public class QudGamemodeModuleWindow : EmbarkBuilderModuleWindowPrefabBase<QudGamemodeModule, HorizontalScroller>
{
	public void Update()
	{
		if (Options.ShowQuickstartOption && Input.GetKeyDown(UnityEngine.KeyCode.Q))
		{
			base.module.SelectMode("_Quickstart");
		}
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (QudGamemodeModule.GameModeDescriptor value in base.module.GameModes.Values)
		{
			yield return new ChoiceWithColorIcon
			{
				Id = value.ID,
				Title = value.Title,
				IconPath = value.IconTile,
				IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconDetail[0]],
				IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconForeground[0]],
				Description = value.Description.Replace("{day_of_year}", DateTime.Now.DayOfYear.ToString()).Replace("{year}", DateTime.Now.Year.ToString()),
				Chosen = IsChoiceSelected
			};
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return choice?.Id == base.module.GetMode();
	}

	public override UIBreadcrumb GetBreadcrumb()
	{
		UIBreadcrumb uIBreadcrumb = new UIBreadcrumb
		{
			Id = "Gamemode",
			Title = "Choose Game Mode",
			IconPath = UIBreadcrumb.DEFAULT_ICON,
			IconDetailColor = Color.clear,
			IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['y']
		};
		foreach (QudGamemodeModule.GameModeDescriptor value in base.module.GameModes.Values)
		{
			if (value.ID == base.module.GetMode())
			{
				uIBreadcrumb.Title = value.Title;
				uIBreadcrumb.IconPath = value.IconTile;
				uIBreadcrumb.IconDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconDetail[0]];
				uIBreadcrumb.IconForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[value.IconForeground[0]];
				return uIBreadcrumb;
			}
		}
		return uIBreadcrumb;
	}

	public override IEnumerable<MenuOption> GetKeyMenuBar()
	{
		foreach (MenuOption item in base.GetKeyMenuBar())
		{
			yield return item;
		}
		if (Options.ShowQuickstartOption)
		{
			yield return new MenuOption
			{
				Id = AbstractBuilderModuleWindowBase.RANDOM,
				InputCommand = "CmdDebugQuickstart",
				KeyDescription = "Q",
				Description = "[Debug] Quickstart"
			};
		}
	}

	public override void RandomSelection()
	{
		base.prefabComponent.scrollContext.GetContextAt(Stat.Random(0, base.prefabComponent.choices.Count - 1)).Activate();
	}

	public void ChoiceSelected(FrameworkDataElement data)
	{
		base.module.SelectMode(data.Id);
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor)
	{
		base.prefabComponent.autoHotkey = true;
		base.prefabComponent.onSelected.RemoveAllListeners();
		base.prefabComponent.onSelected.AddListener(ChoiceSelected);
		base.prefabComponent.BeforeShow(descriptor, GetSelections());
		base.BeforeShow(descriptor);
	}
}
