using System;

namespace XRL.World.Parts;

[Serializable]
public class ElectricalPowerTransmission : IPowerTransmission
{
	public ElectricalPowerTransmission()
	{
		ChargeRate = 500;
		ChanceBreakConnectedOnDestroy = 100;
		Substance = "charge";
		Activity = "conducting";
		Constituent = "wiring";
		Assembly = "power grid";
		Unit = "amp";
		UnitFactor = 0.1;
		SparkWhenBrokenAndPowered = true;
	}

	public override string GetPowerTransmissionType()
	{
		return "electrical";
	}
}
