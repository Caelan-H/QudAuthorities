using XRL.World.Parts;

namespace XRL.World.ZoneBuilders;

public class AmbientStabilization
{
	public string Strength = "40";

	public bool BuildZone(Zone Z)
	{
		Z.GetCell(0, 0).AddObject("AmbientStabilization").GetPart<XRL.World.Parts.AmbientStabilization>()
			.Strength = Strength.RollCached();
		return true;
	}
}
