using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class BeginTakeActionEvent : MinEvent
{
	public GameObject Object;

	public bool Traveling;

	public bool TravelMessagesSuppressed;

	public bool PreventAction;

	public static BeginTakeActionEvent instance;

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

	static BeginTakeActionEvent()
	{
		registeredInstance = new ImmutableEvent("BeginTakeAction");
		ID = MinEvent.AllocateID();
	}

	public BeginTakeActionEvent()
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
				instance = new BeginTakeActionEvent();
			}
			instance.Object = Object;
			instance.Traveling = false;
			instance.TravelMessagesSuppressed = false;
			instance.PreventAction = false;
			flag = Object.HandleEvent(instance) && !instance.PreventAction;
		}
		return flag;
	}

	public static bool Check(GameObject Object, bool Traveling, ref bool TravelMessagesSuppressed)
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
				instance = new BeginTakeActionEvent();
			}
			instance.Object = Object;
			instance.Traveling = Traveling;
			instance.TravelMessagesSuppressed = TravelMessagesSuppressed;
			instance.PreventAction = false;
			flag = Object.HandleEvent(instance) && !instance.PreventAction;
			TravelMessagesSuppressed = instance.TravelMessagesSuppressed;
		}
		return flag;
	}
}
