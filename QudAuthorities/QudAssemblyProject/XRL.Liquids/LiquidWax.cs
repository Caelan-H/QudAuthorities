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
public class LiquidWax : BaseLiquid
{
	public new const string ID = "wax";

	[NonSerialized]
	public static List<string> Colors = new List<string>(1) { "Y" };

	public LiquidWax()
		: base("wax")
	{
		FlameTemperature = 300;
		VaporTemperature = 2000;
		Temperature = 100;
		Combustibility = 65;
		Adsorbence = 25;
		ThermalConductivity = 40;
		Fluidity = 7;
		Cleansing = 3;
		Weight = 0.5;
		InterruptAutowalk = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "Y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{Y|molten wax}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|waxen}}";
	}

	public override string GetWaterRitualName()
	{
		return "wax";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{Y|waxy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|waxy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|wax}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart("Stomach"))
		{
			Message.Compound("It's hot and disgusting.");
			Target.TemperatureChange(100, Target);
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^y" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&y^Y";
		Liquid.ParentObject.pRender.TileColor = "&y";
		Liquid.ParentObject.pRender.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&y";
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
			eRender.TileVariantColors("&Y^y", "&Y", "y");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&Y^y", "&Y", "y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&Y^y";
			pRender.TileColor = "&Y";
			pRender.DetailColor = "y";
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
				pRender.RenderString = "\t";
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
			eRender.ColorString += "&y";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&y";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Splotchy/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (Liquid.IsOpenVolume() && !Liquid.ParentObject.IsFrozen() && !E.Object.Slimewalking && E.Object.HasPart("Body"))
		{
			int stickiness = GetStickiness(Liquid);
			if (!E.Object.MakeSave("Strength,Agility", stickiness, null, null, "Wax Stuck Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				E.Object.ApplyEffect(new Stuck(12, stickiness, "Wax Stuck Restraint"));
			}
		}
	}

	public override float GetValuePerDram()
	{
		return 0f;
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (Slimewalking)
		{
			return 1;
		}
		if (Smart)
		{
			return GetStickiness(Liquid) / 2 + 1;
		}
		return 2;
	}

	public int GetStickiness(LiquidVolume Liquid)
	{
		return Math.Min(24, 1 + Liquid.Amount("wax").DiminishingReturns(0.1));
	}
}
