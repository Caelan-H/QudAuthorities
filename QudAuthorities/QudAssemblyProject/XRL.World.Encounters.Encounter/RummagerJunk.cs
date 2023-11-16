using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class RummagerJunk
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		for (int i = 0; i < 3; i++)
		{
			if (Stat.Random(1, 100) < 25)
			{
				GO.TakeObjectFromPopulation("Junk 2", null, Silent: false, 0, 0, 0, Context);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (Stat.Random(1, 100) < 25)
			{
				GO.TakeObjectFromPopulation("Junk 3", null, Silent: false, 0, 0, 0, Context);
			}
		}
		if (Stat.Random(1, 100) < 5)
		{
			GO.TakeObjectFromPopulation("Junk 4", null, Silent: false, 0, 0, 0, Context);
		}
		string objectBlueprint = "Desert Rifle";
		string text = "Lead Slugs";
		int num = 40;
		int num2 = Stat.Random(1, 100);
		if (num2 <= 25)
		{
			objectBlueprint = "Desert Rifle";
			text = "Lead Slug";
			num = Stat.Random(6, 12);
		}
		else if (num2 <= 60)
		{
			objectBlueprint = "Borderlands Revolver";
			text = "Lead Slug";
			num = Stat.Random(12, 16);
		}
		else if (num2 <= 70)
		{
			objectBlueprint = "Semi-Automatic Pistol";
			text = "Lead Slug";
			num = Stat.Random(12, 26);
		}
		else if (num2 <= 80)
		{
			objectBlueprint = "Carbine";
			text = "Lead Slug";
			num = Stat.Random(16, 26);
		}
		else if (num2 <= 81)
		{
			objectBlueprint = "Grenade Launcher";
			text = "HEGrenade1";
			num = Stat.Random(3, 6);
		}
		else if (num2 <= 83)
		{
			objectBlueprint = "Missile Launcher";
			text = "HE Missile";
			num = Stat.Random(3, 6);
		}
		else if (num2 <= 85)
		{
			objectBlueprint = "Chaingun";
			text = "Lead Slug";
			num = Stat.Random(176, 196);
		}
		else if (num2 <= 88)
		{
			objectBlueprint = "Flamethrower";
			text = "Oilskin";
			num = Stat.Random(1, 2);
		}
		else if (num2 <= 91)
		{
			objectBlueprint = "Sniper Rifle";
			text = "Lead Slug";
			num = Stat.Random(14, 16);
		}
		else if (num2 <= 96)
		{
			objectBlueprint = "Chain Pistol";
			text = "Lead Slug";
			num = Stat.Random(114, 116);
		}
		else if (num2 <= 100)
		{
			objectBlueprint = "Combat Shotgun";
			text = "Shotgun Shell";
			num = Stat.Random(8, 16);
		}
		if (text == "Chem Cell")
		{
			foreach (GameObject item in new List<GameObject> { GameObjectFactory.Factory.CreateObject(objectBlueprint, 0, 0, null, null, Context) })
			{
				(item.GetPart("EnergyCellSocket") as EnergyCellSocket).Cell = GameObjectFactory.Factory.CreateObject("Chem Cell", 0, 0, null, null, Context);
				GO.TakeObject(item, Silent: false, 0);
			}
		}
		else
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(objectBlueprint, 0, 0, null, null, Context);
			LiquidAmmoLoader part = gameObject.GetPart<LiquidAmmoLoader>();
			for (int k = 0; k < num; k++)
			{
				GameObject gameObject2 = GameObjectFactory.Factory.CreateObject(text, 0, 0, null, null, Context);
				if (part != null)
				{
					LiquidVolume liquidVolume = gameObject2.LiquidVolume;
					if (liquidVolume != null)
					{
						liquidVolume.Empty();
						liquidVolume.ComponentLiquids.Add(part.Liquid, 1000);
						liquidVolume.Volume = Stat.Random(liquidVolume.MaxVolume / 3 + 1, liquidVolume.MaxVolume);
					}
				}
				GO.TakeObject(gameObject2, Silent: false, 0);
			}
			GO.TakeObject(gameObject, Silent: false, 0);
		}
		return true;
	}
}
