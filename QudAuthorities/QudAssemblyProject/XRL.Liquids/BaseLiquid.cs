using System;
using System.Collections.Generic;
using System.Text;
using XRL.Messages;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;

namespace XRL.Liquids;

[Serializable]
[HasWishCommand]
public class BaseLiquid
{
	public const int STANDARD_SAVE_CAP = 24;

	public static readonly List<string> PaintGroups = new List<string>();

	public string ID;

	public int FlameTemperature = 99999;

	public int VaporTemperature = 100;

	public int FreezeTemperature;

	public int BrittleTemperature = -100;

	public int Temperature = 25;

	public int ThermalConductivity = 50;

	public int Combustibility;

	public int Adsorbence = 100;

	public int Fluidity = 50;

	public int Evaporativity;

	public int Staining;

	public int Cleansing;

	public int ShallowPaintGroup = -1;

	public int DeepPaintGroup = -1;

	public bool StainOnlyWhenPure;

	public bool EnableCleaning;

	public bool InterruptAutowalk;

	public bool ConsiderDangerousToContact;

	public bool ConsiderDangerousToDrink;

	public bool Glows;

	public double Weight = 0.25;

	public string VaporObject;

	public string CirculatoryLossTerm = "leaking";

	[NonSerialized]
	public static List<string> DefaultColors = new List<string>(2) { "b", "B" };

	[NonSerialized]
	public string _ColoredCirculatoryLossTerm;

	public static readonly string[] Puddles = new string[4] { "Liquids/Water/puddle_1.png", "Liquids/Water/puddle_2.png", "Liquids/Water/puddle_3.png", "Liquids/Water/puddle_4.png" };

	public string Name => GetName();

	public int Viscosity => 100 - Fluidity;

	public string ColoredCirculatoryLossTerm
	{
		get
		{
			if (_ColoredCirculatoryLossTerm == null)
			{
				_ColoredCirculatoryLossTerm = "{{" + GetColor() + "|" + CirculatoryLossTerm + "}}";
			}
			return _ColoredCirculatoryLossTerm;
		}
	}

	public BaseLiquid(string ID)
	{
		this.ID = ID;
	}

	public virtual float GetValuePerDram()
	{
		return 0f;
	}

	public virtual string GetPreparedCookingIngredient()
	{
		return "";
	}

	public virtual string GetName()
	{
		return GetName(null);
	}

	public virtual string GetName(LiquidVolume Liquid)
	{
		return "liquid";
	}

	public virtual string GetAdjective(LiquidVolume Liquid)
	{
		return "liquidy";
	}

	public virtual string GetWaterRitualName()
	{
		return "liquid";
	}

	public virtual string GetSmearedAdjective(LiquidVolume Liquid)
	{
		return "damp";
	}

	public virtual string GetSmearedName(LiquidVolume Liquid)
	{
		return "damp";
	}

	public virtual string GetStainedName(LiquidVolume Liquid)
	{
		return GetName(Liquid);
	}

	public virtual bool SafeContainer(GameObject GO)
	{
		return true;
	}

	public virtual bool Froze(LiquidVolume Liquid)
	{
		return true;
	}

	public virtual bool Vaporized(LiquidVolume Liquid)
	{
		return true;
	}

	public virtual bool Vaporized(LiquidVolume Liquid, GameObject By)
	{
		if (!string.IsNullOrEmpty(VaporObject))
		{
			Cell currentCell = Liquid.ParentObject.GetCurrentCell();
			if (currentCell != null)
			{
				GameObject gameObject = GameObject.create(VaporObject);
				if (gameObject.GetPart("Gas") is Gas gas)
				{
					gas.Density = Liquid.Amount(ID) / 20;
					gas.Creator = By;
				}
				currentCell.AddObject(gameObject);
			}
		}
		return Vaporized(Liquid);
	}

