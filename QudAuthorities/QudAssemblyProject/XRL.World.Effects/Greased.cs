using System;

namespace XRL.World.Effects;

[Serializable]
public class Greased : Effect
{
	public Greased()
	{
		base.DisplayName = "Greased";
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		return 16777248;
	}

	public override string GetDescription()
	{
		return "greased";
	}

	public override string GetDetails()
	{
		return "Can walk over webs without getting stuck.";
	}
}
