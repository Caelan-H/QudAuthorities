using System;

namespace XRL.World.Parts;

[Serializable]
public class GreaterVoiderBite : IPart
{
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
			if (gameObjectParameter != null && ParentObject.Equipped != null)
			{
				ParentObject.Equipped.FireEvent(Event.New("GreaterVoiderBiteHit", "Target", gameObjectParameter));
			}
		}
		return true;
	}
}
