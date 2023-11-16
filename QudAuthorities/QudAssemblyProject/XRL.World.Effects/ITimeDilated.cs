namespace XRL.World.Effects;

public abstract class ITimeDilated : Effect
{
	public int SpeedPenalty;

	public ITimeDilated()
	{
		base.DisplayName = "time-dilated";
		base.Duration = 1;
	}

	public ITimeDilated(int SpeedPenalty)
		: this()
	{
		this.SpeedPenalty = SpeedPenalty;
	}

	public abstract bool DoTimeDilationVisualEffects();

	public override int GetEffectType()
	{
		return 117444608;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check())
		{
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 1);
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "time-dilated ({{C|" + -SpeedPenalty + "}} Quickness)";
	}

	public override string GetDetails()
	{
		return -SpeedPenalty + " Quickness";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "ApplyTimeDilated");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanApplyTimeDilated");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "ApplyTimeDilated");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanApplyTimeDilated");
		base.Unregister(Object);
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		SpeedPenalty = 0;
		base.Remove(Object);
	}

	public virtual void ApplyChanges()
	{
		base.StatShifter.SetStatShift("Speed", -SpeedPenalty);
	}

	public virtual void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && DoTimeDilationVisualEffects())
		{
			E.ColorString += "^b";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanApplyTimeDilated" || E.ID == "ApplyTimeDilated")
		{
			return false;
		}
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
