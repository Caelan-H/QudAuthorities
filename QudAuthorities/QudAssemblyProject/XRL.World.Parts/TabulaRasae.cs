using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;

namespace XRL.World.Parts;

[Serializable]
public class TabulaRasae : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "TookDamage");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "CanApplyBeguile");
		Object.RegisterPartEvent(this, "CanApplyConfusion");
		Object.RegisterPartEvent(this, "CanApplyDomination");
		Object.RegisterPartEvent(this, "CanApplyEffect");
		Object.RegisterPartEvent(this, "CanApplyFear");
		Object.RegisterPartEvent(this, "CanApplyInvoluntarySleep");
		Object.RegisterPartEvent(this, "CanApplyShamed");
		Object.RegisterPartEvent(this, "ApplyEffect");
		Object.RegisterPartEvent(this, "ApplyBeguile");
		Object.RegisterPartEvent(this, "ApplyConfusion");
		Object.RegisterPartEvent(this, "ApplyDomination");
		Object.RegisterPartEvent(this, "ApplyFear");
		Object.RegisterPartEvent(this, "ApplyInvoluntarySleep");
		Object.RegisterPartEvent(this, "ApplyShamed");
		base.Register(Object);
	}

	public List<string> GetDamageImmunities()
	{
		if (!XRLCore.Core.Game.HasGameState("TabulaRasaeDamageImmunities"))
		{
			XRLCore.Core.Game.SetObjectGameState("TabulaRasaeDamageImmunities", new List<string>());
		}
		return XRLCore.Core.Game.GetObjectGameState("TabulaRasaeDamageImmunities") as List<string>;
	}

	public override bool FireEvent(Event E)
	{
		if (!(E.ID == "ApplyBeguile") && !(E.ID == "ApplyConfusion") && !(E.ID == "ApplyDomination") && !(E.ID == "ApplyFear") && !(E.ID == "ApplyShamed") && !(E.ID == "CanApplyBeguile") && !(E.ID == "CanApplyConfusion") && !(E.ID == "CanApplyDomination") && !(E.ID == "CanApplyFear"))
		{
			_ = E.ID == "CanApplyShamed";
		}
		if (E.ID == "TookDamage")
		{
			if (ParentObject.hitpoints <= 0)
			{
				List<string> damageImmunities = GetDamageImmunities();
				if (E.GetParameter("Damage") is Damage damage)
				{
					foreach (string attribute in damage.Attributes)
					{
						if (!damageImmunities.Contains(attribute))
						{
							damageImmunities.Add(attribute);
							MessageQueue.AddPlayerMessage("The Tabula Rasae adapt to " + attribute.ToLower() + " damage.");
						}
					}
				}
			}
			return true;
		}
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage2 = E.GetParameter("Damage") as Damage;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
			if (damage2.HasAnyAttribute(GetDamageImmunities()))
			{
				if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("Your attack does not affect " + ParentObject.DisplayName + ".");
				}
				damage2.Amount = 0;
				return false;
			}
			return true;
		}
		if (!(E.ID == "ApplyEffect"))
		{
			_ = E.ID == "CanApplyEffect";
		}
		return base.FireEvent(E);
	}
}
