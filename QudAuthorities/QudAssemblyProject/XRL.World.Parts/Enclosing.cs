using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Enclosing : IPoweredPart
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

	public int PeriodicEventLevel1;

	public int PeriodicEventLevel2;

	public int PeriodicEventLevel3;

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

	public string RequiresLiquid;

	public string PeriodicEvent1;

	public string PeriodicEvent2;

	public string PeriodicEvent3;

	public string OpenTile;

	public string OpenRenderString;

	public int OpenLayer = int.MinValue;

	public string ClosedTile;

	public string ClosedRenderString;

	public int ClosedLayer = int.MinValue;

	public bool NoDamageWhenDisabled;

	public bool EnterDamageFailOnly;

	public bool ExitDamageFailOnly;

	public bool PeriodicEventSendSource1;

	public bool PeriodicEventSendSource2;

	public bool PeriodicEventSendSource3;

	[FieldSaveVersion(230)]
	public bool PeriodicEventOnSelf1;

	[FieldSaveVersion(230)]
	public bool PeriodicEventOnSelf2;

	[FieldSaveVersion(230)]
	public bool PeriodicEventOnSelf3;

	[FieldSaveVersion(230)]
	public bool PeriodicEventUseGenericNotify1;

	[FieldSaveVersion(230)]
	public bool PeriodicEventUseGenericNotify2;

	[FieldSaveVersion(230)]
	public bool PeriodicEventUseGenericNotify3;

	[FieldSaveVersion(230)]
	public bool ShowGeneralInfoInShortDescription = true;

	public string OpenColor;

	public string ClosedColor;

	public string OpenTileColor;

	public string ClosedTileColor;

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

	public Enclosing()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Enclosing enclosing = p as Enclosing;
		if (enclosing.AVBonus != AVBonus)
		{
			return false;
		}
		if (enclosing.DVPenalty != DVPenalty)
		{
			return false;
		}
		if (enclosing.EnterSaveTarget != EnterSaveTarget)
		{
			return false;
		}
		if (enclosing.ExitSaveTarget != ExitSaveTarget)
		{
			return false;
		}
		if (enclosing.DamageChance != DamageChance)
		{
			return false;
		}
		if (enclosing.EnterDamageChance != EnterDamageChance)
		{
			return false;
		}
		if (enclosing.ExitDamageChance != ExitDamageChance)
		{
			return false;
		}
		if (enclosing.DamageBloodSplatterChance != DamageBloodSplatterChance)
		{
			return false;
		}
		if (enclosing.PeriodicEventTurns1 != PeriodicEventTurns1)
		{
			return false;
		}
		if (enclosing.PeriodicEventTurns2 != PeriodicEventTurns2)
		{
			return false;
		}
		if (enclosing.PeriodicEventTurns3 != PeriodicEventTurns3)
		{
			return false;
		}
		if (enclosing.PeriodicEventLevel1 != PeriodicEventLevel1)
		{
			return false;
		}
		if (enclosing.PeriodicEventLevel2 != PeriodicEventLevel2)
		{
			return false;
		}
		if (enclosing.PeriodicEventLevel3 != PeriodicEventLevel3)
		{
			return false;
		}
		if (enclosing.AffectedProperties != AffectedProperties)
		{
			return false;
		}
		if (enclosing.EnterSaveStat != EnterSaveStat)
		{
			return false;
		}
		if (enclosing.ExitSaveStat != ExitSaveStat)
		{
			return false;
		}
		if (enclosing.Damage != Damage)
		{
			return false;
		}
		if (enclosing.DamageAttributes != DamageAttributes)
		{
			return false;
		}
		if (enclosing.EnterEventSelf != EnterEventSelf)
		{
			return false;
		}
		if (enclosing.EnterEventUser != EnterEventUser)
		{
			return false;
		}
		if (enclosing.ExitEventSelf != ExitEventSelf)
		{
			return false;
		}
		if (enclosing.ExitEventUser != ExitEventUser)
		{
			return false;
		}
		if (enclosing.ApplyChangesEventSelf != ApplyChangesEventSelf)
		{
			return false;
		}
		if (enclosing.ApplyChangesEventUser != ApplyChangesEventUser)
		{
			return false;
		}
		if (enclosing.UnapplyChangesEventSelf != UnapplyChangesEventSelf)
		{
			return false;
		}
		if (enclosing.UnapplyChangesEventUser != UnapplyChangesEventUser)
		{
			return false;
		}
		if (enclosing.EffectDescriptionPrefix != EffectDescriptionPrefix)
		{
			return false;
		}
		if (enclosing.EffectDescriptionPostfix != EffectDescriptionPostfix)
		{
			return false;
		}
		if (enclosing.RequiresLiquid != RequiresLiquid)
		{
			return false;
		}
		if (enclosing.PeriodicEvent1 != PeriodicEvent1)
		{
			return false;
		}
		if (enclosing.PeriodicEvent2 != PeriodicEvent2)
		{
			return false;
		}
		if (enclosing.PeriodicEvent3 != PeriodicEvent3)
		{
			return false;
		}
		if (enclosing.NoDamageWhenDisabled != NoDamageWhenDisabled)
		{
			return false;
		}
		if (enclosing.EnterDamageFailOnly != EnterDamageFailOnly)
		{
			return false;
		}
		if (enclosing.ExitDamageFailOnly != ExitDamageFailOnly)
		{
			return false;
		}
		if (enclosing.PeriodicEventSendSource1 != PeriodicEventSendSource1)
		{
			return false;
		}
		if (enclosing.PeriodicEventSendSource2 != PeriodicEventSendSource2)
		{
			return false;
		}
		if (enclosing.PeriodicEventSendSource3 != PeriodicEventSendSource3)
		{
			return false;
		}
		if (enclosing.PeriodicEventOnSelf1 != PeriodicEventOnSelf1)
		{
			return false;
		}
		if (enclosing.PeriodicEventOnSelf2 != PeriodicEventOnSelf2)
		{
			return false;
		}
		if (enclosing.PeriodicEventOnSelf3 != PeriodicEventOnSelf3)
		{
			return false;
		}
		if (enclosing.PeriodicEventUseGenericNotify1 != PeriodicEventUseGenericNotify1)
		{
			return false;
		}
		if (enclosing.PeriodicEventUseGenericNotify2 != PeriodicEventUseGenericNotify2)
		{
			return false;
		}
		if (enclosing.PeriodicEventUseGenericNotify3 != PeriodicEventUseGenericNotify3)
		{
			return false;
		}
		if (enclosing.ShowGeneralInfoInShortDescription != ShowGeneralInfoInShortDescription)
		{
			return false;
		}
		if (enclosing.OpenColor != OpenColor)
		{
			return false;
		}
		if (enclosing.ClosedColor != ClosedColor)
		{
			return false;
		}
		if (enclosing.OpenTileColor != OpenTileColor)
		{
			return false;
		}
		if (enclosing.ClosedTileColor != ClosedTileColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AddedToInventoryEvent.ID && ID != BeforeDestroyObjectEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		DetachEffects((Cell)null, (IEvent)E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		DetachEffects(E.Cell, E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (IComponent<GameObject>.ThePlayer.GetEffect("Enclosed") is Enclosed enclosed && enclosed.EnclosedBy == ParentObject)
			{
				E.AddAction("Exit", "exit", "ExitEnclosing", null, 'e', FireOnActor: false, 15);
			}
			else
			{
				E.AddAction("Exit", "enter", "EnterEnclosing", null, 'e', FireOnActor: false, 15);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "EnterEnclosing")
		{
			EnterEnclosure(E.Actor, E);
		}
		else if (E.Command == "ExitEnclosing")
		{
			ExitEnclosure(E.Actor, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(EffectDescriptionPrefix))
		{
			E.Postfix.AppendRules(EffectDescriptionPrefix, base.AddStatusSummary);
		}
		if (ShowGeneralInfoInShortDescription)
		{
			if (EnterSaveTarget > 0)
			{
				if (!string.IsNullOrEmpty(EnterSaveStat))
				{
					E.Postfix.AppendRules("Entering may not succeed, and is a task dependent on " + EnterSaveStat + ".");
				}
				else
				{
					E.Postfix.AppendRules("Entering may not succeed.");
				}
			}
			if (!string.IsNullOrEmpty(Damage) && EnterDamageChance > 0)
			{
				if (EnterDamageChance >= 100)
				{
					if (EnterSaveTarget > 0)
					{
						if (EnterDamageFailOnly)
						{
							E.Postfix.AppendRules("Attempting to enter and failing inflicts damage.");
						}
						else
						{
							E.Postfix.AppendRules("Attempting to enter inflicts damage.");
						}
					}
					else
					{
						E.Postfix.AppendRules("Entering inflicts damage.");
					}
				}
				else if (EnterSaveTarget > 0)
				{
					if (EnterDamageFailOnly)
					{
						E.Postfix.AppendRules("Attempting to enter and failing may inflict damage.");
					}
					else
					{
						E.Postfix.AppendRules("Attempting to enter may inflict damage.");
					}
				}
				else
				{
					E.Postfix.AppendRules("Entering may inflict damage.");
				}
			}
			if (AVBonus != 0)
			{
				if (AVBonus > 0)
				{
					E.Postfix.AppendRules("+" + AVBonus + " AV to occupant.");
				}
				else
				{
					E.Postfix.AppendRules(AVBonus + " AV to occupant.");
				}
			}
			if (DVPenalty != 0)
			{
				if (DVPenalty > 0)
				{
					E.Postfix.AppendRules("-" + DVPenalty + " DV to occupant.");
				}
				else
				{
					E.Postfix.AppendRules("+" + -DVPenalty + " DV to occupant.");
				}
			}
			if (!string.IsNullOrEmpty(Damage) && DamageChance > 0)
			{
				if (DamageChance >= 100)
				{
					E.Postfix.AppendRules("Inflicts periodic damage to occupant.");
				}
				else
				{
					E.Postfix.AppendRules("May inflict periodic damage to occupant.");
				}
			}
			E.Postfix.AppendRules("Must spend a turn exiting before moving.");
			if (ExitSaveTarget > 0)
			{
				if (!string.IsNullOrEmpty(ExitSaveStat))
				{
					E.Postfix.AppendRules("Exiting may not succeed, and is a task dependent on " + ExitSaveStat + ".");
				}
				else
				{
					E.Postfix.AppendRules("Exiting may not succeed.");
				}
			}
			if (!string.IsNullOrEmpty(Damage) && ExitDamageChance > 0)
			{
				if (ExitDamageChance >= 100)
				{
					if (ExitSaveTarget > 0)
					{
						if (ExitDamageFailOnly)
						{
							E.Postfix.AppendRules("Attempting to exit and failing inflicts damage.");
						}
						else
						{
							E.Postfix.AppendRules("Attempting to exit inflicts damage.");
						}
					}
					else
					{
						E.Postfix.AppendRules("Exiting inflicts damage.");
					}
				}
				else if (ExitSaveTarget > 0)
				{
					if (ExitDamageFailOnly)
					{
						E.Postfix.AppendRules("Attempting to exit and failing may inflict damage.");
					}
					else
					{
						E.Postfix.AppendRules("Attempting to exit may inflict damage.");
					}
				}
				else
				{
					E.Postfix.AppendRules("Exiting may inflict damage.");
				}
			}
			if (!string.IsNullOrEmpty(RequiresLiquid))
			{
				E.Postfix.AppendRules("Must be completely filled with " + RequiresLiquid + " to be fully functional.");
			}
		}
		if (!string.IsNullOrEmpty(EffectDescriptionPostfix))
		{
			E.Postfix.AppendRules(EffectDescriptionPostfix);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		EndAllEnclosure();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginBeingTaken");
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUse");
		Object.RegisterPartEvent(this, "LeaveCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeaveCell" || E.ID == "BeginBeingTaken")
		{
			EndAllEnclosure();
		}
		else if (E.ID == "CanSmartUse")
		{
			if (ParentObject.Understood())
			{
				return false;
			}
		}
		else if (E.ID == "CommandSmartUse")
		{
			ParentObject.Twiddle();
		}
		return base.FireEvent(E);
	}

	private void EndAllEnclosure()
	{
		ParentObject.CurrentCell?.ForeachObject(delegate(GameObject obj)
		{
			if (obj.GetEffect("Enclosed") is Enclosed enclosed && enclosed.EnclosedBy == ParentObject)
			{
				obj.RemoveEffect(enclosed);
			}
		});
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(RequiresLiquid))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume == null)
			{
				return true;
			}
			if (liquidVolume.ComponentLiquids.Count != 1)
			{
				return true;
			}
			if (liquidVolume.Volume < liquidVolume.MaxVolume)
			{
				return true;
			}
			if (!liquidVolume.ComponentLiquids.ContainsKey(RequiresLiquid))
			{
				return true;
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
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

	public bool AnyEnterDamage()
	{
		if (EnterDamageChance <= 0)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return true;
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

	public bool AnyExitDamage()
	{
		if (ExitDamageChance <= 0)
		{
			return false;
		}
		if (NoDamageWhenDisabled && IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return true;
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
		if (DamageChance.in100())
		{
			return false;
		}
		return PerformDamage(who);
	}

	public bool EnterEnclosure(GameObject who, IEvent E = null)
	{
		if (ParentObject == who)
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot enter " + who.itself + ".");
			}
			return false;
		}
		if (ParentObject.HasTagOrProperty("Sealed"))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail(ParentObject.T() + ParentObject.Is + " sealed.");
			}
			return false;
		}
		if (who.HasEffect("Enclosed", (Effect FX) => (FX as Enclosed).EnclosedBy == ParentObject))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You are already in " + ParentObject.t() + ".");
			}
			return false;
		}
		if (!ParentObject.FireEvent("BeforeClosing"))
		{
			return false;
		}
		if (!who.CanChangeBodyPosition("Enclosed", ShowMessage: true))
		{
			return false;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (who.CurrentCell != cell && (!who.DirectMoveTo(cell) || who.CurrentCell != cell))
		{
			E?.RequestInterfaceExit();
			return false;
		}
		if (EnterSaveTarget > 0 && !who.MakeSave(EnterSaveStat, EnterSaveTarget, null, null, "Enclosure Entry Access", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			if (who.IsPlayer())
			{
				Popup.Show("You fail to get " + who.itself + " into " + ParentObject.t() + ".");
			}
			else if (IComponent<GameObject>.Visible(who))
			{
				IComponent<GameObject>.AddPlayerMessage(who.T() + who.GetVerb("try") + " to get " + ParentObject.itself + " into " + ParentObject.the + ParentObject.DisplayNameOnly + ", but" + who.GetVerb("fail") + ".");
			}
			CheckEnterDamage(who, Failed: true);
			who.UseEnergy(1000, "Position");
			E?.RequestInterfaceExit();
			return false;
		}
		CheckEnterDamage(who, Failed: false);
		IComponent<GameObject>.XDidYToZ(who, "enter", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, who.IsPlayer());
		if (!string.IsNullOrEmpty(EnterEventSelf))
		{
			ParentObject.FireEvent(EnterEventSelf);
		}
		if (!string.IsNullOrEmpty(EnterEventUser))
		{
			who.FireEvent(EnterEventUser);
		}
		who.ApplyEffect(new Enclosed(ParentObject));
		who.UseEnergy(1000, "Position");
		who.FireEvent(Event.New("Entered", "Object", ParentObject));
		ParentObject.FireEvent(Event.New("ObjectEntered", "Object", who));
		E?.RequestInterfaceExit();
		if (ClosedColor != null)
		{
			ParentObject.pRender.ColorString = ClosedColor;
		}
		if (ClosedTileColor != null)
		{
			ParentObject.pRender.TileColor = ClosedTileColor;
		}
		if (ClosedRenderString != null)
		{
			ParentObject.pRender.RenderString = ClosedRenderString;
		}
		if (ClosedTile != null)
		{
			ParentObject.pRender.Tile = ClosedTile;
		}
		if (ClosedLayer != int.MinValue)
		{
			ParentObject.pRender.RenderLayer = ClosedLayer;
		}
		ParentObject.FireEvent("SyncClosed");
		return true;
	}

	public bool ExitEnclosure(GameObject who, IEvent E = null, Enclosed enc = null)
	{
		if (enc == null)
		{
			enc = who.GetEffect("Enclosed") as Enclosed;
			if (enc == null)
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You are not enclosed.");
				}
				return false;
			}
		}
		if (enc.EnclosedBy != ParentObject)
		{
			if (who.IsPlayer())
			{
				Popup.Show("It is not " + ParentObject.t() + " that you are enclosed by.");
			}
			return false;
		}
		if (ExitSaveTarget > 0)
		{
			Event @event = E as Event;
			if ((@event == null || !@event.HasParameter("Teleporting")) && !who.MakeSave(ExitSaveStat, ExitSaveTarget, null, null, "Enclosure Exit Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				if (who.IsPlayer())
				{
					Popup.Show("You fail to extricate " + who.itself + " from " + ParentObject.the + ParentObject.DisplayNameOnly + "!");
				}
				else if (IComponent<GameObject>.Visible(who))
				{
					IComponent<GameObject>.AddPlayerMessage(who.T() + who.GetVerb("try") + " to extricate " + ParentObject.itself + " from " + ParentObject.t() + ", but" + who.GetVerb("fail") + "!");
				}
				bool num = CheckExitDamage(who, Failed: true);
				bool flag = false;
				if (@event == null || (!@event.HasParameter("Dragging") && !@event.HasParameter("Forced")))
				{
					who.UseEnergy(1000, "Position");
					flag = true;
				}
				if (num || flag)
				{
					E?.RequestInterfaceExit();
				}
				return false;
			}
		}
		CheckExitDamage(who, Failed: true);
		if (who.IsPlayer())
		{
			Popup.Show("You extricate " + who.itself + " from " + ParentObject.t() + ".");
		}
		else if (IComponent<GameObject>.Visible(who))
		{
			IComponent<GameObject>.AddPlayerMessage(who.T() + who.GetVerb("extricate") + " " + ParentObject.itself + " from " + ParentObject.t() + ".");
		}
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
		if (OpenColor != null)
		{
			ParentObject.pRender.ColorString = OpenColor;
		}
		if (OpenTileColor != null)
		{
			ParentObject.pRender.TileColor = OpenTileColor;
		}
		if (OpenTile != null)
		{
			ParentObject.pRender.Tile = OpenTile;
		}
		if (OpenRenderString != null)
		{
			ParentObject.pRender.RenderString = OpenRenderString;
		}
		if (OpenLayer != int.MinValue)
		{
			ParentObject.pRender.RenderLayer = OpenLayer;
		}
		ParentObject.FireEvent("SyncOpened");
		return true;
	}

	public bool EnclosureExitImpeded(GameObject who, bool ShowMessage = false, Enclosed Effect = null)
	{
		if (ExitSaveTarget > 0 || AnyExitDamage())
		{
			if (ShowMessage && who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that while enclosed by " + ParentObject.t() + ".");
			}
			return true;
		}
		return false;
	}

	private void SendEvent(GameObject who, string ID, int Level, bool SendSource, bool UseGenericNotify, bool OnSelf)
	{
		GameObject gameObject = (OnSelf ? ParentObject : who);
		if (UseGenericNotify)
		{
			GenericNotifyEvent.Send(gameObject, ID, who, ParentObject, Level);
		}
		else if (SendSource || Level != 0)
		{
			Event @event = Event.New(ID);
			if (SendSource)
			{
				@event.SetParameter("Source", ParentObject);
			}
			if (Level != 0)
			{
				@event.SetParameter("Level", Level);
			}
			gameObject.FireEvent(@event);
		}
		else
		{
			gameObject.FireEvent(ID);
		}
	}

	public void ProcessTurnEnclosed(GameObject who, int TurnsEnclosed)
	{
		bool flag = IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		who.FireEvent(Event.New("StartTurnEnclosed", "Enclosing", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("StartTurnEnclosing", "Object", who, "Disabled", flag ? 1 : 0));
		CheckPeriodicDamage(who);
		if (!flag)
		{
			if (!string.IsNullOrEmpty(PeriodicEvent1) && PeriodicEventTurns1 > 0 && TurnsEnclosed % PeriodicEventTurns1 == 0)
			{
				SendEvent(who, PeriodicEvent1, PeriodicEventLevel1, PeriodicEventSendSource1, PeriodicEventUseGenericNotify1, PeriodicEventOnSelf1);
			}
			if (!string.IsNullOrEmpty(PeriodicEvent2) && PeriodicEventTurns2 > 0 && TurnsEnclosed % PeriodicEventTurns2 == 0)
			{
				SendEvent(who, PeriodicEvent2, PeriodicEventLevel2, PeriodicEventSendSource2, PeriodicEventUseGenericNotify1, PeriodicEventOnSelf1);
			}
			if (!string.IsNullOrEmpty(PeriodicEvent3) && PeriodicEventTurns3 > 0 && TurnsEnclosed % PeriodicEventTurns3 == 0)
			{
				SendEvent(who, PeriodicEvent3, PeriodicEventLevel3, PeriodicEventSendSource3, PeriodicEventUseGenericNotify1, PeriodicEventOnSelf1);
			}
		}
		who.FireEvent(Event.New("EndTurnEnclosed", "Enclosing", ParentObject, "Disabled", flag ? 1 : 0));
		ParentObject.FireEvent(Event.New("EndTurnEnclosing", "Object", who, "Disabled", flag ? 1 : 0));
	}

	private void DetachEffects(GameObject GO, IEvent Event)
	{
		if (GO.GetEffect("Enclosed") is Enclosed enclosed && enclosed.EnclosedBy == ParentObject)
		{
			GO.RemoveEffect(enclosed);
			GO.ApplyEffect(new Prone());
			Event?.RequestInterfaceExit();
		}
	}

	private void DetachEffects(Cell C = null, IEvent Event = null)
	{
		if (C == null)
		{
			C = ParentObject.GetCurrentCell();
		}
		C?.SafeForeachObject(delegate(GameObject o)
		{
			DetachEffects(o, Event);
		});
	}
}
