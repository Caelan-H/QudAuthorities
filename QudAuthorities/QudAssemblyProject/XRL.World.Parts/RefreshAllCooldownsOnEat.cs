using System;

namespace XRL.World.Parts;

[Serializable]
public class RefreshAllCooldownsOnEat : IPart
{
	public int ChancePerAbility = 25;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		Object.RegisterPartEvent(this, "GetShortDescription");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			ActivatedAbilities activatedAbilities = gameObjectParameter?.ActivatedAbilities;
			if (activatedAbilities != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Cooldown > 0 && ChancePerAbility.in100())
					{
						value.Cooldown = 0;
						if (gameObjectParameter.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You suddenly feel ready to use " + value.DisplayName + " again.");
						}
					}
				}
			}
		}
		else if (E.ID == "GetShortDescription")
		{
			E.AddParameter("Postfix", E.GetStringParameter("Postfix") + "\n&CWhen eaten, there's a " + ChancePerAbility + "% chance that each activated ability's cooldown is refreshed.");
		}
		return base.FireEvent(E);
	}
}
