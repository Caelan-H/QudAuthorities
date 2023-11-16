using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering : BaseSkill
{
	[NonSerialized]
	public static bool bIsPlayerOverride;

	public void LearnNewRecipe(int MinTier, int MaxTier)
	{
		Inventory inventory = ParentObject.Inventory;
		int num = ((!ParentObject.IsPlayer()) ? 1 : 3);
		List<GameObject> list = new List<GameObject>(num);
		List<TinkerData> list2 = new List<TinkerData>(num);
		while (list.Count < num)
		{
			GameObject gameObject = GameObject.create("DataDisk");
			DataDisk dataDisk = gameObject.GetPart("DataDisk") as DataDisk;
			if (dataDisk.Data.Tier >= MinTier && dataDisk.Data.Tier <= MaxTier && !list2.CleanContains(dataDisk.Data))
			{
				list.Add(gameObject);
				list2.Add(dataDisk.Data);
			}
		}
		GameObject gameObject2 = null;
		if ((ParentObject.IsPlayer() || bIsPlayerOverride) && !Popup.bSuppressPopups)
		{
			int num2 = -1;
			List<string> list3 = new List<string>(num);
			foreach (GameObject item in list)
			{
				list3.Add(item.DisplayName);
			}
			while (num2 < 0)
			{
				num2 = Popup.ShowOptionList("", list3.ToArray(), null, 0, "Choose a schematic.");
			}
			gameObject2 = list[num2];
		}
		else
		{
			gameObject2 = list[0];
		}
		inventory.AddObject(gameObject2);
		if (ParentObject.IsPlayer())
		{
			Popup.Show("You have a flash of insight and scribe " + gameObject2.a + gameObject2.DisplayName + ".");
		}
	}

	public static int GetIdentifyLevel(GameObject who)
	{
		int num = 0;
		if (who.HasSkill("Tinkering_GadgetInspector") && !who.HasPart("Dystechnia"))
		{
			num += 3;
			if (who.HasSkill("Tinkering_Tinker2"))
			{
				num += 2;
			}
			if (who.HasSkill("Tinkering_Tinker3"))
			{
				num++;
			}
			int bonus = num * (who.StatMod("Intelligence") * 3) / 100;
			num += GetTinkeringBonusEvent.GetFor(who, null, "Inspect", num, bonus);
		}
		return num;
	}
}
