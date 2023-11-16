using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class RadiatesHeatAdjacentEvent : MinEvent
{
	public new static readonly int ID;

	public static RadiatesHeatAdjacentEvent instance;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static RadiatesHeatAdjacentEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public RadiatesHeatAdjacentEvent()
	{
		base.ID = ID;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			if (instance == null)
			{
				instance = new RadiatesHeatAdjacentEvent();
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

	public static bool CheckAdjacent(Cell C)
	{
		if (C != null)
		{
			foreach (Cell localAdjacentCell in C.GetLocalAdjacentCells())
			{
				if (Check(localAdjacentCell))
				{
					return true;
				}
			}
		}
		return false;
	}
}
