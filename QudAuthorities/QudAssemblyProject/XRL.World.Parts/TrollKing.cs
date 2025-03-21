using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class TrollKing : IPart
{
	public static readonly string TROLL_FOAL_BLUEPRINT = "Troll Foal";

	public static readonly int TROLL_FOAL_LIMIT = 18;

	private int Counter;

	private int MaxCounter = 14;

	private bool Budding;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckSpawn();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckSpawn(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckSpawn(100);
	}

	public void CheckSpawn(int Turns = 1)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null || !currentZone.IsActive() || currentZone.IsWorldMap())
		{
			StopBudding(Turns);
			return;
		}
		if (ParentObject.pPhysics.Temperature >= 20 && currentZone.GetObjectCount(TROLL_FOAL_BLUEPRINT) < TROLL_FOAL_LIMIT)
		{
			if (!Budding)
			{
				MaxCounter = "1d5+11".RollCached();
				Counter = MaxCounter;
				Budding = true;
				if (IComponent<GameObject>.Visible(ParentObject))
				{
					Body body = ParentObject.Body;
					if (body != null && body.HasPart("Back"))
					{
						IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + ParentObject.poss("back") + " as " + ParentObject.does("begin", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + ParentObject.t() + " as " + ParentObject.does("begin", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + " to bud!");
					}
				}
			}
			if (Counter <= 0)
			{
				Spawn();
				Budding = false;
			}
			else
			{
				Counter--;
			}
			return;
		}
		if (Budding)
		{
			Budding = false;
			if (IComponent<GameObject>.Visible(ParentObject))
			{
				Body body2 = ParentObject.Body;
				if (body2 != null && body2.HasPart("Back"))
				{
					IComponent<GameObject>.AddPlayerMessage("The protuberance on " + ParentObject.poss("back") + " shrinks.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("The protuberance on " + ParentObject.t() + " shrinks.");
				}
			}
		}
		if (Counter < MaxCounter)
		{
			Counter++;
		}
	}

	public void StopBudding(int Turns = 1)
	{
		if (Budding)
		{
			Budding = false;
			if (Visible())
			{
				Body body = ParentObject.Body;
				if (body != null && body.HasPart("Back"))
				{
					IComponent<GameObject>.AddPlayerMessage("The protuberance on " + ParentObject.poss("back") + " shrinks.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("The protuberance on " + ParentObject.t() + " shrinks.");
				}
			}
		}
		Counter = Math.Min(Counter + Turns, MaxCounter);
	}

	public void Spawn()
	{
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		for (int i = 0; i < localAdjacentCells.Count; i++)
		{
			Cell cell = localAdjacentCells[i];
			if (cell.IsEmpty())
			{
				GameObject gameObject = GameObject.create(TROLL_FOAL_BLUEPRINT);
				gameObject.RemovePart("RandomLoot");
				gameObject.GetStat("XPValue").BaseValue = 0;
				gameObject.IsTrifling = true;
				gameObject.MakeActive();
				cell.AddObject(gameObject);
				IComponent<GameObject>.XDidYToZ(gameObject, "detach", "from", ParentObject, null, "!", null, ParentObject);
				gameObject.PartyLeader = ParentObject;
				ParentObject.Bloodsplatter(bSelfsplatter: false);
				break;
			}
		}
	}
}
