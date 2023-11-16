using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class HelpingHands : IPart
{
	public string ManagerID => ParentObject.id + "::HelpingHands";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsWorn())
		{
			E.Actor.RegisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
			E.Actor.RegisterPartEvent(this, "Dismember");
			AddArms(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
		E.Actor.UnregisterPartEvent(this, "Dismember");
		RemoveArms(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Dismember");
		base.Register(Object);
	}

	public void AddArms(GameObject Wearer = null)
	{
		if (Wearer == null)
		{
			Wearer = ParentObject.Equipped;
			if (Wearer == null)
			{
				return;
			}
		}
		Body body = Wearer.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			BodyPart bodyPart = body2.AddPartAt("Robo-Arm", 2, null, null, null, null, Extrinsic: true, Manager: ManagerID, Category: null, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Arm", OrInsertBefore: new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" });
			bodyPart.AddPart("Robo-Hand", 2, null, "Robo-Hands", null, null, Extrinsic: true, Manager: ManagerID);
			body2.AddPartAt(bodyPart, "Robo-Arm", 1, null, null, null, null, Extrinsic: true, Manager: ManagerID).AddPart("Robo-Hand", 1, null, "Robo-Hands", null, null, Extrinsic: true, Manager: ManagerID);
			body2.AddPartAt("Robo-Hands", 0, null, null, "Robo-Hands", null, Extrinsic: true, Manager: ManagerID, Category: null, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Hands", OrInsertBefore: new string[3] { "Feet", "Roots", "Thrown Weapon" });
		}
	}

	public void RemoveArms(GameObject Wearer = null)
	{
		if (Wearer == null)
		{
			Wearer = ParentObject.Equipped;
			if (Wearer == null)
			{
				return;
			}
		}
		Wearer.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Dismember")
		{
			if (E.GetParameter("Part") is BodyPart bodyPart && bodyPart.Manager != null && bodyPart.Manager == ManagerID)
			{
				ParentObject.ApplyEffect(new Broken());
				return false;
			}
		}
		else if (E.ID == "AttackerQueryWeaponSecondaryAttackChance" && E.GetParameter("Part") is BodyPart bodyPart2 && bodyPart2.Manager != null && bodyPart2.Manager == ManagerID)
		{
			E.SetParameter("Chance", 8);
		}
		return base.FireEvent(E);
	}
}
