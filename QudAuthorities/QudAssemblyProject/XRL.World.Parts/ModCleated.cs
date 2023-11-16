using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ModCleated : IModification
{
	public const string SAVE_VS = "Move,Knockdown,Knockback,Restraint";

	public ModCleated()
	{
	}

	public ModCleated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "StabilityAssist";
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
		string usesSlots = Object.pPhysics.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Feet") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		if (Object.GetPart("SaveModifier") is SaveModifier saveModifier && saveModifier.Vs != "Move,Knockdown,Knockback,Restraint")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.GetPart("SaveModifier") is SaveModifier saveModifier)
		{
			saveModifier.Amount += GetSaveModifierAmount(Tier);
		}
		else
		{
			SaveModifier saveModifier2 = Object.AddPart<SaveModifier>();
			saveModifier2.Vs = "Move,Knockdown,Knockback,Restraint";
			saveModifier2.Amount = GetSaveModifierAmount(Tier);
			saveModifier2.ShowInShortDescription = false;
		}
		IncreaseDifficultyIfComplex(1);
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
			E.AddWithClause("cleats");
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
		return "Fitted with cleats: " + (SavingThrows.GetSaveBonusDescription(GetSaveModifierAmount(Tier), "Move,Knockdown,Knockback,Restraint") ?? "no effect");
	}

	public static int GetSaveModifierAmount(int Tier)
	{
		return 2 + Tier / 4;
	}
}
