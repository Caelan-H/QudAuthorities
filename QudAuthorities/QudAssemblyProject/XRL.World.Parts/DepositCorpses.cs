using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class DepositCorpses : IActivePart
{
	private const string RESERVE_PROPERTY = "DepositeCorpsesReserve";

	public DepositCorpses()
	{
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == IdleQueryEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (TryDepositCorpses(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool TryDepositCorpses(GameObject who)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (who.pBrain == null)
		{
			return false;
		}
		if (who.HasTagOrProperty("NoHauling"))
		{
			return false;
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		List<GameObject> list = Event.NewGameObjectList();
		currentZone.FindObjectsWithTagOrProperty(list, "Corpse");
		if (list.Count <= 0)
		{
			return false;
		}
		GameObject randomElement = list.GetRandomElement();
		if (randomElement.HasIntProperty("DepositeCorpsesReserve"))
		{
			randomElement.ModIntProperty("DepositeCorpsesReserve", -1);
			if (randomElement.HasIntProperty("DepositeCorpsesReserve"))
			{
				return false;
			}
		}
		if (who.WouldBeOverburdened(randomElement))
		{
			return false;
		}
		Cell cell = randomElement.CurrentCell;
		if (cell == null || cell.GetNavigationWeightFor(who) >= 30)
		{
			return false;
		}
		randomElement.SetIntProperty("DepositeCorpsesReserve", 50);
		who.pBrain.PushGoal(new DisposeOfCorpse(randomElement, ParentObject));
		return true;
	}
}
