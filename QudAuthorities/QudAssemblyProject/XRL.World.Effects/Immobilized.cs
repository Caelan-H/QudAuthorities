using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Immobilized : Effect
{
	public string Text = "immobilized";

	public int SaveTarget = 20;

	public string SaveStat = "Agility";

	public string SaveVs = "Immobilization";

	public int SaveTargetTurnDivisor = 5;

	public int Turns;

	public bool LinkedToProne = true;

	public Immobilized()
	{
		base.DisplayName = "immobilized";
		base.Duration = 1;
	}

	public Immobilized(int SaveTarget)
		: this()
	{
		this.SaveTarget = SaveTarget;
	}

	public Immobilized(int SaveTarget, string SaveStat)
		: this(SaveTarget)
	{
		this.SaveStat = SaveStat;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs)
		: this(SaveTarget, SaveStat)
	{
		this.SaveVs = SaveVs;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs, string Text)
		: this(SaveTarget, SaveStat, SaveVs)
	{
		this.Text = Text;
	}

	public Immobilized(int SaveTarget, string SaveStat, string SaveVs, string Text, bool LinkedToProne)
		: this(SaveTarget, SaveStat, SaveVs, Text)
	{
		this.LinkedToProne = LinkedToProne;
	}

	public override int GetEffectType()
	{
		return 100663424;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Can't move or stand up.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.Statistics.ContainsKey("Energy"))
		{
			return false;
		}
		if (!Object.CanChangeBodyPosition("Immobilized", ShowMessage: false, Involuntary: true))
		{
			return false;
		}
		if (Object.FireEvent("ApplyImmobilized"))
		{
			DidX("are", Text, "!", null, null, Object);
			if (!Object.IsPlayer() && Visible())
			{
				Object.ParticleText("*" + Text + "*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
			}
			Object.Statistics["Energy"].BaseValue = 0;
			Object.BodyPositionChanged(null, Involuntary: true);
			return true;
		}
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "CanStandUp");
		Object.RegisterEffectEvent(this, "EndTurn");
		Object.RegisterEffectEvent(this, "IsMobile");
		Object.RegisterEffectEvent(this, "LeaveCell");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "CanStandUp");
		Object.UnregisterEffectEvent(this, "EndTurn");
		Object.UnregisterEffectEvent(this, "IsMobile");
		Object.UnregisterEffectEvent(this, "LeaveCell");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		base.Unregister(Object);
	}

	public int GetEffectiveSaveTarget()
	{
		int num = SaveTarget;
		if (SaveTargetTurnDivisor != 0)
		{
			num -= Turns / SaveTargetTurnDivisor;
		}
		return num;
	}

	public void EndImmobilization()
	{
		DidX("are", "no longer " + Text, null, null, base.Object);
		base.Object.RemoveEffect(this);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" || E.ID == "CanStandUp")
		{
			if (base.Duration > 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (Turns > 0 && base.Object.MakeSave(SaveStat, GetEffectiveSaveTarget(), null, null, SaveVs))
			{
				EndImmobilization();
			}
			else
			{
				Turns++;
			}
		}
		else if (E.ID == "LeaveCell")
		{
			if (base.Duration > 0 && !E.HasParameter("Teleporting") && !E.HasParameter("Dragging") && !E.HasParameter("Forced"))
			{
				if (base.Object.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You are " + Text + "&y!");
				}
				base.Object.UseEnergy(1000);
				return false;
			}
		}
		else if (E.ID == "CanChangeMovementMode" || E.ID == "CanChangeBodyPosition")
		{
			if (base.Duration > 0 && !E.HasFlag("Involuntary"))
			{
				if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.Show("You are {{|" + Text + "}}!");
				}
				return false;
			}
		}
		else if (E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			base.Object.RemoveEffect(this);
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 40 && num < 60)
			{
				E.RenderString = "\u001c";
				E.ColorString = "&K";
			}
		}
		return true;
	}
}
