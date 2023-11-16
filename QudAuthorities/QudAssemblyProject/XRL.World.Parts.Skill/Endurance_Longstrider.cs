using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Endurance_Longstrider : BaseSkill
{
	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetSprintDurationEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSprintDurationEvent E)
	{
		E.LinearIncrease += 10;
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (!GO.HasSkill("Tactics_Run"))
		{
			GO.AddSkill("Tactics_Run");
		}
		return base.AddSkill(GO);
	}
}
