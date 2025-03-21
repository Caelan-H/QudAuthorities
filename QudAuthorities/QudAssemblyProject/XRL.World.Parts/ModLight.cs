using System;

namespace XRL.World.Parts;

[Serializable]
public class ModLight : IModification
{
	public ModLight()
	{
	}

	public ModLight(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseComplexityIfComplex(1);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustWeightEvent.ID && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustWeightEvent E)
	{
		E.AdjustWeight(0.5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("slender", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Slender: This item weighs less than normal.");
		return base.HandleEvent(E);
	}
}
