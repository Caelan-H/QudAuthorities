using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class BaboonHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		int num = Stat.Roll("1d10+2");
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", new Dictionary<string, string> { 
		{
			"*Rings*",
			num + "-ringed"
		} }, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		if (text.Contains("King") || text.Contains("Queen"))
		{
			num += "1d4".RollCached();
			Mutations obj = GO.GetPart("Mutations") as Mutations;
			obj.AddMutation(new MultipleArms(), 2);
			obj.AddMutation(new MultipleArms(), 2);
			GO.TakeObjectFromPopulation("Melee Weapons 2", null, Silent: false, 0, 50);
			GO.TakeObjectFromPopulation("Melee Weapons 2", null, Silent: false, 0);
			GO.TakeObjectFromPopulation("Melee Weapons 1", null, Silent: false, 0, 50);
			GO.TakeObjectFromPopulation("Melee Weapons 1", null, Silent: false, 0);
		}
		GO.TakeObjectFromPopulation("Junk 2R", null, Silent: false, 0);
		GO.TakeObjectFromPopulation("Junk 3", null, Silent: false, 0);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		GO.GetStat("Strength").BaseValue += Stat.Roll("1d4") + num;
		GO.GetStat("Agility").BaseValue += Stat.Roll("1d4") + num;
		GO.GetStat("Intelligence").BaseValue += Stat.Roll("1d4");
		GO.GetStat("Ego").BaseValue += Stat.Roll("1d4");
		GO.GetStat("Willpower").BaseValue += Stat.Roll("1d4");
		GO.GetStat("Toughness").BaseValue += Stat.Roll("1d4");
		GO.GetStat("XPValue").BaseValue += num * 75;
		for (int i = 0; i < num; i++)
		{
			GO.GetStat("Hitpoints").BaseValue += "1d10".RollCached();
		}
		if (text.Contains("Philanderer") && GO.HasPart("BaboonHero1Pack"))
		{
			GO.GetPart<BaboonHero1Pack>().nFollowerMultiplier = 2.5f;
		}
		if (text.Contains("Riddler"))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new Confusion(), 2);
		}
		if (text.Contains("Hermit"))
		{
			GO.GetStat("Hitpoints").BaseValue *= 2;
			if (GO.HasPart("BaboonHero1Pack"))
			{
				GO.GetPart<BaboonHero1Pack>().nFollowerMultiplier = 0f;
			}
		}
		if (text.Contains("Sophisticate"))
		{
			GO.GetStat("Intelligence").BaseValue += Stat.Random(6, 12);
			if (GO.HasPart("BaboonHero1Pack"))
			{
				GO.GetPart<BaboonHero1Pack>().bHat = true;
			}
		}
		return true;
	}
}
