using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Diseased : IPart
{
	public int CloneCooldown;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated")
		{
			if (Stat.Random(1, 100) <= 33)
			{
				switch (Stat.Random(1, 2))
				{
				case 1:
					ParentObject.ApplyEffect(new Glotrot());
					(ParentObject.GetEffect("Glotrot") as Glotrot).Stage = Stat.Random(0, 3);
					break;
				case 2:
					ParentObject.ApplyEffect(new Ironshank());
					(ParentObject.GetEffect("Ironshank") as Ironshank).SetPenalty(Stat.Random(1, 75));
					break;
				}
			}
			return true;
		}
		return true;
	}
}
