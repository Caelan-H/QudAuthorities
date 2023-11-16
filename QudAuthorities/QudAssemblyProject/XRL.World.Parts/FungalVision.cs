using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FungalVision : IPart
{
	public bool originalSolid;

	public bool originalOccluding;

	public override bool SameAs(IPart p)
	{
		FungalVision fungalVision = p as FungalVision;
		if (fungalVision.originalSolid != originalSolid)
		{
			return false;
		}
		if (fungalVision.originalOccluding != originalOccluding)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != BeginTakeActionEvent.ID && ID != CanHaveConversationEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0 && !ParentObject.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanHaveConversationEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0)
		{
			if (ParentObject.pRender.Occluding && ParentObject.pPhysics.CurrentCell != null)
			{
				ParentObject.CurrentCell.ClearOccludeCache();
			}
			ParentObject.pRender.Visible = false;
			ParentObject.pPhysics.Solid = false;
			ParentObject.pRender.Occluding = false;
		}
		else
		{
			if (ParentObject.pRender.Occluding != originalOccluding && ParentObject.pPhysics.CurrentCell != null)
			{
				ParentObject.CurrentCell.ClearOccludeCache();
			}
			ParentObject.pRender.Visible = true;
			ParentObject.pPhysics.Solid = originalSolid;
			ParentObject.pRender.Occluding = originalOccluding;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		originalSolid = ParentObject.pPhysics.Solid;
		originalOccluding = ParentObject.pRender.Occluding;
		if (FungalVisionary.VisionLevel <= 0)
		{
			ParentObject.pPhysics.Solid = false;
			ParentObject.pRender.Occluding = false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforePhysicsRejectObjectEntringCell");
		Object.RegisterPartEvent(this, "CanHypersensesDetect");
		Object.RegisterPartEvent(this, "PreventSmartUse");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PreventSmartUse" || E.ID == "CanHypersensesDetect")
		{
			if (FungalVisionary.VisionLevel <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeforePhysicsRejectObjectEntringCell" && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
