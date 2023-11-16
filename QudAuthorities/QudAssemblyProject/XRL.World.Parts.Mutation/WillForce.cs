using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class WillForce : BaseMutation
{
	public Guid StrengthActivatedAbilityID = Guid.Empty;

	public Guid AgilityActivatedAbilityID = Guid.Empty;

	public Guid ToughnessActivatedAbilityID = Guid.Empty;

	public WillForce()
	{
		DisplayName = "Ego Projection";
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
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandWillForceAgility");
		Object.RegisterPartEvent(this, "CommandWillForceStrength");
		Object.RegisterPartEvent(this, "CommandWillForceToughness");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "Through sheer force of will, you perform uncanny physical feats.";
	}

	public int GetLowDuration(int Level)
	{
		return 16 + 2 * Level;
	}

	public int GetHighDuration(int Level)
	{
		return 20 + 2 * Level;
	}

	public override string GetLevelText(int Level)
	{
		string text = "Augments one physical attribute by an amount equal to twice your Ego bonus\n";
		text = text + "Duration: {{rules|" + GetLowDuration(Level) + "-" + GetHighDuration(Level) + "}} rounds\n";
		return text + "Cooldown: 200 rounds";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 4)
			{
				if (IsMyActivatedAbilityAIUsable(StrengthActivatedAbilityID))
				{
					E.AddAICommand("CommandWillForceStrength", (ParentObject.BaseStat("Strength") <= ParentObject.BaseStat("Agility") || ParentObject.BaseStat("Strength") <= ParentObject.BaseStat("Toughness")) ? 1 : 3);
				}
				if (IsMyActivatedAbilityAIUsable(AgilityActivatedAbilityID))
				{
					E.AddAICommand("CommandWillForceAgility", (ParentObject.BaseStat("Agility") <= ParentObject.BaseStat("Strength") || ParentObject.BaseStat("Agility") <= ParentObject.BaseStat("Toughness")) ? 1 : 3);
				}
				if (IsMyActivatedAbilityAIUsable(ToughnessActivatedAbilityID))
				{
					E.AddAICommand("CommandWillForceToughness");
				}
			}
		}
		else if (E.ID == "CommandWillForceStrength")
		{
			if (!ActivateWillForce("Strength"))
			{
				return false;
			}
		}
		else if (E.ID == "CommandWillForceAgility")
		{
			if (!ActivateWillForce("Agility"))
			{
				return false;
			}
		}
		else if (E.ID == "CommandWillForceToughness" && !ActivateWillForce("Toughness"))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool ActivateWillForce(string Stat)
	{
		CooldownMyActivatedAbility(StrengthActivatedAbilityID, 200);
		CooldownMyActivatedAbility(AgilityActivatedAbilityID, 200);
		CooldownMyActivatedAbility(ToughnessActivatedAbilityID, 200);
		ParentObject.UseEnergy(1000, "Mental Mutation Ego Projection");
		int num = Math.Max(ParentObject.StatMod("Ego") * 2, 1);
		int num2 = XRL.Rules.Stat.Random(GetLowDuration(base.Level), GetHighDuration(base.Level));
		foreach (Effect effect in ParentObject.Effects)
		{
			if (effect is BoostStatistic boostStatistic && boostStatistic.Statistic == Stat)
			{
				if (boostStatistic.Bonus < num)
				{
					boostStatistic.Duration = 0;
					ParentObject.CleanEffects();
					break;
				}
				if (boostStatistic.Duration < num2)
				{
					boostStatistic.Duration = num2;
				}
				return true;
			}
		}
		ParentObject.ApplyEffect(new BoostStatistic(num2, Stat, num));
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		StrengthActivatedAbilityID = AddMyActivatedAbility("Boost Strength", "CommandWillForceStrength", "Mental Mutation", null, "¾");
		AgilityActivatedAbilityID = AddMyActivatedAbility("Boost Agility", "CommandWillForceAgility", "Mental Mutation", null, "\u00af");
		ToughnessActivatedAbilityID = AddMyActivatedAbility("Boost Toughness", "CommandWillForceToughness", "Mental Mutation", null, "\u0003");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref StrengthActivatedAbilityID);
		RemoveMyActivatedAbility(ref AgilityActivatedAbilityID);
		RemoveMyActivatedAbility(ref ToughnessActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
