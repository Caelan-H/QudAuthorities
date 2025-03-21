using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidSunSlag : BaseLiquid
{
	public new const string ID = "sunslag";

	[NonSerialized]
	public static List<string> Colors = new List<string>(3) { "Y", "W", "R" };

	public LiquidSunSlag()
		: base("sunslag")
	{
		VaporTemperature = 2000;
		VaporObject = "Steam";
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
		return "Y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{sunslag|sunslag}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{radiant|radiant}}";
	}

	public override string GetWaterRitualName()
	{
		return "sunslag";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{radiant|radiant}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{radiant|radiant}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{sunslag|sunslag}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Liquid.IsPure() && Target != null)
		{
			if (Target.GetIntProperty("SunslagDramsDrank") >= 10)
			{
				Message.Compound("Brightness burns your mouth, but you cannot be roused any higher.");
			}
			else if (!Target.HasStat("Speed"))
			{
				Message.Compound("Brightness burns your mouth.");
			}
			else
			{
				int num = DrinkMagnifier.Magnify(Target, 1);
				Target.Statistics["Speed"].BaseValue += num;
				Message.Compound("Brightness burns your mouth and inspirits you.\n\nYour Quickness is permanently increased by {{rules|").Append(num.ToString()).Append("}}.");
				Target.SetIntProperty("SunslagDramsDrank", Target.GetIntProperty("SunslagDramsDrank") + 1);
				JournalAPI.AddAccomplishment("You drank the bright cream of the Palladium Reef and were quickened.", "See =name= crack apart the bones of poleis and drink their marrow! Be quickened, our Saad, by the civic lifeblood!", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
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
				eRender.ColorString = "&Y";
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
		Liquid.ParentObject.pRender.ColorString = "&Y^W";
		Liquid.ParentObject.pRender.TileColor = "&Y";
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
			eRender.TileVariantColors("&Y^W", "&Y", "W");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&Y^W", "&Y", "W");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&Y^W";
			pRender.TileColor = "&Y";
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
			return "Liquids/Paisley/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("stars", 1);
	}

	public override float GetValuePerDram()
	{
		return 1000f;
	}
}
