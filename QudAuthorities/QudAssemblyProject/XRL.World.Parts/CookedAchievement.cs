using System;

namespace XRL.World.Parts;

[Serializable]
public class CookedAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public CookedAchievement()
	{
	}

	public CookedAchievement(string AchievementID)
	{
		this.AchievementID = AchievementID;
	}

	public override bool SameAs(IPart p)
	{
		if (p is CookedAchievement cookedAchievement)
		{
			return AchievementID == cookedAchievement.AchievementID;
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "UsedAsIngredient");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "UsedAsIngredient" && !Triggered)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				AchievementManager.SetAchievement(AchievementID);
				Triggered = true;
				ParentObject.RemovePart(this);
			}
		}
		return base.FireEvent(E);
	}
}
