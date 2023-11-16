using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.API;
using XRL.UI;
using XRL.Wish;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class TerrainTravel : IPart
{
	public int LostChance = 12;

	public int Segments = 50;

	public string TravelClass = "";

	[NonSerialized]
	public List<EncounterEntry> Encounters = new List<EncounterEntry>();

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		Encounters = Reader.ReadList<EncounterEntry>();
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		Writer.Write(Encounters);
	}

	public override bool SameAs(IPart p)
	{
		TerrainTravel terrainTravel = p as TerrainTravel;
		if (terrainTravel.LostChance != LostChance)
		{
			return false;
		}
		if (terrainTravel.Segments != Segments)
		{
			return false;
		}
		if (terrainTravel.TravelClass != TravelClass)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == ObjectLeavingCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.IsPlayer())
		{
			if (!ParentObject.FireEvent("CheckLostChance"))
			{
				return false;
			}
			if (Encounters.Count > 0)
			{
				int num = 10;
				int num2 = num;
				if (Options.DebugEncounterChance)
				{
					int @for = EncounterChanceEvent.GetFor(E.Object, TravelClass);
					num2 = num * (100 + @for) / 100;
					IComponent<GameObject>.AddPlayerMessage("Base encounter chance: " + num2 + "%");
				}
				foreach (EncounterEntry encounter in Encounters)
				{
					if (!encounter.Enabled)
					{
						continue;
					}
					int for2 = EncounterChanceEvent.GetFor(E.Object, TravelClass, 0, encounter);
					int num3 = num;
					if (for2 != 0)
					{
						num3 = num3 * (100 + for2) / 100;
					}
					if (num3 != num2 && Options.DebugEncounterChance)
					{
						IComponent<GameObject>.AddPlayerMessage("Modified encounter chance: " + num3 + "%");
					}
					if (num3.in100())
					{
						if (Options.DebugEncounterChance)
						{
							IComponent<GameObject>.AddPlayerMessage("Triggered encounter chance: " + num3 + "%");
						}
						if ((string.IsNullOrEmpty(encounter.secretID) || !JournalAPI.GetMapNote(encounter.secretID).revealed) && (!encounter.Optional || Popup.ShowYesNo(encounter.Text) == DialogResult.Yes))
						{
							if (!encounter.Optional && !string.IsNullOrEmpty(encounter.Text))
							{
								Popup.Show(encounter.Text);
							}
							Zone zone = The.ZoneManager.SetActiveZone(encounter.Zone);
							zone.CheckWeather();
							E.Object.SystemMoveTo(zone.GetPullDownLocation(E.Object));
							The.ZoneManager.ProcessGoToPartyLeader();
							Encounters.Remove(encounter);
							return false;
						}
						break;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectLeavingCellEvent E)
	{
		if (!E.Forced && (E.Type == null || E.Type == "SystemLongDistanceMove") && !ParentObject.CurrentZone.ZoneID.Contains("Serenity"))
		{
			int TotalSegments = 0;
			try
			{
				if (E.Object == null)
				{
					MetricsManager.LogException("TerrainTravel", new Exception("No object in event"));
				}
				else if (E.Object.IsPlayer() && !HandleLeavingCell(E.Object, ref TotalSegments))
				{
					return false;
				}
			}
			finally
			{
				int num = TotalSegments / 10;
				if (num > 0)
				{
					The.ActionManager.SyncTurnTickRecipients();
					The.ActionManager.ProcessTenTurnTickCatchup(num);
					The.ActionManager.ProcessHundredTurnTickCatchup(num);
					The.ActionManager.FlushTurnTickRecipients();
				}
			}
		}
		return base.HandleEvent(E);
	}

	private bool HandleLeavingCell(GameObject Object, ref int TotalSegments)
	{
		Cell cell = IComponent<GameObject>.ThePlayer.CurrentCell;
		if (Object.IsPlayer())
		{
			The.Game.SetStringGameState("LastLocationOnSurface", "");
		}
		int @for = GetLostChanceEvent.GetFor(The.Player, TravelClass);
		int for2 = TravelSpeedEvent.GetFor(The.Player, TravelClass);
		TotalSegments = (int)(300f / ((float)Object.Speed * (100f + (float)for2) / 100f) * (float)Segments);
		int chance = Math.Max(LostChance * (100 - @for) / 100, 0);
		if (The.Core.IDKFA)
		{
			chance = 0;
		}
		if (Object.IsPlayer() && Options.DebugGetLostChance)
		{
			IComponent<GameObject>.AddPlayerMessage("Get lost chance: " + chance + "%");
		}
		ZoneManager zoneManager = The.ZoneManager;
		if (chance.in100())
		{
			string zoneWorld = cell.ParentZone.GetZoneWorld();
			int x = cell.X;
			int y = cell.Y;
			int zoneZ = 10;
			string zoneID = ZoneID.Assemble(zoneWorld, x, y, 1, 1, zoneZ);
			if (!zoneManager.IsZoneBuilt(zoneID))
			{
				Zone zone = zoneManager.GetZone(zoneID);
				Cell pullDownLocation = zone.GetPullDownLocation(The.Player);
				Lost lost = new Lost(1);
				if (pullDownLocation.IsPassable(The.Player) && Object.ApplyEffect(lost))
				{
					zone.CheckWeather();
					lost.Visited.Add(zone.ZoneID);
					The.Player.SystemMoveTo(pullDownLocation);
					Popup.ShowSpace("You're lost! Regain your bearings by exploring your surroundings.");
					The.Player.FireEvent(Event.New("AfterLost", "FromCell", cell));
					return false;
				}
			}
		}
		Cell cell2 = Object.CurrentCell;
		if (Object.IsPlayer() && Options.DebugTravelSpeed)
		{
			IComponent<GameObject>.AddPlayerMessage("Travel speed: " + TotalSegments + " segments/parasang");
		}
		The.ActionManager.SyncTurnTickRecipients();
		bool TravelMessagesSuppressed = false;
		for (int i = 0; i < TotalSegments; i++)
		{
			The.ActionManager.ProcessIndependentEndSegment(Object);
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			if (i % 10 != 0)
			{
				continue;
			}
			The.Game.TimeTicks++;
			if (!BeforeBeginTakeActionEvent.Check(Object) || !BeginTakeActionEvent.Check(Object, Traveling: true, ref TravelMessagesSuppressed))
			{
				return false;
			}
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			if (!CommandTakeActionEvent.Check(Object))
			{
				return false;
			}
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			The.ActionManager.ProcessTurnTickHierarchical();
			if (i % 100 == 0)
			{
				The.ActionManager.ProcessTenTurnTickHierarchical();
				if (i % 1000 == 0)
				{
					The.ActionManager.ProcessHundredTurnTickHierarchical();
				}
			}
			The.ActionManager.ProcessIndependentEndTurn(Object);
			if (cell2 != Object.CurrentCell)
			{
				return false;
			}
			MinEvent.ResetPools();
		}
		The.ActionManager.FlushTurnTickRecipients();
		Object.Energy.BaseValue = 2200;
		Object.CleanEffects();
		Object.FireEvent(Event.New("AfterTravel", "Segments", TotalSegments));
		if (Object.IsPlayer())
		{
			The.Game.Player.Messages.BeginPlayerTurn();
		}
		return true;
	}

	public void AddEncounter(EncounterEntry Entry)
	{
		Encounters.Add(Entry);
		if (ParentObject.HasTag("Immutable") || ParentObject.HasTag("ImmutableWhenUnexplored"))
		{
			ParentObject.SetIntProperty("ForceMutableSave", 1);
		}
	}

	[WishCommand("terrainencounters", null)]
	public static void ShowTerrainEncounters()
	{
		Keys keys = Keys.None;
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Zone zone = The.ZoneManager.GetZone(The.ActiveZone.ZoneWorld);
		List<EncounterEntry> list = new List<EncounterEntry>();
		do
		{
			list.Clear();
			int num = -1;
			int num2 = -1;
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "LeftClick")
			{
				num = Keyboard.CurrentMouseEvent.x;
				num2 = Keyboard.CurrentMouseEvent.y;
			}
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					int k = 0;
					int num3 = 0;
					for (int count = zone.Map[i][j].Objects.Count; k < count; k++)
					{
						TerrainTravel firstPartDescendedFrom = zone.Map[i][j].Objects[k].GetFirstPartDescendedFrom<TerrainTravel>();
						if (firstPartDescendedFrom != null)
						{
							num3 += firstPartDescendedFrom.Encounters.Count;
							scrapBuffer.WriteAt(i, j, (num3 < 10) ? num3.ToString() : "+");
							if (num == i && num2 == j)
							{
								list.AddRange(firstPartDescendedFrom.Encounters);
							}
						}
					}
				}
			}
			int num4 = ((num2 >= 12) ? 1 : 23);
			foreach (EncounterEntry item in list)
			{
				scrapBuffer.WriteAt(1, (num2 < 12) ? num4-- : num4++, "{{W|" + item.Text + "}}");
			}
			scrapBuffer.Draw();
		}
		while ((keys = Keyboard.getvk(MapDirectionToArrows: false)) != Keys.Escape);
	}
}
