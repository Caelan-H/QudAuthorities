using System;
using XRL.Core;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Cudgel_SmashingUp : Effect
{
	public Cudgel_SmashingUp()
	{
		base.DisplayName = "demolishing";
	}

	public Cudgel_SmashingUp(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 67108992;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Slam has no cooldown.\n100% chance to daze with cudgels.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.GetPart("Cudgel_Slam") is Cudgel_Slam cudgel_Slam)
		{
			cudgel_Slam.CooldownMyActivatedAbility(cudgel_Slam.ActivatedAbilityID, 50);
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 45 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "!";
				E.ColorString = "&R";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (base.Duration > 0)
			{
				base.Duration--;
			}
			if (base.Duration > 0 && base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(base.Duration.Things("turn remains", "turns remain") + " until you stop demolishing.");
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
