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
public class LiquidAsphalt : BaseLiquid
{
	public new const string ID = "asphalt";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "K", "y" };

	public LiquidAsphalt()
		: base("asphalt")
	{
		FlameTemperature = 240;
		VaporTemperature = 1240;
		Combustibility = 75;
		Adsorbence = 25;
		ThermalConductivity = 35;
		Fluidity = 1;
		Staining = 1;
		Weight = 0.5;
		InterruptAutowalk = true;
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
		return "{{K|asphalt}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{K|tarry}}";
	}

	public override string GetWaterRitualName()
	{
		return "tar";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{K|tarry}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{K|tarred}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{K|tar}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "stabilityMinor";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("{{K|It burns!}}");
		Target.TemperatureChange(500, Target);
		Damage value = new Damage(Stat.Roll(Liquid.Proportion("asphalt") / 100 + 1 + "d6"));
		Event @event = Event.New("TakeDamage");
		@event.AddParameter("Damage", value);
		@event.AddParameter("Owner", Liquid);
		@event.AddParameter("Attacker", Liquid);
		@event.AddParameter("Message", "from {{K|drinking asphalt}}!");
		Target.FireEvent(@event);
		ExitInterface = true;
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
		Liquid.ParentObject.pRender.ColorString = "&k^K";
		Liquid.ParentObject.pRender.TileColor = "&k";
		Liquid.ParentObject.pRender.DetailColor = "K";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&K";
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
			eRender.TileVariantColors("&k^K", "&k", "K");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&k^K", "&k", "K");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&k^K";
			pRender.TileColor = "&k";
			pRender.DetailColor = "K";
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

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Speckle/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (Liquid.IsOpenVolume() && !Liquid.ParentObject.IsFrozen() && !E.Object.Slimewalking && E.Object.HasPart("Body"))
		{
			int stickiness = GetStickiness(Liquid);
			if (!E.Object.MakeSave("Strength,Agility", stickiness, null, null, "Tar Stuck Restraint", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
			{
				E.Object.ApplyEffect(new Stuck(12, stickiness, "Tar Stuck Restraint"));
			}
		}
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (Slimewalking)
		{
			return 0;
		}
		if (Smart)
		{
			return GetStickiness(Liquid) / 2;
		}
		return 5;
	}

	public static int GetStickiness(LiquidVolume Liquid)
	{
		return Math.Min(24, 4 + Liquid.Amount("asphalt").DiminishingReturns(0.1));
	}
}
