using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Tinkering_Mine : IPart, IDisarmingSifrahHandler
{
	public int Timer = -1;

	public bool PlayerMine = true;

	public GameObject Explosive;

	public GameObject Owner;

	public string OwnerFactions;

	public string Message = "AfterThrown";

	public bool Armed = true;

	public string ArmedDetailColor = "R";

	public string DisarmedDetailColor = "y";

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Tinkering_Mine tinkering_Mine = new Tinkering_Mine();
		tinkering_Mine.Timer = Timer;
		tinkering_Mine.PlayerMine = PlayerMine;
		if (GameObject.validate(ref Explosive))
		{
			tinkering_Mine.Explosive = MapInv?.Invoke(Explosive) ?? Explosive.DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (tinkering_Mine.Explosive != null)
			{
				tinkering_Mine.Explosive.ForeachPartDescendedFrom(delegate(Tinkering_Layable p)
				{
					p.ComponentOf = Parent;
				});
			}
		}
		tinkering_Mine.Owner = Owner;
		tinkering_Mine.Message = Message;
		tinkering_Mine.Armed = Armed;
		tinkering_Mine.ArmedDetailColor = ArmedDetailColor;
		tinkering_Mine.DisarmedDetailColor = DisarmedDetailColor;
		tinkering_Mine.ParentObject = Parent;
		return tinkering_Mine;
	}

	public override bool WantTurnTick()
	{
		if (GameObject.validate(ref Explosive))
		{
			return Explosive.WantTurnTick();
		}
		return false;
	}

	public override bool WantTenTurnTick()
	{
		if (GameObject.validate(ref Explosive))
		{
			return Explosive.WantTenTurnTick();
		}
		return false;
	}

	public override bool WantHundredTurnTick()
	{
		if (GameObject.validate(ref Explosive))
		{
			return Explosive.WantHundredTurnTick();
		}
		return false;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Explosive) && Explosive.WantTurnTick())
		{
			Explosive.TurnTick(TurnNumber);
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Explosive) && Explosive.WantTenTurnTick())
		{
			Explosive.TenTurnTick(TurnNumber);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (GameObject.validate(ref Explosive) && Explosive.WantHundredTurnTick())
		{
			Explosive.HundredTurnTick(TurnNumber);
		}
	}

	public void SetExplosive(GameObject obj)
	{
		if (obj == Explosive)
		{
			return;
		}
		if (Explosive != null)
		{
			Explosive.ForeachPartDescendedFrom(delegate(Tinkering_Layable p)
			{
				p.ComponentOf = null;
			});
		}
		Explosive = obj;
		obj?.ForeachPartDescendedFrom(delegate(Tinkering_Layable p)
		{
			p.ComponentOf = ParentObject;
		});
		FlushWantTurnTickCache();
	}

	private bool ExplosiveWantsEvent(int ID, int cascade)
	{
		if (!GameObject.validate(ref Explosive))
		{
			return false;
		}
		if (!MinEvent.CascadeTo(cascade, 8))
		{
			return false;
		}
		return Explosive.WantEvent(ID, cascade);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != EffectAppliedEvent.ID && ID != EndTurnEvent.ID && ID != GeneralAmnestyEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && (ID != GetAdjacentNavigationWeightEvent.ID || Timer <= 0) && ID != GetNavigationWeightEvent.ID && ID != GetShortDescriptionEvent.ID && (ID != InterruptAutowalkEvent.ID || !Armed || Timer > 0) && ID != InventoryActionEvent.ID && ID != ObjectCreatedEvent.ID && ID != ObjectEnteredCellEvent.ID && ID != OnDestroyObjectEvent.ID && ID != TookDamageEvent.ID)
		{
			return ExplosiveWantsEvent(ID, cascade);
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		ParentObject.Twiddle();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Timer", Timer);
		E.AddEntry(this, "PlayerMine", PlayerMine);
		E.AddEntry(this, "Explosive", Explosive);
		E.AddEntry(this, "Owner", Owner);
		E.AddEntry(this, "OwnerFactions", OwnerFactions);
		E.AddEntry(this, "Message", Message);
		E.AddEntry(this, "Armed", Armed);
		E.AddEntry(this, "ArmedDetailColor", ArmedDetailColor);
		E.AddEntry(this, "DisarmedDetailColor", DisarmedDetailColor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart && Armed && GameObject.validate(ref Explosive) && Avoidable(E.Actor))
		{
			GetComponentNavigationWeightEvent.Process(Explosive, E);
			if (E.Weight < 7 && WillTrigger(E.Actor))
			{
				E.MinWeight(7);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (E.Smart && Armed)
		{
			if (Timer >= 0)
			{
				if (GameObject.validate(ref Explosive) && Avoidable(E.Actor))
				{
					GetComponentAdjacentNavigationWeightEvent.Process(Explosive, E);
					if (E.Weight < 3 && WillTrigger(E.Actor))
					{
						E.MinWeight(3);
					}
				}
			}
			else if (GameObject.validate(ref Explosive) && Avoidable(E.Actor))
			{
				GetComponentAdjacentNavigationWeightEvent.Process(Explosive, E);
				if (E.Weight < 2 && WillTrigger(E.Actor))
				{
					E.MinWeight(2);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(8) && GameObject.validate(ref Explosive) && !Explosive.HandleEvent(E))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (Armed)
		{
			if (IsEMPed())
			{
				Disarm();
			}
			else if (IsBroken() || IsRusted())
			{
				Boom();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (Armed && E.Damage.Amount > 0)
		{
			Boom();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (GameObject.validate(ref Explosive))
		{
			E.Value += Explosive.Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (GameObject.validate(ref Explosive))
		{
			E.Weight += Explosive.GetWeight();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Armed && Timer > 0)
		{
			Timer--;
			if (Timer <= 0)
			{
				Boom();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!RouteEventToExplosiveModifications(E))
		{
			return false;
		}
		if (E.Understood())
		{
			if (!GameObject.validate(ref Explosive))
			{
				E.AddAdjective("empty", -40);
			}
			else if (Armed)
			{
				if (Timer > 0)
				{
					E.AddTag("{{y|[{{R|" + Timer + " sec}}]}}");
				}
			}
			else
			{
				E.AddAdjective("disarmed", -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!RouteEventToExplosiveModifications(E))
		{
			return false;
		}
		if (Armed)
		{
			if (Timer > 0)
			{
				if (E.Base.Length > 0)
				{
					E.Base.Append(' ');
				}
				E.Base.Append(ParentObject.Ithas).Append(' ').Append(Grammar.Cardinal(Timer))
					.Append((Timer == 1) ? "second" : "seconds")
					.Append(" left on ")
					.Append(ParentObject.its)
					.Append(" timer.");
			}
		}
		else
		{
			if (E.Base.Length > 0)
			{
				E.Base.Append(' ');
			}
			E.Base.Append(ParentObject.Ithas).Append(" been disarmed.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			if (Armed)
			{
				E.AddAction("Detonate", "detonate", "Detonate", null, 'n', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
			}
			if (ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
			{
				if (Armed)
				{
					E.AddAction("Disarm", "disarm", "DisarmMine", null, 'd', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
				else
				{
					E.AddAction("Arm", "arm", "ArmMine", null, 'a', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
			}
			else if (ParentObject.InInventory != null && ParentObject.InInventory.IsPlayer() && !ParentObject.InInventory.OnWorldMap())
			{
				if (Timer > 0)
				{
					E.AddAction("Set", "set", "LayMine", null, 's', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
				else
				{
					E.AddAction("Lay", "lay", "LayMine", null, 'L', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LayMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptLay(E.Actor))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "ArmMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptArm(E.Actor))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "DisarmMine")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!AttemptDisarm(E.Actor, E))
			{
				return false;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "Detonate")
		{
			if (!CheckInteraction(E.Actor))
			{
				return false;
			}
			if (!Armed)
			{
				return false;
			}
			if (!Boom())
			{
				return false;
			}
			E.Actor.UseEnergy(1000, "Item Tinkering " + ((Timer > 0) ? "Bomb" : "Mine") + " Detonate");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		Disarm();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (Armed && Timer <= 0 && WillTrigger(E.Object))
		{
			Boom();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (GameObject.validate(ref Explosive))
		{
			Explosive.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		if (Armed && Timer <= 0 && GameObject.validate(ref Explosive) && Avoidable(E.Actor) && WillTrigger(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	private bool RouteEventToExplosiveModifications(MinEvent E)
	{
		if (GameObject.validate(ref Explosive))
		{
			List<IModification> partsDescendedFrom = Explosive.GetPartsDescendedFrom<IModification>();
			if (partsDescendedFrom.Count > 0)
			{
				Type type = E.GetType();
				int iD = E.ID;
				int cascadeLevel = E.GetCascadeLevel();
				int i = 0;
				for (int count = partsDescendedFrom.Count; i < count; i++)
				{
					IModification modification = partsDescendedFrom[i];
					if (modification.WantEvent(iD, cascadeLevel))
					{
						if (!GameObject.callHandleEventMethod(modification, GameObject.getHandleEventMethod(modification.GetType(), type), E))
						{
							return false;
						}
						if (!modification.HandleEvent(E))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanBeDisassembled");
		Object.RegisterPartEvent(this, "CanBeTaken");
		base.Register(Object);
	}

	public bool WillTrigger(GameObject who)
	{
		if (GameObject.validate(ref who) && GameObject.validate(ref Explosive) && who.IsCombatObject() && who.PhaseAndFlightMatches(ParentObject) && ConsiderHostile(who))
		{
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "CanBeTaken" || E.ID == "CanBeDisassembled") && Armed)
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool Boom()
	{
		if (!GameObject.validate(ref Explosive))
		{
			return false;
		}
		GameObject.validate(ref Owner);
		GameObject explosive = Explosive;
		SetExplosive(null);
		Cell C = ParentObject.GetCurrentCell();
		ParentObject.RemoveFromContext();
		if (C != null)
		{
			C.AddObject(explosive, Forced: true, System: false, IgnoreGravity: true);
			if (explosive.pRender != null && ParentObject.pRender != null)
			{
				explosive.pRender.DisplayName = ParentObject.pRender.DisplayName;
			}
			Temporary.CarryOver(ParentObject, explosive);
			Hidden.CarryOver(ParentObject, explosive);
			Phase.carryOver(ParentObject, explosive);
			if (explosive.ForeachPartDescendedFrom((IGrenade p) => (!p.Detonate(C, Owner, null, Indirect: true)) ? true : false))
			{
				Event @event = Event.New(Message);
				if (GameObject.validate(ref Owner))
				{
					@event.SetParameter("Owner", Owner);
				}
				explosive.FireEvent(@event);
			}
		}
		ParentObject.Destroy(null, Silent: true);
		return true;
	}

	public bool AttemptArm(GameObject who)
	{
		if (Armed)
		{
			return false;
		}
		if (who.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (who.OnWorldMap())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		Arm(who);
		IComponent<GameObject>.XDidYToZ(who, "arm", ParentObject, null, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
		who.UseEnergy(1000, "Skill Tinkering " + ((Timer > 0) ? "Bomb" : "Mine") + " Arm");
		return true;
	}

	public bool AttemptLay(GameObject who)
	{
		if (ParentObject.InInventory != who)
		{
			return false;
		}
		if (Armed)
		{
			return false;
		}
		if (who.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (who.OnWorldMap())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		Cell cell;
		if (who.IsPlayer())
		{
			string text = XRL.UI.PickDirection.ShowPicker();
			if (text == null)
			{
				return false;
			}
			cell = who.CurrentCell.GetCellFromDirection(text);
		}
		else
		{
			cell = who.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
		}
		if (cell == null || !cell.IsEmpty())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You can't deploy there!");
			}
			return false;
		}
		GameObject gameObject = ParentObject.RemoveOne();
		gameObject.RemoveFromContext();
		gameObject.GetPart<Tinkering_Mine>().Arm(who);
		if (who.IsPlayer())
		{
			IComponent<GameObject>.XDidY(who, (Timer > 0) ? "set" : "lay", gameObject.the + gameObject.DisplayName);
		}
		else if (who.IsVisible())
		{
			IComponent<GameObject>.XDidY(who, (Timer > 0) ? "set" : "lay", gameObject.a + gameObject.DisplayName);
		}
		cell.AddObject(gameObject);
		who.UseEnergy(1000, "Skill Tinkering " + ((Timer > 0) ? "Mine Lay" : "Bomb Set"));
		return true;
	}

	public bool AttemptDisarm(GameObject who, IEvent FromEvent = null)
	{
		if (!Armed)
		{
			return false;
		}
		if (who.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (who.OnWorldMap())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			return false;
		}
		if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		if (Options.SifrahDisarming)
		{
			int tier = Explosive.GetTier();
			int num = who.Stat("Intelligence");
			if (who.HasSkill("Tinkering_GadgetInspector"))
			{
				num += 4;
			}
			if (who.HasSkill("Tinkering_LayMine"))
			{
				num += 4;
			}
			DisarmingSifrah disarmingSifrah = new DisarmingSifrah(ParentObject, tier, num);
			disarmingSifrah.HandlerID = ParentObject.id;
			disarmingSifrah.HandlerPartName = base.Name;
			disarmingSifrah.Play(ParentObject);
			if (disarmingSifrah.InterfaceExitRequested)
			{
				FromEvent?.RequestInterfaceExit();
			}
		}
		else
		{
			int num2 = 9 + Explosive.GetTier() + Explosive.GetMark();
			if (who.HasSkill("Tinkering_GadgetInspector"))
			{
				num2 -= 4;
			}
			if (who.HasSkill("Tinkering_LayMine"))
			{
				num2 -= 4;
			}
			string vs = ((Timer <= 0) ? "Tinkering Mine Disarm" : "Tinkering Bomb Disarm");
			int num3 = who.SaveChance("Intelligence", num2, null, null, vs);
			int num4 = who.Stat("Intelligence");
			Mutations part = who.GetPart<Mutations>();
			if (part != null)
			{
				if (part.HasMutation("Intuition"))
				{
					num4 += 10;
				}
				if (part.HasMutation("Precognition"))
				{
					num4 += 5;
				}
				if (part.HasMutation("Skittish"))
				{
					num3 -= 10;
				}
			}
			if (who.HasSkill("Discipline_IronMind"))
			{
				num4 += 2;
			}
			if (who.HasSkill("Discipline_Lionheart"))
			{
				num3 += 3;
			}
			if (num4 <= 10)
			{
				num3 += 20;
			}
			else if (num4 <= 15)
			{
				num3 += 10;
			}
			else if (num4 <= 20)
			{
				num3 -= 10;
			}
			else if (num4 <= 25)
			{
				num3 -= 5;
			}
			int num5 = ((num4 <= 10) ? 20 : ((num4 <= 20) ? 10 : ((num4 > 30) ? 1 : 5)));
			if (num5 > 1)
			{
				num3 -= num3 % num5;
			}
			if (num3 > 100 - num5)
			{
				num3 = 100 - num5;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (who.IsPlayer())
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				string chanceColor = Stat.GetChanceColor(num3);
				stringBuilder.Append("Failing to disarm ").Append(ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true)).Append(" will detonate ")
					.Append(ParentObject.it)
					.Append(". You estimate you have");
				if (num3 < num5)
				{
					stringBuilder.Append(" less than a ").Append(chanceColor).Append(num5.ToString())
						.Append("%");
				}
				else
				{
					stringBuilder.Append(" about a ").Append(chanceColor).Append(num3.ToString())
						.Append("%");
				}
				stringBuilder.Append(chanceColor).Append(" chance of success. Do you want to make the attempt?");
				if (Popup.ShowYesNo(stringBuilder.ToString()) != 0)
				{
					return false;
				}
			}
			else if (!num3.in100() && !num3.in100())
			{
				return false;
			}
			if (who.MakeSave("Intelligence", num2, null, null, vs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				IComponent<GameObject>.XDidYToZ(who, "disarm", ParentObject, null, null, null, who, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
				Disarm();
			}
			else
			{
				Boom();
			}
		}
		who.UseEnergy(1000, "Skill Tinkering Mine Disarm");
		return true;
	}

	public void DisarmingResultSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.ShowBlock("You disarm " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
		Disarm();
	}

	public void DisarmingResultExceptionalSuccess(GameObject who, GameObject obj)
	{
		DisarmingResultSuccess(who, obj);
		string randomBits = BitType.GetRandomBits("1d4".Roll(), Explosive.GetTier());
		if (!string.IsNullOrEmpty(randomBits))
		{
			who.RequirePart<BitLocker>().AddBits(randomBits);
			if (who.IsPlayer())
			{
				Popup.Show("You receive tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}>");
			}
		}
	}

	public void DisarmingResultPartialSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You make some progress disarming " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void DisarmingResultFailure(GameObject who, GameObject obj)
	{
		Boom();
	}

	public void DisarmingResultCriticalFailure(GameObject who, GameObject obj)
	{
		List<Cell> localAdjacentCells = ParentObject.CurrentCell.GetLocalAdjacentCells();
		Cell cell = who.CurrentCell;
		ParentObject.Discharge((cell != null && localAdjacentCells.Contains(cell)) ? cell : localAdjacentCells.GetRandomElement(), "3d8".Roll(), "2d4");
		Boom();
	}

	public void Disarm()
	{
		Armed = false;
		if (!string.IsNullOrEmpty(DisarmedDetailColor) && ParentObject.pRender != null)
		{
			ParentObject.pRender.DetailColor = DisarmedDetailColor;
		}
	}

	public void Arm(GameObject who = null)
	{
		Armed = true;
		Owner = who;
		OwnerFactions = ((Owner != null && !Owner.IsPlayerControlled()) ? Owner.Factions : null);
		PlayerMine = Owner != null && Owner.IsPlayer();
		if (!string.IsNullOrEmpty(ArmedDetailColor) && ParentObject.pRender != null)
		{
			ParentObject.pRender.DetailColor = ArmedDetailColor;
		}
	}

	public bool ConsiderNonHostile(GameObject who)
	{
		return !ConsiderHostile(who);
	}

	public bool ConsiderHostile(GameObject who)
	{
		if (PlayerMine && who.IsPlayerControlled())
		{
			return false;
		}
		if (GameObject.validate(Owner) && who.IsHostileTowards(Owner))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(OwnerFactions) && Brain.GetOpinion(OwnerFactions, who) == Brain.CreatureOpinion.hostile)
		{
			return true;
		}
		return false;
	}

	public bool ConsiderAllied(GameObject who)
	{
		if (PlayerMine && who.IsPlayerControlled())
		{
			return true;
		}
		if (GameObject.validate(Owner) && who.IsAlliedTowards(Owner))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(OwnerFactions) && Brain.GetOpinion(OwnerFactions, who) == Brain.CreatureOpinion.allied)
		{
			return true;
		}
		return false;
	}

	public bool Avoidable(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (who.IsPlayer() && !ParentObject.Understood())
		{
			return false;
		}
		if (ParentObject.GetPart("Hidden") is Hidden hidden)
		{
			if (hidden.Found)
			{
				return true;
			}
			if (!who.IsPlayerControlled() && ConsiderAllied(who))
			{
				return true;
			}
		}
		else
		{
			if (PlayerMine && !who.IsHostileTowards(IComponent<GameObject>.ThePlayer))
			{
				return true;
			}
			if (ConsiderNonHostile(who))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckInteraction(GameObject who)
	{
		if (ParentObject.InInventory != who && ParentObject.Equipped != who)
		{
			if (!who.FlightMatches(ParentObject))
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You cannot reach " + ParentObject.t() + ".");
				}
				return false;
			}
			if (!who.PhaseMatches(ParentObject))
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("Your " + (who.GetFirstBodyPart("Hands")?.GetOrdinalName() ?? "appendages") + " pass through " + ParentObject.t() + ".");
				}
				return false;
			}
		}
		return true;
	}
}
