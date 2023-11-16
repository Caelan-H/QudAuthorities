using System;

namespace XRL.World.Parts;

[Serializable]
public class ConveyorDrive : IPart
{
	public int TurnsPerImpulse = 2;

	public int CurrentTurn;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			if (IsEMPed() || IsBroken() || IsRusted())
			{
				return true;
			}
			CurrentTurn++;
			if (CurrentTurn >= TurnsPerImpulse)
			{
				CurrentTurn = 0;
				if (ParentObject.pPhysics.CurrentCell != null)
				{
					foreach (Cell cardinalAdjacentCell in ParentObject.pPhysics.CurrentCell.GetCardinalAdjacentCells())
					{
						foreach (GameObject item in cardinalAdjacentCell.GetObjectsWithPartReadonly("ConveyorPad"))
						{
							item.GetPart<ConveyorPad>().ConveyorImpulse(60, Event.NewGameObjectList());
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
