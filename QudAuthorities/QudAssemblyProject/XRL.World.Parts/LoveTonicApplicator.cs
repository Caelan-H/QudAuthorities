using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LoveTonicApplicator : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyTonic");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter.IsPlayer())
			{
				int num = Stat.Random(500, 700);
				gameObjectParameter.ApplyEffect(new LoveTonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
			}
			else
			{
				GameObject gameObject = E.GetGameObjectParameter("Attacker");
				if (gameObject == null)
				{
					if (gameObjectParameter?.CurrentCell != null)
					{
						List<GameObject> list = gameObjectParameter.CurrentCell.FastFloodVisibility("Combat", 20);
						if (list != null && list.Count > 0)
						{
							gameObject = list.GetRandomElement();
						}
					}
					if (gameObject == null)
					{
						gameObject = IComponent<GameObject>.ThePlayer;
					}
				}
				GameObject by = gameObject;
				if (gameObjectParameter.CheckInfluence(base.Name, by))
				{
					if ((95 + gameObject.Stat("Level") - gameObjectParameter.Stat("Level")).in100())
					{
						gameObjectParameter.ApplyEffect(new Lovesick(Stat.Random(3000, 3600), gameObject));
					}
					else
					{
						IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.ShortDisplayName + gameObjectParameter.GetVerb("look") + " you over and" + gameObjectParameter.GetVerb("metabolize") + " the love tonic with no effect.");
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
