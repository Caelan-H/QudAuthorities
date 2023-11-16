using System;
using XRL.Language;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class ModRazored : IModification
{
	public int Chance = 3;

	public ModRazored()
	{
	}

	public ModRazored(int Tier)
		: base(Tier)
	{
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModRazored).Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part == null)
		{
			return false;
		}
		if (part.Skill != "LongBlades" && part.Skill != "Axe")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
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
			E.AddAdjective("{{Y|serra{{R|t}}ed}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("might", 1);
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
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModRazored Dismember", Chance, subject).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				bool flag = 1.in1000();
				Axe_Dismember.Dismember(gameObjectParameter, gameObjectParameter2, null, null, ParentObject, null, flag, !flag);
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Serrated: This weapon has a chance to dismember opponents.";
	}

	public string GetInstanceDescription()
	{
		int @for = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModRazored Dismember", Chance);
		return "Serrated: This weapon has " + Grammar.A(@for) + "% chance to dismember opponents.";
	}
}
