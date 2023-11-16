using System;

namespace XRL.World.ZoneBuilders;

[Serializable]
public class OmonporchGroveBuilder
{
	public bool BuildZone(Zone Z)
	{
		Z.BuildReachableMap(0, 0, bClearFirst: false);
		return true;
	}
}
