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
public class LiquidSap : BaseLiquid
{
	public new const string ID = "sap";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "W", "Y" };

	public LiquidSap()
		: base("sap")
	{
		FlameTemperature = 250;
		VaporTemperature = 1250;
		Combustibility = 70;
		Adsorbence = 25;
		ThermalConductivity = 40;
		Fluidity = 3;
		Evaporativity = 1;
		Staining = 2;
		Weight = 0.5;
		InterruptAutowalk = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "W";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{W|sap}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|sugary}}";
	}

	public override string GetWaterRitualName()
	{
		return "sap";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{W|sappy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{W|sappy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{W|sap}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart("Stomach"))
		{
			Target.FireEvent(Event.New("AddFood", "Satiation", "Snack"));
			Message.Compound("It's sweet to the taste.");
		}
		return true;
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
		Liquid.ParentObject.pRender.ColorString = "&W^Y";
		Liquid.ParentObject.pRender.TileColor = "&W";
		Liquid.ParentObject.pRender.DetailColor = "Y";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&W";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth() || Liquid == null || Liquid.ParentObject == null)
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&W^Y", "&W", "Y");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&W^Y", "&W", "Y");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&W^Y";
			pRender.TileColor = "&W";
			pRender.DetailColor = "Y";
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
			eRender.ColorString += "&W";
		}
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

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (Liquid.IsOpenVolume() && !Liquid.ParentObject.IsFrozen() && !E.Object.Slimewalking && E.Object.HasPart("Body"))
		{
			int stickiness = GetStickiness(Liquid);
			if (!E.Object.MakeSave("Strength,Agility", stickiness, null, null, "Sap Stuck Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				E.Object.ApplyEffect(new Stuck(12, stickiness, "Sap Stuck Restraint"));
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
		return 1;
	}

	public int GetStickiness(LiquidVolume Liquid)
	{
		return Math.Min(24, 1 + Liquid.Amount("sap").DiminishingReturns(0.1));
	}
}
