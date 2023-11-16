using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IDeathEvent : MinEvent
{
	public GameObject Dying;

	public GameObject Killer;

	public GameObject Weapon;

	public GameObject Projectile;

	public string KillerText;

	public string Reason;

	public string ThirdPersonReason;

	public bool Accidental;

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
		Dying = null;
		Killer = null;
		Weapon = null;
		Projectile = null;
		KillerText = null;
		Reason = null;
		ThirdPersonReason = null;
		Accidental = false;
		base.Reset();
	}
}
