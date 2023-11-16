using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_BeaconFire : BaseSkill
{
	public static bool MeetsCriteria(GameObject GO)
	{
		if (!GameObject.validate(ref GO))
		{
			return false;
		}
		if (!GO.IsVisible())
		{
			return false;
		}
		if (GO.IsAflame())
		{
			return true;
		}
		if (GO.HasEffect("Luminous"))
		{
			return true;
		}
		if (GO.HasPart("Raycat"))
		{
			return true;
		}
		if (GO.HasEffect("LiquidCovered"))
		{
			List<Effect> effects = GO.GetEffects("LiquidCovered");
			for (int i = 0; i < effects.Count; i++)
			{
				if (!(effects[i] is LiquidCovered liquidCovered) || liquidCovered.Liquid == null)
				{
					continue;
				}
				foreach (string key in liquidCovered.Liquid.ComponentLiquids.Keys)
				{
					if (LiquidVolume.getLiquid(key).Glows)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
