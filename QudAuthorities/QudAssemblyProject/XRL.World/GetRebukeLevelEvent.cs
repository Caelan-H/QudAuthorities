using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetRebukeLevelEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Target;

	public int Level;

	public new static readonly int ID;

	public static GetRebukeLevelEvent instance;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetRebukeLevelEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetRebukeLevelEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Target = null;
		Level = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int GetFor(GameObject Actor, GameObject Target)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new GetRebukeLevelEvent();
			}
			instance.Actor = Actor;
			instance.Target = Target;
			instance.Level = Actor.Stat("Level");
			Actor.HandleEvent(instance);
			return instance.Level;
		}
		return Actor.Stat("Level");
	}
}
