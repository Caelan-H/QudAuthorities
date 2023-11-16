using System;
using System.Text;

namespace XRL.World.Effects;

[Serializable]
public class MemberOfPsychicBattle : Effect
{
	public GameObjectReference other;

	public int turnsRemaining = 10;

	public bool isAttacker;

	public MemberOfPsychicBattle()
	{
		base.Duration = 9999;
	}

	public MemberOfPsychicBattle(GameObject other, bool isAttacker)
		: this()
	{
		this.other = other.takeReference();
		this.isAttacker = isAttacker;
	}

	public override int GetEffectType()
	{
		int num = 32770;
		if (!isAttacker)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override string GetDescription()
	{
		return "locked in psychic battle";
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Locked in psychic battle with a foe. (").Append(turnsRemaining.Things("turn")).Append(" remaining)");
		if (turnsRemaining <= 5 && base.Object.CanHaveNosebleed())
		{
			stringBuilder.Append("\nHas a bloody nose.");
		}
		return stringBuilder.ToString();
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeforeTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeforeTakeAction");
		base.Unregister(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID)
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsInvalid())
		{
			base.Object?.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (other.go() == null)
		{
			base.Object.RemoveEffect(this);
			return true;
		}
		Cell cell = base.Object.CurrentCell;
		if (cell != null && (base.Object.IsPlayer() || other.go().IsPlayer()))
		{
			cell.ParentZone?.AddExplored(cell.X, cell.Y, 0);
			cell.ParentZone?.AddLight(cell.X, cell.Y, 0, LightLevel.Omniscient);
		}
		return base.HandleEvent(E);
	}

	public bool IsInvalid()
	{
		GameObject gameObject = other?.go();
		if (gameObject == null)
		{
			return true;
		}
		if (gameObject.IsInvalid())
		{
			return true;
		}
		if (!gameObject.InSameZone(base.Object))
		{
			return true;
		}
		if (!gameObject.HasEffect("MemberOfPsychicBattle"))
		{
			return true;
		}
		if (isAttacker && !base.Object.IsPlayer() && base.Object.IsAlliedTowards(gameObject))
		{
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction")
		{
			if (IsInvalid())
			{
				base.Object.RemoveEffect(this);
			}
			if (base.Object != null && !base.Object.IsPlayer() && isAttacker)
			{
				base.Object.UseEnergy(1000, "Pass");
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
