using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe : BaseSkill
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		return true;
	}

	public override string GetWeaponCriticalDescription()
	{
		return "Axe (cleaves armor on critical hit)";
	}

	public override void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
	{
		if (Attacker.HasSkill("Axe_Cleave"))
		{
			Axe_Cleave.PerformCleave(Attacker, Defender, Weapon, Properties, 100, 1);
		}
		else
		{
			Axe_Cleave.PerformCleave(Attacker, Defender, Weapon, Properties, 100, 0, 1);
		}
		base.WeaponMadeCriticalHit(Attacker, Defender, Weapon, Properties);
	}
}
