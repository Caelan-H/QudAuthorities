#define NLOG_ALL
using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SultanShrine : IPart
{
	public int PresetLocation = -1;

	public int Period;

	public string Inscription = "";

	public string PresetGospelByProperty;

	public bool bInitialized;

	public HistoricEntity sultan;

	public HistoricEntitySnapshot sultanSnap;

	public HistoricEvent revealedEvent;

	public string RegionName;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AutoexploreObjectEvent.ID && (ID != EnteredCellEvent.ID || bInitialized))
		{
			return ID == GetPointsOfInterestEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			string baseDisplayName = ParentObject.BaseDisplayName;
			string key = "Sultan Shrine " + baseDisplayName;
			bool flag = true;
			PointOfInterest pointOfInterest = E.Find(key);
			if (pointOfInterest != null)
			{
				if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
				{
					E.Remove(pointOfInterest);
				}
				else
				{
					flag = false;
					pointOfInterest.Explanation = "nearest";
				}
			}
			if (flag)
			{
				E.Add(ParentObject, baseDisplayName, null, key, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!bInitialized)
		{
			ParentObject.pRender.Tile = "terrain/sw_tombstone_" + Stat.Random(1, 4) + ".bmp";
			ShrineInitialize();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent != "Look") && HasUnrevealedSecret())
		{
			E.Want = true;
			E.FromAdjacent = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		Object.RegisterPartEvent(this, "SpecialInit");
		base.Register(Object);
	}

	public void ShrineInitialize()
	{
		if (bInitialized)
		{
			return;
		}
		History history = null;
		HistoricEntityList historicEntityList = null;
		try
		{
			history = XRLCore.Core.Game.sultanHistory;
			historicEntityList = history.GetEntitiesWherePropertyEquals("type", "sultan");
			if (HasPropertyOrTag("ForceSultan"))
			{
				historicEntityList.entities = new List<HistoricEntity> { history.GetEntity(GetPropertyOrTag("ForceSultan")) };
			}
			string text = null;
			if (PresetLocation != -1)
			{
				text = XRLCore.Core.Game.GetStringGameState("SultanDungeonPlacementOrder_" + PresetLocation);
			}
			if (text != null)
			{
				int num = 0;
				while (num < historicEntityList.entities.Count)
				{
					int num2 = 0;
					while (true)
					{
						if (num2 < historicEntityList.entities[num].events.Count)
						{
							if (historicEntityList.entities[num].events[num2].hasEventProperty("revealsRegion") && historicEntityList.entities[num].events[num2].getEventProperty("revealsRegion") == text)
							{
								sultan = historicEntityList.entities[num];
								sultanSnap = sultan.GetCurrentSnapshot();
								revealedEvent = historicEntityList.entities[num].events[num2];
								goto IL_02d3;
							}
							num2++;
							continue;
						}
						num++;
						break;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.log.Error("SultanShrine initialization error:" + ex.ToString());
		}
		try
		{
			List<HistoricEntity> list = historicEntityList.entities.Shuffle();
			for (int i = 0; i < list.Count; i++)
			{
				if (Period != 0 && int.Parse(list[i].GetCurrentSnapshot().GetProperty("period")) != Period)
				{
					continue;
				}
				List<HistoricEvent> events = list[i].events;
				for (int j = 0; j < events.Count; j++)
				{
					sultan = list[i];
					sultanSnap = sultan.GetCurrentSnapshot();
					revealedEvent = sultan.GetRandomEventWhereDelegate((HistoricEvent ev) => ev.hasEventProperty("gospel") && (PresetGospelByProperty == null || ev.hasEventProperty(PresetGospelByProperty)), Stat.Rnd);
					if (revealedEvent != null)
					{
						break;
					}
				}
				if (revealedEvent != null)
				{
					break;
				}
			}
			if (revealedEvent == null)
			{
				revealedEvent = sultan.GetRandomEventWhereDelegate((HistoricEvent ev) => ev.hasEventProperty("gospel"), Stat.Rnd);
			}
		}
		catch (Exception ex2)
		{
			Logger.log.Error("SultanShrine initialization error (2):" + ex2.ToString());
		}
		goto IL_02d3;
		IL_02d3:
		try
		{
			if (!ParentObject.HasTagOrProperty("HasPregeneratedName"))
			{
				ParentObject.DisplayName = "shrine to " + sultanSnap.GetProperty("name", "<unknown name>");
				string randomElementFromListProperty = sultanSnap.GetRandomElementFromListProperty("cognomen", null, Stat.Rand);
				if (randomElementFromListProperty != null)
				{
					Render pRender = ParentObject.pRender;
					pRender.DisplayName = pRender.DisplayName + ", " + randomElementFromListProperty;
				}
			}
			ParentObject.GetPart<Description>().Short = "The shrine depicts a significant event from the life of the ancient sultan " + sultanSnap.GetProperty("name") + ":\n\n" + revealedEvent.GetEventProperty("gospel", "<unknown gospel>");
			if (sultanSnap != null)
			{
				string text2 = null;
				if (Period > 5 || sultanSnap.GetProperty("name") == "Resheph")
				{
					text2 = "Terrain/sw_resheph_sultanstatue.bmp";
				}
				else
				{
					Dictionary<string, string> dictionary;
					if (XRLCore.Core.Game.HasObjectGameState("SultanAssignedStatues"))
					{
						dictionary = XRLCore.Core.Game.GetObjectGameState("SultanAssignedStatues") as Dictionary<string, string>;
					}
					else
					{
						dictionary = new Dictionary<string, string>();
						XRLCore.Core.Game.SetObjectGameState("SultanAssignedStatues", dictionary);
					}
					string property = sultanSnap.GetProperty("name");
					if (dictionary.ContainsKey(property))
					{
						text2 = dictionary[property];
					}
					else
					{
						List<string> list2 = new List<string>(dictionary.Values);
						List<string> list3 = new List<string>();
						for (int k = 1; k <= 8; k++)
						{
							if (!list2.Contains("Terrain/sw_sultanstatue_" + k + ".bmp"))
							{
								for (int l = 0; l < 10; l++)
								{
									list3.Add("Terrain/sw_sultanstatue_" + k + ".bmp");
								}
							}
							if (!list2.Contains("Terrain/sw_sultanstatue_rare_" + k + ".bmp"))
							{
								list3.Add("Terrain/sw_sultanstatue_rare_" + k + ".bmp");
							}
						}
						text2 = list3.GetRandomElement();
						dictionary.Add(sultanSnap.GetProperty("name"), text2);
					}
				}
				if (text2 != null)
				{
					ParentObject.pRender.Tile = text2;
					ParentObject.pRender.DetailColor = (ParentObject.IsUnderSky() ? "g" : Crayons.GetSubterraneanGrowthColor());
				}
				if (PresetLocation == 0)
				{
					foreach (JournalSultanNote sultanNote in JournalAPI.GetSultanNotes())
					{
						if (sultanNote.sultan == sultanSnap.entity.id)
						{
							sultanNote.attributes.Add("include:Joppa");
						}
					}
				}
			}
		}
		catch (Exception ex3)
		{
			Logger.log.Error("SultanShrine initialization error (3):" + ex3.ToString());
		}
		bInitialized = true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SpecialInit")
		{
			ShrineInitialize();
		}
		else if (E.ID == "AfterLookedAt")
		{
			ShrineInitialize();
			RevealBasedOnHistoricalEvent(revealedEvent, PresetLocation, PresetGospelByProperty);
		}
		return base.FireEvent(E);
	}

	public static void RevealBasedOnHistoricalEvent(HistoricEvent ev, int PresetLocation = -1, string PresetGospelByProperty = null)
	{
		JournalSultanNote journalSultanNote = ev.Reveal();
		if ((PresetLocation == 0 || PresetGospelByProperty == "JoppaShrine") && journalSultanNote != null && !journalSultanNote.Has("nobuy:Joppa"))
		{
			journalSultanNote.attributes.Add("nobuy:Joppa");
			Faction faction = Factions.get("Joppa");
			if (faction.Visible)
			{
				journalSultanNote.history = journalSultanNote.history + " {{K|-learned from " + faction.getFormattedName() + "}}";
			}
		}
	}

	public bool HasUnrevealedSecret()
	{
		if (revealedEvent != null)
		{
			return revealedEvent.HasUnrevealedSecret();
		}
		return false;
	}
}
