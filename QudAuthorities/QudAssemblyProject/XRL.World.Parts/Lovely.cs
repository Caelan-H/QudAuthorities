using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Lovely : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Looker");
			if (gameObjectParameter != null && !gameObjectParameter.HasEffect("Lovesick"))
			{
				gameObjectParameter.ApplyEffect(new Lovesick(Stat.Random(3000, 3600), ParentObject));
			}
		}
		return base.FireEvent(E);
	}
}
