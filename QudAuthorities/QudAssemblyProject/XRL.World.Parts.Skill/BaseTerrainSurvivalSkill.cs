using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public abstract class BaseTerrainSurvivalSkill : BaseSkill
{
	public abstract string GetTerrainName();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EncounterChanceEvent.ID && ID != GetLostChanceEvent.ID)
		{
			return ID == TravelSpeedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EncounterChanceEvent E)
	{
		if (E.TravelClass == base.Name)
		{
			E.PercentageBonus += 100;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetLostChanceEvent E)
	{
		if (E.TravelClass == base.Name)
		{
			E.PercentageBonus += 95;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TravelSpeedEvent E)
	{
		if (E.TravelClass == base.Name)
		{
			E.PercentageBonus += 100;
		}
		return base.HandleEvent(E);
	}
}
