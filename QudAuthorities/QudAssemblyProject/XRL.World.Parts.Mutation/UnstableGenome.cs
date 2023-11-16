using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class UnstableGenome : BaseMutation
{
	public UnstableGenome()
	{
		DisplayName = "Unstable Genome";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool ShouldShowLevel()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLevelGainedEarly");
		base.Register(Object);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.UnregisterPartEvent(this, "AfterLevelGainedEarly");
		return true;
	}

	public override string GetDescription()
	{
		return "You gain one extra mutation each time you buy this, but the mutations don't manifest right away.\nWhenever you gain a level, there's a 33% chance that your genome destabilizes and you get to choose from 3 random mutations.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLevelGainedEarly")
		{
			if (!ParentObject.IsPlayer())
			{
				return true;
			}
			if (Stat.Random(1, 100) <= 33)
			{
				StatusScreen.BuyRandomMutation(ParentObject);
				if (base.Level == 1)
				{
					ParentObject.GetPart<Mutations>().RemoveMutation(this);
				}
				else
				{
					base.Level--;
				}
			}
		}
		return base.FireEvent(E);
	}
}
