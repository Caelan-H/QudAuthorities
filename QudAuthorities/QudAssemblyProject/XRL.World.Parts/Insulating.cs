using System;

namespace XRL.World.Parts;

[Serializable]
public class Insulating : IPart
{
	public float Amount = 0.9f;

	public override bool SameAs(IPart p)
	{
		if ((p as Insulating).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "GetShortDescription");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTemperatureChange")
		{
			E.SetParameter("Amount", (int)((float)E.GetIntParameter("Amount") * Amount));
		}
		else if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Cold") || damage.HasAttribute("Ice"))
			{
				damage.Amount = (int)((float)damage.Amount * Amount);
				if (damage.Amount <= 0)
				{
					return false;
				}
			}
		}
		else if (E.ID == "GetShortDescription")
		{
			if (Amount != 1f)
			{
				string text = (int)((1f - Amount) * 100f) + "%";
				string text2 = "&CSeverity of cooling effects reduced by " + text + ". Cold damage reduced by " + text + ".";
				E.SetParameter("Postfix", E.GetStringParameter("Postfix") + "\n" + text2);
			}
		}
		else if (E.ID == "Equipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			gameObjectParameter.RegisterPartEvent(this, "BeforeTemperatureChange");
			gameObjectParameter.RegisterPartEvent(this, "BeforeApplyDamage");
		}
		else if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("UnequippingObject");
			gameObjectParameter2.UnregisterPartEvent(this, "BeforeTemperatureChange");
			gameObjectParameter2.UnregisterPartEvent(this, "BeforeApplyDamage");
		}
		return base.FireEvent(E);
	}
}
