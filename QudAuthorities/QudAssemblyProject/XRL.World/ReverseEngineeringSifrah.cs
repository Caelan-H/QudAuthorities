using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class ReverseEngineeringSifrah : TinkeringSifrah
{
	public int Complexity;

	public int Difficulty;

	public int ReverseEngineerRating;

	public TinkerData LearnData;

	public bool Abort;

	public bool InterfaceExitRequested;

	public bool CriticallyFailed;

	public bool Succeeded;

	public bool Critical;

	public int XP;

	public int Mods;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("sketching gearworks", "Items/sw_gears2.bmp", "é", "&c", 'c'),
		new SifrahSlotConfiguration("diagramming spark vines", "Items/sw_copper_wire.bmp", "í", "&W", 'G'),
		new SifrahSlotConfiguration("measuring humour flows", "Items/sw_sparktick_plasma.bmp", "÷", "&Y", 'y'),
		new SifrahSlotConfiguration("mapping glow modules", "Items/sw_lens.bmp", "\u001f", "&B", 'b'),
		new SifrahSlotConfiguration("summoning spirits", "Items/sw_computer.bmp", "Ñ", "&c", 'G'),
		new SifrahSlotConfiguration("communing with the beyond", "Items/sw_wind_turbine_3.bmp", "ì", "&m", 'K')
	};

	private static string[] status = new string[5];

	public ReverseEngineeringSifrah(GameObject ContextObject, int Complexity, int Difficulty, int ReverseEngineerRating, TinkerData LearnData)
	{
		Description = "Reverse engineering " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false);
		this.Complexity = Complexity;
		this.Difficulty = Difficulty;
		this.ReverseEngineerRating = ReverseEngineerRating;
		this.LearnData = LearnData;
		int num = 4 + Complexity + Difficulty;
		int num2 = 4;
		if (Complexity >= 3)
		{
			num2++;
		}
		if (Complexity >= 7)
		{
			num2++;
			num += 4;
		}
		if (num2 > slotConfigs.Count)
		{
			num2 = slotConfigs.Count;
		}
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		int @for = GetTinkeringBonusEvent.GetFor(The.Player, ContextObject, "ReverseEngineerTurns", MaxTurns, 0, ref Interrupt, ref PsychometryApplied, Interruptable: false);
		MaxTurns += @for;
		List<SifrahToken> list = new List<SifrahToken>(num);
		if (list.Count < num && (Complexity < 3 || ReverseEngineerRating > 20 + Complexity * 3) && !The.Player.HasPart("Myopia"))
		{
			list.Add(new TinkeringSifrahTokenVisualInspection());
		}
		if (list.Count < num && (Complexity < 4 || ReverseEngineerRating > 10 + Complexity * 2))
		{
			list.Add(new TinkeringSifrahTokenPhysicalManipulation());
		}
		if (list.Count < num)
		{
			Scanning.Scan scan = Scanning.GetScanTypeFor(ContextObject);
			if (scan == Scanning.Scan.Structure)
			{
				scan = Scanning.Scan.Tech;
			}
			if (Scanning.HasScanningFor(The.Player, scan))
			{
				list.Add(new TinkeringSifrahTokenScanning(scan));
			}
		}
		if (list.Count < num && PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("read psychic impressions of manufacturing process"));
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new TinkeringSifrahTokenTenfoldPathBin());
		}
		if (list.Count < num && The.Player.HasPart("Telekinesis") && (Complexity < 4 || ReverseEngineerRating > Difficulty + Complexity * 2))
		{
			list.Add(new TinkeringSifrahTokenTelekinesis());
		}
		int num3 = num - list.Count;
		if (num3 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Complexity < 5 || ReverseEngineerRating > 5 + Complexity * 3)
			{
				list2.Add(new TinkeringSifrahTokenToolkit());
			}
			list2.Add(new TinkeringSifrahTokenComputePower(Complexity + Difficulty));
			list2.Add(new TinkeringSifrahTokenCharge(Math.Min(1000 * (Complexity + Difficulty), 10000)));
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Complexity)
				{
					break;
				}
				list2.Add(new TinkeringSifrahTokenBit(bitType));
			}
			list2.Add(new TinkeringSifrahTokenLiquid("brainbrine"));
			list2.Add(new TinkeringSifrahTokenLiquid("ink"));
			list2.Add(new TinkeringSifrahTokenLiquid("lava"));
			list2.Add(new TinkeringSifrahTokenLiquid("oil"));
			list2.Add(new TinkeringSifrahTokenLiquid("wax"));
			list2.Add(new TinkeringSifrahTokenCopperWire());
			AssignPossibleTokens(list2, list, num3, num);
		}
		if (num > list.Count)
		{
			num = list.Count;
		}
		List<SifrahSlot> list3 = (Slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num2, num));
		Tokens = list;
		if (!AnyUsableTokens(ContextObject))
		{
			Popup.ShowFail("You have no usable options to employ for reverse engineering " + ContextObject.t() + ", giving you no chance of success. You can remedy this situation by improving your Intelligence and tinkering skills, or by obtaining items useful for tinkering.");
			Finished = true;
			Abort = true;
		}
	}

	public override bool CheckEarlyExit(GameObject ContextObject)
	{
		return Popup.ShowYesNo("Exiting will still disassemble " + ContextObject.t() + ", and will result in an attempt at reverse engineering as matters stand. Do you still want to exit?") == DialogResult.Yes;
	}

	public override bool CheckOutOfOptions(GameObject ContextObject)
	{
		Popup.ShowFail("You have no more usable options.");
		return true;
	}

	public override void Finish(GameObject ContextObject)
	{
		if (base.PercentSolved == 100)
		{
			ContextObject.ModIntProperty("ItemNamingBonus", 1);
		}
		if (Abort)
		{
			return;
		}
		GameOutcome gameOutcome = DetermineOutcome();
		PlayOutcomeSound(gameOutcome);
		switch (gameOutcome)
		{
		case GameOutcome.CriticalFailure:
			Succeeded = false;
			Critical = true;
			Popup.Show("You think you've made a terrible mistake...");
			if (The.Player.HasPart("Dystechnia"))
			{
				Dystechnia.CauseExplosion(ContextObject, The.Player);
			}
			else
			{
				The.Player.Sparksplatter();
				The.Player.TakeDamage(Complexity + Difficulty + Stat.Random(1, (Complexity + Difficulty) * 4), "from an electrical discharge!", "Electric", null, null, The.Player, ContextObject);
				The.Player.TakeDamage(Complexity + Difficulty / 2 + Stat.Random(1, (Complexity + Difficulty) * 2), "from a sharp edge!", "Cutting", null, null, The.Player, ContextObject);
				The.Player.ApplyEffect(new Stun(Stat.Random(5, 10), 20));
			}
			RequestInterfaceExit();
			break;
		case GameOutcome.Failure:
			Succeeded = false;
			Critical = false;
			Popup.Show("You fail to reverse engineer " + ContextObject.t() + ".");
			break;
		case GameOutcome.PartialSuccess:
			Succeeded = true;
			Critical = false;
			XP = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty);
			if (base.PercentSolved < 100)
			{
				XP = XP * (100 - (100 - base.PercentSolved) * 3) / 100;
			}
			break;
		case GameOutcome.Success:
			Succeeded = true;
			Critical = false;
			Mods = 1;
			XP = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 3 / 2;
			if (base.PercentSolved < 100)
			{
				XP = XP * (100 - (100 - base.PercentSolved) * 3) / 100;
			}
			break;
		case GameOutcome.CriticalSuccess:
			Succeeded = true;
			Critical = true;
			Mods = 3;
			XP = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 5;
			if (base.PercentSolved < 100)
			{
				XP = XP * (100 - (100 - base.PercentSolved) * 3) / 100;
			}
			break;
		}
		SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count);
	}

	public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
	{
		double num = (double)(ReverseEngineerRating / 2 + base.PercentSolved) * 0.009;
		double num2 = num;
		if (num2 < 1.0)
		{
			for (int i = 1; i < Complexity; i++)
			{
				num2 *= num;
			}
		}
		if (num2 < 0.0)
		{
			num2 = 0.0;
		}
		double num3 = 0.034 + (double)Powerup * 0.04;
		if (Turn > 1)
		{
			num3 += 0.037 * (double)(MaxTurns - Turn);
		}
		double num4 = num2 * num3;
		if (num2 > 1.0)
		{
			num2 = 1.0;
		}
		double num5 = 1.0 - num2;
		double num6 = num2 * 0.3;
		double num7 = num5 * 0.1;
		num2 -= num4;
		num2 -= num6;
		num5 -= num7;
		Success = (int)(num2 * 100.0);
		PartialSuccess = (int)(num6 * 100.0);
		Failure = (int)(num5 * 100.0);
		CriticalSuccess = (int)(num4 * 100.0);
		CriticalFailure = (int)(num7 * 100.0);
		while (Success + Failure + PartialSuccess + CriticalSuccess + CriticalFailure < 100)
		{
			PartialSuccess++;
		}
	}

	public void RequestInterfaceExit()
	{
		InterfaceExitRequested = true;
	}
}
