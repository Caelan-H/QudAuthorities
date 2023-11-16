using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, maximum teleport distance is increased by
///             the standard power load bonus, i.e. 2 for the standard overload power
///             load of 400.
///             </remarks>
[Serializable]
public class Displacer : IPoweredPart
{
	public int MinDistance;

	public int MaxDistance = 2;

	public int Chance = 100;

	public Displacer()
	{
		WorksOnEquipper = true;
		ChargeUse = 1;
		IsPowerLoadSensitive = true;
		NameForStatus = "SpatialTransposer";
	}

	public override bool SameAs(IPart p)
	{
		Displacer displacer = p as Displacer;
		if (displacer.MinDistance != MinDistance)
		{
			return false;
		}
		if (displacer.MaxDistance != MaxDistance)
		{
			return false;
		}
		if (displacer.Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

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
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.OnWorldMap() || !Chance.in100())
		{
			return;
		}
		int num = MyPowerLoadLevel();
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
		{
			return;
		}
		int high = MaxDistance + IComponent<GameObject>.PowerLoadBonus(num);
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				int num2 = Stat.Random(MinDistance, high);
				if (num2 > 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
				{
					int maxDistance = num2;
					bool interruptMovement = !activePartSubject.IsPlayer();
					GameObject parentObject = ParentObject;
					GameObject deviceOperator = activePartSubject;
					activePartSubject.RandomTeleport(IComponent<GameObject>.Visible(activePartSubject), null, parentObject, deviceOperator, null, 0, maxDistance, interruptMovement);
					AIEvaluate(activePartSubject);
				}
			}
			return;
		}
		int num3 = Stat.Random(MinDistance, high);
		if (num3 > 0)
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				int maxDistance = num3;
				bool interruptMovement = !activePartFirstSubject.IsPlayer();
				GameObject deviceOperator = ParentObject;
				GameObject parentObject = activePartFirstSubject;
				activePartFirstSubject.RandomTeleport(IComponent<GameObject>.Visible(activePartFirstSubject), null, deviceOperator, parentObject, null, 0, maxDistance, interruptMovement);
				AIEvaluate(activePartFirstSubject);
			}
		}
	}

	public void AIEvaluate(GameObject Actor)
	{
		if (IsPowerSwitchSensitive && !Actor.IsPlayer() && Actor.pBrain != null && Actor.pBrain.Target == null)
		{
			InventoryActionEvent.Check(ParentObject, Actor, ParentObject, "PowerSwitchOff");
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetDefensiveItemList" && IsPowerSwitchSensitive && GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.SwitchedOff)
		{
			E.AddAICommand("PowerSwitchOn", 1, ParentObject, Inv: true);
		}
		return base.FireEvent(E);
	}
}
