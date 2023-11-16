using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class NeedsReloadEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Weapon;

	public IComponent<GameObject> Skip;

	public new static readonly int ID;

	public static NeedsReloadEvent instance;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static NeedsReloadEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public NeedsReloadEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		Weapon = null;
		Skip = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static bool Check(GameObject Actor, IComponent<GameObject> Skip = null)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new NeedsReloadEvent();
			}
			instance.Reset();
			instance.Actor = Actor;
			instance.Skip = Skip;
			if (!Actor.HandleEvent(instance))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(GameObject Actor, GameObject Weapon, IComponent<GameObject> Skip = null)
	{
		if (Weapon.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new NeedsReloadEvent();
			}
			instance.Reset();
			instance.Actor = Actor;
			instance.Weapon = Weapon;
			instance.Skip = Skip;
			if (!Weapon.HandleEvent(instance))
			{
				return true;
			}
		}
		return false;
	}
}
