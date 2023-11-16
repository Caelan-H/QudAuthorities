using System;

namespace XRL.World.Effects;

[Serializable]
public class Enlarged : Effect
{
	public Enlarged()
	{
		base.DisplayName = "enlarged";
	}

	public Enlarged(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 1024;
	}
}
