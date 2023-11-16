using System;

namespace XRL.World.Parts;

[Serializable]
public class ModLacquered : IModification
{
	public ModLacquered()
	{
	}

	public ModLacquered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart("Metal"))
		{
			return true;
		}
		if (!(Object.GetPart("Springy") is Springy springy))
		{
			return true;
		}
		if ((double)springy.Factor <= 0.5)
		{
			return true;
		}
		if (Object.GetPart("Armor") is Armor armor && armor.AV >= 2)
		{
			return true;
		}
		return false;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RemoveEffect("Rusted");
		Object.RemoveEffect("LiquidCovered");
		Object.RemoveEffect("LiquidStained");
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetMaximumLiquidExposureEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{lacquered|lacquered}}", -5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyRusted");
		Object.RegisterPartEvent(this, "ApplyLiquidCovered");
		Object.RegisterPartEvent(this, "ApplyLiquidStained");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyRusted" || E.ID == "ApplyLiquidCovered" || E.ID == "ApplyLiquidStained")
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Lacquered: This item repels liquids and cannot rust.";
	}
}
