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
public class LiquidWater : BaseLiquid
{
	public new const string ID = "water";

	[NonSerialized]
	public static List<string> Colors = new List<string>(4) { "B", "y", "Y", "b" };

	public LiquidWater()
		: base("water")
	{
		Combustibility = -50;
		VaporObject = "SteamGas";
		Fluidity = 30;
		Evaporativity = 2;
		Cleansing = 5;
		EnableCleaning = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "B";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		if (Liquid != null && Liquid.IsPureLiquid("water"))
		{
			return "{{B|fresh water}}";
		}
		return "{{B|water}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		if (Liquid == null)
		{
			return "{{Y|dilute}}";
		}
		if (Liquid.ComponentLiquids["water"] > 0)
		{
			return "{{Y|dilute}}";
		}
		return null;
	}

	public override string GetWaterRitualName()
	{
		return "water";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		if (Liquid.IsMixed())
		{
			if (Liquid.Proportion("salt", "blood") > Liquid.Proportion("water"))
			{
				return null;
			}
			if (Liquid.Primary != "oil" && Liquid.Primary != "lava" && Liquid.Primary != "wax")
			{
				return "{{Y|dilute}}";
			}
		}
		return "{{B|wet}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{B|wet}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{B|water}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart("Stomach") && !Target.FireEvent(new Event("AddWater", "Amount", 10 * Liquid.ComponentLiquids["water"])))
		{
			return false;
		}
		if (Target.HasPart("Amphibious"))
		{
			Message.Compound("You pour the water on " + Target.itself + "!");
		}
		else if (Liquid.IsFreshWater())
		{
			Message.Compound("Ahh, refreshing!");
		}
		return true;
	}

	public override void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
		if (Liquid.IsOpenVolume() && Liquid.ParentObject.IsFrozen() && E.Object.HasPart("Body") && !E.Object.MakeSave("Agility", 10 - E.Object.GetIntProperty("Stable"), null, null, "Ice Slip Move", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, Liquid.ParentObject))
		{
			if (E.Object.IsPlayer())
			{
				BaseLiquid.AddPlayerMessage("You slip on the ice!", 'C');
			}
			E.Object.ParticleText("&C\u001a");
			E.Object.Move(Directions.GetRandomDirection(), Forced: true);
			if (E.Object.IsPlayer() && E.Object.CurrentZone != null)
			{
				E.Object.CurrentZone.SetActive();
			}
		}
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		if (Target.HasPart("Amphibious") && Liquid.IsFreshWater())
		{
			Target.FireEvent(Event.New("AddWater", "Amount", 100000 * Liquid.Volume, "Forced", 1));
			Liquid.Empty();
		}
		base.SmearOn(Liquid, Target, By, FromCell);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^b" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&b^B";
		Liquid.ParentObject.pRender.TileColor = "&b";
		Liquid.ParentObject.pRender.DetailColor = "B";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&b";
	}

	public override void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (!Liquid.IsWadingDepth() || Liquid.GetSecondaryLiquidID() == "algae" || Liquid.ParentObject == null)
		{
			return;
		}
		if (Liquid.ParentObject.IsFrozen())
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&c^C", "&c", "C");
			return;
		}
		if (Liquid.IsFreshWater())
		{
			Render pRender = Liquid.ParentObject.pRender;
			int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
			if (Stat.RandomCosmetic(1, 600) == 1)
			{
				eRender.RenderString = "÷";
				eRender.TileVariantColors("&Y^B", "&Y", "B");
			}
			if (pRender.ColorString == "&b" || Stat.RandomCosmetic(1, 60) == 1)
			{
				if (num < 15)
				{
					pRender.RenderString = "÷";
					pRender.ColorString = "&b^B";
					pRender.TileColor = "&b";
					pRender.DetailColor = "B";
				}
				else if (num < 30)
				{
					pRender.RenderString = " ";
					pRender.ColorString = "&Y^B";
					pRender.TileColor = "&Y";
					pRender.DetailColor = "B";
				}
				else if (num < 45)
				{
					pRender.RenderString = "÷";
					pRender.ColorString = "&b^B";
					pRender.TileColor = "&b";
					pRender.DetailColor = "B";
				}
				else
				{
					pRender.RenderString = "~";
					pRender.ColorString = "&y^B";
					pRender.TileColor = "&y";
					pRender.DetailColor = "B";
				}
			}
			return;
		}
		if (Liquid.Flowing)
		{
			int num2 = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
			if (num2 < 15)
			{
				eRender.RenderString = "~";
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			else if (num2 < 30)
			{
				eRender.RenderString = Liquid.ParentObject.pRender.RenderString;
				eRender.TileVariantColors("&Y^b", "&Y", "b");
			}
			else if (num2 < 45)
			{
				eRender.RenderString = "~";
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			else
			{
				eRender.RenderString = Liquid.ParentObject.pRender.RenderString;
				eRender.TileVariantColors("&B^b", "&B", "b");
			}
			return;
		}
		Render pRender2 = Liquid.ParentObject.pRender;
		int num3 = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "~";
			eRender.TileVariantColors("&Y^b", "&Y", "b");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			if (num3 < 15)
			{
				pRender2.RenderString = "÷";
				pRender2.ColorString = "&B^b";
				pRender2.TileColor = "&B";
				pRender2.DetailColor = "b";
			}
			else if (num3 < 30)
			{
				pRender2.RenderString = "~";
				pRender2.ColorString = "&B^b";
				pRender2.TileColor = "&B";
				pRender2.DetailColor = "b";
			}
			else if (num3 < 45)
			{
				pRender2.RenderString = " ";
				pRender2.ColorString = "&B^b";
				pRender2.TileColor = "&B";
				pRender2.DetailColor = "b";
			}
			else
			{
				pRender2.RenderString = "~";
				pRender2.ColorString = "&B^b";
				pRender2.TileColor = "&B";
				pRender2.DetailColor = "b";
			}
		}
	}

	public override void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString += "&b";
		}
	}
}
