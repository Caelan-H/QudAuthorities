using System;

namespace XRL.World.Parts;

[Serializable]
public class Temporary : IPart
{
	public int Duration = 12;

	public string TurnInto;

	[FieldSaveVersion(236)]
	public long LastTurn = long.MaxValue;

	public Temporary()
	{
	}

	public Temporary(int Duration)
		: this()
	{
		this.Duration = Duration;
	}

	public Temporary(int Duration, string TurnInto)
		: this(Duration)
	{
		this.TurnInto = TurnInto;
	}

	public Temporary(Temporary src)
		: this(src.Duration, src.TurnInto)
	{
	}

	public override bool SameAs(IPart p)
	{
		Temporary temporary = p as Temporary;
		if (temporary.Duration != Duration)
		{
			return false;
		}
		if (temporary.TurnInto != TurnInto)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != AfterAfterThrownEvent.ID && ID != CanBeTradedEvent.ID && ID != CheckExistenceSupportEvent.ID && ID != DerivationCreatedEvent.ID && ID != GetDebugInternalsEvent.ID && ID != WasDerivedFromEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneThawedEvent.ID)
		{
			if (ID == RealityStabilizeEvent.ID)
			{
				return TurnInto == "*fugue";
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.RemovePart("Temporary");
		E.Derivation.AddPart(new Temporary(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DerivationCreatedEvent E)
	{
		if (!E.Original.HasPart("Temporary"))
		{
			E.Object.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterAfterThrownEvent E)
	{
		if (TurnInto == "*fugue" && ParentObject.IsValid())
		{
			Expire();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (E.Object.GetPart("Temporary") is Temporary temporary && temporary.TurnInto == TurnInto)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (TurnInto == "*fugue" && E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K~");
			if (Visible())
			{
				if (ParentObject.it == "it")
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("worldline through spacetime") + " snaps back to its canonical path, and " + ParentObject.t() + ParentObject.GetVerb("vanish") + ".");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("worldline through spacetime") + " snaps back to its canonical path, and " + ParentObject.it + ParentObject.GetVerb("vanish", PrependSpace: true, PronounAntecedent: true) + ".");
				}
			}
			Expire(Silent: true);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		int num = (int)Math.Max(The.Game.Turns - LastTurn, 0L);
		if (Duration > 0 && (Duration -= num) <= 0)
		{
			Expire();
		}
		LastTurn = The.Game.Turns;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		int num = (int)Math.Max(The.Game.Turns - LastTurn, 0L);
		if (Duration > 0 && (Duration -= num) <= 0)
		{
			Expire();
		}
		LastTurn = The.Game.Turns;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Duration", Duration);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (Duration > 0 && --Duration <= 0)
		{
			Expire();
		}
		LastTurn = TurnNumber;
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (Duration > 0 && (Duration -= 10) <= 0)
		{
			Expire();
		}
		LastTurn = TurnNumber;
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (Duration > 0 && (Duration -= 100) <= 0)
		{
			Expire();
		}
		LastTurn = TurnNumber;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.ModIntProperty("WontSell", 1, RemoveIfZero: true);
	}

	public override void Remove()
	{
		ParentObject.ModIntProperty("WontSell", -1, RemoveIfZero: true);
		base.Remove();
	}

	public bool TurnsIntoSomething()
	{
		if (!string.IsNullOrEmpty(TurnInto))
		{
			return TurnInto != "*fugue";
		}
		return false;
	}

	public void Expire(bool Silent = false)
	{
		Cell cell = null;
		GameObject obj = null;
		if (TurnsIntoSomething())
		{
			cell = ParentObject.GetCurrentCell();
			obj = ParentObject.InInventory ?? ParentObject.Equipped ?? ParentObject.Implantee;
		}
		foreach (GameObject content in ParentObject.GetContents())
		{
			if (content.GetPart("Temporary") is Temporary temporary)
			{
				temporary.Expire(Silent: true);
			}
		}
		ParentObject.RemoveContents(Silent);
		if (!Silent && !TurnsIntoSomething())
		{
			DidX("disappear", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		ParentObject.Obliterate("Faded from existence.", Silent: true);
		if (TurnsIntoSomething())
		{
			if (cell != null)
			{
				cell.AddObject(TurnInto);
			}
			else if (GameObject.validate(ref obj))
			{
				obj.Inventory.AddObject(TurnInto);
			}
		}
	}

	public static void AddHierarchically(GameObject obj, int Duration = -1, string TurnInto = null, GameObject DependsOn = null)
	{
		if (GameObject.validate(ref obj))
		{
			MakeTemporaryEvent.Send(obj, Duration, TurnInto, DependsOn);
		}
	}

	public static void CarryOver(GameObject src, GameObject dest, bool CanRemove = false)
	{
		if (src.GetPart("Temporary") is Temporary src2)
		{
			dest.RemovePart("Temporary");
			dest.AddPart(new Temporary(src2));
		}
		else if (CanRemove)
		{
			dest.RemovePart("Temporary");
		}
		if (src.GetPart("ExistenceSupport") is ExistenceSupport src3)
		{
			dest.RemovePart("ExistenceSupport");
			dest.AddPart(new ExistenceSupport(src3));
		}
		else if (CanRemove)
		{
			dest.RemovePart("ExistenceSupport");
		}
	}

	public static bool IsTemporary(GameObject obj)
	{
		if (!obj.HasPart("Temporary"))
		{
			return obj.HasPart("ExistenceSupport");
		}
		return true;
	}

	public static bool IsNotTemporary(GameObject obj)
	{
		return !IsTemporary(obj);
	}
}
