using System;

namespace XRL.World.Parts;

[Serializable]
public class AcidImmunity : IPart
{
	public int CloneCooldown;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Acid"))
			{
				damage.Amount = 0;
				return false;
			}
			return true;
		}
		return true;
	}
}
