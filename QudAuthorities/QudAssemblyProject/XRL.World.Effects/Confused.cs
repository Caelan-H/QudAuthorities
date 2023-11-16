using System;
using System.Text;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Confused : Effect
{
	public int Level;

	public int AppliedPlayerConfusion;

	public int MentalPenalty;

	public Confused()
	{
		base.DisplayName = "{{R|confused}}";
	}

	public Confused(int Duration, int Level, int MentalPenalty)
		: this()
	{
		base.Duration = Duration;
		this.Level = Level;
		this.MentalPenalty = MentalPenalty;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Acts semi-randomly.\n-").Append(Level).Append(" DV\n-")
			.Append(Level)
			.Append(" MA");
		if (MentalPenalty > 0)
		{
			stringBuilder.Append("\n-").Append(MentalPenalty).Append(" to all mental attributes");
		}
		return stringBuilder.ToString();
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.pBrain == null)
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyConfusion"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyConfusion"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Confusion", this))
		{
			return false;
		}
		if (Object.HasEffect("Confused"))
		{
			return false;
		}
		ApplyChanges();
		DidX("become", "confused", "!", null, null, Object);
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.IsPlayer())
		{
			Object.pBrain.Goals.Clear();
			Object.pBrain.Target = null;
		}
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges()
	{
		base.StatShifter.SetStatShift(base.Object, "DV", -Level);
		base.StatShifter.SetStatShift(base.Object, "MA", -Level);
		base.StatShifter.SetStatShift(base.Object, "Willpower", -MentalPenalty);
		base.StatShifter.SetStatShift(base.Object, "Intelligence", -MentalPenalty);
		base.StatShifter.SetStatShift(base.Object, "Ego", -MentalPenalty);
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts();
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration == 0)
		{
			return true;
		}
		int num = XRLCore.CurrentFrame % 60;
		if (num > 35 && num < 60)
		{
			E.Tile = null;
			E.RenderString = "?";
			E.ColorString = "&R";
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == base.Object)
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = base.Object.Poss("mind") + " is in disarray.";
			}
			else
			{
				E.Message = base.Object.T() + base.Object.GetVerb("don't") + " seem to understand you.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		Object.RegisterEffectEvent(this, "CanApplyConfusion");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		Object.UnregisterEffectEvent(this, "CanApplyConfusion");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (base.Duration > 0)
			{
				base.Duration--;
			}
			if (base.Duration > 0 && !base.Object.IsPlayer())
			{
				int num = Stat.Random(1, 100);
				if (num <= 50)
				{
					base.Object.Move(Directions.GetRandomDirection());
					base.Object.UseEnergy(base.Object.Energy.Value);
					return false;
				}
				if (num <= 60)
				{
					base.Object.Target = base.currentCell?.GetLocalAdjacentCells(5)?.GetRandomElement()?.GetObjectsInCell()?.GetRandomElement();
					base.Object.UseEnergy(base.Object.Energy.Value);
					return false;
				}
			}
		}
		else
		{
			if (E.ID == "CanApplyConfusion")
			{
				return false;
			}
			if (E.ID == "BeforeDeepCopyWithoutEffects")
			{
				UnapplyChanges();
			}
			else if (E.ID == "AfterDeepCopyWithoutEffects")
			{
				ApplyChanges();
			}
		}
		return base.FireEvent(E);
	}
}
