using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class NocturnalApexed : Effect
{
	public int MoveSpeedShift = -10;

	public int AgilityShift = 6;

	public NocturnalApexed()
	{
		base.DisplayName = "{{r|prowling}}";
	}

	public NocturnalApexed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public NocturnalApexed(int Duration, int MoveSpeedShift, int AgilityShift)
		: this(Duration)
	{
		this.MoveSpeedShift = MoveSpeedShift;
		this.AgilityShift = AgilityShift;
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
		return "+6 Agility\n+10 Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyNocturnalApex", "Event", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You start to prowl.", 'g');
		}
		ApplyChanges();
		return true;
	}

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift("MoveSpeed", MoveSpeedShift);
		base.StatShifter.SetStatShift("Agility", AgilityShift);
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&K";
		}
		return true;
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
		base.Duration--;
		Cell cell = base.Object?.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			base.Duration = 0;
		}
		else if (base.Duration > 0)
		{
			base.Duration--;
		}
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
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}
}
