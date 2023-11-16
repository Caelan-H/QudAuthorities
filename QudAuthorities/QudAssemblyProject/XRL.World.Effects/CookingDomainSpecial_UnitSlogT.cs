using System;
using Qud.API;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSpecial_UnitSlogTransform : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "@they became a slug permanently.";
	}

	public override string GetTemplatedDescription()
	{
		return "?????";
	}

	public override void Init(GameObject target)
	{
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		ApplyTo(Object);
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}

	public static void ApplyTo(GameObject Object)
	{
		if (Object.IsChimera())
		{
			if (Object.IsPlayer())
			{
				Popup.Show("Your stomach gurgles agreeably but you don't fully metabolize the meal.");
			}
			return;
		}
		if (Object.GetPropertyOrTag("AteCloacaSurprise") == "true")
		{
			if (Object.IsPlayer())
			{
				Popup.Show("Your genome has already undergone this transformation.");
			}
			return;
		}
		if (Object.IsPlayer())
		{
			Popup.Show("...");
			Popup.Show("You feel an uncomfortable pressure across the length of your body.");
			Popup.Show("Feelers rip through your scalp and shudder with curiosity.");
			Popup.Show("Your arms shrink into your torso.");
			Popup.Show("A bilge hose painted with mucus undulates out of your lower body. It spews the amniotic broth of its birth from its sputtering mouth.");
			JournalAPI.AddAccomplishment("You ate the Cloaca Surprise.", "Slugform! Slugform! On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", in the year " + Calendar.getYear() + " AR, =name= underwent the divine transformation and assumed the Slugform.", "general", JournalAccomplishment.MuralCategory.BodyExperienceNeutral, JournalAccomplishment.MuralWeight.VeryHigh, null, -1L);
			AchievementManager.SetAchievement("ACH_ATE_SURPRISE");
		}
		Object.Body.Rebuild("SlugWithHands");
		Object.GetPart<Mutations>().AddMutation((BaseMutation)Activator.CreateInstance(typeof(SlogGlands)), 1);
		Object.pRender.Tile = "Creatures/sw_slog_flipped.bmp";
		Object.SetStringProperty("AteCloacaSurprise", "true");
	}
}
