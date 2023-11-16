using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class EmptyTheClips : Effect
{
	public EmptyTheClips()
	{
		base.DisplayName = "Empty the Clips";
	}

	public EmptyTheClips(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 128;
	}

	public override string GetDetails()
	{
		return "Fires pistols twice as quickly.";
	}

	public override string GetDescription()
	{
		return "emptying the clips";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect(base.ClassName))
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyEmptyTheClips", "Effect", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You clasp the trigger like it's your darling.");
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EnteredCell");
		Object.RegisterEffectEvent(this, "FiredMissileWeapon");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EnteredCell");
		Object.UnregisterEffectEvent(this, "FiredMissileWeapon");
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 45 && num < 55)
			{
				E.Tile = null;
				E.RenderString = "\u001a";
				E.ColorString = "&B";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			base.Duration--;
		}
		return base.FireEvent(E);
	}
}
