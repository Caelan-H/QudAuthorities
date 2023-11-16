using System;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Conversations.Parts;
using XRL.World.Parts;

namespace XRL.World.Conversations;

[HasModSensitiveStaticCache]
[HasConversationDelegate]
public static class ConversationDelegates
{
	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PredicateReceiver> _Predicates;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, ActionReceiver> _Actions;

	[ModSensitiveStaticCache(false)]
	private static Dictionary<string, PartGeneratorReceiver> _PartGenerators;

	public static Dictionary<string, PredicateReceiver> Predicates
	{
		get
		{
			if (_Predicates == null)
			{
				LoadDelegates();
			}
			return _Predicates;
		}
	}

	public static Dictionary<string, ActionReceiver> Actions
	{
		get
		{
			if (_Actions == null)
			{
				LoadDelegates();
			}
			return _Actions;
		}
	}

	public static Dictionary<string, PartGeneratorReceiver> PartGenerators
	{
		get
		{
			if (_PartGenerators == null)
			{
				LoadDelegates();
			}
			return _PartGenerators;
		}
	}

	public static void CreatePredicate(MethodInfo Method, Dictionary<string, PredicateReceiver> Predicates)
	{
		CreatePredicate(Method.GetCustomAttribute<ConversationDelegate>(), Method, Predicates);
	}

