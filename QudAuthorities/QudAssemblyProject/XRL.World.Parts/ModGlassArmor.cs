using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ModGlassArmor : IModification
{
	public string Type = "glass";

	public ModGlassArmor()
	{
	}

	public ModGlassArmor(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnWearer = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart("Armor");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Reflects " + Tier + "% damage back at your attackers, rounded up.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("glass", 10);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.Amount > 0 && !damage.HasAttribute("reflected") && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
				GameObject equipped = ParentObject.Equipped;
				int num = (int)Math.Ceiling((float)damage.Amount * (float)Tier / 100f);
				if (num > 0 && gameObjectParameter != null && gameObjectParameter != ParentObject && gameObjectParameter != equipped)
				{
					Event @event = new Event("TakeDamage");
					Damage damage2 = new Damage(num);
					damage2.Attributes = new List<string>(damage.Attributes);
					if (!damage2.HasAttribute("reflected"))
					{
						damage2.AddAttribute("reflected");
					}
					@event.SetParameter("Damage", damage2);
					@event.SetParameter("Owner", equipped ?? ParentObject);
					@event.SetParameter("Attacker", equipped ?? ParentObject);
					@event.SetParameter("Source", ParentObject);
					@event.SetParameter("Message", "from %t " + Type + " armor!");
					if (equipped != null && equipped.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("reflect") + " " + num + " damage back at " + gameObjectParameter.the + gameObjectParameter.ShortDisplayName + ".");
					}
					gameObjectParameter.FireEvent(@event);
					ParentObject.Equipped?.FireEvent("ReflectedDamage");
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
