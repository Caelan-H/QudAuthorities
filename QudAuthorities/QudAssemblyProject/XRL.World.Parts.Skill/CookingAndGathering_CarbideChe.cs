using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_CarbideChef : BaseSkill
{
	public void Inspire()
	{
		if (!ParentObject.HasEffect("Inspired"))
		{
			Popup.Show("You swell with inspiration to cook a mouthwatering meal.");
			ParentObject.ApplyEffect(new Inspired(2400));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLevelGained")
		{
			if (ParentObject.IsPlayer())
			{
				Inspire();
			}
		}
		else if (E.ID == "VisitingNewZone" && ParentObject.IsPlayer() && 5.in100())
		{
			Inspire();
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.RegisterPartEvent(this, "VisitingNewZone");
		GO.RegisterPartEvent(this, "AfterLevelGained");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		GO.UnregisterPartEvent(this, "VisitingNewZone");
		GO.UnregisterPartEvent(this, "AfterLevelGained");
		return base.AddSkill(GO);
	}
}
