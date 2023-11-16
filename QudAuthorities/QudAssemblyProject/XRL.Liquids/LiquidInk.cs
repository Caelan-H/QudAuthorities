using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidInk : BaseLiquid
{
	public new const string ID = "ink";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "y" };

	public LiquidInk()
		: base("ink")
	{
		FlameTemperature = 350;
		VaporTemperature = 1350;
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
		return "K";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{K|ink}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetWaterRitualName()
	{
		return "ink";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|inky}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|ink}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's very inky.");
		return true;
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (!Liquid.IsOpenVolume() || Liquid.IsWadingDepth() || !E.Object.HasPart("Body") || E.Object.Slimewalking)
		{
			return;
		}
		int sliminess = GetSliminess(Liquid, E.Object);
		if (!E.Object.MakeSave("Agility", sliminess, null, null, "Ink Slip Move", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (E.Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("{{K|You slip on the ink!}}");
			}
			E.Object.ParticleText("&K\u001a");
			E.Object.Move(Directions.GetRandomDirection(), Forced: true);
			if (E.Object.IsPlayer() && E.Object.CurrentZone != null)
			{
				E.Object.CurrentZone.SetActive();
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
			eRender.RenderString = "~";
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
			eRender.ColorString += "&k";
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
			return GetSliminess(Liquid, GO) / 3;
		}
		return 1;
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("scholarship", 1);
	}

	public int GetSliminess(LiquidVolume Liquid, GameObject GO)
	{
		return Math.Max(Math.Min(24, 5 + Liquid.Amount("ink").DiminishingReturns(0.3) - GO.GetIntProperty("Stable")), 0);
	}
}
