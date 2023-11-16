using System;
using System.Text;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Run : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetRunningBehaviorEvent.ID)
		{
			return ID == PartSupportEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Run")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRunningBehaviorEvent E)
	{
		if (E.Priority < 10)
		{
			int @for = GetSprintDurationEvent.GetFor(E.Actor, 10);
			if (@for > 0)
			{
				E.AbilityName = "Sprint";
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("Run faster for ").Append(@for.Things("turn")).Append('.');
				if (!E.Actor.HasSkill("Tactics_Hurdle"))
				{
					stringBuilder.Compound("-5 DV.", ' ');
				}
				if (!Running.IsEnhanced(E.Actor))
				{
					if (E.Actor.HasSkill("Pistol_SlingAndRun"))
					{
						stringBuilder.Compound("Reduced accuracy with missile weapons, except pistols.", ' ');
					}
					else
					{
						stringBuilder.Compound("Reduced accuracy with missile weapons.", ' ');
					}
					stringBuilder.Compound("-10 to hit in melee combat.", ' ').Compound("Is ended by attacking in melee, by effects that interfere with movement, and by most other actions that have action costs, other than using physical mutations.", ' ');
				}
				E.AbilityDescription = stringBuilder.ToString();
				E.Verb = "sprint";
				E.EffectDisplayName = "sprinting";
				E.EffectMessageName = "sprinting";
				E.EffectDuration = @for;
				E.SpringingEffective = true;
				E.Priority = 10;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.RequirePart<Run>();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		NeedPartSupportEvent.Send(GO, "Run", this);
		return base.AddSkill(GO);
	}

	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}
}
