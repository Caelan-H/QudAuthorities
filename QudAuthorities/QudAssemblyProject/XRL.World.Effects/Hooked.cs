using System;
using XRL.Core;
using XRL.Language;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Hooked : Effect
{
	public int SaveTarget;

	public string HookedMessage = "You are hooked!";

	public GameObject HookingWeapon;

	public Hooked()
	{
		base.DisplayName = "hooked";
		base.Duration = 9;
	}

	public Hooked(GameObject HookingWeapon)
		: this()
	{
		this.HookingWeapon = HookingWeapon;
	}

	public Hooked(GameObject HookingWeapon, int SaveTarget)
		: this(HookingWeapon)
	{
		this.SaveTarget = SaveTarget;
	}

	public Hooked(GameObject HookingWeapon, int SaveTarget, int Duration)
		: this(HookingWeapon, SaveTarget)
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override string GetDetails()
	{
		return "Is being dragged.\nCan't move without breaking free first.";
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
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == CommandTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (HookingWeapon?.Equipped?.GetPart("Axe_HookAndDrag") is Axe_HookAndDrag axe_HookAndDrag)
			{
				axe_HookAndDrag.Validate();
			}
			else
			{
				base.Object.RemoveEffect(this);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (base.Duration > 0 && (!base.Object.FireEvent("BeforeGrabbed") || base.Object.MakeSave("Strength", SaveTarget, HookingWeapon.Equipped, null, "HookAndDrag Continue Grab Restraint Escape", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, HookingWeapon)))
		{
			DidX("break", "free from " + WhatSubjectIsHeldBy(), "!", null, base.Object);
			base.Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(base.Object));
			base.Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginMove");
		Object.RegisterEffectEvent(this, "Juked");
		Object.RegisterEffectEvent(this, "IsMobile");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginMove");
		Object.UnregisterEffectEvent(this, "Juked");
		Object.UnregisterEffectEvent(this, "IsMobile");
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
			E.RenderString = "X";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile")
		{
			return false;
		}
		if (E.ID == "Juked")
		{
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "BeginMove" && HookingWeapon?.Equipped?.GetPart("Axe_HookAndDrag") is Axe_HookAndDrag axe_HookAndDrag && E.GetGameObjectParameter("Dragging") != HookingWeapon && axe_HookAndDrag.Validate() && E.GetParameter("DestinationCell") is Cell cell)
		{
			foreach (GameObject item in cell.LoopObjectsWithPart("Brain"))
			{
				if (!item.pBrain.IsHostileTowards(base.Object) || !item.HasPart("Combat"))
				{
					continue;
				}
				goto IL_01a6;
			}
			if (E.HasParameter("Teleporting"))
			{
				base.Object.ParticleText("*broke free*", IComponent<GameObject>.ConsequentialColorChar(base.Object));
				base.Duration = 0;
			}
			else if (base.Duration > 0 && !E.HasParameter("Dragging") && !E.HasParameter("Teleporting"))
			{
				if (base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(HookedMessage, 'R');
				}
				base.Object.UseEnergy(1000, "Movement Failure");
				return false;
			}
		}
		goto IL_01a6;
		IL_01a6:
		return base.FireEvent(E);
	}

	public string WhatSubjectIsHeldBy()
	{
		GameObject.validate(ref HookingWeapon);
		if (HookingWeapon == null)
		{
			return "the hook maneuver";
		}
		if (HookingWeapon.HasProperName || HookingWeapon.Equipped == null)
		{
			return HookingWeapon.the + HookingWeapon.ShortDisplayName;
		}
		return Grammar.MakePossessive(HookingWeapon.Equipped.the + HookingWeapon.Equipped.ShortDisplayName) + " " + HookingWeapon.ShortDisplayName;
	}
}
