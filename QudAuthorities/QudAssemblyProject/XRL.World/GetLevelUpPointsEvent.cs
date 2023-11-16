using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetLevelUpPointsEvent : MinEvent
{
	public GameObject Actor;

	public int Level;

	public int HitPoints;

	public int SkillPoints;

	public int MutationPoints;

	public int AttributePoints;

	public int AttributeBonus;

	public int RapidAdvancement;

	public new static readonly int ID;

	private static List<GetLevelUpPointsEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetLevelUpPointsEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount("GetLevelUpPointsEvent", () => (Pool != null) ? Pool.Count : 0);
	}

	public GetLevelUpPointsEvent()
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

	public static GetLevelUpPointsEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Actor = null;
		Level = 0;
		HitPoints = 0;
		SkillPoints = 0;
		MutationPoints = 0;
		AttributePoints = 0;
		AttributeBonus = 0;
		RapidAdvancement = 0;
		base.Reset();
	}

	public static void GetFor(GameObject Actor, int Level, ref int HitPoints, ref int SkillPoints, ref int MutationPoints, ref int AttributePoints, ref int AttributeBonus, ref int RapidAdvancement)
	{
		if (Actor.HasRegisteredEvent("GetLevelUpPoints"))
		{
			Event @event = Event.New("GetLevelUpPoints");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Level", Level);
			@event.SetParameter("HitPoints", HitPoints);
			@event.SetParameter("SkillPoints", SkillPoints);
			@event.SetParameter("MutationPoints", MutationPoints);
			@event.SetParameter("AttributePoints", AttributePoints);
			@event.SetParameter("AttributeBonus", AttributeBonus);
			@event.SetParameter("RapidAdvancement", RapidAdvancement);
			bool num = Actor.FireEvent(@event);
			HitPoints = @event.GetIntParameter("HitPoints");
			SkillPoints = @event.GetIntParameter("SkillPoints");
			MutationPoints = @event.GetIntParameter("MutationPoints");
			AttributePoints = @event.GetIntParameter("AttributePoints");
			AttributeBonus = @event.GetIntParameter("AttributeBonus");
			RapidAdvancement = @event.GetIntParameter("RapidAdvancement");
			if (!num)
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetLevelUpPointsEvent getLevelUpPointsEvent = FromPool();
			getLevelUpPointsEvent.Actor = Actor;
			getLevelUpPointsEvent.Level = Level;
			getLevelUpPointsEvent.HitPoints = HitPoints;
			getLevelUpPointsEvent.SkillPoints = SkillPoints;
			getLevelUpPointsEvent.MutationPoints = MutationPoints;
			getLevelUpPointsEvent.AttributePoints = AttributePoints;
			getLevelUpPointsEvent.AttributeBonus = AttributeBonus;
			getLevelUpPointsEvent.RapidAdvancement = RapidAdvancement;
			Actor.HandleEvent(getLevelUpPointsEvent);
			HitPoints = getLevelUpPointsEvent.HitPoints;
			SkillPoints = getLevelUpPointsEvent.SkillPoints;
			MutationPoints = getLevelUpPointsEvent.MutationPoints;
			AttributePoints = getLevelUpPointsEvent.AttributePoints;
			AttributeBonus = getLevelUpPointsEvent.AttributeBonus;
			RapidAdvancement = getLevelUpPointsEvent.RapidAdvancement;
		}
	}
}
