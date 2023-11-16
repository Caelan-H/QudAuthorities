using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Trance : Effect
{
	public int MentalBonus;

	public Trance()
	{
		base.DisplayName = "trance";
	}

	public Trance(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "entranced";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(ModManager.ResolveType("XRL.World.Effects.Trance")))
		{
			base.Duration = 50;
			return true;
		}
		if (Object.FireEvent(Event.New("ApplyTrance", "Effect", this)))
		{
			DidX("enter", "a trance", "!", null, Object);
			Object.ModIntProperty("MentalMutationShift", MentalBonus, RemoveIfZero: true);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		Object.ModIntProperty("MentalMutationShift", -MentalBonus, RemoveIfZero: true);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndSegmentEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndSegmentEvent E)
	{
		if (base.Duration > 0)
		{
			ActivatedAbilities activatedAbilities = base.Object.ActivatedAbilities;
			if (activatedAbilities?.AbilityByGuid != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value._Cooldown > 0)
					{
						value.TickDown();
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "*";
				E.ColorString = "&G";
			}
		}
		return true;
	}
}
