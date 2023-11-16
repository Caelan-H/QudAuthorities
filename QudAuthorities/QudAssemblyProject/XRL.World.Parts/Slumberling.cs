using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Slumberling : IPart
{
	public bool Initial = true;

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
		CheckHibernate();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		CheckHibernate(10);
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		CheckHibernate(100);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return Initial;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Initial)
		{
			Initial = false;
			ParentObject.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, voluntary: true));
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanWakeUpOnHelpBroadcast");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanWakeUpOnHelpBroadcast")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool CheckHibernate(int Chances = 1)
	{
		if (ParentObject.HasEffect("Asleep"))
		{
			return true;
		}
		for (int i = 0; i < Chances; i++)
		{
			if (10.in100())
			{
				if (ParentObject.ApplyEffect(new Asleep(9999, forced: false, quicksleep: false, voluntary: true)))
				{
					DidX("lapse", "back into hibernation", null, null, null, ParentObject);
					return true;
				}
				return false;
			}
		}
		return false;
	}
}
