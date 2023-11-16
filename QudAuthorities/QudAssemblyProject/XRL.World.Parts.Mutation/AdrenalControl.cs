using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class AdrenalControl : BaseMutation
{
	public Guid IncrementActivatedAbilityID = Guid.Empty;

	public Guid DecrementActivatedAbilityID = Guid.Empty;

	public AdrenalControl()
	{
		DisplayName = "Adrenal Control";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandAdrenalControl");
		Object.RegisterPartEvent(this, "CommandCancelAdrenalControl");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
	}

	public override string GetDescription()
	{
		return "You regulate your body's release of adrenaline.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Cooldown: 200 rounds\n";
		text += "Duration: 15 rounds\n";
		text = text + "+" + (13 + Level * 7) + "% quickness\n";
		int num = (int)Math.Round((float)(30 + 50 * Level) / 100f);
		if (num == 1)
		{
			return text + "About 1 point of damage suffered each round";
		}
		return text + "About " + num + " points of damage suffered each round";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (ParentObject.HasEffect("Stressed"))
			{
				EnableMyActivatedAbility(DecrementActivatedAbilityID);
			}
			else
			{
				DisableMyActivatedAbility(DecrementActivatedAbilityID);
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 4 && IsMyActivatedAbilityAIUsable(IncrementActivatedAbilityID))
			{
				E.AddAICommand("CommandAdrenalControl");
			}
		}
		else if (E.ID == "CommandCancelAdrenalControl")
		{
			ParentObject.RemoveEffect("Stressed");
		}
		else if (E.ID == "CommandAdrenalControl")
		{
			CooldownMyActivatedAbility(IncrementActivatedAbilityID, 200);
			ParentObject.ApplyEffect(new Stressed(16, base.Level));
			UseEnergy(1000, "Physical Mutation");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		IncrementActivatedAbilityID = AddMyActivatedAbility("Adrenal Control", "CommandAdrenalControl", "Physical Mutation");
		DecrementActivatedAbilityID = AddMyActivatedAbility("Cancel Adrenal Control", "CommandCancelAdrenalControl", "Physical Mutation");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref IncrementActivatedAbilityID);
		RemoveMyActivatedAbility(ref DecrementActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
