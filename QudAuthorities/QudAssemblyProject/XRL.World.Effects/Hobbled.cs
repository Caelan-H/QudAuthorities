using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Hobbled : Effect
{
	public int Penalty;

	public Hobbled()
	{
		base.DisplayName = "{{C|hobbled}}";
	}

	public override int GetEffectType()
	{
		return 50332672;
	}

	public Hobbled(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "-50% Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasStat("MoveSpeed") && Object.FireEvent("ApplyHobble"))
		{
			DidX("are", "hobbled", "!", null, null, Object);
			Object.ParticleText("*hobbled*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			if (!(Object.GetEffect("Hobbled") is Hobbled hobbled))
			{
				Penalty = Object.Statistics["MoveSpeed"].Value / 2;
				base.StatShifter.SetStatShift("MoveSpeed", Penalty);
				return true;
			}
			if (hobbled.Duration < base.Duration)
			{
				hobbled.Duration = base.Duration;
			}
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.StatShifter.RemoveStatShifts();
		Penalty = 0;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			base.Duration--;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 50 && num < 60)
			{
				E.RenderString = "\u000f";
				E.ColorString = base.Object.GetTag("BleedParticleColor", "&r^r");
			}
		}
		return true;
	}
}
