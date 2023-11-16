using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class AxonsDeflated : Effect
{
	public int Penalty;

	public AxonsDeflated()
	{
		base.DisplayName = "{{r|sluggish}}";
	}

	public AxonsDeflated(int Duration, int Penalty)
		: this()
	{
		base.Duration = Duration;
		this.Penalty = Penalty;
	}

	public override int GetEffectType()
	{
		return 117448704;
	}

	public override string GetDetails()
	{
		return "-" + Penalty + " Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyStressed", "Event", this)))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("&rYou start to feel sluggish.");
			}
			base.StatShifter.SetStatShift("Speed", -Penalty);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.StatShifter.RemoveStatShifts();
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
			Cell cell = base.Object.CurrentCell;
			if (cell == null || cell.OnWorldMap())
			{
				base.Duration = 0;
			}
			else if (base.Duration > 0)
			{
				base.Duration--;
			}
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&r";
		}
		return true;
	}
}
