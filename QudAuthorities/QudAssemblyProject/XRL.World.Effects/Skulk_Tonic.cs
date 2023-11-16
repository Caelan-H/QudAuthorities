using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Skulk_Tonic : ITonicEffect
{
	public Guid BurrowingClawsID;

	public Guid DarkVisionID;

	public Skulk_Tonic()
	{
	}

	public Skulk_Tonic(int Duration)
		: this()
	{
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

	public override string GetDescription()
	{
		return "{{Y|Skulk}} tonic";
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "+40% Move Speed at night and underground.\n+4 Agility at night and underground.\n-20% Move Speed in the daylight.\n-3 Agility in the daylight.\nCan see in the dark (radius 8).\nSuffers double damage from light-based attacks.";
		}
		return "Has grown burrowing claws.\n+25% Move Speed at night and underground.\n+3 Agility at night and underground.\n-20% Move Speed in the daylight.\n-3 Agility in the daylight.\nCan see in the dark (radius 8).\nSuffers double damage from light-based attacks.";
	}

	public void RemoveBonus()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public void ApplyBonus()
	{
		if (base.Object.CurrentCell != null)
		{
			int num;
			int amount;
			if (base.Object.CurrentCell.ParentZone.Z <= 10 && Calendar.IsDay())
			{
				num = (int)((double)(-base.Object.BaseStat("MoveSpeed")) * 0.2);
				amount = -3;
			}
			else if (base.Object.IsTrueKin())
			{
				num = (int)((float)base.Object.BaseStat("MoveSpeed") * 0.4f);
				amount = 4;
			}
			else
			{
				num = (int)((float)base.Object.BaseStat("MoveSpeed") * 0.25f);
				amount = 3;
			}
			base.StatShifter.SetStatShift("MoveSpeed", -num);
			base.StatShifter.SetStatShift("Agility", amount);
		}
	}

	private void ApplyChanges()
	{
		ApplyBonus();
		Mutations mutations = base.Object.RequirePart<Mutations>();
		if (base.Object.IsMutant())
		{
			int bonus = (base.Object.HasPart("BurrowingClaws") ? "2-3".RollCached() : 6);
			BurrowingClawsID = mutations.AddMutationMod("BurrowingClaws", bonus, Mutations.MutationModifierTracker.SourceType.Tonic);
		}
		DarkVisionID = mutations.AddMutationMod("DarkVision", 1, Mutations.MutationModifierTracker.SourceType.Tonic);
		base.Object.GetPart<DarkVision>().Radius += 3;
	}

	private void UnapplyChanges()
	{
		RemoveBonus();
		Mutations mutations = base.Object.RequirePart<Mutations>();
		if (BurrowingClawsID != Guid.Empty)
		{
			mutations.RemoveMutationMod(BurrowingClawsID);
		}
		base.Object.GetPart<DarkVision>().Radius -= 3;
		if (DarkVisionID != Guid.Empty)
		{
			mutations.RemoveMutationMod(DarkVisionID);
		}
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetHeartCount() > 1)
			{
				Popup.Show("Your hearts begin to beat faster and your pupils dilate.");
			}
			else
			{
				Popup.Show("Your heart begins to beat faster and your pupils dilate.");
			}
		}
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		if (Object.GetEffect("Skulk_Tonic") is Skulk_Tonic skulk_Tonic)
		{
			skulk_Tonic.Duration += base.Duration;
			return false;
		}
		ApplyChanges();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your heart rate returns to normal and your pupils shrink.");
		}
		UnapplyChanges();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			ApplyBonus();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeApplyDamage");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "Overdose");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeApplyDamage");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "Overdose");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Light") || damage.HasAttribute("Laser"))
			{
				damage.Amount = (int)Math.Ceiling((float)damage.Amount * 2f);
			}
		}
		else if (E.ID == "Overdose")
		{
			if (base.Duration > 0)
			{
				base.Duration = 0;
				ApplyOverdose(base.Object);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			RemoveBonus();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyBonus();
		}
		return base.FireEvent(E);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		ApplyOverdose(Object);
	}

	public static void ApplyOverdose(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetLongProperty("Overdosing") == 1)
			{
				Popup.Show("Your mutant physiology reacts adversely to the tonic. Your field of vision erupts into a plane of blinding, white light.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. Your field of vision erupts into a plane of blinding, white light.");
			}
		}
		Object.ApplyEffect(new Blind(Stat.Random(1, 10) + 20));
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 15 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "|";
			E.ColorString = "&K";
		}
		return true;
	}
}
