using System;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: temperature changes are increased by a percentage
///             equal to ((power load - 100) / 10), i.e. 30% for the standard overload
///             power load of 400.
///             </remarks>
[Serializable]
public class TemperatureOnHit : IPart
{
	public string Amount = "0";

	public bool Max;

	public int MaxTemp = 400;

	public bool OnWielderHit;

	public bool RequiresLit;

	public TemperatureOnHit()
	{
	}

	public TemperatureOnHit(string Amount, int Max)
	{
		this.Amount = Amount;
		MaxTemp = Max;
	}

	public override bool SameAs(IPart p)
	{
		return (p as TemperatureOnHit).Amount == Amount;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ProjectileHit");
		if (!OnWielderHit)
		{
			Object.RegisterPartEvent(this, "WeaponDealDamage");
		}
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	private bool CheckRequirements()
	{
		if (RequiresLit)
		{
			if (!(ParentObject.GetPart("LightSource") is LightSource lightSource))
			{
				return false;
			}
			if (!lightSource.Lit)
			{
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if ((E.ID == "WeaponHit" || E.ID == "WeaponDealDamage" || E.ID == "ProjectileHit") && CheckRequirements())
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter != null && (!Max || ((Amount.RollMaxCached() >= 0) ? (gameObjectParameter.pPhysics.Temperature < MaxTemp) : (gameObjectParameter.pPhysics.Temperature > MaxTemp))))
			{
				int num = Amount.RollCached();
				int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
				if (num2 != 0)
				{
					num = num * (100 + num2) / 100;
				}
				gameObjectParameter.TemperatureChange(num, E.GetGameObjectParameter("Attacker"), Radiant: false, MinAmbient: false, MaxAmbient: false, ParentObject.GetPhase());
			}
		}
		return base.FireEvent(E);
	}
}
