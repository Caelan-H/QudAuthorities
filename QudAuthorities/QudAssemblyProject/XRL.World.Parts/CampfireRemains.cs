using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CampfireRemains : IPart
{
	public string Blueprint = "Campfire";

	public override bool SameAs(IPart p)
	{
		if ((p as CampfireRemains).Blueprint != Blueprint)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetPointsOfInterestEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor) && Campfire.FindExtinguishingPool(ParentObject) == null)
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("PointOfInterestKey");
			bool flag = true;
			string explanation = null;
			if (!string.IsNullOrEmpty(propertyOrTag))
			{
				PointOfInterest pointOfInterest = E.Find(propertyOrTag);
				if (pointOfInterest != null)
				{
					if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
					{
						E.Remove(pointOfInterest);
						explanation = "nearest";
					}
					else
					{
						flag = false;
						pointOfInterest.Explanation = "nearest";
					}
				}
			}
			if (flag)
			{
				E.Add(ParentObject, ParentObject.BaseDisplayName, explanation, propertyOrTag, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Light", "light", "LightCampfire", null, 'i', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "LightCampfire")
		{
			AttemptLight(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		AttemptLight(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Burn");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Burn")
		{
			ParentObject.ReplaceWith(GameObject.create(Blueprint));
			return false;
		}
		return base.FireEvent(E);
	}

	public void AttemptLight(GameObject who)
	{
		GameObject gameObject = Campfire.FindExtinguishingPool(ParentObject);
		if (gameObject != null)
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You cannot light " + ParentObject.the + ParentObject.ShortDisplayName + " while " + ParentObject.itis + " in " + gameObject.the + gameObject.ShortDisplayName + ".");
			}
		}
		else
		{
			GameObject gameObject2 = GameObject.create(Blueprint);
			IComponent<GameObject>.XDidY(who, "light", gameObject2.the + gameObject2.ShortDisplayName);
			ParentObject.ReplaceWith(gameObject2);
		}
	}
}
