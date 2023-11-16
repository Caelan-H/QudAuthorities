namespace XRL.World;

public class ModMinEvent<T> : MinEvent where T : ModMinEvent<T>
{
	public override bool handlePartDispatch(IPart part)
	{
		return part.HandleEvent((T)this);
	}

	public override bool handleEffectDispatch(Effect effect)
	{
		return effect.HandleEvent((T)this);
	}
}
