using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class Lovesick : Effect
{
	private int Penalty = 6;

	private int SpeedPenalty = 5;

	private int SecondDuration;

	public GameObject Beauty;

	public GameObject PreviousLeader;

	public Lovesick()
	{
		base.DisplayName = "{{lovesickness|lovesick}}";
	}

	public Lovesick(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Lovesick(int Duration, GameObject Beauty)
		: this(Duration)
	{
		this.Beauty = Beauty;
	}

	public override int GetEffectType()
	{
		return 100663298;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "-6 Intelligence\n-6 Willpower\n-5 Move Speed";
	}

	public override string GetDescription()
	{
		return base.DisplayName;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Lovesick"))
		{
			return false;
		}
		if (Object.GetIntProperty("Inorganic") != 0)
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyLovesick", "Duration", base.Duration)))
		{
			return false;
		}
		if (Beauty != null)
		{
			Object.StopFighting(Beauty);
			Beauty.StopFighting(Object);
			if (Object.IsPlayer())
			{
				if (Beauty.IsPlayer())
				{
					AchievementManager.SetAchievement("ACH_LOVE_YOURSELF");
				}
				if (Beauty.GetBlueprint().InheritsFrom("Sign"))
				{
					AchievementManager.SetAchievement("ACH_LOVE_SIGN");
				}
				JournalAPI.AddAccomplishment("Your heart sang at the sight of " + Beauty.an() + ".", "The troubadour-hero =name= rode the tides of " + The.Player.GetPronounProvider().PossessiveAdjective + " passions and shipwrecked on the shores of " + Beauty.an() + ".", "general", JournalAccomplishment.MuralCategory.CommitsFolly, JournalAccomplishment.MuralWeight.High, null, -1L);
			}
			else if (Object.pBrain != null)
			{
				PreviousLeader = Object.pBrain.PartyLeader;
				Object.SetPartyLeader(Beauty, takeOnAttitudesOfLeader: false);
			}
			DidXToY("fall", "in love with", Beauty, null, "!", null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			Beauty = Beauty?.DeepCopy();
		}
		ApplyStats();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		GameObject.validate(ref PreviousLeader);
		if (Object.pBrain != null && PreviousLeader != Object.pBrain.PartyLeader && Object.FireEvent(Event.New("CanRestorePartyLeader", "PreviousLeader", PreviousLeader)))
		{
			GameObject partyLeader = Object.PartyLeader;
			if (partyLeader != null && partyLeader.FireEvent(Event.New("CanCompanionRestorePartyLeader", "Companion", Object, "PreviousLeader", PreviousLeader)))
			{
				Object.SetPartyLeader(PreviousLeader, takeOnAttitudesOfLeader: false);
			}
		}
		UnapplyStats();
		if (Object.IsPlayer() && GameObject.validate(ref Beauty))
		{
			JournalAPI.AddAccomplishment("Though your affection for " + Beauty.an() + " endowed you with a certain wisdom, you no longer feel the tug on your heartstrings.", "The call of the sea rang through the troubadour-hero =name= once again, and " + The.Player.GetPronounProvider().Subjective + " set sail from the familiar shore of " + Beauty.an() + ".", "general", JournalAccomplishment.MuralCategory.HasInspiringExperience, JournalAccomplishment.MuralWeight.High, null, -1L);
		}
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "Intelligence", -Penalty);
		base.StatShifter.SetStatShift(base.Object, "Willpower", -Penalty);
		base.StatShifter.SetStatShift(base.Object, "MoveSpeed", SpeedPenalty);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.validate(ref PreviousLeader);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (base.Duration > 0)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 5 && num < 10)
			{
				E.Tile = null;
				E.RenderString = "\u0003";
				if (SecondDuration > 0)
				{
					E.ColorString = "&R";
				}
				else
				{
					E.ColorString = "&R";
				}
			}
		}
		return true;
	}
}
