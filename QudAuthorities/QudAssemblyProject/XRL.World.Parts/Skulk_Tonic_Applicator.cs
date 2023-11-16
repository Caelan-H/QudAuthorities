using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Skulk_Tonic_Applicator : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

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
			int num = 1000 + Stat.Random(1, 200);
			return gameObjectParameter.ApplyEffect(new Skulk_Tonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
		}
		return base.FireEvent(E);
	}
}
