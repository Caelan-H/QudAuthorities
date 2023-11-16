using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.World.Parts.Mutation;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetMutationTermEvent : MinEvent
{
	public const string DEFAULT_TERM = "mutation";

	public const string DEFAULT_COLOR = "M";

	public GameObject Creature;

	public BaseMutation Mutation;

	public string Term;

	public string Color;

	public new static readonly int ID;

	private static List<GetMutationTermEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetMutationTermEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetMutationTermEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetMutationTermEvent()
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

	public static GetMutationTermEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static GetMutationTermEvent FromPool(GameObject Creature, BaseMutation Mutation, string Term, string Color)
	{
		GetMutationTermEvent getMutationTermEvent = FromPool();
		getMutationTermEvent.Creature = Creature;
		getMutationTermEvent.Mutation = Mutation;
		getMutationTermEvent.Term = Term;
		getMutationTermEvent.Color = Color;
		return getMutationTermEvent;
	}

	public override void Reset()
	{
		Creature = null;
		Term = null;
		Color = null;
		base.Reset();
	}

	public static void GetFor(GameObject Creature, out string Term, out string Color, BaseMutation Mutation = null)
	{
		Term = "mutation";
		Color = "M";
		bool flag = true;
		if (flag && GameObject.validate(ref Creature) && Creature.HasRegisteredEvent("GetMutationTerm"))
		{
			Event @event = Event.New("GetMutationTerm");
			@event.SetParameter("Creature", Creature);
			@event.SetParameter("Mutation", Mutation);
			@event.SetParameter("Term", Term);
			@event.SetParameter("Color", Color);
			flag = Creature.FireEvent(@event);
			Term = @event.GetStringParameter("Term");
			Color = @event.GetStringParameter("Color");
		}
		if (flag && GameObject.validate(ref Creature) && Creature.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMutationTermEvent getMutationTermEvent = FromPool(Creature, Mutation, Term, Color);
			flag = Creature.HandleEvent(getMutationTermEvent);
			Term = getMutationTermEvent.Term;
			Color = getMutationTermEvent.Color;
		}
	}

	public static string GetFor(GameObject Creature, BaseMutation Mutation = null)
	{
		string text = "mutation";
		string text2 = "M";
		bool flag = true;
		if (flag && GameObject.validate(ref Creature) && Creature.HasRegisteredEvent("GetMutationTerm"))
		{
			Event @event = Event.New("GetMutationTerm");
			@event.SetParameter("Creature", Creature);
			@event.SetParameter("Mutation", Mutation);
			@event.SetParameter("Term", text);
			@event.SetParameter("Color", text2);
			flag = Creature.FireEvent(@event);
			text = @event.GetStringParameter("Term");
			text2 = @event.GetStringParameter("Color");
		}
		if (flag && GameObject.validate(ref Creature) && Creature.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetMutationTermEvent getMutationTermEvent = FromPool(Creature, Mutation, text, text2);
			flag = Creature.HandleEvent(getMutationTermEvent);
			text = getMutationTermEvent.Term;
			text2 = getMutationTermEvent.Color;
		}
		return text;
	}
}
