using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class AshPoison : Effect
{
	public int Damage = 2;

	public GameObject Owner;

	public AshPoison()
	{
		base.DisplayName = "{{K|choking on ash}}";
	}

	public AshPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440520;
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent("ApplyAshPoison"))
		{
			return ApplyEffectEvent.Check(Object, "AshPoison", this);
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == GeneralAmnestyEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.validate(ref Owner);
		if (base.Duration > 0 && base.Object?.CurrentCell != null && !base.Object.CurrentCell.HasObjectWithPart("GasAsh"))
		{
			base.Object.TakeDamage(Damage, "from %t {{W|choking ash}}!", "Asphyxiation Gas Unavoidable", null, null, null, Owner, null, null, Accidental: false, Environmental: false, Indirect: true);
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
		Object.RegisterEffectEvent(this, "Recuperating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Recuperating");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			E.ColorString = "&W^r";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			base.Duration = 0;
			DidX("are", "no longer choking", "!", null, base.Object);
		}
		return base.FireEvent(E);
	}
}
