using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetTonicCapacityEvent : MinEvent
{
	public GameObject Actor;

	public int BaseCapacity;

	public int Capacity;

	public new static readonly int ID;

	public static GetTonicCapacityEvent instance;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetTonicCapacityEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetTonicCapacityEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		BaseCapacity = 0;
		Capacity = 0;
		base.Reset();
	}

	public static int GetFor(GameObject Actor, int BaseCapacity = 1)
	{
		if (Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (instance == null)
			{
				instance = new GetTonicCapacityEvent();
			}
			instance.Actor = Actor;
			instance.BaseCapacity = BaseCapacity;
			instance.Capacity = BaseCapacity;
			Actor.HandleEvent(instance);
			return instance.Capacity;
		}
		return BaseCapacity;
	}
}
