using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_DisorientingFire : BaseSkill
{
	public static bool MeetsCriteria(GameObject GO)
	{
		if (!GameObject.validate(ref GO))
		{
			return false;
		}
		if (!GO.HasPart("Combat"))
		{
			return false;
		}
		if (GO.pBrain != null)
		{
			Cell cell = GO.CurrentCell;
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 80, "Brain", GO);
			for (int i = 0; i < list.Count; i++)
			{
				GameObject gameObject = list[i];
				if (gameObject == GO)
				{
					continue;
				}
				Brain pBrain = gameObject.pBrain;
				foreach (string key in GO.pBrain.FactionMembership.Keys)
				{
					foreach (string key2 in pBrain.FactionMembership.Keys)
					{
						if (key == key2 && pBrain.FactionMembership[key2] > 0 && GO.pBrain.FactionMembership[key] > 0)
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}
}
