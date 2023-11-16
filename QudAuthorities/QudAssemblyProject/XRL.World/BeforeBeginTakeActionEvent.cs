using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeBeginTakeActionEvent : MinEvent
{
	public bool PreventAction;

	public static BeforeBeginTakeActionEvent instance;

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

	static BeforeBeginTakeActionEvent()
	{
		registeredInstance = new ImmutableEvent("BeforeBeginTakeAction");
		ID = MinEvent.AllocateID();
	}

	public BeforeBeginTakeActionEvent()
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
				instance = new BeforeBeginTakeActionEvent();
			}
			instance.PreventAction = false;
			flag = Object.HandleEvent(instance) && !instance.PreventAction;
		}
		return flag;
	}
}
