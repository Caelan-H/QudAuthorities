using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class TrollHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", null, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		GO.TakeObjectFromPopulation("Armor 2", null, Silent: false, 0, 50, 0, Context);
		if (text.Contains("frenetic"))
		{
			GO.Statistics["Speed"].BaseValue += Stat.Random(25, 50);
		}
		if (text.Contains("learned"))
		{
			GO.Statistics["Intelligence"].BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("everlasting"))
		{
			GO.Statistics["Hitpoints"].BaseValue += Stat.Random(40, 60);
		}
		if (text.Contains("bestial"))
		{
			GO.Statistics["Ego"].BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("bloodthirsty"))
		{
			GO.Statistics["Agility"].BaseValue += Stat.Random(3, 6);
		}
		if (text.Contains("hulking"))
		{
			GO.Statistics["Strength"].BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("rubberhide"))
		{
			GO.Statistics["AV"].BaseValue += 4;
		}
		if (text.Contains("Skull-collector"))
		{
			GO.TakeObject(GameObject.create("Battle Axe4", 0, 0, Context), Silent: false, 0);
			(GO.GetPart("Skills") as XRL.World.Parts.Skills).AddSkill(new Axe_Dismember());
		}
		if (text.Contains("Heart-eater"))
		{
			GO.Statistics["Strength"].BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("Hunt-master"))
		{
			GO.Statistics["DV"].BaseValue += 4;
		}
		if (text.Contains("Man-eater"))
		{
			GO.Statistics["Agility"].BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("two-headed"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new TwoHeaded(), 2);
		}
		if (text.Contains("Caveking"))
		{
			GO.Statistics["Strength"].BaseValue += Stat.Random(2, 6);
			GO.Statistics["Hitpoints"].BaseValue += Stat.Random(10, 20);
			GO.Statistics["Ego"].BaseValue += Stat.Random(2, 6);
			GO.Statistics["Willpower"].BaseValue += Stat.Random(2, 6);
		}
		GO.TakeObjectFromPopulation("Junk 4R", null, Silent: false, 0, 0, 0, Context);
		GO.TakeObjectFromPopulation("Junk 4", null, Silent: false, 0, 0, 0, Context);
		return true;
	}
}
