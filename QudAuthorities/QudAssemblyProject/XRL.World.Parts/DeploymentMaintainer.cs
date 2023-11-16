using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class DeploymentMaintainer : IPoweredPart
{
	public int Radius = 1;

	public int Chance = 100;

	public int AtLeast;

	public int MaintenanceInterval = 10;

	public string Duration;

	public string Blueprint = "Forcefield";

	public string UsabilityEvent;

	public string AccessibilityEvent;

	public bool ActiveMaintenance = true;

	public bool RealRadius;

	public bool BlockedBySolid = true;

	public bool BlockedByNonEmpty = true;

	public bool Seeping;

	public bool DustPuffEach;

	public bool NoXPValue = true;

	public bool LinkRealityStabilization;

	[NonSerialized]
	private Dictionary<int, GameObject> Deployed = new Dictionary<int, GameObject>();

	[NonSerialized]
	private Cell LastCell;

	[NonSerialized]
	private int MaintenanceCountdown;

	[NonSerialized]
	private List<Cell> AdjacentCells;

	[NonSerialized]
	private bool AdjacentCellsShuffled;

	public DeploymentMaintainer()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		DeploymentMaintainer deploymentMaintainer = p as DeploymentMaintainer;
		if (deploymentMaintainer.Radius != Radius)
		{
			return false;
		}
		if (deploymentMaintainer.Chance != Chance)
		{
			return false;
		}
		if (deploymentMaintainer.AtLeast != AtLeast)
		{
			return false;
		}
		if (deploymentMaintainer.MaintenanceInterval != MaintenanceInterval)
		{
			return false;
		}
		if (deploymentMaintainer.Duration != Duration)
		{
			return false;
		}
		if (deploymentMaintainer.Blueprint != Blueprint)
		{
			return false;
		}
		if (deploymentMaintainer.UsabilityEvent != UsabilityEvent)
		{
			return false;
		}
		if (deploymentMaintainer.AccessibilityEvent != AccessibilityEvent)
		{
			return false;
		}
		if (deploymentMaintainer.ActiveMaintenance != ActiveMaintenance)
		{
			return false;
		}
		if (deploymentMaintainer.RealRadius != RealRadius)
		{
			return false;
		}
		if (deploymentMaintainer.BlockedBySolid != BlockedBySolid)
		{
			return false;
		}
		if (deploymentMaintainer.BlockedByNonEmpty != BlockedByNonEmpty)
		{
			return false;
		}
		if (deploymentMaintainer.Seeping != Seeping)
		{
			return false;
		}
		if (deploymentMaintainer.DustPuffEach != DustPuffEach)
		{
			return false;
		}
		if (deploymentMaintainer.NoXPValue != NoXPValue)
		{
			return false;
		}
		if (deploymentMaintainer.LinkRealityStabilization != LinkRealityStabilization)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CheckExistenceSupportEvent.ID && ID != AddedToInventoryEvent.ID && ID != EndTurnEvent.ID && ID != EnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			if (ID == PowerSwitchFlippedEvent.ID)
			{
				return IsPowerSwitchSensitive;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (Deployed.ContainsValue(E.Object) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		MaintainDeployment();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		MaintainDeployment();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		MaintainDeployment();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (ActiveMaintenance)
		{
			TeardownDeployment();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		if (IsPowerSwitchSensitive)
		{
			MaintenanceCountdown = 0;
			MaintainDeployment();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool CanDeploy(Cell C, Cell CC, Event Check, Dictionary<int, bool> Track = null)
	{
		if (C == null || CC == null)
		{
			return false;
		}
		if (RealRadius && C != CC && C.RealDistanceTo(CC) > (double)Radius)
		{
			return false;
		}
		int localCoordKey = C.LocalCoordKey;
		if (Track != null)
		{
			Track[localCoordKey] = true;
		}
		if (Deployed.ContainsKey(localCoordKey))
		{
			GameObject obj = Deployed[localCoordKey];
			if (GameObject.validate(ref obj) && !obj.IsInGraveyard())
			{
				return false;
			}
		}
		foreach (GameObject @object in C.Objects)
		{
			if (@object.Blueprint == Blueprint)
			{
				return false;
			}
		}
		if (!Chance.in100())
		{
			return false;
		}
		if (BlockedBySolid && C.IsSolid(Seeping))
		{
			return false;
		}
		if (BlockedByNonEmpty && !C.IsEmpty())
		{
			return false;
		}
		if (Check != null && !C.FireEvent(Check))
		{
			return false;
		}
		return true;
	}

	private void Deploy(Cell C)
	{
		GameObject gameObject = GameObject.create(Blueprint);
		if (LinkRealityStabilization && gameObject.GetPart("RealityStabilization") is RealityStabilization realityStabilization)
		{
			realityStabilization.Strength = 0;
			realityStabilization.FromSource = ParentObject;
		}
		if (!string.IsNullOrEmpty(Duration))
		{
			gameObject.RequirePart<Temporary>().Duration = Duration.RollCached();
		}
		if (ActiveMaintenance)
		{
			gameObject.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
		}
		C.AddObject(gameObject);
		gameObject.MakeActive();
		if (NoXPValue && gameObject.HasStat("XPValue"))
		{
			gameObject.Statistics["XPValue"].BaseValue = 0;
		}
		if (DustPuffEach)
		{
			gameObject.DustPuff();
		}
		Deployed[C.LocalCoordKey] = gameObject;
	}

	private void CleanDeployment()
	{
		int num = 0;
		foreach (KeyValuePair<int, GameObject> item in Deployed)
		{
			GameObject obj = item.Value;
			if (!GameObject.validate(ref obj))
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		if (num >= Deployed.Count)
		{
			TeardownDeployment();
			return;
		}
		List<int> list = new List<int>(num);
		foreach (KeyValuePair<int, GameObject> item2 in Deployed)
		{
			int key = item2.Key;
			GameObject obj2 = item2.Value;
			if (!GameObject.validate(ref obj2))
			{
				list.Add(key);
			}
		}
		foreach (int item3 in list)
		{
			Deployed.Remove(item3);
		}
	}

	private void ValidateDeployment(Dictionary<int, bool> Track)
	{
		int num = 0;
		foreach (KeyValuePair<int, GameObject> item in Deployed)
		{
			if (!Track.ContainsKey(item.Key))
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		if (num >= Deployed.Count)
		{
			TeardownDeployment();
			return;
		}
		List<KeyValuePair<int, GameObject>> list = new List<KeyValuePair<int, GameObject>>(num);
		foreach (KeyValuePair<int, GameObject> item2 in Deployed)
		{
			list.Add(item2);
		}
		foreach (KeyValuePair<int, GameObject> item3 in list)
		{
			int key = item3.Key;
			GameObject obj = item3.Value;
			if (GameObject.validate(ref obj))
			{
				obj.Obliterate();
			}
			Deployed.Remove(key);
		}
	}

	private void TeardownDeployment()
	{
		foreach (KeyValuePair<int, GameObject> item in Deployed)
		{
			GameObject obj = item.Value;
			if (GameObject.validate(ref obj))
			{
				obj.Obliterate();
			}
		}
		if (Deployed.Count > 0)
		{
			Deployed.Clear();
		}
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return true;
		}
		if (UsabilityEvent != null && !cell.FireEvent(UsabilityEvent))
		{
			return true;
		}
		return false;
	}

	public bool MaintainDeployment()
	{
		Cell cell = ParentObject.CurrentCell;
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActiveMaintenance)
			{
				TeardownDeployment();
			}
			LastCell = cell;
			return false;
		}
		if (MaintenanceCountdown > 0)
		{
			MaintenanceCountdown--;
			return false;
		}
		if (MaintenanceInterval > 0)
		{
			MaintenanceCountdown = MaintenanceInterval;
		}
		CleanDeployment();
		Event check = ((AccessibilityEvent != null) ? Event.New(AccessibilityEvent) : null);
		if (AdjacentCells == null || LastCell != cell)
		{
			AdjacentCells = ((Radius == -1) ? cell.ParentZone.GetCells() : cell.GetLocalAdjacentCells(Radius));
			AdjacentCellsShuffled = false;
		}
		Dictionary<int, bool> dictionary = ((ActiveMaintenance && LastCell != cell) ? new Dictionary<int, bool>(AdjacentCells.Count) : null);
		int num = 0;
		do
		{
			if (num == 1 && !AdjacentCellsShuffled)
			{
				if (Radius == -1)
				{
					AdjacentCells = new List<Cell>(AdjacentCells);
				}
				AdjacentCells.ShuffleInPlace();
				AdjacentCellsShuffled = true;
			}
			if (CanDeploy(cell, cell, check, dictionary))
			{
				Deploy(cell);
			}
			foreach (Cell adjacentCell in AdjacentCells)
			{
				if (num > 0 && Deployed.Count >= AtLeast)
				{
					break;
				}
				if (CanDeploy(adjacentCell, cell, check, dictionary))
				{
					Deploy(adjacentCell);
				}
			}
		}
		while (AtLeast > 0 && Deployed.Count < AtLeast && ++num < 10);
		if (ActiveMaintenance && dictionary != null)
		{
			ValidateDeployment(dictionary);
		}
		LastCell = cell;
		return true;
	}
}
