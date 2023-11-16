using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class PetPhylactery : IPoweredPart
{
	public string wraithBlueprint = "PetWraith";

	public string wraithID;

	public GameObject wraith;

	public string spawnerID;

	public PetPhylactery()
	{
		ChargeUse = 1;
		WorksOnHolder = true;
		WorksOnCarrier = true;
		WorksOnEquipper = true;
		MustBeUnderstood = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != CheckExistenceSupportEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (E.Object == wraith && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Despawn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (GameObject.validate(ref wraith) && IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Despawn();
		}
		GameObject objectContext = ParentObject.GetObjectContext();
		if (objectContext == null)
		{
			Despawn();
		}
		else if (objectContext.id != spawnerID)
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.GetObjectContext() != null)
		{
			if (GameObject.validate(ref wraith))
			{
				E.AddAction("Deactivate", "deactivate", "DeactivateTemplarPhylactery", null, 'a');
			}
			else
			{
				E.AddAction("Activate", "activate", "ActivateTemplarPhylactery", null, 'a');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTemplarPhylactery")
		{
			Spawn();
		}
		else if (E.Command == "DeactivateTemplarPhylactery")
		{
			Despawn();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.Context != "Sample" && E.ReplacementObject == null && wraithID == null)
		{
			GameObject gameObject = GameObject.create(wraithBlueprint);
			gameObject.RequirePart<HologramMaterial>();
			gameObject.RequirePart<HologramInvulnerability>();
			gameObject.RequirePart<Unreplicable>();
			gameObject.ModIntProperty("IgnoresWalls", 1);
			foreach (GameObject item in gameObject.GetInventoryAndEquipment())
			{
				if (item.HasPropertyOrTag("MeleeWeapon"))
				{
					item.AddPart(new ModPsionic());
				}
				else
				{
					item.Obliterate();
				}
			}
			ParentObject.pRender.DisplayName = "phylactery of " + gameObject.ShortDisplayNameWithoutEpithet;
			if (The.ZoneManager != null)
			{
				wraithID = The.ZoneManager.CacheObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveItemList" && wraith == null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddAICommand("ActivateTemplarPhylactery", 100, ParentObject, Inv: true);
		}
		return base.FireEvent(E);
	}

	public void Despawn()
	{
		if (GameObject.validate(ref wraith))
		{
			wraith.Splatter("&M-");
			wraith.Splatter("&M.");
			wraith.Splatter("&M/");
			IComponent<GameObject>.XDidY(wraith, "disappear", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			wraith.Obliterate();
			wraith = null;
		}
	}

	public void Spawn()
	{
		Despawn();
		GameObject objectContext = ParentObject.GetObjectContext();
		if (objectContext == null)
		{
			return;
		}
		Cell cell = objectContext.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		cell = cell.GetEmptyAdjacentCells(1, 5).GetRandomElement();
		if (cell != null)
		{
			bool takeOnAttitudesOfLeader = false;
			if (!objectContext.IsPlayerControlled() && objectContext.HasProperty("PsychicHunter") && objectContext.HasPart("Extradimensional"))
			{
				takeOnAttitudesOfLeader = true;
			}
			spawnerID = objectContext.id;
			wraith = XRLCore.Core.Game.ZoneManager.peekCachedObject(wraithID).DeepCopy(CopyEffects: false, CopyID: true);
			Temporary.AddHierarchically(wraith, 0, null, ParentObject);
			wraith.MakeActive();
			cell.AddObject(wraith);
			wraith.TeleportSwirl(null, "&B");
			IComponent<GameObject>.XDidYToZ(objectContext, "activate", wraith);
			IComponent<GameObject>.XDidY(wraith, "appear");
			wraith.SetPartyLeader(objectContext, takeOnAttitudesOfLeader, trifling: false, copyTargetWithAttitudes: true);
		}
	}
}
