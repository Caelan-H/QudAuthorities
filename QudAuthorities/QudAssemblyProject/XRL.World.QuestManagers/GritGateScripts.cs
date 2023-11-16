using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.QuestManagers;

[Serializable]
public class GritGateScripts : QuestManager
{
	public static bool IsMerchant()
	{
		GameObject player = The.Player;
		if (player != null && player.Inventory?.HasObject("Merchant's Token") == true)
		{
			return true;
		}
		GameObject player2 = The.Player;
		if (player2 != null && player2.Body?.HasEquippedItem("Merchant's Token") == true)
		{
			return true;
		}
		return false;
	}

	public static void PromoteToApprentice()
	{
		string stringGameState = The.Game.GetStringGameState("BarathrumitesRank");
		if (stringGameState != "apprentice" && stringGameState != "journeyfriend" && stringGameState != "disciple")
		{
			Popup.ShowBlock("You are promoted to the rank of Apprentice.");
			The.Game.SetStringGameState("BarathrumitesRank", "apprentice");
		}
		OpenRank1Doors();
	}

	public static void PromoteToJourneyfriend()
	{
		string stringGameState = The.Game.GetStringGameState("BarathrumitesRank");
		if (stringGameState != "journeyfriend" && stringGameState != "disciple")
		{
			Popup.ShowBlock("You are promoted to the rank of Journeyfriend.");
			The.Game.SetStringGameState("BarathrumitesRank", "journeyfriend");
		}
		OpenRank2Doors();
		IdentifyByTag("GritGateJourneyfriendIdentify");
	}

	public static void PromoteToDisciple()
	{
		if (The.Game.GetStringGameState("BarathrumitesRank") != "disciple")
		{
			Popup.ShowBlock("You are promoted to the rank of Disciple.");
			The.Game.SetStringGameState("BarathrumitesRank", "disciple");
		}
		OpenRank2Doors();
		IdentifyByTag("GritGateDiscipleIdentify");
	}

	public static void IdentifyByTag(string Tag)
	{
		The.Player?.CurrentZone?.ForeachObject(delegate(GameObject GO)
		{
			if (GO.HasTagOrProperty(Tag))
			{
				GO.MakeUnderstood();
				GO.ForeachInventoryAndEquipment(delegate(GameObject OGO)
				{
					OGO.MakeUnderstood();
				});
			}
		});
	}

	public static void OpenRank0Doors()
	{
		List<Cell> cellsWithTaggedObject = The.ZoneManager.GetZone("JoppaWorld.22.14.1.0.13").GetCellsWithTaggedObject("GritGateDoorRank0");
		foreach (Cell item in cellsWithTaggedObject)
		{
			item.ForeachObjectWithTag("GritGateDoorRank0", delegate(GameObject GO)
			{
				Door part3 = GO.GetPart<Door>();
				if (part3 != null && part3.bLocked)
				{
					part3.bLocked = false;
				}
			});
		}
		foreach (Cell item2 in cellsWithTaggedObject)
		{
			item2.ForeachObjectWithTag("GritGateDoorRank0", delegate(GameObject GO)
			{
				Door part2 = GO.GetPart<Door>();
				if (part2 != null && !part2.bOpen)
				{
					part2.Open();
				}
			});
		}
		foreach (Cell item3 in cellsWithTaggedObject)
		{
			item3.ForeachObjectWithTag("GritGateDoorRank0", delegate(GameObject GO)
			{
				ForceProjector part = GO.GetPart<ForceProjector>();
				if (part != null && !part.AllowPassage.Contains(The.Player))
				{
					part.AddAllowPassage(The.Player);
				}
			});
		}
	}

	public static void OpenRank1Doors()
	{
		OpenRank0Doors();
		List<Cell> cellsWithTaggedObject = The.ZoneManager.GetZone("JoppaWorld.22.14.1.0.13").GetCellsWithTaggedObject("GritGateDoorRank1");
		foreach (Cell item in cellsWithTaggedObject)
		{
			item.ForeachObjectWithTag("GritGateDoorRank1", delegate(GameObject GO)
			{
				Door part3 = GO.GetPart<Door>();
				if (part3 != null && part3.bLocked)
				{
					part3.bLocked = false;
				}
			});
		}
		foreach (Cell item2 in cellsWithTaggedObject)
		{
			item2.ForeachObjectWithTag("GritGateDoorRank1", delegate(GameObject GO)
			{
				Door part2 = GO.GetPart<Door>();
				if (part2 != null && !part2.bOpen)
				{
					part2.Open();
				}
			});
		}
		foreach (Cell item3 in cellsWithTaggedObject)
		{
			item3.ForeachObjectWithTag("GritGateDoorRank1", delegate(GameObject GO)
			{
				ForceProjector part = GO.GetPart<ForceProjector>();
				if (part != null && !part.AllowPassage.Contains(The.Player))
				{
					part.AddAllowPassage(The.Player);
				}
			});
		}
	}

	public static void OpenRank2Doors()
	{
		OpenRank1Doors();
		List<Cell> cellsWithTaggedObject = The.ZoneManager.GetZone("JoppaWorld.22.14.1.0.13").GetCellsWithTaggedObject("GritGateDoorRank2");
		foreach (Cell item in cellsWithTaggedObject)
		{
			item.ForeachObjectWithTag("GritGateDoorRank2", delegate(GameObject GO)
			{
				Door part3 = GO.GetPart<Door>();
				if (part3 != null && part3.bLocked)
				{
					part3.bLocked = false;
				}
			});
		}
		foreach (Cell item2 in cellsWithTaggedObject)
		{
			item2.ForeachObjectWithTag("GritGateDoorRank2", delegate(GameObject GO)
			{
				Door part2 = GO.GetPart<Door>();
				if (part2 != null && !part2.bOpen)
				{
					part2.Open();
				}
			});
		}
		foreach (Cell item3 in cellsWithTaggedObject)
		{
			item3.ForeachObjectWithTag("GritGateDoorRank2", delegate(GameObject GO)
			{
				ForceProjector part = GO.GetPart<ForceProjector>();
				if (part != null && !part.AllowPassage.Contains(The.Player))
				{
					part.AddAllowPassage(The.Player);
				}
			});
		}
	}

	public static void BeginInvasion()
	{
		if (!The.Game.HasGameState("CallToArmsStarted"))
		{
			Zone currentZone = The.Player.CurrentZone;
			GameObject gO = GameObject.create("CallToArmsScript");
			currentZone.GetCell(0, 0).AddObject(gO);
			The.ActionManager.AddActiveObject(gO);
		}
	}
}
