using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class ISaveEvent : MinEvent
{
	public delegate ISaveEvent ISaveEventGenerator(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual);

	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Source;

	public string Stat;

	public string AttackerStat;

	public string Vs;

	public int NaturalRoll;

	public int Roll;

	public int BaseDifficulty;

	public int Difficulty;

	public bool IgnoreNatural1;

	public bool IgnoreNatural20;

	public bool Actual;

	public new static int CascadeLevel => 17;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override void Reset()
	{
		Attacker = null;
		Defender = null;
		Source = null;
		Stat = null;
		AttackerStat = null;
		Vs = null;
		NaturalRoll = 0;
		Roll = 0;
		BaseDifficulty = 0;
		Difficulty = 0;
		Actual = false;
		base.Reset();
	}

	public static bool Process(GameObject Target, string RegisteredEventID, int ID, int CascadeLevel, ISaveEventGenerator Generator, GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		bool flag = true;
		if (flag && GameObject.validate(ref Target) && Target.HasRegisteredEvent(RegisteredEventID))
		{
			Event @event = Event.New(RegisteredEventID);
			@event.SetParameter("Attacker", Attacker);
			@event.SetParameter("Defender", Defender);
			@event.SetParameter("Source", Source);
			@event.SetParameter("Stat", Stat);
			@event.SetParameter("AttackerStat", AttackerStat ?? Stat);
			@event.SetParameter("Vs", Vs ?? "");
			@event.SetParameter("NaturalRoll", NaturalRoll);
			@event.SetParameter("Roll", Roll);
			@event.SetParameter("BaseDifficulty", BaseDifficulty);
			@event.SetParameter("Difficulty", Difficulty);
			@event.SetFlag("IgnoreNatural1", IgnoreNatural1);
			@event.SetFlag("IgnoreNatural20", IgnoreNatural20);
			@event.SetFlag("Actual", Actual);
			flag = Target.FireEvent(@event);
			Roll = @event.GetIntParameter("Roll");
			Difficulty = @event.GetIntParameter("Difficulty");
			IgnoreNatural1 = @event.HasFlag("IgnoreNatural1");
			IgnoreNatural20 = @event.HasFlag("IgnoreNatural20");
		}
		if (flag && GameObject.validate(ref Target) && Target.WantEvent(ID, CascadeLevel))
		{
			ISaveEvent saveEvent = Generator(Attacker, Defender, Source, Stat, AttackerStat ?? Stat, Vs ?? "", NaturalRoll, Roll, BaseDifficulty, Difficulty, IgnoreNatural1, IgnoreNatural20, Actual);
			flag = Target.HandleEvent(saveEvent);
			Roll = saveEvent.Roll;
			Difficulty = saveEvent.Difficulty;
			IgnoreNatural1 = saveEvent.IgnoreNatural1;
			IgnoreNatural20 = saveEvent.IgnoreNatural20;
		}
		return true;
	}
}
