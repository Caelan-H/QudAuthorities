using System;

namespace XRL.World.Parts;

[Serializable]
public class GasOnEntering : IPart
{
	public string Blueprint = "AcidGas";

	public string Density = "3d10";

	public override bool SameAs(IPart p)
	{
		GasOnEntering gasOnEntering = p as GasOnEntering;
		if (gasOnEntering.Blueprint != Blueprint)
		{
			return false;
		}
		if (gasOnEntering.Density != Density)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ProjectileEntering");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileEntering" && E.GetParameter("Cell") is Cell cell)
		{
			GameObject gameObject = GameObject.create(Blueprint);
			Gas obj = gameObject.GetPart("Gas") as Gas;
			obj.Density = Density.RollCached() * (100 + MyPowerLoadBonus(int.MinValue, 100, 10)) / 100;
			obj.Creator = E.GetGameObjectParameter("Attacker");
			cell.AddObject(gameObject);
		}
		return base.FireEvent(E);
	}
}
