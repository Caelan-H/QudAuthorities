using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModRelicFreezing : IModification
{
	public ModRelicFreezing()
	{
	}

	public ModRelicFreezing(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 8;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "HeatPump";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			int num = MyPowerLoadLevel();
			if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
				gameObjectParameter2.TemperatureChange(GetTemperatureChangeRange(num).RollCached(), gameObjectParameter);
				gameObjectParameter2.TakeDamage(Stat.Random(GetLowDamage(num), GetHighDamage(num)), "from %t freezing weapon!", "Cold", null, null, null, gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}

	public static int GetLowDamage(int Tier, int PowerLoad = 100)
	{
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 1.6) + 3;
	}

	public int GetLowDamage(int PowerLoad = 100)
	{
		return GetLowDamage(Tier, PowerLoad);
	}

	public static int GetHighDamage(int Tier, int PowerLoad = 100)
	{
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 2.4) + 3;
	}

	public int GetHighDamage(int PowerLoad = 100)
	{
		return GetHighDamage(Tier, PowerLoad);
	}

	public static string GetDamageRange(int Tier, int PowerLoad = 100)
	{
		int lowDamage = GetLowDamage(Tier, PowerLoad);
		int highDamage = GetHighDamage(Tier, PowerLoad);
		if (lowDamage == highDamage)
		{
			return lowDamage.ToString();
		}
		return lowDamage + "-" + highDamage;
	}

	public string GetDamageRange(int PowerLoad = 100)
	{
		return GetDamageRange(Tier, PowerLoad);
	}

	public string GetTemperatureChangeRange(int PowerLoad = 100)
	{
		return "-" + (Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) + "d4";
	}

	public static string GetDescription(int Tier)
	{
		return "Freezing: When powered, this weapon deals " + ((Tier > 0) ? ("an additional " + GetDamageRange(Tier, 100)) : "") + " cold damage on hit.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Freezing: When powered, this weapon deals an additional " + GetDamageRange(Tier, powerLoad) + " cold damage on hit.";
	}
}
