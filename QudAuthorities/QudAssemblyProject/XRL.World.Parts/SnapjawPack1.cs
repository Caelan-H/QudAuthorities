using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SnapjawPack1 : IPart
{
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
				obj.CurrentCell.GetAdjacentCells(4, list);
				List<Cell> list2 = new List<Cell>();
				foreach (Cell item in list)
				{
					if (item.IsEmpty())
					{
						list2.Add(item);
					}
				}
				List<string> list3 = new List<string>();
				int num = Stat.Random(1, 3);
				int num2 = Stat.Random(4, 8);
				for (int i = 0; i < num; i++)
				{
					list3.Add("Snapjaw Hunter 1");
				}
				for (int j = 0; j < num2; j++)
				{
					list3.Add("Snapjaw Scavenger 1");
				}
				for (int k = 0; k < list3.Count; k++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObject.create(list3[k]);
					gameObject.pBrain.PartyLeader = ParentObject;
					Cell randomElement = list2.GetRandomElement();
					randomElement.AddObject(gameObject);
					gameObject.MakeActive();
					list2.Remove(randomElement);
				}
			}
			catch
			{
			}
		}
		return base.FireEvent(E);
	}
}
