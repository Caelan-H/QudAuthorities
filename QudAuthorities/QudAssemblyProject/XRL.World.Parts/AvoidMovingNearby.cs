using System;

namespace XRL.World.Parts;

[Serializable]
public class AvoidMovingNearby : IPart
{
	public int Weight = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as AvoidMovingNearby).Weight != Weight)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		E.MinWeight(Weight);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(Weight);
		return base.HandleEvent(E);
	}
}
