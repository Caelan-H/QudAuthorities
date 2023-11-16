using System;

namespace XRL.World.Parts;

[Serializable]
public class ModGesticulating : IModification
{
	public ModGesticulating()
	{
	}

	public ModGesticulating(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.GetPart("Armor") is Armor armor && armor.WornOn == "Floating Nearby")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Armor armor = Object.GetPart("Armor") as Armor;
		if (Object.UsesSlots == null)
		{
			string text = ((armor == null || !(armor.WornOn != "*")) ? "Hand" : armor.WornOn);
			Object.UsesSlots = text + ",Floating Nearby";
		}
		else
		{
			Object.UsesSlots += ",Floating Nearby";
		}
		if (armor != null)
		{
			armor.Strength += 2;
		}
		else
		{
			EquipStatBoost.AppendBoostOnEquip(Object, "Strength:2");
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
			E.AddAdjective("{{m|gesticulating}}");
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
		return "Gesticulating: This item grants bonus Strength but disallows the use of the Floating Nearby equipment slot.";
	}
}
