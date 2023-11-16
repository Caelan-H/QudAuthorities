using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Precognition : BaseMutation
{
	public int TurnsLeft;

	public bool RealityDistortionBased = true;

	public new Guid ActivatedAbilityID;

	public Guid RevertActivatedAbilityID;

	public bool WasPlayer;

	public int HitpointsAtSave;

	public int TemperatureAtSave;

	[NonSerialized]
	private long ActivatedSegment;

	public Precognition()
	{
		DisplayName = "Precognition";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeDie");
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "CommandPrecognition");
		Object.RegisterPartEvent(this, "CommandPrecognitionRevert");
		Object.RegisterPartEvent(this, "GameRestored");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You peer into your near future.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You may activate this power and then later revert to the point in time when you activated it.\n" + "Duration between use and reversion: {{rules|" + (12 + 4 * Level) + "}} rounds\n", "Cooldown: 500 rounds");
	}

	public static void Save()
	{
		The.Core.SaveGame("Precognition.sav");
	}

	public static void Load(GameObject obj = null)
	{
		Dictionary<string, object> GameState = GetPrecognitionRestoreGameStateEvent.GetFor(obj);
		GameManager.Instance.gameQueue.queueSingletonTask("PrecognitionEnd", delegate
		{
			XRLGame.LoadGame(The.Game.GetCacheDirectory("Precognition.sav"), ShowPopup: false, GameState);
		});
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (TurnsLeft > 0)
			{
				TurnsLeft--;
				if (TurnsLeft <= 0)
				{
					MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
					if (WasPlayer && ParentObject.IsPlayer() && Popup.ShowYesNo("Your precognition is about to run out. Would you like to return to the start of your vision?") == DialogResult.Yes)
					{
						AutoAct.Interrupt();
						Load(ParentObject);
					}
				}
			}
		}
		else if (E.ID == "AIGetDefensiveMutationList")
		{
			if (TurnsLeft <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && (E.GetIntParameter("Distance") <= 1 || ParentObject.hitpoints < ParentObject.baseHitpoints || ParentObject.Con(null, IgnoreHideCon: true) >= 5) && 70.in100() && ParentObject.FireEvent(Event.New("CheckRealityDistortionAdvisability", "Mutation", this)))
			{
				E.AddAICommand("CommandPrecognition");
			}
		}
		else if (E.ID == "CommandPrecognition")
		{
			if (TurnsLeft > 0)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You are already within a precognitive vision.");
				}
				return false;
			}
			if (RealityDistortionBased && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 500);
			if (ParentObject.IsPlayer())
			{
				Save();
				WasPlayer = true;
			}
			else
			{
				WasPlayer = false;
				if (SensePsychic.SensePsychicFromPlayer(ParentObject) != null)
				{
					IComponent<GameObject>.AddPlayerMessage("You sense a subtle psychic disturbance.");
				}
			}
			HitpointsAtSave = ParentObject.hitpoints;
			TemperatureAtSave = ParentObject.pPhysics.Temperature;
			MyActivatedAbility(RevertActivatedAbilityID).Enabled = true;
			TurnsLeft = 12 + 4 * base.Level + 1;
		}
		else if (E.ID == "CommandPrecognitionRevert")
		{
			if (TurnsLeft > 0)
			{
				if (WasPlayer && ParentObject.IsPlayer())
				{
					Load(ParentObject);
					MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
				}
				else if (ParentObject.IsPlayer())
				{
					Popup.Show("You cannot access someone else's precognitive vision.");
				}
			}
		}
		else
		{
			if (E.ID == "BeforeDie" && TurnsLeft > 0)
			{
				return OnBeforeDie(ParentObject, RevertActivatedAbilityID, ref TurnsLeft, ref HitpointsAtSave, ref TemperatureAtSave, ref ActivatedSegment, WasPlayer, RealityDistortionBased);
			}
			if (E.ID == "GameRestored")
			{
				GenericDeepNotifyEvent.Send(ParentObject, "PrecognitionGameRestored");
			}
		}
		return base.FireEvent(E);
	}

	public static bool OnBeforeDie(GameObject Object, Guid RevertAAID, ref int TurnsLeft, ref int HitpointsAtSave, ref int TemperatureAtSave, ref long ActivatedSegment, bool WasPlayer, bool RealityDistortionBased)
	{
		if (ActivatedSegment >= The.Game.Segments)
		{
			Object.hitpoints = HitpointsAtSave;
			if (Object.pPhysics != null)
			{
				Object.pPhysics.Temperature = TemperatureAtSave;
			}
			return false;
		}
		if (Object.IsPlayer())
		{
			AutoAct.Interrupt();
			if (WasPlayer)
			{
				AchievementManager.SetAchievement("ACH_FORESEE_DEATH");
				if (Popup.ShowYesNo("You sense your imminent demise. Would you like to return to the start of your vision?") == DialogResult.Yes)
				{
					Load(Object);
					ActivatedSegment = The.Game.Segments + 100;
					return false;
				}
			}
		}
		else if (!Object.IsOriginalPlayerBody() && (!RealityDistortionBased || Object.FireEvent("CheckRealityDistortionUsability")))
		{
			TurnsLeft = 0;
			if (RevertAAID != Guid.Empty)
			{
				Object.DisableActivatedAbility(RevertAAID);
			}
			if (Object.HasStat("Hitpoints"))
			{
				ActivatedSegment = The.Game.Segments + 1;
				Object.hitpoints = HitpointsAtSave;
				if (Object.pPhysics != null)
				{
					Object.pPhysics.Temperature = TemperatureAtSave;
				}
				Object.DilationSplat();
				Object.pPhysics.DidX("swim", "before your eyes", "!", null, Object);
				return false;
			}
		}
		return true;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Precognition - Start vision", "CommandPrecognition", "Mental Mutation", null, "!", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		RevertActivatedAbilityID = AddMyActivatedAbility("Precognition - End vision", "CommandPrecognitionRevert", "Mental Mutation", null, "?", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		MyActivatedAbility(RevertActivatedAbilityID).Enabled = false;
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		RemoveMyActivatedAbility(ref RevertActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
