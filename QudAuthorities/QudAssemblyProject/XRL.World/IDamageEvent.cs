using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IDamageEvent : MinEvent
{
	public Damage Damage;

	public GameObject Object;

	public GameObject Actor;

	public GameObject Source;

	public GameObject Weapon;

	public GameObject Projectile;

	public bool Indirect;

	public bool WillUseOutcomeMessageFragment;

	public string OutcomeMessageFragment;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override void Reset()
	{
		Damage = null;
		Object = null;
		Actor = null;
		Source = null;
		Weapon = null;
		Projectile = null;
		Indirect = false;
		WillUseOutcomeMessageFragment = false;
		OutcomeMessageFragment = null;
		base.Reset();
	}
}
