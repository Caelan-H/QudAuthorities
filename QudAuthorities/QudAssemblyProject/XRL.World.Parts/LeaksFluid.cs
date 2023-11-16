using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LeaksFluid : IPoweredPart
{
	public int Tick;

	public int Rate = -1;

	public int ChanceSmearParent = 100;

	public string SmearParentDuration = "100-200";

	public int ChanceSkipSelf;

	public int ChanceSkipSameCell;

	public string Liquid = "water";

	public string LiquidConsumed;

	public string Frequency = "300-400";

	public string Drams = "2-3";

	public bool PreferCollectors;

	public bool PureOnFloor;

	public bool ConsumePure;

	public int ConsumeAmount;

	public bool FillSelfOnly;

	public bool Message = true;

	public string MessageVerb = "leak";

	[NonSerialized]
	private static LiquidVolume produced = new LiquidVolume();

	public LeaksFluid()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		LeaksFluid leaksFluid = p as LeaksFluid;
		if (leaksFluid.Rate != Rate)
		{
			return false;
		}
		if (leaksFluid.ChanceSkipSelf != ChanceSkipSelf)
		{
			return false;
		}
		if (leaksFluid.ChanceSkipSameCell != ChanceSkipSameCell)
		{
			return false;
		}
		if (leaksFluid.Liquid != Liquid)
		{
			return false;
		}
		if (leaksFluid.LiquidConsumed != LiquidConsumed)
		{
			return false;
		}
		if (leaksFluid.Frequency != Frequency)
		{
			return false;
		}
		if (leaksFluid.PreferCollectors != PreferCollectors)
		{
			return false;
		}
		if (leaksFluid.PureOnFloor != PureOnFloor)
		{
			return false;
		}
		if (leaksFluid.ConsumePure != ConsumePure)
		{
			return false;
		}
		if (leaksFluid.ConsumeAmount != ConsumeAmount)
		{
			return false;
		}
		if (leaksFluid.FillSelfOnly != FillSelfOnly)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AllowLiquidCollection");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	public bool DistributeLiquid()
	{
		if (!produced.ComponentLiquids.ContainsKey(Liquid) || produced.ComponentLiquids.Count > 1)
		{
			if (produced.ComponentLiquids.Count > 0)
			{
				produced.ComponentLiquids.Clear();
			}
			produced.ComponentLiquids.Add(Liquid, 1000);
		}
		produced.MaxVolume = Stat.Roll(Drams);
		produced.Volume = produced.MaxVolume;
		LiquidVolume V;
		if (FillSelfOnly || (ChanceSkipSelf < 100 && (ChanceSkipSelf <= 0 || Stat.Random(1, 100) > ChanceSkipSelf)))
		{
			V = ParentObject.LiquidVolume;
			if (V != null && V.Volume < V.MaxVolume)
			{
				V.MixWith(produced);
				return true;
			}
		}
		if (FillSelfOnly)
		{
			return false;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.ParentZone.IsWorldMap())
		{
			return false;
		}
		string text = ((produced.Volume == 1) ? "dram" : "drams");
		if (Message && ParentObject.IsVisible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb(MessageVerb) + " " + produced.Volume + " " + text + " of " + produced.GetLiquidName() + ".");
		}
		if (Stat.Random(1, 100) <= ChanceSmearParent)
		{
			ParentObject.ApplyEffect(new LiquidCovered(produced, 1, Stat.Roll(SmearParentDuration), Poured: true));
		}
		bool result;
		if (ChanceSkipSameCell < 100 && !cell.IsSolid(ForFluid: true) && (ChanceSkipSameCell <= 0 || Stat.Random(1, 100) > ChanceSkipSameCell))
		{
			result = cell.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
			{
				V = GO.LiquidVolume;
				if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
				{
					V.MixWith(produced);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
			result = cell.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
			{
				V = GO.LiquidVolume;
				if (V.MaxVolume == -1)
				{
					V.MixWith(produced);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("Water");
			V = gameObject.LiquidVolume;
			V.Volume = produced.Volume;
			if (PureOnFloor)
			{
				V.InitialLiquid = Liquid + "-1000";
			}
			else
			{
				V.InitialLiquid = "salt-1000";
				V.MixWith(produced);
			}
			cell.AddObject(gameObject);
			return true;
		}
		int num = 0;
		result = cell.ForeachLocalAdjacentCell(delegate(Cell AC)
		{
			if (PreferCollectors)
			{
				result = AC.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
				{
					V = GO.LiquidVolume;
					if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
					{
						V.MixWith(produced);
						return false;
					}
					return true;
				});
				if (!result)
				{
					return false;
				}
			}
			if (!AC.IsSolid(ForFluid: true))
			{
				num++;
			}
			return true;
		});
		if (!result)
		{
			return true;
		}
		if (num > 0)
		{
			int select = Stat.Random(1, num);
			int pos = 0;
			result = cell.ForeachLocalAdjacentCell(delegate(Cell AC)
			{
				if (++pos == select)
				{
					result = AC.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
					{
						V = GO.LiquidVolume;
						if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
						{
							V.MixWith(produced);
							return false;
						}
						return true;
					});
					if (!result)
					{
						return false;
					}
					result = AC.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
					{
						V = GO.LiquidVolume;
						if (V.MaxVolume == -1)
						{
							V.MixWith(produced);
							return false;
						}
						return true;
					});
					if (!result)
					{
						return false;
					}
					GameObject gameObject2 = GameObjectFactory.Factory.CreateObject("Water");
					V = gameObject2.LiquidVolume;
					V.Volume = produced.Volume;
					if (PureOnFloor)
					{
						V.InitialLiquid = Liquid + "-1000";
					}
					else
					{
						V.InitialLiquid = "salt-1000";
						V.MixWith(produced);
					}
					AC.AddObject(gameObject2);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
		}
		return false;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(LiquidConsumed))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume == null || liquidVolume.Volume <= 0)
			{
				return true;
			}
			if (!liquidVolume.ComponentLiquids.ContainsKey(LiquidConsumed))
			{
				return true;
			}
			if (ConsumePure && liquidVolume.ComponentLiquids.Count != 1)
			{
				return true;
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	public bool IsNeeded()
	{
		if (!FillSelfOnly)
		{
			return true;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && liquidVolume.Volume >= liquidVolume.MaxVolume)
		{
			if (liquidVolume.ComponentLiquids.Count == 1 && liquidVolume.ComponentLiquids.ContainsKey(Liquid))
			{
				return false;
			}
			if (string.IsNullOrEmpty(LiquidConsumed))
			{
				return false;
			}
			if (!liquidVolume.ComponentLiquids.ContainsKey(LiquidConsumed))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (Liquid == LiquidType)
		{
			return true;
		}
		if (LiquidConsumed != null)
		{
			if (LiquidConsumed == LiquidType)
			{
				return true;
			}
			if (!ConsumePure && LiquidType.IndexOf(',') != -1 && LiquidType.IndexOf(LiquidConsumed) != -1)
			{
				string[] array = LiquidType.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Split('-')[0] == LiquidConsumed)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (IsNeeded() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ++Tick >= Rate)
			{
				if (string.IsNullOrEmpty(LiquidConsumed) || ConsumeLiquid(LiquidConsumed, ConsumeAmount))
				{
					DistributeLiquid();
				}
				Tick = 0;
				if (!string.IsNullOrEmpty(Frequency))
				{
					Rate = Stat.Roll(Frequency);
				}
			}
		}
		else if (E.ID == "AllowLiquidCollection")
		{
			if (!IsLiquidCollectionCompatible(E.GetStringParameter("Liquid")))
			{
				return false;
			}
		}
		else if (E.ID == "ObjectCreated" && !string.IsNullOrEmpty(Frequency))
		{
			Rate = Stat.Roll(Frequency);
		}
		return base.FireEvent(E);
	}
}
