using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Jab : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChanceMultiplier");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerQueryWeaponSecondaryAttackChanceMultiplier")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null && gameObjectParameter.GetWeaponSkill() == "ShortBlades" && (!(E.GetParameter("BodyPart") is BodyPart bodyPart) || bodyPart.Category != 6))
			{
				E.SetParameter("Chance", E.GetIntParameter("Chance") * 2);
			}
		}
		return base.FireEvent(E);
	}
}
