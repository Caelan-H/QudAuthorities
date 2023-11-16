using System;

namespace XRL.World.Parts;

[Serializable]
public class ModColossal : IModification
{
	[NonSerialized]
	private bool HaveAddedWeight;

	[NonSerialized]
	private int AddedWeight;

	public ModColossal()
	{
	}

	public ModColossal(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "MassEnhancement";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!(Object.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon))
		{
			return false;
		}
		if (meleeWeapon.Skill != "Cudgel")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
		{
			meleeWeapon.AdjustDamage(1);
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetItemElementsEvent.ID && ID != GetIntrinsicWeightEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("gigantic", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		E.Weight += GetAddedWeight();
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
		Object.RegisterPartEvent(this, "GetSlamMultiplier");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetSlamMultiplier")
		{
			E.SetParameter("Multiplier", E.GetIntParameter("Multiplier") + 1);
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Gigantic: This weapon has +1 damage and is twice as effective when you Slam with it. It's also much heavier.";
	}

	public int GetAddedWeight()
	{
		if (!HaveAddedWeight)
		{
			try
			{
				AddedWeight = Convert.ToInt32(ParentObject.GetBlueprint().GetPartParameter("Physics", "Weight"));
			}
			catch
			{
			}
			if (AddedWeight < 5)
			{
				AddedWeight = 5;
			}
			HaveAddedWeight = true;
		}
		return AddedWeight;
	}
}
