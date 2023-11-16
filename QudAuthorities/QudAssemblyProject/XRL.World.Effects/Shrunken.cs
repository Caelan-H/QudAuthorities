using System;

namespace XRL.World.Effects;

[Serializable]
public class Shrunken : Effect
{
	public Shrunken()
	{
		base.DisplayName = "shrunken";
	}

	public Shrunken(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 1024;
	}
}
