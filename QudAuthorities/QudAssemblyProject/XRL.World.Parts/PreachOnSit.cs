using System;

namespace XRL.World.Parts;

[Serializable]
public class PreachOnSit : IPart
{
	public bool leaderInCombat;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeingSatOn");
		Object.RegisterPartEvent(this, "CanPreach");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanPreach")
		{
			if (ParentObject.pPhysics.CurrentCell.GetObjectCountWithPart("Combat") > 1)
			{
				return true;
			}
			return false;
		}
		if (E.ID == "BeingSatOn" && ParentObject.HasPart("Preacher"))
		{
			ParentObject.GetPart<Preacher>().PreacherHomily(Dialog: false);
		}
		return base.FireEvent(E);
	}
}
