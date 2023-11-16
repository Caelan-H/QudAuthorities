using System;
using System.Linq;
using Genkit;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class CryptSitterBehavior : IPart
{
	public bool Alerted;

	public Location2D startingLocation;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AICryptHelpBroadcast");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public void Alert()
	{
		Alerted = true;
		ParentObject.pBrain.Hostile = true;
		ParentObject.pRender.DetailColor = "R";
		ParentObject.RemoveIntProperty("ForceFeeling");
		DidX("stir", null, null, null, null, IComponent<GameObject>.ThePlayer);
	}

	public void Unalert()
	{
		Alerted = false;
		ParentObject.pBrain.Hostile = false;
		ParentObject.pRender.DetailColor = "Y";
		ParentObject.SetIntProperty("ForceFeeling", 0);
		DidX("sleep", null, null, null, IComponent<GameObject>.ThePlayer);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" || E.ID == "AICryptHelpBroadcast")
		{
			Alert();
		}
		else if (E.ID == "BeginTakeAction")
		{
			if (startingLocation == null && ParentObject.CurrentCell != null)
			{
				startingLocation = ParentObject.CurrentCell.location;
			}
			if (!Alerted)
			{
				if (ParentObject.CurrentCell.location != startingLocation)
				{
					if (!ParentObject.pBrain.Goals.Items.Any((GoalHandler g) => g.GetType() == typeof(MoveTo) || g.GetType() == typeof(Step)))
					{
						ParentObject.pBrain.Goals.Clear();
						ParentObject.pBrain.MoveTo(ParentObject.CurrentZone, startingLocation);
					}
					return true;
				}
				ParentObject.UseEnergy(1000);
				return false;
			}
			if (ParentObject.DistanceTo(startingLocation) > 10)
			{
				Unalert();
				ParentObject.pBrain.Goals.Clear();
				ParentObject.pBrain.PushGoal(new MoveTo(ParentObject.CurrentZone.GetCell(startingLocation)));
			}
		}
		return base.FireEvent(E);
	}
}
