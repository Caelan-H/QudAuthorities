using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class CommandTakeActionEvent : MinEvent
{
	public bool PreventAction;

	public static CommandTakeActionEvent instance;

	public static ImmutableEvent registeredInstance;

	public new static readonly int ID;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static CommandTakeActionEvent()
	{
		registeredInstance = new ImmutableEvent("CommandTakeAction");
		ID = MinEvent.AllocateID();
	}

	public CommandTakeActionEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Object)
	{
		bool result = true;
		if (Object.HasRegisteredEvent(registeredInstance.ID))
		{
			result = Object.FireEvent(registeredInstance);
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new CommandTakeActionEvent();
			}
			instance.PreventAction = false;
			result = Object.HandleEvent(instance) && !instance.PreventAction;
		}
		return result;
	}
}
