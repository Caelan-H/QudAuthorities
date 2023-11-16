using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class EyelessKingCrabSkuttle1 : IPart
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
				obj.CurrentCell.GetAdjacentCells(3, list);
				List<Cell> list2 = new List<Cell>();
				foreach (Cell item in list)
				{
					if (item.IsEmpty())
					{
						list2.Add(item);
					}
				}
				List<string> list3 = new List<string>();
				int num = Stat.Random(1, 4);
				int num2 = Stat.Random(0, 1);
				for (int i = 0; i < num; i++)
				{
					list3.Add("Eyeless Crab");
				}
				for (int j = 0; j < num2; j++)
				{
					list3.Add("Rustacean");
				}
				for (int k = 0; k < list3.Count; k++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObjectFactory.Factory.CreateObject(list3[k]);
					gameObject.GetPart<Brain>().PartyLeader = ParentObject;
					Cell randomElement = list2.GetRandomElement();
					randomElement.AddObject(gameObject);
					XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
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
