using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeforeTakeActionEvent : MinEvent
{
	public bool PreventAction;

	public static BeforeTakeActionEvent instance;

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

	static BeforeTakeActionEvent()
	{
		registeredInstance = new ImmutableEvent("BeforeTakeAction");
		ID = MinEvent.AllocateID();
	}

	public BeforeTakeActionEvent()
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
				instance = new BeforeTakeActionEvent();
			}
			instance.PreventAction = false;
			flag = Object.HandleEvent(instance) && !instance.PreventAction;
		}
		return flag;
	}
}
