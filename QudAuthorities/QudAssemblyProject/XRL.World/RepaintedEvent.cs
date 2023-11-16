using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RepaintedEvent : MinEvent
{
	public new static readonly int ID;

	public static RepaintedEvent instance;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RepaintedEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public RepaintedEvent()
	{
		base.ID = ID;
	}

	public static void Send(GameObject obj)
	{
		if (obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (instance == null)
			{
				instance = new RepaintedEvent();
			}
			obj.HandleEvent(instance);
		}
	}
}
