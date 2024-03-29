using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Stressed : Effect
{
	public int Bonus;

	public int Level;

	public int Counter;

	public Stressed()
	{
		base.DisplayName = "{{B|stressed}}";
	}

	public Stressed(int Duration, int Level)
		: this()
	{
		this.Level = Level;
		base.Duration = Duration;
		Bonus = 17 + 7 * Level;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "+" + Bonus + " Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyStressed", "Event", this)))
		{
			if (Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your body flushes with adrenaline!", 'g');
			}
			Object.GetStat("Speed").Bonus += Bonus;
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Your adrenaline level returns to normal!", 'R');
		}
		Object.GetStat("Speed").Bonus -= Bonus;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			if (base.Object.OnWorldMap())
			{
				base.Duration = 0;
			}
			else if (base.Duration != 9999)
			{
				base.Duration--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (base.Duration > 0)
		{
			Counter += 30 + 50 * Level;
			int num = 0;
			while (Counter > 100)
			{
				Counter -= 100;
				num++;
			}
			if (num > 0)
			{
				base.Object.TakeDamage(num, "from %t adrenal stress!", "Metabolic Unavoidable", null, null, base.Object);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&r";
		}
		return true;
	}
}
