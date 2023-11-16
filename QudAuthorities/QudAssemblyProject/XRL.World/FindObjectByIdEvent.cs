using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class FindObjectByIdEvent : MinEvent
{
	public string FindID;

	public GameObject Object;

	public new static readonly int ID;

	public static FindObjectByIdEvent instance;

	public new static int CascadeLevel => 15;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static FindObjectByIdEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public FindObjectByIdEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		FindID = null;
		Object = null;
		base.Reset();
	}

	public static GameObject Find(GameObject From, string ID)
	{
		if (instance == null)
		{
			instance = new FindObjectByIdEvent();
		}
		instance.FindID = ID;
		instance.Object = null;
		From.HandleEvent(instance);
		GameObject @object = instance.Object;
		instance.Reset();
		return @object;
	}
}
