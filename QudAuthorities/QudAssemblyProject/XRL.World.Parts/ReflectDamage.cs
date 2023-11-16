using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ReflectDamage : IPart
{
	public int ReflectPercentage = 100;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
			if (damage.Amount > 0 && !damage.HasAttribute("reflected"))
			{
				int num = (int)((float)damage.Amount * ((float)ReflectPercentage / 100f));
				if (ReflectPercentage > 0 && num == 0)
				{
					num = 1;
				}
				if (num > 0 && gameObjectParameter != null && gameObjectParameter != ParentObject)
				{
					Event @event = new Event("TakeDamage");
					Damage damage2 = new Damage(num);
					damage2.Attributes = new List<string>(damage.Attributes);
					if (!damage2.HasAttribute("reflected"))
					{
						damage2.Attributes.Add("reflected");
					}
					@event.AddParameter("Damage", damage2);
					@event.AddParameter("Owner", ParentObject);
					@event.AddParameter("Attacker", ParentObject);
					@event.AddParameter("Message", "from %t damage reflection!");
					if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("&GYou reflect " + num + " damage back at " + gameObjectParameter.the + gameObjectParameter.ShortDisplayName + "&G.");
					}
					else if (gameObjectParameter.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("&R" + ParentObject.The + ParentObject.ShortDisplayName + "&R" + ParentObject.GetVerb("reflect") + " " + num + " damage back at you.");
					}
					else if (Visible())
					{
						IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + "&y" + ParentObject.GetVerb("reflect") + " " + num + " damage back at " + gameObjectParameter.the + gameObjectParameter.ShortDisplayName + "&y.");
					}
					gameObjectParameter.FireEvent(@event);
					ParentObject.FireEvent("ReflectedDamage");
				}
			}
		}
		return base.FireEvent(E);
	}
}
