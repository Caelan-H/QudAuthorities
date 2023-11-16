using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

public static class Disarming
{
	public static GameObject Disarm(GameObject Object, GameObject Disarmer, int SaveTarget, string SaveStat = "Strength", string DisarmerStat = "Agility", GameObject DisarmingWeapon = null)
	{
		Body body = Object.Body;
		if (body == null)
		{
			return null;
		}
		List<BodyPart> part = body.GetPart("Hand");
		part.AddRange(body.GetPart("Missile Weapon"));
		foreach (BodyPart item in part)
		{
			if (item.Equipped == null)
			{
				continue;
			}
			GameObject equipped = item.Equipped;
			GameObjectBlueprint blueprint = equipped.GetBlueprint();
			try
			{
				if (!equipped.IsNatural() && (blueprint.InheritsFrom("MeleeWeapon") || blueprint.InheritsFrom("MissileWeapon")))
				{
					if (Object.MakeSave(SaveStat, SaveTarget, Disarmer, DisarmerStat, "Disarm", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, DisarmingWeapon))
					{
						return null;
					}
					if (Object.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", item)))
					{
						Object.FireEvent(Event.New("CommandDropObject", "Object", equipped, "Forced", 1));
						equipped.Move(Directions.GetRandomDirection(), Forced: true);
						Messaging.XDidYToZ(Disarmer, "disarm", null, Object, null, "!", null, null, Object, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Object.IsPlayer());
						return equipped;
					}
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error when disarming " + equipped, x);
				return null;
			}
		}
		return null;
	}
}
