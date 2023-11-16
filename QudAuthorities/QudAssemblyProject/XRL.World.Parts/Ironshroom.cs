using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Ironshroom : IPart
{
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
			if (gameObjectParameter != null && gameObjectParameter.HasPart("Combat"))
			{
				if (Visible())
				{
					if (gameObjectParameter.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are impaled by " + ParentObject.the + ParentObject.DisplayName + "&y!");
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.DisplayName + gameObjectParameter.GetVerb("are") + " impaled by " + ParentObject.a + ParentObject.DisplayName + ".");
					}
				}
				if (gameObjectParameter.TakeDamage(Stat.Random(1, 10), "from %o impalement.", null, null, null, null, ParentObject))
				{
					gameObjectParameter.Bloodsplatter();
					gameObjectParameter.ApplyEffect(new Bleeding("1d3", 30, ParentObject, Stack: false));
				}
			}
		}
		return base.FireEvent(E);
	}
}
