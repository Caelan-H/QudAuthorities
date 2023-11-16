using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RefreshTileEvent : MinEvent
{
	public new static readonly int ID;

	public static RefreshTileEvent instance;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RefreshTileEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public RefreshTileEvent()
	{
		base.ID = ID;
	}

	public static void Send(GameObject obj)
	{
		if (obj.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (instance == null)
			{
				instance = new RefreshTileEvent();
			}
			obj.HandleEvent(instance);
		}
	}
}
