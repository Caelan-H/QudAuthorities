using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AgolgutWorshipers1 : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			List<Cell> list = new List<Cell>();
			ParentObject.CurrentCell.GetAdjacentCells(4, list);
			List<Cell> list2 = new List<Cell>();
			foreach (Cell item in list)
			{
				if (item.IsEmpty())
				{
					list2.Add(item);
				}
			}
			int num = Stat.Random(4, 8);
			int num2 = Stat.Random(0, 2);
			List<string> list3 = new List<string>(num + num2);
			for (int i = 0; i < num; i++)
			{
				list3.Add("Glow-Wight Cultist of Agolgut");
			}
			for (int j = 0; j < num2; j++)
			{
				list3.Add("Novice of the Sightless Way 2");
			}
			for (int k = 0; k < list3.Count; k++)
			{
				if (list2.Count <= 0)
				{
					break;
				}
				GameObject gameObject = GameObject.create(list3[k]);
				gameObject.BecomeCompanionOf(ParentObject);
				Cell randomElement = list2.GetRandomElement();
				randomElement.AddObject(gameObject);
				gameObject.MakeActive();
				list2.Remove(randomElement);
			}
		}
		catch
		{
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}
