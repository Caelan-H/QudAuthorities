using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHightierRegen_RemoveDebuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "all of @their negative status effects are removed.";
	}

	public override string GetNotification()
	{
		return "@they feel much less afflicted.";
	}

	public override void Apply(GameObject go)
	{
		foreach (Effect effect in go.GetEffects((Effect FX) => FX.IsOfTypes(100663296) && !FX.IsOfType(134217728)))
		{
			go.RemoveEffect(effect);
		}
	}
}
