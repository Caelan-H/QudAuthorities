using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class SporeCloudPoison : Effect
{
	public int Damage = 2;

	public GameObject Owner;

	public SporeCloudPoison()
	{
		base.DisplayName = "{{W|covered in spores}}";
	}

	public SporeCloudPoison(int Duration, GameObject Owner)
		: this()
	{
		this.Owner = Owner;
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override bool SameAs(Effect e)
	{
		SporeCloudPoison sporeCloudPoison = e as SporeCloudPoison;
		if (sporeCloudPoison.Damage != Damage)
		{
			return false;
		}
		if (sporeCloudPoison.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return Damage + " damage per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplySporeCloudInfection")))
		{
			return true;
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
		if (base.Duration > 0)
		{
			base.Object.TakeDamage(Damage, "from %t spores!", "Poison Gas Fungal Spores Unavoidable", null, null, Owner, null, null, null, Accidental: false, Environmental: false, Indirect: true);
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
			DidX("shake", "the spores off", null, null, base.Object);
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
			E.ColorString = "&W^w";
		}
		return true;
	}
}
