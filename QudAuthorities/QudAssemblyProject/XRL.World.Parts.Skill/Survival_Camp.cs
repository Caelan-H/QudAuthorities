using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Survival_Camp : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public Guid StopActivatedAbilityID = Guid.Empty;

	public List<string> CampedZones = new List<string>();

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandSurvivalCamp");
		base.Register(Object);
	}

	public bool AttemptCamp(GameObject who)
	{
		if (who.AreHostilesNearby())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You can't cook with hostiles nearby.");
			}
			return false;
		}
		if (!who.CanChangeMovementMode("Camping", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (who.CurrentCell.ParentZone.IsWorldMap())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You can't cook on the world map.");
			}
			return false;
		}
		PointOfInterest one = GetPointsOfInterestEvent.GetOne(who, "Campfire");
		if (one != null && one.GetDistanceTo(who) <= 24)
		{
			GameObject @object = one.Object;
			if (@object != null)
			{
				if (who.IsPlayer())
				{
					if (one.IsAt(who))
					{
						Popup.ShowFail("There " + @object.Is + " already " + @object.a + @object.ShortDisplayName + " here.");
					}
					else if (Popup.ShowYesNoCancel("There " + @object.Is + " already " + @object.a + @object.ShortDisplayName + " " + who.DescribeDirectionToward(@object) + ". Do you want to go to " + @object.them + "?") == DialogResult.Yes)
					{
						one.NavigateTo(who);
					}
				}
				return false;
			}
		}
		string text = PickDirectionS();
		if (text != null)
		{
			Cell cellFromDirection = who.CurrentCell.GetCellFromDirection(text);
			if (cellFromDirection == null)
			{
				return false;
			}
			if (!cellFromDirection.IsEmpty() || cellFromDirection.HasObjectWithTag("ExcavatoryTerrainFeature"))
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("Something is in the way!");
				}
				return false;
			}
			GameObject gameObject = Campfire.FindExtinguishingPool(cellFromDirection);
			if (gameObject != null)
			{
				if (who.IsPlayer())
				{
					Popup.ShowFail("You cannot start a campfire in " + gameObject.the + gameObject.ShortDisplayName + ".");
				}
				return false;
			}
			IComponent<GameObject>.XDidY(who, "make", "camp");
			GameObject gameObject2 = ((!cellFromDirection.ParentZone.ZoneID.StartsWith("ThinWorld")) ? cellFromDirection.AddObject("Campfire") : cellFromDirection.AddObject("BlueCampfire"));
			if (who.IsPlayer())
			{
				gameObject2.SetIntProperty("PlayerCampfire", 1);
			}
			if (!CampedZones.Contains(who.CurrentZone.ZoneID))
			{
				CampedZones.Add(who.CurrentZone.ZoneID);
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSurvivalCamp")
		{
			AttemptCamp(ParentObject);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Make Camp", "CommandSurvivalCamp", "Maneuvers", "Start a campfire for cooking meals and preserving foods. You can't make camp in combat.", "\u0006");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}
}
