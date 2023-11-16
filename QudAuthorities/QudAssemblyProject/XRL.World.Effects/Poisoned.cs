using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Poisoned : Effect
{
	public GameObject Owner;

	public string DamageIncrement;

	public int Level;

	public Poisoned()
	{
		base.DisplayName = "{{G|poisoned}}";
	}

	public Poisoned(int Duration, string DamageIncrement, int Level, GameObject Owner = null)
		: this()
	{
		base.Duration = Duration;
		this.DamageIncrement = DamageIncrement;
		this.Level = Level;
		this.Owner = Owner;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "(" + DamageIncrement + ")/2 damage per turn.\nDoesn't regenerate hit points.\nHealing effects are only half as effective.\nWill become ill once the poison runs its course.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetEffect("Poisoned") is Poisoned poisoned)
		{
			if (base.Duration > poisoned.Duration)
			{
				poisoned.Duration = base.Duration;
			}
			if (DamageIncrement.RollMax() > poisoned.DamageIncrement.RollMax())
			{
				poisoned.DamageIncrement = DamageIncrement;
			}
			if (Level > poisoned.Level)
			{
				poisoned.Level = Level;
			}
			return false;
		}
		if (Object.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Object, "Poison", this))
		{
			DidX("have", "been poisoned", "!", null, null, Object);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == GeneralAmnestyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		GameObject.validate(ref Owner);
		if (base.Duration > 0 && !base.Object.CurrentCell.HasObjectWithPart("GasPoison"))
		{
			base.Object.TakeDamage((int)Math.Ceiling((float)DamageIncrement.RollCached() / 2f), "from %t {{g|poison}}!", "Poison Unavoidable", null, null, Owner, null, null, null, Accidental: false, Environmental: false, Indirect: true);
			if (base.Duration > 0 && base.Duration != 9999)
			{
				base.Duration--;
			}
			if (base.Duration <= 0)
			{
				int duration = Stat.Random((int)(35f + 0.8f * (float)Level * 4.5f), (int)(35f + 1.2f * (float)Level * 4.5f));
				base.Object.ApplyEffect(new Ill(duration, Level));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Healing");
		Object.RegisterEffectEvent(this, "Recuperating");
		Object.RegisterEffectEvent(this, "Regenerating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Healing");
		Object.UnregisterEffectEvent(this, "Recuperating");
		Object.UnregisterEffectEvent(this, "Regenerating");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0003";
			E.ColorString = "&G^k";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				base.Duration = 0;
				DidX("are", "no longer poisoned", "!", null, base.Object);
			}
		}
		return base.FireEvent(E);
	}
}
