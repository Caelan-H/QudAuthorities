using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class TenfoldPath_Vur : BaseSkill
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ObjectAttacking");
		Object.RegisterPartEvent(this, "TargetedForMissileWeapon");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectAttacking" || E.ID == "TargetedForMissileWeapon")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			if (gameObjectParameter != null && gameObjectParameter.FireEvent("CanApplyFear") && gameObjectParameter.FireEvent("ApplyFear") && !gameObjectParameter.MakeSave("Willpower", 15, null, null, "Vur Counteraggression Fear"))
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You cannot bring yourself to attack " + ParentObject.t() + ".", 'r');
				}
				gameObjectParameter.UseEnergy(1000, "Attempted Attack");
				return false;
			}
		}
		return base.FireEvent(E);
	}
}
