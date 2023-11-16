using System;
using System.Text;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class EnergyCell : IEnergyCell
{
	public int Charge = 100;

	public int MaxCharge = 100;

	public int ChargeRate = 10;

	public string ChargeDisplayStyle = "electrical";

	public string AltChargeDisplayStyle = "percentage";

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);

	public bool ConsiderLive = true;

	public override bool HasAnyCharge()
	{
		return Charge > 0;
	}

	public override bool HasCharge(int Amount)
	{
		return Charge >= Amount;
	}

	public override int GetCharge()
	{
		return Charge;
	}

	public override void UseCharge(int Amount)
	{
		Charge -= Amount;
		if (Charge < 0)
		{
			Charge = 0;
		}
		else if (Charge > MaxCharge)
		{
			Charge = MaxCharge;
		}
	}

	public override void AddCharge(int Amount)
	{
		Charge += Amount;
		if (Charge > MaxCharge)
		{
			Charge = MaxCharge;
		}
		else if (Charge < 0)
		{
			Charge = 0;
		}
		if (ParentObject.pPhysics.InInventory != null || ParentObject.pPhysics.CurrentCell != null)
		{
			ParentObject.CheckStack();
		}
	}

	public override void TinkerInitialize()
	{
		Charge = 0;
	}

	public override void RandomizeCharge()
	{
		Charge = Stat.Random(1, MaxCharge);
	}

	public override void MaximizeCharge()
	{
		Charge = MaxCharge;
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(Charge, MaxCharge);
	}

	public bool UseAltChargeDisplayStyle()
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(AltChargeDisplayProperty))
		{
			return false;
		}
		if (IComponent<GameObject>.ThePlayer.GetIntProperty(AltChargeDisplayProperty) <= 0)
		{
			return false;
		}
		return true;
	}

	public override string ChargeStatus()
	{
		string text = (UseAltChargeDisplayStyle() ? AltChargeDisplayStyle : ChargeDisplayStyle);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return null;
		}
		return EnergyStorage.GetChargeStatus(Charge, MaxCharge, text);
	}

	public override bool CanBeRecharged()
	{
		return true;
	}

	public override int GetRechargeAmount()
	{
		return MaxCharge - Charge;
	}

	public override bool SameAs(IPart p)
	{
		EnergyCell energyCell = p as EnergyCell;
		if (energyCell.MaxCharge != MaxCharge)
		{
			return false;
		}
		if (energyCell.SlotType != SlotType)
		{
			return false;
		}
		if (energyCell.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (energyCell.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (energyCell.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (energyCell.Charge != Charge)
		{
			return false;
		}
		if (energyCell.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetSlottedInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != QueryChargeEvent.ID && ID != QueryRechargeStorageEvent.ID && ID != RechargeAvailableEvent.ID && ID != TestChargeEvent.ID)
		{
			return ID == UseChargeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Charge", Charge);
		E.AddEntry(this, "MaxCharge", MaxCharge);
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
			int num2 = Math.Min(E.Amount, GetCharge() - num);
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

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Context != "Tinkering" && E.Understood())
		{
			string text = ChargeStatus();
			if (text != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{y|(").Append(text).Append(")}}");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CanBeRecharged() && IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Tinker1") && Charge < MaxCharge && !IComponent<GameObject>.ThePlayer.IsFrozen())
		{
			E.AddAction("Recharge", "recharge", "RechargeEnergyCell", null, 'R', FireOnActor: false, 100 - Charge * 100 / MaxCharge);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlottedInventoryActionsEvent E)
	{
		if (CanBeRecharged() && IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Tinker1") && Charge < MaxCharge && ParentObject.Understood() && !IComponent<GameObject>.ThePlayer.IsFrozen())
		{
			E.AddAction("RechargeSlotted", "recharge " + ParentObject.BaseDisplayNameStripped, "RechargeEnergyCell", null, 'R', FireOnActor: false, 90 - Charge * 90 / MaxCharge, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: true, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "RechargeEnergyCell" && CanBeRecharged())
		{
			E.Actor.GetPart<Tinkering_Tinker1>().Recharge(ParentObject, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RechargeAvailableEvent E)
	{
		if (E.Amount > 0 && Charge < MaxCharge && IsReady(UseCharge: false, IgnoreCharge: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = E.Amount;
			if (!E.Forced && ChargeRate >= 0)
			{
				int num2 = ChargeRate * E.Multiple;
				if (num > num2)
				{
					num = num2;
				}
			}
			if (num > MaxCharge - Charge)
			{
				num = MaxCharge - Charge;
			}
			if (num > 0)
			{
				AddCharge(num);
				E.Amount -= num;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryRechargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = MaxCharge - GetCharge();
			if (num > 0)
			{
				E.Amount += num;
			}
		}
		return base.HandleEvent(E);
	}
}
