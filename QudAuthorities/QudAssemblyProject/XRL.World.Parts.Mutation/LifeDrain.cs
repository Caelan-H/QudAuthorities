using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LifeDrain : BaseMutation
{
	public bool RealityDistortionBased = true;

	public new Guid ActivatedAbilityID;

	public LifeDrain()
	{
		DisplayName = "Syphon Vim";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandLifeDrain");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You bond with a nearby creature and leech its life force.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Mental attack versus an organic creature\n";
		text += "Duration: 20 rounds\n";
		text += "Cooldown: 200 rounds\n";
		return text + "Drains {{rules|" + Level + "}} hit " + ((Level == 1) ? "point" : "points") + " per round";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
				if (gameObjectParameter.IsCombatObject() && (!RealityDistortionBased || (ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)) && gameObjectParameter.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))))
				{
					E.AddAICommand("CommandLifeDrain");
				}
			}
		}
		else if (E.ID == "CommandLifeDrain")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDirection();
			if (cell != null)
			{
				GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, AllowInanimate: false);
				if (combatTarget == ParentObject)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot syphon vim from " + ParentObject.itself + ".");
					}
					return false;
				}
				if (combatTarget == null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("No one is there for you to syphon vim from.");
					}
					return false;
				}
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						return false;
					}
				}
				combatTarget.ApplyEffect(new XRL.World.Effects.LifeDrain(20, base.Level, base.Level.ToString(), ParentObject, RealityDistortionBased));
				UseEnergy(1000, "Mental Mutation");
				CooldownMyActivatedAbility(ActivatedAbilityID, 200);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Life Drain", "CommandLifeDrain", "Mental Mutation", null, "í", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
