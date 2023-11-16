using System;

namespace XRL.World.Parts;

[Serializable]
public class QuantumRippler : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == RealityStabilizeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K!");
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("collapse") + " under the pressure of normality and" + ParentObject.GetVerb("implode") + ".");
			}
			ParentObject.Explode(20000, E.Effect.Owner, "12d10+300", 1f, Neutron: true);
			return false;
		}
		return base.HandleEvent(E);
	}
}
