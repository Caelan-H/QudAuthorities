using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class EyelessKingCrabHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", null, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		if (text.Contains("many-legged"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new MultipleLegs(), 5);
		}
		if (text.Contains("one-clawed"))
		{
			GO.Body?.GetPart("Hand")?.GetRandomElement()?.Dismember()?.Destroy();
		}
		if (text.Contains("massive"))
		{
			GO.GetStat("Hitpoints").BaseValue += Stat.Random(10, 20);
			GO.GetStat("MoveSpeed").BaseValue += Stat.Random(10, 20);
		}
		if (text.Contains("echoing"))
		{
			GO.GetStat("Ego").BaseValue += Stat.Random(2, 6);
		}
		if (text.Contains("frenetic"))
		{
			GO.GetStat("Speed").BaseValue += Stat.Random(15, 30);
		}
		if (text.Contains("shell-cracked"))
		{
			GO.GetStat("AV").BaseValue--;
			GO.GetStat("Hitpoints").BaseValue += Stat.Random(20, 30);
		}
		if (text.Contains("Skuttler"))
		{
			GO.GetStat("MoveSpeed").BaseValue -= Stat.Random(10, 20);
		}
		if (text.Contains("Ancient"))
		{
			GO.GetStat("AV").BaseValue += 2;
		}
		if (text.Contains("Deepcrawler"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new BurrowingClaws(), 5);
		}
		if (text.Contains("Goliath"))
		{
			GO.GetStat("AV").BaseValue++;
			GO.GetStat("MoveSpeed").BaseValue += Stat.Random(10, 20);
			GO.GetStat("Hitpoints").BaseValue += Stat.Random(30, 50);
		}
		if (text.Contains("Lord") || text.Contains("Patriarch"))
		{
			GO.GetStat("Strength").BaseValue += Stat.Random(2, 6);
			GO.GetStat("Hitpoints").BaseValue += Stat.Random(10, 20);
			GO.GetStat("Ego").BaseValue += Stat.Random(2, 6);
			GO.GetStat("Willpower").BaseValue += Stat.Random(2, 6);
		}
		GO.TakeObjectFromPopulation("Melee Weapons 2", null, Silent: false, 0, 50, 0, Context);
		GO.TakeObjectFromPopulation("Armor 2", null, Silent: false, 0, 50, 0, Context);
		GO.TakeObjectFromPopulation("Junk 4R", null, Silent: false, 0, 0, 0, Context);
		GO.TakeObjectFromPopulation("Junk 5", null, Silent: false, 0, 0, 0, Context);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		return true;
	}
}
