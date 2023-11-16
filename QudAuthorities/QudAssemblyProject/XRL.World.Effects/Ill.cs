using System;

namespace XRL.World.Effects;

[Serializable]
public class Ill : Effect
{
	public int Level = 1;

	public string Message = "The poison begins to abate, but you still feel nauseous.";

	public Ill()
	{
		base.DisplayName = "{{g|illness}}";
	}

	public Ill(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Ill(int Duration, string Message)
		: this()
	{
		base.Duration = Duration;
		this.Message = Message;
	}

	public Ill(int Duration, int Level)
		: this(Duration)
	{
		this.Level = Level;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 117440516;
	}

	public override string GetDescription()
	{
		return "{{g|ill}}";
	}

	public override string GetDetails()
	{
		return "Doesn't heal hit points naturally.\nExternal healing is only half as effective.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetEffect("Ill") is Ill ill)
		{
			if (base.Duration > ill.Duration)
			{
				ill.Duration = base.Duration;
			}
			return false;
		}
		if (Object.FireEvent("ApplyIll"))
		{
			if (!string.IsNullOrEmpty(Message) && Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage(Message, 'r');
			}
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You no longer feel ill.", 'g');
		}
		base.Remove(Object);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Healing");
		Object.RegisterEffectEvent(this, "Recuperating");
		Object.RegisterEffectEvent(this, "Regenerating");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Healing");
		Object.UnregisterEffectEvent(this, "Recuperating");
		Object.UnregisterEffectEvent(this, "Regenerating");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Healing")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") / 2);
		}
		else
		{
			if (E.ID == "Regenerating")
			{
				E.SetParameter("Amount", 0);
				return false;
			}
			if (E.ID == "Recuperating")
			{
				base.Duration = 0;
				DidX("are", "no longer ill", null, null, base.Object);
			}
		}
		return base.FireEvent(E);
	}
}
