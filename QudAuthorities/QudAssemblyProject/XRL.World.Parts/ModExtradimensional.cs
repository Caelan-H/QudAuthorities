using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Effects;
using XRL.World.Encounters;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class ModExtradimensional : IModification
{
	public int WeaponModIndex;

	public int MissileWeaponModIndex;

	public int ArmorModIndex;

	public int ShieldModIndex;

	public int MiscModIndex;

	public string Training;

	public string DimensionName;

	public string SecretID;

	public bool LookedAt;

	public ModExtradimensional()
	{
	}

	public ModExtradimensional(int Tier)
		: base(Tier)
	{
	}

	public ModExtradimensional(int WeaponModIndex, int MissileWeaponModIndex, int ArmorModIndex, int ShieldModIndex, int MiscModIndex, string Training, string DimensionName, string SecretID)
	{
		this.WeaponModIndex = WeaponModIndex;
		this.MissileWeaponModIndex = MissileWeaponModIndex;
		this.ArmorModIndex = ArmorModIndex;
		this.ShieldModIndex = ShieldModIndex;
		this.MiscModIndex = MiscModIndex;
		this.Training = Training;
		this.DimensionName = DimensionName;
		this.SecretID = SecretID;
	}

	public ModExtradimensional(ModExtradimensional Source)
	{
		WeaponModIndex = Source.WeaponModIndex;
		MissileWeaponModIndex = Source.MissileWeaponModIndex;
		ArmorModIndex = Source.ArmorModIndex;
		ShieldModIndex = Source.ShieldModIndex;
		MiscModIndex = Source.MiscModIndex;
		Training = Source.Training;
		DimensionName = Source.DimensionName;
		SecretID = Source.SecretID;
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		NameForStatus = "***ERROR***";
		if (The.Game == null || !The.Game.HasObjectGameState("PsychicManager"))
		{
			WeaponModIndex = 0;
			MissileWeaponModIndex = 0;
			ArmorModIndex = 0;
			ShieldModIndex = 0;
			MiscModIndex = 0;
			Training = null;
			DimensionName = "The Dead Square";
			return;
		}
		PsychicManager psychicManager = The.Game.GetObjectGameState("PsychicManager") as PsychicManager;
		int num = Stat.Random(0, psychicManager.ExtraDimensions.Count + psychicManager.PsychicFactions.Count - 1);
		if (num < psychicManager.ExtraDimensions.Count)
		{
			ExtraDimension extraDimension = psychicManager.ExtraDimensions[num];
			WeaponModIndex = extraDimension.WeaponIndex;
			MissileWeaponModIndex = extraDimension.MissileWeaponIndex;
			ArmorModIndex = extraDimension.ArmorIndex;
			ShieldModIndex = extraDimension.ShieldIndex;
			MiscModIndex = extraDimension.MiscIndex;
			Training = extraDimension.Training;
			DimensionName = extraDimension.Name.Replace("*dimensionSymbol*", ((char)extraDimension.Symbol).ToString());
			SecretID = extraDimension.SecretID;
		}
		else
		{
			num -= psychicManager.ExtraDimensions.Count;
			PsychicFaction psychicFaction = psychicManager.PsychicFactions[num];
			WeaponModIndex = psychicFaction.dimensionalWeaponIndex;
			MissileWeaponModIndex = psychicFaction.dimensionalMissileWeaponIndex;
			ArmorModIndex = psychicFaction.dimensionalArmorIndex;
			ShieldModIndex = psychicFaction.dimensionalShieldIndex;
			MiscModIndex = psychicFaction.dimensionalMiscIndex;
			Training = psychicFaction.dimensionalTraining;
			DimensionName = psychicFaction.dimensionName.Replace("*dimensionSymbol*", ((char)psychicFaction.dimensionSymbol).ToString());
			SecretID = psychicFaction.dimensionSecretID;
		}
	}

	public override void ApplyModification(GameObject Object)
	{
		AddDimensionalBonus(Object);
		IncreaseDifficultyAndComplexityIfComplex(5, 1);
		Object.RequirePart<AnimatedMaterialExtradimensional>();
		Object.SetImportant(flag: true, force: true);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != RealityStabilizeEvent.ID)
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.AddPart(new ModExtradimensional(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{extradimensional|extradimensional}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("chance", 3);
		E.Add("travel", 2);
		E.Add("might", 1);
		E.Add("stars", 1);
		E.Add("time", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (!E.Projecting && E.Check() && 1.in1000())
		{
			ParentObject.ApplyEffect(new Rusted());
			if (ParentObject == null || ParentObject.IsInvalid() || ParentObject.IsInGraveyard())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public string GetDescription(int Tier)
	{
		return "Extradimensional: This item recently materialized in this dimension having inherited some properties from its home dimension, {{O|" + DimensionName + "}}.\n";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt && !string.IsNullOrEmpty(SecretID) && ParentObject.Understood())
		{
			LookedAt = true;
			JournalAPI.RevealObservation(SecretID, onlyIfNotRevealed: true);
		}
		return base.FireEvent(E);
	}

	public void AddDimensionalBonus(GameObject obj = null)
	{
		if (obj == null)
		{
			obj = ParentObject;
		}
		if (obj.HasTag("MeleeWeapon"))
		{
			if (WeaponModIndex == 0)
			{
				obj.GetPart<MeleeWeapon>().PenBonus++;
			}
			else if (WeaponModIndex == 1)
			{
				obj.GetPart<MeleeWeapon>().AdjustDamageDieSize(2);
			}
			else if (WeaponModIndex == 2)
			{
				obj.GetPart<MeleeWeapon>().AdjustDamage(1);
			}
			else if (WeaponModIndex == 3)
			{
				obj.GetPart<MeleeWeapon>().AdjustBonusCap(3);
			}
			else if (WeaponModIndex == 4)
			{
				obj.GetPart<MeleeWeapon>().HitBonus += 2;
			}
			else if (WeaponModIndex == 5)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "MA:1");
			}
			else if (WeaponModIndex == 6)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Strength:1");
			}
			else if (WeaponModIndex == 7)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Agility:1");
			}
			else if (WeaponModIndex == 8)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Toughness:1");
			}
			else if (WeaponModIndex == 9)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Intelligence:1");
			}
			else if (WeaponModIndex == 10)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Willpower:1");
			}
			else if (WeaponModIndex == 11)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Ego:1");
			}
			else if (WeaponModIndex == 12)
			{
				if (!obj.HasPart("ModGlazed"))
				{
					obj.AddPart(new ModGlazed(1));
				}
			}
			else if (WeaponModIndex == 13)
			{
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(1));
				}
			}
			else if (WeaponModIndex == 14)
			{
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(1));
				}
			}
			else if (WeaponModIndex == 15)
			{
				if (!obj.HasPart("ModBeetlehost"))
				{
					obj.AddPart(new ModBeetlehost(1));
				}
			}
			else if (WeaponModIndex == 16)
			{
				if (!obj.HasPart("ModFatecaller"))
				{
					obj.AddPart(new ModFatecaller(1));
				}
			}
			else if (WeaponModIndex == 17 && !obj.HasPart("ModImprovedElectricalGeneration"))
			{
				obj.AddPart(new ModImprovedElectricalGeneration(1));
			}
		}
		else if (obj.HasPart("MissileWeapon"))
		{
			MissileWeapon missileWeapon = obj.GetPart("MissileWeapon") as MissileWeapon;
			if (MissileWeaponModIndex == 0)
			{
				missileWeapon.NoWildfire = true;
			}
			else if (MissileWeaponModIndex == 1)
			{
				if (missileWeapon.AmmoPerAction == missileWeapon.ShotsPerAction)
				{
					missileWeapon.AmmoPerAction++;
				}
				missileWeapon.ShotsPerAction++;
				if (obj.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader && magazineAmmoLoader.MaxAmmo < missileWeapon.AmmoPerAction)
				{
					magazineAmmoLoader.MaxAmmo = missileWeapon.AmmoPerAction;
				}
			}
			else if (MissileWeaponModIndex == 2)
			{
				obj.RequirePart<MissilePerformance>().PenetrationModifier++;
			}
			else if (MissileWeaponModIndex == 3)
			{
				obj.RequirePart<MissilePerformance>().DamageDieModifier += 2;
			}
			else if (MissileWeaponModIndex == 4)
			{
				obj.RequirePart<MissilePerformance>().DamageModifier++;
			}
			else if (MissileWeaponModIndex == 5)
			{
				obj.RequirePart<MissilePerformance>().PenetrateCreatures = true;
			}
			else if (MissileWeaponModIndex == 6)
			{
				obj.RequirePart<MissilePerformance>().WantAddAttribute("Vorpal");
			}
			else if (MissileWeaponModIndex == 7)
			{
				if (missileWeapon.WeaponAccuracy > 0)
				{
					missileWeapon.WeaponAccuracy = Math.Max(missileWeapon.WeaponAccuracy - Stat.Random(5, 10), 0);
				}
				else
				{
					missileWeapon.AimVarianceBonus += Stat.Random(2, 6);
				}
			}
			else if (MissileWeaponModIndex == 8)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "MA:1");
			}
			else if (MissileWeaponModIndex == 9)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Strength:1");
			}
			else if (MissileWeaponModIndex == 10)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Agility:1");
			}
			else if (MissileWeaponModIndex == 11)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Toughness:1");
			}
			else if (MissileWeaponModIndex == 12)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Intelligence:1");
			}
			else if (MissileWeaponModIndex == 13)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Willpower:1");
			}
			else if (MissileWeaponModIndex == 14)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Ego:1");
			}
			else if (MissileWeaponModIndex == 15)
			{
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(1));
				}
			}
			else if (MissileWeaponModIndex == 16)
			{
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(1));
				}
			}
			else if (MissileWeaponModIndex == 17 && !obj.HasPart("ModImprovedElectricalGeneration"))
			{
				obj.AddPart(new ModImprovedElectricalGeneration(1));
			}
		}
		else if (obj.HasPart("Armor"))
		{
			Armor armor = obj.GetPart("Armor") as Armor;
			if (ArmorModIndex == 0)
			{
				armor.DV++;
			}
			else if (ArmorModIndex == 1)
			{
				armor.AV++;
			}
			else if (ArmorModIndex == 2)
			{
				armor.MA += 2;
			}
			else if (ArmorModIndex == 3)
			{
				armor.Acid += 10;
			}
			else if (ArmorModIndex == 4)
			{
				armor.Cold += 10;
			}
			else if (ArmorModIndex == 5)
			{
				armor.Heat += 10;
			}
			else if (ArmorModIndex == 6)
			{
				armor.Elec += 10;
			}
			else if (ArmorModIndex == 7)
			{
				armor.Strength++;
			}
			else if (ArmorModIndex == 8)
			{
				armor.Agility++;
			}
			else if (ArmorModIndex == 9)
			{
				armor.Toughness++;
			}
			else if (ArmorModIndex == 10)
			{
				armor.Intelligence++;
			}
			else if (ArmorModIndex == 11)
			{
				armor.Willpower++;
			}
			else if (ArmorModIndex == 12)
			{
				armor.Ego++;
			}
			else if (ArmorModIndex == 13)
			{
				armor.ToHit += 2;
			}
			else if (ArmorModIndex == 14)
			{
				if (armor.SpeedPenalty > 0)
				{
					armor.SpeedPenalty = Math.Max(armor.SpeedPenalty - Stat.Random(5, 10), 0);
				}
				else
				{
					armor.SpeedBonus += Stat.Random(1, 5);
				}
			}
			else if (ArmorModIndex == 15)
			{
				if (!obj.HasPart("ModGlassArmor"))
				{
					obj.AddPart(new ModGlassArmor(1));
				}
			}
			else if (ArmorModIndex == 16)
			{
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(1));
				}
			}
			else if (ArmorModIndex == 17)
			{
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(1));
				}
			}
			else if (ArmorModIndex == 18)
			{
				if (!obj.HasPart("ModBlinkEscape"))
				{
					obj.AddPart(new ModBlinkEscape(1));
				}
			}
			else if (ArmorModIndex == 19 && !obj.HasPart("ModImprovedElectricalGeneration"))
			{
				obj.AddPart(new ModImprovedElectricalGeneration(1));
			}
		}
		else if (obj.HasPart("Shield"))
		{
			Shield shield = obj.GetPart("Shield") as Shield;
			if (ShieldModIndex == 0)
			{
				if (!obj.HasPart("ModImprovedBlock"))
				{
					obj.AddPart(new ModImprovedBlock(1));
				}
			}
			else if (ShieldModIndex == 1)
			{
				if (shield.DV < 0)
				{
					shield.DV++;
					if (shield.DV < 0 && 50.in100())
					{
						shield.DV++;
					}
				}
			}
			else if (ShieldModIndex == 2)
			{
				if (shield.SpeedPenalty > 0)
				{
					shield.SpeedPenalty = Math.Max(shield.SpeedPenalty - Stat.Random(5, 10), 0);
				}
			}
			else if (ShieldModIndex == 3)
			{
				shield.AV++;
			}
			else if (ShieldModIndex == 4)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "MA:1");
			}
			else if (ShieldModIndex == 5)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Strength:1");
			}
			else if (ShieldModIndex == 6)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Agility:1");
			}
			else if (ShieldModIndex == 7)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Toughness:1");
			}
			else if (ShieldModIndex == 8)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Willpower:1");
			}
			else if (ShieldModIndex == 9)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Intelligence:1");
			}
			else if (ShieldModIndex == 10)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "Ego:1");
			}
			else if (ShieldModIndex == 11)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "AcidResistance:10");
			}
			else if (ShieldModIndex == 12)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "ColdResistance:10");
			}
			else if (ShieldModIndex == 13)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "HeatResistance:10");
			}
			else if (ShieldModIndex == 14)
			{
				EquipStatBoost.AppendBoostOnEquip(obj, "ElectricResistance:10");
			}
			else if (ShieldModIndex == 15)
			{
				if (!obj.HasPart("ModGlassArmor"))
				{
					obj.AddPart(new ModGlassArmor(1));
				}
			}
			else if (ShieldModIndex == 16)
			{
				if (!obj.HasPart("ModImprovedLightManipulation"))
				{
					obj.AddPart(new ModImprovedLightManipulation(1));
				}
			}
			else if (ShieldModIndex == 17)
			{
				if (!obj.HasPart("ModImprovedTemporalFugue"))
				{
					obj.AddPart(new ModImprovedTemporalFugue(1));
				}
			}
			else if (ShieldModIndex == 18 && !obj.HasPart("ModImprovedElectricalGeneration"))
			{
				obj.AddPart(new ModImprovedElectricalGeneration(1));
			}
		}
		else if (obj.GetInventoryCategory() == "Books")
		{
			TrainingBook trainingBook = obj.RequirePart<TrainingBook>();
			if (Statistic.Attributes.Contains(Training))
			{
				trainingBook.Attribute = Training;
				trainingBook.Skill = null;
			}
			else
			{
				trainingBook.Attribute = null;
				trainingBook.Skill = Training;
			}
		}
		if (obj.GetPart("Commerce") is Commerce commerce)
		{
			commerce.Value *= 2.0;
		}
	}

	[WishCommand(null, null)]
	public static void MakeExtradimensional()
	{
		Popup.PickGameObject("Pick item", The.Player.GetInventoryAndEquipment(), AllowEscape: true, ShowContext: true)?.AddPart(new ModExtradimensional());
	}
}
