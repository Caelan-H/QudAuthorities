using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModElectrified : IMeleeModification
{
	public ModElectrified()
	{
	}

	public ModElectrified(int Tier)
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
		NameForStatus = "ArcDischarger";
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
			E.AddAdjective("{{electrical|electrified}}");
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
		E.Add("circuitry", 5);
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
				int lowDamage = GetLowDamage(num);
				int highDamage = GetHighDamage(num);
				int voltage = Stat.Random(lowDamage, highDamage);
				string damageRange = GetDamageRange(num);
				gameObjectParameter.Discharge(gameObjectParameter2.CurrentCell, voltage, damageRange, gameObjectParameter);
			}
		}
		return base.FireEvent(E);
	}

	public static int GetLowDamage(int Tier, int PowerLoad = 100)
	{
		return Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad);
	}

	public int GetLowDamage(int PowerLoad = 100)
	{
		return GetLowDamage(Tier, PowerLoad);
	}

	public static int GetHighDamage(int Tier, int PowerLoad = 100)
	{
		return (Tier + IComponent<GameObject>.PowerLoadBonus(PowerLoad)) * 3 / 2;
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

	public static string GetDescription(int Tier)
	{
		return "Electrified: When powered, this weapon deals " + ((Tier > 0) ? ("an additional " + GetDamageRange(Tier, 100)) : "additional") + " electrical damage on hit.";
	}

	public string GetInstanceDescription()
	{
		int powerLoad = MyPowerLoadLevel();
		return "Electrified: When powered, this weapon deals an additional " + GetDamageRange(Tier, powerLoad) + " electrical damage on hit.";
	}
}
