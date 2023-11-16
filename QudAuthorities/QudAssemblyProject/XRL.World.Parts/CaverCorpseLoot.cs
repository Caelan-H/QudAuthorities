using System;

namespace XRL.World.Parts;

[Serializable]
public class CaverCorpseLoot : IPart
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
			Physics obj = ParentObject.GetPart("Physics") as Physics;
			obj.CurrentCell.AddObject(GameObject.create("Miner's Helmet"));
			obj.CurrentCell.AddObject(GameObject.create("Pickaxe"));
			obj.CurrentCell.AddObject(GameObject.create("Sheaf1"));
			obj.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			obj.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			obj.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			obj.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			obj.CurrentCell.AddObject(GameObject.create("Canned Have-It-All"));
			obj.CurrentCell.AddObject(GameObject.create("Small Sphere of Negative Weight"));
			obj.CurrentCell.AddObject(GameObject.create("CyberneticsCreditWedge"));
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
			return true;
		}
		return base.FireEvent(E);
	}
}
