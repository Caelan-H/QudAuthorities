using System;

namespace XRL.World.Effects;

[Serializable]
public class Burning : Effect
{
	public Burning()
	{
		base.DisplayName = "{{R|burning}}";
	}

	public Burning(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 33554944;
	}

	public static string GetBurningAmount(GameObject go)
	{
		if (go == null)
		{
			return "1";
		}
		int num = go.pPhysics.Temperature - go.pPhysics.FlameTemperature;
		if (num < 0)
		{
			return "1";
		}
		if (num <= 100)
		{
			return "1";
		}
		if (num <= 300)
		{
			return "1-2";
		}
		if (num <= 500)
		{
			return "2-3";
		}
		if (num <= 700)
		{
			return "3-4";
		}
		if (num <= 900)
		{
			return "4-5";
		}
		return "5-6";
	}

	public override string GetDetails()
	{
		return GetBurningAmount(base.Object) + " damage per turn.";
	}

	public override bool CanApplyToStack()
	{
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		return true;
	}
}
