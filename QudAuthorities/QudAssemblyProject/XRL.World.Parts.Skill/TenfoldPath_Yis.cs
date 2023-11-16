using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class TenfoldPath_Yis : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetLevelUpSkillPointsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetLevelUpSkillPointsEvent E)
	{
		E.Amount += 30;
		return base.HandleEvent(E);
	}
}
