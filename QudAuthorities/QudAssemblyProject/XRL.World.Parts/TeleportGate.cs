using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class TeleportGate : IPoweredPart
{
	[Obsolete("save compat")]
	[FieldSaveVersion(239)]
	public int Placeholder1;

	[Obsolete("save compat")]
	public int Placeholder2;

	[Obsolete("save compat")]
	public string Placeholder3;

	[FieldSaveVersion(243)]
	public bool RingBasedName = true;

	public GlobalLocation Target;

	public TeleportGate()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		TeleportGate teleportGate = p as TeleportGate;
		if (teleportGate.RingBasedName != RingBasedName)
		{
			return false;
		}
		if (teleportGate.Target != Target)
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
		CheckPossibleSubjects();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckPossibleSubjects();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckPossibleSubjects();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetNavigationWeightEvent.ID && ID != ObjectEnteredCellEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.CurrentZone?.Built == true)
		{
			CheckIncomingTarget();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			Zone zone = ParentObject?.CurrentZone;
			if (zone != null)
			{
				string text = The.ZoneManager.GetZoneProperty(zone.ZoneID, "TeleportGateName") as string;
				if (!string.IsNullOrEmpty(text))
				{
					E.ReplacePrimaryBase(text);
				}
				else
				{
					string desc = "secant";
					if (The.ZoneManager.HasZoneProperty(zone.ZoneID, "TeleportGateRingSize"))
					{
						int num = (int)The.ZoneManager.GetZoneProperty(zone.ZoneID, "TeleportGateRingSize");
						if (num > 0)
						{
							desc = num + "-ring";
						}
					}
					E.AddBase(desc, -5);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		CheckIncomingTarget();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(60);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.CurrentZone?.Built == true)
		{
			CheckPossibleSubject(E.Object, E);
		}
		return base.HandleEvent(E);
	}

	public int CheckPossibleSubjects()
	{
		int num = 0;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				if (CheckPossibleSubject(cell.Objects[i], null, ReadyKnown: true))
				{
					num++;
					i = 0;
					count = cell.Objects.Count;
				}
			}
		}
		return num;
	}

	public bool CheckPossibleSubject(GameObject obj, IEvent FromEvent = null, bool ReadyKnown = false)
	{
		if (obj != ParentObject && obj.IsReal && !obj.IsScenery && (obj.IsCreature || obj.IsTakeable()) && obj.PhaseAndFlightMatches(ParentObject) && (ReadyKnown || IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L)))
		{
			GlobalLocation target = GetTarget();
			if (target != null)
			{
				if (target.ResolveCell() != null && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					return obj.ZoneTeleport(target.ZoneID, target.CellX, target.CellY, FromEvent, ParentObject, obj.IsCreature ? obj : null);
				}
			}
			else
			{
				obj.RandomTeleport(Swirl: true, null, null, null, null, 0, 10);
			}
		}
		return false;
	}

	public void CheckIncomingTarget()
	{
		Cell cell = ParentObject?.CurrentCell;
		if (cell == null)
		{
			return;
		}
		Zone parentZone = cell.ParentZone;
		if (parentZone == null)
		{
			return;
		}
		string zoneID = parentZone.ZoneID;
		if (!The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") || !The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			Cell cell2 = cell.GetEmptyConnectedAdjacentCells(1).GetRandomElement() ?? cell.GetEmptyConnectedAdjacentCells(2).GetRandomElement() ?? parentZone.GetEmptyCells().GetRandomElement();
			if (cell2 != null)
			{
				The.ZoneManager.SetZoneProperty(zoneID, "TeleportGateIncomingX", cell2.X);
				The.ZoneManager.SetZoneProperty(zoneID, "TeleportGateIncomingY", cell2.Y);
			}
		}
	}

	private string GetRandomDestinationZoneID(string World)
	{
		if (World != "JoppaWorld")
		{
			return null;
		}
		int parasangX = Stat.Random(0, 79);
		int parasangY = Stat.Random(0, 24);
		int zoneX = Stat.Random(0, 2);
		int zoneY = Stat.Random(0, 2);
		int zoneZ = (50.in100() ? Stat.Random(10, 40) : 10);
		return ZoneID.Assemble(World, parasangX, parasangY, zoneX, zoneY, zoneZ);
	}

	private string GetRandomTeleportGateZoneID(string World)
	{
		if (The.Game.GetObjectGameState((World ?? "Default") + "TeleportGateZones") is List<string> list && list.Count > 0)
		{
			return list.GetRandomElement();
		}
		return null;
	}

	private GlobalLocation GetRandomTarget()
	{
		string world = ParentObject.CurrentZone?.GetZoneWorld();
		string text = GetRandomTeleportGateZoneID(world);
		if (text == null)
		{
			text = GetRandomDestinationZoneID(world);
			if (text == null)
			{
				return null;
			}
		}
		return GetTargetFromZone(text);
	}

	private GlobalLocation GetPreferredTarget()
	{
		string text = ParentObject?.CurrentZone?.ZoneID;
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = The.ZoneManager.GetZoneProperty(text, "TeleportGateDestinationZone") as string;
			if (!string.IsNullOrEmpty(text2))
			{
				return GetTargetFromZone(text2);
			}
		}
		return null;
	}

	private GlobalLocation GetTarget()
	{
		if (Target == null)
		{
			Target = GetPreferredTarget() ?? GetRandomTarget();
		}
		return Target;
	}

	private GlobalLocation GetTargetFromZone(string zoneID)
	{
		if (The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") && The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			return GlobalLocation.FromZoneId(zoneID, (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingX"), (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingY"));
		}
		Zone zone = The.ZoneManager.GetZone(zoneID);
		if (The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingX") && The.ZoneManager.HasZoneProperty(zoneID, "TeleportGateIncomingY"))
		{
			return GlobalLocation.FromZoneId(zoneID, (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingX"), (int)The.ZoneManager.GetZoneProperty(zoneID, "TeleportGateIncomingY"));
		}
		Cell cell = zone.GetEmptyCells().GetRandomElement() ?? zone.GetCells().GetRandomElement();
		if (cell != null)
		{
			return GlobalLocation.FromZoneId(zoneID, cell.X, cell.Y);
		}
		return GlobalLocation.FromZoneId(zoneID, zone.Width / 2, zone.Height / 2);
	}
}
