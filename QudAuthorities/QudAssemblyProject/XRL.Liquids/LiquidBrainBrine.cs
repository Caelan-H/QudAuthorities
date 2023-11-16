using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidBrainBrine : BaseLiquid
{
	public new const string ID = "brainbrine";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "g", "W" };

	public LiquidBrainBrine()
		: base("brainbrine")
	{
		VaporTemperature = 2000;
		VaporObject = "ConfusionGas";
		Combustibility = 30;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 3;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "g";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{brainbrine|brain brine}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{nervous|nervous}}";
	}

	public override string GetWaterRitualName()
	{
		return "brain brine";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{nervous|nervous}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{nervous|nervous}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{brainbrine|brain-brine}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Liquid.IsPure())
		{
			Message.Compound("It tastes salty. Your mind starts to swim.");
			int duration = Stat.Random(20, 30);
			if (Target != null)
			{
				Target.ApplyEffect(new Confused(duration, 1, 3));
				Target.ApplyEffect(new BrainBrineCurse());
				if (Target.IsPlayer())
				{
					AchievementManager.IncrementAchievement("ACH_20_DRAMS_BRAIN_BRINE", Volume);
				}
			}
		}
		return true;
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&W";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^W" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&g^W";
		Liquid.ParentObject.pRender.TileColor = "&g";
		Liquid.ParentObject.pRender.DetailColor = "W";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&W";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&g^W", "&g", "W");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&g^W", "&g", "W");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&g^W";
			pRender.TileColor = "&g";
			pRender.DetailColor = "W";
			if (num < 15)
			{
				pRender.RenderString = "÷";
			}
			else if (num < 30)
			{
				pRender.RenderString = "~";
			}
			else if (num < 45)
			{
				pRender.RenderString = " ";
			}
			else
			{
				pRender.RenderString = "~";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&W";
		}
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Dot/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("scholarship", 1);
	}

	public override float GetValuePerDram()
	{
		return 1233f;
	}
}
