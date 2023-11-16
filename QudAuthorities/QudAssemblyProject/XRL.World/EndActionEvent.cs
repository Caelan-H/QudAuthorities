using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EndActionEvent : MinEvent
{
	public new static readonly int ID;

	public static EndActionEvent instance;

	public static ImmutableEvent registeredInstance;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static EndActionEvent()
	{
		registeredInstance = new ImmutableEvent("EndAction");
		ID = MinEvent.AllocateID();
	}

	public EndActionEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static void Send(GameObject Object)
	{
		bool flag = true;
		if (flag && Object.HasRegisteredEvent(registeredInstance.ID))
		{
			flag = Object.FireEvent(registeredInstance);
		}
		if (flag && Object.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new EndActionEvent();
			}
			flag = Object.HandleEvent(instance);
		}
	}
}
