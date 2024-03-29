using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Hammer : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "DealDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DealDamage" && (E.GetGameObjectParameter("Weapon")?.GetPart("MeleeWeapon") as MeleeWeapon)?.Skill == "Cudgel" && 2.in100())
		{
			E.GetGameObjectParameter("Defender").Body.GetEquippedParts().GetRandomElement().Equipped.ApplyEffect(new Broken());
		}
		return base.FireEvent(E);
	}
}
