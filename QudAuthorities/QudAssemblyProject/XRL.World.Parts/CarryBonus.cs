using System;

namespace XRL.World.Parts;

[Serializable]
public class CarryBonus : IPart
{
	public string Style = "Flat";

	public int Amount;

	public bool BonusApplied;

	public CarryBonus()
	{
	}

	public CarryBonus(int Amount, string Style = "Flat")
	{
		this.Amount = Amount;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (!BonusApplied || ID != GetMaxCarriedWeightEvent.ID) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		if (BonusApplied)
		{
			if (Style == "Flat")
			{
				E.Weight += Amount;
			}
			else
			{
				E.AdjustWeight((double)(100 + Amount) / 100.0);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Amount > 0)
		{
			if (Style == "Flat")
			{
				E.Postfix.Append("\n{{rules|").AppendSigned(Amount).Append(" lbs. carry capacity}}");
			}
			else
			{
				E.Postfix.Append("\n{{rules|").AppendSigned(Amount).Append("% carry capacity}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		BonusApplied = ParentObject.IsEquippedProperly(E.Part);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		BonusApplied = false;
		return base.HandleEvent(E);
	}
}
