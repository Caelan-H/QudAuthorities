using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;
using XRL.World.ZoneBuilders;

namespace XRL.World.Parts;

[Serializable]
public class TemplarPhylactery : IPoweredPart, IHackingSifrahHandler
{
	public string wraithID;

	public GameObject wraith;

	public TemplarPhylactery()
	{
		ChargeUse = 1;
		WorksOnHolder = true;
		MustBeUnderstood = true;
		NameForStatus = "ContinuityEngine";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != CheckExistenceSupportEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == UnequippedEvent.ID;
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

	public override bool HandleEvent(UnequippedEvent E)
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
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!IsHacked() && E.Actor.IsPlayer() && Options.SifrahHacking)
		{
			E.AddAction("Hack", "hack", "HackTemplarPhylactery", null, 'h');
		}
		if (ParentObject.Equipped != null)
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
			if (!E.Actor.CanMoveExtremities("Activate", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(ParentObject.Does("are") + " unresponsive.");
				}
				return false;
			}
			Spawn();
			E.RequestInterfaceExit();
		}
		else if (E.Command == "DeactivateTemplarPhylactery")
		{
			Despawn();
			E.RequestInterfaceExit();
		}
		else if (E.Command == "HackTemplarPhylactery" && !IsHacked() && E.Actor.IsPlayer() && Options.SifrahHacking)
		{
			int techTier = ParentObject.GetTechTier();
			HackingSifrah hackingSifrah = new HackingSifrah(ParentObject, techTier, techTier, E.Actor.Stat("Intelligence"));
			hackingSifrah.HandlerID = ParentObject.id;
			hackingSifrah.HandlerPartName = GetType().Name;
			hackingSifrah.Play(ParentObject);
			E.Actor.UseEnergy(1000, "Sifrah Hack TemplarPhylactery");
			if (!hackingSifrah.InterfaceExitRequested)
			{
				Statistic energy = E.Actor.Energy;
				if (energy == null || energy.Value >= 0)
				{
					goto IL_01a6;
				}
			}
			E.RequestInterfaceExit();
		}
		goto IL_01a6;
		IL_01a6:
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (E.Context != "Sample" && E.Context != "Initialization" && wraithID == null && E.ReplacementObject == null)
		{
			GameObject gameObject = HeroMaker.MakeHero(GameObject.create("Wraith-Knight Templar"), null, "SpecialFactionHeroTemplate_TemplarWraith");
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
			wraithID = The.ZoneManager?.CacheObject(gameObject);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		Object.RegisterPartEvent(this, "AdjustWeaponScore");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveItemList")
		{
			if (wraith == null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && WantsToUse(E.GetGameObjectParameter("User")))
			{
				E.AddAICommand("ActivateTemplarPhylactery", 100, ParentObject, Inv: true);
			}
		}
		else if (E.ID == "AdjustWeaponScore" && WantsToUse(E.GetGameObjectParameter("User")))
		{
			E.ModParameter("Score", 100);
		}
		return base.FireEvent(E);
	}

	public bool WantsToUse(GameObject Actor)
	{
		if (Actor != null && Actor.Stat("Intelligence") > 22)
		{
			if (wraithID != null)
			{
				return !The.ZoneManager.peekCachedObject(wraithID).IsHostileTowards(Actor);
			}
			return false;
		}
		return true;
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
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null || wraithID == null)
		{
			return;
		}
		Cell cell = equipped.GetCurrentCell();
		if (cell == null)
		{
			return;
		}
		cell = cell.GetEmptyAdjacentCells(1, 5).GetRandomElement();
		if (cell != null)
		{
			wraith = The.ZoneManager.peekCachedObject(wraithID).DeepCopy(CopyEffects: false, CopyID: true);
			Temporary.AddHierarchically(wraith, 0, null, ParentObject);
			if (IsHacked() || (!equipped.IsPlayerControlled() && equipped.HasProperty("PsychicHunter") && equipped.HasPart("Extradimensional")))
			{
				wraith.TakeOnAttitudesOf(equipped, CopyLeader: false, CopyTarget: true);
			}
			wraith.MakeActive();
			cell.AddObject(wraith);
			wraith.TeleportSwirl(null, "&B");
			IComponent<GameObject>.XDidYToZ(equipped, "activate", wraith);
			IComponent<GameObject>.XDidY(wraith, "appear");
		}
	}

	public bool IsHacked()
	{
		return ParentObject.GetIntProperty("SifrahTemplarPhylacteryHack") > 0;
	}

	public void HackingResultSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", 1);
		if (ParentObject.GetIntProperty("SifrahTemplarPhylacteryHack", 1) > 0)
		{
			ChargeUse = 100;
			if (who.IsPlayer())
			{
				Popup.Show("You hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			}
		}
		else if (who.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
		}
	}

	public void HackingResultExceptionalSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.SetIntProperty("SifrahTemplarPhylacteryHack", 1);
			ChargeUse = 10;
			if (who.IsPlayer())
			{
				Popup.Show("You hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ", and find a way to reduce " + obj.its + " power consumption in the process!");
			}
		}
	}

	public void HackingResultPartialSuccess(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject && who.IsPlayer())
		{
			Popup.Show("You feel like you're making progress on hacking " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
		}
	}

	public void HackingResultFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj == ParentObject)
		{
			ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", -1);
			if (who.IsPlayer())
			{
				Popup.Show("You cannot seem to work out how to hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			}
		}
	}

	public void HackingResultCriticalFailure(GameObject who, GameObject obj, HackingSifrah game)
	{
		if (obj != ParentObject)
		{
			return;
		}
		ParentObject.ModIntProperty("SifrahTemplarPhylacteryHack", -1);
		if (who.HasPart("Dystechnia"))
		{
			Dystechnia.CauseExplosion(ParentObject, who);
			game.RequestInterfaceExit();
			return;
		}
		if (who.IsPlayer())
		{
			Popup.Show("Your attempt to hack " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " has gone very wrong.");
		}
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = who.CurrentCell;
		ParentObject.UseCharge(Stat.Random(5000, 15000), LiveOnly: false, 0L);
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".Roll(), "2d4");
	}
}
