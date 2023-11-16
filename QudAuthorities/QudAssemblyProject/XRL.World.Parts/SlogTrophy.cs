using System;

namespace XRL.World.Parts;

[Serializable]
public class SlogTrophy : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Poison"))
			{
				damage.Amount /= 2;
				if (damage.Amount <= 0)
				{
					return false;
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "BeforeApplyDamage");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "BeforeApplyDamage");
		}
		return base.FireEvent(E);
	}
}
