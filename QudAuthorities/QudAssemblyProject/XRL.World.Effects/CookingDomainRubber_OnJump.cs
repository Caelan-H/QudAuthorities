namespace XRL.World.Effects;

public class CookingDomainRubber_OnJump : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature jump@s,";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "Jumped");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Jumped");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Jumped")
		{
			string stringParameter = E.GetStringParameter("SourceKey");
			if (stringParameter == null || !stringParameter.Contains("CookingDomainRubber"))
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
