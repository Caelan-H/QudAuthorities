using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Engulfing : IPoweredPart
{
	public int AVBonus;

	public int DVPenalty;

	public int EnterSaveTarget;

	public int ExitSaveTarget;

	public int DamageChance;

	public int EnterDamageChance;

	public int ExitDamageChance;

	public int DamageBloodSplatterChance = 50;

	public int PeriodicEventTurns1;

	public int PeriodicEventTurns2;

	public int PeriodicEventTurns3;

	public string _AffectedProperties;

	public string EnterSaveStat = "Agility";

	public string ExitSaveStat = "Strength";

	public string Damage;

	public string DamageAttributes;

	public string EnterEventSelf;

	public string EnterEventUser;

	public string ExitEventSelf;

	public string ExitEventUser;

	public string ApplyChangesEventSelf;

	public string ApplyChangesEventUser;

	public string UnapplyChangesEventSelf;

	public string UnapplyChangesEventUser;

	public string EffectDescriptionPrefix;

	public string EffectDescriptionPostfix;

	public string PeriodicEvent1;

	public string PeriodicEvent2;

	public string PeriodicEvent3;

	public bool NoDamageWhenDisabled;

	public bool EnterDamageFailOnly;

	public bool ExitDamageFailOnly;

	public GameObject Engulfed;

	public bool Pull;

	public Guid ActivatedAbilityID;

	private Dictionary<string, int> _PropertyMap;

	public string AffectedProperties
	{
		get
		{
			return _AffectedProperties;
		}
		set
		{
			_AffectedProperties = value;
			_PropertyMap = null;
		}
	}

	public Dictionary<string, int> PropertyMap
	{
		get
		{
			if (_PropertyMap == null)
			{
				_PropertyMap = IComponent<GameObject>.MapFromString(_AffectedProperties);
			}
			return _PropertyMap;
		}
	}

	public Engulfing()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
		base.IsBioScannable = true;
	}

	public override bool SameAs(IPart p)
	{
		Engulfing engulfing = p as Engulfing;
		if (engulfing.AVBonus != AVBonus)
		{
			return false;
		}
		if (engulfing.DVPenalty != DVPenalty)
		{
			return false;
		}
		if (engulfing.EnterSaveTarget != EnterSaveTarget)
		{
			return false;
		}
		if (engulfing.ExitSaveTarget != ExitSaveTarget)
		{
			return false;
		}
		if (engulfing.DamageChance != DamageChance)
		{
			return false;
		}
		if (engulfing.EnterDamageChance != EnterDamageChance)
		{
			return false;
		}
		if (engulfing.ExitDamageChance != ExitDamageChance)
		{
			return false;
		}
		if (engulfing.DamageBloodSplatterChance != DamageBloodSplatterChance)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns1 != PeriodicEventTurns1)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns2 != PeriodicEventTurns2)
		{
			return false;
		}
		if (engulfing.PeriodicEventTurns3 != PeriodicEventTurns3)
		{
			return false;
		}
		if (engulfing.AffectedProperties != AffectedProperties)
		{
			return false;
		}
		if (engulfing.EnterSaveStat != EnterSaveStat)
		{
			return false;
		}
		if (engulfing.ExitSaveStat != ExitSaveStat)
		{
			return false;
		}
		if (engulfing.Damage != Damage)
		{
			return false;
		}
		if (engulfing.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (engulfing.EnterEventSelf != EnterEventSelf)
		{
			return false;
		}
		if (engulfing.EnterEventUser != EnterEventUser)
		{
			return false;
		}
		if (engulfing.ExitEventSelf != ExitEventSelf)
		{
			return false;
		}
		if (engulfing.ExitEventUser != ExitEventUser)
		{
			return false;
		}
		if (engulfing.ApplyChangesEventSelf != ApplyChangesEventSelf)
		{
			return false;
		}
		if (engulfing.ApplyChangesEventUser != ApplyChangesEventUser)
		{
			return false;
		}
		if (engulfing.UnapplyChangesEventSelf != UnapplyChangesEventSelf)
		{
			return false;
		}
		if (engulfing.UnapplyChangesEventUser != UnapplyChangesEventUser)
		{
			return false;
		}
		if (engulfing.EffectDescriptionPrefix != EffectDescriptionPrefix)
		{
			return false;
		}
		if (engulfing.EffectDescriptionPostfix != EffectDescriptionPostfix)
		{
			return false;
		}
		if (engulfing.PeriodicEvent1 != PeriodicEvent1)
		{
			return false;
		}
		if (engulfing.PeriodicEvent2 != PeriodicEvent2)
		{
			return false;
		}
		if (engulfing.PeriodicEvent3 != PeriodicEvent3)
		{
			return false;
		}
		if (engulfing.NoDamageWhenDisabled != NoDamageWhenDisabled)
		{
			return false;
		}
		if (engulfing.EnterDamageFailOnly != EnterDamageFailOnly)
		{
			return false;
		}
		if (engulfing.ExitDamageFailOnly != ExitDamageFailOnly)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		EndAllEngulfment();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandEngulf");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "BeginBeingTaken");
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "ObjectEnteredAdjacentCell");
		base.Register(Object);
	}

	public override void Initialize()
	{
		base.Initialize();
		ActivatedAbilityID = AddMyActivatedAbility("Engulf", "CommandEngulf", "Physical Mutation", null, "@");
	}

	public bool PerformDamage(GameObject who)
	{
		if (string.IsNullOrEmpty(Damage))
		{
			return false;
		}
		bool num = who.TakeDamage(Damage.RollCached(), "from %O!", DamageAttributes, null, null, ParentObject);
		if (num && DamageBloodSplatterChance.in100())
		{
			who.Bloodsplatter();
		}
		return num;
	}

	public bool CheckEnterDamage(GameObject who, bool Failed)
	{
		if (EnterDamageChance <= 0)
		{
			return false;
		}
		if (EnterDamageFailOnly && !Failed)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!EnterDamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public bool CheckExitDamage(GameObject who, bool Failed)
	{
		if (ExitDamageChance <= 0)
		{
			return false;
		}
		if (ExitDamageFailOnly && !Failed)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!ExitDamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public bool CheckPeriodicDamage(GameObject who)
	{
		if (DamageChance <= 0)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (!DamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public override bool Render(RenderEvent E)
	{
		if (GameObject.validate(ref Engulfed))
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= 31 && num <= 60)
			{
				E.ColorString = Engulfed.pRender.ColorString;
				E.DetailColor = Engulfed.pRender.DetailColor;
				E.Tile = Engulfed.pRender.Tile;
				E.RenderString = Engulfed.pRender.RenderString;
			}
		}
		return base.Render(E);
	}

	public bool Engulf(GameObject who, Event E = null)
	{
		if (CheckEngulfed())
		{
			EndEngulfment(Engulfed);
			if (Engulfed != null)
			{
				return false;
			}
		}
		if (who.SameAs(ParentObject))
		{
			return false;
		}
		if (!who.CanChangeMovementMode("Engulfed", ShowMessage: false, Involuntary: true) || !who.CanChangeBodyPosition("Engulfed", ShowMessage: false, Involuntary: true))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You cannot do that while " + who.the + who.ShortDisplayName + who.Is + " in " + who.its + " present situation.");
			}
			return false;
		}
		if (!who.PhaseAndFlightMatches(ParentObject))
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (EnterSaveTarget > 0 && who.MakeSave(EnterSaveStat, EnterSaveTarget, ParentObject, "Strength", "Engulfment"))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowFail("You fail to engulf " + who.the + who.ShortDisplayName + ".");
			}
			else if (who.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("try") + " to engulf you, but" + ParentObject.GetVerb("fail") + ".");
			}
			else if (IComponent<GameObject>.Visible(ParentObject))
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("try") + " to engulf " + who.the + who.ShortDisplayName + ", but" + ParentObject.GetVerb("fail") + ".");
			}
			CheckEnterDamage(who, Failed: true);
			ParentObject.UseEnergy(1000, "Position");
			E?.RequestInterfaceExit();
			return false;
		}
		if (Pull)
		{
			if (who.CurrentCell != ParentObject.CurrentCell && !who.DirectMoveTo(ParentObject.CurrentCell, 0, forced: false, ignoreCombat: true))
			{
				E?.RequestInterfaceExit();
				return false;
			}
		}
		else if (who.CurrentCell != cell && !ParentObject.DirectMoveTo(who.CurrentCell, 0, forced: false, ignoreCombat: true))
		{
			E?.RequestInterfaceExit();
			return false;
		}
		CheckEnterDamage(who, Failed: false);
		IComponent<GameObject>.XDidYToZ(who, "are", "engulfed by", ParentObject, null, null, null, null, who, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		if (!string.IsNullOrEmpty(EnterEventSelf))
		{
			ParentObject.FireEvent(EnterEventSelf);
		}
		if (!string.IsNullOrEmpty(EnterEventUser))
		{
			who.FireEvent(EnterEventUser);
		}
		Engulfed = who;
		who.ApplyEffect(new Engulfed(ParentObject));
		ParentObject.UseEnergy(1000, "Position");
		ParentObject.FireEvent(Event.New("Engulfed", "Object", who));
		who.FireEvent(Event.New("ObjectEngulfed", "Object", ParentObject));
		E?.RequestInterfaceExit();
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (CheckEngulfed() && !ParentObject.pBrain.HasGoal("FleeLocation"))
			{
				ParentObject.pBrain.Goals.Clear();
			}
		}
		else if (E.ID == "ObjectEnteredAdjacentCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && ParentObject.IsHostileTowards(gameObjectParameter) && CheckEngulfed() && !gameObjectParameter.MakeSave("Strength", 20, null, null, "Engulfment"))
			{
				Engulf(gameObjectParameter);
			}
		}
		else if (E.ID == "EnteredCell")
		{
			if (CheckEngulfed())
			{
				if (ParentObject.CurrentCell != null)
				{
					if (Engulfed.CurrentCell != null && Engulfed.CurrentCell != ParentObject.CurrentCell)
					{
						Engulfed.CurrentCell.RemoveObject(Engulfed);
					}
					if (!ParentObject.CurrentCell.Objects.Contains(Engulfed))
					{
						ParentObject.CurrentCell.AddObject(Engulfed);
					}
				}
				Engulfed.FireEvent(Event.New("EngulfDragged", "Object", ParentObject));
				ParentObject.FireEvent(Event.New("EngulferDragged", "Object", Engulfed));
			}
		}
		else if (E.ID == "BeginBeingTaken")
		{
			EndAllEngulfment();
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (!CheckEngulfed() && E.GetIntParameter("Distance") == 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Target");
				if (gameObjectParameter2.IsCombatObject(NoBrainOnly: true) && gameObjectParameter2.PhaseAndFlightMatches(ParentObject))
				{
					E.AddAICommand("CommandEngulf");
				}
			}
		}
		else if (E.ID == "CommandEngulf")
		{
			Cell cell = PickDirection();
			if (cell != null)
			{
				GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, AllowInanimate: false);
				if (combatTarget == null || combatTarget == ParentObject)
				{
					return false;
				}
				if (!Engulf(combatTarget))
				{
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}

	public bool EndEngulfment(GameObject who, Event E = null, Engulfed enc = null)
	{
		if (who == null)
		{
			Engulfed = null;
			return true;
		}
		if (enc == null)
		{
			enc = who.GetEffect("Engulfed") as Engulfed;
			if (enc == null)
			{
				Engulfed = null;
				return true;
			}
		}
		if (enc.EngulfedBy != ParentObject)
		{
			Engulfed = null;
			return true;
		}
		CheckExitDamage(who, Failed: true);
		if (!string.IsNullOrEmpty(ExitEventSelf))
		{
			ParentObject.FireEvent(ExitEventSelf);
		}
		if (!string.IsNullOrEmpty(ExitEventUser))
		{
			who.FireEvent(ExitEventUser);
		}
		who.RemoveEffect(enc);
		who.UseEnergy(1000, "Position");
		who.FireEvent(Event.New("Exited", "Object", ParentObject));
		ParentObject.FireEvent(Event.New("ObjectExited", "Object", who));
		E?.RequestInterfaceExit();
		Engulfed = null;
		return true;
	}

	public void EndAllEngulfment()
	{
		ParentObject.CurrentCell?.ForeachObject(delegate(GameObject obj)
		{
			if (obj.GetEffect("Engulfed") is Engulfed engulfed && engulfed.EngulfedBy == ParentObject)
			{
				obj.RemoveEffect(engulfed);
			}
		});
	}

	public void ProcessTurnEngulfed(GameObject who, int TurnsEngulfed)
	{
		bool flag = IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		who.FireEvent(Event.New("StartTurnEngulfed", "Object", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("StartTurnEngulfing", "Object", who, "Disabled", flag ? 1 : 0));
		CheckPeriodicDamage(who);
		if (!flag)
		{
			if (!string.IsNullOrEmpty(PeriodicEvent1) && PeriodicEventTurns1 > 0 && TurnsEngulfed % PeriodicEventTurns1 == 0)
			{
				who.FireEvent(PeriodicEvent1);
			}
			if (!string.IsNullOrEmpty(PeriodicEvent2) && PeriodicEventTurns2 > 0 && TurnsEngulfed % PeriodicEventTurns2 == 0)
			{
				who.FireEvent(PeriodicEvent2);
			}
			if (!string.IsNullOrEmpty(PeriodicEvent3) && PeriodicEventTurns3 > 0 && TurnsEngulfed % PeriodicEventTurns3 == 0)
			{
				who.FireEvent(PeriodicEvent3);
			}
		}
		who.FireEvent(Event.New("EndTurnEngulfed", "Object", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("EndTurnEngulfing", "Object", who, "Disabled", flag ? 1 : 0));
	}

	public bool IsOurEffect(Effect FX)
	{
		if (FX is Engulfed engulfed)
		{
			return engulfed.EngulfedBy == ParentObject;
		}
		return false;
	}

	public bool CheckEngulfed()
	{
		if (GameObject.validate(ref Engulfed) && !Engulfed.HasEffect("Engulfed", IsOurEffect))
		{
			Engulfed = null;
		}
		return Engulfed != null;
	}
}
