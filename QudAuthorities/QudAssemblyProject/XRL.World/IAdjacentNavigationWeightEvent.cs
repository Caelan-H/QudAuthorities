using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IAdjacentNavigationWeightEvent : INavigationWeightEvent
{
	public Cell AdjacentCell;

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

	public override void Reset()
	{
		AdjacentCell = null;
		base.Reset();
	}

	public override void ApplyTo(INavigationWeightEvent E)
	{
		base.ApplyTo(E);
		if (E is IAdjacentNavigationWeightEvent adjacentNavigationWeightEvent)
		{
			adjacentNavigationWeightEvent.AdjacentCell = AdjacentCell;
		}
	}
}
