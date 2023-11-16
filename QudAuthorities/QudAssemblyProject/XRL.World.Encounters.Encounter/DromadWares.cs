using System;
using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class DromadWares
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		switch (Math.Min(Math.Max(Stat.Roll("5d5") / 5, 1), 5))
		{
		case 1:
			new Tier1Wares().BuildObject(GO);
			break;
		case 2:
			new Tier2Wares().BuildObject(GO);
			break;
		case 3:
			new Tier3Wares().BuildObject(GO);
			break;
		case 4:
			new Tier4Wares().BuildObject(GO);
			break;
		case 5:
			new Tier5Wares().BuildObject(GO);
			break;
		case 6:
			new Tier6Wares().BuildObject(GO);
			break;
		case 7:
			new Tier7Wares().BuildObject(GO);
			break;
		case 8:
			new Tier8Wares().BuildObject(GO);
			break;
		}
		return true;
	}
}
