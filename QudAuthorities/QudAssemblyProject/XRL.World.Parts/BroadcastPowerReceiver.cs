using System;
using System.Collections.Generic;
using HistoryKit;
using XRL.Core;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class BroadcastPowerReceiver : IPoweredPart
{
	public int ChargeRate = 10;

	public bool CanReceiveSatellitePower = true;

	public int MaxSatellitePowerDepth = 12;

	public string SatelliteWorld = "JoppaWorld";

	public bool IgnoresSatellitePowerOcclusion;

	public bool Obvious;

	public bool SatellitePowerOcclusionReadout;

	public List<string> SatelliteWorlds;

	public static long SatellitePowerOcclusionCheckTurn
	{
		get
		{
			return XRLCore.Core.Game.GetInt64GameState("SatellitePowerOcclusionCheckTurn", 0L);
		}
		set
		{
			XRLCore.Core.Game.SetInt64GameState("SatellitePowerOcclusionCheckTurn", value);
		}
	}

	public static bool SatellitePowerOccluded
	{
		get
		{
			return XRLCore.Core.Game.GetBooleanGameState("SatellitePowerOccluded");
		}
		set
		{
			XRLCore.Core.Game.SetBooleanGameState("SatellitePowerOccluded", value);
		}
	}

	public static string SatellitePowerOcclusionReason
	{
		get
		{
			return XRLCore.Core.Game.GetStringGameState("SatellitePowerOcclusionReason");
		}
		set
		{
			XRLCore.Core.Game.SetStringGameState("SatellitePowerOcclusionReason", value);
		}
	}

	public static void CheckSatellitePowerOcclusion(int Turns = 1)
	{
		if (SatellitePowerOcclusionCheckTurn >= The.CurrentTurn)
		{
			return;
		}
		int intSetting = GlobalConfig.GetIntSetting("SatellitePowerOcclusionChance", 1);
		int intSetting2 = GlobalConfig.GetIntSetting("SatellitePowerDeocclusionChance", 5);
		for (int i = 0; i < Turns; i++)
		{
			if (SatellitePowerOccluded)
			{
				if (intSetting2.in1000())
				{
					SatellitePowerOccluded = false;
				}
			}
			else if (intSetting.in1000())
			{
				SatellitePowerOccluded = true;
				SatellitePowerOcclusionReason = HistoricStringExpander.ExpandString("<spice.satellitePower.occlusionReasons.!random>", null, null);
			}
		}
		SatellitePowerOcclusionCheckTurn = The.CurrentTurn;
	}

	public BroadcastPowerReceiver()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		BroadcastPowerReceiver broadcastPowerReceiver = p as BroadcastPowerReceiver;
		if (broadcastPowerReceiver.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (broadcastPowerReceiver.CanReceiveSatellitePower != CanReceiveSatellitePower)
		{
			return false;
		}
		if (broadcastPowerReceiver.MaxSatellitePowerDepth != MaxSatellitePowerDepth)
		{
			return false;
		}
		if (broadcastPowerReceiver.SatelliteWorld != SatelliteWorld)
		{
			return false;
		}
		if (broadcastPowerReceiver.IgnoresSatellitePowerOcclusion != IgnoresSatellitePowerOcclusion)
		{
			return false;
		}
		if (broadcastPowerReceiver.Obvious != Obvious)
		{
			return false;
		}
		if (broadcastPowerReceiver.SatellitePowerOcclusionReadout != SatellitePowerOcclusionReadout)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == QueryBroadcastDrawEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Obvious || IComponent<GameObject>.ThePlayer.GetIntProperty("TechScannerEquipped") > 0)
		{
			E.Postfix.Append("\n{{rules|This object has a broadcast power receiver that can pick up electrical charge").Append(CanReceiveSatellitePower ? " either from satellites if not too far underground or" : "").Append(" from a nearby broadcast power transmitter.");
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		if (!IgnoresSatellitePowerOcclusion && SatellitePowerOccluded && (SatellitePowerOcclusionReadout || Scanning.HasScanningFor(IComponent<GameObject>.ThePlayer, Scanning.Scan.Tech)) && CouldReceivePowerFromSatellite())
		{
			E.Postfix.AppendRules("\n{{rules|Satellite broadcast power is currently occluded by {{R|" + SatellitePowerOcclusionReason + "}}.}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryBroadcastDrawEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Draw += ChargeRate;
		}
		return base.HandleEvent(E);
	}

	public bool CouldReceivePowerFromSatellite(Cell C = null)
	{
		if (C == null)
		{
			C = GetAnyBasisCell();
			if (C == null)
			{
				return false;
			}
		}
		Zone parentZone = C.ParentZone;
		if (string.IsNullOrEmpty(SatelliteWorld))
		{
			if (parentZone == null)
			{
				return false;
			}
			SatelliteWorld = parentZone.GetZoneWorld();
			SatelliteWorlds = new List<string>(1) { SatelliteWorld };
		}
		else if (SatelliteWorld != "*")
		{
			if (SatelliteWorlds == null)
			{
				SatelliteWorlds = SatelliteWorld.CachedCommaExpansion();
			}
			if (parentZone == null || !SatelliteWorlds.Contains(parentZone.GetZoneWorld()))
			{
				return false;
			}
		}
		if (!parentZone.IsWorldMap() && parentZone.Z > MaxSatellitePowerDepth && !C.ConsideredOutside())
		{
			return false;
		}
		return true;
	}

	public bool IsReceivingPowerFromSatellite(Cell C = null)
	{
		if (!CanReceiveSatellitePower)
		{
			return false;
		}
		if (!IgnoresSatellitePowerOcclusion && SatellitePowerOccluded)
		{
			return false;
		}
		return CouldReceivePowerFromSatellite(C);
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
		ReceiveCharge();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		ReceiveCharge(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		ReceiveCharge(100);
	}

	public void ReceiveCharge(int Turns = 1)
	{
		Cell anyBasisCell = GetAnyBasisCell();
		if (anyBasisCell == null || (anyBasisCell.ParentZone != null && !anyBasisCell.ParentZone.IsActive()))
		{
			return;
		}
		CheckSatellitePowerOcclusion(Turns);
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		if (IsReceivingPowerFromSatellite(anyBasisCell))
		{
			ParentObject.ChargeAvailable(ChargeRate, 0L, Turns);
		}
		else if (!anyBasisCell.OnWorldMap())
		{
			int @for = CollectBroadcastChargeEvent.GetFor(ParentObject, anyBasisCell.ParentZone, anyBasisCell, ChargeRate, Turns);
			if (@for < ChargeRate)
			{
				int charge = ChargeRate - @for;
				ParentObject.ChargeAvailable(charge, 0L, Turns);
			}
		}
	}
}
