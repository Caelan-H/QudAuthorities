using System;

namespace XRL.World.Effects;

[Serializable]
public class Inspired : Effect
{
	public Inspired()
	{
		base.Duration = 1;
		base.DisplayName = "{{M|inspired}}";
	}

	public Inspired(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "The next time you cook a meal by choosing ingredients, you get a choice of three dynamically-generated effects to apply. You create a recipe for the chosen effect.";
	}
}