	public virtual bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, ref bool ExitInterface)
	{
		return true;
	}

	public bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message)
	{
		bool ExitInterface = false;
		return Drank(Liquid, Volume, Target, Message, ref ExitInterface);
	}

	public bool Drank(LiquidVolume Liquid, int Volume, GameObject Target, StringBuilder Message, IEvent E)
	{
		bool ExitInterface = false;
		bool result = Drank(Liquid, Volume, Target, Message, ref ExitInterface);
		if (ExitInterface)
		{
			E.RequestInterfaceExit();
		}
		return result;
	}

	public virtual void SmearOnTick(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
	}

	public virtual void SmearOn(LiquidVolume Liquid, GameObject Target, GameObject By, bool FromCell)
	{
	}

	public virtual void FillingContainer(GameObject Container, LiquidVolume Liquid)
	{
		if (Temperature != 25)
		{
			Container.TemperatureChange(Liquid.MilliAmount(ID) / (20000 / (Temperature - 25)), null, Radiant: false, MinAmbient: false, MaxAmbient: false, 5, null, Temperature);
		}
	}

	public virtual void EndTurn(LiquidVolume Liquid, GameObject Container)
	{
		if (Temperature != 25 && !Liquid.IsOpenVolume())
		{
			Container.TemperatureChange(Liquid.MilliAmount(ID) / (20000 / (Temperature - 25)), null, Radiant: false, MinAmbient: false, MaxAmbient: false, 5, null, Temperature);
		}
	}

	public virtual void BeforeRender(LiquidVolume Liquid)
	{
	}

	public virtual void BeforeRenderSecondary(LiquidVolume Liquid)
	{
	}

	public virtual void MixingWith(LiquidVolume Liquid, LiquidVolume NewLiquid)
	{
	}

	public virtual void MixedWith(LiquidVolume Liquid, LiquidVolume NewLiquid, ref bool ExitInterface)
	{
	}

	public virtual void RenderBackgroundPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderBackgroundSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void BaseRenderPrimary(LiquidVolume Liquid)
	{
	}

	public virtual void BaseRenderSecondary(LiquidVolume Liquid)
	{
	}

	public virtual void RenderPrimary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderSecondary(LiquidVolume Liquid, RenderEvent eRender)
	{
	}

	public virtual void RenderSmearPrimary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
	}

	public virtual void RenderSmearSecondary(LiquidVolume Liquid, RenderEvent eRender, GameObject obj)
	{
	}

	public virtual void ObjectGoingProne(LiquidVolume Liquid, GameObject GO)
	{
	}

	public virtual void ObjectEnteredCell(LiquidVolume Liquid, ObjectEnteredCellEvent E)
	{
	}

	[Obsolete("Replaced by ObjectEnteredCell(LiquidVolume, ObjectEnteredCellEvent), retained for mod compatibility")]
	public virtual void ObjectEnteredCell(LiquidVolume Liquid, GameObject GO)
	{
	}

	public virtual bool EnteredCell(LiquidVolume Liquid, ref bool ExitInterface)
	{
		return true;
	}

	public virtual string SplashSound(LiquidVolume Liquid)
	{
		return "SplashStep1";
	}

	public virtual void ObjectInCell(LiquidVolume Liquid, GameObject GO)
	{
	}

	public virtual List<string> GetColors()
	{
		return DefaultColors;
	}

	public virtual string GetColor()
	{
		return "b";
	}

	public virtual int GetNavigationWeight(LiquidVolume Liquid, GameObject GO, bool Smart, bool Slimewalking, ref bool Uncacheable)
	{
		return 0;
	}

	public virtual int GetHealingLocationValue(LiquidVolume Liquid, GameObject Actor)
	{
		return 0;
	}

	public virtual void StainElements(LiquidVolume Liquid, GetItemElementsEvent E)
	{
	}

	public virtual string GetPaint(LiquidVolume Liquid)
	{
		if (!Liquid.IsWadingDepth())
		{
			return Liquid.ParentObject.GetTag("PaintedShallowLiquid", "shallow");
		}
		return Liquid.ParentObject.GetTag("PaintedLiquid", "deep");
	}

	public virtual string GetPaintAtlas(LiquidVolume Liquid)
	{
		if (!Liquid.IsWadingDepth())
		{
			return Liquid.ParentObject.GetTag("PaintedShallowLiquidAtlas", "Liquids/Water/");
		}
		return Liquid.ParentObject.GetTag("PaintedLiquidAtlas", "Liquids/Water/");
	}

	public virtual string GetPaintExtension(LiquidVolume Liquid)
	{
		return Liquid.ParentObject.GetTag("PaintedLiquidExtension", ".png");
	}

	public virtual int GetPaintGroup(LiquidVolume Liquid)
	{
		if (Liquid.IsWadingDepth())
		{
			if (DeepPaintGroup == -1)
			{
				return DeepPaintGroup = AllocatePaintGroup(Liquid);
			}
			return DeepPaintGroup;
		}
		if (ShallowPaintGroup == -1)
		{
			return ShallowPaintGroup = AllocatePaintGroup(Liquid);
		}
		return ShallowPaintGroup;
	}

	protected virtual int AllocatePaintGroup(LiquidVolume Liquid)
	{
		string item = GetPaintAtlas(Liquid) + GetPaint(Liquid);
		int num = PaintGroups.IndexOf(item);
		if (num == -1)
		{
			num = PaintGroups.Count;
			PaintGroups.Add(item);
		}
		return num;
	}

	public virtual string GetPuddle(LiquidVolume Liquid)
	{
		return Puddles.GetRandomElementCosmetic();
	}

	public static void AddPlayerMessage(string msg, string color = null, bool capitalize = true)
	{
		MessageQueue.AddPlayerMessage(msg, color, capitalize);
	}

	public static void AddPlayerMessage(string msg, char color, bool capitalize = true)
	{
		MessageQueue.AddPlayerMessage(msg, color, capitalize);
	}

	[WishCommand("tileliquids", null)]
	public static void TestLiquids()
	{
		Zone activeZone = The.ActiveZone;
		for (int i = 0; i < 80; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				activeZone.Map[i][j].Clear(null, Important: true, Combat: true, (GameObject x) => x.IsPlayer());
			}
		}
		Dictionary<string, BaseLiquid> liquids = LiquidVolume.Liquids;
		int num = activeZone.Width / liquids.Count;
		int num2 = activeZone.Width % liquids.Count;
		string text = "";
		foreach (KeyValuePair<string, BaseLiquid> liquid in liquids)
		{
			if (text != "")
			{
				text += "\n";
			}
			text += liquid.Key;
			Action<GameObject> beforeObjectCreated = delegate(GameObject x)
			{
				x.LiquidVolume.InitialLiquid = liquid.Key + "-1000";
			};
			for (int k = 5; k < 20; k++)
			{
				if (k != 12)
				{
					Cell cell = activeZone.Map[num2][k];
					GameObject gameObject = GameObjectFactory.Factory.CreateObject("SlimePuddle", beforeObjectCreated);
					cell.Objects.Add(gameObject);
					gameObject.pPhysics.EnterCell(cell);
					gameObject.LiquidVolume.CheckImage();
				}
			}
			foreach (Cell localAdjacentCell in activeZone.Map[num2][12].GetLocalAdjacentCells())
			{
				if (localAdjacentCell.Objects.Count == 0)
				{
					GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("SlimePuddle", beforeObjectCreated);
					localAdjacentCell.Objects.Add(gameObject2);
					gameObject2.pPhysics.EnterCell(localAdjacentCell);
					gameObject2.LiquidVolume.CheckImage();
				}
			}
			num2 += num;
			if (num2 >= 80)
			{
				break;
			}
		}
		MetricsManager.LogError(text);
	}
}
