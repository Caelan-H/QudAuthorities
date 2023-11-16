using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class TeleportOther : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public TeleportOther()
	{
		DisplayName = "Teleport Other";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandTeleportOther");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You teleport an adjacent creature to a random nearby location.";
	}

	public override string GetLevelText(int Level)
	{
		return "Cooldown: {{rules|" + GetCooldownTurns(Level) + "}} rounds";
	}

	public int GetCooldownTurns(int Level)
	{
		return Math.Max(115 - 10 * Level, 5);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && E.GetGameObjectParameter("Target").FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
			{
				E.AddAICommand("CommandTeleportOther");
			}
		}
		else if (E.ID == "CommandTeleportOther")
		{
			if (!ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			if (cell == ParentObject.CurrentCell)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You may not teleport " + ParentObject.itself + " with Teleport Other!");
				}
				return false;
			}
			GameObject gameObject = null;
			foreach (GameObject item in cell.GetObjectsInCell())
			{
				if (item.HasPart("Combat"))
				{
					if (item.GetPart("MentalMirror") is MentalMirror mentalMirror && mentalMirror.CheckActive())
					{
						mentalMirror.Activate();
						gameObject = ParentObject;
					}
					else
					{
						gameObject = item;
					}
					break;
				}
			}
			if (gameObject == null)
			{
				return false;
			}
			if (!gameObject.RandomTeleport(Swirl: true, this, null, null, E))
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldownTurns(base.Level));
			UseEnergy(1000, "Mental Mutation Teleport Other");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Teleport Other", "CommandTeleportOther", "Mental Mutation", null, "\u001b");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
