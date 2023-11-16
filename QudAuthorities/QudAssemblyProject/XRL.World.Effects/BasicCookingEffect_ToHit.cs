using XRL.UI;

namespace XRL.World.Effects;

public class BasicCookingEffect_ToHit : BasicCookingEffect
{
	public BasicCookingEffect_ToHit()
	{
	}

	public BasicCookingEffect_ToHit(string tastyMessage)
		: this()
	{
		wellFedMessage = tastyMessage;
	}

	public override string GetDetails()
	{
		return "+1 to hit";
	}

	public override void ApplyEffect(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AttackerRollMeleeToHit");
		if (Object.IsPlayer())
		{
			Popup.Show(wellFedMessage + "\n\n{{W|+1 to hit for the rest of the day}}");
		}
	}

	public override void RemoveEffect(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AttackerRollMeleeToHit");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerRollMeleeToHit")
		{
			E.SetParameter("Result", E.GetIntParameter("Result") + 1);
		}
		return base.FireEvent(E);
	}
}
