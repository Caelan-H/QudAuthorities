using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ThinWorldMaterial : IPart
{
	public string Tile;

	public string RenderString = "@";

	public int FlickerFrame;

	public int FrameOffset;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeDismemberedEvent.ID && ID != CanBeInvoluntarilyMovedEvent.ID && ID != GetMatterPhaseEvent.ID)
		{
			return ID == GetMaximumLiquidExposureEvent.ID;
		}
		return true;
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

	public override bool Render(RenderEvent E)
	{
		if (Tile == null)
		{
			Tile = ParentObject.pRender.Tile;
			RenderString = ParentObject.pRender.RenderString;
		}
		Render pRender = ParentObject.pRender;
		pRender.Tile = null;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (Stat.Random(1, 200) == 1 || FlickerFrame > 0)
		{
			pRender.Tile = null;
			if (FlickerFrame == 0)
			{
				pRender.RenderString = "_";
			}
			if (FlickerFrame == 1)
			{
				pRender.RenderString = "-";
			}
			if (FlickerFrame == 2)
			{
				pRender.RenderString = "|";
			}
			E.ColorString = "&C";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		else
		{
			pRender.RenderString = RenderString;
			pRender.Tile = Tile;
		}
		if (num < 4)
		{
			pRender.ColorString = "&C";
			pRender.TileColor = "&C";
			pRender.DetailColor = "c";
		}
		else if (num < 8)
		{
			pRender.ColorString = "&b";
			pRender.TileColor = "&b";
			pRender.DetailColor = "C";
		}
		else if (num < 12)
		{
			pRender.ColorString = "&c";
			pRender.TileColor = "&c";
			pRender.DetailColor = "b";
		}
		else
		{
			pRender.ColorString = "&B";
			pRender.TileColor = "&B";
			pRender.DetailColor = "b";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			pRender.ColorString = "&Y";
			pRender.TileColor = "&Y";
		}
		return true;
	}
}
