using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class ModFactionSlayer : IModification
{
	public string Faction;

	public ModFactionSlayer()
	{
	}

	public ModFactionSlayer(int Tier)
		: base(Tier)
	{
	}

	public ModFactionSlayer(int Tier, string Faction)
		: this(Tier)
	{
		this.Faction = Faction;
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "ProfilingEngine";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModFactionSlayer).Faction != Faction)
		{
			return false;
		}
		return base.SameAs(p);
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
		Extensions.AppendRules(text: GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModFactionSlayer Decapitate", Tier) + "% chance to behead " + XRL.World.Faction.getFormattedName(Faction) + " on hit.", SB: E.Postfix);
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
			if (gameObjectParameter2.IsFactionMember(Faction))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModFactionSlayer Decapitate", Tier, subject).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					Axe_Decapitate.Decapitate(gameObjectParameter, gameObjectParameter2);
				}
			}
		}
		return base.FireEvent(E);
	}
}
