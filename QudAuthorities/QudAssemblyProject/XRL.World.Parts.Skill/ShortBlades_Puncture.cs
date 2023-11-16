using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Puncture : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "GetAttackerHitDice");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetAttackerHitDice")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
			if (gameObjectParameter != null && (gameObjectParameter.GetPart("MeleeWeapon") as MeleeWeapon)?.Skill == "ShortBlades")
			{
				E.SetParameter("AV", E.GetIntParameter("AV") - 2);
			}
		}
		return base.FireEvent(E);
	}
}
