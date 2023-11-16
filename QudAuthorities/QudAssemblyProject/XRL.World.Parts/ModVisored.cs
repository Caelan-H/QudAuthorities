using System;

namespace XRL.World.Parts;

[Serializable]
public class ModVisored : IModification
{
	public ModVisored()
	{
	}

	public ModVisored(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "Visor";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (armor.WornOn != "Head" && armor.WornOn != "Back" && armor.WornOn != "*")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Head") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<Armor>().DV++;
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
			E.AddAdjective("visored");
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
		return "Visored: +1 DV";
	}
}
