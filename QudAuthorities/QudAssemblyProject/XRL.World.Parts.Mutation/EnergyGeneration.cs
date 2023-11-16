using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class EnergyGeneration : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public EnergyGeneration()
	{
		DisplayName = "Energy Generation";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandEnergyGeneration");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		_ = E.ID == "CommandEnergyGeneration";
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (GO.GetPart("ActivatedAbilities") is ActivatedAbilities activatedAbilities)
		{
			ActivatedAbilityID = activatedAbilities.AddAbility("Energy Generation", "CommandEnergyGeneration", "Physical Mutation");
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (ActivatedAbilityID != Guid.Empty)
		{
			(GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(ActivatedAbilityID);
		}
		return base.Unmutate(GO);
	}
}
