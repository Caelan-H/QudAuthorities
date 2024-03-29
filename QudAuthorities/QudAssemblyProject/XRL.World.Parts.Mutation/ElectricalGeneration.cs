using System;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ElectricalGeneration : BaseMutation
{
	public const int PER_TURN_PER_LEVEL_BASE = 100;

	public const int BASE_MAX = 2000;

	public const int PER_LEVEL_MAX = 2000;

	public const int DISCHARGE_CHUNK = 1000;

	public const int DAMAGE_ABSORB_FACTOR = 100;

	public const int WILLPOWER_BASELINE = 16;

	public const int WILLPOWER_FACTOR = 5;

	public const int WILLPOWER_CEILING_FACTOR = 5;

	public const int WILLPOWER_FLOOR_DIVISOR = 5;

	public int Charge;

	public int AdvancedCharge;

	public int BaseChargePerTurnPercent = 100;

	public bool ConsiderLive = true;

	public bool CanDrinkTransient;

	public Guid DischargeActivatedAbilityID = Guid.Empty;

	public Guid ProvideChargeActivatedAbilityID = Guid.Empty;

	public ElectricalGeneration()
	{
		DisplayName = "Electrical Generation";
	}

	public static int GetBaseChargePerTurn(int Level, int Percent)
	{
		return Level * 100 * Percent / 100;
	}

	public int GetBaseChargePerTurn()
	{
		return GetBaseChargePerTurn(base.Level, BaseChargePerTurnPercent);
	}

	public static int GetChargePerTurn(int Level, int Willpower, int Percent)
	{
		int baseChargePerTurn = GetBaseChargePerTurn(Level, Percent);
		int num = baseChargePerTurn;
		int num2 = (Willpower - 16) * 5;
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return Math.Max(Math.Min(num, baseChargePerTurn * 5), baseChargePerTurn / 5);
	}

	public int GetChargePerTurn(int Level)
	{
		return GetChargePerTurn(Level, ParentObject.Stat("Willpower"), BaseChargePerTurnPercent);
	}

	public int GetChargePerTurn()
	{
		return GetChargePerTurn(base.Level);
	}

	public static int GetMaxCharge(int Level)
	{
		return 2000 + Level * 2000;
	}

	public int GetMaxCharge()
	{
		return GetMaxCharge(base.Level);
	}

	public static string GetDischargeDamageRoll(int Charge)
	{
		if (Charge < 1000)
		{
			return null;
		}
		return Charge / 1000 + "d4";
	}

	public string GetDischargeDamageRoll()
	{
		return GetDischargeDamageRoll(Charge);
	}

	public static int GetDischargeVoltage(int Charge)
	{
		return Charge / 1000;
	}

	public int GetDischargeVoltage()
	{
		return GetDischargeVoltage(Charge);
	}

	public override string GetDescription()
	{
		return "You accrue electrical charge that you can use and discharge to deal damage.";
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Maximum charge: {{C|").Append(GetMaxCharge(Level)).Append("}}");
		stringBuilder.Append("\nAccrue base {{C|").Append(GetBaseChargePerTurn(Level, BaseChargePerTurnPercent)).Append("}} charge per turn");
		stringBuilder.Append("\nCan discharge all held charge for 1d4 damage per ").Append(1000).Append(" charge");
		stringBuilder.Append("\nDischarge can arc to adjacent targets dealing reduced damage, up to 1 target per ").Append(1000).Append(" charge");
		stringBuilder.Append("\nEMP causes involuntary discharge (difficulty 18 Willpower save)");
		if (CanDrinkTransient)
		{
			stringBuilder.Append("\nYou can drink change from electrical power sources.");
		}
		else
		{
			stringBuilder.Append("\nYou can drink charge from energy cells and capacitors.");
		}
		stringBuilder.Append("\nYou gain ").Append(100).Append(" charge per point of electrical damage taken.");
		stringBuilder.Append("\nYou can provide charge to equipped devices that have integrated power systems.");
		return stringBuilder.ToString();
	}

	public bool PlayerCanSeeChargeAmounts()
	{
		if (ParentObject.IsPlayer())
		{
			return true;
		}
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return false;
		}
		if (IComponent<GameObject>.ThePlayer.HasPart("ElectricalGeneration"))
		{
			return true;
		}
		if (Scanning.HasScanningFor(IComponent<GameObject>.ThePlayer, ParentObject))
		{
			return true;
		}
		return false;
	}

	public void Discharge(Cell C, bool Accidental = false)
	{
		int charge = Charge;
		string dischargeDamageRoll = GetDischargeDamageRoll();
		if (dischargeDamageRoll != null)
		{
			int dischargeVoltage = GetDischargeVoltage();
			Charge = 0;
			SyncDischargeAbility();
			if (PlayerCanSeeChargeAmounts())
			{
				DidX("discharge", charge + " units of electrical charge", "!");
			}
			else
			{
				DidX("discharge", "an electrical arc", "!");
			}
			ParentObject.Discharge(C, dischargeVoltage, dischargeDamageRoll, ParentObject, null, 0, Accidental);
		}
	}

	public void SyncDischargeAbility()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Discharge [").Append(Charge).Append(" charge]");
		SetMyActivatedAbilityDisplayName(DischargeActivatedAbilityID, stringBuilder.ToString());
		if (Charge < 1000)
		{
			DisableMyActivatedAbility(DischargeActivatedAbilityID);
		}
		else
		{
			EnableMyActivatedAbility(DischargeActivatedAbilityID);
		}
	}

	public int GetCharge()
	{
		return Charge + GetChargePerTurn() - AdvancedCharge;
	}

	public int AddCharge(int Amount)
	{
		int num = Math.Min(Charge + Amount, GetMaxCharge());
		if (num != Charge)
		{
			int result = num - Charge;
			Charge = num;
			SyncDischargeAbility();
			return result;
		}
		return 0;
	}

	public int UseCharge(int Amount)
	{
		if (Amount <= 0)
		{
			return 0;
		}
		int num = Math.Min(GetChargePerTurn() - AdvancedCharge, Amount);
		int result = 0;
		if (num > 0)
		{
			AdvancedCharge += num;
			Amount -= num;
		}
		if (Amount > 0)
		{
			Charge -= Amount;
			result = Amount;
			if (Amount < 0)
			{
				Amount = 0;
			}
		}
		SyncDischargeAbility();
		return result;
	}

	public bool IsDrinkable(GameObject obj)
	{
		if (obj != ParentObject)
		{
			return obj.TestCharge(1, LiveOnly: true, 0L, CanDrinkTransient, IncludeBiological: false);
		}
		return false;
	}

	public virtual bool InteractWithChargeEvent(IChargeEvent E)
	{
		if ((ConsiderLive || !E.LiveOnly) && E.IncludeBiological)
		{
			return IsMyActivatedAbilityVoluntarilyUsable(ProvideChargeActivatedAbilityID);
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != InventoryActionEvent.ID && ID != OwnerGetInventoryActionsEvent.ID && ID != OwnerGetShortDescriptionEvent.ID && ID != QueryChargeEvent.ID && ID != TestChargeEvent.ID && ID != UseChargeEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if (InteractWithChargeEvent(E))
		{
			int charge = GetCharge();
			if (charge > 0)
			{
				E.Amount += charge;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if (InteractWithChargeEvent(E))
		{
			int num = Math.Min(E.Amount, GetCharge());
			if (num > 0)
			{
				E.Amount -= num;
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
		if (E.Pass == 1 && InteractWithChargeEvent(E))
		{
			int num = Math.Min(E.Amount, GetCharge());
			if (num > 0)
			{
				E.Amount -= num;
				UseCharge(num);
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (IsDrinkable(E.Object))
		{
			E.AddAction("Drink Charge", "drink charge", "DrinkCharge", null, 'k', FireOnActor: true, -1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DrinkCharge")
		{
			E.Item.SplitFromStack();
			if (E.Item.IsHostileTowards(ParentObject) && Stat.Random(1, 20) + ParentObject.StatMod("Agility") < Stats.GetCombatDV(E.Item))
			{
				DidXToY("try", "to touch", E.Item, ", but " + E.Item.it + E.Item.GetVerb("evade", PrependSpace: true, PronounAntecedent: true) + " " + ParentObject.them, null, null, null, ParentObject, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			}
			else
			{
				int num = E.Item.QueryCharge(LiveOnly: true, 0L, CanDrinkTransient, IncludeBiological: false);
				if (num > 0 && E.Item.UseCharge(num, LiveOnly: true, 0L, CanDrinkTransient, IncludeBiological: false))
				{
					int num2 = AddCharge(num);
					if (PlayerCanSeeChargeAmounts())
					{
						if (num2 > 0)
						{
							DidXToY("drink", "the juice from", E.Item, "and recharge " + num2 + " " + ((num2 == 1) ? "unit" : "units") + " of electrical charge", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						}
						else
						{
							DidXToY("drink", "the juice from", E.Item, "but don't seem to retain any of it", null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
						}
					}
					else if (E.Item.CurrentCell != null)
					{
						DidXToY("touch", E.Item);
					}
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("You can't seem to drink any of the juice from " + E.Item.the + E.Item.ShortDisplayName + ".");
				}
				else if (E.Item.CurrentCell != null)
				{
					DidXToY("touch", E.Item, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				}
			}
			E.Actor.UseEnergy(1000, "Physical Mutation ElectricalGeneration Drink");
			E.RequestInterfaceExit();
			E.Item.CheckStack();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		AddCharge(GetChargePerTurn() - AdvancedCharge);
		AdvancedCharge = 0;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetShortDescriptionEvent E)
	{
		if (E.Object.GetPart("IntegratedPowerSystems") is IntegratedPowerSystems integratedPowerSystems && integratedPowerSystems.RequiresEvent == "HasPowerConnectors")
		{
			E.Postfix.AppendRules("Integrated power systems: When equipped, you can power " + ((!E.Object.UseBareIndicative) ? E.Object.them : (E.Object.IsPlural ? "these devices" : "this device")) + " via Electrical Generation.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Damage.IsElectricDamage())
		{
			int num = AddCharge(E.Damage.Amount * 100);
			if (num > 0)
			{
				if (PlayerCanSeeChargeAmounts())
				{
					DidX("recharge", num + " " + ((num == 1) ? "unit" : "units") + " of electrical charge from the damage");
				}
				else
				{
					DidX("seem", "to have absorbed some of the electrical charge");
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "ApplyEMP");
		Object.RegisterPartEvent(this, "CommandDischarge");
		Object.RegisterPartEvent(this, "CommandToggleProvideCharge");
		Object.RegisterPartEvent(this, "HasPowerConnectors");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (Charge > 1000 && E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(DischargeActivatedAbilityID))
			{
				E.AddAICommand("CommandDischarge");
			}
		}
		else if (E.ID == "HasPowerConnectors")
		{
			if (IsMyActivatedAbilityVoluntarilyUsable(ProvideChargeActivatedAbilityID))
			{
				return false;
			}
		}
		else if (E.ID == "ApplyEMP")
		{
			if (Charge >= 1000 && IsMyActivatedAbilityUsable(DischargeActivatedAbilityID) && !ParentObject.MakeSave("Willpower", 18, null, null, "EMP ForcedDischarge", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				Discharge(ParentObject.CurrentCell.GetAdjacentCells().GetRandomElement(), Accidental: true);
			}
		}
		else if (E.ID == "CommandDischarge")
		{
			if (Charge < 1000)
			{
				return false;
			}
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Electrical Generation Discharge");
			Discharge(cell);
		}
		else if (E.ID == "CommandToggleProvideCharge")
		{
			ToggleMyActivatedAbility(ProvideChargeActivatedAbilityID);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		DischargeActivatedAbilityID = AddMyActivatedAbility("Discharge", "CommandDischarge", "Physical Mutation", null, "û", "You need at least " + 1000 + " charge to generate a discharge.");
		ProvideChargeActivatedAbilityID = AddMyActivatedAbility("Power Devices", "CommandToggleProvideCharge", "Physical Mutation", null, "ñ", null, Toggleable: true, DefaultToggleState: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref DischargeActivatedAbilityID);
		RemoveMyActivatedAbility(ref ProvideChargeActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
