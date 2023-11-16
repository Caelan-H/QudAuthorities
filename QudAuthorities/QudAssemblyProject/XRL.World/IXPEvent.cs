using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class IXPEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Kill;

	public GameObject InfluencedBy;

	public GameObject PassedUpFrom;

	public GameObject PassedDownFrom;

	public int Amount;

	public int AmountBefore;

	public int Tier;

	public int Minimum;

	public int Maximum;

	public string Deed;

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
		Actor = null;
		Kill = null;
		InfluencedBy = null;
		PassedUpFrom = null;
		PassedDownFrom = null;
		Amount = 0;
		AmountBefore = 0;
		Tier = 0;
		Minimum = 0;
		Maximum = 0;
		Deed = null;
		base.Reset();
	}

	public void ApplyTo(IXPEvent E)
	{
		E.Actor = Actor;
		E.Kill = Kill;
		E.InfluencedBy = InfluencedBy;
		E.PassedUpFrom = PassedUpFrom;
		E.PassedDownFrom = PassedDownFrom;
		E.Amount = Amount;
		E.AmountBefore = AmountBefore;
		E.Tier = Tier;
		E.Minimum = Minimum;
		E.Maximum = Maximum;
		E.Deed = Deed;
	}
}
