using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

[Serializable]
public class ConversationScript : IPart
{
	public Conversation customConversation;

	public bool RecordConversationAsProperty;

	public string ConversationID;

	public string Quest;

	public string PreQuestConversationID;

	public string InQuestConversationID;

	public string PostQuestConversationID;

	public bool ClearLost;

	public int ChargeUse;

	public string Filter;

	public string FilterExtras;

	public string Color;

	public string Append;

	[NonSerialized]
	private static Event eCanHaveSmartUseConversation = new Event("CanHaveSmartUseConversation");

	public ConversationScript()
	{
	}

	public ConversationScript(string ConversationID)
		: this()
	{
		this.ConversationID = ConversationID;
	}

	public ConversationScript(string ConversationID, bool ClearLost)
		: this(ConversationID)
	{
		this.ClearLost = ClearLost;
	}

	public override bool SameAs(IPart p)
	{
		ConversationScript conversationScript = p as ConversationScript;
		if (conversationScript.customConversation != customConversation)
		{
			return false;
		}
		if (conversationScript.ConversationID != ConversationID)
		{
			return false;
		}
		if (conversationScript.Quest != Quest)
		{
			return false;
		}
		if (conversationScript.PreQuestConversationID != PreQuestConversationID)
		{
			return false;
		}
		if (conversationScript.PostQuestConversationID != PostQuestConversationID)
		{
			return false;
		}
		if (conversationScript.ClearLost != ClearLost)
		{
			return false;
		}
		if (conversationScript.ChargeUse != ChargeUse)
		{
			return false;
		}
		if (conversationScript.Filter != Filter)
		{
			return false;
		}
		if (conversationScript.FilterExtras != FilterExtras)
		{
			return false;
		}
		if (conversationScript.Color != Color)
		{
			return false;
		}
		if (conversationScript.Append != Append)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginConversationEvent.ID && (ID != CanGiveDirectionsEvent.ID || !ClearLost) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "RecordConversationAsProperty", RecordConversationAsProperty);
		E.AddEntry(this, "ConversationID", ConversationID);
		E.AddEntry(this, "Quest", Quest);
		E.AddEntry(this, "PreQuestConversationID", PreQuestConversationID);
		E.AddEntry(this, "InQuestConversationID", InQuestConversationID);
		E.AddEntry(this, "PostQuestConversationID", PostQuestConversationID);
		E.AddEntry(this, "ClearLost", ClearLost);
		E.AddEntry(this, "ChargeUse", ChargeUse);
		E.AddEntry(this, "Filter", Filter);
		E.AddEntry(this, "FilterExtras", FilterExtras);
		E.AddEntry(this, "Color", Color);
		E.AddEntry(this, "Append", Append);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginConversationEvent E)
	{
		if (E.SpeakingWith == ParentObject)
		{
			if (ChargeUse > 0)
			{
				ParentObject.UseCharge(ChargeUse, LiveOnly: false, 0L);
			}
			if (RecordConversationAsProperty)
			{
				E.Actor.ModIntProperty(ParentObject.Blueprint + "Chat", 1);
			}
			if (!Filter.IsNullOrEmpty())
			{
				The.Conversation?.AddPart(new TextFilter(Filter, FilterExtras));
			}
			if (!Append.IsNullOrEmpty())
			{
				The.Conversation?.AddPart(new TextInsert
				{
					Text = Append
				});
			}
			int intProperty = E.Actor.GetIntProperty("PronounSetTick");
			bool speakerGivePronouns = false;
			bool speakerGetPronouns = false;
			bool speakerGetNewPronouns = false;
			ParentObject.ModIntProperty("ConversationCount", 1);
			if (PronounSet.EnableConversationalExchange && ParentObject.pBrain != null && !ParentObject.HasProperty("FugueCopy"))
			{
				if (E.Actor.GetPronounSet() != null)
				{
					if (ParentObject.GetIntProperty("ConversationCount") == 1)
					{
						speakerGetPronouns = true;
					}
					else if (ParentObject.GetIntProperty("KnowsPlayerPronounsAsOfTick") < intProperty)
					{
						speakerGetPronouns = true;
						speakerGetNewPronouns = true;
					}
					ParentObject.SetIntProperty("KnowsPlayerPronounsAsOfTick", intProperty);
				}
				if (ParentObject.GetPronounSet() != null && !ParentObject.IsPronounSetKnown())
				{
					ParentObject.PronounSetKnown = true;
					speakerGivePronouns = true;
				}
			}
			string text = PronounExchangeDescription(E.Actor, ParentObject, speakerGivePronouns, speakerGetPronouns, speakerGetNewPronouns);
			if (text != null)
			{
				string text2 = "[" + ColorUtility.CapitalizeExceptFormatting(text) + ".]\n\n";
				E.Conversation.Introduction += text2;
				ConversationUI.StartNode?.AddPart(new TextInsert
				{
					Text = text2,
					Prepend = true,
					Spoken = false
				});
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanGiveDirectionsEvent E)
	{
		if (E.SpeakingWith == ParentObject && ClearLost && !E.PlayerCompanion)
		{
			E.CanGiveDirections = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject != E.Actor)
		{
			E.AddAction("Chat", "chat", "Chat", null, 'h', FireOnActor: false, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Chat")
		{
			AttemptConversation(E.Actor, Silent: false, null, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && (!ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && ParentObject.FireEvent(eCanHaveSmartUseConversation))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && (!ParentObject.IsPlayerLed() || !ParentObject.IsMobile()) && ParentObject.FireEvent(eCanHaveSmartUseConversation) && AttemptConversation())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool IsPhysicalConversationPossible(GameObject Who, GameObject With, bool ShowPopup = true, bool AllowCombat = false, bool AllowFrozen = false, int ChargeUse = 0)
	{
		if (Who.HasEffect("Stun") || Who.HasEffect("Exhausted") || Who.HasEffect("Paralyzed") || Who.HasEffect("Asleep"))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You are in no shape to start a conversation.");
			}
			return false;
		}
		if (Who.HasEffect("Confused"))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You can't seem to make out what " + With.t() + With.Is + " saying.");
			}
			return false;
		}
		if (!IsConversationallyResponsiveEvent.Check(With, Who, out var Message, Physical: true))
		{
			if (!string.IsNullOrEmpty(Message) && ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(Message);
			}
			return false;
		}
		if (ChargeUse > 0 && !With.TestCharge(ChargeUse, LiveOnly: false, 0L))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.T() + With.Is + " utterly unresponsive.");
			}
			return false;
		}
		if (!AllowFrozen && With.IsFrozen())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You hear a muffled grunting coming from inside the block of ice.");
			}
			return false;
		}
		if (!AllowCombat && With.IsHostileTowards(Who))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.T() + With.GetVerb("refuse") + " to speak to you.");
			}
			return false;
		}
		if (!AllowCombat && With.IsEngagedInMelee())
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(With.T() + With.Is + " engaged in hand-to-hand combat and" + With.Is + " too busy to have a conversation with you.");
			}
			return false;
		}
		if (!CanStartConversationEvent.Check(Who, With, out var FailureMessage, Physical: true))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				if (!string.IsNullOrEmpty(FailureMessage))
				{
					Popup.ShowFail(FailureMessage);
				}
				else
				{
					Popup.ShowFail("You cannot seem to engage " + With.T() + " in conversation.");
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsPhysicalConversationPossible(GameObject With, bool ShowPopup = false, bool AllowCombat = false, bool AllowFrozen = false, int ChargeUse = 0)
	{
		return IsPhysicalConversationPossible(IComponent<GameObject>.ThePlayer, With, ShowPopup, AllowCombat, AllowFrozen, ChargeUse);
	}

	public bool IsPhysicalConversationPossible(bool ShowPopup = false, bool AllowCombat = false, bool AllowFrozen = false)
	{
		return IsPhysicalConversationPossible(ParentObject, ShowPopup, AllowCombat, AllowFrozen, ChargeUse);
	}

	public static bool IsMentalConversationPossible(GameObject Who, GameObject With, bool ShowPopup = true, bool AllowCombat = false, int ChargeUse = 0)
	{
		if (!Who.HasPart("Telepathy"))
		{
			return false;
		}
		if (!Who.CanMakeTelepathicContactWith(With) || (ChargeUse > 0 && !With.TestCharge(ChargeUse, LiveOnly: false, 0L)))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You can sense nothing from " + With.t() + ".");
			}
			return false;
		}
		if (!IsConversationallyResponsiveEvent.Check(With, Who, out var Message, Physical: false, Mental: true))
		{
			if (!string.IsNullOrEmpty(Message) && ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail(Message);
			}
			return false;
		}
		if (!AllowCombat && With.IsHostileTowards(Who))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				Popup.ShowFail("You sense only hostility from " + With.t() + ".");
			}
			return false;
		}
		if (!CanStartConversationEvent.Check(Who, With, out var FailureMessage, Physical: false, Mental: true))
		{
			if (ShowPopup && Who.IsPlayer())
			{
				if (!string.IsNullOrEmpty(FailureMessage))
				{
					Popup.ShowFail(FailureMessage);
				}
				else
				{
					Popup.ShowFail("You cannot seem to make contact with " + With.t() + ".");
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsMentalConversationPossible(GameObject With, bool ShowPopup = false, bool AllowCombat = false, int ChargeUse = 0)
	{
		return IsMentalConversationPossible(IComponent<GameObject>.ThePlayer, With, ShowPopup, AllowCombat, ChargeUse);
	}

	public bool IsMentalConversationPossible(bool ShowPopup = false, bool AllowCombat = false)
	{
		return IsMentalConversationPossible(ParentObject, ShowPopup, AllowCombat, ChargeUse);
	}

	public string GetActiveConversationID()
	{
		if (!string.IsNullOrEmpty(Quest))
		{
			if (!string.IsNullOrEmpty(PostQuestConversationID) && XRLCore.Core.Game.FinishedQuest(Quest))
			{
				return PostQuestConversationID;
			}
			if (!string.IsNullOrEmpty(PreQuestConversationID) && !XRLCore.Core.Game.HasQuest(Quest))
			{
				return PreQuestConversationID;
			}
			if (!string.IsNullOrEmpty(InQuestConversationID) && XRLCore.Core.Game.HasQuest(Quest) && !XRLCore.Core.Game.FinishedQuest(Quest))
			{
				return InQuestConversationID;
			}
		}
		return ConversationID;
	}

	public bool AttemptConversation(GameObject With, bool Silent = false, bool? Mental = null, IEvent ParentEvent = null)
	{
		if (!GameObject.validate(ParentObject) || !GameObject.validate(ref With) || ParentObject == With)
		{
			return false;
		}
		int num = ParentObject.DistanceTo(With);
		if (!Mental.HasValue && num > 1)
		{
			Mental = true;
		}
		if (Mental == true && !IsMentalConversationPossible(!Silent))
		{
			return false;
		}
		if (num > 1 || !IsPhysicalConversationPossible(!Silent))
		{
			if (!Mental.HasValue)
			{
				if (!IsMentalConversationPossible(!Silent))
				{
					return false;
				}
			}
			else if (Mental == false)
			{
				return false;
			}
		}
		if (customConversation != null)
		{
			OldConversationUI.HaveConversation(customConversation, ParentObject, TradeEnabled: true, bCheckObjectTalking: true, null, ParentEvent, Filter, FilterExtras, Append, Color, Mental != true, Mental == true);
		}
		else
		{
			ConversationUI.HaveConversation(GetActiveConversationID(), ParentObject, TradeEnabled: true, Mental != true, Mental == true);
		}
		return true;
	}

	public bool AttemptConversation(bool Silent = false, bool? Mental = null, IEvent ParentEvent = null)
	{
		return AttemptConversation(IComponent<GameObject>.ThePlayer, Silent, Mental, ParentEvent);
	}

	public static string PronounExchangeDescription(GameObject Player, GameObject Speaker, bool SpeakerGivePronouns, bool SpeakerGetPronouns, bool SpeakerGetNewPronouns)
	{
		if (Speaker.pBrain == null)
		{
			return null;
		}
		if (Speaker.HasProperty("FugueCopy"))
		{
			return null;
		}
		if (SpeakerGivePronouns && SpeakerGetPronouns)
		{
			return "you and " + Speaker.t() + " exchange pronouns; " + Speaker.its + " are " + Speaker.GetPronounSet().GetShortName();
		}
		if (SpeakerGivePronouns)
		{
			return Speaker.t() + Speaker.GetVerb("give") + " you " + Speaker.its + " pronouns, which are " + Speaker.GetPronounSet().GetShortName();
		}
		if (SpeakerGetNewPronouns)
		{
			return "you give " + Speaker.t() + " your new pronouns";
		}
		if (SpeakerGetPronouns)
		{
			return "you give " + Speaker.t() + " your pronouns";
		}
		return null;
	}
}
