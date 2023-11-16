using System;

namespace XRL.World.Parts;

[Serializable]
public class RecruitAchievement : IPart
{
	public bool Triggered;

	public string AchievementID;

	public override bool SameAs(IPart p)
	{
		RecruitAchievement recruitAchievement = p as RecruitAchievement;
		if (recruitAchievement.Triggered != Triggered)
		{
			return false;
		}
		if (recruitAchievement.AchievementID != AchievementID)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BecomeCompanion");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (!Triggered && E.ID == "BecomeCompanion" && (E.GetGameObjectParameter("Leader") ?? ParentObject.pBrain?.PartyLeader).IsPlayer() && !AchievementID.IsNullOrEmpty())
		{
			AchievementManager.SetAchievement(AchievementID);
			Triggered = true;
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}
}
