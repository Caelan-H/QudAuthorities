using System;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Sed : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetSocialSifrahSetupEvent.ID)
		{
			return ID == ReputationChangeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		E.Rating += 10;
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReputationChangeEvent E)
	{
		if (E.BaseAmount < 0 && !E.Transient)
		{
			E.Amount /= 2;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EvilTwinAttitudeSetup")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Twin");
			if (gameObjectParameter?.pBrain != null)
			{
				gameObjectParameter.pBrain.SetFeeling(ParentObject, 100);
				gameObjectParameter.pBrain.Factions = "highly entropic beings-100";
				gameObjectParameter.pBrain.InitFromFactions();
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
