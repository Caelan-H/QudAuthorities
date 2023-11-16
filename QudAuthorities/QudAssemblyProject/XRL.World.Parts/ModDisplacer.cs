using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
///             which it is by default, the maximum teleport distance is increased
///             by the standard power load bonus, i.e. 2 for the standard overload
///             power load of 400.
///             </remarks>
[Serializable]
public class ModDisplacer : IModification
{
	public const int MIN_DISTANCE = 1;

	public const int MAX_DISTANCE = 4;

	public ModDisplacer()
	{
	}

	public ModDisplacer(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		ChargeUse = 250;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "SpatialTransposer";
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(2, 1);
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
			E.AddAdjective("{{displacer|displacer}}", 5);
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
		E.Add("travel", 2);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "LauncherProjectileHit");
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "WeaponPseudoThrowHit");
		Object.RegisterPartEvent(this, "WeaponThrowHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			int num = MyPowerLoadLevel();
			int num2 = Stat.Random(1, 4 + IComponent<GameObject>.PowerLoadBonus(num));
			if (num2 > 0 && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, num))
			{
				GameObject obj = E.GetGameObjectParameter("Attacker");
				GameObject obj2 = E.GetGameObjectParameter("Defender");
				if (GameObject.validate(ref obj) && GameObject.validate(ref obj2))
				{
					GameObject gameObject = obj2;
					int maxDistance = num2;
					gameObject.RandomTeleport(Swirl: true, null, ParentObject, obj, E, 0, maxDistance);
				}
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Displacer: When powered, this weapon randomly teleports its target " + 1 + "-" + 4 + " tiles away on a successful hit.";
	}

	public string GetInstanceDescription()
	{
		int load = MyPowerLoadLevel();
		return "Displacer: When powered, this weapon randomly teleports its target " + 1 + "-" + (4 + IComponent<GameObject>.PowerLoadBonus(load)) + " tiles away on a successful hit.";
	}
}
