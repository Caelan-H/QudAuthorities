using System;

namespace XRL.World.Effects;

[Serializable]
public class BasicTriggeredCookingEffect : Effect
{
	public string wellFedMessage = "You eat the meal. It's tastier than usual.";

	public bool removed;

	public BasicTriggeredCookingEffect()
	{
		base.DisplayName = "{{W|well fed}}";
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{w|metabolized effect}}";
	}

	public virtual void ApplyEffect(GameObject Object)
	{
	}

	public virtual void RemoveEffect(GameObject Object)
	{
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent(Event.New("ApplyBasicTriggeredCookingEffect")))
		{
			ApplyEffect(Object);
			return true;
		}
		return false;
	}

	public override void Remove(GameObject Object)
	{
		if (base.Duration > 0)
		{
			base.Duration = 0;
		}
		if (!removed)
		{
			RemoveEffect(Object);
			removed = true;
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			base.Duration--;
		}
		return true;
	}
}
