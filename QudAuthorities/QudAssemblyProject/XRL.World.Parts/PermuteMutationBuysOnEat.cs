using System;

namespace XRL.World.Parts;

[Serializable]
public class PermuteMutationBuysOnEat : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			E.GetGameObjectParameter("Eater")?.PermuteRandomMutationBuys();
		}
		return base.FireEvent(E);
	}
}
