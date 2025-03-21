using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class RepairBay : IPoweredPart
{
	public int MaxRepairTier;

	public string Verb = "repair";

	public RepairBay()
	{
		ChargeUse = 1000;
		WorksOnInventory = true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ProcessItems();
		}
	}

	public bool ProcessItems()
	{
		bool num = ForeachActivePartSubjectWhile(ProcessItem, MayMoveAddOrDestroy: true);
		if (!num)
		{
			ConsumeCharge();
		}
		return num;
	}

	private bool ProcessItem(GameObject obj)
	{
		if (!IsRepairableEvent.Check(ParentObject, obj, ParentObject, null, MaxRepairTier))
		{
			return true;
		}
		if (!Tinkering_Repair.IsRepairableBy(obj, ParentObject, null, null, MaxRepairTier))
		{
			return true;
		}
		obj.SplitFromStack();
		if (!string.IsNullOrEmpty(Verb))
		{
			DidXToY(Verb, obj, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		}
		RepairedEvent.Send(ParentObject, obj, null, null, MaxRepairTier);
		obj.CheckStack();
		return false;
	}
}
