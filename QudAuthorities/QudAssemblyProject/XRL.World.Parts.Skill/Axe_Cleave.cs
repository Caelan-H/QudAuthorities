using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Cleave : BaseSkill
{
	[NonSerialized]
	private static int Penalty;

	[NonSerialized]
	private static string PenaltyEffect;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterAttack");
		base.Register(Object);
	}

	private static void ProcessPenalty(Effect FX)
	{
		if (FX is IShatterEffect shatterEffect)
		{
			Penalty += shatterEffect.GetPenalty();
		}
	}

	private static void ProcessPenalties(GameObject obj)
	{
		obj.ForeachEffect(PenaltyEffect, ProcessPenalty);
	}

	public static int GetCurrentPenalty(GameObject who, string EffectName, string EquipmentEffectName)
	{
		Penalty = 0;
		if (EffectName != null)
		{
			PenaltyEffect = EffectName;
			ProcessPenalties(who);
		}
		if (EquipmentEffectName != null)
		{
			PenaltyEffect = EquipmentEffectName;
			who.ForeachEquippedObject(ProcessPenalties);
		}
		return Penalty;
	}

	public static void PerformCleave(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties = null, int Chance = 75, int AdjustAVPenalty = 0, int? MaxAVPenalty = null)
	{
		if (Attacker == null || Defender == null || Weapon == null || Defender.HasPart("Gas") || Defender.HasPart("NoDamage") || !(Weapon.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon) || meleeWeapon.Skill != "Axe")
		{
			return;
		}
		string stat = meleeWeapon.Stat;
		string name;
		string text;
		string text2;
		string equipmentEffectName;
		string iD;
		if (Statistic.IsMental(stat))
		{
			name = "MA";
			text = "mental armor";
			text2 = "ShatterMentalArmor";
			equipmentEffectName = null;
			iD = "CanApplyShatterMentalArmor";
		}
		else
		{
			name = "AV";
			text = "armor";
			text2 = "ShatterArmor";
			equipmentEffectName = "ShatteredArmor";
			iD = "CanApplyShatterArmor";
		}
		if (Defender == null || !Defender.HasStat(name) || !Defender.FireEvent(iD) || !CanApplyEffectEvent.Check(Defender, text2))
		{
			return;
		}
		int num = Attacker.StatMod(stat);
		int num2;
		if (!MaxAVPenalty.HasValue)
		{
			num2 = num / 2 + AdjustAVPenalty;
			if (num % 2 == 1)
			{
				num2++;
			}
		}
		else
		{
			num2 = MaxAVPenalty.Value + AdjustAVPenalty;
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		bool flag = Properties != null && Properties.Contains("Charging") && Attacker.HasSkill("Axe_ChargingStrike");
		if (flag)
		{
			num2++;
		}
		Statistic stat2 = Defender.GetStat(name);
		int currentPenalty = GetCurrentPenalty(Defender, text2, equipmentEffectName);
		if (stat2.Value <= stat2.Min || currentPenalty >= num2)
		{
			return;
		}
		GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Skill Cleave", Chance, Defender);
		if (!Chance.in100())
		{
			return;
		}
		if (Attacker.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You cleave through " + Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName) + " " + text + ".", 'G');
		}
		else if (Defender.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(Attacker.The + Attacker.ShortDisplayName + Attacker.GetVerb("cleave") + " through your " + text + ".", 'R');
		}
		else if (IComponent<GameObject>.Visible(Defender))
		{
			if (Defender.IsPlayerLed())
			{
				IComponent<GameObject>.AddPlayerMessage(Attacker.The + Attacker.ShortDisplayName + Attacker.GetVerb("cleave") + " through " + Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName) + " " + text + ".", 'r');
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(Attacker.The + Attacker.ShortDisplayName + Attacker.GetVerb("cleave") + " through " + Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName) + " " + text + ".", 'g');
			}
		}
		if (!(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text2)) is IShatterEffect shatterEffect))
		{
			return;
		}
		shatterEffect.Duration = 300;
		shatterEffect.SetOwner(Attacker);
		if (flag)
		{
			if (Attacker.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("The momentum from your charge causes your axe to cleave deeper through " + Defender.the + Grammar.MakePossessive(Defender.ShortDisplayName) + " " + text + ".", 'g');
			}
			Defender.DustPuff("&c");
			if (currentPenalty < num2 - 1)
			{
				shatterEffect.IncrementPenalty();
			}
		}
		Defender.ApplyEffect(shatterEffect);
		Defender.ShatterSplatter();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterAttack" && !E.HasFlag("Critical"))
		{
			PerformCleave(E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Defender"), E.GetGameObjectParameter("Weapon"), E.GetStringParameter("Properties"));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return base.RemoveSkill(GO);
	}
}
