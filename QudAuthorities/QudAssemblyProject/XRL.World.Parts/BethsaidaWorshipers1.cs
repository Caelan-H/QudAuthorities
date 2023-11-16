using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class BethsaidaWorshipers1 : IPart
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
			ParentObject.pPhysics.CurrentCell.GetAdjacentCells(4, list);
			List<Cell> list2 = new List<Cell>();
			foreach (Cell item in list)
			{
				if (item.IsEmpty())
				{
					list2.Add(item);
				}
			}
			int num = Stat.Random(4, 8);
			int num2 = Stat.Random(1, 3);
			int num3 = Stat.Random(0, 1);
			List<string> list3 = new List<string>(num + num2 + num3);
			for (int i = 0; i < num; i++)
			{
				list3.Add("Glow-Wight Cultist of Bethsaida");
			}
			for (int j = 0; j < num2; j++)
			{
				list3.Add("Novice of the Sightless Way 3");
			}
			for (int k = 0; k < num3; k++)
			{
				list3.Add("Disciple of the Sightless Way");
			}
			for (int l = 0; l < list3.Count; l++)
			{
				if (list2.Count <= 0)
				{
					break;
				}
				GameObject gameObject = GameObjectFactory.Factory.CreateObject(list3[l]);
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
		return true;
	}
}
