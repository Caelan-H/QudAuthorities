using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EndTurnEvent : MinEvent
{
	public new static readonly int ID;

	public static EndTurnEvent instance;

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

	static EndTurnEvent()
	{
		registeredInstance = new ImmutableEvent("EndTurn");
		ID = MinEvent.AllocateID();
	}

	public EndTurnEvent()
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
				instance = new EndTurnEvent();
			}
			flag = Object.HandleEvent(instance);
		}
	}
}
