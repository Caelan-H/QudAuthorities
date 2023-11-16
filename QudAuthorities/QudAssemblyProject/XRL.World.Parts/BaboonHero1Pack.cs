using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class BaboonHero1Pack : IPart
{
	[NonSerialized]
	public bool bHat;

	[NonSerialized]
	public int nRings = 1;

	[NonSerialized]
	public float nFollowerMultiplier = 1f;

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
				_ = ParentObject;
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
				int num2 = Stat.Random(1, 3);
				for (int i = 0; (float)i < (float)num * nFollowerMultiplier; i++)
				{
					list3.Add("Baboon");
				}
				for (int j = 0; (float)j < (float)num2 * nFollowerMultiplier; j++)
				{
					list3.Add("Hulking Baboon");
				}
				for (int k = 0; k < list3.Count; k++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObjectFactory.Factory.CreateObject(list3[k]);
					if (bHat)
					{
						gameObject.TakeObject(GameObjectFactory.Factory.CreateObject("Leather Cap"), Silent: false, 0);
					}
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
