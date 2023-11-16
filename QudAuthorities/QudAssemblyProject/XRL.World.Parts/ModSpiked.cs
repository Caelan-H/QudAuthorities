using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSpiked : IModification
{
	public ModSpiked()
	{
	}

	public ModSpiked(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "PenetrationModule";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart("Shield"))
		{
			return true;
		}
		if (Object.GetPart("Armor") is Armor armor)
		{
			if (armor.WornOn != "Hands")
			{
				return false;
			}
			string usesSlots = Object.UsesSlots;
			if (!string.IsNullOrEmpty(usesSlots) && !usesSlots.Contains("Hands"))
			{
				return false;
			}
			if (Object.HasPart("BleedingOnHit"))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (!Object.HasPart("Shield"))
		{
			BleedingOnHit bleedingOnHit = new BleedingOnHit();
			bleedingOnHit.Amount = "1d3";
			bleedingOnHit.SaveTarget = 20 + Tier * 2;
			bleedingOnHit.SelfOnly = false;
			bleedingOnHit.RequireDamageAttribute = "Unarmed";
			bleedingOnHit.Stack = true;
			Object.AddPart(bleedingOnHit);
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
			E.AddAdjective("{{spiked|spiked}}", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Spiked: This item causes additional bleeding damage.";
	}

	public static string GetDescription(int Tier, GameObject obj)
	{
		if (obj.HasPart("Shield"))
		{
			return "Spiked: This item adds bonus damage to Shield Slam equal to your Strength modifier and causes your target to bleed.";
		}
		return "Spiked: Unarmed attacks performed while this item is equipped cause bleeding.";
	}

	public string GetInstanceDescription()
	{
		return GetDescription(Tier, ParentObject);
	}
}
