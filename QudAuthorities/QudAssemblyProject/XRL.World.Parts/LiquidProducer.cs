using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LiquidProducer : IPoweredPart
{
	public int Tick;

	public int Rate = 1000;

	public int ChanceSkipSelf;

	public int ChanceSkipSameCell;

	public string Liquid = "water";

	public string LiquidConsumed;

	public string VariableRate;

	public bool PreferCollectors;

	public bool PureOnFloor;

	public bool ConsumePure;

	public int ConsumeAmount = 1;

	public bool FillSelfOnly;

	[NonSerialized]
	private static LiquidVolume produced = new LiquidVolume();

	public LiquidProducer()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		LiquidProducer liquidProducer = p as LiquidProducer;
		if (liquidProducer.Rate != Rate)
		{
			return false;
		}
		if (liquidProducer.ChanceSkipSelf != ChanceSkipSelf)
		{
			return false;
		}
		if (liquidProducer.ChanceSkipSameCell != ChanceSkipSameCell)
		{
			return false;
		}
		if (liquidProducer.Liquid != Liquid)
		{
			return false;
		}
		if (liquidProducer.LiquidConsumed != LiquidConsumed)
		{
			return false;
		}
		if (liquidProducer.VariableRate != VariableRate)
		{
			return false;
		}
		if (liquidProducer.PreferCollectors != PreferCollectors)
		{
			return false;
		}
		if (liquidProducer.PureOnFloor != PureOnFloor)
		{
			return false;
		}
		if (liquidProducer.ConsumePure != ConsumePure)
		{
			return false;
		}
		if (liquidProducer.ConsumeAmount != ConsumeAmount)
		{
			return false;
		}
		if (liquidProducer.FillSelfOnly != FillSelfOnly)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != ObjectCreatedEvent.ID && ID != ProducesLiquidEvent.ID)
		{
			return ID == WantsLiquidCollectionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (WantsLiquidCollection(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ProducesLiquidEvent E)
	{
		if (E.Liquid == Liquid)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!string.IsNullOrEmpty(VariableRate))
		{
			Rate = VariableRate.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(LiquidConsumed))
		{
			return !LiquidAvailable(LiquidConsumed, 1, !ConsumePure);
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override bool WantTenTurnTick()
	{
		return true;
	}

	public override bool WantHundredTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (IsNeeded() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && ++Tick >= Rate)
		{
			Tick = 0;
			if (string.IsNullOrEmpty(LiquidConsumed) || ConsumeLiquid(LiquidConsumed, ConsumeAmount, !ConsumePure))
			{
				DistributeLiquid();
			}
			if (!string.IsNullOrEmpty(VariableRate))
			{
				Rate = VariableRate.RollCached();
			}
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (!IsNeeded() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Tick += 10;
		while (Tick >= Rate)
		{
			Tick -= Rate;
			if (string.IsNullOrEmpty(LiquidConsumed) || ConsumeLiquid(LiquidConsumed, ConsumeAmount, !ConsumePure))
			{
				DistributeLiquid();
			}
			if (!string.IsNullOrEmpty(VariableRate))
			{
				Rate = VariableRate.RollCached();
			}
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (!IsNeeded() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Tick += 100;
		while (Tick >= Rate)
		{
			Tick -= Rate;
			if (string.IsNullOrEmpty(LiquidConsumed) || ConsumeLiquid(LiquidConsumed, ConsumeAmount, !ConsumePure))
			{
				DistributeLiquid();
			}
			if (!string.IsNullOrEmpty(VariableRate))
			{
				Rate = VariableRate.RollCached();
			}
		}
	}

	public bool IsNeeded()
	{
		if (!FillSelfOnly)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && !cell.OnWorldMap())
			{
				return true;
			}
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && liquidVolume.Volume >= liquidVolume.MaxVolume)
		{
			if (liquidVolume.IsPureLiquid(Liquid))
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
				foreach (string item in LiquidType.CachedCommaExpansion())
				{
					if (item.Split('-')[0] == LiquidConsumed)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool WantsLiquidCollection(string LiquidType)
	{
		if (LiquidConsumed != null)
		{
			if (LiquidConsumed == LiquidType)
			{
				return true;
			}
			if (!ConsumePure && LiquidType.IndexOf(',') != -1 && LiquidType.IndexOf(LiquidConsumed) != -1)
			{
				foreach (string item in LiquidType.CachedCommaExpansion())
				{
					if (item.Split('-')[0] == LiquidConsumed)
					{
						return true;
					}
				}
			}
		}
		return false;
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
		produced.MaxVolume = 1;
		produced.Volume = 1;
		LiquidVolume V;
		if (FillSelfOnly || !ChanceSkipSelf.in100())
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
		Cell C = ParentObject.GetCurrentCell();
		if (C == null || C.ParentZone.IsWorldMap())
		{
			return false;
		}
		bool result;
		if (!ChanceSkipSameCell.in100() && !C.IsSolid(ForFluid: true))
		{
			result = C.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
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
			result = C.ForeachObjectWithPart("LiquidVolume", delegate(GameObject GO)
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
			GameObject gameObject = GameObject.create("Water");
			V = gameObject.LiquidVolume;
			V.Volume = produced.Volume;
			if (PureOnFloor || string.IsNullOrEmpty(C.GroundLiquid))
			{
				V.InitialLiquid = Liquid + "-1000";
			}
			else
			{
				V.InitialLiquid = C.GroundLiquid;
				V.MixWith(produced);
			}
			C.AddObject(gameObject);
			return true;
		}
		int num = 0;
		result = C.ForeachLocalAdjacentCell(delegate(Cell AC)
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
			result = C.ForeachLocalAdjacentCell(delegate(Cell AC)
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
					GameObject gameObject2 = GameObject.create("Water");
					V = gameObject2.LiquidVolume;
					V.Volume = produced.Volume;
					if (PureOnFloor || string.IsNullOrEmpty(C.GroundLiquid))
					{
						V.InitialLiquid = Liquid + "-1000";
					}
					else
					{
						V.InitialLiquid = C.GroundLiquid;
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
}
