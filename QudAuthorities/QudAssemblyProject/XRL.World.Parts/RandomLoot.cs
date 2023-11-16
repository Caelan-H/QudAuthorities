using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class RandomLoot : IPart
{
	[NonSerialized]
	public static int nDeaths;

	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDeathRemoval");
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" || E.ID == "ObjectCreated")
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			if (ParentObject.HasProperty("NoLoot"))
			{
				return true;
			}
			if (E.ID == "ObjectCreated" && ParentObject.GetIntProperty("Humanoid") == 0)
			{
				if (ParentObject.GetIntProperty("Ape") == 0)
				{
					return true;
				}
				if (Stat.Random(1, 10) <= 9)
				{
					return true;
				}
			}
			string stringParameter = E.GetStringParameter("Context");
			List<GameObject> list = new List<GameObject>(8);
			if (ParentObject.HasIntProperty("RareLoot"))
			{
				int intProperty = ParentObject.GetIntProperty("RareLoot");
				int num = Stat.Random(1, 3);
				for (int i = 0; i < num; i++)
				{
					list.Add(PopulationManager.CreateOneFrom("Junk " + intProperty + "R", null, 100, 0, stringParameter));
				}
			}
			else
			{
				int num2 = Stat.Random(1, 100) + ParentObject.Stat("Level");
				nDeaths++;
				num2 += nDeaths;
				int num3 = 0;
				while (num2 > 98)
				{
					list.Add(PopulationManager.CreateOneFrom("Junk " + Tier.Constrain(ParentObject.Stat("Level") / 5 + 1), null, 0, 0, stringParameter));
					nDeaths = 0;
					num2 -= 50 + Stat.Random(1, 100);
				}
			}
			if (E.ID == "ObjectCreated")
			{
				ParentObject.TakeObject(list, Silent: true, 0);
				bCreated = true;
			}
			else
			{
				Cell cell = ParentObject.pPhysics.CurrentCell;
				if (cell != null)
				{
					if (cell.IsOccluding())
					{
						foreach (Cell adjacentCell in cell.GetAdjacentCells())
						{
							if (!adjacentCell.IsOccluding())
							{
								cell = adjacentCell;
								break;
							}
						}
					}
					foreach (GameObject item in list)
					{
						if (item.GetPart("Physics") is Physics physics && physics.IsReal)
						{
							physics.InInventory = null;
							cell.AddObject(item);
							item.HandleEvent(DroppedEvent.FromPool(null, item));
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
