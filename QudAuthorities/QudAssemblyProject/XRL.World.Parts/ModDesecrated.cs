using System;

namespace XRL.World.Parts;

[Serializable]
public class ModDesecrated : IModification
{
	public ModDesecrated()
	{
	}

	public ModDesecrated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.pRender != null)
		{
			Object.pRender.ColorString = "&K";
			Object.pRender.DetailColor = "r";
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{K|desecrated}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Desecrated: This object has been desecrated by vandals.");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			ApplyModification(ParentObject);
		}
		return base.FireEvent(E);
	}
}
