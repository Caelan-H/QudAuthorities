using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class PoisonGasPoison : Effect
{
	public int Damage = 2;

	public GameObject Owner;

	public PoisonGasPoison()
	{
		base.DisplayName = "{{G|poisoned by gas}}";
	}

	public PoisonGasPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440520;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent("ApplyPoisonGasPoison"))
		{
			return ApplyEffectEvent.Check(Object, "PoisonGasPoison", this);
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
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
			base.Object.TakeDamage(Damage, "from %t {{g|poison}}!", "Poison Gas Unavoidable", null, null, Owner, null, null, null, Accidental: false, Environmental: false, Indirect: true);
			if (base.Duration > 0 && base.Duration != 9999)
			{
				base.Duration--;
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
		Object.RegisterEffectEvent(this, "Recuperating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Recuperating");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Recuperating")
		{
			base.Duration = 0;
			DidX("are", "no longer poisoned", "!", null, base.Object);
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			E.ColorString = "&g^c";
		}
		return true;
	}
}
