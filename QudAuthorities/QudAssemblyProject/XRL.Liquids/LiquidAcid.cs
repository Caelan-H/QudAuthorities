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
public class LiquidAcid : BaseLiquid
{
	public new const string ID = "acid";

	[NonSerialized]
	public static List<string> Colors = new List<string>(2) { "G", "g" };

	public LiquidAcid()
		: base("acid")
	{
		Combustibility = 3;
		ThermalConductivity = 45;
		Fluidity = 30;
		Evaporativity = 1;
		Staining = 1;
		Cleansing = 30;
		VaporObject = "AcidGas";
		InterruptAutowalk = true;
		ConsiderDangerousToContact = true;
		ConsiderDangerousToDrink = true;
	}

	public override List<string> GetColors()
	{
		return Colors;
	}

	public override string GetColor()
	{
		return "G";
	}

	public override string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "{{G|acidic}}";
	}

	public override string GetSmearedName(LiquidVolume Liquid)
	{
		return "{{G|acid-covered}}";
	}

	public override string GetName(LiquidVolume Liquid)
	{
		return "{{G|acid}}";
	}

	public override string GetWaterRitualName()
	{
		return "acid";
	}

	public override string GetStainedName(LiquidVolume Liquid)
	{
		return "{{G|acid}}";
	}

	public override string GetAdjective(LiquidVolume Liquid)
	{
		return "{{G|acidic}}";
	}

	public override string GetPreparedCookingIngredient()
	{
		return "acidMinor";
	}

	public override bool SafeContainer(GameObject GO)
	{
		return GO.GetIntProperty("Inorganic") != 0;
	}

	public override float GetValuePerDram()
	{
		return 1.5f;
	}

	public override bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		Message.Compound("{{G|IT BURNS!}}");
		string dice = Liquid.Proportion("acid") / 100 + 1 + "d10";
		Target.TakeDamage(dice.Roll(), "from {{G|drinking acid}}!", "Acid", null, null, Target, Liquid.ParentObject);
		ExitInterface = true;
		return true;
	}

	public override void FillingContainer(GameObject Container, LiquidVolume Liquid)
	{
		if (!SafeContainer(Container))
		{
			Container.ApplyEffect(new ContainedAcidEating());
		}
		base.FillingContainer(Container, Liquid);
	}

	public override void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
		if (eRender.ColorsVisible)
		{
			eRender.ColorString = "^g" + eRender.ColorString;
		}
	}

	public override void BaseRenderPrimary(LiquidVolume Liquid)
	{
		Liquid.ParentObject.pRender.ColorString = "&G^g";
		Liquid.ParentObject.pRender.TileColor = "&G";
		Liquid.ParentObject.pRender.DetailColor = "g";
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
			eRender.TileVariantColors("&G^g", "&G", "g");
			return;
		}
		Render pRender = Liquid.ParentObject.pRender;
		int num = (XRLCore.CurrentFrame + Liquid.nFrameOffset) % 60;
		if (Stat.RandomCosmetic(1, 600) == 1)
		{
			eRender.RenderString = "\u000f";
			eRender.TileVariantColors("&G^g", "&G", "g");
		}
		if (Stat.RandomCosmetic(1, 60) == 1)
		{
			pRender.ColorString = "&G^g";
			pRender.TileColor = "&G";
			pRender.DetailColor = "g";
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
			eRender.ColorString += "&G";
		}
	}

	public void ApplyAcid(LiquidVolume Liquid, GameObject GO, GameObject By, bool FromCell = false)
	{
		int liquidExposureMillidrams = Liquid.GetLiquidExposureMillidrams(GO, "acid");
		int num = liquidExposureMillidrams / 20000 + Stat.Random(1, liquidExposureMillidrams) / 10000 + ((Stat.Random(0, 10000) < liquidExposureMillidrams) ? 1 : 0) + ((Stat.Random(0, 100000) < liquidExposureMillidrams) ? 4 : 0);
		GO.TakeDamage(num, "from {{G|acid}}!", "Acid", null, null, null, Environmental: FromCell, Attacker: By ?? Liquid.ParentObject, Source: null, Perspective: null, Accidental: false, Indirect: false, ShowUninvolved: false, ShowForInanimate: false, SilentIfNoDamage: true);
	}

	public override void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		ApplyAcid(Liquid, Target, By, FromCell);
		base.SmearOn(Liquid, Target, By, FromCell);
	}

	public override void SmearOnTick(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
		ApplyAcid(Liquid, Target, By, FromCell);
		base.SmearOnTick(Liquid, Target, By, FromCell);
	}

	public override int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		if (Smart && GO != null)
		{
			Uncacheable = true;
			int num = GO.Stat("AcidResistance");
			if (num > 0)
			{
				List<GameObject> list = Event.NewGameObjectList();
				int num2;
				if (Liquid.IsSwimmingDepth())
				{
					foreach (GameObject content in GO.GetContents())
					{
						if (!content.IsNatural())
						{
							list.Add(content);
						}
					}
					num2 = 30;
				}
				else if (Liquid.IsWadingDepth())
				{
					GO.Body?.GetEquippedObjectsExceptNatural(list);
					num2 = 5;
				}
				else
				{
					GO.Body?.GetEquippedObjectsExceptNatural(list);
					num2 = 1;
				}
				num2 = Math.Min(num2 + list.Count * 2, 95);
				if (num >= 100)
				{
					return num2;
				}
				return Math.Min(Math.Max((65 + num2) * (100 - num) / 100, num2), 99);
			}
		}
		return 99;
	}
}
