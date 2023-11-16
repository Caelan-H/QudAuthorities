using System;

namespace XRL.World.Parts;

[Serializable]
public class MaxPenetration : IPart
{
	public int Max = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as MaxPenetration).Max != Max)
		{
			return false;
		}
		return base.SameAs(p);
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
		if (E.ID == "WeaponHit" && E.GetIntParameter("Penetrations") > Max)
		{
			E.SetParameter("Penetrations", Max);
		}
		return base.FireEvent(E);
	}
}
