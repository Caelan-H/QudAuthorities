using System;

namespace XRL.World.Parts;

[Serializable]
public class Breeder : IPart
{
	public int Chance;

	public int ReductionChance;

	public string Blueprint;

	public override bool SameAs(IPart p)
	{
		Breeder breeder = p as Breeder;
		if (breeder.Chance != Chance)
		{
			return false;
		}
		if (breeder.ReductionChance != ReductionChance)
		{
			return false;
		}
		if (breeder.Blueprint != Blueprint)
		{
			return false;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (!ParentObject.IsValid() || !Chance.in100())
		{
			return;
		}
		foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
		{
			if (!adjacentCell.IsEmpty())
			{
				continue;
			}
			GameObject gameObject = GameObject.create(Blueprint.Contains(",") ? Blueprint.Split(',').GetRandomElement() : Blueprint);
			Chance--;
			if (gameObject.GetPart("Breeder") is Breeder breeder)
			{
				if (ReductionChance.in100())
				{
					breeder.Chance = Chance - 1;
				}
				else
				{
					breeder.Chance = Chance;
				}
			}
			gameObject.TakeOnAttitudesOf(ParentObject);
			if (ParentObject.PartyLeader != null && gameObject.PartyLeader == null)
			{
				gameObject.PartyLeader = ParentObject.PartyLeader;
			}
			gameObject.MakeActive();
			adjacentCell.AddObject(gameObject);
			break;
		}
	}
}
