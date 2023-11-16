using System;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Asleep : Effect
{
	public bool quicksleep;

	public bool forced;

	public GameObject AsleepOn;

	public int TurnsAsleep;

	public bool Voluntary;

	public int IndefinitePlayerSleepTurns;

	[NonSerialized]
	private bool WakeMessageDone;

	public Asleep()
	{
		base.DisplayName = "{{c|asleep}}";
	}

	public Asleep(int Duration, bool forced = false, bool quicksleep = false, bool voluntary = false)
		: this()
	{
		base.Duration = Duration;
		this.forced = forced;
		this.quicksleep = quicksleep;
		Voluntary = voluntary;
	}

	public Asleep(GameObject AsleepOn, int Duration, bool forced = false, bool quicksleep = false, bool voluntary = false)
		: this(Duration, forced, quicksleep, voluntary)
	{
		this.AsleepOn = AsleepOn;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		int num = 117440514;
		if (Voluntary)
		{
			num |= 0x8000000;
		}
		return num;
	}

	public static bool UsesSleepMode(GameObject obj)
	{
		return obj?.HasTag("Robot") ?? false;
	}

	public bool UsesSleepMode()
	{
		return UsesSleepMode(base.Object);
	}

	public override string GetDescription()
	{
		if (UsesSleepMode())
		{
			return "{{c|sleep mode}}";
		}
		return base.GetDescription();
	}

	public override string GetDetails()
	{
		if (UsesSleepMode())
		{
			return "In power conservation mode.\n-12 DV.\nAttackers receive +4 to penetration rolls.\nWill return to normal operations dazed if damage is taken.";
		}
		return "Unconscious.\n-12 DV.\nAttackers receive +4 to penetration rolls.\nWill wake up dazed if damage is taken.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart("Brain"))
		{
			return false;
		}
		if (Object.HasEffect("Asleep"))
		{
			return false;
		}
		if (((Voluntary || Object.FireEvent("CanApplyInvoluntarySleep")) && Object.FireEvent("CanApplySleep") && (Voluntary || Object.FireEvent("ApplyInvoluntarySleep")) && Object.FireEvent("ApplySleep") && ApplyEffectEvent.Check(Object, "Sleep", this)) || forced)
		{
			if (!UsesSleepMode(Object))
			{
				Object.ApplyEffect(new Prone(AsleepOn, Voluntary));
			}
			Object.MovementModeChanged("Asleep", !Voluntary);
			if (Object.IsPlayer())
			{
				if (UsesSleepMode(Object))
				{
					IComponent<GameObject>.AddPlayerMessage("You enter {{C|sleep mode}}.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("You fall {{C|asleep}}!");
				}
			}
			else if (Visible())
			{
				if (UsesSleepMode(Object))
				{
					IComponent<GameObject>.AddPlayerMessage(Object.The + Object.ShortDisplayName + Object.GetVerb("go") + " into {{C|sleep mode}}.");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(Object.The + Object.ShortDisplayName + Object.GetVerb("fall") + " {{C|asleep}}.");
				}
			}
			if (Object.pBrain != null)
			{
				Object.pBrain.Goals.Clear();
			}
			Object.ForfeitTurn();
			if (Object.IsPlayer())
			{
				AutoAct.Interrupt();
			}
			ApplyStats();
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		if (!WakeMessageDone)
		{
			if (UsesSleepMode(Object))
			{
				DidX("exit", "sleep mode");
			}
			else
			{
				DidX("wake", "up");
			}
		}
		if (!Voluntary)
		{
			Object.ApplyEffect(new Wakeful(Stat.Random(3, 5)));
		}
		base.Remove(Object);
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("DV", -12);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != EndTurnEvent.ID && ID != GetDefenderHitDiceEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == base.Object)
		{
			if (E.Mental && !E.Physical)
			{
				string text = (20.in100() ? GameText.GenerateMarkovMessageParagraph() : GameText.GenerateMarkovMessageSentence());
				text = GameText.VariableReplace(text.Trim(), base.Object);
				if (base.Object.GetPrimaryFaction() == "Birds")
				{
					text = ((!(base.Object.GetTag("Species") == "flamingo")) ? TextFilters.Corvid(text) : TextFilters.WaterBird(text));
				}
				Brain pBrain = base.Object.pBrain;
				if (pBrain != null && pBrain.Aquatic)
				{
					text = TextFilters.Fish(text);
				}
				E.Message = "{{c|" + text + "}}";
			}
			else if (UsesSleepMode(base.Object))
			{
				E.Message = base.Object.T() + base.Object.Is + " utterly unresponsive.";
			}
			else
			{
				E.Message = base.Object.T() + base.Object.GetVerb("snore") + " loudly.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (base.Duration == 9999)
			{
				if (base.Object.IsPlayer() && (1.in1000() || IndefinitePlayerSleepTurns == int.MaxValue))
				{
					base.Duration = 0;
				}
				else
				{
					IndefinitePlayerSleepTurns++;
					if (IndefinitePlayerSleepTurns % 10 == 0)
					{
						XRLCore.TenPlayerTurnsPassed();
						The.Core.RenderBase(UpdateSidebar: false);
					}
					if (base.Object.IsPlayer() && The.Game.Player.Messages.LastLine != "You are asleep.")
					{
						IComponent<GameObject>.AddPlayerMessage("You are asleep.");
					}
				}
			}
			else
			{
				base.Duration--;
			}
			if (base.Duration > 0)
			{
				if (base.Duration % 10 == 0)
				{
					XRLCore.TenPlayerTurnsPassed();
					XRLCore.Core.RenderBase();
				}
				if (base.Object.IsPlayer() && The.Game.Player.Messages.LastLine != "You are asleep.")
				{
					IComponent<GameObject>.AddPlayerMessage("You are asleep.");
				}
				E.PreventAction = true;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDefenderHitDiceEvent E)
	{
		E.PenetrationBonus += 4;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (base.Duration > 0)
		{
			CheckAsleepOn();
			TurnsAsleep++;
			if (AsleepOn?.GetPart("Bed") is Bed bed)
			{
				bed.ProcessTurnAsleep(base.Object, TurnsAsleep);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanWake(E.Actor))
		{
			E.AddAction("Wake", "wake", "WakeSleeper", null, 'w', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "WakeSleeper")
		{
			GameObject actor = E.Actor;
			if (CanWake(actor) && actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				base.Duration = 0;
				if (actor.IsPlayer())
				{
					if (UsesSleepMode(base.Object))
					{
						Popup.Show("You press " + Grammar.MakePossessive(base.Object.the + base.Object.ShortDisplayName) + " activation panel.");
					}
					else
					{
						Popup.Show("You gently shake " + base.Object.the + base.Object.ShortDisplayName + " awake.");
						WakeMessageDone = true;
					}
				}
				else if (base.Object.IsPlayer())
				{
					if (UsesSleepMode(base.Object))
					{
						IComponent<GameObject>.AddPlayerMessage(actor.The + actor.ShortDisplayName + actor.GetVerb("press") + " your activation panel.");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(actor.The + actor.ShortDisplayName + " gently" + actor.GetVerb("shake") + " you awake.");
						WakeMessageDone = true;
					}
				}
				else if (Visible())
				{
					if (UsesSleepMode(base.Object))
					{
						IComponent<GameObject>.AddPlayerMessage(actor.The + actor.ShortDisplayName + actor.GetVerb("press") + " " + Grammar.MakePossessive(base.Object.the + base.Object.ShortDisplayName) + " activation panel.");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(actor.The + actor.ShortDisplayName + " gently" + actor.GetVerb("shake") + " " + base.Object.the + base.Object.ShortDisplayName + " awake.");
						WakeMessageDone = true;
					}
				}
				base.Object.RemoveEffect(this);
				actor.UseEnergy(1000, "Physical Action Wake");
				E.RequestInterfaceExit();
			}
			else if (actor.IsPlayer())
			{
				Popup.Show("You can't figure out how to wake " + base.Object.the + base.Object.ShortDisplayName + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			base.Object.Twiddle();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "CanHaveSmartUseConversation");
		Object.RegisterEffectEvent(this, "CanMoveExtremities");
		Object.RegisterEffectEvent(this, "IsMobile");
		Object.RegisterEffectEvent(this, "TakeDamage");
		Object.RegisterEffectEvent(this, "WakeUp");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "CanHaveSmartUseConversation");
		Object.UnregisterEffectEvent(this, "CanMoveExtremities");
		Object.UnregisterEffectEvent(this, "IsMobile");
		Object.UnregisterEffectEvent(this, "TakeDamage");
		Object.UnregisterEffectEvent(this, "WakeUp");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			if (base.Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "TakeDamage" || E.ID == "WakeUp")
		{
			base.Duration = 0;
			if (UsesSleepMode(base.Object))
			{
				DidX("exit", "sleep mode by emergency interrupt", null, null, null, base.Object);
			}
			else
			{
				DidX("wake", "up in a daze", null, null, null, base.Object);
			}
			WakeMessageDone = true;
			base.Object.ApplyEffect(new Dazed(Stat.Random(3, 4)));
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "CanChangeBodyPosition" || E.ID == "CanChangeMovementMode" || E.ID == "CanMoveExtremities")
		{
			if (!E.HasFlag("Involuntary"))
			{
				return false;
			}
		}
		else
		{
			if (E.ID == "CanHaveSmartUseConversation")
			{
				return false;
			}
			if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyStats();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyStats();
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		_ = base.Object.pRender;
		int num = XRLCore.CurrentFrame % 60;
		if (num > 10 && num < 25)
		{
			E.RenderString = "z";
			E.ColorString = "&C^c";
			return false;
		}
		return true;
	}

	public bool IsAsleepOnValid()
	{
		if (!GameObject.validate(ref AsleepOn))
		{
			return false;
		}
		if (AsleepOn.CurrentCell == null)
		{
			return false;
		}
		if (base.Object == null)
		{
			return false;
		}
		if (base.Object.CurrentCell == null)
		{
			return false;
		}
		if (AsleepOn.CurrentCell != base.Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckAsleepOn()
	{
		if (IsAsleepOnValid())
		{
			return true;
		}
		AsleepOn = null;
		return false;
	}

	public bool CanWake(GameObject who)
	{
		if (!UsesSleepMode(base.Object))
		{
			return true;
		}
		if (who.HasSkill("Tinkering_Repair") || who.HasSkill("Tinkering_Tinker1"))
		{
			return true;
		}
		return false;
	}
}
