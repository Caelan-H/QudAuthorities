using System;
using ConsoleLib.Console;
using Kobold;
using Qud.UI;
using UnityEngine;

namespace XRL.UI.Framework;

public class TitledBigIconButton : MonoBehaviour, IFrameworkControl
{
	public void setData(FrameworkDataElement data)
	{
		if (!(data is ChoiceWithColorIcon choiceWithColorIcon))
		{
			throw new ArgumentException("TitledBigIconButton expected ChoiceWithColorIcon data");
		}
		ImageTinyFrame obj = GetComponent<ImageTinyFrame>() ?? GetComponent<TitledIconButton>().ImageTinyFrame;
		GetComponent<TitledIconButton>().SetTitle(choiceWithColorIcon.Title + (string.IsNullOrEmpty(choiceWithColorIcon.Hotkey) ? "" : ("\n" + choiceWithColorIcon.Hotkey)));
		obj.sprite = SpriteManager.GetUnitySprite(choiceWithColorIcon.IconPath);
		obj.unselectedBorderColor = (choiceWithColorIcon.IsChosen() ? ConsoleLib.Console.ColorUtility.ColorMap['C'] : ConsoleLib.Console.ColorUtility.ColorMap['K']);
		obj.selectedBorderColor = (choiceWithColorIcon.IsChosen() ? ConsoleLib.Console.ColorUtility.ColorMap['W'] : ConsoleLib.Console.ColorUtility.ColorMap['W']);
		obj.selectedForegroundColor = choiceWithColorIcon.IconForegroundColor;
		obj.selectedDetailColor = choiceWithColorIcon.IconDetailColor;
		obj.Sync(force: true);
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
