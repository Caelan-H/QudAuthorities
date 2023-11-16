using System.Collections.Generic;
using Genkit;
using XRL.Rules;
using XRL.World.AI.Pathfinding;

namespace XRL.World.ZoneBuilders;

public class ForceConnectionsPlus : ZoneBuilderSandbox
{
	public bool CaveLike = true;

	public bool BuildZone(Zone Z)
	{
		return _BuildZone(Z);
	}

	public bool _BuildZone(Zone Z, List<Point2D> additional = null)
	{
		if (Z.GetZoneProperty("DisableForcedConnections") == "Yes")
		{
			return true;
		}
		List<Cell> cells = Z.GetCells((Cell c) => c.IsPassable());
		Z.ClearReachableMap();
		Z.SetReachable(cells[0].X, cells[0].Y);
		Z.BuildReachableMap(cells[0].X, cells[0].Y, bClearFirst: false);
		GameObject gameObject = null;
		gameObject = GameObjectFactory.Factory.CreateObject("Drillbot");
		for (int i = 1; i < cells.Count; i++)
		{
			if (Z.IsReachable(cells[i].X, cells[i].Y))
			{
				continue;
			}
			FindPath findPath = new FindPath(Z, cells[0].X, cells[0].Y, Z, cells[i].X, cells[i].Y, PathGlobal: false, PathUnlimited: true, gameObject, AddNoise: true);
			if (!findPath.bFound)
			{
				continue;
			}
			foreach (Cell step in findPath.Steps)
			{
				Z.ReachableMap[step.X, step.Y] = true;
				step.ClearObjectsWithIntProperty("Wall");
			}
			if (CaveLike)
			{
				foreach (Cell step2 in findPath.Steps)
				{
					if (step2.IsReachable() || step2.HasObjectWithTag("Wall"))
					{
						continue;
					}
					foreach (Cell localAdjacentCell in step2.GetLocalAdjacentCells(Stat.Random(1, 2)))
					{
						if (Stat.Random(1, 100) <= 75)
						{
							Z.ReachableMap[localAdjacentCell.X, localAdjacentCell.Y] = true;
							localAdjacentCell.ClearObjectsWithIntProperty("Wall");
						}
					}
				}
			}
			Z.BuildReachableMap(cells[i].X, cells[i].Y, bClearFirst: false);
		}
		return true;
	}
}
