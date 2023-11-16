using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Wading : Effect
{
	private const int BASE_MOVE_SPEED_PENALTY = 20;

	public int? MoveSpeedShiftApplied;

	[NonSerialized]
	private long validatedOn;

	public Wading()
	{
		base.DisplayName = "{{B|wading}}";
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		int num = 16777344;
		if (base.Object == null || GetTargetMoveSpeedPenalty(base.Object) > 0)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{B|wading}}";
	}

	public override bool SuppressInLookDisplay()
	{
		return true;
	}

	public override string GetDetails()
	{
		CheckMoveSpeedPenalty();
		int num = -(MoveSpeedShiftApplied ?? 20);
		if (num > 0)
		{
			return "+" + num + " move speed.";
		}
		if (num < 0)
		{
			return num + " move speed.";
		}
		return "Moving at full speed.";
	}

	public static int GetTargetMoveSpeedPenalty(GameObject obj)
	{
		int MoveSpeedPenalty = 20;
		GetWadingPerformanceEvent.GetFor(obj, ref MoveSpeedPenalty);
		return MoveSpeedPenalty;
	}

	public void CheckMoveSpeedPenalty()
	{
		if (base.Object != null)
		{
			MoveSpeedShiftApplied = GetTargetMoveSpeedPenalty(base.Object);
			base.StatShifter.SetStatShift("MoveSpeed", MoveSpeedShiftApplied.Value);
		}
	}

	public void RemoveMoveSpeedPenalty()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Wading"))
		{
			return false;
		}
		if (!Object.CanChangeMovementMode("Wading", ShowMessage: false, Involuntary: true))
		{
			return false;
		}
		CheckMoveSpeedPenalty();
		Object.FireEvent("StartWading");
		Object.MovementModeChanged("Wading", Involuntary: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		RemoveMoveSpeedPenalty();
		Object.MovementModeChanged("NotWading", Involuntary: true);
		Object.FireEvent("StopWading");
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != EnteredCellEvent.ID)
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Validate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		Validate(E.Cell);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		Validate();
		if (base.Duration > 0)
		{
			E.AddTag("{{y|[{{B|wading}}]}}");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanChangeMovementMode")
		{
			if (base.Duration > 0)
			{
				string stringParameter = E.GetStringParameter("To");
				if (stringParameter != "Flying" && stringParameter != "Swimming" && stringParameter != "Dodging" && stringParameter != "Juking" && stringParameter != "Jumping" && stringParameter != "Charging" && stringParameter != "Stuck")
				{
					if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
					{
						Popup.Show("You cannot do that while wading.");
					}
					return false;
				}
			}
		}
		else if (E.ID == "MovementModeChanged")
		{
			string stringParameter2 = E.GetStringParameter("To");
			if (stringParameter2 == "Flying" || stringParameter2 == "Swimming")
			{
				base.Object.RemoveEffect(this);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (num > 45 && num < 55)
		{
			E.Tile = null;
			E.RenderString = "ñ";
			E.ColorString = "&B";
		}
		return true;
	}

	private void ApplyStats()
	{
		CheckMoveSpeedPenalty();
	}

	private void UnapplyStats()
	{
		RemoveMoveSpeedPenalty();
	}

	public void Validate(Cell C = null)
	{
		if (base.Object != null && (C != null || XRLCore.Core.Game.Segments > validatedOn))
		{
			if (C == null)
			{
				C = base.Object.CurrentCell;
			}
			if (C == null || !C.HasWadingDepthLiquid() || C.HasBridge() || base.Object.HasEffect("Swimming") || base.Object.IsFlying)
			{
				base.Duration = 0;
				base.Object.RemoveEffect(this, NeedStackCheck: false);
			}
			validatedOn = XRLCore.Core.Game.Segments;
		}
	}
}
