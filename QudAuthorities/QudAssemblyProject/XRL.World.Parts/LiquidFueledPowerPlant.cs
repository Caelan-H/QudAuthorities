using System;
using System.Collections.Generic;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class LiquidFueledPowerPlant : IPoweredPart
{
	public string Liquid;

	public string Liquids;

	public int ChargePerDram = 10000;

	public int ChargeCounter;

	public int ChargeRate;

	public bool ConsiderLive;

	[NonSerialized]
	public static Event eLiquidFueledPowerPlantFueled = new ImmutableEvent("LiquidFueledPowerPlantFueled");

	[NonSerialized]
	private Dictionary<string, int> _LiquidMap;

	public Dictionary<string, int> LiquidMap
	{
		get
		{
			if (_LiquidMap == null && !string.IsNullOrEmpty(Liquids))
			{
				_LiquidMap = Liquids.CachedNumericDictionaryExpansion();
			}
			return _LiquidMap;
		}
	}

	public LiquidFueledPowerPlant()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (Liquid == null)
		{
			if (LiquidMap != null && LiquidMap.Count > 0)
			{
				KeyValuePair<string, int> keyValuePair = LiquidMap.First();
				Liquid = keyValuePair.Key;
				ChargePerDram = keyValuePair.Value;
			}
			else
			{
				Liquid = "water";
			}
		}
	}

	public LiquidVolume EnergyLiquid()
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null)
		{
			return null;
		}
		if (!IsValidLiquid(liquidVolume))
		{
			return null;
		}
		if (liquidVolume.Volume <= 0)
		{
			return null;
		}
		return liquidVolume;
	}

	private bool IsValidLiquid(LiquidVolume Volume)
	{
		if (Volume.ComponentLiquids.Count != 1)
		{
			return false;
		}
		if (!Volume.ComponentLiquids.ContainsKey(Liquid))
		{
			string primaryLiquidID = Volume.GetPrimaryLiquidID();
			if (LiquidMap == null || !LiquidMap.TryGetValue(primaryLiquidID, out var value))
			{
				return false;
			}
			Liquid = primaryLiquidID;
			ChargePerDram = value;
		}
		return true;
	}

	public bool HasCharge(int Amount)
	{
		return GetCharge() >= Amount;
	}

	public int GetCharge()
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return 0;
		}
		return liquidVolume.Volume * ChargePerDram - ChargeCounter;
	}

	public LiquidFueledPowerPlant UseCharge(int Amount)
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume != null)
		{
			ChargeCounter += Amount;
			while (ChargeCounter >= ChargePerDram)
			{
				liquidVolume.UseDram();
				if (liquidVolume.Volume == 0)
				{
					ChargeCounter = 0;
					break;
				}
				ChargeCounter -= ChargePerDram;
			}
		}
		return this;
	}

	public LiquidFueledPowerPlant AddCharge(int Amount)
	{
		if (Amount < 0)
		{
			UseCharge(-Amount);
		}
		return this;
	}

	public string ChargeStatus()
	{
		LiquidVolume liquidVolume = EnergyLiquid();
		if (liquidVolume == null)
		{
			return "{{K|-}}";
		}
		string primaryLiquidColor = liquidVolume.GetPrimaryLiquidColor();
		if (primaryLiquidColor != null)
		{
			return "{{" + primaryLiquidColor + "|" + liquidVolume.Volume + "}}";
		}
		return liquidVolume.Volume.ToString();
	}

	public override bool SameAs(IPart p)
	{
		LiquidFueledPowerPlant liquidFueledPowerPlant = p as LiquidFueledPowerPlant;
		if (liquidFueledPowerPlant.Liquid != Liquid)
		{
			return false;
		}
		if (liquidFueledPowerPlant.ChargePerDram != ChargePerDram)
		{
			return false;
		}
		if (liquidFueledPowerPlant.ChargeCounter != ChargeCounter)
		{
			return false;
		}
		if (liquidFueledPowerPlant.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (liquidFueledPowerPlant.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetPreferredLiquidEvent.ID && ID != LiquidMixedEvent.ID && ID != QueryChargeEvent.ID && ID != QueryChargeProductionEvent.ID && ID != TestChargeEvent.ID && ID != UseChargeEvent.ID)
		{
			return ID == WantsLiquidCollectionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ChargePerDram", ChargePerDram);
		E.AddEntry(this, "ChargeCounter", ChargeCounter);
		E.AddEntry(this, "ChargeRate", ChargeRate);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LiquidMixedEvent E)
	{
		if (EnergyLiquid() != null)
		{
			ParentObject.FireEvent(eLiquidFueledPowerPlantFueled);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null && !string.IsNullOrEmpty(Liquid))
		{
			E.Liquid = Liquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = GetCharge() - num;
			if (num2 > 0)
			{
				E.Amount += num2;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = GetCharge() - num;
			if (num2 > 0)
			{
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		if (E.Pass == 2 && (!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = Math.Min(E.Amount, GetCharge() - num);
			if (num2 > 0)
			{
				UseCharge(num2 + num);
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Math.Min(ChargeRate * E.Multiple, GetCharge()) - ChargeUse * E.Multiple;
			if (num > 0)
			{
				E.Amount += num;
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		return Liquid == LiquidType;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Math.Min(ChargeRate, GetCharge()) - ChargeUse;
			if (num > 0)
			{
				UseCharge(ParentObject.ChargeAvailable(num, 0L));
			}
		}
	}
}
