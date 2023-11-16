using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModTwoFaced : IModification
{
	public int ExtraFaceID;

	public ModTwoFaced()
	{
	}

	public ModTwoFaced(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (armor.WornOn != "Head")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Head"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyIfComplex(1);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModTwoFaced).ExtraFaceID != ExtraFaceID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "Dismember");
		AddFace(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		RemoveFace(E.Actor);
		E.Actor.UnregisterPartEvent(this, "Dismember");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("two-faced");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Dismember" && E.GetParameter("Part") is BodyPart bodyPart && bodyPart.idMatch(ExtraFaceID))
		{
			ParentObject.ApplyEffect(new Broken());
			return false;
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Two-faced: This item grants an additional face slot.";
	}

	public void AddFace(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Equipped;
			if (who == null)
			{
				return;
			}
		}
		BodyPart bodyPart = ParentObject.EquippedOn();
		if (bodyPart != null)
		{
			BodyPart bodyPart2 = bodyPart.AddPartAt("Extra Face", 0, null, null, null, null, null, BodyPartCategory.BestGuessForCategoryDerivedFromGameObject(ParentObject), null, null, null, null, null, null, true, null, null, null, null, "Face", new string[2] { "Fungal Outcrop", "Icy Outcrop" });
			ExtraFaceID = bodyPart2.ID;
		}
	}

	public void RemoveFace(GameObject who = null)
	{
		if (ExtraFaceID == 0)
		{
			return;
		}
		if (who == null)
		{
			who = ParentObject.pPhysics.Equipped;
			if (who == null)
			{
				return;
			}
		}
		who.Body?.RemovePartByID(ExtraFaceID);
		ExtraFaceID = 0;
	}
}
