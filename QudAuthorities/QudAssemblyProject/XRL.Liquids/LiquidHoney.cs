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
public class LiquidHoney : BaseLiquid
{
	public new const string ID = "honey";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "w", "W" };

	public LiquidHoney()
		: base("honey")
	{
		FlameTemperature = 300;
		VaporTemperature = 1300;
		Combustibility = 60;
		Adsorbence = 25;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Weight = 0.5;
		InterruptAutowalk = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "w";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{w|honey}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{w|honeyed}}";
	}

	public override string GetWaterRitualName()
	{
		return "honey";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{w|sticky}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{w|sticky}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{w|honey}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "medicinalMinor";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart("Stomach"))
		{
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			Message.Compound("Delicious!");
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^w" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&w^W";
		Liquid.ParentObject.pRender.TileColor = "&w";
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
			eRender.TileVariantColors("&W^w", "&W", "w");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&W^w", "&W", "w");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&W^w";
			pRender.TileColor = "&W";
			pRender.DetailColor = "w";
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
			eRender.ColorString += "&w";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&w";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Gunk/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (Liquid.IsOpenVolume() && !Liquid.ParentObject.IsFrozen() && !E.Object.Slimewalking && E.Object.HasPart("Body"))
		{
			int stickiness = GetStickiness(Liquid);
			if (!E.Object.MakeSave("Strength,Agility", stickiness, null, null, "Honey Stuck Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				E.Object.ApplyEffect(new Stuck(12, stickiness, "Honey Stuck Restraint"));
			}
		}
	}

	public override float GetValuePerDram()
	{
		return 2f;
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (Slimewalking)
		{
			return 0;
		}
		if (Smart)
		{
			return GetStickiness(Liquid);
		}
		return 2;
	}

	public int GetStickiness(LiquidVolume Liquid)
	{
		return Math.Min(24, 1 + Liquid.Amount("honey").DiminishingReturns(0.1));
	}
}
