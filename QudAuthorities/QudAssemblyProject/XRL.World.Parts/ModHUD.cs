using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModHUD : IModification
{
	public ModHUD()
	{
	}

	public ModHUD(int Tier)
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
		if (armor.WornOn != "Head" && armor.WornOn != "Face" && armor.WornOn != "*")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Head") && !usesSlots.Contains("Face"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<HUD>();
		Object.RequirePart<EnergyCellSocket>();
		int num = Math.Max((Tier >= 5) ? (8 - Tier) : (Tier + 1), 2);
		BootSequence part = Object.GetPart<BootSequence>();
		if (part == null)
		{
			part = new BootSequence();
			part.BootTime = num;
			part.ReadoutInName = true;
			part.ReadoutInDescription = true;
			Object.AddPart(part);
		}
		else if (part.BootTime < num)
		{
			part.BootTime = num;
		}
		IncreaseDifficultyAndComplexity(3, 2);
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
			E.AddAdjective("HUD");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier), base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "HUD: When powered and started up, this item can be used with a smartgun to enable its bonuses.";
	}
}
