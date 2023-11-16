using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class AxonsInflated : Effect
{
	public int Bonus;

	public AxonsInflated()
	{
		base.DisplayName = "{{g|hyper-responsive}}";
	}

	public AxonsInflated(int Duration, int Bonus)
		: this()
	{
		base.Duration = Duration;
		this.Bonus = Bonus;
	}

	public override int GetEffectType()
	{
		return 83894272;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "+" + Bonus + " Quickness\nWill become sluggish soon.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyStressed", "Event", this)))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The hurdles that separate the will and the way begin to collapse.", 'g');
			}
			base.StatShifter.SetStatShift("Speed", Bonus);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		base.StatShifter.RemoveStatShifts();
		Object.ApplyEffect(new AxonsDeflated(10, 10));
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

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&C";
		}
		return true;
	}
}
