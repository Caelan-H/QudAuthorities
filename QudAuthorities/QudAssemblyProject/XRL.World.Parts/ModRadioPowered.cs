using System;

namespace XRL.World.Parts;

[Serializable]
public class ModRadioPowered : IModification
{
	public ModRadioPowered()
	{
	}

	public ModRadioPowered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("EnergyCell"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		BroadcastPowerReceiver broadcastPowerReceiver = Object.GetPart<BroadcastPowerReceiver>();
		if (broadcastPowerReceiver == null)
		{
			broadcastPowerReceiver = new BroadcastPowerReceiver();
			if (Tier > 1)
			{
				broadcastPowerReceiver.ChargeRate *= Tier;
			}
			broadcastPowerReceiver.MaxSatellitePowerDepth = 11 + Tier;
			broadcastPowerReceiver.Obvious = true;
			broadcastPowerReceiver.SatellitePowerOcclusionReadout = true;
			Object.AddPart(broadcastPowerReceiver);
		}
		else
		{
			if (Tier > 0 && broadcastPowerReceiver.ChargeRate < Tier * 10)
			{
				broadcastPowerReceiver.ChargeRate = Tier * 10;
			}
			if (broadcastPowerReceiver.MaxSatellitePowerDepth < 11 + Tier)
			{
				broadcastPowerReceiver.MaxSatellitePowerDepth = 11 + Tier;
			}
			broadcastPowerReceiver.CanReceiveSatellitePower = true;
			broadcastPowerReceiver.Obvious = true;
			broadcastPowerReceiver.SatellitePowerOcclusionReadout = true;
		}
		IntegralRecharger part = Object.GetPart<IntegralRecharger>();
		if (part == null)
		{
			part = new IntegralRecharger();
			part.ChargeRate = broadcastPowerReceiver.ChargeRate;
			Object.AddPart(part);
		}
		else if (part.ChargeRate != 0 && part.ChargeRate < broadcastPowerReceiver.ChargeRate)
		{
			part.ChargeRate = broadcastPowerReceiver.ChargeRate;
		}
		IncreaseDifficultyAndComplexity(1, 1);
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
			E.AddAdjective("{{C|radio-powered}}");
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
		return "Radio-powered: This item can be recharged via broadcast power.";
	}
}
