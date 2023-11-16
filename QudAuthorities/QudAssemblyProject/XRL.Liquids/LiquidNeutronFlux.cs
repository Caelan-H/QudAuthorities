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
public class LiquidNeutronFlux : BaseLiquid
{
	public new const string ID = "neutronflux";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "y", "Y" };

	public LiquidNeutronFlux()
		: base("neutronflux")
	{
		VaporTemperature = 10000;
		FreezeTemperature = -100;
		BrittleTemperature = -250;
		ThermalConductivity = 0;
		Fluidity = 100;
		Evaporativity = 100;
		Cleansing = 100;
		Weight = 2.5;
		InterruptAutowalk = true;
		CirculatoryLossTerm = "emitting";
		ConsiderDangerousToContact = true;
		ConsiderDangerousToDrink = true;
		Glows = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "y";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{neutronic|neutron}} {{Y|flux}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{neutronic|neutronic}}";
	}

	public override string GetWaterRitualName()
	{
		return "flux";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{neutronic|neutronic}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{neutronic|neutral}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{neutronic|flux}}";
	}

	public override float GetValuePerDram()
	{
		return 1000f;
	}

	public override string GetPreparedCookingIngredient()
	{
		return "density";
	}

	private bool Explode(GameObject obj, int Phase = 0)
	{
		return obj.Explode(15000, null, "10d10+250", 1f, Neutron: true, SuppressDestroy: false, Indirect: false, Phase);
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Explode(Target);
		ExitInterface = true;
		return true;
	}

	public override void BeforeRender(LiquidVolume Liquid)
	{
		if (!Liquid.Sealed || Liquid.LiquidVisibleWhenSealed)
		{
			Liquid.AddLight(0);
		}
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^Y" + eRender.ColorString;
		}
	}

	public override void MixedWith(LiquidVolume Liquid, LiquidVolume NewLiquid, ref bool ExitInterface)
	{
		base.MixedWith(Liquid, NewLiquid, ref ExitInterface);
		if (NewLiquid.ParentObject != null && NewLiquid.ParentObject.GetCurrentCell() != null && NewLiquid.ParentObject.FireEvent("NeutronFluxPourExplodes") && (Liquid.ParentObject == null || Liquid.ParentObject.FireEvent("NeutronFluxPourExplodes")))
		{
			int num = 0;
			if (Explode(Phase: (Liquid.ParentObject == null || !Liquid.ContainsLiquid("neutronflux")) ? NewLiquid.ParentObject.GetPhase() : (Liquid.ParentObject.GetObjectContext() ?? Liquid.ParentObject).GetPhase(), obj: NewLiquid.ParentObject))
			{
				ExitInterface = true;
			}
		}
	}

	public override bool EnteredCell(LiquidVolume Liquid, ref bool ExitInterface)
	{
		bool result = base.EnteredCell(Liquid, ref ExitInterface);
		if (Liquid.IsOpenVolume() && Explode(Liquid.ParentObject))
		{
			ExitInterface = true;
			result = false;
		}
		return result;
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&Y^y";
		Liquid.ParentObject.pRender.TileColor = "&Y";
		Liquid.ParentObject.pRender.DetailColor = "y";
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
			eRender.ColorString += "&Y";
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

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		return 99;
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("stars", 1);
	}
}
