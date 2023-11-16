using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("save compat")]
public class RandomBreather : IPart
{
	public string Table = "Breathers";

	[Obsolete("save compat")]
	public string Placeholder1;

	[Obsolete("save compat")]
	public bool Placeholder2;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		List<string> each = PopulationManager.GetEach(Table);
		int index = Stat.SeededRandom(ZoneManager.zoneGenerationContextZoneID, 0, each.Count - 1);
		E.ReplacementObject = GameObject.create(each[index]);
		return base.HandleEvent(E);
	}
}
