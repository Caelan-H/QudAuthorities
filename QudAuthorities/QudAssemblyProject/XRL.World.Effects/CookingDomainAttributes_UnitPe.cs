using System;
using Qud.API;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAttributes_UnitPermanentAllStats_25Percent : ProceduralCookingEffectUnit
{
	public const string RANDOM_SEED = "CookingDomainAttributes_UnitPermanentAllStats_25Percent";

	public int Bonus = 1;

	public bool bSucceed;

	public override string GetDescription()
	{
		if (!bSucceed)
		{
			return "Nothing happened.";
		}
		return "+" + Bonus + " to all six attributes permanently";
	}

	public override string GetTemplatedDescription()
	{
		return "25% chance to gain +1 Strength, Agility, Toughness, Intelligence, Willpower, and Ego permanently.";
	}

	public override void Init(GameObject target)
	{
		target.WithSeededRandom(delegate(Random rng)
		{
			bSucceed = 25.in100(rng);
		}, "CookingDomainAttributes_UnitPermanentAllStats_25Percent");
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.PermuteRandomMutationBuys();
		if (bSucceed)
		{
			Object.GetStat("Strength").BaseValue += Bonus;
			Object.GetStat("Agility").BaseValue += Bonus;
			Object.GetStat("Toughness").BaseValue += Bonus;
			Object.GetStat("Intelligence").BaseValue += Bonus;
			Object.GetStat("Willpower").BaseValue += Bonus;
			Object.GetStat("Ego").BaseValue += Bonus;
			if (Object.IsPlayer())
			{
				JournalAPI.AddAccomplishment("You rejoiced in a drop of nectar.", "=name= the Eater supped on royal nectar and metamorphosed into Godhead.", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Low, null, -1L);
			}
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
