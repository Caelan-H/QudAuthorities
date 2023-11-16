using System;

namespace XRL.World.Parts;

[Serializable]
public class ModCybrid : IModification
{
	public ModCybrid()
	{
	}

	public ModCybrid(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "BiomechanicalAdapter";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("ElectricalPowerTransmission") is ElectricalPowerTransmission electricalPowerTransmission))
		{
			return false;
		}
		if (!electricalPowerTransmission.IsConsumer)
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		ElectricalPowerTransmission electricalPowerTransmission = Object.GetPart("ElectricalPowerTransmission") as ElectricalPowerTransmission;
		int num = ((electricalPowerTransmission != null) ? (electricalPowerTransmission.ChargeRate * 2 / 5) : (Tier * Tier * 50));
		BiomechanicalPowerTransmission biomechanicalPowerTransmission = Object.RequirePart<BiomechanicalPowerTransmission>();
		if (biomechanicalPowerTransmission.ChargeRate < num)
		{
			biomechanicalPowerTransmission.ChargeRate = num;
		}
		if (electricalPowerTransmission == null || electricalPowerTransmission.IsConsumer)
		{
			biomechanicalPowerTransmission.IsConsumer = true;
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{biomech|cybrid}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Cybrid: Can draw power from biomechanical power transmission systems in addition to an electrical power grid.";
	}
}
