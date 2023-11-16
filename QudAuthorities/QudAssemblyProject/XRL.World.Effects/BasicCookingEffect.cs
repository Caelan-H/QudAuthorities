using System;

namespace XRL.World.Effects;

[Serializable]
public class BasicCookingEffect : Effect
{
	public string wellFedMessage = "You eat the meal. It's tastier than usual.";

	public bool removed;

	public BasicCookingEffect()
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

	public virtual void ApplyEffect(GameObject Object)
	{
	}

	public virtual void RemoveEffect(GameObject Object)
	{
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.FireEvent("ApplyBasicCookingEffect"))
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
		Object.RegisterEffectEvent(this, "ApplyWellFed");
		Object.RegisterEffectEvent(this, "BecameHungry");
		Object.RegisterEffectEvent(this, "BecameFamished");
		Object.RegisterEffectEvent(this, "ClearFoodEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyWellFed");
		Object.UnregisterEffectEvent(this, "BecameHungry");
		Object.UnregisterEffectEvent(this, "BecameFamished");
		Object.UnregisterEffectEvent(this, "ClearFoodEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BecameHungry" || E.ID == "BecameFamished" || E.ID == "ApplyBasicCookingEffect" || E.ID == "ClearFoodEffects")
		{
			Remove(base.Object);
		}
		return true;
	}
}
