using System;

namespace XRL.World.Parts;

[Serializable]
public class CaverCorpseLoot2 : IPart
{
	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
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
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			Physics physics = ParentObject.GetPart("Physics") as Physics;
			if (physics.CurrentCell.ParentZone.X == 1 && physics.currentCell.ParentZone.Y == 1)
			{
				physics.CurrentCell.AddObject(GameObject.create("Laser Pistol"));
				physics.CurrentCell.AddObject(GameObject.create("Solar Cell"));
				physics.CurrentCell.AddObject(GameObject.create("DataDisk"));
				physics.CurrentCell.AddObject(GameObject.create("Floating Glowsphere"));
				physics.CurrentCell.AddObject(GameObject.create("Basic Toolkit"));
				physics.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
				physics.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
