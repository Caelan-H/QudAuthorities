using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AnchorRoomTile : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.pPhysics != null && gameObjectParameter.HasMarkOfDeath() && !gameObjectParameter.HasEffect("CorpseTethered"))
			{
				gameObjectParameter.ApplyEffect(new CorpseTethered());
				if (gameObjectParameter.IsPlayer())
				{
					gameObjectParameter.ParticleText("Tomb-tethered!", 'G');
				}
			}
		}
		return base.FireEvent(E);
	}
}
