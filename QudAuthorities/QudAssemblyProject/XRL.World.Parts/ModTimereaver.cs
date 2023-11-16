using System;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModTimereaver : IModification
{
	public int Chance = 3;

	public int RealityStabilizationPenetration = 50;

	public ModTimereaver()
	{
	}

	public ModTimereaver(int Tier)
		: base(Tier)
	{
	}

	public ModTimereaver(int Tier, int Chance)
		: this(Tier)
	{
		this.Chance = Chance;
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "TimelikeManifoldResonator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart("MeleeWeapon"))
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification()
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetItemElementsEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int @for = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModTimereaver TimeStop", Chance);
		E.Postfix.AppendRules("Timereaver: This weapon has " + Grammar.A(@for) + "% chance to stop time for two turns when " + ParentObject.it + ParentObject.GetVerb("hit", PrependSpace: true, PronounAntecedent: true) + ".");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 10);
		E.Add("might", 2);
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
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModTimereaver TimeStop", Chance, subject).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && gameObjectParameter.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", gameObjectParameter, "Device", ParentObject, "RealityStabilizationPenetration", RealityStabilizationPenetration), E))
			{
				gameObjectParameter.DilationSplat();
				gameObjectParameter.ApplyEffect(new TimeCubed(2200));
			}
		}
		return base.FireEvent(E);
	}
}
