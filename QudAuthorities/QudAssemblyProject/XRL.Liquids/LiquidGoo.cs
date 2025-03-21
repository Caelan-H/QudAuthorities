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
public class LiquidGoo : BaseLiquid
{
	public new const string ID = "goo";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "G", "Y" };

	public LiquidGoo()
		: base("goo")
	{
		FlameTemperature = 400;
		VaporTemperature = 110;
		VaporObject = "PoisonGas";
		Combustibility = 20;
		ThermalConductivity = 40;
		Fluidity = 10;
		Evaporativity = 1;
		Staining = 1;
		CirculatoryLossTerm = "oozing";
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "G";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{G|green goo}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{G|gooey}}";
	}

	public override string GetWaterRitualName()
	{
		return "goo";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{G|gooey}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{G|gooey}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{G|goo}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "selfPoison";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's awful!");
		if (Target.ApplyEffect(new Poisoned(Stat.Roll("1d4+4"), Stat.Roll("1d2+2") + "d2", 10)))
		{
			Message.Compound("You feel poison course through your veins!");
			ExitInterface = true;
		}
		return true;
	}

	public override void ObjectGoingProne(LiquidVolume Liquid, GameObject GO)
	{
		if (Liquid.IsWadingDepth())
		{
			if (GO.IsPlayer())
			{
				BaseLiquid.AddPlayerMessage("{{R|Poisonous goo burns your eyes.}}");
			}
			GO.Splatter("&w.");
			if (!GO.MakeSave("Toughness", 30, null, null, "Goo Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				GO.ApplyEffect(new Poisoned(Stat.Roll("1d4+4"), Stat.Roll("1d2+2") + "d2", 10));
			}
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&G";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^G" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&Y^G";
		Liquid.ParentObject.pRender.TileColor = "&Y";
		Liquid.ParentObject.pRender.DetailColor = "G";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&G";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&Y^G", "&Y", "G");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&Y^G", "&Y", "G");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&Y^G";
			pRender.TileColor = "&Y";
			pRender.DetailColor = "G";
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
			eRender.ColorString += "&G";
		}
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Splotchy/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (!Smart)
		{
			return 1;
		}
		return 3;
	}
}
