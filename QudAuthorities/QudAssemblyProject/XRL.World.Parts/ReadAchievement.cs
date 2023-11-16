using System;

namespace XRL.World.Parts;

[Serializable]
public class ReadAchievement : IPart
{
	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			if (ID == InventoryActionEvent.ID)
			{
				return !Triggered;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			if (AchievementManager.GetAchievement("ACH_READ_10_BOOKS"))
			{
				AchievementManager.IncrementAchievement("ACH_READ_100_BOOKS");
			}
			else
			{
				AchievementManager.IncrementAchievement("ACH_READ_10_BOOKS");
			}
			Triggered = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("scholarship", 2);
		return base.HandleEvent(E);
	}
}
