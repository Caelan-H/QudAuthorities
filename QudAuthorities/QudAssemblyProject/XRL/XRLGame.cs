using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Newtonsoft.Json;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL;

[Serializable]
public class XRLGame
{
	public static TextConsole Console;

	public static ScreenBuffer Buffer;

	public string GameID;

	public string PlayerName = "Player";

	public bool Running;

	public bool bZoned = true;

	public string DeathReason = "You quit.";

	public long Turns;

	public long TimeTicks;

	public int TimeOffset;

	public long Segments;

	public long ActionTicks;

	public long _walltime;

	[NonSerialized]
	public bool forceNoDeath;

	[NonSerialized]
	public string lastFindId;

	[NonSerialized]
	public XRL.World.GameObject lastFind;

	[NonSerialized]
	public const int SaveVersion = 264;

	[NonSerialized]
	public const int MinSaveVersion = 223;

	[NonSerialized]
	public const int MaxSaveVersion = 264;

	[NonSerialized]
	public History sultanHistory;

	public List<string> Accomplishments = new List<string>();

	[NonSerialized]
	public Stopwatch WallTime;

	public Dictionary<string, Maze> WorldMazes = new Dictionary<string, Maze>();

	[NonSerialized]
	public GamePlayer _Player;

	[NonSerialized]
	public Reputation PlayerReputation;

	[NonSerialized]
	public ActionManager ActionManager;

	[NonSerialized]
	public ZoneManager ZoneManager;

	[NonSerialized]
	public Zone GraveyardZone;

	[NonSerialized]
	public GraveyardCell Graveyard;

	[NonSerialized]
	public List<XRL.World.GameObject> Objects = new List<XRL.World.GameObject>();

	[NonSerialized]
	public Dictionary<string, Quest> Quests = new Dictionary<string, Quest>();

	[NonSerialized]
	public Dictionary<string, Quest> FinishedQuests = new Dictionary<string, Quest>();

	[NonSerialized]
	private bool? _AlternateStart;

	public List<IGameSystem> Systems = new List<IGameSystem>();

	public Dictionary<string, Gender> Genders = new Dictionary<string, Gender>(32);

	public Dictionary<string, PronounSet> PronounSets = new Dictionary<string, PronounSet>(64);

	public Dictionary<string, string> StringGameState = new Dictionary<string, string>();

	public Dictionary<string, int> IntGameState = new Dictionary<string, int>();

	public Dictionary<string, long> Int64GameState = new Dictionary<string, long>();

	[NonSerialized]
	public Dictionary<string, object> ObjectGameState = new Dictionary<string, object>();

	public Dictionary<string, bool> BooleanGameState = new Dictionary<string, bool>();

	[NonSerialized]
	public HashSet<string> BlueprintsSeen = new HashSet<string>();

	private static char[] spaceSplitter = new char[1] { ' ' };

	[NonSerialized]
	public string _CacheDirectory;

