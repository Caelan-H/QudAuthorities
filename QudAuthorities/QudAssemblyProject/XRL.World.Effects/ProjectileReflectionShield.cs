using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class ProjectileReflectionShield : Effect
{
	public int Chance = 100;

	public string RetroVariance = "0";

	public string Details = "";

	public string Verb = "reflect";

	public ProjectileReflectionShield()
	{
		base.DisplayName = "{{Y|reflectively shielded}}";
		base.Duration = 1;
		Chance = 100;
		Details = "Reflects incoming projectiles and thrown objects at a " + Chance + "% chance.";
	}

	public ProjectileReflectionShield(int Chance, string RetroVariance = "0", string Verb = "reflect")
		: this()
	{
		this.Chance = Chance;
		this.RetroVariance = RetroVariance;
		Details = "Reflects incoming projectiles and thrown objects at a " + Chance + "% chance.";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Details;
	}

	public override int GetEffectType()
	{
		return 64;
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ReflectProjectile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ReflectProjectile");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ReflectProjectile" && Chance.in100())
		{
			E.SetParameter("By", base.Object);
			if (!string.IsNullOrEmpty(RetroVariance))
			{
				float num = (float)E.GetParameter("Angle");
				E.SetParameter("Direction", (int)num + 180 + RetroVariance.RollCached());
			}
			if (!string.IsNullOrEmpty(Verb))
			{
				E.SetParameter("Verb", Verb);
			}
			return false;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 60)
			{
				E.RenderString = "\t";
				E.ColorString = "&Y";
			}
		}
		return true;
	}
}
