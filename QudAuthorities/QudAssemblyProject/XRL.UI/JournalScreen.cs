using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using Rewired;
using XRL.Language;
using XRL.World;

namespace XRL.UI;

public class JournalScreen : IScreen
{
	[Serializable]
	public class JournalEntry
	{
		public IBaseJournalEntry baseEntry;

		public int entryAPIPosition;

		public string entry;

		public int topLine;

		public int lines;
	}

	private class CursorMemory
	{
		public int Position;

		public int Cursor;
	}

	private class TabMemory : CursorMemory
	{
		public static TabMemory[] Stock;

		public string Selected;

		public Dictionary<string, CursorMemory> Categories = new Dictionary<string, CursorMemory>();

		public void Memorize(string Category, int Position, int Cursor)
		{
			CursorMemory value = this;
			if (Category != null && !Categories.TryGetValue(Category, out value))
			{
				value = (Categories[Category] = new CursorMemory());
			}
			value.Position = Position;
			value.Cursor = Cursor;
		}

		public void Recall(string Category, out int Position, out int Cursor)
		{
			CursorMemory value;
			if (Category == null)
			{
				Position = base.Position;
				Cursor = base.Cursor;
			}
			else if (Categories.TryGetValue(Category, out value))
			{
				Position = value.Position;
				Cursor = value.Cursor;
			}
			else
			{
				Position = 0;
				Cursor = 0;
			}
		}
	}

	public static readonly string STR_LOCATIONS = "Locations";

	public static readonly string STR_CHRONOLOGY = "Chronology";

	public static readonly string STR_OBSERVATIONS = "Gossip and Lore";

	public static readonly string STR_SULTANS = "Sultan Histories";

	public static readonly string STR_VILLAGES = "Village Histories";

	public static readonly string STR_GENERAL = "General Notes";

	public static readonly string STR_RECIPES = "Recipes";

	private List<string> categories = new List<string>();

	private List<JournalEntry> entries = new List<JournalEntry>();

	private List<string> displayLines = new List<string>();

	private List<int> entryForDisplayLine = new List<int>();

	private int cursorPosition;

	private int currentTopLine;

	public string selectedCategory;

	private int LastHash;

	private List<IBaseJournalEntry> LastRaw = new List<IBaseJournalEntry>();

	public void Memorize(int Tab, bool Tabinate = false)
	{
		TabMemory[] stock = TabMemory.Stock;
		TabMemory tabMemory = stock[Tab] ?? (stock[Tab] = new TabMemory());
		tabMemory.Memorize(selectedCategory, currentTopLine, cursorPosition);
		if (Tabinate)
		{
			tabMemory.Selected = selectedCategory;
		}
	}

	public void Recall(int Tab, bool Tabinate = false)
	{
		TabMemory[] stock = TabMemory.Stock;
		TabMemory tabMemory = stock[Tab] ?? (stock[Tab] = new TabMemory());
		if (Tabinate)
		{
			selectedCategory = tabMemory.Selected;
		}
		tabMemory.Recall(selectedCategory, out currentTopLine, out cursorPosition);
	}

	public List<IBaseJournalEntry> GetRawEntriesFor(string Tab, string Category = null)
	{
		int num = Tab.GetHashCode() ^ JournalAPI.Count;
		if (Category != null)
		{
			num ^= Category.GetHashCode();
		}
		if (LastHash == num)
		{
			return LastRaw;
		}
		LastHash = num;
		LastRaw.Clear();
		if (Tab == STR_CHRONOLOGY)
		{
			LastRaw.AddRange(JournalAPI.Accomplishments.Where((JournalAccomplishment c) => c.revealed));
		}
		else if (Tab == STR_OBSERVATIONS)
		{
			LastRaw.AddRange(JournalAPI.Observations.Where((JournalObservation c) => c.revealed));
		}
		else if (Tab == STR_SULTANS)
		{
			LastRaw.AddRange(from c in JournalAPI.SultanNotes
				where c.revealed && c.sultan == Category
				orderby c.eventId, c.Has("sultan") descending
				select c);
		}
		else if (Tab == STR_VILLAGES)
		{
			LastRaw.AddRange(JournalAPI.VillageNotes.Where((JournalVillageNote c) => c.revealed));
		}
		else if (Tab == STR_LOCATIONS)
		{
			LastRaw.AddRange(from c in JournalAPI.MapNotes
				where c.revealed && c.category == Category
				select c into x
				orderby x.tracked descending, x.Visited, x.text, x.time
				select x);
		}
		else if (Tab == STR_GENERAL)
		{
			LastRaw.AddRange(JournalAPI.GeneralNotes.Where((JournalGeneralNote x) => x.revealed));
		}
		else if (Tab == STR_RECIPES)
		{
			LastRaw.AddRange(JournalAPI.RecipeNotes.Where((JournalRecipeNote x) => x.revealed));
		}
		return LastRaw;
	}

