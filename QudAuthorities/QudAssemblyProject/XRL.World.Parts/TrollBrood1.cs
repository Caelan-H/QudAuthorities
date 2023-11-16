using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class TrollBrood1 : IPart
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
				int num = Stat.Random(4, 8);
				int num2 = Stat.Random(1, 4);
				int num3 = Stat.Random(1, 4);
				for (int i = 0; i < num; i++)
				{
					list3.Add("Troll Dwarfling");
				}
				for (int j = 0; j < num2; j++)
				{
					list3.Add("Troll Berserker");
				}
				for (int k = 0; k < num3; k++)
				{
					list3.Add("Troll Fireslinger");
				}
				for (int l = 0; l < list3.Count; l++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObjectFactory.Factory.CreateObject(list3[l]);
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
		return true;
	}
}
