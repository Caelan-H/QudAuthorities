using System;

namespace XRL.World.Parts;

[Serializable]
public class CompleteQuestOnTaken : IPart
{
	public string Quest;

	public string QuestStep;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != InventoryActionEvent.ID && ID != TakenEvent.ID && ID != DroppedEvent.ID && ID != OnQuestAddedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		CompleteQuest(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnQuestAddedEvent E)
	{
		if (E.Quest.Name == Quest)
		{
			CompleteQuest(E.Subject);
		}
		return base.HandleEvent(E);
	}

	public void CompleteQuest(GameObject actor)
	{
		if (actor != null && actor.IsPlayer())
		{
			The.Game.FinishQuestStep(Quest, QuestStep);
		}
	}
}
