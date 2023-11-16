using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class HologramMaterialPrimary : IPoweredPart
{
	public string Tile;

	public string RenderString = "@";

	public int FlickerFrame;

	public int FrameOffset;

	public HologramMaterialPrimary()
	{
		ChargeUse = 1;
		IsBootSensitive = false;
		IsEMPSensitive = true;
		MustBeUnderstood = false;
		WorksOnWearer = true;
		WorksOnSelf = true;
	}

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeDismemberedEvent.ID && ID != CanBeInvoluntarilyMovedEvent.ID && ID != GetMatterPhaseEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != GetScanTypeEvent.ID && ID != ObjectCreatedEvent.ID)
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

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override void Initialize()
	{
		base.Initialize();
		Tile = ParentObject.pRender.Tile;
		RenderString = ParentObject.pRender.RenderString;
	}

	public override bool Render(RenderEvent E)
	{
		Render pRender = ParentObject.pRender;
		pRender.Tile = null;
		if (GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) == ActivePartStatus.Operational)
		{
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
			}
			else if (num < 8)
			{
				pRender.ColorString = "&b";
			}
			else if (num < 12)
			{
				pRender.ColorString = "&c";
			}
			else
			{
				pRender.ColorString = "&B";
			}
			if (!Options.DisableTextAnimationEffects)
			{
				FrameOffset += Stat.Random(0, 20);
			}
			if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
			{
				pRender.ColorString = "&Y";
			}
		}
		else
		{
			pRender.Tile = Tile;
			pRender.ColorString = "&K";
		}
		return true;
	}
}
