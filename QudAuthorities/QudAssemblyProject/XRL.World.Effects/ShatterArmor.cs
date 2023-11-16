using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ShatterArmor : IShatterEffect
{
	public int AVPenalty = 1;

	public GameObject Owner;

	public ShatterArmor()
	{
		base.DisplayName = "Shatter Armor";
	}

	public ShatterArmor(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override bool SameAs(Effect e)
	{
		ShatterArmor shatterArmor = e as ShatterArmor;
		if (shatterArmor.AVPenalty != AVPenalty)
		{
			return false;
		}
		if (shatterArmor.Owner != Owner)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDescription()
	{
		return "{{r|cleaved ({{C|-" + AVPenalty + " AV}})}}";
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-" + AVPenalty + " AV";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != IsRepairableEvent.ID)
		{
			return ID == RepairedEvent.ID;
		}
		return true;
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

	public override int GetPenalty()
	{
		return AVPenalty;
	}

	public override void IncrementPenalty()
	{
		AVPenalty++;
	}

	public override GameObject GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(GameObject Owner)
	{
		this.Owner = Owner;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			bool flag = Object.BaseStat("AV") > 0;
			List<GameObject> list = Event.NewGameObjectList();
			Body body = Object.Body;
			if (body != null)
			{
				foreach (BodyPart equippedPart in body.GetEquippedParts())
				{
					if (equippedPart.Equipped != null)
					{
						if (equippedPart.Equipped.GetPart("Armor") is Armor armor && armor.AV > 0)
						{
							list.Add(equippedPart.Equipped);
						}
					}
					else if (flag && equippedPart.Contact && !equippedPart.Abstract && !equippedPart.Extrinsic)
					{
						list.Add(null);
					}
				}
			}
			GameObject randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				randomElement.ApplyEffect(new ShatteredArmor(AVPenalty, base.Duration));
				return false;
			}
			if (!flag)
			{
				return false;
			}
		}
		else if (Object.Stat("AV") <= 0)
		{
			return false;
		}
		if (Object.Energy == null)
		{
			return false;
		}
		if (Object.GetEffect("ShatterArmor") is ShatterArmor shatterArmor)
		{
			Object.pPhysics?.PlayWorldSound("breakage", 0.5f, 0f, combat: true);
			if (base.Duration > shatterArmor.Duration)
			{
				shatterArmor.Duration = base.Duration;
			}
			shatterArmor.UnapplyStats();
			shatterArmor.AVPenalty += AVPenalty;
			shatterArmor.ApplyStats();
			Object.ParticleText("*cleave (-" + shatterArmor.AVPenalty + " AV)*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			return false;
		}
		if (!Object.FireEvent("ApplyShatterArmor"))
		{
			return false;
		}
		Object.pPhysics?.PlayWorldSound("breakage", 0.5f, 0f, combat: true);
		ApplyStats();
		Object.ParticleText("*cleave (-" + AVPenalty + " AV)*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("AV", -AVPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts();
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

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 40)
			{
				E.Tile = null;
				E.RenderString = "X";
				E.ColorString = "&B^c";
			}
		}
		return true;
	}
}
