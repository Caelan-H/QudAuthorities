using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class GelatenousPalmProperties : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DefendMeleeHit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefendMeleeHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			GameObject parentObject = ParentObject;
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
			gameObjectParameter = Disarming.Disarm(parentObject, gameObjectParameter2, 100);
			if (gameObjectParameter != null)
			{
				ParentObject.TakeObject(gameObjectParameter, Silent: true, 0);
				if (gameObjectParameter2.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage((gameObjectParameter.HasProperName ? gameObjectParameter.The : "Your ") + gameObjectParameter.ShortDisplayName + gameObjectParameter.Is + " lost in the goop!");
				}
			}
		}
		return base.FireEvent(E);
	}
}
