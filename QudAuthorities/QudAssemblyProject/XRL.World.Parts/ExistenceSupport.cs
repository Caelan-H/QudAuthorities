using System;

namespace XRL.World.Parts;

[Serializable]
public class ExistenceSupport : IPart
{
	public string SupportedById;

	public bool ValidateEveryTurn;

	public bool SilentRemoval;

	public GameObject SupportedBy
	{
		get
		{
			return GameObject.findById(SupportedById);
		}
		set
		{
			SupportedById = value?.id;
		}
	}

	public ExistenceSupport()
	{
	}

	public ExistenceSupport(ExistenceSupport src)
	{
		SupportedById = src.SupportedById;
		ValidateEveryTurn = src.ValidateEveryTurn;
		SilentRemoval = src.SilentRemoval;
	}

	public override bool SameAs(IPart p)
	{
		ExistenceSupport existenceSupport = p as ExistenceSupport;
		if (existenceSupport.SupportedById != SupportedById)
		{
			return false;
		}
		if (existenceSupport.ValidateEveryTurn != ValidateEveryTurn)
		{
			return false;
		}
		if (existenceSupport.SilentRemoval != SilentRemoval)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != CanBeTradedEvent.ID && ID != DerivationCreatedEvent.ID && ID != SynchronizeExistenceEvent.ID && ID != WasDerivedFromEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.RemovePart("ExistenceSupport");
		E.Derivation.AddPart(new ExistenceSupport(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DerivationCreatedEvent E)
	{
		if (!E.Original.HasPart("ExistenceSupport"))
		{
			E.Object.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(SynchronizeExistenceEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return ValidateEveryTurn;
	}

	public override bool WantTenTurnTick()
	{
		return ValidateEveryTurn;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		CheckSupport();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckSupport();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckSupport();
	}

	public void CheckSupport()
	{
		if (!IsSupported())
		{
			Unsupported();
		}
	}

	public bool IsSupported()
	{
		return CheckExistenceSupportEvent.Check(SupportedBy, ParentObject);
	}

	public void Unsupported(bool Silent = false)
	{
		if (SilentRemoval)
		{
			Silent = true;
		}
		if (ParentObject.GetPart("Temporary") is Temporary temporary)
		{
			temporary.Expire(Silent);
			return;
		}
		ParentObject.RemoveContents(Silent);
		if (!Silent)
		{
			DidX("disappear", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		ParentObject.Obliterate(null, Silent);
	}
}
