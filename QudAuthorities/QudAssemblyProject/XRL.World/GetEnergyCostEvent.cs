using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class GetEnergyCostEvent : MinEvent
{
	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public string Type;

	public int PercentageReduction;

	public int LinearReduction;

	public new static readonly int ID;

	public static GetEnergyCostEvent instance;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	static GetEnergyCostEvent()
	{
		ID = MinEvent.AllocateID();
	}

	public GetEnergyCostEvent()
	{
		base.ID = ID;
	}

	public override void Reset()
	{
		Actor = null;
		BaseAmount = 0;
		Amount = 0;
		Type = null;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public bool TypeMatches(string lookFor)
	{
		if (string.IsNullOrEmpty(Type))
		{
			return false;
		}
		return Type.Contains(lookFor);
	}

	public static int GetFor(GameObject Actor, int BaseAmount, string Type, int PercentageReduction = 0, int LinearReduction = 0, int MinAmount = 0)
	{
		int num = BaseAmount;
		if (Actor.HasRegisteredEvent("UsingEnergy"))
		{
			Event @event = Event.New("UsingEnergy", "Amount", num, "Type", Type);
			Actor.FireEvent(@event);
			int intParameter = @event.GetIntParameter("Amount");
			if (intParameter != num)
			{
				LinearReduction += num - intParameter;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			if (instance == null)
			{
				instance = new GetEnergyCostEvent();
			}
			instance.Actor = Actor;
			instance.BaseAmount = BaseAmount;
			instance.Amount = num;
			instance.Type = Type;
			instance.PercentageReduction = PercentageReduction;
			instance.LinearReduction = LinearReduction;
			Actor.HandleEvent(instance);
			num = instance.Amount;
			PercentageReduction = instance.PercentageReduction;
			LinearReduction = instance.LinearReduction;
		}
		if (PercentageReduction != 0)
		{
			num = num * (100 - PercentageReduction) / 100;
		}
		if (LinearReduction != 0)
		{
			num -= LinearReduction;
		}
		if (num < MinAmount)
		{
			num = MinAmount;
		}
		return num;
	}
}
