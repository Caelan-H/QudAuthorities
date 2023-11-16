using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class SettingLimb : Effect
{
	public SettingLimb()
	{
		base.DisplayName = "SettingLimb";
	}

	public SettingLimb(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override string GetDetails()
	{
		return "Will no longer be crippled soon.\nStops setting the limb if another action is taken or damage is taken.";
	}

	public override string GetDescription()
	{
		return "&gsetting limb";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("SettingLimb"))
		{
			base.Duration = 5;
			return false;
		}
		if (Object.FireEvent(Event.New("ApplySettingLimb", "Effect", this)))
		{
			DidX("begin", "setting a limb", null, null, Object);
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type == null || !E.Type.Contains("Pass"))
		{
			if (base.Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your limb-setting is interrupted!", 'R');
			}
			base.Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override void Remove(GameObject Object)
	{
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		Object.RegisterEffectEvent(this, "TakeDamage");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		Object.UnregisterEffectEvent(this, "TakeDamage");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 25 && num < 35)
			{
				E.Tile = null;
				E.RenderString = "Z";
				E.ColorString = "&g";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TakeDamage")
		{
			if ((E.GetParameter("Damage") as Damage).Amount > 0)
			{
				if (base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your limb-setting is interrupted!", 'R');
				}
				base.Duration = 0;
			}
		}
		else if (E.ID == "EndTurn")
		{
			if (!base.Object.CanMoveExtremities())
			{
				base.Duration = 0;
			}
			else if (base.Duration > 0)
			{
				base.Duration--;
				if (base.Duration <= 0)
				{
					base.Object.RemoveEffect("Cripple");
				}
			}
		}
		return base.FireEvent(E);
	}
}
