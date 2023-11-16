using System;

namespace XRL.World.Parts;

[Serializable]
public class HealOnEat : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter != null && gameObjectParameter.HasStat("Hitpoints"))
			{
				if (gameObjectParameter.Statistics["Hitpoints"].Penalty > 0)
				{
					gameObjectParameter.Heal(gameObjectParameter.Statistics["Hitpoints"].Penalty, Message: true, FloatText: true);
				}
				gameObjectParameter.FireEvent("Recuperating");
			}
		}
		return base.FireEvent(E);
	}
}
