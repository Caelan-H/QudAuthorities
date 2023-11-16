using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsychicCombatSifrah : PsionicSifrah
{
	public int Difficulty;

	public int Rating;

	public bool Abort;

	public bool AsMaster;

	public bool Bypass;

	public bool InterfaceExitRequested;

	public GameOutcome Outcome;

	public int Performance;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("clearing the mind", "Items/sw_cherubic.bmp", "\t", "&B", 'K', 2),
		new SifrahSlotConfiguration("defending the psyche", "Items/sw_cyber_reactive_cranial_plating.bmp", "è", "&b", 'm', 1),
		new SifrahSlotConfiguration("marshaling the will to victory", "Items/sw_cyber_medassist_module.bmp", "\u000f", "&y", 'K', 3),
		new SifrahSlotConfiguration("focusing the mind to a single point", "Items/sw_cyber_anchor_spikes.bmp", "\a", "&W", 'B', 5),
		new SifrahSlotConfiguration("charging the destructive intention", "Items/sw_power_cut_large.png", "\u001e", "&M", 'R', 4),
		new SifrahSlotConfiguration("delivering the psychic strike", "Items/sw_esper.bmp", "\u001a", "&W", 'M')
	};

	public PsychicCombatSifrah(GameObject ContextObject, string Subtype, int Rating, int Difficulty, string Description = null)
	{
		base.Description = Description ?? ("engaging in psychic combat with " + ContextObject.an());
		string primaryFaction = ContextObject.GetPrimaryFaction();
		int num = ((!string.IsNullOrEmpty(primaryFaction)) ? The.Game.PlayerReputation.getLevel(primaryFaction) : 0);
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		GetPsionicSifrahSetupEvent.GetFor(The.Player, ContextObject, "PsychicCombat", Subtype, Interruptable: false, ref Difficulty, ref Rating, ref MaxTurns, ref Interrupt, ref PsychometryApplied);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		this.Rating = Rating;
		this.Difficulty = Difficulty;
		int num2 = Math.Max(Difficulty, 3);
		int num3 = 4;
		if (Difficulty >= 5)
		{
			num3++;
		}
		if (Difficulty >= 8)
		{
			num3++;
		}
		if (num3 > slotConfigs.Count)
		{
			num3 = slotConfigs.Count;
		}
		List<SifrahToken> list = new List<SifrahToken>(num2);
		if (Difficulty < 1)
		{
			Difficulty = 1;
		}
		if (Rating < 1)
		{
			Rating = 1;
		}
		if (MaxTurns < 1)
		{
			MaxTurns = 1;
		}
		if (list.Count < num2)
		{
			int num4 = 0;
			foreach (KeyValuePair<string, PowerEntry> item in SkillFactory.Factory.PowersByClass)
			{
				if (item.Key.StartsWith("Discipline_") && The.Player.HasSkill(item.Value.Class))
				{
					num4++;
				}
			}
			if (num4 > 0 && num4 >= Difficulty / 2)
			{
				list.Add(new PsionicSifrahTokenDiscipline());
			}
		}
		if (list.Count < num2 && The.Player.CanMakeTelepathicContactWith(ContextObject))
		{
			list.Add(new PsionicSifrahTokenTelepathy());
		}
		if (list.Count < num2 && The.Player.CanMakeEmpathicContactWith(ContextObject))
		{
			list.Add(new PsionicSifrahTokenEmpathy());
		}
		if (list.Count < num2 && The.Player.Stat("Ego") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenExertWill());
		}
		if (list.Count < num2 && The.Player.Stat("Intelligence") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenApplyIntellect());
		}
		if (list.Count < num2 && The.Player.Stat("Willpower") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenCalmMind());
		}
		if (list.Count < num2 && !The.Player.HasEquippedItem("Cyclopean Prism") && The.Player.Stat("Ego") < 10 + Difficulty * 3)
		{
			list.Add(new PsionicSifrahTokenPrayHumbly());
		}
		if (list.Count < num2 && The.Player.HasSkill("Customs_Sharer") && Rating > Difficulty)
		{
			list.Add(new PsionicSifrahTokenApplyAncientLore());
		}
		if (list.Count < num2 && PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("leverage psychic impressions of opponent's history"));
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Khu") && Rating / 16 + The.Player.Stat("Willpower") - ContextObject.Stat("Willpower") - (ContextObject.HasSkill("TenfoldPath_Khu") ? 10 : 0) > Difficulty * 4)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathKhu());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Yis") && Rating / 16 + The.Player.Stat("Willpower") - ContextObject.Stat("Willpower") - (ContextObject.HasSkill("TenfoldPath_Yis") ? 10 : 0) > Difficulty * 3)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathYis());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Hod") && Rating / 16 + The.Player.Stat("Intelligence") - ContextObject.Stat("Intelligence") - (ContextObject.HasSkill("TenfoldPath_Hod") ? 10 : 0) > Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathHod());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Tza") && Rating / 16 + The.Player.Stat("Ego") - ContextObject.Stat("Ego") - (ContextObject.HasSkill("TenfoldPath_Tza") ? 10 : 0) > Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathTza());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Ret") && Rating / 8 + The.Player.Stat("Willpower") - ContextObject.Stat("Willpower") - (ContextObject.HasSkill("TenfoldPath_Ret") ? 10 : 0) > Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathRet());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Vur") && Rating / 8 + The.Player.Stat("Intelligence") - ContextObject.Stat("Intelligence") - (ContextObject.HasSkill("TenfoldPath_Vur") ? 10 : 0) > Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathVur());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Sed") && Rating / 8 + The.Player.Stat("Ego") - ContextObject.Stat("Ego") - (ContextObject.HasSkill("TenfoldPath_Sed") ? 10 : 0) > Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathSed());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Bin") && Rating / 4 + The.Player.Stat("Intelligence") - ContextObject.Stat("Intelligence") - (ContextObject.HasSkill("TenfoldPath_Bin") ? 10 : 0) > Difficulty / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathBin());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Hok") && Rating / 4 + The.Player.Stat("Ego") - ContextObject.Stat("Ego") - (ContextObject.HasSkill("TenfoldPath_Hok") ? 10 : 0) > Difficulty / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathHok());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Ket") && Rating / 4 + The.Player.Stat("Willpower") - ContextObject.Stat("Willpower") - (ContextObject.HasSkill("TenfoldPath_Ket") ? 10 : 0) > Difficulty / 4)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathKet());
		}
		switch (num)
		{
		case 2:
			if (list.Count < num2)
			{
				list.Add(new SocialSifrahTokenLeverageBeingLoved(primaryFaction));
			}
			break;
		case 1:
			if (list.Count < num2)
			{
				list.Add(new SocialSifrahTokenLeverageBeingFavored(primaryFaction));
			}
			break;
		}
		int num5 = num2 - list.Count;
		if (num5 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			PsionicSifrahTokenCreationKnowledge psionicSifrahTokenCreationKnowledge = new PsionicSifrahTokenCreationKnowledge(ContextObject);
			if (psionicSifrahTokenCreationKnowledge.IsPotentiallyAvailable())
			{
				list2.Add(psionicSifrahTokenCreationKnowledge);
			}
			foreach (string invokableHighlyEntropicBeing in Factions.GetInvokableHighlyEntropicBeings())
			{
				list2.Add(new RitualSifrahTokenInvokeHighlyEntropicBeing(invokableHighlyEntropicBeing));
			}
			if (!The.Player.HasEffect("Lovesick") && The.Player.CanApplyEffect("Lovesick") && The.Player.GetIntProperty("Inorganic") == 0)
			{
				int num6 = Math.Max(10 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectLovesick(num6));
				}
			}
			if (!The.Player.HasEffect("Shamed") && The.Player.CanApplyEffect("Shamed"))
			{
				int num7 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num7 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShamed(num7));
				}
			}
			if (!The.Player.HasEffect("Dazed") && !The.Player.HasEffect("Stun") && The.Player.CanApplyEffect("Dazed"))
			{
				int num8 = Math.Max(50 + (Difficulty - Rating) * 5, 5);
				if (num8 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDazed(num8));
				}
			}
			if (!The.Player.HasEffect("Shaken") && The.Player.CanApplyEffect("Shaken"))
			{
				int num9 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num9 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShaken(num9));
				}
			}
			if (!The.Player.HasEffect("Exhausted") && The.Player.CanApplyEffect("Exhausted"))
			{
				int num10 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num10 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectExhausted(num10));
				}
			}
			if (!The.Player.HasEffect("Terrified") && The.Player.CanApplyEffect("Terrified", 0, "CanApplyFear"))
			{
				int num11 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num11 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectTerrified(num11));
				}
			}
			if (!The.Player.HasEffect("Asleep") && The.Player.CanApplyEffect("Asleep", 0, "CanApplySleep"))
			{
				int num12 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num12 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectAsleep(num12));
				}
			}
			if (Rating >= 5 && !The.Player.HasEffect("Lost") && The.Player.CanApplyEffect("Lost"))
			{
				int num13 = Math.Max(25 + (Difficulty - Rating) * 10, 5);
				if (num13 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectLost(num13));
				}
			}
			if (!The.Player.HasEffect("Nosebleed") && The.Player.CanApplyEffect("Nosebleed") && The.Player.CanApplyEffect("Bleeding"))
			{
				int num14 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num14 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectNosebleed(num14));
				}
			}
			if (!The.Player.HasEffect("CardiacArrest") && The.Player.CanApplyEffect("CardiacArrest"))
			{
				int num15 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num15 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectCardiacArrest(num15));
				}
			}
			if (!The.Player.HasEffect("ShatterMentalArmor") && The.Player.CanApplyEffect("ShatterMentalArmor"))
			{
				int num16 = Math.Max(40 + (Difficulty - Rating) / 2 * 5, 5);
				if (num16 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShatterMentalArmor(num16));
				}
			}
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Ego"));
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Intelligence"));
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Willpower"));
			AssignPossibleTokens(list2, list, num5, num2);
		}
		if (num2 > list.Count)
		{
			num2 = list.Count;
		}
		List<SifrahSlot> list3 = (Slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num3, num2));
		Tokens = list;
		if (!AnyUsableTokens(ContextObject))
		{
			Popup.ShowFail("You have no usable options to employ for " + base.Description + ", giving you no chance of performing well. You can remedy this situation by improving your Ego, Willpower, Intelligence, and esoteric skills.");
			Finished = true;
			Abort = true;
			return;
		}
		if (GetGameCount((GameOutcome?)null, AsMaster: (bool?)true, Slots: (int?)Slots.Count, Tokens: (int?)Tokens.Count, Mastered: (bool?)null) > 0)
		{
			AsMaster = true;
		}
		else
		{
			int gameCount = GetGameCount(null, Slots.Count, Tokens.Count);
			int num17 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.Failure);
			int num18 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.CriticalFailure);
			int num19 = Tokens.Count + num17 / 2 + num18 + gameCount / 10;
			int gameCount2 = GetGameCount(null, Slots.Count, Tokens.Count, null, true);
			int num20 = GetGameCount((GameOutcome?)null, (int?)null, (int?)null, Mastered: (bool?)true, AsMaster: (bool?)false);
			if (gameCount2 + num20 / 100 >= num19)
			{
				AsMaster = true;
			}
		}
		if (!AsMaster)
		{
			return;
		}
		switch (Options.SifrahRealityDistortionAuto)
		{
		case "Always":
			Finished = true;
			Bypass = true;
			return;
		case "Never":
			return;
		}
		DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered psychic combat at this level of difficulty. Do you want to guide the process in detail anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically receive the results of strong but unexceptional performance.");
		if (dialogResult != 0)
		{
			Finished = true;
			if (dialogResult == DialogResult.No)
			{
				Bypass = true;
			}
			else
			{
				Abort = true;
			}
		}
	}

	public override bool CheckEarlyExit(GameObject ContextObject)
	{
		if (Turn == 1)
		{
			CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var _);
			if (Success <= 0 && CriticalSuccess <= 0)
			{
				Abort = true;
				return true;
			}
			switch (Popup.ShowYesNoCancel("Do you want to finish psychic combat as matters stand?"))
			{
			case DialogResult.Yes:
				return true;
			case DialogResult.No:
				Abort = true;
				return true;
			default:
				return false;
			}
		}
		return Popup.ShowYesNo("Exiting now will finish psychic combat as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
	}

	public override bool CheckOutOfOptions(GameObject ContextObject)
	{
		if (Turn > 1)
		{
			Popup.ShowFail("You have no more usable options, so your performance so far will determine the outcome.");
		}
		return true;
	}

	public override void Finish(GameObject ContextObject)
	{
		if (Bypass)
		{
			PlayOutcomeSound(GameOutcome.Success);
			ResultSuccess(ContextObject);
		}
		else
		{
			if (Abort)
			{
				return;
			}
			Outcome = DetermineOutcome();
			PlayOutcomeSound(Outcome);
			bool mastered = false;
			if (Outcome == GameOutcome.CriticalFailure)
			{
				ResultCriticalFailure(ContextObject);
			}
			else if (Outcome == GameOutcome.Failure)
			{
				ResultFailure(ContextObject);
			}
			else if (Outcome == GameOutcome.PartialSuccess)
			{
				ResultPartialSuccess(ContextObject);
			}
			else if (Outcome == GameOutcome.Success)
			{
				if (base.PercentSolved >= 100)
				{
					mastered = true;
				}
				ResultSuccess(ContextObject);
				int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
				if (base.PercentSolved < 100)
				{
					num = num * (100 - (100 - base.PercentSolved) * 3) / 100;
				}
				if (num > 0)
				{
					The.Player.AwardXP(num);
				}
			}
			else if (Outcome == GameOutcome.CriticalSuccess)
			{
				if (base.PercentSolved >= 100)
				{
					mastered = true;
				}
				ResultExceptionalSuccess(ContextObject);
				PsionicSifrah.AwardInsight();
				int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
				if (base.PercentSolved < 100)
				{
					num2 = num2 * (100 - (100 - base.PercentSolved) * 3) / 100;
				}
				if (num2 > 0)
				{
					The.Player.AwardXP(num2);
				}
			}
			SifrahGame.RecordOutcome(this, Outcome, Slots.Count, Tokens.Count, AsMaster, mastered);
		}
	}

	public virtual void ResultCriticalFailure(GameObject ContextObject)
	{
		Performance = -100;
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Performance = 0;
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Performance = 50;
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		Performance = 100;
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
		Performance = 300;
	}

	public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
	{
		double num = (double)(Rating + base.PercentSolved) * 0.01;
		double num2 = num;
		if (num2 < 1.0)
		{
			for (int i = 1; i < Difficulty; i++)
			{
				num2 *= num;
			}
		}
		if (num2 < 0.0)
		{
			num2 = 0.0;
		}
		double num3 = 0.02 + (double)Powerup * 0.01;
		if (Turn > 1)
		{
			num3 += 0.01 * (double)(MaxTurns - Turn);
		}
		if (AsMaster)
		{
			num3 *= 3.0;
		}
		double num4 = num2 * num3;
		if (num2 > 1.0)
		{
			num2 = 1.0;
		}
		double num5 = 1.0 - num2;
		double num6 = num5 * 0.5;
		double num7 = num5 * 0.1;
		num2 -= num4;
		num5 -= num6;
		num5 -= num7;
		Success = (int)(num2 * 100.0);
		Failure = (int)(num5 * 100.0);
		PartialSuccess = (int)(num6 * 100.0);
		CriticalSuccess = (int)(num4 * 100.0);
		CriticalFailure = (int)(num7 * 100.0);
		while (Success + Failure + PartialSuccess + CriticalSuccess + CriticalFailure < 100)
		{
			if (AsMaster)
			{
				CriticalSuccess++;
			}
			else
			{
				PartialSuccess++;
			}
		}
	}

	public void RequestInterfaceExit()
	{
		InterfaceExitRequested = true;
	}
}
