using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Run : IPart
{
	public const string SUPPORT_TYPE = "Run";

	public Guid ActivatedAbilityID = Guid.Empty;

	public string ActiveAbilityName;

	public string ActiveAbilityDescription;

	public string ActiveVerb;

	public string ActiveEffectDisplayName;

	public string ActiveEffectMessageName;

	public int ActiveEffectDuration;

	public bool ActiveSpringingEffective;

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandTakeActionEvent.ID && ID != NeedPartSupportEvent.ID)
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "Run" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = IsRunning();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleRunning" && !ToggleRunning())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList" && E.GetIntParameter("Distance") >= 6 && !string.IsNullOrEmpty(ActiveAbilityName) && !ParentObject.IsFlying && !ParentObject.HasEffectByClass("Running") && ParentObject.CanChangeMovementMode(ActiveEffectMessageName) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.AddAICommand("CommandToggleRunning");
		}
		return base.FireEvent(E);
	}

	public void SyncAbility(bool Silent = false)
	{
		GetRunningBehaviorEvent.Retrieve(ParentObject, out var AbilityName, out var AbilityDescription, out var Verb, out var EffectDisplayName, out var EffectMessageName, out var EffectDuration, out var SpringingEffective);
		if (ActivatedAbilityID == Guid.Empty || AbilityName != ActiveAbilityName || AbilityDescription != ActiveAbilityDescription || Verb != ActiveVerb || EffectDisplayName != ActiveEffectDisplayName || EffectMessageName != ActiveEffectMessageName || EffectDuration != ActiveEffectDuration || SpringingEffective != ActiveSpringingEffective)
		{
			bool flag = ActiveAbilityName == AbilityName;
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			if (!flag)
			{
				ParentObject.RemoveAllEffects("Running");
			}
			ActiveAbilityName = AbilityName;
			ActiveAbilityDescription = AbilityDescription;
			ActiveVerb = Verb;
			ActiveEffectDisplayName = EffectDisplayName;
			ActiveEffectMessageName = EffectMessageName;
			ActiveEffectDuration = EffectDuration;
			ActiveSpringingEffective = SpringingEffective;
			if (!string.IsNullOrEmpty(ActiveAbilityName))
			{
				ActivatedAbilityID = AddMyActivatedAbility(ActiveAbilityName, "CommandToggleRunning", "Maneuvers", ActiveAbilityDescription, "\u001a", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, Silent || flag);
			}
		}
	}

	public static void SyncAbility(GameObject who, bool Silent = false)
	{
		who.GetPart<Run>()?.SyncAbility(Silent);
	}

	public bool ToggleRunning()
	{
		if (!IsRunning())
		{
			return StartRunning();
		}
		return StopRunning();
	}

	public bool IsRunning()
	{
		return ParentObject.HasEffectByClass("Running");
	}

	public bool StartRunning()
	{
		if (IsRunning())
		{
			return false;
		}
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		if (string.IsNullOrEmpty(ActiveAbilityName))
		{
			return false;
		}
		if (ParentObject.OnWorldMap())
		{
			Popup.ShowFail("You cannot " + ActiveVerb + " on the world map.");
			return false;
		}
		if (!ParentObject.CanChangeMovementMode(ActiveEffectMessageName, ShowMessage: true))
		{
			return false;
		}
		ParentObject.ApplyEffect(new Running(ActiveEffectDuration, ActiveEffectDisplayName, ActiveEffectMessageName, ActiveSpringingEffective));
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = true;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, 100, null, "Agility");
		return true;
	}

	public bool StopRunning()
	{
		if (!IsRunning())
		{
			return false;
		}
		ParentObject.RemoveAllEffects("Running");
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = false;
		}
		return true;
	}
}
