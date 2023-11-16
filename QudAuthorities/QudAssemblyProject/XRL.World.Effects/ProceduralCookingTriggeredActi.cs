using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingTriggeredAction
{
	public virtual void Init(GameObject target)
	{
	}

	public virtual string GetDescription()
	{
		return "[action takes place]";
	}

	public virtual string GetTemplatedDescription()
	{
		return GetDescription();
	}

	public virtual void Apply(GameObject go)
	{
	}

	public virtual void Remove(GameObject go)
	{
	}

	public virtual string GetNotification()
	{
		return GetDescription();
	}
}
