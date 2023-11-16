using System;
using XRL.World.Skills;

namespace XRL.World.Parts.Skill;

[Serializable]
public class BaseSkill : IPart
{
	public string DisplayName
	{
		get
		{
			if (SkillFactory.Factory.SkillByClass.TryGetValue(base.Name, out var value))
			{
				return value.Name;
			}
			if (SkillFactory.Factory.PowersByClass.TryGetValue(base.Name, out var value2))
			{
				return value2.Name;
			}
			string text = base.Name;
			int num = text.LastIndexOf('_');
			if (num != -1)
			{
				text = text.Substring(num + 1);
			}
			return text;
		}
		set
		{
			if (value == DisplayName)
			{
				MetricsManager.LogCallingModError("You do not need to set the display name of the skill " + DisplayName + ", please remove the attempt to set it");
				return;
			}
			MetricsManager.LogCallingModError("You cannot set the display name of the skill " + DisplayName + " to " + value + ", please remove the attempt to set it");
		}
	}

	public virtual bool AddSkill(GameObject GO)
	{
		return true;
	}

	public virtual bool RemoveSkill(GameObject GO)
	{
		return true;
	}

	public virtual void UseEnergy(int Amount)
	{
		ParentObject.UseEnergy(Amount, "Skill");
	}

	public virtual void UseEnergy(int Amount, string Type)
	{
		ParentObject.UseEnergy(Amount, Type);
	}

	public virtual string GetWeaponCriticalDescription()
	{
		return null;
	}

	public virtual int GetWeaponCriticalModifier(GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return 0;
	}

	public virtual void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
	{
	}
}
