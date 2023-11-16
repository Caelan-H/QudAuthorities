using System;

namespace XRL.World.Parts;

[Serializable]
public class ReachabilityBuilder : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			try
			{
				Zone currentZone = ParentObject.CurrentZone;
				for (int i = 0; i < currentZone.Height; i++)
				{
					for (int j = 0; j < currentZone.Width; j++)
					{
						Cell cell = currentZone.GetCell(j, i);
						if (!cell.IsReachable() && !cell.IsSolid())
						{
							currentZone.BuildReachableMap(j, i);
						}
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Reachability build", x);
			}
		}
		return base.FireEvent(E);
	}
}
