using System;
using System.Collections.Generic;
using System.Reflection;
using Qud.API;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class ConversationNode
{
	public string ID;

	public string Text;

	public string PrependUnspoken;

	public string AppendUnspoken;

	public Conversation ParentConversation;

	public bool bCloseable = true;

	public string IfHaveQuest;

	public string IfHaveItemWithID;

	public string IfHaveState;

	public string IfNotHaveState;

	public string IfTestState;

	public string IfHavePart;

	public string IfNotHavePart;

	public string IfNotHaveQuest;

	public string IfFinishedQuest;

	public string IfFinishedQuestStep;

	public string IfNotFinishedQuest;

	public string IfHaveObservation;

	public string IfNotHaveObservation;

	public string IfHaveObservationWithTag;

	public string IfHaveSultanNoteWithTag;

	public string IfHaveVillageNote;

	public string IfNotFinishedQuestStep;

	public bool IfTrueKin;

	public bool IfNotTrueKin;

	public string IfGenotype;

	public string IfSubtype;

	public string IfNotGenotype;

	public string IfNotSubtype;

	public string StartQuest;

	public string FinishQuest;

	public string RevealMapNoteId;

	public string CompleteQuestStep;

	public string GiveItem;

	public string GiveOneItem;

	public string TakeItem;

	public string ClearOwner;

	public string SetStringState;

	public string SetIntState;

	public string AddIntState;

	public string SetBooleanState;

	public string ToggleBooleanState;

	public string IfWearingBlueprint;

	public string IfHasBlueprint;

	public string IfLevelLessOrEqual;

	public string SpecialRequirement;

	public Func<bool> IfDelegate;

	public Action OnLeaveNode;

	public bool alwaysShowAsNotVisted;

	public bool TradeNote;

	public string Filter;

	public object ScriptObject;

	public List<ConversationChoice> Choices = new List<ConversationChoice>();

	public static Dictionary<string, bool> VisitedNodes => The.Game.Player.ConversationNodesVisited;

	public bool Visited => VisitedNodes.ContainsKey(ParentConversation.ID + ID);

	public void Copy(ConversationNode source)
	{
		ID = source.ID;
		Text = source.Text;
		ParentConversation = source.ParentConversation;
		bCloseable = source.bCloseable;
		IfHaveQuest = source.IfHaveQuest;
		IfHaveItemWithID = source.IfHaveItemWithID;
		IfHaveState = source.IfHaveState;
		IfNotHaveState = source.IfNotHaveState;
		IfTestState = source.IfTestState;
		IfNotHaveQuest = source.IfNotHaveQuest;
		IfFinishedQuest = source.IfFinishedQuest;
		IfFinishedQuestStep = source.IfFinishedQuestStep;
		IfNotFinishedQuest = source.IfNotFinishedQuest;
		IfHaveObservation = source.IfHaveObservation;
		IfNotHaveObservation = source.IfNotHaveObservation;
		IfHaveObservationWithTag = source.IfHaveObservationWithTag;
		IfHaveSultanNoteWithTag = source.IfHaveSultanNoteWithTag;
		IfHaveVillageNote = source.IfHaveVillageNote;
		IfNotFinishedQuestStep = source.IfNotFinishedQuestStep;
		StartQuest = source.StartQuest;
		FinishQuest = source.FinishQuest;
		RevealMapNoteId = source.RevealMapNoteId;
		CompleteQuestStep = source.CompleteQuestStep;
		GiveItem = source.GiveItem;
		GiveOneItem = source.GiveOneItem;
		TakeItem = source.TakeItem;
		ClearOwner = source.ClearOwner;
		SetStringState = source.SetStringState;
		IfTrueKin = source.IfTrueKin;
		IfNotTrueKin = source.IfNotTrueKin;
		IfGenotype = source.IfGenotype;
		IfNotGenotype = source.IfNotGenotype;
		IfSubtype = source.IfSubtype;
		IfNotSubtype = source.IfNotSubtype;
		SetIntState = source.SetIntState;
		AddIntState = source.AddIntState;
		SetBooleanState = source.SetBooleanState;
		ToggleBooleanState = source.ToggleBooleanState;
		IfWearingBlueprint = source.IfWearingBlueprint;
		IfHasBlueprint = source.IfHasBlueprint;
		IfLevelLessOrEqual = source.IfLevelLessOrEqual;
		SpecialRequirement = source.SpecialRequirement;
		IfDelegate = source.IfDelegate;
		OnLeaveNode = source.OnLeaveNode;
		alwaysShowAsNotVisted = source.alwaysShowAsNotVisted;
		TradeNote = source.TradeNote;
		Filter = source.Filter;
		ScriptObject = source.ScriptObject;
		Choices = new List<ConversationChoice>(source.Choices.Count);
		foreach (ConversationChoice choice in source.Choices)
		{
			ConversationChoice conversationChoice = new ConversationChoice();
			conversationChoice.Copy(choice);
			conversationChoice.ParentNode = this;
			Choices.Add(conversationChoice);
		}
	}

	[Obsolete("just sort the choice list normally")]
	public void SortEndChoicesToEnd()
	{
		Choices.Sort();
	}

	public ConversationChoice AddChoice(ConversationChoice newChoice, Action<ConversationChoice> finalizer = null, bool sort = true)
	{
		newChoice.ParentNode = this;
		Choices.Add(newChoice);
		finalizer?.Invoke(newChoice);
		if (sort)
		{
			Choices.Sort();
		}
		return newChoice;
	}

	public ConversationChoice AddChoice(string text, string gotoId, Action<ConversationChoice> finalizer = null)
	{
		ConversationChoice conversationChoice = new ConversationChoice();
		conversationChoice.Text = text;
		conversationChoice.GotoID = gotoId;
		conversationChoice.ParentNode = this;
		Choices.Add(conversationChoice);
		finalizer?.Invoke(conversationChoice);
		Choices.Sort();
		return conversationChoice;
	}

	public string GetScriptClassName()
	{
		return "ConversationNodeScript_" + ParentConversation.ID + "_" + ID.ToString();
	}

	public virtual ConversationNode Enter(ConversationNode previous, GameObject speaker)
	{
		return this;
	}

	public bool TestFilter()
	{
		if (ScriptObject == null)
		{
			return true;
		}
		object[] args = new object[0];
		return (bool)ScriptObject.GetType().InvokeMember("TestFilter", BindingFlags.InvokeMethod, null, ScriptObject, args);
	}

	public virtual bool Test()
	{
		if (TestFilter() && (IfDelegate == null || IfDelegate()) && (IfHaveSultanNoteWithTag == null || JournalAPI.HasSultanNoteWithTag(IfHaveSultanNoteWithTag)) && (IfHaveObservation == null || JournalAPI.HasObservation(IfHaveObservation)) && (IfNotHaveObservation == null || !JournalAPI.HasObservation(IfNotHaveObservation)) && (IfHaveObservationWithTag == null || JournalAPI.HasObservationWithTag(IfHaveObservationWithTag)) && (IfHaveVillageNote == null || JournalAPI.HasVillageNote(IfHaveVillageNote)) && (IfHaveState == null || The.Game.HasGameState(IfHaveState)) && (IfNotHaveState == null || !The.Game.HasGameState(IfNotHaveState)) && (IfTestState == null || The.Game.HasGameState(IfTestState)) && (IfHaveQuest == null || The.Game.HasQuest(IfHaveQuest)) && ConversationChoice.TestHaveItemWithID(IfHaveItemWithID) && (IfNotHaveQuest == null || !The.Game.HasQuest(IfNotHaveQuest)) && (IfFinishedQuestStep == null || The.Game.FinishedQuestStep(IfFinishedQuestStep)) && (IfNotFinishedQuest == null || !The.Game.FinishedQuest(IfNotFinishedQuest)) && (IfFinishedQuest == null || The.Game.FinishedQuest(IfFinishedQuest)) && (IfNotFinishedQuestStep == null || !The.Game.FinishedQuestStep(IfNotFinishedQuestStep)) && (!IfTrueKin || The.Player.IsTrueKin()) && (!IfNotTrueKin || !The.Player.IsTrueKin()) && (IfGenotype == null || The.Player.GetGenotype() == IfGenotype) && (IfSubtype == null || The.Player.GetSubtype() == IfSubtype) && (IfNotGenotype == null || The.Player.GetGenotype() != IfNotGenotype) && (IfNotSubtype == null || The.Player.GetSubtype() != IfNotSubtype) && (IfHasBlueprint == null || The.Player.Inventory.FireEvent(Event.New("HasBlueprint", "Blueprint", IfHasBlueprint))) && (IfLevelLessOrEqual == null || The.Player.Stat("Level") <= Convert.ToInt32(IfLevelLessOrEqual)) && (IfWearingBlueprint == null || The.Player.HasObjectEquipped(IfWearingBlueprint)) && ConversationChoice.TestSpecialRequirement(SpecialRequirement))
		{
			return true;
		}
		return false;
	}

	public virtual void Visit(GameObject speaker, GameObject player)
	{
		VisitedNodes[ParentConversation.ID + ID] = true;
		string[] array;
		if (GiveItem != null)
		{
			array = GiveItem.Split(',');
			foreach (string objectBlueprint in array)
			{
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(objectBlueprint);
				player.ReceiveObject(gameObject);
				Popup.ShowBlock("You receive " + gameObject.a + gameObject.DisplayNameOnly + "!");
			}
		}
		if (CompleteQuestStep == null)
		{
			return;
		}
		array = CompleteQuestStep.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('~');
			if (!The.Game.HasQuest(array2[0]))
			{
				continue;
			}
			string text = array2[1];
			int num = -1;
			if (text.Contains("|"))
			{
				string[] array3 = text.Split('|');
				text = array3[0];
				num = Convert.ToInt32(array3[1]);
			}
			QuestStep questStep = The.Game.Quests[array2[0]].StepsByID[text];
			if (questStep != null && !questStep.Finished)
			{
				if (num == -1)
				{
					The.Game.FinishQuestStep(array2[0], text);
				}
				else
				{
					The.Game.FinishQuestStepXP(array2[0], text, num);
				}
			}
		}
	}
}
