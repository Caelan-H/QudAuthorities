using System;

namespace XRL.World.Parts;

[Serializable]
public class GasGlitter : IPart
{
	public string GasType = "Glitter";

	public override bool SameAs(IPart p)
	{
		if ((p as GasGlitter).GasType != GasType)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "RefractLight");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "RefractLight" && ParentObject.GetPart("Gas") is Gas gas && gas.Density.in100())
		{
			E.SetParameter("By", ParentObject);
			return false;
		}
		return base.FireEvent(E);
	}
}
