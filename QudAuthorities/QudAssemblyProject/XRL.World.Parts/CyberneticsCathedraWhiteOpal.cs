using System;
using System.Collections.Generic;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraWhiteOpal : CyberneticsCathedra
{
	public int BillowsTimer = -1;

	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		ActivatedAbilityID = Object.AddActivatedAbility("Glitter Bomb", "CommandActivateCathedra", "Cybernetics", null, "á");
	}

	public override void Activate(GameObject Actor)
	{
		int level = GetLevel(Actor);
		BillowsTimer = 1 + level / 2;
		IComponent<GameObject>.XDidY(Actor, "start", "releasing glitter dust", "!");
		GlitterBomb(Actor, level, 800);
		base.Activate(Actor);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (BillowsTimer < 0)
		{
			return true;
		}
		GameObject user = base.User;
		if (user == null)
		{
			BillowsTimer = 0;
			return true;
		}
		GlitterBomb(user, GetLevel(user), 800);
		BillowsTimer--;
		return base.HandleEvent(E);
	}

	public static void GlitterBomb(GameObject Actor, int Level, int Density)
	{
		if (Actor.OnWorldMap())
		{
			return;
		}
		List<Cell> cells = new List<Cell>(8);
		Actor.CurrentCell.ForeachAdjacentCell(delegate(Cell C)
		{
			if (!C.IsOccluding())
			{
				cells.Add(C);
			}
		});
		if (cells.Count == 0)
		{
			cells.Add(Actor.CurrentCell);
		}
		Phase.carryOverPrep(Actor, out var FX, out var FX2);
		Event @event = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item in cells)
		{
			GameObject gameObject = GameObject.create("GlitterGas");
			Gas gas = gameObject.GetPart("Gas") as Gas;
			gas.Creator = Actor;
			gas.Density = Density / cells.Count;
			gas.Level = Level;
			Phase.carryOver(Actor, gameObject, FX, FX2);
			@event.SetParameter("Gas", gas);
			Actor.FireEvent(@event);
			item.AddObject(gameObject);
		}
	}
}
