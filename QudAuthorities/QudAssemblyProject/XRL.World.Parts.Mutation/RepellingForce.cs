using System;
using System.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class RepellingForce : BaseMutation
{
	public const string ABL_CMD = "CommandRepellingForce";

	public const int ABL_CLD = 30;

	public new Guid ActivatedAbilityID;

	public RepellingForce()
	{
		DisplayName = "Repelling Force";
		Type = "Mental";
	}

	public override string GetDescription()
	{
		return "You invoke a repelling force in the surrounding area, throwing enemies back.";
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Compound("Area: 7x7", '\n');
		if (Level == base.Level)
		{
			stringBuilder.Compound("Creatures are pushed away from center of blast.", '\n');
		}
		else
		{
			stringBuilder.Compound("{{rules|Increased push force}}", '\n');
		}
		stringBuilder.Compound("Cooldown: ", '\n').Append(30).Append(" rounds");
		return stringBuilder.ToString();
	}

	public override bool Mutate(GameObject Object, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(DisplayName, "CommandRepellingForce", "Mental Mutation", null, "#");
		return base.Mutate(Object, Level);
	}

	public override bool Unmutate(GameObject Object)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(Object);
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

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandRepellingForce");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandRepellingForce");
			}
		}
		else if (E.ID == "CommandRepellingForce")
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.ShowFailure("You cannot use " + DisplayName + " on the world map.");
			}
			DidX("flip", "polarity");
			StunningForce.Concussion(ParentObject.CurrentCell, ParentObject, base.Level, 3, ParentObject.GetPhase(), null, Stun: false, Damage: false);
			CooldownMyActivatedAbility(ActivatedAbilityID, 30);
			UseEnergy(1000);
		}
		return base.FireEvent(E);
	}
}
