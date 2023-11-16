using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class DefensiveChromatophores : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public DefensiveChromatophores()
	{
		DisplayName = "Defensive Chromatophores";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandTakeActionEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (!ParentObject.IsPlayer() && AttemptScintillate(Auto: true))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("jewels", 2);
		E.Add("chance", 1);
		return base.HandleEvent(E);
	}

	public override string GetLevelText(int Level)
	{
		return "You can't act while scintillating.\nConfuses nearby hostile creatures per Confusion " + Level + ".\nDuration: 5 rounds\nCooldown: 200 rounds";
	}

	public override string GetDescription()
	{
		return "In stressful situations, you scintillate.";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandScintillate");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandScintillate" && !AttemptScintillate())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Scintillate", "CommandScintillate", "Physical Mutation", null, "\u000f");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}

	public bool AttemptScintillate(bool Auto = false)
	{
		if (!ShouldStartScintillating())
		{
			if (!Auto && ParentObject.IsPlayer())
			{
				if (IsMyActivatedAbilityCoolingDown(ActivatedAbilityID))
				{
					Popup.Show("You can't scintillate again so soon.");
				}
				else
				{
					Popup.Show("You're not under enough stress to scintillate.");
				}
			}
			return false;
		}
		if (!ParentObject.ApplyEffect(new Scintillating(5, base.Level)))
		{
			return false;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, 200);
		return true;
	}

	public bool ShouldStartScintillating()
	{
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			return false;
		}
		if (ParentObject.GetHPPercent() > 15 && (ParentObject.PartyLeader == null || ParentObject.PartyLeader.GetHPPercent() > 15 || !ParentObject.HasLOSTo(ParentObject.PartyLeader) || ParentObject.DistanceTo(ParentObject.PartyLeader) > 30))
		{
			return false;
		}
		return true;
	}
}
