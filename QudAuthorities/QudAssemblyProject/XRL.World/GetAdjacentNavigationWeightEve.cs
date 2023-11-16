using System.Collections.Generic;
using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetAdjacentNavigationWeightEvent : IAdjacentNavigationWeightEvent
{
	public new static readonly int ID;

	private static List<GetAdjacentNavigationWeightEvent> Pool;

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

	static GetAdjacentNavigationWeightEvent()
	{
		ID = MinEvent.AllocateID();
		MinEvent.RegisterPoolReset(ResetPool);
		MinEvent.RegisterPoolCount(typeof(GetAdjacentNavigationWeightEvent).Name, () => (Pool != null) ? Pool.Count : 0);
	}

	public GetAdjacentNavigationWeightEvent()
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

	public static GetAdjacentNavigationWeightEvent FromPool()
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
			GetAdjacentNavigationWeightEvent getAdjacentNavigationWeightEvent = null;
			List<Cell> localAdjacentCells = Cell.GetLocalAdjacentCells();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				Cell cell = localAdjacentCells[i];
				int j = 0;
				for (int count2 = cell.Objects.Count; j < count2; j++)
				{
					int priorWeight = Weight;
					GameObject gameObject = cell.Objects[j];
					int intProperty = gameObject.GetIntProperty("AdjacentNavigationWeight");
					if (intProperty > Weight)
					{
						Weight = intProperty;
					}
					if (gameObject.WantEvent(ID, MinEvent.CascadeLevel))
					{
						if (getAdjacentNavigationWeightEvent == null)
						{
							Zone.UnpackNav(Nav, out var Smart, out var Burrower, out var Autoexploring, out var Flying, out var WallWalker, out var IgnoresWalls, out var Swimming, out var Slimewalking, out var Aquatic, out var Polypwalking, out var Strutwalking, out var Juggernaut, out var Reefer);
							getAdjacentNavigationWeightEvent = FromPool();
							getAdjacentNavigationWeightEvent.Cell = Cell;
							getAdjacentNavigationWeightEvent.Actor = Actor;
							getAdjacentNavigationWeightEvent.Uncacheable = Uncacheable;
							getAdjacentNavigationWeightEvent.Weight = Weight;
							getAdjacentNavigationWeightEvent.PriorWeight = 0;
							getAdjacentNavigationWeightEvent.Smart = Smart;
							getAdjacentNavigationWeightEvent.Burrower = Burrower;
							getAdjacentNavigationWeightEvent.Autoexploring = Autoexploring;
							getAdjacentNavigationWeightEvent.Flying = Flying;
							getAdjacentNavigationWeightEvent.WallWalker = WallWalker;
							getAdjacentNavigationWeightEvent.IgnoresWalls = IgnoresWalls;
							getAdjacentNavigationWeightEvent.Swimming = Swimming;
							getAdjacentNavigationWeightEvent.Slimewalking = Slimewalking;
							getAdjacentNavigationWeightEvent.Aquatic = Aquatic;
							getAdjacentNavigationWeightEvent.Polypwalking = Polypwalking;
							getAdjacentNavigationWeightEvent.Strutwalking = Strutwalking;
							getAdjacentNavigationWeightEvent.Juggernaut = Juggernaut;
							getAdjacentNavigationWeightEvent.Reefer = Reefer;
						}
						getAdjacentNavigationWeightEvent.Object = gameObject;
						getAdjacentNavigationWeightEvent.PriorWeight = priorWeight;
						getAdjacentNavigationWeightEvent.AdjacentCell = cell;
						gameObject.HandleEvent(getAdjacentNavigationWeightEvent);
						Weight = getAdjacentNavigationWeightEvent.Weight;
						Uncacheable = getAdjacentNavigationWeightEvent.Uncacheable;
					}
					if (Weight >= 100)
					{
						return Weight;
					}
				}
			}
		}
		return Weight;
	}
}
