using XRL.Names;
using XRL.Rules;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class ChromeIdolHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", null, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		GO.TakeObjectFromPopulation("Melee Weapons 2", null, Silent: false, 0, 50, 0, Context);
		GO.TakeObjectFromPopulation("Armor 2", null, Silent: false, 0, 50, 0, Context);
		if (GO.DisplayName.Contains("revered") || GO.DisplayName.Contains("venerated") || GO.DisplayName.Contains("terrible"))
		{
			GO.GetStat("Ego").BaseValue += Stat.Random(2, 6);
		}
		if (GO.DisplayName.Contains("ancient") || GO.DisplayName.Contains("corroded") || GO.DisplayName.Contains("rusted"))
		{
			GO.GetStat("Toughness").BaseValue += Stat.Random(2, 6);
		}
		if (GO.DisplayName.Contains("joyous"))
		{
			GO.GetStat("Willpower").BaseValue += Stat.Random(2, 6);
		}
		GO.TakeObjectFromPopulation("Junk 3R", null, Silent: false, 0, 0, 0, Context);
		GO.TakeObjectFromPopulation("Junk 3", null, Silent: false, 0, 0, 0, Context);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		return true;
	}
}
