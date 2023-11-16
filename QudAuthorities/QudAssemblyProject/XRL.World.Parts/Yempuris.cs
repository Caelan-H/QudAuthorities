using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Yempuris : IPart
{
	public string ClusterSize = "1";

	public string Damage = "1d12";

	public string[] D = new string[8] { "NW", "SE", "SW", "NE", "N", "S", "E", "W" };

	public static long LastSplitTurn;

	public static long Growth;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (ParentObject.pPhysics.IsFrozen())
			{
				return true;
			}
			if (LastSplitTurn == XRLCore.Core.Game.Turns)
			{
				return true;
			}
			if (ParentObject.pPhysics.CurrentCell != null)
			{
				int num = 0;
				List<Cell> localAdjacentCells = ParentObject.pPhysics.CurrentCell.GetLocalAdjacentCells();
				for (int i = 0; i < localAdjacentCells.Count; i++)
				{
					if (localAdjacentCells[i].HasObjectWithBlueprint("Yempuris"))
					{
						num++;
					}
				}
				if (num > 6)
				{
					ParentObject.Destroy();
					return true;
				}
				string[] d = D;
				foreach (string direction in d)
				{
					Cell localCellFromDirection = ParentObject.pPhysics.CurrentCell.GetLocalCellFromDirection(direction);
					if (localCellFromDirection != null && !localCellFromDirection.HasObjectWithBlueprint("Yempuris"))
					{
						localCellFromDirection.AddObject("Yempuris");
						LastSplitTurn = XRLCore.Core.Game.Turns;
						return true;
					}
					if (Stat.Random(1, 100) <= 50)
					{
						return true;
					}
				}
			}
			return true;
		}
		if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObject = E.GetParameter("Object") as GameObject;
			Brain brain = ParentObject.GetPart("Brain") as Brain;
			if (gameObject != null && gameObject.HasPart("Combat") && brain.IsHostileTowards(gameObject) && gameObject.PhaseAndFlightMatches(ParentObject))
			{
				Damage value = new Damage(Stat.Roll(Damage));
				Event @event = Event.New("TakeDamage");
				@event.AddParameter("Damage", value);
				@event.AddParameter("Owner", null);
				@event.AddParameter("Attacker", ParentObject);
				@event.AddParameter("Message", "from %o impalement.");
				if (gameObject.Statistics.ContainsKey("Energy"))
				{
					gameObject.Energy.BaseValue -= 500;
				}
				if (gameObject.FireEvent(@event))
				{
					gameObject.Bloodsplatter();
				}
			}
		}
		return base.FireEvent(E);
	}
}
