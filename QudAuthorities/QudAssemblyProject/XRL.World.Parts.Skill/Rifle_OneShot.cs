using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Rifle_OneShot : BaseSkill
{
	public int Cooldown;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndSegment");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndSegment" && Cooldown > 0)
		{
			Cooldown--;
		}
		return base.FireEvent(E);
	}
}
