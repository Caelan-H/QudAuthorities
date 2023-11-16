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
public class LiquidPutrescence : BaseLiquid
{
	public new const string ID = "putrid";

	[NonSerialized]
	public static List<string> Colors = new List<string>(3) { "K", "g", "w" };

	public LiquidPutrescence()
		: base("putrid")
	{
		FlameTemperature = 600;
		VaporTemperature = 1600;
		Combustibility = 5;
		ThermalConductivity = 25;
		Fluidity = 15;
		Evaporativity = 1;
		Staining = 2;
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
		return "{{putrid|putrescence}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetWaterRitualName()
	{
		return "putrescence";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{putrid|putrid}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{putrid|putrescence}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.IsPlayer())
		{
			Message.Compound("It's disgusting! You vomit!");
		}
		else
		{
			MessageQueue.AddPlayerMessage(Target.The + Target.ShortDisplayName + Target.GetVerb("vomit") + "!");
		}
		Stomach stomach = Target.GetPart("Stomach") as Stomach;
		stomach.Water -= Stat.Random(20000, 30000);
		if (stomach.Water < 0)
		{
			stomach.Water = 0;
		}
		Target.ApplyEffect(new LiquidCovered("putrid", 2));
		GameObject gameObject = GameObject.create("VomitPool");
		if (Target.CurrentCell != null && !Target.OnWorldMap())
		{
			Target.CurrentCell.AddObject(gameObject);
		}
		else
		{
			gameObject.Obliterate();
		}
		Target.UseEnergy(1000, "Vomit");
		ExitInterface = true;
		return true;
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
			eRender.ColorString += "^K";
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&g^K";
		Liquid.ParentObject.pRender.TileColor = "&g";
		Liquid.ParentObject.pRender.DetailColor = "K";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&g";
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
			eRender.TileVariantColors("&G^K", "&G", "K");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
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
			eRender.ColorString += "&K";
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
		return 0;
	}
}
