using System;

namespace XRL.World.Parts;

[Serializable]
public class AIShootCooldown : IPart
{
	public string Cooldown = "1d6";

	public int CurrentCooldown;

	public long LastFireTimeTick;

	public override bool SameAs(IPart p)
	{
		if ((p as AIShootCooldown).Cooldown != Cooldown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIWantUseWeapon");
		Object.RegisterPartEvent(this, "AIAfterMissile");
		base.Register(Object);
	}

	public bool CooldownActive()
	{
		if (LastFireTimeTick != The.Game.TimeTicks)
		{
			return LastFireTimeTick + CurrentCooldown > The.Game.TimeTicks;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIWantUseWeapon")
		{
			if (CooldownActive())
			{
				return false;
			}
		}
		else if (E.ID == "AIAfterMissile")
		{
			LastFireTimeTick = The.Game.TimeTicks;
			CurrentCooldown = Cooldown.RollCached();
		}
		return base.FireEvent(E);
	}
}
