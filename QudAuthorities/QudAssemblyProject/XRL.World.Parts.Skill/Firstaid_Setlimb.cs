using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Firstaid_Setlimb : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandFirstaidSetlimb");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFirstaidSetlimb")
		{
			if (!ParentObject.HasEffect("Cripple"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("You aren't crippled!");
				}
				return true;
			}
			if (ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, 500);
				ParentObject.ApplyEffect(new SettingLimb(5));
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Set Limb", "CommandFirstaidSetlimb", "Skill", null, "\u001c");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}
}
