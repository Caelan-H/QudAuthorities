using System;

namespace XRL.World.Parts;

[Serializable]
public class BlinkOnDamage : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && !ParentObject.OnWorldMap() && !(E.GetParameter("Damage") as Damage).HasAttribute("Unavoidable") && ParentObject.FireEvent("CheckRealityDistortionUsability"))
		{
			DidX("blink", "away from the danger");
			ParentObject.RandomTeleport(Swirl: true);
		}
		return base.FireEvent(E);
	}
}
