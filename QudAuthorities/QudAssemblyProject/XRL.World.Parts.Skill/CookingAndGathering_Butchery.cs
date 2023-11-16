using System;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_Butchery : BaseSkill
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
		if ((!E.Want || E.FromAdjacent == null) && IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			Butcherable butcherable = E.Item.GetPart("Butcherable") as Butcherable;
			CyberneticsButcherableCybernetic cyberneticsButcherableCybernetic = E.Item.GetPart("CyberneticsButcherableCybernetic") as CyberneticsButcherableCybernetic;
			if (((butcherable != null && butcherable.IsButcherable()) || (cyberneticsButcherableCybernetic != null && cyberneticsButcherableCybernetic.IsButcherable())) && !E.Item.IsImportant() && Options.AutoexploreAutopickups)
			{
				E.Want = true;
				E.FromAdjacent = "Butcher";
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandButcherToggle");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandButcherToggle")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Butcher Corpses", "CommandButcherToggle", "Skill", "Toggle to enable or disable the butchering of corpses", "b", null, Toggleable: true, DefaultToggleState: true);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
