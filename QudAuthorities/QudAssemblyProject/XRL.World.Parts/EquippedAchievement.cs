using System;

namespace XRL.World.Parts;

[Serializable]
public class EquippedAchievement : IPart
{
	public string AchievementID = "";

	public bool Triggered;

	public EquippedAchievement()
	{
	}

	public EquippedAchievement(string ID)
	{
		AchievementID = ID;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped" && !Triggered)
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("EquippingObject");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				AchievementManager.SetAchievement(AchievementID);
				Triggered = true;
			}
		}
		return base.FireEvent(E);
	}
}
