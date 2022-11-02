using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Genkit;
using HistoryKit;
using XRL;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Skills.Cooking;
using XRL.World.WorldBuilders;

namespace Qud.API;

[HasWishCommand]
public static class JournalAPI
{
	public static List<string> _mapNoteCategories = null;

	private static Dictionary<string, List<JournalMapNote>> _mapNotesByZone = null;

	public static string[] attributesToExcludedFromRandomNotes = new string[1] { "hindrenclue" };

	public static bool sorting = true;

	public static List<JournalAccomplishment> Accomplishments
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.Accomplishments"))
			{
				if (XRLCore.Core.Game.Accomplishments.Count > 0)
				{
					List<JournalAccomplishment> value = new List<JournalAccomplishment>();
					for (int i = 0; i < XRLCore.Core.Game.Accomplishments.Count; i++)
					{
						new JournalAccomplishment
						{
							category = "general",
							text = XRLCore.Core.Game.Accomplishments[i],
							time = XRLCore.Core.Game.TimeTicks
						};
					}
					XRLCore.Core.Game.SetObjectGameState("Journal.Accomplishments", value);
				}
				else
				{
					XRLCore.Core.Game.SetObjectGameState("Journal.Accomplishments", new List<JournalAccomplishment>());
				}
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.Accomplishments") as List<JournalAccomplishment>;
		}
	}

	public static List<JournalObservation> Observations
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.Observations"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.Observations", new List<JournalObservation>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.Observations") as List<JournalObservation>;
		}
	}

	public static List<JournalMapNote> MapNotes
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.MapNotes"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.MapNotes", new List<JournalMapNote>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.MapNotes") as List<JournalMapNote>;
		}
	}

	public static List<JournalRecipeNote> RecipeNotes
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.RecipeNotes"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.RecipeNotes", new List<JournalRecipeNote>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.RecipeNotes") as List<JournalRecipeNote>;
		}
	}

	public static List<JournalGeneralNote> GeneralNotes
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.GeneralNotes"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.GeneralNotes", new List<JournalGeneralNote>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.GeneralNotes") as List<JournalGeneralNote>;
		}
	}

	public static List<JournalSultanNote> SultanNotes
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.SultanList"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.SultanList", new List<JournalSultanNote>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.SultanList") as List<JournalSultanNote>;
		}
	}

	public static int Count => Accomplishments.Count + MapNotes.Count + Observations.Count + RecipeNotes.Count + SultanNotes.Count + GeneralNotes.Count;

	public static List<JournalVillageNote> VillageNotes
	{
		get
		{
			if (!XRLCore.Core.Game.HasObjectGameState("Journal.VillageNoteList"))
			{
				XRLCore.Core.Game.SetObjectGameState("Journal.VillageNoteList", new List<JournalVillageNote>());
			}
			return XRLCore.Core.Game.GetObjectGameState("Journal.VillageNoteList") as List<JournalVillageNote>;
		}
	}

	private static Dictionary<string, List<JournalMapNote>> mapNotesByZone
	{
		get
		{
			if (_mapNotesByZone == null)
			{
				_mapNotesByZone = new Dictionary<string, List<JournalMapNote>>();
				foreach (JournalMapNote mapNote in MapNotes)
				{
					if (!_mapNotesByZone.ContainsKey(mapNote.zoneid))
					{
						_mapNotesByZone.Add(mapNote.zoneid, new List<JournalMapNote>());
					}
					_mapNotesByZone[mapNote.zoneid].Add(mapNote);
				}
			}
			return _mapNotesByZone;
		}
	}

	public static bool GetCategoryMapNoteToggle(string category)
	{
		return XRLCore.Core.Game.GetIntGameState(category + "_mapnotetoggle", 1) == 1;
	}

	public static void SetCategoryMapNoteToggle(string category, bool val)
	{
		XRLCore.Core.Game.SetIntGameState(category + "_mapnotetoggle", val ? 1 : 0);
		Cell currentCell = XRLCore.Core.Game.Player.Body.GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone.IsWorldMap())
		{
			currentCell.ParentZone.Activated();
		}
	}

	public static List<JournalSultanNote> GetSultanNotes(Func<JournalSultanNote, bool> condition = null)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (condition == null || condition(sultanNote))
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static IEnumerable<IBaseJournalEntry> GetAllNotes()
	{
		foreach (JournalAccomplishment accomplishment in Accomplishments)
		{
			yield return accomplishment;
		}
		foreach (JournalMapNote mapNote in MapNotes)
		{
			yield return mapNote;
		}
		foreach (JournalObservation observation in Observations)
		{
			yield return observation;
		}
		foreach (JournalRecipeNote recipeNote in RecipeNotes)
		{
			yield return recipeNote;
		}
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			yield return sultanNote;
		}
		foreach (JournalGeneralNote generalNote in GeneralNotes)
		{
			yield return generalNote;
		}
	}

	public static IEnumerable<IBaseJournalEntry> GetKnownNotes(Predicate<IBaseJournalEntry> filter = null)
	{
		if (filter == null)
		{
			filter = (IBaseJournalEntry n) => true;
		}
		foreach (IBaseJournalEntry allNote in GetAllNotes())
		{
			if (allNote.revealed && filter(allNote))
			{
				yield return allNote;
			}
		}
	}

	public static List<JournalSultanNote> GetKnownSultanNotes()
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.revealed)
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetKnownNotesForSultan(string sultan, Predicate<JournalSultanNote> filter = null)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.sultan == sultan && sultanNote.revealed && (filter == null || filter(sultanNote)))
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetKnownNotesForResheph(Predicate<JournalSultanNote> Filter = null)
	{
		return GetKnownNotesForSultan(HistoryAPI.GetResheph().id, Filter);
	}

	public static List<JournalSultanNote> GetNotesForSultan(string sultan)
	{
		List<JournalSultanNote> list = new List<JournalSultanNote>();
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.sultan == sultan)
			{
				list.Add(sultanNote);
			}
		}
		return list;
	}

	public static List<JournalSultanNote> GetNotesForResheph()
	{
		return GetNotesForSultan(HistoryAPI.GetResheph().id);
	}

	public static JournalSultanNote RevealSultanEventBySecretID(string id)
	{
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.secretid == id)
			{
				sultanNote.Reveal();
				return sultanNote;
			}
		}
		return null;
	}

	public static JournalSultanNote RevealSultanEvent(long id)
	{
		foreach (JournalSultanNote sultanNote in SultanNotes)
		{
			if (sultanNote.eventId == id)
			{
				sultanNote.Reveal();
				return sultanNote;
			}
		}
		return null;
	}

	public static bool KnowsSultanEvent(long id)
	{
		return SultanNotes.Any((JournalSultanNote n) => n.eventId == id && n.revealed);
	}

	public static bool HasUnrevealedSultanEvent(long id)
	{
		int i = 0;
		for (int count = SultanNotes.Count; i < count; i++)
		{
			if (SultanNotes[i].eventId == id)
			{
				return !SultanNotes[i].revealed;
			}
		}
		return false;
	}

	public static bool HasUnrevealedSultanEvent(string id)
	{
		int i = 0;
		for (int count = SultanNotes.Count; i < count; i++)
		{
			if (SultanNotes[i].secretid == id)
			{
				return !SultanNotes[i].revealed;
			}
		}
		return false;
	}

	public static List<JournalVillageNote> GetKnownNotesForVillage(string village)
	{
		List<JournalVillageNote> list = new List<JournalVillageNote>();
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.villageID == village && villageNote.revealed)
			{
				list.Add(villageNote);
			}
		}
		return list;
	}

	public static List<JournalVillageNote> GetNotesForVillage(string village)
	{
		List<JournalVillageNote> list = new List<JournalVillageNote>();
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.villageID == village)
			{
				list.Add(villageNote);
			}
		}
		return list;
	}

	public static bool IsMapOrVillageNoteRevealed(string secretID)
	{
		JournalMapNote journalMapNote = MapNotes.Find((JournalMapNote m) => m.secretid == secretID);
		if (journalMapNote != null && journalMapNote.revealed)
		{
			return true;
		}
		JournalVillageNote journalVillageNote = VillageNotes.Find((JournalVillageNote m) => m.secretid == secretID);
		if (journalVillageNote != null && journalVillageNote.revealed)
		{
			return true;
		}
		return false;
	}

	public static bool RevealVillageNote(string secretID)
	{
		JournalVillageNote journalVillageNote = VillageNotes.Find((JournalVillageNote n) => secretID == n.secretid);
		journalVillageNote?.Reveal();
		return journalVillageNote?.revealed ?? false;
	}

	public static void InitializeGossip()
	{
		int num = 60;
		for (int i = 0; i < num; i++)
		{
			string text = Guid.NewGuid().ToString();
			if (Stat.Random(1, 100) <= 75)
			{
				Faction randomFaction = Factions.GetRandomFaction();
				AddObservation(Grammar.InitCap(Gossip.GenerateGossip_OneFaction(randomFaction.Name)), text, "Gossip", text, new string[2]
				{
					"gossip:" + randomFaction.Name,
					"gossip"
				}, revealed: false, -1L);
				continue;
			}
			Faction randomFaction2 = Factions.GetRandomFaction();
			Faction faction = null;
			do
			{
				faction = Factions.GetRandomFaction();
			}
			while (faction == randomFaction2);
			AddObservation(Grammar.InitCap(Gossip.GenerateGossip_TwoFactions(randomFaction2.Name, faction.Name)), text, "Gossip", text, new string[3]
			{
				"gossip:" + randomFaction2.Name,
				"gossip:" + faction.Name,
				"gossip"
			}, revealed: false, -1L);
		}
	}

	public static void InitializeObservations()
	{
		AddObservation("Yurl claims that Asphodel can be swayed via social favors, high reputation with the Consortium of Phyta, or brute force.", "YurlAsphodel", "general", null, new string[1] { "Consortium" }, revealed: false, -1L, null, initCapAsFragment: true);
		AddObservation("Qud was once called Salum.", "QudSalum", "general", null, new string[1] { "old" }, revealed: false, -1L, "{{W|Qud was once called Salum.}}\n\n", initCapAsFragment: true);
		AddObservation("The shomer Rainwater claims that Brightsheol is the dream of a seraph who lives atop the Spindle.", "BrightsheolDream", "Gossip", null, new string[2] { "gossip", "old" }, revealed: false, -1L, "{{W|Rainwater Shomer claims that Brightsheol is the dream of a seraph who lives atop the Spindle.}}\n\n", initCapAsFragment: true);
		AddObservation("The Palladium Reef was once called Maqqom Yd, the Place outside Itself.", "MaqqomYd", "general", null, new string[1] { "old" }, revealed: false, -1L, "{{W|The Palladium Reef was once called Maqqom Yd, the Place outside Itself.}}\n\n", initCapAsFragment: true);
	}

	public static void InitializeVillageEntries()
	{
		foreach (HistoricEntity village in HistoryAPI.GetVillages())
		{
			foreach (string item in village.GetCurrentSnapshot().GetList("Gospels"))
			{
				JournalVillageNote journalVillageNote = new JournalVillageNote();
				journalVillageNote.secretid = Guid.NewGuid().ToString();
				journalVillageNote.text = item;
				journalVillageNote.villageID = village.id;
				journalVillageNote.attributes.Add("village");
				VillageNotes.Add(journalVillageNote);
			}
		}
		JournalVillageNote journalVillageNote2 = new JournalVillageNote();
		journalVillageNote2.secretid = "KyakukyaSecret1";
		journalVillageNote2.text = "Under the Beetle Moon, folks settled here to gather goods of passage for Ape Saad Oboroqoru, and so Kyakukya was founded.";
		journalVillageNote2.villageID = "Kyakukya";
		journalVillageNote2.attributes.Add("village");
		VillageNotes.Add(journalVillageNote2);
		JournalVillageNote journalVillageNote3 = new JournalVillageNote();
		journalVillageNote3.secretid = "KyakukyaSecret2";
		journalVillageNote3.text = "In 979, Nuntu, the legendary albino ape, grew tired of bludgeoning things to death and journeyed to a root-strangled place on the River Svy. There he came upon Kyakukya and its inhabitants. The villagers of Kyakukya asked Nuntu to employ his magisterial skills and lead the village.";
		journalVillageNote3.villageID = "Kyakukya";
		journalVillageNote3.attributes.Add("village");
		VillageNotes.Add(journalVillageNote3);
		JournalVillageNote journalVillageNote4 = new JournalVillageNote();
		journalVillageNote4.secretid = "YdFreeholdSecret1";
		journalVillageNote4.text = "Centuries after the height of the Gyre, the svardym Goek, Mak, and Geeub met the galgal Many Eyes while languishing inside of a reef sponge. Together they founded the Yd Freehold to live out the rest of their lives.";
		journalVillageNote4.villageID = "The Yd Freehold";
		journalVillageNote4.attributes.Add("village");
		VillageNotes.Add(journalVillageNote4);
		JournalVillageNote journalVillageNote5 = new JournalVillageNote();
		journalVillageNote5.secretid = "YdFreeholdSecret2";
		journalVillageNote5.text = "In 901, the villagers of the Yd Freehold and its founders abolished all forms of hierarchy from their settlement. The two pillars of civic life in the Freehold thus became anarchy and authenticity.";
		journalVillageNote5.villageID = "The Yd Freehold";
		journalVillageNote5.attributes.Add("village");
		VillageNotes.Add(journalVillageNote5);
	}

	public static void InitializeSultanEntries()
	{
		foreach (HistoricEntity sultan in HistoryAPI.GetSultans())
		{
			List<string> sultanLikedFactions = HistoryAPI.GetSultanLikedFactions(sultan);
			List<string> sultanHatedFactions = HistoryAPI.GetSultanHatedFactions(sultan);
			foreach (HistoricEvent sultanEventsWithGospel in HistoryAPI.GetSultanEventsWithGospels(sultan.id))
			{
				JournalSultanNote journalSultanNote = new JournalSultanNote();
				journalSultanNote.secretid = Guid.NewGuid().ToString();
				journalSultanNote.text = sultanEventsWithGospel.GetEventProperty("gospel");
				journalSultanNote.sultan = sultan.id;
				journalSultanNote.eventId = sultanEventsWithGospel.id;
				journalSultanNote.Forget(fast: true);
				foreach (string item in sultanLikedFactions)
				{
					journalSultanNote.attributes.Add("include:" + item);
				}
				foreach (string item2 in sultanHatedFactions)
				{
					journalSultanNote.attributes.Add("include:" + item2);
				}
				journalSultanNote.attributes.Add("sultan");
				if (sultanEventsWithGospel.GetEventProperty("rebekah") == "true")
				{
					journalSultanNote.attributes.Add("rebekah");
				}
				if (sultanEventsWithGospel.GetEventProperty("rebekahWasHealer") == "true")
				{
					journalSultanNote.attributes.Add("rebekahWasHealer");
				}
				if (sultanEventsWithGospel.GetEventProperty("gyreplagues") == "true")
				{
					journalSultanNote.attributes.Add("gyreplagues");
				}
				SultanNotes.Add(journalSultanNote);
			}
			foreach (HistoricEvent sultanEventsWithTombInscription in HistoryAPI.GetSultanEventsWithTombInscriptions(sultan.id))
			{
				JournalSultanNote journalSultanNote2 = new JournalSultanNote();
				journalSultanNote2.secretid = Guid.NewGuid().ToString();
				journalSultanNote2.text = sultanEventsWithTombInscription.GetEventProperty("tombInscription");
				journalSultanNote2.sultan = sultan.id;
				journalSultanNote2.eventId = sultanEventsWithTombInscription.id;
				journalSultanNote2.Forget(fast: true);
				foreach (string item3 in sultanLikedFactions)
				{
					journalSultanNote2.attributes.Add("include:" + item3);
				}
				foreach (string item4 in sultanHatedFactions)
				{
					journalSultanNote2.attributes.Add("include:" + item4);
				}
				journalSultanNote2.attributes.Add("sultanTombPropaganda");
				journalSultanNote2.attributes.Add("onlySellIfTargetedAndInterested");
				SultanNotes.Add(journalSultanNote2);
			}
		}
	}

	public static void DeleteAccomplishment(JournalAccomplishment accomplishmentToDelete)
	{
		Accomplishments.Remove(accomplishmentToDelete);
	}

	public static void DeleteGeneralNote(JournalGeneralNote noteToDelete)
	{
		GeneralNotes.Remove(noteToDelete);
	}

	public static void AddAccomplishment(string text, string muralText = null, string category = "general", JournalAccomplishment.MuralCategory muralCategory = JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight muralWeight = JournalAccomplishment.MuralWeight.Medium, string secretId = null, long time = -1L, bool revealed = true)
	{
		JournalAccomplishment journalAccomplishment = new JournalAccomplishment();
		journalAccomplishment.category = category;
		journalAccomplishment.muralCategory = muralCategory;
		journalAccomplishment.muralWeight = muralWeight;
		journalAccomplishment.muralText = HistoricStringExpander.ExpandString(muralText);
		if (muralText == null && muralWeight != 0)
		{
			string word = text.Replace("You ", XRLCore.Core.Game.PlayerName + " ").Replace("you ", XRLCore.Core.Game.PlayerName + " ").Replace("Your ", Grammar.MakePossessive(XRLCore.Core.Game.PlayerName) + " ")
				.Replace("your ", Grammar.MakePossessive(XRLCore.Core.Game.PlayerName) + " ");
			journalAccomplishment.muralText = Grammar.InitCap(word);
			journalAccomplishment.muralCategory = JournalAccomplishment.MuralCategory.DoesSomethingRad;
			journalAccomplishment.muralWeight = JournalAccomplishment.MuralWeight.Medium;
		}
		if (XRLCore.Core.Game != null && XRLCore.Core.Game.Player != null && XRLCore.Core.Game.Player.Body != null)
		{
			if (XRLCore.Core.Game.Player.Body.HasEffect("Lovesick"))
			{
				Lovesick lovesick = XRLCore.Core.Game.Player.Body.GetEffect("Lovesick") as Lovesick;
				text = HistoricStringExpander.ExpandString("<spice.lovesick.!random>", null, null).Replace("*var*", text).Replace("*varLower*", Grammar.InitLower(text))
					.Replace("*sob*", Grammar.Stutterize(text, "{{w|*sob*}}"))
					.Replace("*varReplacePeriod*", text.Replace(".", ""))
					.Replace("*obj*", lovesick.Beauty.the + lovesick.Beauty.ShortDisplayName);
			}
			if (XRLCore.Core.Game.Player.Body.pPhysics.Temperature > XRLCore.Core.Game.Player.Body.pPhysics.FlameTemperature)
			{
				text = "While on fire, " + Grammar.InitLower(text);
			}
		}
		journalAccomplishment.text = text;
		journalAccomplishment.secretid = secretId;
		journalAccomplishment.secretSold = true;
		if (time == -1)
		{
			journalAccomplishment.time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalAccomplishment.time = time;
		}
		Accomplishments.Add(journalAccomplishment);
		if (sorting)
		{
			Accomplishments.Sort((JournalAccomplishment a, JournalAccomplishment b) => a.time.CompareTo(b.time));
		}
		try
		{
			Physics pPhysics = XRLCore.Core.Game.Player.Body.pPhysics;
			if (pPhysics != null && pPhysics.CurrentCell != null)
			{
				pPhysics.CurrentCell.ParentZone.FireEvent(Event.New("AccomplishmentAdded", "Text", text, "Category", category, "SecretId", secretId));
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
		if (revealed)
		{
			journalAccomplishment.Reveal();
		}
	}

	public static void Init()
	{
		_mapNotesByZone = null;
		_mapNoteCategories = null;
	}

	public static void AddObservation(string text, string id, string category = "general", string secretId = null, string[] attributes = null, bool revealed = false, long time = -1L, string additionalRevealText = null, bool initCapAsFragment = false)
	{
		if (GetObservation(id) != null)
		{
			return;
		}
		JournalObservation journalObservation = new JournalObservation();
		journalObservation.id = id;
		journalObservation.text = text;
		journalObservation.secretid = secretId;
		if (revealed)
		{
			journalObservation.Reveal();
		}
		else
		{
			journalObservation.Forget();
		}
		journalObservation.additionalRevealText = additionalRevealText;
		journalObservation.initCapAsFragment = initCapAsFragment;
		if (attributes != null)
		{
			journalObservation.attributes.AddRange(attributes);
		}
		if (time == -1)
		{
			journalObservation.time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalObservation.time = time;
		}
		journalObservation.category = category;
		Observations.Add(journalObservation);
		if (sorting)
		{
			Observations.Sort((JournalObservation a, JournalObservation b) => a.time.CompareTo(b.time));
		}
	}

	public static void RevealObservation(JournalObservation note)
	{
		note.Reveal();
		note.Updated();
	}

	public static bool IsObservationRevealed(string secretID)
	{
		JournalObservation journalObservation = Observations.Find((JournalObservation m) => m.secretid == secretID);
		if (journalObservation != null && journalObservation.revealed)
		{
			return true;
		}
		return false;
	}

	public static void RevealObservation(string id, bool onlyIfNotRevealed = false)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.id == id && (!onlyIfNotRevealed || !observation.revealed))
			{
				RevealObservation(observation);
			}
		}
	}

	public static JournalObservation GetObservation(string id)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.id == id)
			{
				return observation;
			}
		}
		return null;
	}

	public static List<JournalObservation> GetObservations(Func<JournalObservation, bool> compare = null)
	{
		List<JournalObservation> list = new List<JournalObservation>();
		foreach (JournalObservation observation in Observations)
		{
			if (compare == null || compare(observation))
			{
				list.Add(observation);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetRevealedMapNotesForWorldMapCell(int x, int y)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.revealed && mapNote.wx == x && mapNote.wy == y)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetMapNotesForWorldMapCell(int x, int y)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.wx == x && mapNote.wy == y)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<string> GetMapNoteCategories()
	{
		if (_mapNoteCategories != null)
		{
			return _mapNoteCategories;
		}
		_mapNoteCategories = new List<string>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.revealed && !_mapNoteCategories.Contains(mapNote.category))
			{
				_mapNoteCategories.Add(mapNote.category);
			}
		}
		_mapNoteCategories.Sort();
		return _mapNoteCategories;
	}

	public static List<JournalMapNote> GetMapNotesForZone(string zoneId)
	{
		if (mapNotesByZone.TryGetValue(zoneId, out var value))
		{
			return value;
		}
		return new List<JournalMapNote>();
	}

	public static JournalMapNote GetMapNote(string secretId)
	{
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.secretid == secretId)
			{
				return mapNote;
			}
		}
		return null;
	}

	public static List<JournalMapNote> GetMapNotes(Func<JournalMapNote, bool> compare = null)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (compare == null || compare(mapNote))
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static List<JournalMapNote> GetMapNotesWithAllAttributes(string attributes)
	{
		return GetMapNotes(delegate(JournalMapNote note)
		{
			bool result = true;
			string[] array = attributes.Split(',');
			foreach (string att in array)
			{
				if (!note.Has(att))
				{
					result = false;
					break;
				}
			}
			return result;
		});
	}

	public static List<JournalMapNote> GetMapNotesInCardinalDirections(string zoneid)
	{
		Zone z = new Zone();
		z.ZoneID = zoneid;
		List<JournalMapNote> mapNotes = GetMapNotes(delegate(JournalMapNote note)
		{
			if (string.Equals(note.category, "Miscellaneous"))
			{
				return false;
			}
			if (z.resolvedLocation == note.location)
			{
				return false;
			}
			if (z.resolvedX == note.x && z.resolvedY == note.y)
			{
				return false;
			}
			if (z.resolvedX == note.x)
			{
				return true;
			}
			return (z.resolvedY == note.y) ? true : false;
		});
		if (mapNotes.Count == 0)
		{
			return null;
		}
		mapNotes.Sort((JournalMapNote a, JournalMapNote b) => z.resolvedLocation.Distance(a.location).CompareTo(z.resolvedLocation.Distance(b.location)));
		return mapNotes;
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinWorldRadiusN(string zoneid, int min, int max)
	{
		Zone z = new Zone();
		z.ZoneID = zoneid;
		return GetMapNotes(delegate(JournalMapNote note)
		{
			if (note.revealed)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.wx) > max)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.wy) > max)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) > max)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.wx) < min)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.wy) < min)
			{
				return false;
			}
			return (Math.Abs(z.Z - note.cz) >= min) ? true : false;
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinZoneRadiusN(string zoneid, int min, int max, Predicate<Location2D> isValid = null)
	{
		Zone z = new Zone();
		z.ZoneID = zoneid;
		return GetMapNotes(delegate(JournalMapNote note)
		{
			if (note.revealed)
			{
				return false;
			}
			if (note.zoneid == zoneid)
			{
				return false;
			}
			if (Math.Abs(z.resolvedX - note.x) > max)
			{
				return false;
			}
			if (Math.Abs(z.resolvedY - note.y) > max)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) > 0)
			{
				return false;
			}
			if (Math.Abs(z.resolvedX - note.x) < min)
			{
				return false;
			}
			if (Math.Abs(z.resolvedY - note.y) < min)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) > 0)
			{
				return false;
			}
			return isValid == null || isValid(note.location);
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinWorldRadiusN(Zone z, int min, int max, Predicate<Location2D> isValid = null)
	{
		return GetMapNotes(delegate(JournalMapNote note)
		{
			if (note.revealed)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.wx) > max)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.wy) > max)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) > max)
			{
				return false;
			}
			if (Math.Abs(z.wX - note.wx) < min)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.wy) < min)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) < min)
			{
				return false;
			}
			return isValid == null || isValid(note.location);
		});
	}

	public static List<JournalMapNote> GetUnrevealedMapNotesWithinZoneRadiusN(Zone z, int min, int max, Predicate<Location2D> isValid = null)
	{
		return GetMapNotes(delegate(JournalMapNote note)
		{
			if (note.revealed)
			{
				return false;
			}
			if (Math.Abs(z.resolvedX - note.x) > max)
			{
				return false;
			}
			if (Math.Abs(z.resolvedY - note.y) > max)
			{
				return false;
			}
			if (Math.Abs(z.resolvedX - note.x) < min)
			{
				return false;
			}
			if (Math.Abs(z.resolvedY - note.y) < min)
			{
				return false;
			}
			if (Math.Abs(z.Z - note.cz) > 0)
			{
				return false;
			}
			return isValid == null || isValid(note.location);
		});
	}

	public static List<JournalMapNote> GetMapNotesWithinRadiusN(Zone z, int radius)
	{
		return GetMapNotes(delegate(JournalMapNote note)
		{
			if (string.Equals(note.category, "Miscellaneous"))
			{
				return false;
			}
			if (Math.Abs(z.wX - note.wx) > radius)
			{
				return false;
			}
			if (Math.Abs(z.wY - note.wy) > radius)
			{
				return false;
			}
			return (Math.Abs(z.Z - note.cz) <= radius) ? true : false;
		});
	}

	public static List<JournalRecipeNote> GetRecipes(Func<JournalRecipeNote, bool> compare = null)
	{
		List<JournalRecipeNote> list = new List<JournalRecipeNote>();
		foreach (JournalRecipeNote recipeNote in RecipeNotes)
		{
			if (compare == null || compare(recipeNote))
			{
				list.Add(recipeNote);
			}
		}
		return list;
	}

	public static bool HasObservation(string id)
	{
		foreach (JournalObservation observation in Observations)
		{
			if (observation.id == id && observation.revealed)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasObservationWithTag(string tag)
	{
		return GetObservations((JournalObservation note) => note.revealed && note.Has(tag)).Count > 0;
	}

	public static bool HasSultanNoteWithTag(string tag)
	{
		return GetSultanNotes((JournalSultanNote note) => note.revealed && note.Has(tag)).Count > 0;
	}

	public static bool HasVillageNote(string id)
	{
		foreach (JournalVillageNote villageNote in VillageNotes)
		{
			if (villageNote.secretid == id && villageNote.revealed)
			{
				return true;
			}
		}
		return false;
	}

	public static void DeleteMapNote(JournalMapNote note)
	{
		_mapNoteCategories = null;
		MapNotes.Remove(note);
		if (note.zoneid != null && mapNotesByZone.TryGetValue(note.zoneid, out var value))
		{
			value.Remove(note);
		}
	}

	public static IBaseJournalEntry GetRandomRevealedNote(Predicate<IBaseJournalEntry> filter = null)
	{
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>();
		list.AddRange((from c in ((IEnumerable<JournalAccomplishment>)Accomplishments).Select((Func<JournalAccomplishment, IBaseJournalEntry>)((JournalAccomplishment c) => c))
			where c.revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalObservation>)Observations).Select((Func<JournalObservation, IBaseJournalEntry>)((JournalObservation c) => c))
			where c.revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalSultanNote>)GetKnownSultanNotes()).Select((Func<JournalSultanNote, IBaseJournalEntry>)((JournalSultanNote c) => c))
			where c.revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalMapNote>)MapNotes).Select((Func<JournalMapNote, IBaseJournalEntry>)((JournalMapNote c) => c))
			where c.revealed && (filter == null || filter(c))
			select c).ToList());
		list.AddRange((from c in ((IEnumerable<JournalRecipeNote>)RecipeNotes).Select((Func<JournalRecipeNote, IBaseJournalEntry>)((JournalRecipeNote c) => c))
			where c.revealed && (filter == null || filter(c))
			select c).ToList());
		return list.GetRandomElement();
	}

	public static IBaseJournalEntry GetRandomUnrevealedNote(Predicate<IBaseJournalEntry> filter = null)
	{
		List<IBaseJournalEntry> list = new List<IBaseJournalEntry>();
		List<JournalSultanNote> sultanNotes = GetSultanNotes((JournalSultanNote note) => !note.revealed && !IsNoteExcluded(note));
		List<JournalObservation> observations = GetObservations((JournalObservation note) => !note.revealed && !IsNoteExcluded(note));
		List<JournalMapNote> mapNotes = GetMapNotes((JournalMapNote note) => !note.revealed && !IsNoteExcluded(note));
		foreach (JournalSultanNote item in sultanNotes)
		{
			if (filter == null || filter(item))
			{
				list.Add(item);
			}
		}
		foreach (JournalObservation item2 in observations)
		{
			if (filter == null || filter(item2))
			{
				list.Add(item2);
			}
		}
		foreach (JournalMapNote item3 in mapNotes)
		{
			if (filter == null || filter(item3))
			{
				list.Add(item3);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list.GetRandomElement();
	}

	public static void RevealRandomSecret()
	{
		GetRandomUnrevealedNote().Reveal();
	}

	public static void RevealMapNote(JournalMapNote note, bool silent = false)
	{
		note.Reveal(silent);
	}

	public static void AddMapNote(JournalMapNote newNote)
	{
		if (!MapNotes.Contains(newNote))
		{
			MapNotes.Add(newNote);
			if (!mapNotesByZone.TryGetValue(newNote.zoneid, out var value))
			{
				value = (mapNotesByZone[newNote.zoneid] = new List<JournalMapNote>());
			}
			if (!value.Contains(newNote))
			{
				value.Add(newNote);
			}
		}
	}

	public static void AddMapNote(string ZoneID, string text, string category = "general", string[] attributes = null, string secretId = null, bool revealed = false, bool sold = false, long time = -1L, bool silent = false)
	{
		if (!ZoneID.Contains("."))
		{
			int x = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.X;
			int y = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.Y;
			string stringGameState = XRLCore.Core.Game.GetStringGameState("LastLocationOnSurface");
			ZoneID = ZoneID + "." + x + "." + y + ".2.2.10";
			if (stringGameState.Contains("."))
			{
				string[] array = stringGameState.Split('.');
				if (Convert.ToInt16(array[1]) == x && Convert.ToInt16(array[2]) == y)
				{
					ZoneID = stringGameState.Split('@')[0];
				}
			}
		}
		_mapNoteCategories = null;
		JournalMapNote journalMapNote = new JournalMapNote();
		if (time == -1)
		{
			journalMapNote.time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalMapNote.time = time;
		}
		journalMapNote.secretSold = sold;
		journalMapNote.text = text;
		journalMapNote.zoneid = ZoneID;
		journalMapNote.category = category;
		journalMapNote.secretid = secretId;
		if (attributes != null)
		{
			journalMapNote.attributes.AddRange(attributes);
		}
		string[] array2 = ZoneID.Split('.');
		if (array2.Length > 1)
		{
			journalMapNote.w = array2[0];
			journalMapNote.wx = Convert.ToInt32(array2[1]);
			journalMapNote.wy = Convert.ToInt32(array2[2]);
			journalMapNote.cx = Convert.ToInt32(array2[3]);
			journalMapNote.cy = Convert.ToInt32(array2[4]);
			journalMapNote.cz = Convert.ToInt32(array2[5]);
		}
		else
		{
			journalMapNote.w = ZoneID;
			journalMapNote.wx = -1;
			journalMapNote.wy = -1;
			journalMapNote.cx = -1;
			journalMapNote.cy = -1;
			journalMapNote.cz = -1;
		}
		MapNotes.Add(journalMapNote);
		if (sorting)
		{
			journalMapNote.Forget(fast: true);
			MapNotes.Sort((JournalMapNote a, JournalMapNote b) => a.text.CompareTo(b.text));
			if (revealed)
			{
				RevealMapNote(journalMapNote, silent);
			}
		}
		else if (revealed)
		{
			journalMapNote.Reveal(silent);
		}
		else
		{
			journalMapNote.Forget(fast: true);
		}
		if (!mapNotesByZone.TryGetValue(journalMapNote.zoneid, out var value))
		{
			value = (mapNotesByZone[journalMapNote.zoneid] = new List<JournalMapNote>());
		}
		value.Add(journalMapNote);
	}

	public static List<JournalMapNote> GetMapNotesForColumn(string world, int wx, int wy)
	{
		List<JournalMapNote> list = new List<JournalMapNote>();
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (mapNote.w == world && mapNote.wx == wx && mapNote.wy == wy)
			{
				list.Add(mapNote);
			}
		}
		return list;
	}

	public static JournalRecipeNote AddRecipeNote(CookingRecipe recipe, bool revealed = true, bool silent = false, string id = null)
	{
		if (id == null)
		{
			id = Guid.NewGuid().ToString();
		}
		JournalRecipeNote journalRecipeNote = new JournalRecipeNote();
		journalRecipeNote.recipe = recipe;
		journalRecipeNote.text = recipe.GetDisplayName() + "\n" + recipe.GetIngredients() + "\n\n" + recipe.GetDescription();
		journalRecipeNote.secretid = id;
		journalRecipeNote.attributes.Add("recipe");
		journalRecipeNote.attributes.Add(id);
		RecipeNotes.Add(journalRecipeNote);
		if (sorting)
		{
			RecipeNotes.Sort((JournalRecipeNote a, JournalRecipeNote b) => a.text.CompareTo(b.text));
		}
		if (revealed)
		{
			journalRecipeNote.Reveal(silent);
		}
		return journalRecipeNote;
	}

	public static void DeleteRecipeNote(JournalRecipeNote note)
	{
		RecipeNotes.Remove(note);
	}

	public static JournalGeneralNote AddGeneralNote(string text, string secretId = null, long time = -1L, bool revealed = true)
	{
		JournalGeneralNote journalGeneralNote = new JournalGeneralNote();
		journalGeneralNote.text = text;
		journalGeneralNote.secretid = secretId;
		journalGeneralNote.secretSold = true;
		if (time == -1)
		{
			journalGeneralNote.time = XRLCore.Core.Game.TimeTicks;
		}
		else
		{
			journalGeneralNote.time = time;
		}
		GeneralNotes.Add(journalGeneralNote);
		if (sorting)
		{
			GeneralNotes.Sort((JournalGeneralNote a, JournalGeneralNote b) => a.time.CompareTo(b.time));
		}
		if (revealed)
		{
			journalGeneralNote.Reveal();
		}
		return journalGeneralNote;
	}

	public static void SuspendSorting()
	{
		sorting = false;
	}

	public static void ResumeSorting()
	{
		sorting = true;
		MapNotes.Sort((JournalMapNote a, JournalMapNote b) => a.text.CompareTo(b.text));
		Accomplishments.Sort((JournalAccomplishment a, JournalAccomplishment b) => a.time.CompareTo(b.time));
		Observations.Sort((JournalObservation a, JournalObservation b) => a.time.CompareTo(b.time));
		GeneralNotes.Sort((JournalGeneralNote a, JournalGeneralNote b) => a.time.CompareTo(b.time));
		RecipeNotes.Sort((JournalRecipeNote a, JournalRecipeNote b) => a.text.CompareTo(b.text));
	}

	public static bool IsNoteExcluded(IBaseJournalEntry note)
	{
		string[] array = attributesToExcludedFromRandomNotes;
		foreach (string att in array)
		{
			if (note.Has(att))
			{
				return true;
			}
		}
		return false;
	}

	[WishCommand(null, null, Regex = "^reveal\\s*settlements?$")]
	public static bool HandleRevealSettlementsWish(Match match)
	{
		foreach (JournalMapNote mapNote in MapNotes)
		{
			if (!mapNote.revealed && mapNote.category == "Settlements" && mapNote.Has("villages"))
			{
				RevealMapNote(mapNote);
			}
		}
		return true;
	}

	[Obsolete("save compat")]
	internal static void FixMerchantMapNotes()
	{
		WorldInfo worldInfo = The.Game.GetObjectGameState("JoppaWorldInfo") as WorldInfo;
		if (worldInfo?.lairs == null)
		{
			return;
		}
		foreach (GeneratedLocationInfo lair in worldInfo.lairs)
		{
			if (lair.ownerID.IsNullOrEmpty() || lair.secretID.IsNullOrEmpty())
			{
				continue;
			}
			JournalMapNote mapNote = GetMapNote(lair.secretID);
			if (mapNote == null || !The.ZoneManager.CachedObjects.TryGetValue(lair.ownerID, out var value))
			{
				continue;
			}
			List<string> list = value.GetTag("SecretAdjectives")?.CachedCommaExpansion();
			if (list.IsNullOrEmpty())
			{
				continue;
			}
			foreach (string item in list)
			{
				if (!mapNote.attributes.Contains(item))
				{
					mapNote.attributes.Add(item);
				}
			}
		}
	}
}
