using System.Collections.Generic;
using XRL.Names;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class BethsaidaHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", new Dictionary<string, string> { { "*Patron*", "Bethsaida" } }, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		if (text.Contains(" prescient "))
		{
			(GO.GetPart("Mutations") as Mutations).AddMutation(new Precognition(), 5);
		}
		if (text.Contains(" wise "))
		{
			GO.GetStat("Willpower").BaseValue += "2d3".RollCached();
		}
		if (20.in100())
		{
			GO.TakeObjectFromPopulation("Melee Weapons 4", null, Silent: false, 0, 50, 0, Context);
		}
		else
		{
			GO.TakeObjectFromPopulation("Melee Weapons 3", null, Silent: false, 0, 50, 0, Context);
		}
		if (20.in100())
		{
			GO.TakeObjectFromPopulation("Armor 4", null, Silent: false, 0, 50, 0, Context);
		}
		else
		{
			GO.TakeObjectFromPopulation("Armor 3", null, Silent: false, 0, 50, 0, Context);
		}
		GO.TakeObjectFromPopulation("Junk 3R", null, Silent: false, 0, 0, 0, Context);
		GO.TakeObjectFromPopulation("Junk 4", null, Silent: false, 0, 0, 0, Context);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		return true;
	}
}
