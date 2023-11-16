using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, the item's tier for purposes of calculating
///             damage is increased by the standard power load bonus, i.e. 2 for the
///             standard overload power load of 400.
///             </remarks>
[Serializable]
public class ModFreezing : IMeleeModification
{
	public ModFreezing()
	{
	}

	public ModFreezing(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 10;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "HeatPump";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{freezing|freezing}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("ice", 5);
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
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 0.8);
	}

	public int GetLowDamage(int PowerLoad = 100)
	{
		return GetLowDamage(Tier, PowerLoad);
	}

	public static int GetHighDamage(int Tier, int PowerLoad = 100)
	{
		return (int)Math.Round((double)(Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 1.2);
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
		return "Freezing: When powered, this weapon deals " + ((Tier > 0) ? ("an additional " + GetDamageRange(Tier, 100)) : "additional") + " cold damage on hit.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Freezing: When powered, this weapon deals an additional " + GetDamageRange(Tier, powerLoad) + " cold damage on hit.";
	}
}
