using System;

namespace XRL.World.Parts;

[Serializable]
public class ModImprovedWindmillStance : IModification
{
	public bool Applied;

	public ModImprovedWindmillStance()
	{
	}

	public ModImprovedWindmillStance(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		NameForStatus = "ImprovedWindmillStance";
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
		E.Postfix.AppendRules("Your windmill stance lasts twice as long.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped")
		{
			if (ParentObject.IsEquippedProperly())
			{
				E.GetGameObjectParameter("EquippingObject").ModIntProperty("ImprovedWindmillStance", 1, RemoveIfZero: true);
				Applied = true;
			}
		}
		else if (E.ID == "Unequipped" && Applied)
		{
			Applied = false;
			E.GetGameObjectParameter("UnequippingObject").ModIntProperty("ImprovedWindmillStance", 1, RemoveIfZero: true);
		}
		return base.FireEvent(E);
	}
}
