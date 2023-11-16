using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[IsLiquid]
public class LiquidProteanGunk : BaseLiquid
{
	public new const string ID = "proteangunk";

	public const int MAXIMUM_SLUDGE = 30;

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "c", "C" };

	[NonSerialized]
	public static Zone LastZone;

	[NonSerialized]
	public static long LastZoneCheck = -2147483648L;

	[NonSerialized]
	public static long LastZoneResult = 0L;

	public LiquidProteanGunk()
		: base("proteangunk")
	{
		VaporTemperature = 2000;
		Combustibility = 40;
		ThermalConductivity = 40;
		Fluidity = 25;
		Evaporativity = 1;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "c";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{c|primordial soup}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{c|soupy}}";
	}

	public override string GetWaterRitualName()
	{
		return "soup";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{c|soupy}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{c|soupy}}";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{c|soup}}";
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		if (Target.HasPart("Stomach"))
		{
			Target.FireEvent(Event.New("AddWater", "Amount", 2 * Volume, "Forced", 1));
			Message.Compound("You feel the soup slosh around your stomach.");
		}
		return true;
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^c" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&c^C";
		Liquid.ParentObject.pRender.TileColor = "&c";
		Liquid.ParentObject.pRender.DetailColor = "C";
	}

	public override void BaseRenderSecondary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString += "&c";
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
			eRender.TileVariantColors("&c^C", "&c", "C");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&c^C", "&c", "C");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&c^C";
			pRender.TileColor = "&c";
			pRender.DetailColor = "C";
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
			eRender.ColorString += "&c";
		}
	}

	public override void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
		if (eRender.ColorsVisible)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 15)
			{
				eRender.ColorString = "&c";
			}
		}
		base.RenderSmearPrimary(Liquid, eRender, obj);
	}

	public override string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			return "Liquids/Gunk/";
		}
		return base.GetPaintAtlas(Liquid);
	}

	public override float GetValuePerDram()
	{
		return 4f;
	}

	public override void EndTurn(LiquidVolume Liquid, GameObject GO)
	{
		int intProperty = GO.GetIntProperty("SoupCounter");
		string primaryLiquidID = Liquid.GetPrimaryLiquidID();
		string secondaryLiquidID = Liquid.GetSecondaryLiquidID();
		string stringProperty = GO.GetStringProperty("ReactingSecondary");
		if (secondaryLiquidID != null && Liquid.IsMixed() && primaryLiquidID == "proteangunk" && secondaryLiquidID != primaryLiquidID && Liquid.Volume >= 500 && (stringProperty == secondaryLiquidID || stringProperty == null))
		{
			if (intProperty == 1)
			{
				GameObject gameObject = GameObject.create("SoupSludge");
				gameObject.AddPart(new SoupSludge(secondaryLiquidID));
				GO.CurrentCell.getClosestPassableCellFor(gameObject).AddObject(gameObject);
				gameObject.MakeActive();
				Messaging.EmitMessage(GO, "The reacting liquids congeal into a " + gameObject.ShortDisplayName + ".");
				Liquid.UseDrams(500);
				GO.SetStringProperty("ReactingSecondary", null);
				GO.SetIntProperty("SoupCounter", -5);
			}
			else if (intProperty < 0 && CanSpawnSludgeIn(GO.CurrentZone))
			{
				GO.SetIntProperty("SoupCounter", Stat.Random(10, 150));
				GO.SetStringProperty("ReactingSecondary", secondaryLiquidID);
				if (secondaryLiquidID != null)
				{
					Messaging.EmitMessage(GO, "The primordial soup starts reacting with the " + LiquidVolume.getLiquid(secondaryLiquidID).GetName(Liquid) + ".");
				}
			}
			GO.SetIntProperty("SoupCounter", GO.GetIntProperty("SoupCounter") - 1);
		}
		else if (intProperty > 0)
		{
			Messaging.EmitMessage(GO, "The liquids stop reacting.");
			GO.SetIntProperty("SoupCounter", -5);
		}
		base.EndTurn(Liquid, GO);
	}

	public override void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
		E.Add("chance", 1);
	}

	public static bool CanSpawnSludgeIn(Zone Z)
	{
		if (!Z.IsActive())
		{
			return false;
		}
		if (Z != LastZone)
		{
			LastZone = Z;
			LastZoneCheck = -2147483648L;
			LastZoneResult = 0L;
		}
		if (The.Game.Turns - 20 >= LastZoneCheck)
		{
			LastZoneCheck = The.Game.Turns;
			LastZoneResult = 0L;
			foreach (GameObject item in Z.YieldObjects())
			{
				if (item.Blueprint == "SoupSludge" || (item._Property != null && item._Property.TryGetValue("ReactingSecondary", out var value) && value != null))
				{
					LastZoneResult++;
				}
			}
		}
		return LastZoneResult++ < 30;
	}
}
