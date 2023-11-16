using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RadiatesHeatEvent : MinEvent
{
	public new static readonly int ID;

	public static RadiatesHeatEvent instance;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RadiatesHeatEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public RadiatesHeatEvent()
	{
		base.ID = ID;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (instance == null)
			{
				instance = new RadiatesHeatEvent();
			}
			if (!Object.HandleEvent(instance))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				if (Check(C.Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}
}
