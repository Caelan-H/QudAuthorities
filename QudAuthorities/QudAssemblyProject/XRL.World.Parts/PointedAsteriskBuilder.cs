using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HistoryKit;
using Qud.API;
using XRL.Rules;
using XRL.Wish;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class PointedAsteriskBuilder : IPart
{
	private class FactionData
	{
		public string Name;

		public string Color;

		public string Postfix;

		public bool Article;
	}

	private delegate FactionData FactionEffect(GameObject Object, int Points);

	private static readonly Dictionary<string, FactionEffect> FactionEffects = new Dictionary<string, FactionEffect>
	{
		{ "JOPPA", AddJoppaEffect },
		{ "BARATHRUMITES", AddBarathrumitesEffect },
		{ "HINDREN", AddHindrenEffect },
		{ "KYAKUKYA", AddKyakukyaEffect },
		{ "EZRA", AddEzraEffect },
		{ "YDFREEHOLD", AddYdFreeholdEffect },
		{ "MECHANIMISTS", AddMechanimistsEffect },
		{ "MOPANGO", AddMopangoEffect },
		{ "PARIAHS", AddPariahsEffect }
	};

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		if (system != null && system.settledFaction != null && E.Context != "Wish" && E.ReplacementObject == null)
		{
			BuildAsterisk(ParentObject, system.settledFaction, Math.Min(10, system.CandidateCount));
			if (system.CandidateCount >= 10)
			{
				AchievementManager.SetAchievement("ACH_GIFTED_10ASTERISK");
			}
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public static GameObject BuildAsterisk(GameObject Object, string Faction, int Points)
	{
		HistoricEntitySnapshot villageSnapshot = HistoryAPI.GetVillageSnapshot(Faction);
		FactionData factionData;
		if (FactionEffects.TryGetValue(Faction.ToUpper(), out var value))
		{
			factionData = value(Object, Points);
		}
		else
		{
			value = FactionEffects.Values.GetRandomElement();
			value(Object, Math.Min(10, Points + 1));
			factionData = new FactionData
			{
				Name = villageSnapshot?.Name,
				Color = (villageSnapshot?.GetList("palette")?.FirstOrDefault() ?? Crayons.GetRandomColor()),
				Postfix = HistoryAPI.ExpandVillageText("It's engraved with icons dedicated to =village.sacred=.", null, villageSnapshot)
			};
		}
		Object.DisplayName = BuildAsteriskName(Faction, Points, factionData);
		Object.RequirePart<Commerce>().Value += Points * 500;
		Description description = Object.RequirePart<Description>();
		description._Short = description._Short + " " + factionData.Postfix;
		switch (Points)
		{
		case 3:
			Object.pRender.Tile = "Items/sw_asterisk_3.bmp";
			break;
		case 4:
			Object.pRender.Tile = "Items/sw_asterisk_4.bmp";
			break;
		case 5:
			Object.pRender.Tile = "Items/sw_asterisk_5.bmp";
			break;
		default:
			Object.pRender.Tile = "Items/sw_asterisk_6plus.bmp";
			break;
		}
		Object.MakeUnderstood();
		return Object;
	}

	[WishCommand(null, null, Command = "slynthasterisk")]
	public static void AsteriskWish()
	{
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("n-Pointed Asterisk", 0, 0, null, null, "Wish");
		foreach (FactionEffect value in FactionEffects.Values)
		{
			value(gameObject, 10);
		}
		gameObject.DisplayName = "{{paisley|The 10-Pointed Asterisk of the Ensemble}}";
		gameObject.MakeUnderstood();
		The.Player.TakeObject(gameObject, Silent: false, 0);
	}

	[WishCommand(null, null, Command = "slynthasterisk")]
	public static void AsteriskWish(string Value)
	{
		string faction = Value;
		int result = 3;
		if (Value.Contains(':'))
		{
			string[] array = Value.Split(':');
			faction = array[0];
			if (!int.TryParse(array[1], out result))
			{
				result = Stat.Roll(3, 10);
			}
		}
		else
		{
			result = Stat.Roll(3, 10);
		}
		GameObject @object = GameObjectFactory.Factory.CreateObject("n-Pointed Asterisk", 0, 0, null, null, "Wish");
		The.Player.TakeObject(BuildAsterisk(@object, faction, result), Silent: false, 0);
	}

	private static string BuildAsteriskName(string Faction, int Points, FactionData Data)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder().Append(Points).Append("-pointed asterisk of ");
		if (Data.Name == null)
		{
			Faction faction = Factions.loop().FirstOrDefault((Faction x) => x.Name.EqualsNoCase(Faction));
			if (faction != null)
			{
				if (faction.FormatWithArticle)
				{
					stringBuilder.Append("the ");
				}
				stringBuilder.Append('{', 2);
				stringBuilder.Append(Data.Color);
				stringBuilder.Append("|");
				stringBuilder.Append(faction.DisplayName);
			}
			else
			{
				stringBuilder.Append('{', 2);
				stringBuilder.Append(Data.Color);
				stringBuilder.Append("|");
				stringBuilder.Append(Faction);
			}
		}
		else
		{
			if (Data.Article)
			{
				stringBuilder.Append("the ");
			}
			stringBuilder.Append('{', 2);
			stringBuilder.Append(Data.Color);
			stringBuilder.Append("|");
			stringBuilder.Append(Data.Name);
		}
		return stringBuilder.Append('}', 2).ToString();
	}

	private static FactionData AddJoppaEffect(GameObject Object, int Points)
	{
		Object.AddPart(new DrinkMagnifier
		{
			Chance = 4 + 2 * Points,
			Percent = 200
		});
		return new FactionData
		{
			Name = "Joppa",
			Color = "g",
			Postfix = "It's carved in relief with watervine crawling along each point."
		};
	}

	private static FactionData AddBarathrumitesEffect(GameObject Object, int Points)
	{
		Object.AddPart(new HighBitBonus
		{
			Chance = 2 * Points,
			Amount = 1
		});
		return new FactionData
		{
			Name = "Grit Gate",
			Color = "c",
			Postfix = "It's embossed with angular lines meant to evoke schematics and circuitry."
		};
	}

	private static FactionData AddHindrenEffect(GameObject Object, int Points)
	{
		Object.AddPart(new AilingQuickness
		{
			SpeedBonus = 5 + 2 * Points
		});
		return new FactionData
		{
			Name = "Bey Lah",
			Color = "beylah",
			Postfix = "It's engraved with cervine hoofprints leading from the spokes to the center."
		};
	}

	private static FactionData AddKyakukyaEffect(GameObject Object, int Points)
	{
		Object.AddPart(new AddsRep
		{
			Faction = "Fungi",
			Value = 200,
			WorksOnCarrier = true
		});
		int num = Points / 3;
		Object.AddPart(new FungalFortitude
		{
			AVBonus = num,
			ResistBonus = 4 * num
		});
		return new FactionData
		{
			Name = "Kyakukya",
			Color = "r",
			Postfix = "It's etched with tiny mushrooms all over."
		};
	}

	private static FactionData AddEzraEffect(GameObject Object, int Points)
	{
		Object.AddPart(new AddsRep
		{
			Faction = "*alloldfactions",
			Value = 10 + 10 * Points,
			WorksOnCarrier = true
		});
		return new FactionData
		{
			Name = "Ezra",
			Color = "W",
			Postfix = "It's carved in relief with various types of musa."
		};
	}

	private static FactionData AddYdFreeholdEffect(GameObject Object, int Points)
	{
		Object.AddPart(new WaterRitualDiscount
		{
			Types = "Join",
			Percent = 17 + 3 * Points
		});
		return new FactionData
		{
			Name = "Yd Freehold",
			Color = "ydfreehold",
			Postfix = "It's carved in relief with figures of svardym hopping from the spokes toward the hub.",
			Article = true
		};
	}

	private static FactionData AddMechanimistsEffect(GameObject Object, int Points)
	{
		Object.AddPart(new CompanionCapacity
		{
			Proselytized = ((Points >= 3) ? 1 : 0),
			Beguiled = ((Points >= 7) ? 1 : 0)
		});
		return new FactionData
		{
			Name = "Six Day Stilt",
			Color = "C",
			Postfix = "It's engraved with a preacher, hand outstretched, on each spoke.",
			Article = true
		};
	}

	private static FactionData AddMopangoEffect(GameObject Object, int Points)
	{
		Object.AddPart(new ArtifactDetection
		{
			Radius = ((Points >= 10) ? 80 : (2 * (Points - 1) + 1)),
			IdentifyChance = 0
		});
		return new FactionData
		{
			Color = "Y",
			Postfix = "It's embossed with starbursts along each spoke and a circular rondure around the hub."
		};
	}

	private static FactionData AddPariahsEffect(GameObject Object, int Points)
	{
		Object.AddPart(new FeelingOnTarget
		{
			Chance = Points,
			Feeling = 25,
			CalmHate = false
		});
		return new FactionData
		{
			Color = "m",
			Postfix = "It's engraved with winding roads, a single lone figure walking them."
		};
	}
}
