using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IRechargeable : IPoweredPart
{
	public IRechargeable()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public abstract void AddCharge(int Amount);

	public abstract bool CanBeRecharged();

	public abstract int GetRechargeAmount();
}
