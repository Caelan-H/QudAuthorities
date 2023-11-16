using System;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsBaseItem : IPart
{
	public GameObject ImplantedOn;

	public string Slots;

	public int Cost;

	public string BehaviorDescription;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetContextEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID && ID != RemoveFromContextEvent.ID && ID != ReplaceInContextEvent.ID && ID != OnDestroyObjectEvent.ID && ID != TakenEvent.ID && ID != TryRemoveFromContextEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		ImplantedOn = E.Implantee;
		ParentObject.SetIntProperty("CannotEquip", 1);
		ParentObject.SetIntProperty("CannotDrop", 1);
		ParentObject.SetIntProperty("NoRemoveOptionInInventory", 1);
		if (GetCyberneticRejectionSyndromeChance(E.Implantee).in100())
		{
			string cyberneticRejectionSyndromeKey = GetCyberneticRejectionSyndromeKey(E.Implantee);
			if (!ParentObject.HasIntProperty(cyberneticRejectionSyndromeKey))
			{
				ParentObject.SetIntProperty(cyberneticRejectionSyndromeKey, 20.in100() ? 1 : 0);
			}
			if (ParentObject.GetIntProperty(cyberneticRejectionSyndromeKey) > 0)
			{
				E.Implantee.ForceApplyEffect(new CyberneticRejectionSyndrome(Cost));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		ClearImplantConfiguration();
		if (E.Implantee.IsMutant())
		{
			string cyberneticRejectionSyndromeKey = GetCyberneticRejectionSyndromeKey(E.Implantee);
			if (ParentObject.GetIntProperty(cyberneticRejectionSyndromeKey) > 0)
			{
				(E.Implantee.GetEffect("CyberneticRejectionSyndrome") as CyberneticRejectionSyndrome)?.Reduce(Cost);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.validate(ref ImplantedOn))
		{
			E.ObjectContext = ImplantedOn;
			E.Relation = 4;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.validate(ref ImplantedOn))
		{
			BodyPart bodyPart = ImplantedOn.FindCybernetics(ParentObject);
			if (bodyPart != null)
			{
				ParentObject.Unimplant();
				bodyPart.Implant(E.Replacement);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ParentObject.Unimplant();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.validate(ref ImplantedOn))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddAdjective("[{{W|Implant}}] -", 40);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription).Append('\n');
		}
		if (ParentObject != null && ParentObject.HasTag("CyberneticsDestroyOnRemoval"))
		{
			E.Postfix.AppendRules("Destroyed when uninstalled.");
		}
		if (!string.IsNullOrEmpty(Slots))
		{
			E.Postfix.AppendRules("Target body parts: " + Slots.Replace(",", ", "));
		}
		E.Postfix.AppendRules("License points: " + Cost).AppendRules("Only compatible with True Kin genotypes");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		ClearImplantConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		ClearImplantConfiguration();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ParentObject.Unimplant();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingUnequipped");
		Object.RegisterPartEvent(this, "CanBeTaken");
		Object.RegisterPartEvent(this, "CanBeUnequipped");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	private void ClearImplantConfiguration()
	{
		ImplantedOn = null;
		ParentObject.DeleteStringProperty("CannotEquip");
		ParentObject.DeleteStringProperty("CannotDrop");
		ParentObject.RemoveIntProperty("CannotEquip");
		ParentObject.RemoveIntProperty("CannotDrop");
		ParentObject.RemoveIntProperty("NoRemoveOptionInInventory");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeUnequipped" || E.ID == "CanBeTaken" || E.ID == "BeginBeingUnequipped")
		{
			if (ImplantedOn != null && E.GetIntParameter("Forced") < 1)
			{
				return false;
			}
		}
		else if (E.ID == "EnteredCell")
		{
			ClearImplantConfiguration();
		}
		return base.FireEvent(E);
	}

	public static string GetCyberneticRejectionSyndromeKey(GameObject Implantee)
	{
		return "CyberneticRejection" + Implantee.id;
	}

	public int GetCyberneticRejectionSyndromeChance(GameObject Implantee)
	{
		if (!Implantee.IsMutant())
		{
			return 0;
		}
		int num = 5 + Cost / 2;
		if (Implantee.GetPart("Mutations") is Mutations mutations && mutations.MutationList != null)
		{
			foreach (BaseMutation mutation in mutations.MutationList)
			{
				num = ((!mutation.IsPhysical() && !(Slots == "Head")) ? (num + 1) : (num + mutation.Level));
			}
		}
		return Math.Min(num, 80);
	}
}
