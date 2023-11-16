using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Refresh : Effect
{
	public int Damage = 2;

	public Refresh()
	{
		base.DisplayName = "{{G|refreshed}}";
	}

	public Refresh(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 4;
	}

	public override bool SameAs(Effect e)
	{
		if ((e as Refresh).Damage != Damage)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override string GetDetails()
	{
		return "5% HP healed per turn.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
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
			if (base.Object.HasStat("Hitpoints"))
			{
				base.Object.Heal(Math.Max(base.Object.GetStat("Hitpoints").BaseValue / 20, 1));
			}
			if (base.Duration != 9999)
			{
				base.Duration--;
			}
		}
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

	public override bool Render(RenderEvent E)
	{
		if (base.Duration <= 0)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "\u0001";
			E.ColorString = "&G";
		}
		return true;
	}
}
