using System;

namespace XRL.World.Parts;

[Serializable]
public class ModHardened : IModification
{
	public ModHardened()
	{
	}

	public ModHardened(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "ElectromagneticShielding";
	}

	private void HardenActivePart(IActivePart Part)
	{
		Part.IsEMPSensitive = false;
	}

	public void Harden(GameObject Object)
	{
		Object.ForeachPartDescendedFrom((Action<IActivePart>)HardenActivePart);
	}

	public bool HardenablePartCheck(IActivePart Part)
	{
		return !Part.IsEMPSensitive;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return !Object.ForeachPartDescendedFrom((Predicate<IActivePart>)HardenablePartCheck);
	}

	public override void ApplyModification(GameObject Object)
	{
		Harden(Object);
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ModificationAppliedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{mercurial|electromagnetic}} shielding");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModificationAppliedEvent E)
	{
		Harden(ParentObject);
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Electromagnetic shielding: This item is immune to electromagnetic pulses.";
	}
}
