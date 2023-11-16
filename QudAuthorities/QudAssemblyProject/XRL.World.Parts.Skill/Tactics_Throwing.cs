using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Throwing : BaseSkill
{
	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.ModIntProperty("CloseThrowRangeAccuracySkillBonus", 50);
		GO.ModIntProperty("ThrowRangeSkillBonus", 3);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		GO.ModIntProperty("CloseThrowRangeAccuracySkillBonus", -50, RemoveIfZero: true);
		GO.ModIntProperty("ThrowRangeSkillBonus", -3);
		return base.AddSkill(GO);
	}
}
