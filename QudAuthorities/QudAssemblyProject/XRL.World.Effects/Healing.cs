using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Healing : Effect
{
	private int DamageLeft;

	public Healing()
	{
		base.DisplayName = "healing";
	}

	public Healing(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 83886084;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "5% HP healed per turn.\nStops healing if another action is taken or damage is taken.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Healing"))
		{
			base.Duration = 5;
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyingHealing", "Effect", this)))
		{
			return false;
		}
		DidX("begin", "healing", null, null, Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!base.Object.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			base.Duration = 0;
		}
		else if (base.Duration > 0 && base.Duration != 9999)
		{
			base.Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		base.Object.Heal(Math.Max(base.Object.BaseStat("Hitpoints") / 20, 1));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type == null || !E.Type.Contains("Pass"))
		{
			if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your healing is interrupted!", 'r');
			}
			base.Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "TakeDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "TakeDamage");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TakeDamage" && (E.GetParameter("Damage") as Damage).Amount > 0)
		{
			if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your healing is interrupted!", 'r');
			}
			base.Duration = 0;
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "Z";
				E.ColorString = "&g";
			}
		}
		return true;
	}
}
