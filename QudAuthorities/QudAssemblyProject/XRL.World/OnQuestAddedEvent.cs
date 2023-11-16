using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class OnQuestAddedEvent : MinEvent
{
	public GameObject Subject;

	public Quest Quest;

	public new static readonly int ID;

	private static List<OnQuestAddedEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static OnQuestAddedEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(OnQuestAddedEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public OnQuestAddedEvent()
	{
		base.ID = ID;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public override void Reset()
	{
		Subject = null;
		Quest = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static OnQuestAddedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static void Send(GameObject Subject, Quest Quest)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Subject) && Subject.HasRegisteredEvent("OnQuestAdded"))
		{
			Event @event = Event.New("OnQuestAdded");
			@event.SetParameter("Subject", Subject);
			@event.SetParameter("Quest", Quest);
			@event.SetParameter("QuestName", Quest.Name);
			flag = Subject.FireEvent(@event);
		}
		if (flag && GameObject.validate(ref Subject) && Subject.WantEvent(ID, CascadeLevel))
		{
			OnQuestAddedEvent onQuestAddedEvent = FromPool();
			onQuestAddedEvent.Subject = Subject;
			onQuestAddedEvent.Quest = Quest;
			flag = Subject.HandleEvent(onQuestAddedEvent);
		}
	}
}
