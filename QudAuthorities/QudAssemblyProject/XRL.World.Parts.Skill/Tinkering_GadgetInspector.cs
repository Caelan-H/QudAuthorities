using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_GadgetInspector : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetTinkeringBonusEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Inspect")
		{
			E.Bonus += 5;
		}
		return base.HandleEvent(E);
	}
}
