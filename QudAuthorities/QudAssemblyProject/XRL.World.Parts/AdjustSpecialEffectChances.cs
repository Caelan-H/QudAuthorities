using System;

namespace XRL.World.Parts;

[Serializable]
public class AdjustSpecialEffectChances : IPoweredPart
{
	public int LinearAdjust;

	public float FactorAdjust = 1f;

	public string RequireType;

	public string ExcludeType;

	public bool UsesChargePerTurn;

	public bool UsesChargePerActivation;

	public AdjustSpecialEffectChances()
	{
		IsPowerLoadSensitive = true;
	}

	public override bool SameAs(IPart p)
	{
		AdjustSpecialEffectChances adjustSpecialEffectChances = p as AdjustSpecialEffectChances;
		if (adjustSpecialEffectChances.LinearAdjust != LinearAdjust)
		{
			return false;
		}
		if (adjustSpecialEffectChances.FactorAdjust != FactorAdjust)
		{
			return false;
		}
		if (adjustSpecialEffectChances.RequireType != RequireType)
		{
			return false;
		}
		if (adjustSpecialEffectChances.ExcludeType != ExcludeType)
		{
			return false;
		}
		if (adjustSpecialEffectChances.UsesChargePerTurn != UsesChargePerTurn)
		{
			return false;
		}
		if (adjustSpecialEffectChances.UsesChargePerActivation != UsesChargePerActivation)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public new bool WantTurnTick()
	{
		return UsesChargePerTurn;
	}

	public new bool WantTenTurnTick()
	{
		return UsesChargePerTurn;
	}

	public new bool WantHundredTurnTick()
	{
		return UsesChargePerTurn;
	}

	public new void TurnTick(long TurnNumber)
	{
		if (UsesChargePerTurn && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetSpecialEffectChanceEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSpecialEffectChanceEvent E)
	{
		if ((IsObjectActivePartSubject(E.Actor) || IsObjectActivePartSubject(E.Object)) && (RequireType == null || E.Type.Contains(RequireType)) && (ExcludeType == null || !E.Type.Contains(ExcludeType)))
		{
			int num = MyPowerLoadLevel();
			if (IsReady(UsesChargePerActivation && E.BaseChance < 100, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				int num2 = LinearAdjust;
				float num3 = FactorAdjust;
				int num4 = IComponent<GameObject>.PowerLoadBonus(num, 100, 10);
				if (num4 != 0)
				{
					if (num2 != 0)
					{
						num2 = num2 * (100 + num4) / 100;
					}
					if (num3 != 1f)
					{
						num3 = 1f + (num3 - 1f) * (float)(100 + num4) / 100f;
					}
				}
				if (num2 != 0)
				{
					E.Chance += num2;
				}
				if (num3 != 1f)
				{
					E.Chance += (int)Math.Round((float)E.BaseChance * (num3 - 1f), MidpointRounding.AwayFromZero);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
