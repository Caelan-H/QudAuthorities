using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidOil : BaseLiquid
{
	public new const string ID = "oil";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "Y" };

	public LiquidOil()
		: base("oil")
	{
		FlameTemperature = 250;
		VaporTemperature = 2000;
		Combustibility = 90;
		ThermalConductivity = 40;
		Fluidity = 25;
		Staining = 1;
		Cleansing = 1;
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
		return "{{K|oil}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetWaterRitualName()
	{
		return "oil";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|oily}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|oil}}";
	}

	public override float GetValuePerDram()
	{
		return 3f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("{{K|Disgusting!}}");
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^K" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&Y^k";
		Liquid.ParentObject.pRender.TileColor = "&Y";
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
			eRender.TileVariantColors("&Y^k", "&Y", "k");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&Y^k";
			pRender.TileColor = "&Y";
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
				pRender.RenderString = "÷";
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

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (!Liquid.IsOpenVolume() || Liquid.IsWadingDepth() || !E.Object.HasPart("Body") || E.Object.Slimewalking)
		{
			return;
		}
		int slipperiness = GetSlipperiness(Liquid, E.Object);
		if (!E.Object.MakeSave("Agility", slipperiness, null, null, "Oil Slip Move", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (E.Object.IsPlayer())
			{
				BaseLiquid.AddPlayerMessage("You slip on the oil!", 'C');
			}
			E.Object.ParticleText("&C\u001a");
			E.Object.Move(Directions.GetRandomDirection(), Forced: true);
			if (E.Object.IsPlayer() && E.Object.CurrentZone != null)
			{
				E.Object.CurrentZone.SetActive();
			}
		}
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (Slimewalking)
		{
			return 0;
		}
		if (Smart && GO != null)
		{
			Uncacheable = true;
			return GetSlipperiness(Liquid, GO) / 3;
		}
		return 1;
	}

	public int GetSlipperiness(LiquidVolume Liquid, GameObject GO)
	{
		return Math.Max(Math.Min(24, 5 + Liquid.Amount("oil").DiminishingReturns(0.3) - GO.GetIntProperty("Stable")), 0);
	}
}
