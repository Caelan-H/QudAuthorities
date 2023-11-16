using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMetallized : IModification
{
	public ModMetallized()
	{
	}

	public ModMetallized(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.pPhysics == null)
		{
			return false;
		}
		if (Object.HasPart("Metal"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.AddPart<Metal>();
		Armor part = Object.GetPart<Armor>();
		Shield part2 = Object.GetPart<Shield>();
		if (part == null && part2 == null)
		{
			MeleeWeapon part3 = Object.GetPart<MeleeWeapon>();
			if (part3 != null)
			{
				part3.PenBonus++;
			}
		}
		else
		{
			if (part != null)
			{
				part.AV++;
			}
			if (part2 != null)
			{
				part2.AV++;
			}
		}
		Object.SetIntProperty("Inorganic", 1);
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
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
			E.AddAdjective("{{c|metallized}}", 20);
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
		return "Metallized: +1 AV or penetration";
	}
}
