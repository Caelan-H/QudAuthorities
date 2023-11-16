using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Salve_Tonic_Applicator : IPart
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
			double num = E.Actor.Health();
			if (E.ForPermission)
			{
				if (num < 1.0)
				{
					E.ApplyScore(1);
				}
			}
			else if (num < 0.5)
			{
				E.ApplyScore((int)(5.0 / num));
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
			GameObject gameObject = E.GetParameter("Target") as GameObject;
			int num = 5;
			gameObject.FireEvent("ApplyingSalve");
			if (!gameObject.ApplyEffect(new Salve_Tonic((int)((float)num * (float)gameObject.GetIntProperty("TonicDurationMultiplier", 100) / 100f))))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
