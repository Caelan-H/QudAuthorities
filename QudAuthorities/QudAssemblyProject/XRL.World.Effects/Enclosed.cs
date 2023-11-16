using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Enclosed : Effect
{
	public GameObject EnclosedBy;

	public bool ChangesWereApplied;

	public int TurnsEnclosed;

	public Enclosed()
	{
		base.Duration = 1;
	}

	public Enclosed(GameObject EnclosedBy)
		: this()
	{
		this.EnclosedBy = EnclosedBy;
		base.DisplayName = "{{C|enclosed in " + EnclosedBy.a + EnclosedBy.ShortDisplayName + "}}";
	}

	public override int GetEffectType()
	{
		return 32;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (!CheckEnclosedBy())
		{
			return null;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part.MustBeUnderstood && base.Object.IsPlayer() && !EnclosedBy.Understood())
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
		stringBuilder.Append("Must spend a turn exiting before moving.\n");
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
		if (Object.FireEvent("ApplyEnclosed"))
		{
			if (Object.CurrentCell != null)
			{
				Object.CurrentCell.FireEvent(Event.New("ObjectBecomingEnclosed", "Object", Object));
			}
			return true;
		}
		return false;
	}

	public bool IsEnclosedByValid()
	{
		if (EnclosedBy == null)
		{
			return false;
		}
		if (EnclosedBy.IsInvalid())
		{
			return false;
		}
		if (EnclosedBy.CurrentCell == null)
		{
			return false;
		}
		if (base.Object == null)
		{
			return false;
		}
		if (base.Object.CurrentCell == null)
		{
			return false;
		}
		if (EnclosedBy.CurrentCell != base.Object.CurrentCell)
		{
			return false;
		}
		return true;
	}

	public bool CheckEnclosedBy()
	{
		if (!IsEnclosedByValid())
		{
			EnclosedBy = null;
			base.Object.RemoveEffect(this);
			return false;
		}
		base.DisplayName = "{{C|enclosed in " + EnclosedBy.a + EnclosedBy.ShortDisplayName + "}}";
		return true;
	}

	private void ApplyChangesCore()
	{
		if (ChangesWereApplied)
		{
			UnapplyChangesCore();
		}
		if (EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(part.AffectedProperties))
		{
			Dictionary<string, int> propertyMap = part.PropertyMap;
			foreach (string key in propertyMap.Keys)
			{
				base.Object.ModIntProperty(key, propertyMap[key], RemoveIfZero: true);
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
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if ((!part.MustBeUnderstood || !base.Object.IsPlayer() || EnclosedBy.Understood()) && !part.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ApplyChangesCore();
			if (!string.IsNullOrEmpty(part.ApplyChangesEventSelf))
			{
				EnclosedBy.FireEvent(part.ApplyChangesEventSelf);
			}
			if (!string.IsNullOrEmpty(part.ApplyChangesEventUser))
			{
				base.Object.FireEvent(part.ApplyChangesEventUser);
			}
		}
	}

	private void UnapplyChangesCore()
	{
		if (!ChangesWereApplied || EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
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
		if (base.Object == null || !ChangesWereApplied || EnclosedBy == null)
		{
			return;
		}
		Enclosing part = EnclosedBy.GetPart<Enclosing>();
		if (part != null)
		{
			UnapplyChangesCore();
			if (!string.IsNullOrEmpty(part.UnapplyChangesEventSelf))
			{
				EnclosedBy.FireEvent(part.UnapplyChangesEventSelf);
			}
			if (!string.IsNullOrEmpty(part.UnapplyChangesEventUser))
			{
				base.Object.FireEvent(part.UnapplyChangesEventUser);
			}
			ChangesWereApplied = false;
		}
	}

	private void ApplyStats()
	{
		if (EnclosedBy != null)
		{
			Enclosing part = EnclosedBy.GetPart<Enclosing>();
			base.StatShifter.SetStatShift(base.Object, "AV", part.AVBonus);
			base.StatShifter.SetStatShift(base.Object, "DV", -part.DVPenalty);
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
		if (CheckEnclosedBy())
		{
			E.AddTag("[{{B|enclosed in " + EnclosedBy.a + EnclosedBy.ShortDisplayName + "}}]");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (CheckEnclosedBy())
		{
			Enclosing enclosing = EnclosedBy.GetPart("Enclosing") as Enclosing;
			enclosing.ProcessTurnEnclosed(base.Object, ++TurnsEnclosed);
			if (ChangesWereApplied)
			{
				if (enclosing.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					UnapplyChanges();
				}
			}
			else if (!enclosing.IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
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
		Object.RegisterEffectEvent(this, "BodyPositionChanged");
		Object.RegisterEffectEvent(this, "CanChangeBodyPosition");
		Object.RegisterEffectEvent(this, "CanChangeMovementMode");
		Object.RegisterEffectEvent(this, "CanMoveExtremities");
		Object.RegisterEffectEvent(this, "LeaveCell");
		Object.RegisterEffectEvent(this, "MovementModeChanged");
		ApplyChanges();
		ApplyStats();
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BodyPositionChanged");
		Object.UnregisterEffectEvent(this, "CanChangeBodyPosition");
		Object.UnregisterEffectEvent(this, "CanChangeMovementMode");
		Object.UnregisterEffectEvent(this, "CanMoveExtremities");
		Object.UnregisterEffectEvent(this, "LeaveCell");
		Object.UnregisterEffectEvent(this, "MovementModeChanged");
		UnapplyStats();
		UnapplyChanges();
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "LeaveCell" || E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged")
		{
			if (CheckEnclosedBy() && !EnclosedBy.GetPart<Enclosing>().ExitEnclosure(base.Object, E, this))
			{
				return false;
			}
		}
		else if (E.ID == "CanChangeMovementMode" || E.ID == "CanChangeBodyPosition")
		{
			if (CheckEnclosedBy() && EnclosedBy.GetPart<Enclosing>().EnclosureExitImpeded(base.Object, E?.HasFlag("ShowMessage") ?? false, this))
			{
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
