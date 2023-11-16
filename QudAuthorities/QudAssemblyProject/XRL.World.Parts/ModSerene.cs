using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ModSerene : IModification
{
	public ModSerene()
	{
	}

	public ModSerene(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "SereneVisage";
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
		if (!string.IsNullOrEmpty(usesSlots) && usesSlots != armor.WornOn)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.pPhysics.UsesSlots = "Head,Face";
		Object.GetPart<Armor>().Willpower++;
		string propertyOrTag = Object.GetPropertyOrTag("Mods");
		if (!string.IsNullOrEmpty(propertyOrTag))
		{
			if (propertyOrTag != "None")
			{
				List<string> list = new List<string>(propertyOrTag.Split(','));
				if (!list.Contains("HeadwearMods"))
				{
					list.Add("HeadwearMods");
				}
				if (!list.Contains("MaskMods"))
				{
					list.Add("MaskMods");
				}
				Object.SetStringProperty("Mods", string.Join(",", list.ToArray()));
			}
		}
		else
		{
			Object.SetStringProperty("Mods", "HeadwearMods,MaskMods");
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.ModIntProperty("Serene", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.ModIntProperty("Serene", -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{Y|serene}} visage");
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
		return "Serene visage: This item grants bonus Willpower and reduces the cooldown of Meditate by 40 rounds.";
	}
}
