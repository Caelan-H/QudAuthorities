using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetNavigationWeightEvent : INavigationWeightEvent
{
	public new static readonly int ID;

	private static List<GetNavigationWeightEvent> Pool;

	private static int PoolCounter;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static GetNavigationWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetNavigationWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetNavigationWeightEvent()
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

	public static GetNavigationWeightEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public static int GetFor(Cell Cell, GameObject Actor, ref bool Uncacheable, int Weight = 0, int Nav = 0)
	{
		if (Weight >= 100)
		{
			return Weight;
		}
		if (Cell != null)
		{
			GetNavigationWeightEvent getNavigationWeightEvent = null;
			int i = 0;
			for (int count = Cell.Objects.Count; i < count; i++)
			{
				int priorWeight = Weight;
				GameObject gameObject = Cell.Objects[i];
				int intProperty = gameObject.GetIntProperty("NavigationWeight");
				if (intProperty > Weight)
				{
					Weight = intProperty;
				}
				if (gameObject.WantEvent(ID, MinEvent.CascadeLevel))
				{
					if (getNavigationWeightEvent == null)
					{
						Zone.UnpackNav(Nav, out var Smart, out var Burrower, out var Autoexploring, out var Flying, out var WallWalker, out var IgnoresWalls, out var Swimming, out var Slimewalking, out var Aquatic, out var Polypwalking, out var Strutwalking, out var Juggernaut, out var Reefer);
						getNavigationWeightEvent = FromPool();
						getNavigationWeightEvent.Cell = Cell;
						getNavigationWeightEvent.Actor = Actor;
						getNavigationWeightEvent.Uncacheable = Uncacheable;
						getNavigationWeightEvent.Weight = Weight;
						getNavigationWeightEvent.PriorWeight = 0;
						getNavigationWeightEvent.Smart = Smart;
						getNavigationWeightEvent.Burrower = Burrower;
						getNavigationWeightEvent.Autoexploring = Autoexploring;
						getNavigationWeightEvent.Flying = Flying;
						getNavigationWeightEvent.WallWalker = WallWalker;
						getNavigationWeightEvent.IgnoresWalls = IgnoresWalls;
						getNavigationWeightEvent.Swimming = Swimming;
						getNavigationWeightEvent.Slimewalking = Slimewalking;
						getNavigationWeightEvent.Aquatic = Aquatic;
						getNavigationWeightEvent.Polypwalking = Polypwalking;
						getNavigationWeightEvent.Strutwalking = Strutwalking;
						getNavigationWeightEvent.Juggernaut = Juggernaut;
						getNavigationWeightEvent.Reefer = Reefer;
					}
					getNavigationWeightEvent.Object = gameObject;
					getNavigationWeightEvent.PriorWeight = priorWeight;
					gameObject.HandleEvent(getNavigationWeightEvent);
					Weight = getNavigationWeightEvent.Weight;
					Uncacheable = getNavigationWeightEvent.Uncacheable;
				}
				if (Weight >= 100)
				{
					return Weight;
				}
			}
		}
		return Weight;
	}
}
