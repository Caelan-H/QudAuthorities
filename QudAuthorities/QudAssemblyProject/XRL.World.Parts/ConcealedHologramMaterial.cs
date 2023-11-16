using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ConcealedHologramMaterial : IPart
{
	public int FlickerFrame;

	public int FrameOffset;

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeDismemberedEvent.ID && ID != CanBeInvoluntarilyMovedEvent.ID && ID != GetMatterPhaseEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != GetScanTypeEvent.ID && ID != GetShortDescriptionEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == RespiresEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeImperviousToHeat();
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

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(4);
		return false;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ScanType = Scanning.Scan.Tech;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(ParentObject.It + ParentObject.GetVerb("flicker") + " subtly.", ' ');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.CurrentCell == null)
		{
			return true;
		}
		if (!ParentObject.CurrentCell.IsExplored())
		{
			return true;
		}
		if (!ParentObject.CurrentCell.IsVisible())
		{
			return true;
		}
		if (IComponent<GameObject>.ThePlayer?.CurrentCell == null)
		{
			return true;
		}
		if (ParentObject.DistanceTo(IComponent<GameObject>.ThePlayer) > 1)
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (Stat.Random(1, 200) == 1 || FlickerFrame > 0)
		{
			E.Tile = null;
			if (FlickerFrame == 0)
			{
				E.RenderString = "_";
			}
			if (FlickerFrame == 1)
			{
				E.RenderString = "-";
			}
			if (FlickerFrame == 2)
			{
				E.RenderString = "|";
			}
			E.ColorString = "&C";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		if (num < 4)
		{
			E.ColorString = "&C";
			E.DetailColor = "c";
		}
		else if (num < 8)
		{
			E.ColorString = "&b";
			E.DetailColor = "C";
		}
		else if (num < 12)
		{
			E.ColorString = "&c";
			E.DetailColor = "b";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			E.ColorString = "&Y";
		}
		return true;
	}
}
