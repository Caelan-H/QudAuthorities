using System;

namespace XRL.World.Effects;

[Serializable]
public class Springing : Effect
{
	public GameObject Source;

	public Springing()
	{
		base.DisplayName = "springing";
		base.Duration = 1;
	}

	public Springing(GameObject Source)
		: this()
	{
		this.Source = Source;
	}

	public override int GetEffectType()
	{
		return 16777344;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "springing";
	}

	public override string GetDetails()
	{
		return "Sprints at thrice move speed.";
	}
}