	public void UpdateEntries(string selectedTab)
	{
		displayLines.Clear();
		entries.Clear();
		entryForDisplayLine.Clear();
		categories.Clear();
		if (selectedCategory == null)
		{
			if (selectedTab == STR_VILLAGES)
			{
				if (JournalAPI.GetKnownNotesForVillage("Kyakukya").Count > 0)
				{
					displayLines.Add("Kyakukya");
					categories.Add("Kyakukya");
				}
				if (JournalAPI.GetKnownNotesForVillage("The Yd Freehold").Count > 0)
				{
					displayLines.Add("The Yd Freehold");
					categories.Add("The Yd Freehold");
				}
				foreach (HistoricEntity knownVillage in HistoryAPI.GetKnownVillages())
				{
					displayLines.Add(knownVillage.GetCurrentSnapshot().GetProperty("name", knownVillage.id));
					categories.Add(knownVillage.id);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no knowledge of any villages.}}");
				}
				return;
			}
			if (selectedTab == STR_SULTANS)
			{
				foreach (HistoricEntity knownSultan in HistoryAPI.GetKnownSultans())
				{
					displayLines.Add(knownSultan.GetCurrentSnapshot().GetProperty("name", knownSultan.id));
					categories.Add(knownSultan.id);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no knowledge of the sultans.}}");
				}
				return;
			}
			if (selectedTab == STR_LOCATIONS)
			{
				foreach (string mapNoteCategory in JournalAPI.GetMapNoteCategories())
				{
					string item = (JournalAPI.GetCategoryMapNoteToggle(mapNoteCategory) ? "[{{G|X}}] " : "[ ] ") + mapNoteCategory;
					displayLines.Add(item);
					categories.Add(mapNoteCategory);
				}
				if (displayLines.Count == 0)
				{
					displayLines.Add("{{K|You have no map notes.}}");
				}
				return;
			}
		}
		int num = 0;
		foreach (IBaseJournalEntry item2 in GetRawEntriesFor(selectedTab, selectedCategory))
		{
			JournalEntry journalEntry = new JournalEntry();
			journalEntry.entry = item2.GetDisplayText();
			if (Options.DebugInternals && selectedTab == STR_CHRONOLOGY && item2 is JournalAccomplishment)
			{
				string muralText = (item2 as JournalAccomplishment).muralText;
				journalEntry.entry = journalEntry.entry + "\n\n{{internals|Ã " + muralText.Replace("=name=", The.Player.BaseDisplayNameStripped) + "}}";
			}
			journalEntry.baseEntry = item2;
			journalEntry.entryAPIPosition = num;
			entries.Add(journalEntry);
			num++;
		}
		int num2 = 0;
		if (selectedTab == STR_SULTANS && selectedCategory != null)
		{
			displayLines.Add("[History of " + HistoryAPI.GetEntityName(selectedCategory) + "]");
			displayLines.Add("");
			entryForDisplayLine.Add(0);
			entryForDisplayLine.Add(0);
			num2 += 2;
		}
		if (selectedTab == STR_VILLAGES && selectedCategory != null)
		{
			displayLines.Add("[History of " + HistoryAPI.GetEntityName(selectedCategory) + "]");
			displayLines.Add("");
			entryForDisplayLine.Add(0);
			entryForDisplayLine.Add(0);
			num2 += 2;
		}
		int maxWidth = 75;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		for (int i = 0; i < entries.Count; i++)
		{
			int MaxClippedWidth = 0;
			List<string> list;
			if (selectedTab == STR_CHRONOLOGY)
			{
				JournalAccomplishment journalAccomplishment = (JournalAccomplishment)entries[i].baseEntry;
				stringBuilder.Clear().Append(entries[i].entry);
				if (journalAccomplishment.category == "player")
				{
					stringBuilder.Insert(0, "@ ");
				}
				else
				{
					stringBuilder.Insert(0, journalAccomplishment.secretSold ? "{{K|$}} " : "{{G|$}} ");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			else if (selectedTab == STR_LOCATIONS)
			{
				JournalMapNote journalMapNote = (JournalMapNote)entries[i].baseEntry;
				stringBuilder.Clear().Append(journalMapNote.tracked ? "[{{G|X}}] " : "[ ] ").Append(Grammar.InitCapWithFormatting(entries[i].entry));
				if (journalMapNote.category == "player")
				{
					stringBuilder.Insert(0, "@ ");
				}
				else
				{
					stringBuilder.Insert(0, journalMapNote.Visited ? "{{K|?}} " : "{{G|?}} ");
					stringBuilder.Insert(0, journalMapNote.secretSold ? "{{K|$}} " : "{{G|$}} ");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			else
			{
				stringBuilder.Clear().Append("{{").Append(entries[i].baseEntry.secretSold ? 'K' : 'G')
					.Append("|$}} ");
				bool num3 = entries[i].baseEntry.Has("sultanTombPropaganda");
				if (num3)
				{
					stringBuilder.Append("{{w|[tomb engraving] ");
				}
				stringBuilder.Append(entries[i].entry);
				if (num3)
				{
					stringBuilder.Append("}}");
				}
				list = StringFormat.ClipTextToArray(stringBuilder.ToString(), maxWidth, out MaxClippedWidth, KeepNewlines: true);
			}
			entries[i].topLine = num2;
			entries[i].lines = list.Count;
			displayLines.AddRange(list);
			if (i < entries.Count - 1)
			{
				entries[i].lines++;
				displayLines.Add("");
			}
			for (int j = 0; j < list.Count + 1; j++)
			{
				entryForDisplayLine.Add(i);
			}
			num2 += entries[i].lines;
		}
		if (displayLines.Count == 0)
		{
			if (selectedTab == STR_CHRONOLOGY)
			{
				displayLines.Add("{{K|You have no history. That's pretty weird to be honest.}}");
			}
			if (selectedTab == STR_OBSERVATIONS)
			{
				displayLines.Add("{{K|You have made no observations.}}");
			}
			if (selectedTab == STR_LOCATIONS)
			{
				displayLines.Add("{{K|You have made no map notes. Hit + to add a new one.}}");
			}
			if (selectedTab == STR_GENERAL)
			{
				displayLines.Add("{{K|You have made no general notes. Hit + to add a new one.}}");
			}
			if (selectedTab == STR_RECIPES)
			{
				displayLines.Add("{{K|You have learned no recipes.}}");
			}
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Journal");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		Keys keys = Keys.None;
		bool flag = false;
		string[] array = new string[7] { STR_LOCATIONS, STR_OBSERVATIONS, STR_SULTANS, STR_VILLAGES, STR_CHRONOLOGY, STR_GENERAL, STR_RECIPES };
		TabMemory.Stock = new TabMemory[array.Length];
		int num = 0;
		cursorPosition = 0;
		currentTopLine = 0;
		selectedCategory = null;
		UpdateEntries(array[0]);
		string s = "< {{W|7}} Quests | Tinkering {{W|9}} >";
		if (ControlManager.activeControllerType == ControllerType.Joystick)
		{
			s = "< {{W|" + ControlManager.getCommandInputDescription("Previous Page", mapGlyphs: false) + "}} Quests | Tinkering {{W|" + ControlManager.getCommandInputDescription("Next Page", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool(resetMinEventPools: false);
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(35, 0);
			scrapBuffer.Write("[ {{W|Journal}} ]");
			scrapBuffer.Goto(1, 2);
			for (int i = Math.Max(num - 4, 0); i < array.Length && i < Math.Max(num - 4, 0) + 5; i++)
			{
				if (num == i)
				{
					scrapBuffer.Write("  ");
					scrapBuffer.Write("{{W|" + array[i] + "}}");
				}
				else
				{
					scrapBuffer.Write("  ");
					scrapBuffer.Write("{{K|" + array[i] + "}}");
				}
			}
			if (num > 4)
			{
				scrapBuffer.Goto(1, 2);
				scrapBuffer.Write("{{G|<<}}");
			}
			if (num <= 4)
			{
				scrapBuffer.Goto(76, 2);
				scrapBuffer.Write("{{G|>>}}");
			}
			if (ControlManager.activeControllerType == ControllerType.Joystick)
			{
				scrapBuffer.Goto(60, 0);
				scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Back", mapGlyphs: false) + "}} to exit ");
			}
			else
			{
				scrapBuffer.Goto(60, 0);
				scrapBuffer.Write(" {{W|ESC}} or {{W|5}} to exit ");
			}
			scrapBuffer.Goto(79 - ColorUtility.StripFormatting(s).Length, 24);
			scrapBuffer.Write(s);
			if (array[num] == STR_RECIPES)
			{
				scrapBuffer.Goto(2, 24);
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Delete Journal Entry", mapGlyphs: false) + "}} - Delete ");
				}
				else
				{
					scrapBuffer.Write(" {{W|del}} - Delete ");
				}
			}
			if (array[num] == STR_CHRONOLOGY || array[num] == STR_GENERAL || array[num] == STR_LOCATIONS)
			{
				scrapBuffer.Goto(2, 24);
				if (ControlManager.activeControllerType == ControllerType.Joystick)
				{
					scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Add Journal Entry", mapGlyphs: false) + "}} Add {{W|" + ControlManager.getCommandInputDescription("Delete Journal Entry", mapGlyphs: false) + "}} - Delete ");
				}
				else
				{
					scrapBuffer.Write(" {{W|+}} Add {{W|del}} - Delete ");
				}
				if (array[num] == STR_LOCATIONS)
				{
					if (ControlManager.activeControllerType == ControllerType.Joystick)
					{
						scrapBuffer.Write("{{W|" + ControlManager.getCommandInputDescription("New Map Pin", mapGlyphs: false) + "}} - Map Pin");
					}
					else if (ControlManager.activeControllerType == ControllerType.Keyboard)
					{
						scrapBuffer.Write("{{W|Tab}} - Map Pin {{W|N}} - Name Here ");
					}
				}
			}
			int num2 = 4;
			int num3 = 23;
			int num4 = num3 - num2 + 1;
			int num5 = currentTopLine;
			int num6 = 0;
			for (int j = num2; j <= num3; j++)
			{
				if (num5 >= displayLines.Count)
				{
					break;
				}
				if (j - num2 == cursorPosition)
				{
					scrapBuffer.Goto(2, j);
					scrapBuffer.Write("{{Y|>}}");
				}
				scrapBuffer.Goto(3, j);
				scrapBuffer.Write(displayLines[num5]);
				num5++;
				num6++;
			}
			if (displayLines.Count > num4)
			{
				int num7 = (int)((float)num4 / (float)displayLines.Count * 23f);
				int num8 = (int)((float)currentTopLine / (float)displayLines.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num8, 79, 1 + num8 + num7, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = ConsoleLib.Console.Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && ConsoleLib.Console.Keyboard.RawCode != Keys.Prior && ConsoleLib.Console.Keyboard.RawCode != Keys.Next))
			{
				flag = true;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				if ((array[num] == STR_SULTANS || array[num] == STR_VILLAGES || array[num] == STR_LOCATIONS) && selectedCategory != null)
				{
					Memorize(num);
					selectedCategory = null;
					Recall(num);
					UpdateEntries(array[num]);
				}
				else
				{
					flag = true;
				}
			}
			if ((keys == Keys.N || (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "Passthrough:New Map Pin")) && array[num] == STR_LOCATIONS)
			{
				Zone parentZone = The.Player.CurrentCell.ParentZone;
				if (parentZone.IsWorldMap())
				{
					Popup.Show("You cannot do that on the world map.");
				}
				else if (parentZone.HasProperName && !parentZone.NamedByPlayer)
				{
					Popup.Show("This place already has a name.");
				}
				else
				{
					_ = parentZone.DisplayName;
					string baseDisplayName = parentZone.BaseDisplayName;
					bool namedByPlayer = parentZone.NamedByPlayer;
					string text = Popup.AskString(namedByPlayer ? ("Enter a new name for " + baseDisplayName + ".") : "Enter a name for this location.", "", 40);
					if (!string.IsNullOrEmpty(text))
					{
						if (namedByPlayer)
						{
							Popup.Show("You stop calling this location '" + baseDisplayName + "' and start calling it '" + text + "'.");
							JournalAPI.AddAccomplishment("You stopped calling a location '" + baseDisplayName + "' and start calling it '" + text + "'.", "In " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= commanded " + The.Player.GetPronounProvider().PossessiveAdjective + " cartographers to change the name of " + Grammar.GetProsaicZoneName(parentZone) + " in the world atlas to " + text + ".", "general", JournalAccomplishment.MuralCategory.DoesBureaucracy, JournalAccomplishment.MuralWeight.Low, null, -1L);
						}
						else
						{
							Popup.Show("You start calling this location '" + text + "'.");
							JournalAPI.AddAccomplishment("You started calling a location '" + text + "'.", "In " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= commanded " + The.Player.GetPronounProvider().PossessiveAdjective + " cartographers to change the name of " + Grammar.GetProsaicZoneName(parentZone) + " in the world atlas to " + text + ".", "general", JournalAccomplishment.MuralCategory.DoesBureaucracy, JournalAccomplishment.MuralWeight.Low, null, -1L);
						}
						parentZone.IncludeContextInZoneDisplay = true;
						parentZone.IncludeStratumInZoneDisplay = false;
						parentZone.NamedByPlayer = true;
						parentZone.HasProperName = true;
						parentZone.BaseDisplayName = text;
						JournalAPI.AddMapNote(parentZone.ZoneID, text, "Named Locations", null, null, revealed: true, sold: true, -1L);
						UpdateEntries(array[num]);
						int num9 = Math.Max(0, displayLines.Count - num4);
						currentTopLine = num9 + 100;
					}
				}
			}
			int num10 = currentTopLine + cursorPosition;
			if (keys == Keys.Tab)
			{
				if (array[num] == STR_LOCATIONS && selectedCategory == null && num10 < categories.Count)
				{
					JournalAPI.SetCategoryMapNoteToggle(categories[num10], !JournalAPI.GetCategoryMapNoteToggle(categories[num10]));
					UpdateEntries(array[num]);
				}
				else if (array[num] == STR_LOCATIONS && selectedCategory != null && num10 < entryForDisplayLine.Count)
				{
					(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).tracked = !(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).tracked;
					UpdateEntries(array[num]);
				}
			}
			if ((keys == Keys.Delete || (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "Passthrough:Delete Journal Entry")) && num10 < entryForDisplayLine.Count)
			{
				if (array[num] == STR_CHRONOLOGY)
				{
					if ((entries[entryForDisplayLine[num10]].baseEntry as JournalAccomplishment).category == "player")
					{
						if (Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
						{
							JournalAPI.DeleteAccomplishment(entries[entryForDisplayLine[num10]].baseEntry as JournalAccomplishment);
							UpdateEntries(array[num]);
						}
					}
					else
					{
						Popup.Show("You can't delete automatically recorded chronology entries.");
					}
				}
				if (array[num] == STR_GENERAL && Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
				{
					JournalAPI.DeleteGeneralNote(entries[entryForDisplayLine[num10]].baseEntry as JournalGeneralNote);
					UpdateEntries(array[num]);
				}
				if (array[num] == STR_LOCATIONS && Popup.ShowYesNo("Are you sure you want to delete this entry?") == DialogResult.Yes)
				{
					JournalAPI.DeleteMapNote(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote);
					UpdateEntries(array[num]);
				}
				if (array[num] == STR_RECIPES)
				{
					JournalRecipeNote journalRecipeNote = entries[entryForDisplayLine[num10]].baseEntry as JournalRecipeNote;
					if (Popup.ShowYesNo("Are you sure you want to delete {{y|" + journalRecipeNote.recipe.DisplayName + "}}?") == DialogResult.Yes)
					{
						JournalAPI.DeleteRecipeNote(entries[entryForDisplayLine[num10]].baseEntry as JournalRecipeNote);
						UpdateEntries(array[num]);
					}
				}
			}
			if ((keys == Keys.Oemplus || keys == Keys.Add || (keys == Keys.MouseEvent && ConsoleLib.Console.Keyboard.CurrentMouseEvent.Event == "Passthrough:Add Journal Entry")) && (array[num] == STR_CHRONOLOGY || array[num] == STR_GENERAL || array[num] == STR_LOCATIONS))
			{
				string text2 = Popup.AskString("Entry text", "", 2147483646);
				if (!string.IsNullOrEmpty(text2))
				{
					if (array[num] == STR_CHRONOLOGY)
					{
						JournalAPI.AddAccomplishment(text2, null, "player", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Nil, null, -1L);
						UpdateEntries(array[num]);
						int num11 = Math.Max(0, displayLines.Count - num4);
						currentTopLine = num11 + 100;
					}
					if (array[num] == STR_GENERAL)
					{
						JournalAPI.AddGeneralNote(text2, null, -1L);
						UpdateEntries(array[num]);
						int num12 = Math.Max(0, displayLines.Count - num4);
						currentTopLine = num12 + 100;
					}
					if (array[num] == STR_LOCATIONS)
					{
						JournalAPI.AddMapNote(The.Player.CurrentZone.ZoneID, text2, "Miscellaneous", null, null, revealed: true, sold: true, -1L);
						UpdateEntries(array[num]);
						int num13 = Math.Max(0, displayLines.Count - num4);
						currentTopLine = num13 + 100;
					}
				}
			}
			if (keys == Keys.Space || keys == Keys.Enter)
			{
				if ((array[num] == STR_SULTANS || array[num] == STR_VILLAGES || array[num] == STR_LOCATIONS) && selectedCategory == null)
				{
					if (categories.Count > num10)
					{
						Memorize(num);
						selectedCategory = categories[num10];
						Recall(num);
						UpdateEntries(array[num]);
					}
				}
				else if (selectedCategory != null && array[num] == STR_LOCATIONS)
				{
					(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).tracked = !(entries[entryForDisplayLine[num10]].baseEntry as JournalMapNote).tracked;
					UpdateEntries(array[num]);
				}
			}
			if (keys == Keys.NumPad8)
			{
				if (cursorPosition == 0)
				{
					if (currentTopLine > 0)
					{
						currentTopLine--;
					}
				}
				else
				{
					cursorPosition--;
				}
			}
			if (keys == Keys.NumPad2)
			{
				if (cursorPosition >= num3 - num2)
				{
					currentTopLine++;
				}
				else
				{
					cursorPosition++;
				}
			}
			switch (keys)
			{
			case Keys.Next:
				currentTopLine += num4;
				cursorPosition += num4;
				break;
			case Keys.Prior:
				currentTopLine -= num4;
				cursorPosition -= num4;
				break;
			case Keys.Home:
				currentTopLine = (cursorPosition = 0);
				break;
			case Keys.End:
				currentTopLine = (cursorPosition = int.MaxValue);
				break;
			}
			int num14 = Math.Max(0, displayLines.Count - num4);
			if (currentTopLine < 0)
			{
				currentTopLine = 0;
			}
			if (currentTopLine > num14)
			{
				currentTopLine = num14;
			}
			if (cursorPosition >= num6)
			{
				cursorPosition = num6 - 1;
			}
			if (cursorPosition < 0)
			{
				cursorPosition = 0;
			}
			if (cursorPosition + currentTopLine > displayLines.Count)
			{
				cursorPosition = displayLines.Count - currentTopLine;
			}
			switch (keys)
			{
			case Keys.NumPad4:
				Memorize(num, Tabinate: true);
				num--;
				if (num < 0)
				{
					num = array.Length - 1;
				}
				selectedCategory = null;
				Recall(num, Tabinate: true);
				UpdateEntries(array[num]);
				break;
			case Keys.NumPad6:
				Memorize(num, Tabinate: true);
				num++;
				if (num >= array.Length)
				{
					num = 0;
				}
				selectedCategory = null;
				Recall(num, Tabinate: true);
				UpdateEntries(array[num]);
				break;
			}
		}
		TabMemory.Stock = null;
		GameManager.Instance.PopGameView();
		switch (keys)
		{
		case Keys.NumPad7:
			return ScreenReturn.Previous;
		case Keys.NumPad9:
			return ScreenReturn.Next;
		default:
			if (The.Player.OnWorldMap())
			{
				The.Player.CurrentZone.Activated();
			}
			return ScreenReturn.Exit;
		}
	}
}
