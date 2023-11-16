using System;
using Qud.API;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainDensity_UnitPermanentAV : ProceduralCookingEffectUnit
{
	public int Bonus = 1;

	public bool Collapse;

	public override string GetDescription()
	{
		if (Collapse)
		{
			return "{{R|CRITICAL GRAVITATIONAL COLLAPSE}}";
		}
		return "+" + Bonus + " AV permanently";
	}

	public override string GetTemplatedDescription()
	{
		return "+1 AV permanently. Small chance of gravitational collapse.";
	}

	public override void Init(GameObject target)
	{
		Collapse = 10.in100();
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Collapse)
		{
			Object.Explode(15000, null, "10d10+250", 1f, Neutron: true);
			return;
		}
		Object.GetStat("AV").BaseValue += Bonus;
		if (Object.IsPlayer())
		{
			JournalAPI.AddAccomplishment("You became denser.", "On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", =name= uncorked the cosmic wine and imbibed the forbidden starjuice. O, the mass within!", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			AchievementManager.SetAchievement("ACH_COOKED_FLUX");
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
