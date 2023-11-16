using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
internal class DisperseEMP : IPart
{
	public bool Active;

	public string Tile;

	public string RenderString = "@";

	public int FlickerFrame;

	public int FrameOffset;

	public int Chance = 1500;

	public int Countdown = 20;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeDismemberedEvent.ID && ID != CanBeInvoluntarilyMovedEvent.ID && ID != BeginTakeActionEvent.ID && ID != GetMatterPhaseEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != GetScanTypeEvent.ID)
		{
			return ID == RespiresEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Active)
		{
			Countdown--;
			if (Countdown <= 0 && ParentObject.CurrentCell != null)
			{
				Countdown = 20;
				Active = false;
				ElectromagneticPulse.EMP(ParentObject.CurrentCell, 10, Stat.Roll("24-33"));
				ParentObject.Destroy();
			}
			return false;
		}
		if (Stat.Random(1, Chance) <= 1)
		{
			Active = true;
			ParentObject.Effects.Add(new Crackling());
			DidX("start", "crackling");
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

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
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

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
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
		if (!Active)
		{
			return true;
		}
		if (Tile == null)
		{
			Tile = ParentObject.pRender.Tile;
			RenderString = ParentObject.pRender.RenderString;
		}
		Render pRender = ParentObject.pRender;
		pRender.Tile = null;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 90;
		if (Stat.Random(1, 80) == 1 || FlickerFrame > 0)
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
			E.ColorString = "&W";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
			ParentObject.Sparksplatter();
		}
		else
		{
			pRender.RenderString = RenderString;
			pRender.Tile = Tile;
		}
		if (num < 4)
		{
			pRender.ColorString = "&W";
			pRender.TileColor = "&W";
			pRender.DetailColor = "c";
		}
		else if (num < 8)
		{
			pRender.ColorString = "&b";
			pRender.TileColor = "&b";
			pRender.DetailColor = "W";
		}
		else if (num < 12)
		{
			pRender.ColorString = "&c";
			pRender.TileColor = "&c";
			pRender.DetailColor = "b";
		}
		else
		{
			pRender.ColorString = "&C";
			pRender.TileColor = "&C";
			pRender.DetailColor = "W";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			pRender.ColorString = "&b";
			pRender.TileColor = "&b";
		}
		return false;
	}
}
