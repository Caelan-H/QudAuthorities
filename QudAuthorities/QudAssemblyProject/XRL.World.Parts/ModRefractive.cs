using System;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModRefractive : IModification
{
	public ModRefractive()
	{
	}

	public ModRefractive(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart("Armor") || Object.HasPart("Shield"))
		{
			return !Object.HasPart("RefractLight");
		}
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "RefractLight");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "RefractLight");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{refractive|refractive}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("glass", 2);
		E.Add("stars", 2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "RefractLight");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RefractLight" && GetRefractChance().in100())
		{
			E.SetParameter("By", ParentObject);
			float num = (float)E.GetParameter("Angle");
			E.SetParameter("Direction", (int)num + 180 + 90 - Stat.Random(0, 180));
			return false;
		}
		return base.FireEvent(E);
	}

	public int GetRefractChance()
	{
		if (ParentObject.GetPart("Armor") is Armor armor)
		{
			if (armor.WornOn == "Body")
			{
				return 40;
			}
			if (armor.WornOn == "Back")
			{
				return 40;
			}
			if (armor.WornOn == "Arm")
			{
				return 20;
			}
			return 10;
		}
		if (ParentObject.GetPart("Shield") is Shield shield)
		{
			if (shield.WornOn == "Hand")
			{
				return 30;
			}
			return 10;
		}
		return 0;
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Refractive: This item has a chance to refract light-based attacks.";
	}

	public string GetInstanceDescription()
	{
		return "Refractive: This item has " + Grammar.AOrAnBeforeNumber(GetRefractChance()) + " " + GetRefractChance() + "% chance to refract light-based attacks.";
	}
}
