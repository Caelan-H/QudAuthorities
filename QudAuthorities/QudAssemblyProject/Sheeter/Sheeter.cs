using System;
using System.Collections.Generic;
using UnityEngine;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace Sheeter;

public class Sheeter
{
	public static int CalculateAttackDamage(XRL.World.GameObject Attacker)
	{
		int num = 0;
		_ = Attacker.Blueprint == "Barkbiter";
		if (Attacker.pBrain == null)
		{
			return 0;
		}
		Attacker.pBrain.PerformReequip();
		Body part = Attacker.GetPart<Body>();
		if (part == null)
		{
			return 0;
		}
		XRL.World.GameObject PrimaryWeapon = null;
		bool PickedFromHand = false;
		part.ForeachPart(delegate(BodyPart pPart)
		{
			if ((pPart.Equipped != null || pPart.DefaultBehavior != null) && (pPart.Primary || PrimaryWeapon == null || (pPart.Type == "Hand" && !PickedFromHand)))
			{
				if (pPart.Equipped != null && pPart.Equipped.GetPart("MeleeWeapon") != null)
				{
					PrimaryWeapon = pPart.Equipped;
					if (pPart.Type == "Hand")
					{
						PickedFromHand = true;
					}
				}
				else if (pPart.DefaultBehavior != null && pPart.DefaultBehavior.GetPart("MeleeWeapon") != null)
				{
					PrimaryWeapon = pPart.DefaultBehavior;
					if (!pPart.DefaultBehavior.HasTag("UndesireableWeapon"))
					{
						PickedFromHand = true;
					}
				}
			}
		});
		if (PrimaryWeapon != null)
		{
			if (PrimaryWeapon.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
			{
				int num2 = Stat.RollMin(meleeWeapon.BaseDamage);
				int num3 = Stat.RollMax(meleeWeapon.BaseDamage);
				num += num2 + (num3 - num2) / 2;
			}
			if (PrimaryWeapon.GetPart("ElementalDamage") is ElementalDamage elementalDamage)
			{
				int num4 = Stat.RollMin(elementalDamage.Damage);
				int num5 = Stat.RollMax(elementalDamage.Damage);
				num += (num4 + (num5 - num4) / 2) * elementalDamage.Chance / 100;
			}
		}
		int SecondaryChance = GlobalConfig.GetIntSetting("BaseSecondaryAttackChance", 15);
		XRL.World.Event E = new XRL.World.Event("CommandAttack");
		if (Attacker.HasSkill("Dual_Wield_Offhand_Strikes"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("OffhandStrikesSecondaryAttackChance", 35);
		}
		if (Attacker.HasSkill("Dual_Wield_Ambidexterity"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("AmbidexteritySecondaryAttackChance", 55);
		}
		if (Attacker.HasSkill("Dual_Wield_Two_Weapon_Fighting"))
		{
			SecondaryChance = GlobalConfig.GetIntSetting("TwoWeaponFightingSecondaryAttackChance", 75);
		}
		if (Attacker.HasProperty("AlwaysOffhand"))
		{
			SecondaryChance = 100;
		}
		if (E.HasParameter("AlwaysOffhand"))
		{
			SecondaryChance = E.GetIntParameter("AlwaysOffhand");
		}
		List<XRL.World.GameObject> WeaponList = new List<XRL.World.GameObject>(8);
		part.ForeachPart(delegate(BodyPart pPart)
		{
			if (pPart.Equipped != null || pPart.DefaultBehavior != null)
			{
				XRL.World.GameObject gameObject = pPart.Equipped;
				if (gameObject == null || gameObject.GetPart<MeleeWeapon>() == null)
				{
					gameObject = pPart.DefaultBehavior;
				}
				if (!WeaponList.Contains(gameObject))
				{
					MeleeWeapon meleeWeapon3 = gameObject.GetPart("MeleeWeapon") as MeleeWeapon;
					if ((meleeWeapon3 != null && meleeWeapon3.Slot == null) || pPart.Type == null || meleeWeapon3.Slot.Contains(pPart.Type))
					{
						XRL.World.Event @event = XRL.World.Event.New("QueryWeaponSecondaryAttackChance");
						@event.AddParameter("Weapon", gameObject);
						@event.AddParameter("BodyPart", pPart);
						@event.AddParameter("Chance", SecondaryChance);
						@event.AddParameter("Properties", E.GetParameter("Properties"));
						gameObject.FireEvent(@event);
						if (Attacker != null)
						{
							@event.ID = "AttackerQueryWeaponSecondaryAttackChance";
							Attacker.FireEvent(@event);
							@event.ID = "AttackerQueryWeaponSecondaryAttackChanceMultiplier";
							Attacker.FireEvent(@event);
						}
						int num10 = @event.GetIntParameter("Chance");
						if (Attacker.HasTag("AttackWithEverything"))
						{
							num10 = 100;
						}
						if (E.HasParameter("AlwaysOffhand"))
						{
							num10 = E.GetIntParameter("AlwaysOffhand");
						}
						while (num10 > 0)
						{
							if (Stat.Random(1, 100) <= num10)
							{
								WeaponList.Add(gameObject);
							}
							num10 -= 100;
						}
					}
				}
			}
		});
		foreach (XRL.World.GameObject item in WeaponList)
		{
			if (item.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon2)
			{
				int num6 = Stat.RollMin(meleeWeapon2.BaseDamage);
				int num7 = Stat.RollMax(meleeWeapon2.BaseDamage);
				num += num6 + (num7 - num6) / 2;
			}
			if (item.GetPart("ElementalDamage") is ElementalDamage elementalDamage2)
			{
				int num8 = Stat.RollMin(elementalDamage2.Damage);
				int num9 = Stat.RollMax(elementalDamage2.Damage);
				num += (num8 + (num9 - num8) / 2) * elementalDamage2.Chance / 100;
			}
		}
		return num;
	}

	public static void FactionSheeter()
	{
		Sheet sheet = new Sheet(DataManager.SavePath("factionsheet.csv"));
		List<Faction> list = new List<Faction>();
		List<int> list2 = new List<int>();
		sheet.addColumn("Tier");
		foreach (Faction item in Factions.loop())
		{
			if (item.Visible && !item.Name.Contains("villagers"))
			{
				list.Add(item);
				list2.Add(0);
				sheet.addColumn(item.Name);
			}
		}
		sheet.endColumns();
		int num = 0;
		for (int i = 1; i <= 8; i++)
		{
			sheet.writeColumn(i);
			for (int j = 0; j < list.Count; j++)
			{
				foreach (GameObjectBlueprint factionMember in GameObjectFactory.Factory.GetFactionMembers(list[j].Name))
				{
					if (factionMember.Tier == i)
					{
						num++;
					}
				}
				sheet.writeColumn(num);
				list2[j] += num;
				num = 0;
			}
			sheet.endRow();
		}
		for (int k = 0; k < list.Count + 1; k++)
		{
			sheet.writeColumn("");
		}
		sheet.endRow();
		sheet.writeColumn("");
		for (int l = 0; l < list.Count; l++)
		{
			sheet.writeColumn(list2[l]);
		}
		sheet.finish();
	}

	public static void MonsterSheeter()
	{
		Sheet sheet = new Sheet(DataManager.SavePath("monstersheet.csv"));
		sheet.addColumn("Name");
		sheet.addColumn("Level");
		sheet.addColumn("Tier");
		sheet.addColumn("Role");
		sheet.addStatColumn("Melee");
		sheet.addStatColumn("StrMod");
		sheet.addStatColumn("Hitpoints");
		sheet.addStatColumn("AV");
		sheet.addStatColumn("DV");
		sheet.addStatColumn("MA");
		sheet.addColumn("Elec");
		sheet.addColumn("Heat");
		sheet.addColumn("Cold");
		sheet.addColumn("Acid");
		sheet.addStatColumn("QN");
		sheet.addStatColumn("MS");
		sheet.addColumn("XP");
		sheet.endColumns();
		Sheet sheet2 = new Sheet(DataManager.SavePath("monstersheet_short.csv"));
		sheet2.addColumn("Name");
		sheet2.addColumn("Level");
		sheet2.addColumn("Tier");
		sheet2.addColumn("Role");
		sheet2.addShortStatColumn("Melee");
		sheet2.addShortStatColumn("StrMod");
		sheet2.addShortStatColumn("Hitpoints");
		sheet2.addShortStatColumn("AV");
		sheet2.addShortStatColumn("DV");
		sheet2.addShortStatColumn("MA");
		sheet2.addColumn("Elec");
		sheet2.addColumn("Heat");
		sheet2.addColumn("Cold");
		sheet2.addColumn("Acid");
		sheet2.addShortStatColumn("QN");
		sheet2.addShortStatColumn("MS");
		sheet2.addColumn("XP");
		sheet2.endColumns();
		int num = 0;
		foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
		{
			XRL.World.Event.ResetPool();
			try
			{
				num++;
				if (num % 10 == 0)
				{
					Debug.Log(num + " of " + GameObjectFactory.Factory.Blueprints.Values.Count + " sheeted out...");
				}
				if (value.Name.Contains("Trader") || value.HasPart("Tier1Wares") || value.HasPart("Tier2Wares") || value.HasPart("Tier3Wares") || value.HasPart("Tier4Wares") || value.HasPart("Tier5Wares") || value.HasPart("Tier6Wares") || value.HasPart("Tier7Wares") || value.HasPart("Tier8Wares") || value.HasPart("YurlWares") || value.Builders.ContainsKey("DataDiskWares") || value.HasPart("DataDiskWares") || value.HasPart("GlowpadOasisWares") || value.HasPart("ScrapWares") || value.Builders.ContainsKey("Tier1Wares") || value.Builders.ContainsKey("Tier2Wares") || value.Builders.ContainsKey("Tier3Wares") || value.Builders.ContainsKey("Tier4Wares") || value.Builders.ContainsKey("Tier5Wares") || value.Builders.ContainsKey("Tier6Wares") || value.Builders.ContainsKey("Tier7Wares") || value.Builders.ContainsKey("Tier8Wares") || value.Builders.ContainsKey("YurlWares") || value.Builders.ContainsKey("GlowpadOasisWares") || value.Builders.ContainsKey("ScrapWares") || !value.HasStat("ElectricResistance") || !value.HasStat("Level") || !value.HasPart("Combat"))
				{
					continue;
				}
				XRL.World.Event.ResetPool();
				int tier = value.Tier;
				sheet.writeColumn(value.Name);
				sheet2.writeColumn(value.Name);
				if (value.HasStat("Level"))
				{
					sheet.writeColumn(value.GetStat("Level", new Statistic()).Value);
					sheet2.writeColumn(value.Name);
				}
				else
				{
					sheet.writeColumn("-1");
					sheet2.writeColumn("-1");
				}
				sheet.writeColumn(tier);
				sheet2.writeColumn(tier);
				string text = "unassiged";
				if (value.Tags.ContainsKey("Role") && !string.IsNullOrEmpty(value.Tags["Role"]))
				{
					text = value.Tags["Role"];
				}
				if (value.Props.ContainsKey("Role") && !string.IsNullOrEmpty(value.Props["Role"]))
				{
					text = value.Props["Role"];
				}
				sheet.writeColumn(text);
				sheet2.writeColumn(text);
				List<XRL.World.GameObject> list = new List<XRL.World.GameObject>();
				for (int i = 0; i < 100; i++)
				{
					list.Add(value.createOne());
				}
				sheet.writeStats(list.Map((XRL.World.GameObject o) => CalculateAttackDamage(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.StatMod("Strength")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Hitpoints")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatAV(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatDV(o)));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatMA(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => CalculateAttackDamage(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.StatMod("Strength")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Hitpoints")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatAV(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatDV(o)));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => Stats.GetCombatMA(o)));
				sheet.writeColumn(value.GetStat("ElectricResistance", new Statistic()).Value);
				sheet.writeColumn(value.GetStat("HeatResistance", new Statistic()).Value);
				sheet.writeColumn(value.GetStat("ColdResistance", new Statistic()).Value);
				sheet.writeColumn(value.GetStat("AcidResistance", new Statistic()).Value);
				sheet2.writeColumn(value.GetStat("ElectricResistance", new Statistic()).Value);
				sheet2.writeColumn(value.GetStat("HeatResistance", new Statistic()).Value);
				sheet2.writeColumn(value.GetStat("ColdResistance", new Statistic()).Value);
				sheet2.writeColumn(value.GetStat("AcidResistance", new Statistic()).Value);
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Speed")));
				sheet.writeStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("MoveSpeed")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("Speed")));
				sheet2.writeShortStats(list.Map((XRL.World.GameObject o) => o.GetStatValue("MoveSpeed")));
				int num2 = 0;
				if (value.GetStat("XPValue", new Statistic()).sValue == "*XPValue")
				{
					num2 = Convert.ToInt32(value.GetProp("*XPValue", "0"));
				}
				else
				{
					string valueOrSValue = value.GetStat("XPValue", new Statistic()).ValueOrSValue;
					if (!(value.GetStat("XPValue", new Statistic()).sValue == "*XP"))
					{
						num2 = ((!(valueOrSValue != "")) ? (-1) : Convert.ToInt32(valueOrSValue));
					}
					else
					{
						float num3 = Convert.ToInt32(value.GetStat("Level", new Statistic()).Value);
						num3 /= 2f;
						num2 = text switch
						{
							"Minion" => (int)(num3 * 20f), 
							"Leader" => (int)(num3 * 100f), 
							"Hero" => (int)(num3 * 200f), 
							_ => (int)(num3 * 50f), 
						};
					}
				}
				sheet.writeColumn(num2);
				sheet2.writeColumn(num2);
				sheet.endRow();
				sheet2.endRow();
			}
			catch (Exception ex)
			{
				Debug.LogError("error blueprint=" + value.Name);
				throw ex;
			}
		}
		sheet.finish();
		sheet2.finish();
	}
}
