using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ShatteredArmor : IShatterEffect
{
	public int Amount;

	public GameObject Owner;

	public ShatteredArmor()
	{
		base.DisplayName = "ShatteredArmor";
		base.Duration = 1;
	}

	public ShatteredArmor(int Amount)
		: this()
	{
		this.Amount = Amount;
	}

	public ShatteredArmor(int Amount, int Duration)
		: this(Amount)
	{
		base.Duration = Duration;
	}

	public ShatteredArmor(int Amount, int Duration, GameObject Owner)
		: this(Amount, Duration)
	{
		this.Owner = Owner;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override bool SameAs(Effect e)
	{
		ShatteredArmor shatteredArmor = e as ShatteredArmor;
		if (shatteredArmor.Amount != Amount)
		{
			return false;
		}
		if (shatteredArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override int GetPenalty()
	{
		return Amount;
	}

	public override void IncrementPenalty()
	{
		Amount++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override string GetDetails()
	{
		return "-" + Amount + " AV";
	}

	public override string GetDescription()
	{
		return "{{r|cracked}}";
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object == null || Object.Equipped == null)
		{
			return false;
		}
		if (!(Object.GetPart("Armor") is Armor armor))
		{
			return false;
		}
		if (Amount > armor.AV)
		{
			Amount = armor.AV;
		}
		if (Amount <= 0)
		{
			return false;
		}
		if (!Object.FireEvent("ApplyShatteredArmor"))
		{
			return false;
		}
		bool result = true;
		Object.pPhysics?.PlayWorldSound("breakage", 0.5f, 0f, combat: true);
		if (Object.GetEffect("ShatteredArmor") is ShatteredArmor shatteredArmor)
		{
			shatteredArmor.UnapplyStats();
			shatteredArmor.Amount += Amount;
			shatteredArmor.ApplyStats();
			if (shatteredArmor.Duration < base.Duration)
			{
				shatteredArmor.Duration = base.Duration;
			}
			result = false;
		}
		else
		{
			ApplyStats();
		}
		Object.Equipped.ParticleText("*" + Object.ShortDisplayNameStripped + " cracked*", IComponent<GameObject>.ConsequentialColorChar(null, Object.Equipped));
		if (Object.Equipped.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(Object.Equipped.Poss(Object) + Object.GetVerb("were") + " cracked.", 'R');
		}
		return result;
	}

	private void ApplyStats()
	{
		if (base.Object != null)
		{
			Armor part = base.Object.GetPart<Armor>();
			if (part != null)
			{
				part.AV -= Amount;
			}
		}
	}

	private void UnapplyStats()
	{
		if (base.Object != null)
		{
			Armor part = base.Object.GetPart<Armor>();
			if (part != null)
			{
				part.AV += Amount;
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != GetDisplayNameEvent.ID && ID != IsRepairableEvent.ID)
		{
			return ID == RepairedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddTag("[{{r|cracked}}]");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		if (Amount > 1)
		{
			E.AdjustValue(1.0 / (double)Amount);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		base.Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
