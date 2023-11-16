using System;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_Harvestry : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if ((!E.Want || E.FromAdjacent == null) && IsMyActivatedAbilityToggledOn(ActivatedAbilityID) && E.Item.GetPart("Harvestable") is Harvestable harvestable && harvestable.IsHarvestable() && !E.Item.IsImportant() && Options.AutoexploreAutopickups)
		{
			E.Want = true;
			E.FromAdjacent = "Harvest";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandHarvestToggle");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandHarvestToggle")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Harvest Plants", "CommandHarvestToggle", "Skill", "Toggle to enable or disable the harvesting of plants", "h", null, Toggleable: true, DefaultToggleState: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
