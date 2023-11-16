using System;

namespace XRL.World.Parts;

[Serializable]
public class UrbanCamouflage : ICamouflage
{
	public UrbanCamouflage()
	{
		base.EffectClass = "UrbanCamouflaged";
		Description = "Urban camouflage: This item grants the wearer +=level= DV in trash and debris.";
	}
}
