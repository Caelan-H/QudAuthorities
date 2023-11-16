using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainLove_BeguilingCharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public int AppliedBonus;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(7, 8);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they beguile a creature as per Beguiling at rank 7-8 for the duration of this effect.";
	}

	public override string GetNotification()
	{
		return "@they feel the swell of love inside.";
	}

	public override void Apply(GameObject go)
	{
		if (go.IsPlayer())
		{
			go.ModIntProperty("MaxBeguiledBonus", 1);
			if (Beguiling.Cast(go, null, null, Tier))
			{
				AppliedBonus++;
			}
			else
			{
				go.ModIntProperty("MaxBeguiledBonus", -1, RemoveIfZero: true);
			}
		}
	}

	public override void Remove(GameObject go)
	{
		go.ModIntProperty("MaxBeguiledBonus", -AppliedBonus, RemoveIfZero: true);
		Beguiling.SyncTarget(go);
		base.Remove(go);
	}
}
