using System;

namespace XRL.World.Parts;

[Serializable]
public class AmbientCollector : IPoweredPart
{
	[NonSerialized]
	private static int LiveArtifactCount;

	public AmbientCollector()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == QueryChargeProductionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Zone anyBasisZone = GetAnyBasisZone();
			if (anyBasisZone != null)
			{
				LiveArtifactCount = 0;
				anyBasisZone.ForeachObject(FindLiveArtifacts);
				int num = LiveArtifactCount * E.Multiple;
				if (num >= 500)
				{
					E.Amount += Math.Max(1, Math.Min(num / 1000, E.Multiple));
				}
			}
		}
		return base.HandleEvent(E);
	}

	private void FindLiveArtifacts(GameObject obj)
	{
		if (obj != ParentObject)
		{
			if (obj.HasPart("Robot") || obj.IsImplant)
			{
				LiveArtifactCount++;
			}
			else if (obj.HasPartDescendedFrom<IActivePart>())
			{
				bool flag = false;
				foreach (IActivePart item in obj.GetPartsDescendedFrom<IActivePart>())
				{
					if (item.ChargeUse > 0)
					{
						flag = true;
						break;
					}
				}
				if (flag && obj.TestCharge(1, LiveOnly: false, 0L))
				{
					LiveArtifactCount++;
				}
			}
		}
		obj.ForeachInventoryEquipmentAndCybernetics(FindLiveArtifacts);
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
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone != null && !anyBasisZone.IsWorldMap())
		{
			LiveArtifactCount = 0;
			anyBasisZone.ForeachObject(FindLiveArtifacts);
			if (LiveArtifactCount.in1000())
			{
				ParentObject.ChargeAvailable(1, 0L);
				ConsumeCharge();
			}
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone != null)
		{
			LiveArtifactCount = 0;
			anyBasisZone.ForeachObject(FindLiveArtifacts);
			if (LiveArtifactCount.in100())
			{
				ParentObject.ChargeAvailable(1, 0L);
				ConsumeCharge();
			}
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone != null)
		{
			LiveArtifactCount = 0;
			anyBasisZone.ForeachObject(FindLiveArtifacts);
			if (LiveArtifactCount.in10())
			{
				ParentObject.ChargeAvailable(1, 0L);
				ConsumeCharge();
			}
		}
	}
}
