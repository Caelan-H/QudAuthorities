using System;
using System.Text;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class CyberElectricalGeneration : BaseMutation
{
	public int nCharges = 5;

	public int nTurnCounter;

	public Guid DischargeActivatedAbilityID = Guid.Empty;

	public int OldConductivity;

	public CyberElectricalGeneration()
	{
		DisplayName = "Electrical Generation";
		Type = "Cyber";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "ApplyEMP");
		Object.RegisterPartEvent(this, "CommandDischarge");
		Object.RegisterPartEvent(this, "DefendMeleeHit");
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You accrue electrical charge that you can discharge at will.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Maximum charges: " + (2 + Level * 2) + "\n", "Accrue an additional charge every ", (10 - (int)Math.Floor((decimal)(Level / 2))).ToString(), " rounds up to the maximum\n"), "Damage per charge: 1d4\n"), "Electricity will arc to adjacent targets dealing reduced damage");
	}

	public void Discharge(Cell C, int Voltage)
	{
		string damage = nCharges + "d4";
		nCharges = 0;
		UpdateAbility();
		ParentObject.Discharge(C, Voltage, damage, ParentObject);
	}

	public void UpdateAbility()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Discharge [").Append(nCharges).Append(" charges]");
		SetMyActivatedAbilityDisplayName(DischargeActivatedAbilityID, stringBuilder.ToString());
		if (nCharges == 0)
		{
			DisableMyActivatedAbility(DischargeActivatedAbilityID);
		}
		else
		{
			EnableMyActivatedAbility(DischargeActivatedAbilityID);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			nTurnCounter++;
			int num = 10 - (int)Math.Floor((decimal)(base.Level / 2));
			if (nTurnCounter >= num)
			{
				nTurnCounter = 0;
				int num2 = 2 + 2 * base.Level;
				if (nCharges < num2)
				{
					nCharges++;
				}
			}
			UpdateAbility();
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (nCharges > 0 && E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(DischargeActivatedAbilityID))
			{
				E.AddAICommand("CommandDischarge");
			}
		}
		else if (E.ID == "ApplyEMP")
		{
			Discharge(ParentObject.CurrentCell.GetAdjacentCells().GetRandomElement(), nCharges);
			UpdateAbility();
		}
		else if (E.ID == "CommandDischarge")
		{
			if (nCharges == 0)
			{
				return false;
			}
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation");
			Discharge(cell, nCharges);
			UpdateAbility();
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		nCharges = 2 + 2 * base.Level;
		UpdateAbility();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (ParentObject.pPhysics != null)
		{
			OldConductivity = ParentObject.pPhysics.Conductivity;
			ParentObject.pPhysics.Conductivity = 0;
		}
		DischargeActivatedAbilityID = AddMyActivatedAbility("Discharge", "CommandDischarge", "Physical Mutation", null, "û");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		if (ParentObject.pPhysics != null)
		{
			ParentObject.pPhysics.Conductivity = OldConductivity;
			OldConductivity = 0;
		}
		RemoveMyActivatedAbility(ref DischargeActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
