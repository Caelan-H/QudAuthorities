using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class GoatfolkHero1
{
	public string ForceTitle;

	public string ForceName;

	public bool BuildObject(GameObject GO, string Context = null)
	{
		string text;
		if (!string.IsNullOrEmpty(ForceName))
		{
			text = ForceName;
		}
		else
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(ForceTitle))
			{
				dictionary["*Epithet*"] = ForceTitle;
			}
			text = NameMaker.MakeName(GO, null, null, null, null, null, null, null, null, "Hero", dictionary, FailureOkay: false, SpecialFaildown: true);
		}
		GO.DisplayName = "{{M|" + text + "}}";
		GO.HasProperName = true;
		Mutations mutations = GO.GetPart("Mutations") as Mutations;
		GO.TakeObjectFromPopulation("Armor 4", null, Silent: false, 0, 50, 0, Context);
		if (text.Contains("Stargazer"))
		{
			GO.BoostStat("Intelligence", 2);
			if (!mutations.HasMutation("LightManipulation"))
			{
				mutations.AddMutation(new LightManipulation(), 5);
			}
		}
		if (text.Contains("Heartbiter") && !mutations.HasMutation("AdrenalControl"))
		{
			mutations.AddMutation(new AdrenalControl(), 1);
		}
		if (text.Contains("Twicetalker") && !mutations.HasMutation("TwoHeaded"))
		{
			mutations.AddMutation(new TwoHeaded(), 1);
			GO.TakeObject(GameObject.create("Goatfolk_Horns", 0, 0, Context), Silent: false, 0);
		}
		if (text.Contains("Souldrinker") && !mutations.HasMutation("Syphon Vim"))
		{
			mutations.AddMutation(new LifeDrain(), 1);
		}
		if (text.Contains("Whitefinger") && !mutations.HasMutation("ElectricalGeneration"))
		{
			mutations.AddMutation(new ElectricalGeneration(), 4);
		}
		if (text.Contains("Clovenhorn"))
		{
			GO.BoostStat("Strength", 1);
			if (!mutations.HasMutation("Horns"))
			{
				mutations.AddMutation(new Horns(), Stat.Random(5, 6));
			}
			(GO.GetPart("Skills") as XRL.World.Parts.Skills).AddSkill(new Tactics_Charge());
		}
		if (text.Contains("Clan Hotur"))
		{
			GO.BoostStat("Strength", 1);
		}
		if (text.Contains("Clan Ibex"))
		{
			mutations.AddMutation(new Horns(), 2);
		}
		if (text.Contains("Clan Sol") && !mutations.HasMutation("PhotosyntheticSkin"))
		{
			mutations.AddMutation(new PhotosyntheticSkin(), 4);
		}
		if (text.Contains("Whitetongue"))
		{
			GO.BoostStat("Intelligence", 1);
			GO.BoostStat("Ego", 1);
			GO.BoostStat("Willpower", 1);
		}
		if (text.Contains("Clan Yr"))
		{
			GO.BoostStat("MoveSpeed", -0.75);
		}
		if (text.Contains("Clan Mnim"))
		{
			GO.BoostStat("Toughness", 1);
		}
		GO.TakeObjectFromPopulation("Junk 4", null, Silent: false, 0, 0, 0, Context);
		GO.TakeObjectFromPopulation("Junk 4R", null, Silent: false, 0, 0, 0, Context);
		GO.MultiplyStat("Hitpoints", 2);
		return true;
	}
}
