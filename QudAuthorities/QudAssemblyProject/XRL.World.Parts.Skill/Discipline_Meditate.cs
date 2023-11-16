using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline_Meditate : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public int RestCounter;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetEnergyCostEvent.ID)
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && E.Type.Contains("Pass"))
		{
			RestCounter++;
			if (RestCounter >= 10 && !ParentObject.HasEffect("Meditating") && !ParentObject.HasEffect("Asleep") && !ParentObject.HasEffect("Stasis"))
			{
				ParentObject.ApplyEffect(new Meditating(1, FromResting: true));
			}
		}
		else
		{
			RestCounter = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandDisciplineMeditate")
		{
			if (ParentObject.HasEffect("Meditating"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are already meditating!");
				}
				return false;
			}
			int turns = Math.Max(200 - ParentObject.GetIntProperty("Serene") * 40, 5);
			CooldownMyActivatedAbility(ActivatedAbilityID, turns);
			ParentObject.ApplyEffect(new Meditating());
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Meditate", "CommandDisciplineMeditate", "Skill", null, "\u0001");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
