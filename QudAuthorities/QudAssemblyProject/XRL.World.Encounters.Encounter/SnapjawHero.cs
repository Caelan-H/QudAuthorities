using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public static class SnapjawHero
{
	public static void ApplySnapjawTraits(GameObject GO, string name, string Context = null)
	{
		if (name.Contains("fleet-footed"))
		{
			GO.Statistics["Speed"].BaseValue += Stat.Random(25, 50);
		}
		if (name.Contains("learned"))
		{
			GO.Statistics["Intelligence"].BaseValue += Stat.Random(2, 6);
		}
		if (name.Contains("stalwart"))
		{
			GO.Statistics["Hitpoints"].BaseValue += Stat.Random(10, 20);
		}
		if (name.Contains("fearsome"))
		{
			GO.Statistics["Ego"].BaseValue += Stat.Random(2, 6);
		}
		if (name.Contains("nimble"))
		{
			GO.Statistics["DV"].BaseValue += Stat.Random(3, 6);
		}
		if (name.Contains("hulking"))
		{
			GO.Statistics["Strength"].BaseValue += Stat.Random(2, 6);
		}
		if (name.Contains("calloused"))
		{
			GO.Statistics["AV"].BaseValue++;
		}
		if (name.Contains("Skullsplitter"))
		{
			GO.TakeObject(GameObject.create("Steel Battle Axe", 0, 0, Context), Silent: false, 0);
			XRL.World.Parts.Skills obj = GO.GetPart("Skills") as XRL.World.Parts.Skills;
			obj.AddSkill(new Axe());
			obj.AddSkill(new Axe_Dismember());
		}
		if (name.Contains("Firesnarler"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new Pyrokinesis(), 2);
		}
		if (name.Contains("Bear-baiter"))
		{
			GO.Statistics["DV"].BaseValue++;
		}
		if (name.Contains("Tot-eater"))
		{
			GO.Statistics["Agility"].BaseValue += Stat.Random(2, 6);
		}
		if (name.Contains("Gutspiller"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new Horns(), 2);
		}
		if (name.Contains("King"))
		{
			GO.Statistics["Strength"].BaseValue += Stat.Random(2, 6);
			GO.Statistics["Hitpoints"].BaseValue += Stat.Random(10, 20);
			GO.Statistics["Ego"].BaseValue += Stat.Random(2, 6);
			GO.Statistics["Willpower"].BaseValue += Stat.Random(2, 6);
		}
	}
}
