using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class EarlyBeforeBeginTakeActionEvent : MinEvent
{
	public bool PreventAction;

	public static EarlyBeforeBeginTakeActionEvent instance;

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

	static EarlyBeforeBeginTakeActionEvent()
	{
		registeredInstance = new ImmutableEvent("EarlyBeforeBeginTakeAction");
		ID = MinEvent.AllocateID();
	}

	public EarlyBeforeBeginTakeActionEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Object)
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
				instance = new EarlyBeforeBeginTakeActionEvent();
			}
			instance.PreventAction = false;
			flag = Object.HandleEvent(instance) && !instance.PreventAction;
		}
		return flag;
	}
}
