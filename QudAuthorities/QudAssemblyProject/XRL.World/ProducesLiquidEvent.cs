using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ProducesLiquidEvent : ILiquidEvent
{
	public GameObject Object;

	private static ProducesLiquidEvent instance;

	public new static readonly int ID;

	public new static int CascadeLevel => 0;

	public override bool handlePartDispatch(IPart Part)
	{
		if (!base.handlePartDispatch(Part))
		{
			return false;
		}
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		if (!base.handleEffectDispatch(Effect))
		{
			return false;
		}
		return Effect.HandleEvent(this);
	}

	static ProducesLiquidEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public ProducesLiquidEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public new void Reset()
	{
		Object = null;
		base.Reset();
	}

	public static bool Check(GameObject Object, string Liquid)
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new ProducesLiquidEvent();
				instance.Object = Object;
				instance.Liquid = Liquid;
			}
			if (!Object.HandleEvent(instance))
			{
				return true;
			}
		}
		return false;
	}
}