	public bool AlternateStart
	{
		get
		{
			bool valueOrDefault = _AlternateStart.GetValueOrDefault();
			if (!_AlternateStart.HasValue)
			{
				valueOrDefault = !GetStringGameState("embark", "Joppa").EqualsNoCase("Joppa");
				_AlternateStart = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public string gameMode
	{
		get
		{
			return GetStringGameState("GameMode");
		}
		set
		{
			SetStringGameState("GameMode", value);
		}
	}

	public GamePlayer Player
	{
		get
		{
			return _Player;
		}
		set
		{
			_Player = value;
		}
	}

	public XRLGame(TextConsole _Console, ScreenBuffer _Buffer)
	{
		Console = _Console;
		Buffer = _Buffer;
		GraveyardZone = new Zone();
		Graveyard = GraveyardZone.GetCell(0, 0) as GraveyardCell;
		ActionManager = new ActionManager();
		ZoneManager = new ZoneManager();
		PlayerReputation = new Reputation();
		WallTime = new Stopwatch();
	}

	public T GetSystem<T>() where T : IGameSystem
	{
		for (int i = 0; i < Systems.Count; i++)
		{
			if (Systems[i].GetType() == typeof(T))
			{
				return Systems[i] as T;
			}
		}
		return null;
	}

	public T RequireSystem<T>(Func<T> generator) where T : IGameSystem
	{
		for (int i = 0; i < Systems.Count; i++)
		{
			if (Systems[i].GetType() == typeof(T))
			{
				return Systems[i] as T;
			}
		}
		return AddSystem(generator()) as T;
	}

	public void FireSystemsEvent(XRL.World.Event e)
	{
		for (int i = 0; i < Systems.Count; i++)
		{
			Systems[i].FireEvent(e);
		}
	}

	public IGameSystem AddSystem(string system)
	{
		IGameSystem gameSystem = ModManager.CreateInstance<IGameSystem>(system);
		if (gameSystem == null)
		{
			MetricsManager.LogError("Unknown system class: " + system);
			return null;
		}
		return AddSystem(gameSystem);
	}

	public IGameSystem AddSystem(IGameSystem system)
	{
		Systems.Add(system);
		Systems.Sort((IGameSystem a, IGameSystem b) => a.GetPriority().CompareTo(b.GetPriority()));
		system.OnAdded();
		return system;
	}

	public void RemoveSystem(IGameSystem system)
	{
		system.OnRemoved();
		Systems.Remove(system);
	}

	public void Release()
	{
		if (ZoneManager != null)
		{
			ZoneManager.Release();
		}
	}

	public bool HasObjectGameState(string Value)
	{
		return ObjectGameState.ContainsKey(Value);
	}

	public bool HasIntGameState(string Value)
	{
		return IntGameState.ContainsKey(Value);
	}

	public bool HasInt64GameState(string Value)
	{
		return IntGameState.ContainsKey(Value);
	}

	public bool HasStringGameState(string Value)
	{
		return StringGameState.ContainsKey(Value);
	}

	public bool HasBooleanGameState(string Value)
	{
		return BooleanGameState.ContainsKey(Value);
	}

	public bool HasBlueprintBeenSeen(string Name)
	{
		return BlueprintsSeen.Contains(Name);
	}

	public void BlueprintSeen(string Name)
	{
		if (!BlueprintsSeen.Contains(Name))
		{
			BlueprintsSeen.Add(Name);
		}
	}

	public void IncrementIntGameState(string Value, int Amount)
	{
		if (!IntGameState.ContainsKey(Value))
		{
			IntGameState.Add(Value, 0);
		}
		IntGameState[Value] += Amount;
	}

	public void IncrementInt64GameState(string Value, long Amount)
	{
		if (!Int64GameState.ContainsKey(Value))
		{
			Int64GameState.Add(Value, 0L);
		}
		Int64GameState[Value] += Amount;
	}

	public bool ToggleBooleanGameState(string Value)
	{
		if (!BooleanGameState.ContainsKey(Value))
		{
			BooleanGameState.Add(Value, value: false);
		}
		return BooleanGameState[Value] = !BooleanGameState[Value];
	}

	public void StartQuest(Quest newQuest)
	{
		if (!HasQuest(newQuest.ID))
		{
			Quests.Add(newQuest.ID, newQuest);
			if (_Player != null && _Player.Body != null)
			{
				_Player.Body.FireEvent(XRL.World.Event.New("GotNewQuest", "Quest", newQuest));
			}
			Popup.ShowBlock("You have received a new quest, " + newQuest.DisplayName + "!");
			if (newQuest.Manager != null)
			{
				newQuest.Manager.OnQuestAdded();
			}
			if (newQuest.System != null)
			{
				The.Game.AddSystem(newQuest.System);
			}
			OnQuestAddedEvent.Send(Player.Body, newQuest);
			MetricsManager.LogEvent("Quest:Start:" + newQuest.ID);
		}
	}

	public void StartQuest(string questID)
	{
		if (HasQuest(questID))
		{
			return;
		}
		if (QuestLoader.Loader.QuestsByID.ContainsKey(questID))
		{
			Quest quest = QuestLoader.Loader.QuestsByID[questID].Copy();
			Quests.Add(quest.ID, quest);
			if (_Player != null && _Player.Body != null)
			{
				_Player.Body.FireEvent(XRL.World.Event.New("GotNewQuest", "Quest", quest, "QuestName", quest.Name));
			}
			Popup.ShowBlock("You have received a new quest, " + quest.DisplayName + "!");
			if (quest.Manager != null)
			{
				quest.Manager.OnQuestAdded();
			}
			if (quest.System != null)
			{
				The.Game.AddSystem(quest.System);
			}
			OnQuestAddedEvent.Send(Player.Body, quest);
		}
		else if (DynamicQuestsGamestate.instance.quests.ContainsKey(questID))
		{
			Quest quest2 = DynamicQuestsGamestate.instance.quests[questID].Copy();
			Quests.Add(quest2.ID, quest2);
			if (_Player != null && _Player.Body != null)
			{
				_Player.Body.FireEvent(XRL.World.Event.New("GotNewQuest", "Quest", quest2));
			}
			Popup.ShowBlock("You have received a new quest, " + quest2.DisplayName + "!");
			if (quest2.Manager != null)
			{
				quest2.Manager.OnQuestAdded();
			}
			if (quest2.System != null)
			{
				The.Game.AddSystem(quest2.System);
			}
			OnQuestAddedEvent.Send(Player.Body, quest2);
		}
		MetricsManager.LogEvent("Quest:Start:" + questID);
	}

	public void CompleteQuest(string questID)
	{
		if (FinishedQuests.ContainsKey(questID))
		{
			return;
		}
		if (!HasQuest(questID))
		{
			if (!QuestLoader.Loader.QuestsByID.ContainsKey(questID))
			{
				return;
			}
			StartQuest(questID);
		}
		foreach (string key in Quests[questID].StepsByID.Keys)
		{
			FinishQuestStep(questID, key);
		}
	}

	public void FinishQuestStepXP(string questID, string questStepList, int XP)
	{
		string[] array = questStepList.Split('~');
		foreach (string text in array)
		{
			if (!HasQuest(questID))
			{
				continue;
			}
			QuestStep questStep = Quests[questID].StepsByID[text];
			if (questStep == null)
			{
				continue;
			}
			if (!questStep.Finished)
			{
				questStep.Finished = true;
				string text2 = "You have finished the step, {{G|" + questStep.Name + "}},\nof the quest " + Quests[questID].DisplayName + "!";
				if (XP > 0)
				{
					Popup.Show(text2 + "\nYou gain {{C|" + XP + "}} XP!", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
					MessageQueue.AddPlayerMessage(text2);
					XRL.World.GameObject influencedBy = Quests[questID].Manager?.GetQuestInfluencer();
					Player.Body.AwardXP(XP, -1, 0, int.MaxValue, null, influencedBy);
				}
				else
				{
					Popup.Show(text2);
				}
				if (Quests[questID].Manager != null)
				{
					Quests[questID].Manager.OnStepComplete(text);
				}
			}
			bool flag = true;
			foreach (QuestStep value in Quests[questID].StepsByID.Values)
			{
				if (!value.Name.Contains("Optional:") && !value.Finished)
				{
					flag = false;
					break;
				}
			}
			if (flag && !FinishedQuests.ContainsKey(questID))
			{
				FinishQuest(questID);
			}
		}
	}

	public long GetQuestFinishTime(string questID)
	{
		return GetInt64GameState("QuestFinishedTime_" + questID, -1L);
	}

	public void FinishQuest(string questID)
	{
		if (FinishedQuests.ContainsKey(questID) || !Quests.TryGetValue(questID, out var value))
		{
			return;
		}
		try
		{
			value.Finish();
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Quest::FinishQuest@" + questID, x);
		}
		SetInt64GameState("QuestFinishedTime_" + questID, XRL.World.Calendar.TotalTimeTicks);
		Popup.ShowBlock("You have completed the quest " + value.DisplayName + "!");
		FinishedQuests.Add(questID, value);
		foreach (IGameSystem system in The.Game.Systems)
		{
			system.QuestCompleted(value);
		}
		QuestManager manager = value.Manager;
		manager?.OnQuestComplete();
		if (_Player != null)
		{
			XRL.World.GameObject influencedBy = manager?.GetQuestInfluencer();
			ItemNaming.Opportunity(_Player.Body, null, influencedBy, "Quest", 6, 0, 0, 1);
		}
		try
		{
			value.FinishPost();
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("Quest::FinishQuest@" + questID, x2);
		}
		if (!string.IsNullOrEmpty(value.Accomplishment))
		{
			string text = ((!string.IsNullOrEmpty(value.Hagiograph)) ? value.Hagiograph : value.Accomplishment);
			string value2 = ((!string.IsNullOrEmpty(value.HagiographCategory)) ? value.HagiographCategory : "DoesSomethingRad");
			JournalAccomplishment.MuralWeight muralWeight = JournalAccomplishment.MuralWeight.High;
			if (string.IsNullOrEmpty(value.Hagiograph))
			{
				muralWeight = JournalAccomplishment.MuralWeight.Nil;
			}
			if (questID.Equals("O Glorious Shekhinah!"))
			{
				if (XRLCore.Core.Game.GetIntGameState("VisitedSixDayStilt") != 1)
				{
					JournalAPI.AddAccomplishment(value.Accomplishment, text.Replace("=month=", XRL.World.Calendar.getMonth()).Replace("=year=", XRL.World.Calendar.getYear() + " AR").Replace("=them=", The.Player.GetPronounProvider().Objective)
						.Replace("=their=", The.Player.GetPronounProvider().PossessiveAdjective), "general", MuralCategoryHelpers.parseCategory(value2), muralWeight, null, -1L);
					XRLCore.Core.Game.SetIntGameState("VisitedSixDayStilt", 1);
					AchievementManager.SetAchievement("ACH_SIX_DAY_STILT");
				}
			}
			else
			{
				JournalAPI.AddAccomplishment(value.Accomplishment, text.Replace("=month=", XRL.World.Calendar.getMonth()).Replace("=year=", XRL.World.Calendar.getYear() + " AR").Replace("=them=", The.Player.GetPronounProvider().Objective), "general", MuralCategoryHelpers.parseCategory(value2), muralWeight, null, -1L);
			}
		}
		if (!string.IsNullOrEmpty(value.Factions) && !string.IsNullOrEmpty(value.Reputation))
		{
			int delta = int.Parse(value.Reputation);
			string[] array = value.Factions.Split(',');
			foreach (string faction in array)
			{
				XRLCore.Core.Game.PlayerReputation.modify(faction, delta);
			}
		}
		if (!value.Achievement.IsNullOrEmpty())
		{
			AchievementManager.SetAchievement(value.Achievement);
		}
		MetricsManager.LogEvent("Quest:Complete:" + questID);
		XRL.World.GameObject body = XRLCore.Core.Game.Player.Body;
		if (body != null)
		{
			if (body.HasStat("Level"))
			{
				MetricsManager.LogEvent("Quest:Complete:Level:" + questID + ":" + body.Stat("Level"));
			}
			if (body.HasStat("HP"))
			{
				MetricsManager.LogEvent("Quest:Complete:HP:" + questID + ":" + body.Stat("Level"));
			}
			MetricsManager.LogEvent("Quest:Complete:Turns:" + questID + ":" + XRLCore.Core.Game.Turns);
			MetricsManager.LogEvent("Quest:Complete:Walltime:" + questID + ":" + XRLCore.Core.Game._walltime);
		}
	}

	public void FinishQuestStep(string questID, string questStepList)
	{
		try
		{
			string[] array = questStepList.Split('~');
			foreach (string text in array)
			{
				if (!HasQuest(questID))
				{
					continue;
				}
				if (!Quests.ContainsKey(questID) || !Quests[questID].StepsByID.ContainsKey(text))
				{
					MetricsManager.LogError("quest step finisher", "Invalid quest step finish: " + questID + "@" + text);
					continue;
				}
				QuestStep questStep = Quests[questID].StepsByID[text];
				if (questStep == null)
				{
					continue;
				}
				if (!questStep.Finished)
				{
					questStep.Finished = true;
					QuestManager manager = Quests[questID].Manager;
					string text2 = "You have finished the step, {{G|" + questStep.Name + "}},\nof the quest " + Quests[questID].DisplayName + "!";
					if (questStep.XP > 0)
					{
						Popup.ShowBlock(text2 + "\nYou gain {{C|" + questStep.XP + "}} XP!", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
						MessageQueue.AddPlayerMessage(text2);
						XRL.World.GameObject influencedBy = manager?.GetQuestInfluencer();
						Player.Body.AwardXP(questStep.XP, -1, 0, int.MaxValue, null, influencedBy);
					}
					else
					{
						Popup.ShowBlock(text2);
					}
					manager?.OnStepComplete(text);
				}
				bool flag = true;
				foreach (QuestStep value in Quests[questID].StepsByID.Values)
				{
					if (!value.Name.Contains("Optional:") && !value.Finished)
					{
						flag = false;
						break;
					}
				}
				if (flag && !FinishedQuests.ContainsKey(questID))
				{
					FinishQuest(questID);
				}
			}
		}
		catch (Exception ex)
		{
			XRLCore.LogError("FinishQuestStep", ex);
			MessageQueue.AddPlayerMessage("Error finishing quest step " + questID + " @ " + questStepList + " : " + ex.ToString(), 'R');
		}
	}

	public bool FinishedQuest(string questID)
	{
		return FinishedQuests.ContainsKey(questID);
	}

	public bool FinishedQuestStep(string questID)
	{
		string[] array = questID.Split('~');
		if (!Quests.ContainsKey(array[0]))
		{
			return false;
		}
		if (!Quests[array[0]].StepsByID.ContainsKey(array[1]))
		{
			return false;
		}
		return Quests[array[0]].StepsByID[array[1]].Finished;
	}

	public bool HasGameState(string Name)
	{
		if (!HasStringGameState(Name) && !HasIntGameState(Name) && !HasObjectGameState(Name) && !HasInt64GameState(Name))
		{
			return HasBooleanGameState(Name);
		}
		return true;
	}

	public bool TestGameState(string Spec)
	{
		string[] array = Spec.Split(spaceSplitter, 3);
		string name = array[0];
		string text = ((array.Length >= 2) ? array[1] : null);
		string val = ((array.Length >= 3) ? array[2] : null);
		if (text != null && text[0] == '!')
		{
			return !TestGameStateInternal(name, text.Substring(1), val);
		}
		return TestGameStateInternal(name, text, val);
	}

	private bool TestGameStateInternal(string name, string op, string val)
	{
		if (op != null && val != null && StringGameState.TryGetValue(name, out var value))
		{
			switch (op)
			{
			case "=":
				if (value == val)
				{
					return true;
				}
				break;
			case "~":
				if (value.EqualsNoCase(val))
				{
					return true;
				}
				break;
			case "contains":
				if (value.Contains(val))
				{
					return true;
				}
				break;
			case "~contains":
				if (value.Contains(val, CompareOptions.IgnoreCase))
				{
					return true;
				}
				break;
			case "isin":
				if (val.Contains(value))
				{
					return true;
				}
				break;
			case "~isin":
				if (val.Contains(value, CompareOptions.IgnoreCase))
				{
					return true;
				}
				break;
			}
		}
		if (op != null && val != null && IntGameState.TryGetValue(name, out var value2) && int.TryParse(val, out var result))
		{
			switch (op)
			{
			case "=":
				break;
			case ">":
				goto IL_01d4;
			case ">=":
				goto IL_01da;
			case "<":
				goto IL_01e0;
			case "<=":
				goto IL_01e6;
			case "%":
				goto IL_01ec;
			case "&":
				goto IL_01f3;
			default:
				goto IL_0201;
			case null:
				goto IL_033f;
			}
			if (value2 == result)
			{
				return true;
			}
		}
		goto IL_01fb;
		IL_0201:
		if (val != null && Int64GameState.TryGetValue(name, out var value3) && long.TryParse(val, out var result2))
		{
			switch (op)
			{
			case "=":
				if (value3 == result2)
				{
					return true;
				}
				break;
			case ">":
				if (value3 > result2)
				{
					return true;
				}
				break;
			case ">=":
				if (value3 >= result2)
				{
					return true;
				}
				break;
			case "<":
				if (value3 < result2)
				{
					return true;
				}
				break;
			case "<=":
				if (value3 <= result2)
				{
					return true;
				}
				break;
			case "%":
				if (value3 % result2 == 0L)
				{
					return true;
				}
				break;
			case "&":
				if ((value3 & result2) == result2)
				{
					return true;
				}
				break;
			}
		}
		goto IL_033f;
		IL_01ec:
		if (value2 % result == 0)
		{
			return true;
		}
		goto IL_01fb;
		IL_01e0:
		if (value2 < result)
		{
			return true;
		}
		goto IL_01fb;
		IL_033f:
		if (BooleanGameState.TryGetValue(name, out var value4))
		{
			bool result3;
			if (op == null && val == null)
			{
				if (value4)
				{
					return true;
				}
			}
			else if (op != null && val != null && bool.TryParse(val, out result3) && op != null && op == "=" && value4 == result3)
			{
				return true;
			}
		}
		return false;
		IL_01f3:
		if ((value2 & result) == result)
		{
			return true;
		}
		goto IL_01fb;
		IL_01e6:
		if (value2 <= result)
		{
			return true;
		}
		goto IL_01fb;
		IL_01da:
		if (value2 >= result)
		{
			return true;
		}
		goto IL_01fb;
		IL_01fb:
		if (op != null)
		{
			goto IL_0201;
		}
		goto IL_033f;
		IL_01d4:
		if (value2 > result)
		{
			return true;
		}
		goto IL_01fb;
	}

	public bool FailQuest(string questID)
	{
		if (Quests.ContainsKey(questID))
		{
			Popup.Show("You have failed the quest: " + Quests[questID].Name);
			Quests[questID].Fail();
			FinishedQuests.Add(questID, Quests[questID]);
			Quests.Remove(questID);
			return true;
		}
		return false;
	}

	public bool HasQuest(string questID)
	{
		if (Quests.ContainsKey(questID))
		{
			return true;
		}
		if (FinishedQuests.ContainsKey(questID))
		{
			return true;
		}
		return false;
	}

	public bool HasFinishedQuest(string questID)
	{
		return FinishedQuests.ContainsKey(questID);
	}

	public bool HasFinishedQuestStep(string questID, string questStepID)
	{
		if (FinishedQuests.ContainsKey(questID))
		{
			return true;
		}
		if (HasQuest(questID) && Quests[questID].StepsByID.ContainsKey(questStepID) && Quests[questID].StepsByID[questStepID].Finished)
		{
			return true;
		}
		return false;
	}

	public bool HasUnfinishedQuest(string questID)
	{
		if (HasQuest(questID))
		{
			return !HasFinishedQuest(questID);
		}
		return false;
	}

	public string GetStringGameState(string State, string Default = "")
	{
		if (StringGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void AppendStringGameState(string State, string Value)
	{
		if (!StringGameState.ContainsKey(State))
		{
			StringGameState.Add(State, Value);
		}
		else
		{
			StringGameState[State] += Value;
		}
	}

	public void AppendStringGameState(string State, string Value, string Separator)
	{
		if (!StringGameState.ContainsKey(State))
		{
			StringGameState.Add(State, Value);
		}
		else
		{
			StringGameState[State] = StringGameState[State] + Separator + Value;
		}
	}

	public void SetStringGameState(string State, string Value)
	{
		StringGameState[State] = Value;
	}

	public void RemoveStringGameState(string State)
	{
		StringGameState.Remove(State);
	}

	public int GetIntGameState(string State, int Default = 0)
	{
		if (IntGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void SetIntGameState(string State, int Value)
	{
		IntGameState[State] = Value;
	}

	public int ModIntGameState(string State, int Value)
	{
		int num = GetIntGameState(State) + Value;
		IntGameState[State] = num;
		return num;
	}

	public void RemoveIntGameState(string State)
	{
		IntGameState.Remove(State);
	}

	public long GetInt64GameState(string State, long Default = 0L)
	{
		if (Int64GameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public void SetInt64GameState(string State, long Value)
	{
		Int64GameState[State] = Value;
	}

	public void RemoveInt64GameState(string State)
	{
		Int64GameState.Remove(State);
	}

	public bool GetBooleanGameState(string State, bool Default = false)
	{
		if (BooleanGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return Default;
	}

	public bool TryGetBooleanGameState(string State, out bool Result)
	{
		return BooleanGameState.TryGetValue(State, out Result);
	}

	public void SetBooleanGameState(string State, bool Value)
	{
		BooleanGameState[State] = Value;
	}

	public void RemoveBooleanGameState(string State)
	{
		BooleanGameState.Remove(State);
	}

	public object GetObjectGameState(string State)
	{
		if (ObjectGameState.TryGetValue(State, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetObjectGameState(string State, Location2D _Value)
	{
		throw new InvalidDataException("Don't set Location2D game states!");
	}

	public T RequireGameState<T>(string StateID, Func<T> generator) where T : class
	{
		if (ObjectGameState.ContainsKey(StateID))
		{
			return ObjectGameState[StateID] as T;
		}
		ObjectGameState.Add(StateID, generator());
		return ObjectGameState[StateID] as T;
	}

	public void SetObjectGameState(string State, object Value)
	{
		ObjectGameState[State] = Value;
	}

	public int GetWorldSeed(string Key = null)
	{
		if (!GetBooleanGameState("WorldSeedReady"))
		{
			MetricsManager.LogError("world seed requested before world init, will result in state undetermined by world seed");
		}
		if (!HasIntGameState("WorldSeed"))
		{
			bool flag = false;
			string stringGameState = GetStringGameState("OriginalWorldSeed");
			if (!string.IsNullOrEmpty(stringGameState) && stringGameState[0] == '#')
			{
				try
				{
					SetIntGameState("WorldSeed", Convert.ToInt32(GetStringGameState("OriginalWorldSeed").Substring(1)));
					flag = true;
				}
				catch (Exception)
				{
				}
			}
			if (!flag)
			{
				SetIntGameState("WorldSeed", new System.Random(Hash.String("WorldSeed" + stringGameState)).Next(0, int.MaxValue));
			}
		}
		if (Key == null)
		{
			return GetIntGameState("WorldSeed");
		}
		return Hash.String(Key + GetIntGameState("WorldSeed"));
	}

	public void CreateNewGame()
	{
		_Player = new GamePlayer();
		PlayerName = "";
		Quests = new Dictionary<string, Quest>();
		FinishedQuests = new Dictionary<string, Quest>();
		StringGameState = new Dictionary<string, string>();
		IntGameState = new Dictionary<string, int>();
		Int64GameState = new Dictionary<string, long>();
		ObjectGameState = new Dictionary<string, object>();
		BooleanGameState = new Dictionary<string, bool>();
		BlueprintsSeen = new HashSet<string>();
		WorldMazes = new Dictionary<string, Maze>();
		Segments = 0L;
		Turns = 0L;
		TimeOffset = Stat.Random(0, 365) * 1200 + 325;
		TimeTicks = TimeOffset;
		PlayerReputation.Init();
		WallTime = new Stopwatch();
		WallTime.Start();
		_walltime = 0L;
		MemoryHelper.GCCollectMax();
	}

	public void SaveQuests(SerializationWriter Writer)
	{
		Writer.Write(Quests.Count);
		foreach (string key in Quests.Keys)
		{
			Writer.Write(key);
			Quests[key].Save(Writer);
		}
		Writer.Write(FinishedQuests.Count);
		foreach (string key2 in FinishedQuests.Keys)
		{
			Writer.Write(key2);
			FinishedQuests[key2].Save(Writer);
		}
	}

	public void LoadQuests(SerializationReader Reader)
	{
		Quests = new Dictionary<string, Quest>();
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string key = Reader.ReadString();
			Quests.Add(key, Quest.Load(Reader));
		}
		FinishedQuests = new Dictionary<string, Quest>();
		num = Reader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			string key2 = Reader.ReadString();
			FinishedQuests.Add(key2, Quest.Load(Reader));
		}
	}

	public static string GetSaveDirectory()
	{
		return Path.Combine(GetAppdataDirectory(), "Saves");
	}

	public static string GetAppdataDirectory()
	{
		return XRLCore.SavePath;
	}

	public string GetCacheDirectory(string FileName = null)
	{
		if (_CacheDirectory == null)
		{
			string text = Path.Combine(GetSaveDirectory(), GameID);
			try
			{
				Directory.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				MetricsManager.LogException("GetCacheDirectory", ex);
				XRLCore.LogError("Thread Exception during GetCacheDirectory " + text + "  it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex.ToString());
				return null;
			}
			_CacheDirectory = text;
		}
		if (!FileName.IsNullOrEmpty())
		{
			return Path.Combine(_CacheDirectory, FileName);
		}
		return _CacheDirectory;
	}

	public static XRLGame LoadGame(string GameName, bool ShowPopup = false, Dictionary<string, object> GameState = null)
	{
		XRLGame Return = null;
		int fileVersion = -1;
		string gameVersion = "{unknown}";
		Loading.LoadTask("Loading game", delegate
		{
			try
			{
				File.Exists(DataManager.SavePath(GameName + ".gz"));
				if (!File.Exists(DataManager.SavePath(GameName)))
				{
					if (ShowPopup)
					{
						Popup.Show("No saved game exists. (" + DataManager.SavePath(GameName) + ")");
					}
				}
				else
				{
					FileStream fileStream = File.OpenRead(DataManager.SavePath(GameName));
					byte[] array = new byte[fileStream.Length];
					fileStream.Read(array, 0, array.Length);
					fileStream.Close();
					fileStream.Dispose();
					using (MemoryStream stream = new MemoryStream(array))
					{
						using SerializationReader serializationReader = new SerializationReader(stream);
						XRL.World.GameObject.ExternalLoadBindings = new Dictionary<XRL.World.GameObject, List<ExternalEventBind>>();
						if (serializationReader.ReadInt32() != 123457)
						{
							gameVersion = "2.0.167.0 or prior";
							throw new Exception("Save file is the incorrect version.");
						}
						fileVersion = (serializationReader.FileVersion = serializationReader.ReadInt32());
						gameVersion = serializationReader.ReadString();
						if (serializationReader.FileVersion < 223 || serializationReader.FileVersion > 264)
						{
							throw new Exception("Save file is the incorrect version (" + gameVersion + ").");
						}
						The.Core.ResetGameBasedStaticCaches();
						ImposterManager.qudClearImposters();
						UnityEngine.Debug.Log("Load game object...");
						Return = (XRLGame)serializationReader.ReadObject();
						XRLCore.Core.Game = Return;
						UnityEngine.Debug.Log("Load player...");
						Return._Player = GamePlayer.Load(serializationReader);
						if (serializationReader.ReadInt32() != 111111)
						{
							throw new Exception("checkval (1) wasn't correct");
						}
						UnityEngine.Debug.Log("Load zone manager...");
						Return.ZoneManager = ZoneManager.Load(serializationReader);
						if (serializationReader.ReadInt32() != 222222)
						{
							throw new Exception("checkval (2) wasn't correct");
						}
						Return.ActionManager = ActionManager.Load(serializationReader);
						AbilityManager.Load(serializationReader);
						Return.LoadQuests(serializationReader);
						if (serializationReader.ReadInt32() != 333333)
						{
							throw new Exception("checkval (3) wasn't correct");
						}
						Return.PlayerReputation = Reputation.Load(serializationReader);
						if (serializationReader.ReadInt32() != 333444)
						{
							throw new Exception("checkval (34 wasn't correct");
						}
						UnityEngine.Debug.Log("Load globals...");
						Examiner.LoadGlobals(serializationReader);
						if (serializationReader.ReadInt32() != 444444)
						{
							throw new Exception("checkval (4) wasn't correct");
						}
						TinkerItem.LoadGlobals(serializationReader);
						Return.WorldMazes = serializationReader.ReadDictionary<string, Maze>();
						Return.Accomplishments = serializationReader.ReadList<string>();
						Factions.Load(serializationReader);
						XRLCore.Core.Game.sultanHistory = History.Load(serializationReader);
						if (serializationReader.ReadInt32() != 555555)
						{
							throw new Exception("checkval (5) wasn't correct");
						}
						Gender.Clear();
						PronounSet.Clear();
						Gender.LoadAll(serializationReader);
						PronounSet.LoadAll(serializationReader);
						if (serializationReader.ReadInt32() != 666666)
						{
							throw new Exception("checkval (6) wasn't correct");
						}
						int num = serializationReader.ReadInt32();
						Return.ObjectGameState = new Dictionary<string, object>();
						for (int i = 0; i < num; i++)
						{
							string text = serializationReader.ReadString();
							IGamestatePostload gamestatePostload = null;
							if (text.StartsWith("~!"))
							{
								IObjectGamestateCustomSerializer objectGamestateCustomSerializer = Activator.CreateInstance(ModManager.ResolveType(text.Substring(2))) as IObjectGamestateCustomSerializer;
								Return.ObjectGameState.Add(serializationReader.ReadString(), objectGamestateCustomSerializer.GameLoad(serializationReader));
								gamestatePostload = objectGamestateCustomSerializer as IGamestatePostload;
							}
							else
							{
								object obj = serializationReader.ReadObject();
								Return.ObjectGameState.Add(text, obj);
								gamestatePostload = obj as IGamestatePostload;
							}
							gamestatePostload?.OnGamestatePostload(Return, serializationReader);
						}
						int num2 = serializationReader.ReadInt32();
						Return.BlueprintsSeen = new HashSet<string>();
						for (int j = 0; j < num2; j++)
						{
							Return.BlueprintSeen(serializationReader.ReadString());
						}
						UnityEngine.Debug.Log("Read game objects...");
						serializationReader.ReadGameObjects();
						Return.GraveyardZone = new Zone();
						Return.Graveyard = Return.GraveyardZone.GetCell(0, 0) as GraveyardCell;
						foreach (IGameSystem system in Return.Systems)
						{
							system.LoadGame(serializationReader);
						}
						Return.RequireSystem(() => new PsychicHunterSystem());
						XRL.World.GameObject.ExternalLoadBindings = null;
						if (serializationReader.Errors > 0)
						{
							Popup.DisplayLoadError("save", serializationReader.Errors);
						}
					}
					The.Core.HostileWalkObjects = new List<XRL.World.GameObject>();
					The.Core.OldHostileWalkObjects = new List<XRL.World.GameObject>();
					The.Core.CludgeCreaturesRendered = new List<XRL.World.GameObject>();
					UnityEngine.Debug.Log("Collect...");
					MemoryHelper.GCCollect();
					The.Core.Reset();
					int intGameState = Return.GetIntGameState("NextRandomSeed");
					if (intGameState == 0)
					{
						intGameState = Return.GetIntGameState("RandomSeed");
					}
					Stat.ReseedFrom(Return.ZoneManager?.ActiveZone?.ZoneID + intGameState);
					MarkovBook.CorpusData.Clear();
					Gender.Init();
					PronounSet.Init();
					NameStyles.CheckInit();
					try
					{
						FungalVisionary.VisionLevel = Return.GetIntGameState("FungalVisionLevel");
						GameManager.Instance.GreyscaleLevel = Return.GetIntGameState("GreyscaleLevel");
					}
					catch (Exception)
					{
					}
					Return.ImportGameState(GameState);
					UnityEngine.Debug.Log("Seed: " + Return.GetStringGameState("OriginalWorldSeed"));
					Return._Player._Body.Inventory?.Validate();
					Return._Player.Messages.Cache_0_12Valid = false;
				}
			}
			catch (Exception ex3)
			{
				string message = "That save file appears to be corrupt, you can try to restore the backup in your save directory (" + DataManager.SanitizePathForDisplay(GameName) + ".bak) by removing the 'bak' file extension.";
				if (ModManager.TryGetStackMod(ex3, out var Mod, out var Frame))
				{
					MethodBase method = Frame.GetMethod();
					string text2 = method.DeclaringType?.FullName + "." + method.Name;
					Mod.Error(text2 + "::" + ex3);
					message = "That save file is likely not loading because of a mod error from " + Mod.DisplayTitleStripped + " (" + text2 + "), make sure the correct mods are enabled or contact the mod author.";
				}
				else
				{
					if (fileVersion < 264)
					{
						message = "That save file looks like it's from an older save format revision (" + gameVersion + "). Sorry!\n\nYou can probably change to a previous branch in your game client and get it to load if you want to finish it off.";
					}
					else if (fileVersion > 264)
					{
						message = "That save file looks like it's from a newer save format revision (" + gameVersion + ").\n\nYou can probably change to a newer branch in your game client and get it to load if you want to finish it off.";
					}
					MetricsManager.LogException("XRLGame.LoadGame::", ex3, "serialization_error");
				}
				if (ShowPopup)
				{
					Popup.Show(message);
				}
				throw;
			}
		});
		try
		{
			The.Player.FireEvent("GameRestored");
			AfterGameLoadedEvent.Send();
			ModManager.CallAfterGameLoaded();
			if (Return?.ZoneManager?.ActiveZone != null)
			{
				ZoneManager.PaintWalls(Return.ZoneManager.ActiveZone);
				ZoneManager.PaintWater(Return.ZoneManager.ActiveZone);
				Return.ZoneManager.ActiveZone.Activated();
			}
			if (fileVersion <= 261)
			{
				JournalAPI.FixMerchantMapNotes();
			}
			MemoryHelper.GCCollectMax();
		}
		catch (Exception)
		{
		}
		return Return;
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		Directory.CreateDirectory(destDirName);
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite: false);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array = directories;
			foreach (DirectoryInfo directoryInfo2 in array)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				DirectoryCopy(directoryInfo2.FullName, destDirName2, copySubDirs);
			}
		}
	}

	public bool RestoreCheckpoint()
	{
		string text = "Checkpoint.sav";
		if (!File.Exists(The.Game.GetCacheDirectory("Checkpoint.sav")))
		{
			text = "checkpoint.sav";
		}
		if (LoadGame(The.Game.GetCacheDirectory(text)) == null)
		{
			return false;
		}
		if (Directory.Exists(GetCacheDirectory("CheckpointZoneCache")))
		{
			if (Directory.Exists(GetCacheDirectory("ZoneCache")))
			{
				Directory.Delete(GetCacheDirectory("ZoneCache"), recursive: true);
			}
			DirectoryCopy(GetCacheDirectory("CheckpointZoneCache"), GetCacheDirectory("ZoneCache"), copySubDirs: true);
		}
		SaveCopy(text, "Primary.sav");
		return true;
	}

	public void Checkpoint()
	{
		UnityEngine.Debug.Log("Checkpointing...");
		if (Directory.Exists(GetCacheDirectory("CheckpointZoneCache")))
		{
			Directory.Delete(GetCacheDirectory("CheckpointZoneCache"), recursive: true);
		}
		if (Directory.Exists(GetCacheDirectory("ZoneCache")))
		{
			DirectoryCopy(GetCacheDirectory("ZoneCache"), GetCacheDirectory("CheckpointZoneCache"), copySubDirs: true);
		}
		if (SaveGame("Checkpoint.sav", "Saving Checkpoint"))
		{
			SaveCopy("Checkpoint.sav", "Primary.sav");
		}
	}

	public void QuickSave()
	{
		if (Directory.Exists(Path.Combine(GetCacheDirectory(), "QuickZoneCache")))
		{
			Directory.Delete(Path.Combine(GetCacheDirectory(), "QuickZoneCache"), recursive: true);
		}
		if (Directory.Exists(Path.Combine(GetCacheDirectory(), "ZoneCache")))
		{
			DirectoryCopy(Path.Combine(GetCacheDirectory(), "ZoneCache"), Path.Combine(GetCacheDirectory(), "QuickZoneCache"), copySubDirs: true);
		}
		if (SaveGame("Quick.sav"))
		{
			SaveCopy("Quick.sav", "Primary.sav");
		}
	}

	public bool QuickLoad()
	{
		if (!File.Exists(The.Game.GetCacheDirectory("Quick.sav")))
		{
			return LoadGame(The.Game.GetCacheDirectory("Primary.sav")) != null;
		}
		if (LoadGame(The.Game.GetCacheDirectory("Quick.sav")) == null)
		{
			return false;
		}
		if (Directory.Exists(GetCacheDirectory("QuickZoneCache")))
		{
			if (Directory.Exists(GetCacheDirectory("ZoneCache")))
			{
				Directory.Delete(GetCacheDirectory("ZoneCache"), recursive: true);
			}
			DirectoryCopy(GetCacheDirectory("QuickZoneCache"), GetCacheDirectory("ZoneCache"), copySubDirs: true);
		}
		SaveCopy("Quick.sav", "Primary.sav");
		return true;
	}

	public void SaveCopy(string From, string To)
	{
		if (!File.Exists(GetCacheDirectory(From)))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (File.Exists(Path.Combine(GetCacheDirectory(), To)))
		{
			try
			{
				File.Copy(GetCacheDirectory(To), GetCacheDirectory(To + ".bak"), overwrite: true);
				flag = true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Exception making save backup: " + ex);
			}
		}
		try
		{
			File.Copy(GetCacheDirectory(From), GetCacheDirectory(To), overwrite: true);
			flag2 = true;
		}
		catch (Exception ex2)
		{
			UnityEngine.Debug.LogError("Exception copying save: " + ex2);
			if (flag)
			{
				try
				{
					File.Copy(GetCacheDirectory(To + ".bak"), GetCacheDirectory(To), overwrite: true);
					UnityEngine.Debug.LogWarning("Backup restored to" + To);
				}
				catch (Exception ex3)
				{
					UnityEngine.Debug.LogError("Exception restoring backup: " + ex3);
				}
			}
		}
		if (!flag2)
		{
			return;
		}
		try
		{
			File.Copy(GetCacheDirectory(From + ".json"), GetCacheDirectory(To + ".json"), overwrite: true);
		}
		catch (Exception ex4)
		{
			UnityEngine.Debug.LogError("Exception copying save info: " + ex4);
		}
	}

	public bool SaveGame(string GameName, string message = "Saving game")
	{
		if (!Running)
		{
			return false;
		}
		bool result = true;
		Loading.LoadTask(message, delegate
		{
			MemoryHelper.GCCollectMax();
			try
			{
				SetIntGameState("FungalVisionLevel", FungalVisionary.VisionLevel);
				SetIntGameState("GreyscaleLevel", GameManager.Instance.GreyscaleLevel);
			}
			catch (Exception)
			{
			}
			try
			{
				if (WallTime != null)
				{
					_walltime += WallTime.ElapsedTicks;
					WallTime.Reset();
					WallTime.Start();
				}
				else
				{
					WallTime = new Stopwatch();
					WallTime.Start();
				}
				SetIntGameState("NextRandomSeed", Stat.Rnd.Next());
				XRL.World.GameObject body = Player.Body;
				SaveGameJSON saveGameJSON = new SaveGameJSON
				{
					SaveVersion = 264,
					GameVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
					ID = GameID,
					Name = PlayerName,
					Level = body.Statistics["Level"].Value,
					GenoSubType = ""
				};
				if (body.HasProperty("Genotype"))
				{
					saveGameJSON.GenoSubType += body.Property["Genotype"];
				}
				if (body.HasProperty("Subtype"))
				{
					saveGameJSON.GenoSubType = saveGameJSON.GenoSubType + " " + body.Property["Subtype"];
				}
				saveGameJSON.GameMode = GetStringGameState("GameMode", "Classic");
				RenderEvent renderEvent = body.RenderForUI();
				saveGameJSON.CharIcon = renderEvent.Tile;
				saveGameJSON.FColor = renderEvent.GetForegroundColorChar();
				saveGameJSON.DColor = renderEvent.GetDetailColorChar();
				saveGameJSON.Location = ZoneManager.GetZoneDisplayName(body.CurrentZone.ZoneID);
				TimeSpan timeSpan = TimeSpan.FromTicks(_walltime);
				saveGameJSON.InGameTime = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
				saveGameJSON.Turn = Turns;
				saveGameJSON.SaveTime = DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString();
				saveGameJSON.ModsEnabled = ModManager.GetRunningMods();
				File.WriteAllText(Path.Combine(GetCacheDirectory(), GameName + ".json"), JsonConvert.SerializeObject(saveGameJSON, Formatting.Indented));
				if (File.Exists(Path.Combine(GetCacheDirectory(), GameName)))
				{
					try
					{
						File.Copy(Path.Combine(GetCacheDirectory(), GameName), Path.Combine(GetCacheDirectory(), GameName) + ".bak", overwrite: true);
					}
					catch (Exception ex2)
					{
						UnityEngine.Debug.LogError("Exception making save backup:" + ex2.ToString());
					}
				}
				using (FileStream stream = File.Create(Path.Combine(GetCacheDirectory(), GameName + ".tmp")))
				{
					using SerializationWriter serializationWriter = new SerializationWriter(stream, _bSerializePlayer: true);
					serializationWriter.Write(123457);
					serializationWriter.Write(264);
					serializationWriter.Write(Assembly.GetExecutingAssembly().GetName().Version.ToString());
					serializationWriter.FileVersion = 264;
					serializationWriter.WriteObject(this);
					_Player.Save(serializationWriter);
					serializationWriter.Write(111111);
					ZoneManager.Save(serializationWriter);
					serializationWriter.Write(222222);
					ActionManager.Save(serializationWriter);
					AbilityManager.Save(serializationWriter);
					SaveQuests(serializationWriter);
					serializationWriter.Write(333333);
					PlayerReputation.Save(serializationWriter);
					serializationWriter.Write(333444);
					Examiner.SaveGlobals(serializationWriter);
					serializationWriter.Write(444444);
					TinkerItem.SaveGlobals(serializationWriter);
					serializationWriter.Write(WorldMazes);
					serializationWriter.Write<string>(Accomplishments);
					Factions.Save(serializationWriter);
					sultanHistory.Save(serializationWriter);
					serializationWriter.Write(555555);
					Gender.SaveAll(serializationWriter);
					PronounSet.SaveAll(serializationWriter);
					serializationWriter.Write(666666);
					if (ObjectGameState.Count > 0)
					{
						serializationWriter.Write(ObjectGameState.Count);
						foreach (KeyValuePair<string, object> item in ObjectGameState)
						{
							if (item.Value is IObjectGamestateCustomSerializer objectGamestateCustomSerializer)
							{
								serializationWriter.Write("~!" + objectGamestateCustomSerializer.GetType().FullName);
								serializationWriter.Write(item.Key);
								objectGamestateCustomSerializer.GameSave(serializationWriter);
							}
							else
							{
								serializationWriter.Write(item.Key);
								serializationWriter.WriteObject(item.Value);
								if (item.Value is IGamestatePostsave gamestatePostsave)
								{
									gamestatePostsave.OnGamestatePostsave(this, serializationWriter);
								}
							}
						}
					}
					else
					{
						serializationWriter.Write(0);
					}
					if (BlueprintsSeen.Count > 0)
					{
						serializationWriter.Write(BlueprintsSeen.Count);
						foreach (string item2 in BlueprintsSeen)
						{
							serializationWriter.Write(item2);
						}
					}
					else
					{
						serializationWriter.Write(0);
					}
					serializationWriter.WriteGameObjects();
					foreach (IGameSystem system in Systems)
					{
						system.SaveGame(serializationWriter);
					}
					serializationWriter.AppendTokenTables();
				}
				File.Copy(Path.Combine(GetCacheDirectory(), GameName + ".tmp"), Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
				File.Delete(Path.Combine(GetCacheDirectory(), GameName + ".tmp"));
			}
			catch (Exception ex3)
			{
				result = false;
				MetricsManager.LogException("SaveGame", ex3);
				XRLCore.LogError("Exception during SaveGame it's also automatically on clipboard so just paste into IM or e-mail to support@freeholdentertainment.com: " + ex3.ToString());
				if (File.Exists(Path.Combine(GetCacheDirectory(), GameName) + ".bak"))
				{
					try
					{
						File.Copy(Path.Combine(GetCacheDirectory(), GameName) + ".bak", Path.Combine(GetCacheDirectory(), GameName), overwrite: true);
					}
					catch (Exception)
					{
					}
				}
				Popup.Show("There was a fatal exception attempting to save your game. Caves of Qud attempted to recover your prior save. You probably want to close the game and reload your most recent save. It'd be helpful to send the save and logs to support@freeholdgames.com");
			}
		});
		MemoryHelper.GCCollectMax();
		return result;
	}

	public void ImportGameState(Dictionary<string, object> GameState)
	{
		if (GameState == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in GameState)
		{
			if (item.Value is string value)
			{
				SetStringGameState(item.Key, value);
				continue;
			}
			int? num = item.Value as int?;
			if (num.HasValue)
			{
				SetIntGameState(item.Key, num.Value);
				continue;
			}
			long? num2 = item.Value as long?;
			if (num2.HasValue)
			{
				SetInt64GameState(item.Key, num2.Value);
				continue;
			}
			bool? flag = item.Value as bool?;
			if (flag.HasValue)
			{
				SetBooleanGameState(item.Key, flag.Value);
			}
			else
			{
				SetObjectGameState(item.Key, item.Value);
			}
		}
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (ZoneManager == null)
		{
			return false;
		}
		return ZoneManager.WantEvent(ID, cascade);
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		if (ZoneManager == null)
		{
			return false;
		}
		return ZoneManager.HandleEvent(E);
	}
}
