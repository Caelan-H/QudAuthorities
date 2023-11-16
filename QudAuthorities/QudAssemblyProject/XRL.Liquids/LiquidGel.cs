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
public class LiquidGel : BaseLiquid
{
	public new const string ID = "gel";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "Y", "y" };

	public LiquidGel()
		: base("gel")
	{
		ThermalConductivity = 70;
		Fluidity = 5;
		Evaporativity = 1;
		Cleansing = 1;
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
		return "{{Y|gel}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetWaterRitualName()
	{
		return "gel";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("It's very greasy.");
		return true;
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{Y|unctuous}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{Y|gel}}";
	}

	public override float GetValuePerDram()
	{
		return 0.5f;
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (!Liquid.IsOpenVolume() || Liquid.IsWadingDepth() || !E.Object.HasPart("Body") || E.Object.Slimewalking)
		{
			return;
		}
		int sliminess = GetSliminess(Liquid, E.Object);
		if (!E.Object.MakeSave("Agility", sliminess, null, null, "Gel Slip Move", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (E.Object.IsPlayer())
			{
				BaseLiquid.AddPlayerMessage("{{Y|You slip on the gel!}}");
			}
			E.Object.ParticleText("&Y\u001a");
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
				eRender.ColorString = "&Y";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^Y" + eRender.ColorString;
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
		Liquid.ParentObject.pRender.ColorString += "&Y";
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
			eRender.TileVariantColors("&y^C", "&y", "C");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&y^C", "&y", "C");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&y^Y";
			pRender.TileColor = "&y";
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
			eRender.ColorString += "&Y";
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

	public int GetSliminess(LiquidVolume Liquid, GameObject GO)
	{
		return Math.Max(Math.Min(24, 5 + Liquid.Amount("gel").DiminishingReturns(0.3) - GO.GetIntProperty("Stable")), 1);
	}
}
