using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Exhausted : Effect
{
	public Exhausted()
	{
		base.DisplayName = "{{K|exhaustion}}";
	}

	public Exhausted(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override string GetDetails()
	{
		return "Can't take actions.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyExhausted")))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You are {{K|exhausted}}!");
		}
		Object.ParticleText("*exhausted*", 'C');
		Object.ForfeitTurn();
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (base.Duration != 9999)
			{
				base.Duration--;
			}
			if (base.Object.IsPlayer())
			{
				XRLCore.Core.RenderBase();
				Popup.Show("You are too exhausted to act!");
			}
			E.PreventAction = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == base.Object)
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = base.Object.Poss("mind") + " is present, but doesn't seem to be responding to you at all.";
			}
			else
			{
				E.Message = base.Object.T() + base.Object.GetVerb("stare") + " at you dully.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "CanMoveExtremities");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "CanMoveExtremities");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanMoveExtremities" && base.Duration > 0 && !E.HasFlag("Involuntary"))
		{
			if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
			{
				Popup.ShowFail("You are too exhausted to do that.");
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 10 && num < 25)
		{
			E.Tile = null;
			E.RenderString = "_";
			E.ColorString = "&C^c";
		}
		return true;
	}
}
