using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EquippedFungusAchievement : IPart
{
	public bool Triggered;

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
				Body body = gameObjectParameter.Body;
				List<string> list = new List<string>();
				foreach (GameObject equippedObject in body.GetEquippedObjects())
				{
					if (equippedObject.HasPart("EquippedFungusAchievement") && !list.Contains(equippedObject.Blueprint))
					{
						list.Add(equippedObject.Blueprint);
					}
				}
				if (list.Count >= 3)
				{
					AchievementManager.SetAchievement("ACH_GET_FUNGAL_INFECTIONS");
				}
				Triggered = true;
			}
		}
		return base.FireEvent(E);
	}
}
