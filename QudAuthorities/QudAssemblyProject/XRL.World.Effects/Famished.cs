using System;
using XRL.World.Capabilities;

namespace XRL.World.Effects;

[Serializable]
public class Famished : Effect
{
	public string mode = "famished";

	public int Penalty;

	public Famished()
	{
		base.Duration = 1;
	}

	public override int GetEffectType()
	{
		return 33554436;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (mode == "wilted")
		{
			return "&Rwilted";
		}
		return "&Rfamished";
	}

	public override string GetDetails()
	{
		if (mode == "wilted")
		{
			return "-5 Quickness\n-10% to natural healing rate";
		}
		return "-10 Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Famished"))
		{
			return true;
		}
		if (Object.FireEvent("ApplyFamished"))
		{
			if (Object.HasPart("PhotosyntheticSkin"))
			{
				mode = "wilted";
				Penalty = 5;
			}
			else
			{
				Penalty = 10;
			}
			base.StatShifter.SetStatShift("Speed", -Penalty);
			Object.RegisterEffectEvent(this, "Regenerating2");
			if (Object.IsPlayer())
			{
				AutoAct.Interrupt();
			}
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Regenerating2" && mode == "wilted")
		{
			E.SetParameter("Amount", E.GetIntParameter("Amount") * 9 / 10);
		}
		return base.FireEvent(E);
	}

	public override void Remove(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Regenerating2");
		base.StatShifter.RemoveStatShifts();
		Penalty = 0;
		base.Remove(Object);
	}
}
