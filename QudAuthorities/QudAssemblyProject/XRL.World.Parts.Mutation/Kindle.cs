using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Kindle : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Kindle()
	{
		DisplayName = "Kindle";
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandKindle");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return string.Concat(string.Concat("" + "You ignite a small fire with your mind.\n\n", "Range: 12\n"), "Cooldown: 50");
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 12 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandKindle");
			}
		}
		else if (E.ID == "CommandKindle")
		{
			Cell cell = PickDestinationCell(12, AllowVis.Any, Locked: false);
			if (cell == null)
			{
				return false;
			}
			if (ParentObject.DistanceTo(cell) > 12)
			{
				return ParentObject.ShowFailure("That is out of range (12 squares)");
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 50);
			GameObject gameObject = GameObject.create("Kindleflame");
			gameObject.RequirePart<TorchProperties>().LastThrower = ParentObject;
			gameObject.RequirePart<Temporary>().Duration = Stat.Random(50, 75);
			cell.AddObject(gameObject);
			UseEnergy(1000, "Mental Mutation Kindle");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Kindle", "CommandKindle", "Mental Mutation", null, "\u00a8");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
