using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetWaterRitualLiquidEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Target;

	public string Liquid;

	public new static readonly int ID;

	private static List<GetWaterRitualLiquidEvent> Pool;

	private static int PoolCounter;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetWaterRitualLiquidEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetWaterRitualLiquidEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetWaterRitualLiquidEvent()
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

	public static GetWaterRitualLiquidEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override void Reset()
	{
		Actor = null;
		Target = null;
		Liquid = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static string GetFor(GameObject Actor, GameObject Target)
	{
		string text = Target.GetPropertyOrTag("WaterRitualLiquid", "water");
		bool flag = true;
		if (flag && Target.HasRegisteredEvent("GetWaterRitualLiquid"))
		{
			Event @event = Event.New("GetWaterRitualLiquid");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("Target", Target);
			@event.SetParameter("Liquid", text);
			flag = Target.FireEvent(@event);
			text = @event.GetStringParameter("Liquid");
		}
		if (flag && Target.WantEvent(ID, CascadeLevel))
		{
			GetWaterRitualLiquidEvent getWaterRitualLiquidEvent = FromPool();
			getWaterRitualLiquidEvent.Actor = Actor;
			getWaterRitualLiquidEvent.Target = Target;
			getWaterRitualLiquidEvent.Liquid = text;
			flag = Target.HandleEvent(getWaterRitualLiquidEvent);
			text = getWaterRitualLiquidEvent.Liquid;
		}
		int num = text.IndexOf('-');
		if (num != -1)
		{
			string text2 = text.Substring(0, num);
			if (text2 == "water")
			{
				int num2 = text.IndexOf(',');
				if (num2 != -1)
				{
					text2 = text.Substring(num2 + 1);
					num = text2.IndexOf('-');
					if (num != -1)
					{
						text2 = text2.Substring(0, num);
					}
				}
			}
			text = text2;
		}
		return text;
	}
}
