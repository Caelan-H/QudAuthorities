using System;

namespace XRL.World.Effects;

[Serializable]
public class Overburdened : Effect
{
	public Overburdened()
	{
		base.DisplayName = "{{K|overburdened}}";
		base.Duration = 1;
	}

	public override string GetDetails()
	{
		return "Unable to move.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Overburdened"))
		{
			return false;
		}
		if (Object.IsPlayer() || Visible())
		{
			Object.ParticleText("*overburdened*", 'K');
		}
		return true;
	}

	public override int GetEffectType()
	{
		return 33554560;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "IsMobile");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" && base.Duration > 0 && !base.Object.IsTryingToJoinPartyLeader())
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
