using System;

namespace XRL.World.Parts;

[Serializable]
public class RefreshCooldownsOnEat : IPart
{
	public bool Controlled;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			ActivatedAbilities activatedAbilities = E.GetGameObjectParameter("Eater")?.ActivatedAbilities;
			if (activatedAbilities != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Class.Contains("Mental") && value.Cooldown > 0)
					{
						value.Cooldown = 0;
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
