using System;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Firstaid_StaunchWounds : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandStaunchWounds");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandStaunchWounds")
		{
			if (ParentObject.AreHostilesNearby())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't staunch wounds with hostiles nearby!");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			if (ParentObject.HasEffect("Bleeding"))
			{
				int num = 1;
				string text = ((num == 1) ? "a bandage" : (Grammar.Cardinal(num) + " bandages"));
				if (!ParentObject.HasObjectInInventory("Bandage", num))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You need " + text + " to do that!");
					}
					return true;
				}
				for (int i = 0; i < num; i++)
				{
					ParentObject.UseObject("Bandage");
				}
				if (ParentObject.HasPart("Hemophilia"))
				{
					Bleeding bleeding = ParentObject.GetEffect("Bleeding") as Bleeding;
					if (bleeding.SaveTarget <= 20)
					{
						ParentObject.RemoveEffect("Bleeding");
						Popup.Show("You staunch the bleeding with " + text + ".");
					}
					else
					{
						bleeding.SaveTarget -= 20;
						Popup.Show("You try to staunch the wound with " + text + ", but you're still bleeding.");
					}
				}
				else
				{
					ParentObject.RemoveEffect("Bleeding");
					Popup.Show("You staunch the bleeding with " + text + ".");
				}
				ParentObject.UseEnergy(1000, "Physical Skill");
			}
			else
			{
				Popup.ShowFail("You aren't bleeding.");
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Staunch Wounds", "CommandStaunchWounds", "Skill", null, "+");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.AddSkill(GO);
	}
}
