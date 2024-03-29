using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidOoze : BaseLiquid
{
	public new const string ID = "ooze";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "y" };

	public LiquidOoze()
		: base("ooze")
	{
		FlameTemperature = 500;
		VaporTemperature = 150;
		VaporObject = "Miasma";
		Combustibility = 15;
		ThermalConductivity = 30;
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
		return "K";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{K|black ooze}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|oozing}}";
	}

	public override string GetWaterRitualName()
	{
		return "ooze";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|oozing}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|oozing}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|ooze}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "selfGlotrot";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's repulsive!");
		bool flag = false;
		if ((Target.CurrentZone.Z % 2 != 0) ? Target.ApplyEffect(new GlotrotOnset()) : Target.ApplyEffect(new IronshankOnset()))
		{
			Message.Compound("You feel sick!");
		}
		return true;
	}

	public override void ObjectGoingProne(LiquidVolume Liquid, GameObject GO)
	{
		if (!Liquid.IsWadingDepth())
		{
			return;
		}
		if (GO.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("{{R|Putrid ooze splashes into your mouth. You gag at the awful taste.}}");
		}
		GO.Splatter("&w.");
		if (!GO.MakeSave("Toughness", 13, null, null, "Ooze Disease", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (GO.pPhysics.CurrentCell.ParentZone.Z % 2 == 0)
			{
				GO.ApplyEffect(new IronshankOnset());
			}
			else
			{
				GO.ApplyEffect(new GlotrotOnset());
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
				eRender.ColorString = "&K";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^k" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&y^k";
		Liquid.ParentObject.pRender.TileColor = "&y";
		Liquid.ParentObject.pRender.DetailColor = "k";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&k";
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
			eRender.TileVariantColors("&y^k", "&y", "k");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&y^k", "&y", "k");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&y^k";
			pRender.TileColor = "&y";
			pRender.DetailColor = "k";
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
			eRender.ColorString += "&k";
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
