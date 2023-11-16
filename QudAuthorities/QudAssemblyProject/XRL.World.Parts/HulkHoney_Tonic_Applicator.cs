using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class HulkHoney_Tonic_Applicator : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage == null)
		{
			if (E.Actor.HasEffect("Terrified"))
			{
				E.ApplyScore(100);
			}
			else
			{
				double num = E.Actor.Health();
				if (num < 0.2)
				{
					E.ApplyScore((int)(1.0 / num));
				}
			}
		}
		return base.HandleEvent(E);
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
			int num = Stat.Random(1, 10) + 40;
			return gameObjectParameter.ApplyEffect(new HulkHoney_Tonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
		}
		return base.FireEvent(E);
	}
}
