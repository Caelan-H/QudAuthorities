using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class NightSightInterpolators : IPoweredPart
{
	public int Radius = 18;

	public NightSightInterpolators()
	{
		WorksOnEquipper = true;
		IsPowerLoadSensitive = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != EarlyBeforeBeginTakeActionEvent.ID && ID != EquippedEvent.ID)
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EarlyBeforeBeginTakeActionEvent E)
	{
		if (!base.OnWorldMap && ConsumeChargeIfOperational())
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null && equipped.IsPlayer() && AutoAct.IsInterruptable() && !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				AutoAct.Interrupt(equipped.poss(ParentObject) + ParentObject.GetVerb("have") + " stopped working");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (!base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped != null && equipped.IsPlayer() && !equipped.OnWorldMap())
		{
			Cell cell = equipped.CurrentCell;
			if (cell != null)
			{
				int lastPowerLoadLevel = GetLastPowerLoadLevel();
				if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, lastPowerLoadLevel))
				{
					int num = Radius;
					int num2 = MyPowerLoadBonus(lastPowerLoadLevel, 100, 10);
					if (num2 != 0)
					{
						num = num * (100 + num2) / 100;
					}
					cell.ParentZone.AddLight(cell.X, cell.Y, num, LightLevel.Interpolight);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
