using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModNanochelated : IModification
{
	public ModNanochelated()
	{
	}

	public ModNanochelated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("Metal"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RemovePart("Metal");
		Armor armor = Object.GetPart("Armor") as Armor;
		Shield shield = Object.GetPart("Shield") as Shield;
		if (armor == null && shield == null)
		{
			if (Object.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
			{
				meleeWeapon.PenBonus--;
			}
		}
		else
		{
			if (armor != null)
			{
				if (armor.AV > 0)
				{
					armor.AV--;
				}
				int num = (ParentObject.HasPart("ModVisored") ? 1 : 0);
				if (armor.DV < num)
				{
					armor.DV++;
				}
			}
			if (shield != null)
			{
				if (shield.AV > 0)
				{
					shield.AV--;
				}
				if (shield.DV < 0)
				{
					shield.DV++;
				}
			}
		}
		Object.SetIntProperty("Inorganic", 0);
		IncreaseDifficultyAndComplexityIfComplex(2, 1);
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
			E.AddAdjective("{{K|nanochelated}}");
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
		return "Nanochelated: This item's metals have been replaced with carbon fiber. -1 AV or penetration, +1 DV if below zero";
	}
}
