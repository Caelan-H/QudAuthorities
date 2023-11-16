using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[Serializable]
public class GenericPack : IPart
{
	public string Table = "Bethesda Susa Wharf";

	public string Amount = "3d6";

	public int Radius = 4;

	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			try
			{
				if (bCreated)
				{
					return true;
				}
				bCreated = true;
				Physics obj = ParentObject.GetPart("Physics") as Physics;
				List<Cell> list = new List<Cell>();
				obj.CurrentCell.GetAdjacentCells(Radius, list);
				List<Cell> list2 = new List<Cell>();
				foreach (Cell item in list)
				{
					if (item.IsEmpty())
					{
						list2.Add(item);
					}
				}
				int num = Stat.Roll(Amount);
				for (int i = 0; i < num; i++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					int high = list2.Count - 1;
					int index = Stat.Random(0, high);
					Cell cell = list2[index];
					list2.Remove(cell);
					GameObject gameObject = EncounterFactory.Factory.RollOneFromTable(Table);
					(gameObject.GetPart("Brain") as Brain).PartyLeader = ParentObject;
					cell.AddObject(gameObject);
					XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
				}
			}
			catch
			{
			}
		}
		return base.FireEvent(E);
	}
}
