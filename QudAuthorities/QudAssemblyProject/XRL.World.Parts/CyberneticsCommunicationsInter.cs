using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCommunicationsInterlock : IPart
{
	public int Bonus = 5;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetRebukeLevelEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetRebukeLevelEvent E)
	{
		E.Level += GetAvailableComputePowerEvent.AdjustUp(E.Actor, Bonus);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
