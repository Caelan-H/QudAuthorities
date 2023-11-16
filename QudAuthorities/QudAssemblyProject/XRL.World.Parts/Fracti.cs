using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Fracti : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ObjectCreatedEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.pRender.Tile = "Terrain/sw_fracti" + Stat.Random(1, 8) + ".bmp";
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object != null && E.Object.HasPart("Combat") && Factions.GetFeelingFactionToObject("Succulents", E.Object) < 50 && E.Object.PhaseAndFlightMatches(ParentObject) && E.Object != ParentObject)
		{
			if (E.Object.TakeDamage(1, "from %o thorns.", "Thorns", null, null, null, ParentObject))
			{
				E.Object.Bloodsplatter();
			}
			if (E.Object.Energy != null)
			{
				E.Object.Energy.BaseValue -= 500;
			}
		}
		return base.HandleEvent(E);
	}

	public void Grow(string OkFloor, int Size, string Direction = null)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null || Size == 0)
		{
			return;
		}
		if (Direction == null)
		{
			Grow(OkFloor, Size, "N");
			Grow(OkFloor, Size, "S");
			Grow(OkFloor, Size, "E");
			Grow(OkFloor, Size, "W");
			return;
		}
		if (15.in100())
		{
			if (40.in100())
			{
				Cell localCellFromDirection = cell.GetLocalCellFromDirection(Direction);
				if (localCellFromDirection != null && localCellFromDirection.Objects.Count == 1 && localCellFromDirection.HasObject(OkFloor))
				{
					localCellFromDirection.AddObject("Fracti").GetPart<Fracti>().Grow(OkFloor, Size - 1, Direction);
				}
			}
			switch (Direction)
			{
			case "N":
				Direction = "W";
				break;
			case "W":
				Direction = "S";
				break;
			case "S":
				Direction = "E";
				break;
			case "E":
				Direction = "N";
				break;
			}
		}
		Cell localCellFromDirection2 = cell.GetLocalCellFromDirection(Direction);
		if (localCellFromDirection2 != null && localCellFromDirection2.Objects.Count == 1 && localCellFromDirection2.HasObject(OkFloor))
		{
			localCellFromDirection2.AddObject("Fracti").GetPart<Fracti>().Grow(OkFloor, Size - 1, Direction);
		}
	}
}
