using Occult.Engine.CodeGeneration;

namespace XRL.World;

[GenerateMinEventDispatchPartials]
public class IMentalAttackEvent : MinEvent
{
	public GameObject Attacker;

	public GameObject Defender;

	public GameObject Source;

	public string Command;

	public string Dice;

	public int Type;

	public int Magnitude;

	public int Penetrations;

	public int Difficulty;

	public int BaseDifficulty;

	public int Modifier;

	public bool Reflected
	{
		get
		{
			return Type.HasBit(16777216);
		}
		set
		{
			Type.SetBit(16777216, value);
		}
	}

	public bool Reflectable
	{
		get
		{
			return Type.HasBit(8388608);
		}
		set
		{
			Type.SetBit(8388608, value);
		}
	}

	public bool Psionic
	{
		get
		{
			return Type.HasBit(1);
		}
		set
		{
			Type.SetBit(1, value);
		}
	}

	public new static int CascadeLevel => 1;

	public override bool handlePartDispatch(IPart Part)
	{
		return Part.HandleEvent(this);
	}

	public override bool handleEffectDispatch(Effect Effect)
	{
		return Effect.HandleEvent(this);
	}

	public bool IsPlayerInvolved()
	{
		if (!Attacker.IsPlayer())
		{
			return Defender.IsPlayer();
		}
		return true;
	}

	public override void Reset()
	{
		Attacker = null;
		Defender = null;
		Source = null;
		Command = null;
		Dice = null;
		Type = 0;
		Magnitude = int.MinValue;
		Penetrations = int.MinValue;
		BaseDifficulty = 0;
		base.Reset();
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public void SetFrom(IMentalAttackEvent E)
	{
		Attacker = E.Attacker;
		Defender = E.Defender;
		Source = E.Source;
		Command = E.Command;
		Dice = E.Dice;
		Type = E.Type;
		Magnitude = E.Magnitude;
		Penetrations = E.Penetrations;
		Difficulty = E.Difficulty;
		BaseDifficulty = E.BaseDifficulty;
		Modifier = E.Modifier;
	}

	public void ApplyTo(IMentalAttackEvent E)
	{
		E.SetFrom(this);
	}

	public void SetFrom(Event E)
	{
		Attacker = E.GetGameObjectParameter("Attacker");
		Defender = E.GetGameObjectParameter("Defender");
		Source = E.GetGameObjectParameter("Source");
		Command = E.GetStringParameter("Command");
		Dice = E.GetStringParameter("Dice");
		Type = E.GetIntParameter("Type");
		Magnitude = E.GetIntParameter("Magnitude");
		Penetrations = E.GetIntParameter("Penetrations");
		BaseDifficulty = E.GetIntParameter("BaseDifficulty");
		Difficulty = E.GetIntParameter("Difficulty");
		Modifier = E.GetIntParameter("Modifier");
	}

	public void ApplyTo(Event E)
	{
		E.SetParameter("Attacker", Attacker);
		E.SetParameter("Defender", Defender);
		E.SetParameter("Source", Source);
		E.SetParameter("Command", Command);
		E.SetParameter("Dice", Dice);
		E.SetParameter("Type", Type);
		E.SetParameter("Magnitude", Magnitude);
		E.SetParameter("Penetrations", Penetrations);
		E.SetParameter("BaseDifficulty", BaseDifficulty);
		E.SetParameter("Difficulty", Difficulty);
		E.SetParameter("Modifier", Modifier);
	}
}
