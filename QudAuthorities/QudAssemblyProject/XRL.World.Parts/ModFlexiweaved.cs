using System;

namespace XRL.World.Parts;

[Serializable]
public class ModFlexiweaved : IModification
{
	public ModFlexiweaved()
	{
	}

	public ModFlexiweaved(int Tier)
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
		int num = armor.DV;
		if (Object.HasPart("ModVisored"))
		{
			num--;
		}
		if (num >= 0)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Armor armor = Object.GetPart("Armor") as Armor;
		armor.DV += GetModificationLevel();
		int num = (Object.HasPart("ModVisored") ? 1 : 0);
		if (armor.DV > num)
		{
			armor.DV = num;
		}
		IncreaseComplexity(1);
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
			E.AddAdjective("flexiweaved(" + GetModificationLevel() + ")");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static int GetModificationLevel(int Tier)
	{
		return (int)Math.Ceiling((float)Tier / 2f);
	}

	public int GetModificationLevel()
	{
		return GetModificationLevel(Tier);
	}

	public static string GetDescription(int Tier)
	{
		return "Flexiweaved: This item's DV penalty is reduced" + ((Tier > 0) ? (" by " + GetModificationLevel(Tier)) : "") + ".";
	}
}
