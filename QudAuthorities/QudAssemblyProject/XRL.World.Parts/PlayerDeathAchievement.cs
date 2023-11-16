using System;

namespace XRL.World.Parts;

[Serializable]
public class PlayerDeathAchievement : IPart
{
	public string AchievementID = "";

	public string Killer;

	public string Weapon;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		The.Player?.RegisterPartEvent(this, "BeforeDie");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie" && ParentObject.IsValid())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Dying");
			if (gameObjectParameter == null || !gameObjectParameter.IsPlayer())
			{
				return true;
			}
			if ((gameObjectParameter.CurrentZone ?? The.ActiveZone) != ParentObject.CurrentZone)
			{
				return true;
			}
			if (!Killer.IsNullOrEmpty())
			{
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Killer");
				if (gameObjectParameter2 == null)
				{
					return true;
				}
				if (!gameObjectParameter2.GetBlueprint().DescendsFrom(Killer))
				{
					return true;
				}
			}
			if (!Weapon.IsNullOrEmpty())
			{
				GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
				if (gameObjectParameter3 == null)
				{
					return true;
				}
				if (!gameObjectParameter3.GetBlueprint().DescendsFrom(Weapon))
				{
					return true;
				}
			}
			AchievementManager.SetAchievement(AchievementID);
			ParentObject.Destroy();
		}
		return base.FireEvent(E);
	}
}
