using Occult.Engine.CodeGeneration;
using XRL.Core;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class AfterGameLoadedEvent : MinEvent
{
	public new static readonly int ID;

	public static AfterGameLoadedEvent instance;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static AfterGameLoadedEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public AfterGameLoadedEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static void Send(Zone Z)
	{
		if (Z != null)
		{
			if (instance == null)
			{
				instance = new AfterGameLoadedEvent();
			}
			Z.HandleEvent(instance);
			instance.Reset();
		}
	}

	public static void Send()
	{
		if (instance == null)
		{
			instance = new AfterGameLoadedEvent();
		}
		XRLCore.HandleEvent(instance);
		instance.Reset();
	}
}
