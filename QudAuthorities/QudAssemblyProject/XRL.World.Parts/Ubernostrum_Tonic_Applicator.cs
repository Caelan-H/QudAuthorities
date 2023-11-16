using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Ubernostrum_Tonic_Applicator : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

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
			Body body = E.Actor.Body;
			if (body != null && body.FindRegenerablePart() != null)
			{
				E.ApplyScore(80);
			}
			else
			{
				double num = E.Actor.Health();
				if (E.ForPermission)
				{
					if (num < 1.0)
					{
						E.ApplyScore(1);
					}
				}
				else if (num < 0.1)
				{
					E.ApplyScore((int)(0.6 / num));
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
			int num = 10;
			gameObjectParameter.FireEvent("ApplyingUbernostrum");
			return gameObjectParameter.ApplyEffect(new Ubernostrum_Tonic((int)((float)num * (float)gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100f)));
		}
		return base.FireEvent(E);
	}
}
