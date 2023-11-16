using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Encounters.EncounterObjectBuilders;

public class GunslingerEquipment1
{
	public bool BuildObject(GameObject GO, string Context = null)
	{
		string objectBlueprint = "Laser Pistol";
		string text = "Chem Cell";
		int num = 2;
		int num2 = Stat.Random(1, 100);
		if (num2 <= 25)
		{
			objectBlueprint = "Laser Pistol";
		}
		else if (num2 <= 55)
		{
			objectBlueprint = "Borderlands Revolver";
			text = "Lead Slug";
			num = Stat.Random(36, 46);
		}
		else if (num2 <= 75)
		{
			objectBlueprint = "Semi-Automatic Pistol";
			text = "Lead Slug";
			num = Stat.Random(76, 96);
		}
		else if (num2 <= 100)
		{
			objectBlueprint = "Chain Pistol";
			text = "Lead Slug";
			num = Stat.Random(136, 246);
		}
		if (text == "Chem Cell")
		{
			foreach (GameObject item in new List<GameObject>
			{
				GameObjectFactory.Factory.CreateObject(objectBlueprint, 45, 0, null, null, Context),
				GameObjectFactory.Factory.CreateObject(objectBlueprint, 0, 0, null, null, Context)
			})
			{
				(item.GetPart("EnergyCellSocket") as EnergyCellSocket).Cell = GameObjectFactory.Factory.CreateObject("Chem Cell", 0, 0, null, null, Context);
				GO.TakeObject(item, Silent: false, 0);
			}
		}
		else
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(objectBlueprint, 45, 0, null, null, Context);
			GameObject gO = GameObjectFactory.Factory.CreateObject(objectBlueprint, 0, 0, null, null, Context);
			LiquidAmmoLoader part = gameObject.GetPart<LiquidAmmoLoader>();
			for (int i = 0; i < num; i++)
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
			GO.TakeObject(gO, Silent: false, 0);
		}
		return true;
	}
}
