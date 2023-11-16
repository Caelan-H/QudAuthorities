using System;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnit
{
	[NonSerialized]
	public ProceduralCookingEffect parent;

	public virtual string GetDescription()
	{
		return "[effect]";
	}

	public virtual string GetTemplatedDescription()
	{
		return GetDescription();
	}

	public virtual void Apply(GameObject go, Effect parent)
	{
	}

	public virtual void Remove(GameObject go, Effect parent)
	{
	}

	public virtual void FireEvent(Event E)
	{
	}

	public virtual void Init(GameObject target)
	{
	}
}
