using System;

namespace XRL.World.Effects;

[Serializable]
public class Crackling : Effect
{
	public Crackling()
	{
		base.DisplayName = "{{W|crackling}}";
		base.Duration = 9999;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override string GetDescription()
	{
		return "{{W|crackling}}";
	}

	public override string GetDetails()
	{
		return "Electromagnetically unstable.";
	}
}
