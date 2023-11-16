using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class TenfoldPath_Tza : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetCooldownEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCooldownEvent E)
	{
		if (E.Ability.Class == "Mental Mutation")
		{
			E.PercentageReduction += 10;
		}
		return base.HandleEvent(E);
	}
}
