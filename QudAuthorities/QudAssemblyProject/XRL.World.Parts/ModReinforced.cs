using System;

namespace XRL.World.Parts;

[Serializable]
public class ModReinforced : IModification
{
	public ModReinforced()
	{
	}

	public ModReinforced(int Tier)
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
		if (armor.WornOn != "Body" && armor.WornOn != "Back" && armor.WornOn != "*")
		{
			return false;
		}
		string usesSlots = Object.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Body") && !usesSlots.Contains("Back"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<Armor>().AV++;
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("reinforced");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Reinforced: +1 AV";
	}
}
