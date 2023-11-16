using XRL.Names;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class SnapjawHero1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", null, FailureOkay: false, SpecialFaildown: true);
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		SnapjawHero.ApplySnapjawTraits(GO, text, Context);
		GO.TakeObjectFromPopulation("Melee Weapons 2", null, Silent: false, 0, 25, 0, Context);
		GO.TakeObjectFromPopulation("Armor 2", null, Silent: false, 0, 25, 0, Context);
		GO.GetStat("Hitpoints").BaseValue *= 2;
		GO.TakeObjectFromPopulation("Junk 1R", null, Silent: false, 0, 0, 0, Context);
		if (50.in100())
		{
			GO.TakeObjectFromPopulation("Junk 3", null, Silent: false, 0, 0, 0, Context);
		}
		return true;
	}
}
