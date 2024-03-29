using System;

namespace XRL.World.Parts;

[Serializable]
public class FallsApart : IPart
{
	public string Duration = "2d4";

	public string Message = "=subject.T= =verb:fall= apart.";

	public int TurnsLeft = -1;

	public bool bActive = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		TurnsLeft = Duration.RollCached();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			bActive = false;
		}
		return base.FireEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (bActive && --TurnsLeft <= 0)
		{
			ParentObject.Die(null, "impermanence", "You fell apart.", ParentObject.It + " @@fell apart.", Accidental: false, null, null, Force: false, Message);
			if (GameObject.validate(ParentObject) && !ParentObject.IsInGraveyard())
			{
				TurnsLeft = Duration.RollCached();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
