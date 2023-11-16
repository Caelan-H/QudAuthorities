using System;

namespace XRL.World.Parts;

[Serializable]
public class NoDamageWhileFrozen : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == CanBeDismemberedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (!ParentObject.IsFrozen())
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
