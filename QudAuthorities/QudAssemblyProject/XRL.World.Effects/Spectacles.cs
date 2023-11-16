using System;

namespace XRL.World.Effects;

[Serializable]
public class Spectacles : Effect
{
	public Spectacles()
	{
		base.DisplayName = "corrected vision";
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		return 16777344;
	}

	public override string GetDescription()
	{
		if (base.Object.GetEffect("Spectacles") != this)
		{
			return null;
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		return "Can see things at normal distances.";
	}
}
