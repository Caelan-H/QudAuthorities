using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class MechanimistHoundmaster1 : IPart
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
				for (int i = 0; i < 9; i++)
				{
					list3.Add("Hyrkhound");
				}
				for (int j = 0; j < list3.Count; j++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObject.create(list3[j]);
					gameObject.GetPart<Brain>().PartyLeader = ParentObject;
					Cell randomElement = list2.GetRandomElement();
					randomElement.AddObject(gameObject);
					gameObject.RequirePart<Frenzy>();
					gameObject.MakeActive();
					list2.Remove(randomElement);
				}
			}
			catch
			{
			}
		}
		return true;
	}
}
