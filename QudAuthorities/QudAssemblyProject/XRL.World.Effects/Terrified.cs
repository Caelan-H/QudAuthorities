using System;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Effects;

[Serializable]
public class Terrified : Effect
{
	public string TerrifiedOfID;

	public GlobalLocation TerrifiedOfLocation;

	public bool Panicked;

	public bool Silent;

	public bool Psionic;

	[NonSerialized]
	private IMovementGoal Goal;

	public Terrified()
	{
		base.DisplayName = "{{W|terrified}}";
	}

	public Terrified(int Duration)
		: this()
	{
		base.Duration = Duration + 1;
	}

	public Terrified(int Duration, GameObject TerrifiedOf, bool Panicked = false, bool Psionic = false, bool Silent = false)
		: this(Duration)
	{
		TerrifiedOfID = TerrifiedOf.id;
		this.Panicked = Panicked;
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public Terrified(int Duration, GlobalLocation TerrifiedOfLocation, bool Panicked = false, bool Psionic = false, bool Silent = false)
		: this(Duration)
	{
		this.TerrifiedOfLocation = TerrifiedOfLocation;
		this.Panicked = Panicked;
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public Terrified(int Duration, Cell TerrifiedOfCell)
		: this(Duration, new GlobalLocation(TerrifiedOfCell))
	{
	}

	public Terrified(int Duration, Cell TerrifiedOfCell, bool Panicked = false, bool Psionic = false, bool Silent = false)
		: this(Duration, TerrifiedOfCell)
	{
		this.Panicked = Panicked;
		this.Psionic = Psionic;
		this.Silent = Silent;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		int num = 117440514;
		if (Psionic)
		{
			num |= 0x8000;
		}
		return num;
	}

	public override string GetDescription()
	{
		return "{{W|terrified}}";
	}

	public override string GetDetails()
	{
		if (!Panicked)
		{
			return "Fleeing.";
		}
		return "Fleeing in panic.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent("CanApplyFear") || !Object.FireEvent("ApplyFear"))
		{
			return false;
		}
		Object.RemoveEffect("Terrified");
		if (!CheckGoal())
		{
			return false;
		}
		Object.ParticleText("&W!");
		if (!Silent)
		{
			if (Panicked)
			{
				DidX("are", "overwhelmed by terror", "!", null, null, Object);
			}
			else
			{
				DidX("become", "afraid", "!", null, null, Object);
			}
		}
		Object.FireEvent("FearApplied");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Goal != null)
		{
			Goal.FailToParent();
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			CheckGoal();
		}
		return base.HandleEvent(E);
	}

	private bool CheckGoal()
	{
		if (Goal != null)
		{
			return true;
		}
		return SetUpGoal();
	}

	private bool SetUpGoal()
	{
		if (!SetUpObjectGoal())
		{
			return SetUpLocationGoal();
		}
		return true;
	}

	private bool SetUpObjectGoal()
	{
		if (string.IsNullOrEmpty(TerrifiedOfID))
		{
			return false;
		}
		GameObject gameObject = GameObject.findById(TerrifiedOfID);
		if (gameObject == null)
		{
			return false;
		}
		if (base.Object.pBrain == null)
		{
			return false;
		}
		Goal = new Flee(gameObject);
		base.Object.pBrain.PushGoal(Goal);
		return true;
	}

	private bool SetUpLocationGoal()
	{
		if (TerrifiedOfLocation == null)
		{
			return false;
		}
		if (base.Object.pBrain == null)
		{
			return false;
		}
		Cell cell = TerrifiedOfLocation.ResolveCell();
		if (cell == null)
		{
			return false;
		}
		Goal = new FleeLocation(cell);
		base.Object.pBrain.PushGoal(Goal);
		return true;
	}

	public static bool OfAttacker(MentalAttackEvent E)
	{
		return Attack(E, E.Attacker);
	}

	public static bool OfCell(MentalAttackEvent E)
	{
		return Attack(E, null, E.Attacker.CurrentCell);
	}

	public static bool Attack(MentalAttackEvent E, GameObject Object = null, Cell Cell = null, bool Panicked = false, bool Psionic = false, bool Silent = false)
	{
		if (E.Penetrations > 0)
		{
			if (Object != null)
			{
				Terrified e = new Terrified(E.Magnitude, Object, Panicked, Psionic, Silent);
				if (E.Defender.ApplyEffect(e))
				{
					return true;
				}
			}
			else if (Cell != null)
			{
				Terrified e2 = new Terrified(E.Magnitude, Cell, Panicked, Psionic, Silent);
				if (E.Defender.ApplyEffect(e2))
				{
					return true;
				}
			}
		}
		IComponent<GameObject>.XDidY(E.Defender, "resist", "becoming afraid", null, null, E.Defender);
		return false;
	}
}
