using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_ShatteringBlows : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DealDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DealDamage" && E.GetGameObjectParameter("Weapon")?.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.Skill == "Cudgel" && 10.in100())
		{
			E.GetGameObjectParameter("Defender").ApplyEffect(new ShatterArmor(2000));
		}
		return base.FireEvent(E);
	}
}
