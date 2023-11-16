using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class PlayerMuralController : IPart
{
	public bool initialized;

	public bool carving;

	public string carvingStage = "waiting";

	public int currentMural;

	public int currentPanel;

	public int turnTick;

	public List<List<Location2D>> murals;

	public List<JournalAccomplishment> playerMuralEventList;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		turnTick++;
		if (turnTick == 2)
		{
			if (carvingStage == "done")
			{
				return true;
			}
			if (currentMural >= murals.Count || currentMural >= playerMuralEventList.Count)
			{
				if (XRLCore.Core.Game.HasIntGameState("ReshephDisguise"))
				{
					Popup.Show("Herododicus says '&WI'm finished, Moloch! Praise =name= Resheph, all who canter in this House!&Y'".Replace("=name=", XRLCore.Core.Game.PlayerName));
				}
				else
				{
					Popup.Show("Herododicus says '&WI'm done!&Y'");
				}
				carvingStage = "done";
				if (XRLCore.Core.Game.HasIntGameState("ReshephDisguise"))
				{
					XRLCore.Core.Game.SetIntGameState("PlayerEngravingsDone_ReshephDisguise", 1);
				}
				else
				{
					XRLCore.Core.Game.SetIntGameState("PlayerEngravingsDone", 1);
				}
				foreach (GameObject item in ParentObject.pPhysics.CurrentCell.ParentZone.GetObjectsWithPart("ReshephsCrypt"))
				{
					item.RemoveIntProperty("Sealed");
					item.FireEvent("SyncOpened");
					item.pPhysics.PlayWorldSound("WoodDoorOpen");
				}
			}
			if (carvingStage == "go")
			{
				if (getBiographer().pPhysics.CurrentCell == getCurrentTargetCell())
				{
					getBiographer().pPhysics.PlayWorldSound("ShieldBlockWood", 1.5f);
					getBiographer().pPhysics.CurrentCell.GetCellFromDirection("N").GetObjectsWithPart("Physics")[0].Sparksplatter();
					updatePlayerMural(murals[currentMural], playerMuralEventList[currentMural], currentPanel);
					currentPanel++;
					if (currentPanel > 2)
					{
						currentPanel = 0;
						currentMural++;
					}
				}
				else if (!getBiographer().pBrain.Goals.Items.Any((GoalHandler g) => g.GetType() == typeof(MoveTo) || g.GetType() == typeof(Step)))
				{
					getBiographer().pBrain.Goals.Clear();
					getBiographer().pBrain.MoveTo(getCurrentTargetCell());
				}
			}
			turnTick = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ParentObject.CurrentCell.ParentZone.IsActive() && (!initialized || murals == null))
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!initialized || murals == null)
		{
			initializeMurals();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginPlayerMural");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginPlayerMural")
		{
			getBiographer().pBrain.Wanders = false;
			getBiographer().pBrain.WandersRandomly = false;
			getBiographer().pBrain.StartingCell = null;
			getBiographer().SetIntProperty("NoStay", 1);
			carvingStage = "go";
			currentMural = 0;
			currentPanel = 0;
			turnTick = 1;
		}
		return true;
	}

	public int RightToLeftTopToBottom(List<Location2D> a, List<Location2D> b)
	{
		if (a[0] == b[0])
		{
			return 0;
		}
		if (a == null)
		{
			return 1;
		}
		if (b == null)
		{
			return -1;
		}
		if (a[0].x == b[0].x)
		{
			return a[0].y.CompareTo(b[0].y);
		}
		return b[0].x.CompareTo(a[0].x);
	}

	public List<HistoricEvent> GetMuralEventsForPeriod(int period)
	{
		HistoricEntityList entitiesWherePropertyEquals = XRLCore.Core.Game.sultanHistory.GetEntitiesWherePropertyEquals("type", "sultan");
		for (int i = 0; i < entitiesWherePropertyEquals.entities.Count; i++)
		{
			if (int.Parse(entitiesWherePropertyEquals.entities[i].GetCurrentSnapshot().GetProperty("period")) == period)
			{
				return (from e in entitiesWherePropertyEquals.entities[i].events
					where e.hasEventProperty("tombInscription")
					orderby e.year
					select e).ToList().ShuffleInPlace();
			}
		}
		return new List<HistoricEvent>();
	}

	public void clearMurals()
	{
	}

	public void initializeMurals()
	{
		if (ParentObject.pPhysics.CurrentCell != null)
		{
			murals = new List<List<Location2D>>();
			initialized = true;
			Zone zone = ParentObject.pPhysics.currentCell.ParentZone;
			HashSet<Cell> used = new HashSet<Cell>();
			for (int i = 0; i < zone.Width - 2; i++)
			{
				for (int j = 0; j < zone.Height; j++)
				{
					Cell cell = zone.GetCell(i, j);
					if (used.Contains(cell) || !cell.HasObjectWithPart("SultanMural"))
					{
						continue;
					}
					List<Location2D> list = new List<Location2D>(3);
					list.Add(cell.location);
					list.Add(cell.GetCellFromDirection("E").location);
					list.Add(cell.GetCellFromDirection("E").GetCellFromDirection("E").location);
					if (list.All((Location2D c) => c != null && zone.GetCell(c).HasObjectWithPart("SultanMural") && !used.Contains(zone.GetCell(c))))
					{
						murals.Add(list);
						list.ForEach(delegate(Location2D c)
						{
							used.Add(zone.GetCell(c));
						});
						blankMural(list);
					}
				}
			}
			murals.Sort(RightToLeftTopToBottom);
			List<JournalAccomplishment> originalList = (from a in JournalAPI.Accomplishments
				where !string.IsNullOrEmpty(a.muralText)
				orderby a.muralWeight
				select a).ToList();
			List<JournalAccomplishment> list2 = (from a in JournalAPI.Accomplishments
				where !string.IsNullOrEmpty(a.muralText) && originalList.IndexOf(a) != 0
				orderby a.muralWeight
				select a).ToList();
			playerMuralEventList = new List<JournalAccomplishment>();
			while (list2.Count > 0 && playerMuralEventList.Count < 16)
			{
				list2.ShuffleInPlace();
				int num = list2.Sum((JournalAccomplishment a) => (int)a.muralWeight);
				int num2 = Stat.Random(0, num - 1);
				int num3 = 0;
				int k;
				for (k = 0; k < list2.Count; k++)
				{
					num3 = (int)(num3 + list2[k].muralWeight);
					if (num3 >= num2)
					{
						break;
					}
				}
				playerMuralEventList.Add(list2[k]);
				list2.RemoveAt(k);
			}
			playerMuralEventList.Sort((JournalAccomplishment a, JournalAccomplishment b) => originalList.IndexOf(a).CompareTo(originalList.IndexOf(b)));
			playerMuralEventList.Remove(originalList[0]);
			playerMuralEventList.Add(originalList[0]);
			JournalAccomplishment journalAccomplishment = new JournalAccomplishment();
			journalAccomplishment.category = "Dies";
			journalAccomplishment.muralCategory = JournalAccomplishment.MuralCategory.Dies;
			journalAccomplishment.muralText = "On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", " + XRLCore.Core.Game.PlayerName + " died peacefully and was laid to rest in the Tomb of Sultans.";
			playerMuralEventList.Add(journalAccomplishment);
		}
		int count = playerMuralEventList.Count;
		int num4 = 9 - count / 2;
		if (num4 > 0)
		{
			for (int l = 0; l < num4; l++)
			{
				murals.RemoveAt(0);
			}
		}
	}

	public void blankMural(List<Location2D> muralCells)
	{
		Zone parentZone = ParentObject.pPhysics.CurrentCell.ParentZone;
		if (muralCells.Count != 3)
		{
			foreach (Location2D muralCell in muralCells)
			{
				GameObject firstObjectWithPart = parentZone.GetCell(muralCell).GetFirstObjectWithPart("SultanMural");
				string text = "c";
				if (parentZone.GetCell(muralCell).GetCellFromDirection("W") == null || !parentZone.GetCell(muralCell).GetCellFromDirection("W").HasWall())
				{
					text = "l";
				}
				if (parentZone.GetCell(muralCell).GetCellFromDirection("E") == null || !parentZone.GetCell(muralCell).GetCellFromDirection("E").HasWall())
				{
					text = "r";
				}
				firstObjectWithPart.DisplayName = "ruined mural slate";
				firstObjectWithPart.pRender.RenderString = 'ÿ'.ToString();
				firstObjectWithPart.pRender.Tile = "Walls/sw_mural_ruined" + Stat.Random(1, 6) + "_" + text + ".bmp";
			}
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			string text2 = "c";
			if (i == 0)
			{
				text2 = "l";
			}
			if (i == 1)
			{
				text2 = "c";
			}
			if (i == 2)
			{
				text2 = "r";
			}
			GameObject firstObjectWithPart2 = parentZone.GetCell(muralCells[i]).GetFirstObjectWithPart("SultanMural");
			firstObjectWithPart2.DisplayName = "blank mural slate";
			firstObjectWithPart2.pRender.RenderString = 'ÿ'.ToString();
			firstObjectWithPart2.pRender.Tile = "Walls/sw_mural_blank_" + text2 + ".bmp";
		}
	}

	public static void SetStateReshephDisguise()
	{
		XRLCore.Core.Game.SetIntGameState("ReshephDisguise", 1);
	}

	public static void RevealMarkOfDeath()
	{
		JournalAPI.RevealObservation("MarkOfDeathSecret", onlyIfNotRevealed: true);
	}

	public static void BeginPlayerMuralSequence()
	{
		IComponent<GameObject>.ThePlayer.CurrentZone.FireEvent("BeginPlayerMural");
	}

	public void updatePlayerMural(List<Location2D> muralCells, JournalAccomplishment a, int panel)
	{
		if (muralCells.Count != 3)
		{
			Debug.LogError("mural not 3 cells long!");
			return;
		}
		string text = a.muralCategory.ToString().ToLower();
		string text2 = The.Player.BaseDisplayName;
		string text3 = null;
		if (!string.IsNullOrEmpty(text3))
		{
			text2 = text2 + ", " + text3;
		}
		Zone currentZone = ParentObject.CurrentZone;
		for (int i = 0; i < 3; i++)
		{
			if (i == panel)
			{
				string text4 = "c";
				if (i == 0)
				{
					text4 = "l";
				}
				if (i == 1)
				{
					text4 = "c";
				}
				if (i == 2)
				{
					text4 = "r";
				}
				GameObject firstObjectWithPart = currentZone.GetCell(muralCells[i]).GetFirstObjectWithPart("SultanMural");
				Debug.Log(text);
				if (SultanMuralController.muralAscii.ContainsKey(text))
				{
					firstObjectWithPart.pRender.RenderString = ((char)SultanMuralController.muralAscii[text][i]).ToString();
				}
				firstObjectWithPart.SetIntProperty("_wasEngraved", 1);
				firstObjectWithPart.DisplayName = "mural of " + text2;
				firstObjectWithPart.pRender.Tile = "Walls/sw_mural_" + text + "_" + text4 + ".bmp";
				firstObjectWithPart.pPhysics.Category = text;
				firstObjectWithPart.GetPart<Description>().Short = "The tomb mural depicts a significant event from the life of the sultan " + text2 + ":\n\n" + a.muralText;
			}
		}
	}

	private GameObject getBiographer()
	{
		return GameObject.findByBlueprint("Biographer");
	}

	private Cell getCurrentTargetCell()
	{
		return ParentObject.CurrentZone.GetCell(murals[currentMural][currentPanel]).GetCellFromDirection("S");
	}
}
