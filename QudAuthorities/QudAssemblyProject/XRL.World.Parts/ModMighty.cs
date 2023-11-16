using System;

namespace XRL.World.Parts;

[Serializable]
public class ModMighty : IModification
{
	public ModMighty()
	{
	}

	public ModMighty(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart("MeleeWeapon");
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<MeleeWeapon>().MaxStrengthBonus = 999;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GetShortDescription");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetShortDescription")
		{
			string text = "\n&GMighty: This weapon has no strength bonus penetration cap.";
			E.SetParameter("Postfix", E.GetStringParameter("Postfix") + text);
		}
		return base.FireEvent(E);
	}
}