	public static void CreatePredicate(ConversationDelegate Attribute, MethodInfo Method, Dictionary<string, PredicateReceiver> Predicates)
	{
		string text = Attribute.Key ?? Method.Name;
		int startIndex = text.UpToNthIndex(char.IsUpper, 2, 0);
		ConversationPredicate predicate = (ConversationPredicate)Method.CreateDelegate(typeof(ConversationPredicate));
		Predicates[text] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Player));
		if (Attribute.Inverse)
		{
			if (Attribute.InverseKey != null)
			{
				Predicates[Attribute.InverseKey] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Player));
			}
			else
			{
				Predicates[text.Insert(startIndex, "Not")] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Player));
			}
		}
		if (!Attribute.Speaker)
		{
			return;
		}
		if (Attribute.SpeakerKey != null)
		{
			Predicates[Attribute.SpeakerKey] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		else
		{
			Predicates[text.Insert(startIndex, "Speaker")] = (IConversationElement e, string x) => predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		if (!Attribute.Inverse)
		{
			return;
		}
		if (Attribute.SpeakerInverseKey != null)
		{
			Predicates[Attribute.SpeakerInverseKey] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Speaker));
		}
		else
		{
			Predicates[text.Insert(startIndex, "SpeakerNot")] = (IConversationElement e, string x) => !predicate(DelegateContext.Set(e, x, The.Speaker));
		}
	}

	public static void CreateAction(MethodInfo Method, Dictionary<string, ActionReceiver> Actions)
	{
		CreateAction(Method.GetCustomAttribute<ConversationDelegate>(), Method, Actions);
	}

	public static void CreateAction(ConversationDelegate Attribute, MethodInfo Method, Dictionary<string, ActionReceiver> Actions)
	{
		string text = Attribute.Key ?? Method.Name;
		int startIndex = text.UpToNthIndex(char.IsUpper, 2, 0);
		ConversationAction action = Method.CreateDelegate(typeof(ConversationAction)) as ConversationAction;
		Actions[text] = delegate(IConversationElement e, string x)
		{
			action(DelegateContext.Set(e, x, The.Player));
		};
		if (!Attribute.Speaker)
		{
			return;
		}
		if (Attribute.SpeakerKey != null)
		{
			Actions[Attribute.SpeakerKey] = delegate(IConversationElement e, string x)
			{
				action(DelegateContext.Set(e, x, The.Speaker));
			};
		}
		else
		{
			Actions[text.Insert(startIndex, "Speaker")] = delegate(IConversationElement e, string x)
			{
				action(DelegateContext.Set(e, x, The.Speaker));
			};
		}
	}

	[PreGameCacheInit]
	public static void LoadDelegates()
	{
		_Predicates = new Dictionary<string, PredicateReceiver>();
		_Actions = new Dictionary<string, ActionReceiver>();
		_PartGenerators = new Dictionary<string, PartGeneratorReceiver>();
		Type typeFromHandle = typeof(ConversationDelegate);
		Type typeFromHandle2 = typeof(HasConversationDelegate);
		ConversationPartGenerator generator;
		foreach (MethodInfo item in ModManager.GetMethodsWithAttribute(typeFromHandle, typeFromHandle2, Cache: false))
		{
			try
			{
				ConversationDelegate customAttribute = item.GetCustomAttribute<ConversationDelegate>();
				string text = customAttribute.Key ?? item.Name;
				if (item.ReturnType == typeof(bool))
				{
					CreatePredicate(customAttribute, item, Predicates);
				}
				else if (item.ReturnType == typeof(void))
				{
					CreateAction(customAttribute, item, Actions);
				}
				else if (item.ReturnType == typeof(IConversationPart))
				{
					generator = item.CreateDelegate(typeof(ConversationPartGenerator)) as ConversationPartGenerator;
					PartGenerators[text] = (IConversationElement e, string x) => generator(DelegateContext.Set(e, x, The.Player));
				}
				else
				{
					MetricsManager.LogAssemblyError(item, "Conversation delegate " + text + " does not match signature.");
				}
			}
			catch (Exception message)
			{
				MetricsManager.LogAssemblyError(item, message);
			}
		}
	}

	[ConversationDelegate]
	public static bool IfHaveQuest(DelegateContext Context)
	{
		return The.Game.HasQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveActiveQuest(DelegateContext Context)
	{
		return The.Game.HasUnfinishedQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfFinishedQuest(DelegateContext Context)
	{
		return The.Game.HasFinishedQuest(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfFinishedQuestStep(DelegateContext Context)
	{
		return The.Game.FinishedQuestStep(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveObservation(DelegateContext Context)
	{
		return JournalAPI.HasObservation(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveObservationWithTag(DelegateContext Context)
	{
		return JournalAPI.HasObservationWithTag(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveSultanNoteWithTag(DelegateContext Context)
	{
		return JournalAPI.HasSultanNoteWithTag(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveVillageNote(DelegateContext Context)
	{
		return JournalAPI.HasVillageNote(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveState(DelegateContext Context)
	{
		return The.Game.HasGameState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfHaveConversationState(DelegateContext Context)
	{
		return The.Conversation.HasState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfTestState(DelegateContext Context)
	{
		return The.Game.TestGameState(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfLastChoice(DelegateContext Context)
	{
		return ConversationUI.LastChoice?.ID == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfHaveText(DelegateContext Context)
	{
		return (ConversationUI.CurrentNode?.Text)?.Contains(Context.Value) ?? false;
	}

	[ConversationDelegate]
	public static bool IfTime(DelegateContext Context)
	{
		if (!Context.Value.TryParseRange(out var Low, out var High))
		{
			return false;
		}
		long num = Calendar.TotalTimeTicks % 1200;
		if (High < Low)
		{
			if (num < Low)
			{
				return num <= High;
			}
			return true;
		}
		if (num >= Low)
		{
			return num <= High;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfLedBy(DelegateContext Context)
	{
		if (The.Speaker.pBrain?.PartyLeader == null)
		{
			return false;
		}
		if (Context.Value == "*")
		{
			return true;
		}
		if (Context.Value.EqualsNoCase("Player"))
		{
			return The.Speaker.IsPlayerLed();
		}
		return The.Speaker.pBrain.IsLedBy(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfZoneName(DelegateContext Context)
	{
		if (The.ActiveZone != null)
		{
			string zoneBaseDisplayName = The.ZoneManager.GetZoneBaseDisplayName(The.ActiveZone.ZoneID, Mutate: false);
			if (zoneBaseDisplayName != null && zoneBaseDisplayName.Contains(Context.Value))
			{
				return true;
			}
			string zoneNameContext = The.ZoneManager.GetZoneNameContext(The.ActiveZone.ZoneID);
			if (zoneNameContext != null && zoneNameContext.Contains(Context.Value))
			{
				return true;
			}
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneLevel(DelegateContext Context)
	{
		if (The.ActiveZone == null)
		{
			return false;
		}
		int z = The.ActiveZone.Z;
		if (Context.Value.TryParseRange(out var Low, out var High) && Low <= z)
		{
			return High >= z;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneTier(DelegateContext Context)
	{
		if (The.ActiveZone == null)
		{
			return false;
		}
		int newTier = The.ActiveZone.NewTier;
		if (Context.Value.TryParseRange(out var Low, out var High) && Low <= newTier)
		{
			return High >= newTier;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfZoneWorld(DelegateContext Context)
	{
		return The.ActiveZone?.ZoneWorld == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfZoneID(DelegateContext Context)
	{
		if (The.ActiveZone?.ZoneID == null)
		{
			return false;
		}
		return The.ActiveZone.ZoneID.StartsWith(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfUnderstood(DelegateContext Context)
	{
		if (Examiner.UnderstandingTable == null)
		{
			Loading.LoadTask("Loading medication names", Examiner.Reset);
		}
		if (Examiner.UnderstandingTable.TryGetValue(Context.Value, out var value))
		{
			return value == 2;
		}
		return false;
	}

	[ConversationDelegate]
	public static bool IfCommand(DelegateContext Context)
	{
		return PredicateEvent.GetFor(Context.Element, "IfCommand", Context.Value);
	}

	[ConversationDelegate]
	public static bool IfReputationAtLeast(DelegateContext Context)
	{
		int num = Context.Value.ToUpperInvariant() switch
		{
			"LOVED" => 2, 
			"LIKED" => 1, 
			"INDIFFERENT" => 0, 
			"DISLIKED" => -1, 
			"HATED" => -2, 
			_ => int.MaxValue, 
		};
		return The.Game.PlayerReputation.getLevel(The.Speaker.GetPrimaryFaction()) >= num;
	}

	[ConversationDelegate]
	public static bool IfSlynthCandidate(DelegateContext Context)
	{
		SlynthQuestSystem system = The.Game.GetSystem<SlynthQuestSystem>();
		if (system == null)
		{
			return false;
		}
		if (Context.Value.EqualsNoCase("Mayor"))
		{
			string propertyOrTag = The.Speaker.GetPropertyOrTag("Mayor");
			if (propertyOrTag != null)
			{
				return system.candidateFactions.Contains(propertyOrTag);
			}
			return false;
		}
		if (Context.Value.EqualsNoCase("Primary"))
		{
			string primaryFaction = The.Speaker.GetPrimaryFaction();
			if (primaryFaction != null)
			{
				return system.candidateFactions.Contains(primaryFaction);
			}
			return false;
		}
		return system.candidateFactions.Contains(Context.Value);
	}

	[ConversationDelegate]
	public static bool IfSlynthChosen(DelegateContext Context)
	{
		if (!The.Game.StringGameState.TryGetValue("SlynthSettlementFaction", out var value))
		{
			return false;
		}
		if (Context.Value.EqualsNoCase("Mayor"))
		{
			return value == The.Speaker.GetPropertyOrTag("Mayor");
		}
		if (Context.Value.EqualsNoCase("Primary"))
		{
			return value == The.Speaker.GetPrimaryFaction();
		}
		return value == Context.Value;
	}

	[ConversationDelegate]
	public static bool IfHindriarch(DelegateContext Context)
	{
		if (!The.Game.StringGameState.TryGetValue("HindrenMysteryOutcomeHindriarch", out var value))
		{
			return false;
		}
		return value == Context.Value;
	}

	[ConversationDelegate(Inverse = false)]
	public static bool IfIn100(DelegateContext Context)
	{
		if (int.TryParse(Context.Value, out var result))
		{
			if (result > 0)
			{
				if (result < 100)
				{
					return Stat.Random(1, 100) <= result;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfGenotype(DelegateContext Context)
	{
		return Context.Target.GetGenotype() == Context.Value;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfSubtype(DelegateContext Context)
	{
		return Context.Target.GetSubtype() == Context.Value;
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfTrueKin(DelegateContext Context)
	{
		return Context.Target.IsTrueKin();
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfMutant(DelegateContext Context)
	{
		return Context.Target.IsMutant();
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveItem(DelegateContext Context)
	{
		return Context.Target.HasObjectInInventory(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveItemWithID(DelegateContext Context)
	{
		return Context.Target.HasObjectInInventory((GameObject o) => o.idmatch(Context.Value));
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHavePart(DelegateContext Context)
	{
		return Context.Target.HasPart(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfWearingBlueprint(DelegateContext Context)
	{
		return Context.Target.HasObjectEquipped(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveBlueprint(DelegateContext Context)
	{
		return Context.Target.Inventory.HasObject(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveTag(DelegateContext Context)
	{
		return Context.Target.HasTag(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveProperty(DelegateContext Context)
	{
		return Context.Target.HasProperty(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfHaveTagOrProperty(DelegateContext Context)
	{
		return Context.Target.HasTagOrProperty(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static bool IfLevelLessOrEqual(DelegateContext Context)
	{
		return Context.Target.Stat("Level") <= Convert.ToInt32(Context.Value);
	}

	[ConversationDelegate]
	[Obsolete("TODO: Replace usage with parts")]
	public static void Execute(DelegateContext Context)
	{
		try
		{
			string[] array = Context.Value.Split(':');
			ModManager.ResolveType(array[0]).GetMethod(array[1]).Invoke(null, null);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Error invoking command:" + Context.Value, x);
		}
	}

	[ConversationDelegate]
	public static void FinishQuest(DelegateContext Context)
	{
		The.Game.FinishQuest(Context.Value);
	}

	[ConversationDelegate]
	public static void Achievement(DelegateContext Context)
	{
		AchievementManager.SetAchievement(Context.Value);
	}

	[ConversationDelegate(Speaker = true)]
	public static void FireEvent(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		Event @event = new Event(array[0]);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length > 1)
			{
				@event.SetParameter(array2[0], array2[1]);
			}
			else
			{
				@event.SetParameter(array2[0], 1);
			}
		}
		Context.Target.FireEvent(@event);
	}

	[ConversationDelegate]
	public static void FireSystemsEvent(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		Event @event = new Event(array[0]);
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length > 1)
			{
				@event.SetParameter(array2[0], array2[1]);
			}
			else
			{
				@event.SetParameter(array2[0], 1);
			}
		}
		The.Game.FireSystemsEvent(@event);
	}

	[ConversationDelegate]
	public static void SetStringState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		if (array.Length > 1)
		{
			The.Game.SetStringGameState(array[0], array[1]);
		}
		else
		{
			The.Game.RemoveStringGameState(array[0]);
		}
	}

	[ConversationDelegate]
	public static void SetIntState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		int result;
		if (array.Length == 1)
		{
			The.Game.RemoveIntGameState(array[0]);
		}
		else if (int.TryParse(array[1], out result))
		{
			The.Game.SetIntGameState(array[0], result);
		}
	}

	[ConversationDelegate]
	public static void AddIntState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		if (array.Length > 1 && int.TryParse(array[1], out var result))
		{
			The.Game.IntGameState.TryGetValue(array[0], out var value);
			The.Game.IntGameState[array[0]] = value + result;
		}
	}

	[ConversationDelegate]
	public static void SetBooleanState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		bool result;
		if (array.Length == 1)
		{
			The.Game.RemoveBooleanGameState(array[0]);
		}
		else if (bool.TryParse(array[1], out result))
		{
			The.Game.SetBooleanGameState(array[0], result);
		}
	}

	[ConversationDelegate]
	public static void ToggleBooleanState(DelegateContext Context)
	{
		The.Game.SetBooleanGameState(Context.Value, !The.Game.GetBooleanGameState(Context.Value));
	}

	[ConversationDelegate(Speaker = true)]
	public static void SetStringProperty(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		if (array.Length == 1)
		{
			Context.Target.RemoveStringProperty(Context.Value);
		}
		else
		{
			Context.Target.SetStringProperty(array[0], array[1]);
		}
	}

	[ConversationDelegate(Speaker = true)]
	public static void SetIntProperty(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		int result;
		if (array.Length == 1)
		{
			Context.Target.RemoveIntProperty(Context.Value);
		}
		else if (int.TryParse(array[1], out result))
		{
			Context.Target.SetIntProperty(array[0], result);
		}
	}

	[ConversationDelegate]
	public static void SetStringConversationState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		if (array.Length == 1)
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else
		{
			The.Conversation[array[0]] = array[1];
		}
	}

	[ConversationDelegate]
	public static void SetIntConversationState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		int result;
		if (array.Length == 1)
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else if (int.TryParse(array[1], out result))
		{
			The.Conversation[array[0]] = result;
		}
	}

	[ConversationDelegate]
	public static void SetBooleanConversationState(DelegateContext Context)
	{
		string[] array = Context.Value.Split(',');
		bool result;
		if (array.Length == 1)
		{
			The.Conversation.RemoveState(Context.Value);
		}
		else if (bool.TryParse(array[1], out result))
		{
			The.Conversation[array[0]] = result;
		}
	}

	[ConversationDelegate]
	public static void CallScript(DelegateContext Context)
	{
		int num = Context.Value.LastIndexOf('.');
		if (num != -1)
		{
			ModManager.ResolveType(Context.Value.Substring(0, num)).GetMethod(Context.Value.Substring(num + 1))?.Invoke(null, null);
		}
	}

	[ConversationDelegate]
	public static void ClearOwner(DelegateContext Context)
	{
		Zone activeZone = The.ActiveZone;
		for (int i = 0; i < activeZone.Width; i++)
		{
			for (int j = 0; j < activeZone.Height; j++)
			{
				int k = 0;
				for (int count = activeZone.Map[i][j].Objects.Count; k < count; k++)
				{
					if (activeZone.Map[i][j].Objects[k].HasTagOrProperty(Context.Value))
					{
						activeZone.Map[i][j].Objects[k].pPhysics.Owner = null;
					}
				}
			}
		}
	}

	[ConversationDelegate]
	public static void RevealMapNoteByID(DelegateContext Context)
	{
		JournalMapNote mapNote = JournalAPI.GetMapNote(Context.Value);
		string text = The.Speaker?.pBrain?.GetPrimaryFaction();
		if (mapNote != null && !text.IsNullOrEmpty())
		{
			Faction faction = Factions.get(text);
			mapNote.attributes.Add(faction.NoBuySecretString);
			if (faction.Visible)
			{
				mapNote.AppendHistory(" {{K|-learned from " + faction.getFormattedName() + "}}");
			}
			JournalAPI.RevealMapNote(mapNote);
		}
	}

	[ConversationDelegate]
	public static void RevealObservation(DelegateContext Context)
	{
		JournalObservation observation = JournalAPI.GetObservation(Context.Value);
		string text = The.Speaker?.pBrain?.GetPrimaryFaction();
		if (observation != null && !text.IsNullOrEmpty())
		{
			Faction faction = Factions.get(text);
			observation.attributes.Add(faction.NoBuySecretString);
			if (faction.Visible)
			{
				observation.AppendHistory(" {{K|-learned from " + faction.getFormattedName() + "}}");
			}
			JournalAPI.RevealObservation(observation);
		}
	}

	[ConversationDelegate]
	public static IConversationPart StartQuest(DelegateContext Context)
	{
		return new QuestHandler(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart GiveItem(DelegateContext Context)
	{
		return new ReceiveItem(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart TakeItem(DelegateContext Context)
	{
		return new TakeItem(Context.Value);
	}

	[ConversationDelegate]
	public static IConversationPart CompleteQuestStep(DelegateContext Context)
	{
		string[] array = Context.Value.Split('~');
		int result = -1;
		int num = array[1].IndexOf('|');
		if (num >= 0)
		{
			if (!int.TryParse(array[1].Substring(num + 1), out result))
			{
				result = -1;
			}
			array[1] = array[1].Substring(0, num);
		}
		return new QuestHandler(array[0], array[1], result, 2);
	}
}
