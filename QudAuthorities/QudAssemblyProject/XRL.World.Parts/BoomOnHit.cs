using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class BoomOnHit : IPart
{
	public int Chance;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter.HasPart("Stomach") && GetSpecialEffectChanceEvent.GetFor(E.GetGameObjectParameter("Attacker"), ParentObject, "Part BoomOnHit Activation", Chance).in100())
			{
				LiquidVolume.getLiquid("neutronflux").Drank(null, 1, gameObjectParameter, new StringBuilder());
			}
		}
		return base.FireEvent(E);
	}
}
