using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_InspiringPresence : BaseSkill
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "MinionTakingAction");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "MinionTakingAction")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			int amount = ParentObject.StatMod("Ego") * 4;
			gameObjectParameter.ApplyEffect(new Emboldened(5, "Hitpoints", amount));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.IsPlayer())
		{
			SocialSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
