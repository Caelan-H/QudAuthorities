using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GeometricHealOnEat : IPart
{
	public string Amount;

	public string Ratio;

	public string Duration;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

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
			if (gameObjectParameter != null && CanApplyEffectEvent.Check(gameObjectParameter, "GeometricHeal"))
			{
				gameObjectParameter.ApplyEffect(new GeometricHeal(Stat.Roll(Amount), Stat.Roll(Ratio), Stat.Roll(Duration)));
			}
		}
		return base.FireEvent(E);
	}
}
