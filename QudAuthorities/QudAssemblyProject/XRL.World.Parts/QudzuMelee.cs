using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class QudzuMelee : IPart
{
	public int Chance = 15;

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
			if (Chance.in100())
			{
				List<GameObject> list = Event.NewGameObjectList();
				gameObjectParameter.GetInventoryAndEquipment(list);
				if (list.Count > 0)
				{
					list.GetRandomElement().ApplyEffect(new Rusted(1));
				}
			}
			if (!gameObjectParameter.IsHostileTowards(ParentObject))
			{
				gameObjectParameter.GetAngryAt(ParentObject);
			}
		}
		return base.FireEvent(E);
	}
}
