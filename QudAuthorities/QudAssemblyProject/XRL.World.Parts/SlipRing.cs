using XRL.World.Capabilities;

namespace XRL.World.Parts;

public class SlipRing : IPoweredPart
{
	public const string SAVE_BONUS_VS = "Grab";

	public int SaveBonus = 15;

	public int ActivationChance = 5;

	public SlipRing()
	{
		IsEMPSensitive = false;
		IsPowerLoadSensitive = true;
		WorksOnWearer = true;
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
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational();
		}
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 10);
		}
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (ChargeUse > 0 && !base.OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 100);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "DefenderBeforeHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "DefenderBeforeHit");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Grab", E))
		{
			int num = MyPowerLoadLevel();
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				E.Roll += GetSaveBonus(num);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int powerLoad = MyPowerLoadLevel();
		SavingThrows.AppendSaveBonusDescription(E.Postfix, GetSaveBonus(powerLoad), "Grab", HighlightNumber: false, Highlight: true);
		E.Postfix.AppendRules("+" + GetActivationChance(powerLoad) + "% chance to slip away from natural melee attacks");
		return base.HandleEvent(E);
	}

	public int GetSaveBonus(int PowerLoad = 100)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return SaveBonus;
		}
		return SaveBonus * (100 + num) / 100;
	}

	public int GetActivationChance(int PowerLoad = 100)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return ActivationChance;
		}
		return ActivationChance * (100 + num) / 100;
	}

	public bool IsActiveFor(GameObject Weapon)
	{
		if (Weapon == null || !Weapon.IsNatural())
		{
			return false;
		}
		int num = MyPowerLoadLevel();
		if (GetActivationChance(num).in100())
		{
			return IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderBeforeHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (IsActiveFor(gameObjectParameter))
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Defender");
				if (gameObjectParameter2 != null && gameObjectParameter3 != null)
				{
					IComponent<GameObject>.XDidYToZ(gameObjectParameter3, "slip", "away from", gameObjectParameter2, gameObjectParameter.ShortDisplayName, "!", null, gameObjectParameter3, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: true);
					E.SetParameter("NoMissMessage", 1);
				}
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
