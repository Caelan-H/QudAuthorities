using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMercurial : IModification
{
	public int Chance = 100;

	public ModMercurial()
	{
		WorksOnEquipper = true;
		ChargeUse = 100;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "ReactiveTeleporter";
	}

	public ModMercurial(int Tier)
		: base(Tier)
	{
	}

	public ModMercurial(int Tier, int Chance)
		: this(Tier)
	{
		this.Chance = Chance;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(3, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{Y|mercurial}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), base.AddStatusSummary);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (!activePartSubject.OnWorldMap() && activePartSubject.FireEvent("CheckRealityDistortionUsability") && Chance.in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					activePartSubject.RandomTeleport(Swirl: true);
				}
			}
		}
		return base.FireEvent(E);
	}

	public string GetInstanceDescription()
	{
		return "Mercurial: Teleports the user to safety upon taking damage" + ((Chance == 100) ? "" : (" (" + Chance + "% chance)")) + ".";
	}
}
