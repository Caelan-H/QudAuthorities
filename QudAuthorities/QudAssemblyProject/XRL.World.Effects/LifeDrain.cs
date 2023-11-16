using System;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class LifeDrain : Effect
{
	public string Damage = "1";

	public int SaveTarget = 20;

	public GameObject Drainer;

	public int Level;

	public bool RealityDistortionBased;

	public LifeDrain()
	{
		base.DisplayName = "syphoned";
	}

	public LifeDrain(int Duration, int Level, string DamagePerRound, GameObject Drainer)
		: this()
	{
		Damage = DamagePerRound;
		base.Duration = Duration;
		this.Drainer = Drainer;
		this.Level = Level;
	}

	public LifeDrain(int Duration, int Level, string DamagePerRound, GameObject Drainer, bool RealityDistortionBased)
		: this(Duration, Level, DamagePerRound, Drainer)
	{
		this.RealityDistortionBased = RealityDistortionBased;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33587204;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Damage + " life drained per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("ApplyLifeDrain"))
		{
			return false;
		}
		IComponent<GameObject>.XDidYToZ(Drainer, "bond", "with", Object, null, "!", null, null, Object);
		Object.ParticleText("*bonded*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		IComponent<GameObject>.XDidYToZ(Drainer, "begin", "to drain life essence from", Object, null, "!", null, null, Object);
		if (Drainer.IsPlayer() && Drainer.Target == null)
		{
			Drainer.Target = Object;
		}
		if (Object.IsPlayer())
		{
			AutoAct.Interrupt();
		}
		else if (Object.IsPlayerLed() && !Object.IsTrifling)
		{
			if (Object.IsVisible())
			{
				AutoAct.Interrupt(null, null, Object);
			}
			else if (Object.IsAudible(IComponent<GameObject>.ThePlayer))
			{
				AutoAct.Interrupt("you hear a cry of distress from " + Object.the + Object.BaseDisplayName, null, Object);
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor == Drainer && GameObject.validate(ref Drainer))
		{
			E.AddAction("CancelLifeDrain", "cancel life drain", "CancelLifeDrain", null, 'c', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "CancelLifeDrain" && E.Actor == Drainer)
		{
			IComponent<GameObject>.XDidYToZ(E.Actor, "release", base.Object, "from " + E.Actor.its + " life drain", null, null, E.Actor, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: true);
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!GameObject.validate(ref Drainer) || !Drainer.HasHitpoints() || Drainer.IsInGraveyard())
		{
			base.Duration = 0;
			Drainer = null;
			return true;
		}
		if (RealityDistortionBased && (!Drainer.FireEvent("CheckRealityDistortionUsability") || !base.Object.LocalEvent("CheckRealityDistortionAccessibility")))
		{
			base.Duration = 0;
			return true;
		}
		if (base.Duration > 0)
		{
			if (Stat.Random(1, 8) + Math.Max(Drainer.StatMod("Ego"), Level) > Stats.GetCombatMA(base.Object) + 4)
			{
				int Amount = Damage.RollCached();
				if (Amount > 0 && base.Object.TakeDamage(ref Amount, "Drain Unavoidable", null, null, Drainer, null, null, null, "from %t life drain!", Accidental: false, Environmental: false, Indirect: true) && Amount > 0)
				{
					Drainer.Heal(Amount, Message: true, FloatText: true, RandomMinimum: true);
				}
			}
			else if (Drainer.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(base.Object.The + base.Object.ShortDisplayName + base.Object.GetVerb("resist") + " your life drain!", 'r');
			}
			else if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("You resist " + Grammar.MakePossessive(Drainer.the + Drainer.ShortDisplayName) + " life drain!", 'g');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 25 && num < 35)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			E.ColorString = "&K^k";
		}
		return true;
	}
}
