using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public abstract class INavigationWeightEvent : MinEvent
{
	public Cell Cell;

	public GameObject Actor;

	public bool Uncacheable;

	public int Weight;

	public int PriorWeight;

	public bool Smart;

	public bool Burrower;

	public bool Autoexploring;

	public bool Flying;

	public bool WallWalker;

	public bool IgnoresWalls;

	public bool Swimming;

	public bool Slimewalking;

	public bool Aquatic;

	public bool Polypwalking;

	public bool Strutwalking;

	public bool Juggernaut;

	public bool Reefer;

	public GameObject Object;

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
		Cell = null;
		Actor = null;
		Uncacheable = false;
		Weight = 0;
		PriorWeight = 0;
		Smart = false;
		Burrower = false;
		Autoexploring = false;
		Flying = false;
		WallWalker = false;
		IgnoresWalls = false;
		Swimming = false;
		Slimewalking = false;
		Aquatic = false;
		Polypwalking = false;
		Strutwalking = false;
		Juggernaut = false;
		Reefer = false;
		Object = null;
		base.Reset();
	}

	public void MinWeight(int Value)
	{
		if (Weight < Value)
		{
			Weight = Value;
		}
	}

	public void MinWeight(int Value, int Max)
	{
		if (Value > Max)
		{
			Value = Max;
		}
		if (Weight < Value)
		{
			Weight = Value;
		}
	}

	public virtual void ApplyTo(INavigationWeightEvent E)
	{
		E.Cell = Cell;
		E.Actor = Actor;
		E.Uncacheable = Uncacheable;
		E.Weight = Weight;
		E.PriorWeight = PriorWeight;
		E.Smart = Smart;
		E.Burrower = Burrower;
		E.Autoexploring = Autoexploring;
		E.Flying = Flying;
		E.WallWalker = WallWalker;
		E.IgnoresWalls = IgnoresWalls;
		E.Swimming = Swimming;
		E.Slimewalking = Slimewalking;
		E.Aquatic = Aquatic;
		E.Polypwalking = Polypwalking;
		E.Strutwalking = Strutwalking;
		E.Object = Object;
	}
}
