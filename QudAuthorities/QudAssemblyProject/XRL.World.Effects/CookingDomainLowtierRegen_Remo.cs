using System;
using System.Collections.Generic;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLowtierRegen_RemoveDebuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "one of @their negative status effects is removed at random.";
	}

	public override string GetNotification()
	{
		return "@they feel less afflicted.";
	}

	public override void Apply(GameObject go)
	{
		List<Effect> effects = go.GetEffects((Effect FX) => FX.IsOfTypes(100663296) && !FX.IsOfType(134217728));
		if (effects.Count > 0)
		{
			go.RemoveEffect(effects.GetRandomElement());
		}
	}
}
