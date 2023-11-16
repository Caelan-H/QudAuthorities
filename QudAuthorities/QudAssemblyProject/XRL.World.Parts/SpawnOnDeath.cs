using System;

namespace XRL.World.Parts;

[Serializable]
public class SpawnOnDeath : IPart
{
	public string Blueprint = "Bloatfly";

	public bool DoPuff = true;

	public string PuffColor = "&K";

	public SpawnOnDeath()
	{
	}

	public SpawnOnDeath(string blueprint)
	{
		Blueprint = blueprint;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeDeathRemoval");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeathRemoval" && ParentObject.pPhysics.CurrentCell != null)
		{
			ParentObject.pPhysics.CurrentCell.AddObject(Blueprint);
			if (DoPuff)
			{
				ParentObject.DustPuff(PuffColor);
			}
		}
		return base.FireEvent(E);
	}
}
