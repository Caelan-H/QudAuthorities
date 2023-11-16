using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSharp : IModification
{
	public ModSharp()
	{
	}

	public ModSharp(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "EdgeEnhancement";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return false;
		}
		if (part.Skill != "LongBlades" && part.Skill != "ShortBlades" && part.Skill != "Axe")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<MeleeWeapon>().PenBonus++;
		IncreaseDifficultyIfComplex(1);
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
			E.AddAdjective("sharp", -20);
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
		return "Sharp: +1 penetration";
	}
}
