using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Engulfed : Effect
{
	public GameObject EngulfedBy;

	public bool ChangesWereApplied;

	public int TurnsEngulfed;

	public Engulfed()
	{
		base.Duration = 1;
	}

	public Engulfed(GameObject EngulfedBy)
		: this()
	{
		this.EngulfedBy = EngulfedBy;
		base.DisplayName = "{{B|Engulfed by " + EngulfedBy.a + EngulfedBy.ShortDisplayName + "}}";
	}

	public override int GetEffectType()
	{
		return 33554464;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (EngulfedBy != null && EngulfedBy.IsValid())
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num >= 0 && num <= 30)
			{
				E.ColorString = EngulfedBy.pRender.ColorString;
				E.DetailColor = EngulfedBy.pRender.DetailColor;
				E.Tile = EngulfedBy.pRender.Tile;
				E.RenderString = EngulfedBy.pRender.RenderString;
			}
		}
		return base.Render(E);
	}

	public override string GetDetails()
	{
		if (!CheckEngulfedBy())
		{
			return null;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part.MustBeUnderstood && base.Object.IsPlayer() && !EngulfedBy.Understood())
		{
			return null;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!string.IsNullOrEmpty(part.EffectDescriptionPrefix))
		{
			stringBuilder.Append(part.EffectDescriptionPrefix);
		}
		if (part.AVBonus != 0)
		{
			if (part.AVBonus > 0)
			{
				stringBuilder.Append("+").Append(part.AVBonus).Append(" AV.\n");
			}
			else
			{
				stringBuilder.Append(part.AVBonus).Append(" AV.\n");
			}
		}
		if (part.DVPenalty != 0)
		{
			if (part.DVPenalty > 0)
			{
				stringBuilder.Append("-").Append(part.DVPenalty).Append(" DV.\n");
			}
			else
			{
				stringBuilder.Append("+").Append(-part.DVPenalty).Append(" DV.\n");
			}
		}
		if (!string.IsNullOrEmpty(part.Damage) && part.DamageChance > 0)
		{
			if (part.DamageChance >= 100)
			{
				stringBuilder.Append("Inflicts periodic damage.\n");
			}
			else
			{
				stringBuilder.Append("May inflict periodic damage.\n");
			}
		}
		stringBuilder.Append("Cannot move until @they break free.\n");
		if (part.ExitSaveTarget > 0)
		{
			if (!string.IsNullOrEmpty(part.ExitSaveStat))
			{
				stringBuilder.Append("Exiting may not succeed, and is a task dependent on " + part.ExitSaveStat + ".\n");
			}
			else
			{
				stringBuilder.Append("Exiting may not succeed.\n");
			}
		}
		if (!string.IsNullOrEmpty(part.Damage) && part.ExitDamageChance > 0)
		{
			if (part.ExitDamageChance >= 100)
			{
				if (part.ExitSaveTarget > 0)
				{
					if (part.ExitDamageFailOnly)
					{
						stringBuilder.Append("Attempting to exit and failing inflicts damage.\n");
					}
					else
					{
						stringBuilder.Append("Attempting to exit inflicts damage.\n");
					}
				}
				else
				{
					stringBuilder.Append("Exiting inflicts damage.\n");
				}
			}
			else if (part.ExitSaveTarget > 0)
			{
				if (part.ExitDamageFailOnly)
				{
					stringBuilder.Append("Attempting to exit and failing may inflict damage.\n");
				}
				else
				{
					stringBuilder.Append("Attempting to exit may inflict damage.\n");
				}
			}
			else
			{
				stringBuilder.Append("Exiting may inflict damage.\n");
			}
		}
		if (!string.IsNullOrEmpty(part.EffectDescriptionPostfix))
		{
			stringBuilder.Append(part.EffectDescriptionPostfix);
		}
		return stringBuilder.ToString().TrimEnd('\n');
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyEngulfed")))
		{
			if (Object.CurrentCell != null)
			{
				Object.CurrentCell.FireEvent(Event.New("ObjectBecomingEngulfed", "Object", Object));
			}
			Object.MovementModeChanged(null, Involuntary: true);
			Object.BodyPositionChanged(null, Involuntary: true);
			return true;
		}
		return false;
	}

	public bool IsEngulfedByValid()
	{
		if (!GameObject.validate(ref EngulfedBy))
		{
			return false;
		}
		if (EngulfedBy.CurrentCell == null)
		{
			return false;
		}
		if (base.Object == null)
		{
			return false;
		}
		if (EngulfedBy.CurrentCell != base.Object.CurrentCell)
		{
			return false;
		}
		if (!base.Object.PhaseMatches(EngulfedBy))
		{
			return false;
		}
		return true;
	}

	public bool CheckEngulfedBy()
	{
		if (!IsEngulfedByValid())
		{
			base.Duration = 0;
			if (base.Object != null)
			{
				base.Object.RemoveEffect(this);
			}
			EngulfedBy = null;
			return false;
		}
		if (EngulfedBy != null)
		{
			base.DisplayName = "{{B|engulfed by " + EngulfedBy.a + EngulfedBy.ShortDisplayName + "}}";
		}
		return true;
	}

	private void ApplyChangesCore()
	{
		if (ChangesWereApplied)
		{
			UnapplyChangesCore();
		}
		if (EngulfedBy != null)
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			if (part != null && !string.IsNullOrEmpty(part.AffectedProperties))
			{
				Dictionary<string, int> propertyMap = part.PropertyMap;
				foreach (string key in propertyMap.Keys)
				{
					base.Object.ModIntProperty(key, propertyMap[key], RemoveIfZero: true);
				}
			}
		}
		ChangesWereApplied = true;
	}

	public void ApplyChanges()
	{
		if (base.Object == null)
		{
			return;
		}
		if (ChangesWereApplied)
		{
			UnapplyChanges();
		}
		if (EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part != null && (!part.MustBeUnderstood || !base.Object.IsPlayer() || EngulfedBy.Understood()) && !part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyChangesCore();
			if (!string.IsNullOrEmpty(part.ApplyChangesEventSelf))
			{
				EngulfedBy.FireEvent(part.ApplyChangesEventSelf);
			}
			if (!string.IsNullOrEmpty(part.ApplyChangesEventUser))
			{
				base.Object.FireEvent(part.ApplyChangesEventUser);
			}
		}
	}

	private void UnapplyChangesCore()
	{
		if (!ChangesWereApplied || EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(part.AffectedProperties))
		{
			Dictionary<string, int> propertyMap = part.PropertyMap;
			foreach (string key in propertyMap.Keys)
			{
				base.Object.ModIntProperty(key, -propertyMap[key], RemoveIfZero: true);
			}
		}
		ChangesWereApplied = false;
	}

	public void UnapplyChanges()
	{
		if (base.Object == null || !ChangesWereApplied || EngulfedBy == null)
		{
			return;
		}
		Engulfing part = EngulfedBy.GetPart<Engulfing>();
		if (part != null)
		{
			UnapplyChangesCore();
			if (!string.IsNullOrEmpty(part.UnapplyChangesEventSelf))
			{
				EngulfedBy.FireEvent(part.UnapplyChangesEventSelf);
			}
			if (!string.IsNullOrEmpty(part.UnapplyChangesEventUser))
			{
				base.Object.FireEvent(part.UnapplyChangesEventUser);
			}
		}
	}

	private void ApplyStats()
	{
		if (EngulfedBy != null)
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			if (part != null)
			{
				base.StatShifter.SetStatShift(base.Object, "AV", part.AVBonus);
				base.StatShifter.SetStatShift(base.Object, "DV", -part.DVPenalty);
			}
		}
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (CheckEngulfedBy())
		{
			E.AddTag("[{{B|engulfed by " + EngulfedBy.a + EngulfedBy.DisplayNameOnly + "}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckEngulfedBy())
		{
			Engulfing part = EngulfedBy.GetPart<Engulfing>();
			part.ProcessTurnEngulfed(base.Object, ++TurnsEngulfed);
			if (ChangesWereApplied)
			{
				if (part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					UnapplyChanges();
				}
			}
			else if (!part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				ApplyChanges();
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginMove");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "LeaveCell");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		ApplyChanges();
		ApplyStats();
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		if (Object != null)
		{
			Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
			Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
			Object.UnregisterEffectEvent(this, "BeginMove");
			Object.UnregisterEffectEvent(this, "BeginTakeAction");
			Object.UnregisterEffectEvent(this, "BodyPositionChanged");
			Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
			Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
			Object.UnregisterEffectEvent(this, "LeaveCell");
			Object.UnregisterEffectEvent(this, "MovementModeChanged");
			UnapplyStats();
			UnapplyChanges();
		}
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (base.Duration > 0)
			{
				CheckEngulfedBy();
			}
		}
		else if (E.ID == "BeginMove")
		{
			if (base.Duration > 0 && !E.HasFlag("Teleporting") && CheckEngulfedBy())
			{
				base.Object.PerformMeleeAttack(EngulfedBy);
				if (!GameObject.validate(ref EngulfedBy) && base.Object != null)
				{
					base.Object.RemoveEffect(this);
				}
				return false;
			}
		}
		else if (E.ID == "LeaveCell" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			base.Object.RemoveEffect(this);
		}
		else if (E.ID == "CanChangeMovementMode" || E.ID == "CanChangeBodyPosition")
		{
			if (CheckEngulfedBy())
			{
				if (E.HasFlag("ShowMessage") && base.Object.IsPlayer())
				{
					Popup.Show("You cannot do that while engulfed by " + EngulfedBy.the + EngulfedBy.ShortDisplayName + ".");
				}
				return false;
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
			UnapplyChangesCore();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChangesCore();
			ApplyStats();
		}
		return base.FireEvent(E);
	}
}
