using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class BaetylOfferingSifrah : RitualSifrah
{
	public int Difficulty;

	public int Rating;

	public int Performance;

	public bool Abort;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("opening the ceremony", "Items/sw_cyber_reactive_cranial_plating.bmp", "\u000f", "&B", 'y', 1),
		new SifrahSlotConfiguration("cleansing the space", "Items/sw_skin_glitter.bmp", "\u0015", "&K", 'y', 4),
		new SifrahSlotConfiguration("preparing the offering", "Items/sw_shawl.bmp", "\n", "&Y", 'W', 3),
		new SifrahSlotConfiguration("performing the offering", "Items/sw_urn_1.bmp", "é", "&Y", 'B'),
		new SifrahSlotConfiguration("honoring the space", "Items/ms_face_heart.png", "§", "&y", 'M', 5),
		new SifrahSlotConfiguration("closing the ceremony", "Items/sw_orb.bmp", "\a", "&B", 'b', 2)
	};

	public BaetylOfferingSifrah(GameObject ContextObject, int Rating, int Difficulty)
	{
		Description = "Making an offering to " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		this.Difficulty = Difficulty;
		this.Rating = Rating;
		string primaryFaction = ContextObject.GetPrimaryFaction();
		int num = ((!string.IsNullOrEmpty(primaryFaction)) ? The.Game.PlayerReputation.getLevel(primaryFaction) : 0);
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		GetRitualSifrahSetupEvent.GetFor(The.Player, ContextObject, "BaetylOffering", Interruptable: false, ref Difficulty, ref Rating, ref MaxTurns, ref Interrupt, ref PsychometryApplied);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		int num2 = Math.Max(Difficulty * 5 / 4, 3);
		int num3 = 4;
		if (Difficulty >= 4)
		{
			num3++;
		}
		if (Difficulty >= 7)
		{
			num3++;
		}
		switch (num)
		{
		case -1:
			num2++;
			break;
		case -2:
			num2 += 2;
			num3++;
			MaxTurns--;
			break;
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
		if (list.Count < num2 && Rating >= (The.Player.IsTrueKin() ? (Difficulty / 2) : Difficulty))
		{
			list.Add(new RitualSifrahTokenInvokeAncientCompacts());
		}
		if (list.Count < num2 && Rating >= Difficulty * 2)
		{
			list.Add(new SocialSifrahTokenPayACompliment());
		}
		switch (num)
		{
		case 2:
			MaxTurns++;
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
		if (list.Count < num2 && The.Player.HasSkill("Customs_Sharer"))
		{
			list.Add(new RitualSifrahTokenSingAHistoricalEpic());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathBin());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Sed"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathSed());
		}
		int num4 = num2 - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Rating >= Difficulty / 2 && ContextObject.Respires)
			{
				list2.Add(new SocialSifrahTokenHookah());
			}
			list2.Add(new TinkeringSifrahTokenComputePower(Difficulty * Difficulty));
			if (Rating >= Difficulty / 2)
			{
				list.Add(new SocialSifrahTokenReadFromTheCanticlesChromaic());
			}
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Difficulty)
				{
					break;
				}
				list2.Add(new SocialSifrahTokenBit(bitType));
			}
			list2.Add(new SocialSifrahTokenCharge(Difficulty * 2000));
			foreach (string invokableHighlyEntropicBeing in Factions.GetInvokableHighlyEntropicBeings())
			{
				list2.Add(new RitualSifrahTokenInvokeHighlyEntropicBeing(invokableHighlyEntropicBeing));
			}
			if (!The.Player.HasEffect("Lovesick") && The.Player.CanApplyEffect("Lovesick") && The.Player.GetIntProperty("Inorganic") == 0)
			{
				int num5 = Math.Max(10 + (Difficulty - Rating) * 5, 5);
				if (num5 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectLovesick(num5));
				}
			}
			if (!The.Player.HasEffect("Shamed") && The.Player.CanApplyEffect("Shamed"))
			{
				int num6 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShamed(num6));
				}
			}
			if (!The.Player.HasEffect("Dazed") && !The.Player.HasEffect("Stun") && The.Player.CanApplyEffect("Dazed"))
			{
				int num7 = Math.Max(50 + (Difficulty - Rating) * 5, 5);
				if (num7 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDazed(num7));
				}
			}
			if (!The.Player.HasEffect("Shaken") && The.Player.CanApplyEffect("Shaken"))
			{
				int num8 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num8 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShaken(num8));
				}
			}
			if (Rating >= 4 && !The.Player.HasEffect("Exhausted") && The.Player.CanApplyEffect("Exhausted"))
			{
				int num9 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num9 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectExhausted(num9));
				}
			}
			if (Rating >= 5 && !The.Player.HasEffect("Terrified") && The.Player.CanApplyEffect("Terrified", 0, "CanApplyFear"))
			{
				int num10 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num10 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectTerrified(num10));
				}
			}
			if (Rating >= 6 && !The.Player.HasEffect("Asleep") && The.Player.CanApplyEffect("Asleep", 0, "CanApplySleep"))
			{
				int num11 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num11 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectAsleep(num11));
				}
			}
			if (Rating >= 7 && !The.Player.HasEffect("Lost") && The.Player.CanApplyEffect("Lost"))
			{
				int num12 = Math.Max(25 + (Difficulty - Rating) * 10, 5);
				if (num12 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectLost(num12));
				}
			}
			if (Rating >= 2 && !The.Player.HasEffect("Bleeding") && The.Player.CanApplyEffect("Bleeding"))
			{
				int num13 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num13 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectBleeding(num13));
				}
			}
			if (Rating >= 2 && !The.Player.HasEffect("Poisoned") && The.Player.CanApplyEffect("Poisoned", 0, "CanApplyPoison"))
			{
				int num14 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num14 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectPoisoned(num14));
				}
			}
			if (Rating >= 3 && !The.Player.HasEffect("Ill") && !The.Player.HasEffect("Poisoned") && The.Player.CanApplyEffect("Ill"))
			{
				int num15 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num15 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectIll(num15));
				}
			}
			if (Rating >= 5 && !The.Player.HasEffect("CardiacArrest") && The.Player.CanApplyEffect("CardiacArrest"))
			{
				int num16 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num16 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectCardiacArrest(num16));
				}
			}
			if (Rating >= 12 && !The.Player.HasEffect("ShatterMentalArmor") && The.Player.CanApplyEffect("ShatterMentalArmor"))
			{
				int num17 = Math.Max(40 + (Difficulty - Rating) * 5, 5);
				if (num17 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShatterMentalArmor(num17));
				}
			}
			SocialSifrah.AddTokenTokens(list2, ContextObject);
			list2.Add(new SocialSifrahTokenSecret(ContextObject));
			AssignPossibleTokens(list2, list, num4, num2);
		}
		if (num2 > list.Count)
		{
			num2 = list.Count;
		}
		List<SifrahSlot> list3 = (Slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num3, num2));
		Tokens = list;
		if (!AnyUsableTokens(ContextObject))
		{
			Popup.ShowFail("You have no usable options to employ for performing an offering to " + ContextObject.t() + ", giving you no chance of doing so well. You can remedy this situation by improving your Intelligence or by obtaining items useful in such a ritual.");
			Finished = true;
			Abort = true;
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
			switch (Popup.ShowYesNoCancel("Do you want to finish the offering ritual as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish the offering ritual as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
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
		if (Abort)
		{
			return;
		}
		GameOutcome gameOutcome = DetermineOutcome();
		PlayOutcomeSound(gameOutcome);
		switch (gameOutcome)
		{
		case GameOutcome.CriticalFailure:
			ResultCriticalFailure(ContextObject);
			break;
		case GameOutcome.Failure:
			ResultFailure(ContextObject);
			break;
		case GameOutcome.PartialSuccess:
			ResultPartialSuccess(ContextObject);
			break;
		case GameOutcome.Success:
		{
			ResultSuccess(ContextObject);
			int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
			if (base.PercentSolved < 100)
			{
				num2 = num2 * (100 - (100 - base.PercentSolved) * 3) / 100;
			}
			if (num2 > 0)
			{
				The.Player.AwardXP(num2);
			}
			break;
		}
		case GameOutcome.CriticalSuccess:
		{
			ResultExceptionalSuccess(ContextObject);
			RitualSifrah.AwardInsight();
			int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
			if (base.PercentSolved < 100)
			{
				num = num * (100 - (100 - base.PercentSolved) * 3) / 100;
			}
			if (num > 0)
			{
				The.Player.AwardXP(num);
			}
			break;
		}
		}
		SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count);
	}

	public virtual void ResultCriticalFailure(GameObject ContextObject)
	{
		Popup.Show("Your abysmal ritual performance deeply shames you.");
		The.Player.ApplyEffect(new Shamed(Stat.Random(100, 500)));
		Performance = 10;
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Popup.Show("Your performance of the offering ritual was adequate, if barely.");
		Performance = 50;
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the offering ritual was passable.");
		Performance = 100;
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the offering ritual was solemn and respectful.");
		Performance = 150;
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the offering ritual was gracious and dignified.");
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
		double num3 = 0.03 + (double)Difficulty * 0.01 + (double)Powerup * 0.01;
		if (Turn > 1)
		{
			num3 += 0.01 * (double)(MaxTurns - Turn);
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
			Success++;
		}
	}
}
