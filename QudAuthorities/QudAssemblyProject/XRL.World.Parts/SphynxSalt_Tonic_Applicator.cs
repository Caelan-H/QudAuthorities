using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SphynxSalt_Tonic_Applicator : IPart
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyTonic");
		Object.RegisterPartEvent(this, "GameRestored");
		base.Register(Object);
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
		if (E.ForPermission)
		{
			E.ApplyScore(1);
		}
		else if (E.Damage != null)
		{
			if (E.Damage.Amount >= E.Actor.baseHitpoints * 2 / 3)
			{
				E.ApplyScore(8 * E.Damage.Amount / E.Actor.baseHitpoints);
			}
			else if (E.Damage.Amount >= E.Actor.hitpoints * 3 / 4)
			{
				E.ApplyScore(8 * E.Damage.Amount / E.Actor.hitpoints);
			}
		}
		else if (E.Actor.HasEffect("Confused"))
		{
			E.ApplyScore(100);
		}
		else
		{
			double num = E.Actor.Health();
			if (num < 0.1)
			{
				E.ApplyScore((int)(0.7 / num));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			int num = Stat.Random(18, 22);
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Owner");
			if (gameObjectParameter2 != null && !gameObjectParameter2.IsPlayer() && IComponent<GameObject>.Visible(gameObjectParameter2) && !E.HasFlag("External"))
			{
				IComponent<GameObject>.AddPlayerMessage(gameObjectParameter2.The + gameObjectParameter2.ShortDisplayName + gameObjectParameter2.GetVerb("apply") + " " + ParentObject.a + ParentObject.ShortDisplayNameSingle + ".");
			}
			ParentObject.Destroy();
			gameObjectParameter.ApplyEffect(new SphynxSalt_Tonic(num * gameObjectParameter.GetIntProperty("TonicDurationMultiplier", 100) / 100));
			return false;
		}
		return base.FireEvent(E);
	}
}
