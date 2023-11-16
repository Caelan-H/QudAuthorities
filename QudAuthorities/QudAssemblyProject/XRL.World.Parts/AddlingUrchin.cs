using System;

namespace XRL.World.Parts;

[Serializable]
public class AddlingUrchin : IPart
{
	public int Puffed;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "BeforeDie");
		Object.RegisterPartEvent(this, "PuffPlease");
		base.Register(Object);
	}

	public void Puff()
	{
		Puffed = 40;
		foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
		{
			adjacentCell.AddObject(GameObject.create("ConfusionGas80"));
		}
		ParentObject.DustPuff();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie")
		{
			Puff();
		}
		else if (E.ID == "PuffPlease")
		{
			Puff();
		}
		else if (E.ID == "EndTurn")
		{
			if (Puffed > 0)
			{
				Puffed--;
			}
			else if (ParentObject.CurrentCell != null)
			{
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
				{
					foreach (GameObject item in adjacentCell.GetObjectsWithPartReadonly("Brain"))
					{
						if (ParentObject.IsHostileTowards(item))
						{
							Puff();
							return true;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
