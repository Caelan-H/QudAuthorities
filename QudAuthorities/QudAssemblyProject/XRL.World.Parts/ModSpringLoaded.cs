using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModSpringLoaded : IModification
{
	public ModSpringLoaded()
	{
	}

	public ModSpringLoaded(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "MobilityAssist";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (armor.WornOn != "Feet" && armor.WornOn != "Back" && armor.WornOn != "*")
		{
			return false;
		}
		if (Object.pPhysics == null)
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Feet") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		int num = Stat.Random(-10, -5) + Tier - Object.GetTier();
		MoveCostMultiplier part = ParentObject.GetPart<MoveCostMultiplier>();
		if (part != null)
		{
			part.Amount += num;
		}
		else
		{
			ParentObject.AddPart(new MoveCostMultiplier(num));
		}
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("spring-loaded");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Spring-loaded: This item grants bonus move speed.";
	}
}
