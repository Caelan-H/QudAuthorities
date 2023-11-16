using System;

namespace XRL.World.Effects;

[Serializable]
public class StingerPoisoned : Poisoned
{
	public StingerPoisoned()
	{
	}

	public StingerPoisoned(int Duration, string DamageIncrement, int Level, GameObject Owner = null)
		: base(Duration, DamageIncrement, Level, Owner)
	{
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.GetEffect("StingerPoisoned") is StingerPoisoned stingerPoisoned)
		{
			stingerPoisoned.Duration = (stingerPoisoned.Duration + base.Duration) / 2;
			stingerPoisoned.DamageIncrement = Convert.ToInt32(stingerPoisoned.DamageIncrement.Split('d')[0]) + Convert.ToInt32(DamageIncrement.Split('d')[0]) + "d2";
			stingerPoisoned.Level = (stingerPoisoned.Level + Level) / 2;
			return false;
		}
		if (Object.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Object, "Poison", this))
		{
			DidX("have", "been poisoned", "!", null, null, Object);
			return true;
		}
		return false;
	}
}
