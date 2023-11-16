using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SpringBoots : IPart
{
	public int Timer;

	public override bool SameAs(IPart p)
	{
		if ((p as SpringBoots).Timer != Timer)
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
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Timer > 0)
			{
				Timer--;
			}
			if (Timer == 0)
			{
				Timer--;
				if (ParentObject.pPhysics != null)
				{
					GameObject equipped = ParentObject.pPhysics.Equipped;
					if (equipped != null)
					{
						equipped.ApplyEffect(new Springing(ParentObject));
						equipped.UnregisterPartEvent(this, "BeginTakeAction");
					}
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "BeginTakeAction");
			Timer = 100;
		}
		else if (E.ID == "Unequipped")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("UnequippingObject");
			gameObjectParameter.UnregisterPartEvent(this, "BeginTakeAction");
			Timer = 0;
			Effect effect = gameObjectParameter.GetEffect("Springing", (Effect Effect) => (Effect as Springing).Source == ParentObject);
			if (effect != null)
			{
				gameObjectParameter.RemoveEffect(effect);
			}
		}
		return base.FireEvent(E);
	}
}
