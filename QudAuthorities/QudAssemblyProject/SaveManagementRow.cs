using System.Collections.Generic;
using ConsoleLib.Console;
using Kobold;
using Qud.API;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

public class SaveManagementRow : MonoBehaviour, IFrameworkControl
{
	public ImageTinyFrame imageTinyFrame;

	public List<UITextSkin> TextSkins;

	public Image background;

	public GameObject modsDiffer;

	public FrameworkContext deleteButton;

	private FrameworkContext _context;

	private bool? wasSelected;

	public FrameworkContext context => _context ?? (_context = GetComponent<FrameworkContext>());

	public void setData(FrameworkDataElement data)
	{
		FrameworkContext frameworkContext = deleteButton;
		if (frameworkContext.context == null)
		{
			NavigationContext navigationContext2 = (frameworkContext.context = new NavigationContext());
		}
		deleteButton.context.parentContext = context.context;
		if (data is SaveInfoData saveInfoData)
		{
			SaveGameJSON saveGameJSON = saveInfoData.SaveGame?.json;
			if (saveGameJSON != null)
			{
				ImageTinyFrame obj = imageTinyFrame;
				obj.sprite = SpriteManager.GetUnitySprite(saveGameJSON.CharIcon);
				obj.unselectedBorderColor = ConsoleLib.Console.ColorUtility.ColorMap['K'];
				obj.selectedBorderColor = ConsoleLib.Console.ColorUtility.ColorMap['W'];
				obj.unselectedForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap['K'];
				obj.unselectedDetailColor = ConsoleLib.Console.ColorUtility.ColorMap['K'];
				obj.selectedForegroundColor = ConsoleLib.Console.ColorUtility.ColorMap[saveGameJSON.FColor];
				obj.selectedDetailColor = ConsoleLib.Console.ColorUtility.ColorMap[saveGameJSON.DColor];
				obj.Sync(force: true);
			}
			else
			{
				ImageTinyFrame obj2 = imageTinyFrame;
				obj2.sprite = SpriteManager.GetUnitySprite("Text/32.bmp");
				obj2.unselectedBorderColor = ConsoleLib.Console.ColorUtility.ColorMap['K'];
				obj2.selectedBorderColor = ConsoleLib.Console.ColorUtility.ColorMap['W'];
				obj2.unselectedForegroundColor = Color.clear;
				obj2.unselectedDetailColor = Color.clear;
				obj2.selectedForegroundColor = Color.clear;
				obj2.selectedDetailColor = Color.clear;
				obj2.Sync(force: true);
			}
			TextSkins[0].SetText("{{W|" + saveInfoData.SaveGame.Name + " :: " + saveInfoData.SaveGame.Description + " }}");
			TextSkins[1].SetText("{{C|Location:}} " + saveInfoData.SaveGame.Info);
			TextSkins[2].SetText("{{C|Last saved:}} " + saveInfoData.SaveGame.SaveTime);
			TextSkins[3].SetText("{{K|" + saveInfoData.SaveGame.Size + " {" + saveInfoData.SaveGame.ID + "} }}");
			modsDiffer.SetActive(saveInfoData.SaveGame.DifferentMods());
			wasSelected = null;
			Update();
		}
	}

	public void Update()
	{
		bool valueOrDefault = (context?.context?.IsActive()).GetValueOrDefault();
		if (valueOrDefault == wasSelected)
		{
			return;
		}
		wasSelected = valueOrDefault;
		deleteButton.gameObject.SetActive(valueOrDefault);
		Color color = ConsoleLib.Console.ColorUtility.ColorMap['c'];
		color.a = (valueOrDefault ? 0.25f : 0f);
		background.color = color;
		bool flag = true;
		foreach (UITextSkin textSkin in TextSkins)
		{
			if (valueOrDefault)
			{
				textSkin.color = ConsoleLib.Console.ColorUtility.ColorMap['y'];
				textSkin.StripFormatting = false;
			}
			else
			{
				textSkin.color = (flag ? ConsoleLib.Console.ColorUtility.ColorMap['c'] : ConsoleLib.Console.ColorUtility.ColorMap['K']);
				textSkin.StripFormatting = true;
			}
			textSkin.Apply();
			flag = false;
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return null;
	}
}
