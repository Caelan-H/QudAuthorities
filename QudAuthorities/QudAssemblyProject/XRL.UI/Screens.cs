using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World;

namespace XRL.UI;

public class Screens
{
	public static Screens _Screens = new Screens();

	public static int CurrentScreen = 0;

	private static List<IScreen> ScreenList;

	private static Dictionary<string, IScreen> PopupScreens;

	public static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	private Screens()
	{
		ScreenList = new List<IScreen>();
		ScreenList.Add(new SkillsAndPowersScreen());
		ScreenList.Add(new StatusScreen());
		ScreenList.Add(new InventoryScreen());
		ScreenList.Add(new EquipmentScreen());
		ScreenList.Add(new FactionsScreen());
		ScreenList.Add(new QuestLog());
		ScreenList.Add(new JournalScreen());
		ScreenList.Add(new TinkeringScreen());
		PopupScreens = new Dictionary<string, IScreen>();
		PopupScreens.Add("Factions", new FactionsScreen());
	}

	public static void ShowPopup(string Screen, GameObject GO)
	{
		PopupScreens[Screen].Show(GO);
	}

	public static void Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("StatusScreens");
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		ScreenReturn screenReturn = ScreenReturn.Next;
		while ((screenReturn = ScreenList[CurrentScreen].Show(GO)) != ScreenReturn.Exit)
		{
			if (screenReturn == ScreenReturn.Next)
			{
				CurrentScreen++;
			}
			if (screenReturn == ScreenReturn.Previous)
			{
				CurrentScreen--;
			}
			if (CurrentScreen < 0)
			{
				CurrentScreen = ScreenList.Count - 1;
			}
			if (CurrentScreen > ScreenList.Count - 1)
			{
				CurrentScreen = 0;
			}
		}
		Popup._TextConsole.DrawBuffer(OldBuffer);
		GameManager.Instance.PopGameView();
	}
}
