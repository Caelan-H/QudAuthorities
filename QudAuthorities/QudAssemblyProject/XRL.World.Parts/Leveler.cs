using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Leveler : IPart
{
	[NonSerialized]
	public static bool PlayerLedPrompt;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AwardedXPEvent.ID)
		{
			return ID == StatChangeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardedXPEvent E)
	{
		int newValue = E.Actor.Stat("XP");
		while (valuePassed(E.AmountBefore, newValue, GetXPForLevel(ParentObject.Stat("Level") + 1)))
		{
			LevelUp(E.Kill, E.InfluencedBy);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Toughness")
		{
			int scoreModifier = Stat.GetScoreModifier(E.OldValue);
			int scoreModifier2 = Stat.GetScoreModifier(E.NewValue);
			int num = E.NewValue - E.OldValue + (scoreModifier2 - scoreModifier) * (ParentObject.Stat("Level") - 1);
			if (num != 0)
			{
				Statistic stat = ParentObject.GetStat("Hitpoints");
				if (stat != null)
				{
					int baseValue = stat.BaseValue;
					if (num > 0)
					{
						stat.BaseValue += num;
					}
					if (stat.Penalty != 0)
					{
						int num2 = baseValue + num;
						double num3 = (double)stat.Penalty / (double)baseValue;
						int num5 = (stat.Penalty = (int)Math.Min(Math.Round((double)num2 * num3, MidpointRounding.AwayFromZero), num2 - 1));
					}
					if (num < 0)
					{
						stat.BaseValue += num;
					}
				}
			}
		}
		else if (E.Name == "Intelligence" && E.Type == "BaseValue")
		{
			int intProperty = ParentObject.GetIntProperty("PeakIntelligence", E.OldValue);
			if (E.NewValue > intProperty && ParentObject.HasStat("SP"))
			{
				int num6 = (E.NewValue - intProperty) * 4 * (ParentObject.Stat("Level") - 1);
				if (num6 > 0)
				{
					ParentObject.GetStat("SP").BaseValue += num6;
				}
				if (E.OldValue != 0 && E.NewValue > intProperty)
				{
					ParentObject.SetIntProperty("PeakIntelligence", E.NewValue);
				}
			}
		}
		return base.HandleEvent(E);
	}

	private bool valuePassed(int oldValue, int newValue, int threshold)
	{
		if (oldValue < threshold)
		{
			return newValue >= threshold;
		}
		return false;
	}

	public static int GetXPForLevel(int L)
	{
		if (L <= 1)
		{
			return 0;
		}
		return (int)(Math.Floor(Math.Pow(L, 3.0) * 15.0) + 100.0);
	}

	public void GetEntryDice(out string BaseHPGain, out string BaseSPGain, out string BaseMPGain)
	{
		GenotypeEntry genotypeEntry = GenotypeFactory.RequireGenotypeEntry(ParentObject.GetGenotype());
		SubtypeEntry subtypeEntry = SubtypeFactory.GetSubtypeEntry(ParentObject.GetSubtype());
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseHPGain))
		{
			BaseHPGain = subtypeEntry.BaseHPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseHPGain))
		{
			BaseHPGain = genotypeEntry.BaseHPGain;
		}
		else
		{
			BaseHPGain = "1-4";
		}
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseSPGain))
		{
			BaseSPGain = subtypeEntry.BaseSPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseSPGain))
		{
			BaseSPGain = genotypeEntry.BaseSPGain;
		}
		else
		{
			BaseSPGain = "50";
		}
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseMPGain))
		{
			BaseMPGain = subtypeEntry.BaseMPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseMPGain))
		{
			BaseMPGain = genotypeEntry.BaseMPGain;
		}
		else
		{
			BaseMPGain = "1";
		}
	}

	public int RollHP(string BaseHPGain)
	{
		return Math.Max(Stat.RollLevelupChoice(BaseHPGain) + ParentObject.StatMod("Toughness"), 1);
	}

	public int RollSP(string BaseSPGain)
	{
		int num = Stat.RollLevelupChoice(BaseSPGain);
		num += (ParentObject.BaseStat("Intelligence") - 10) * 4;
		return GetLevelUpSkillPointsEvent.GetFor(ParentObject, num);
	}

	public int RollMP(string BaseMPGain)
	{
		return Stat.RollLevelupChoice(BaseMPGain);
	}

	public void AppendStatMessage(StringBuilder SB, string Type, int Amount, bool Pluralize = true)
	{
		if (SB != null)
		{
			int num = Math.Abs(Amount);
			SB.CompoundItVerb(ParentObject, (Amount > 0) ? "gain" : "lose", '\n').Append(" {{rules|").Append(num)
				.Append("}} ")
				.Append(Type);
			if (Pluralize && num != 1)
			{
				SB.Append('s');
			}
		}
	}

	public void AddHitpoints(StringBuilder SB, int HPGain)
	{
		if (HPGain != 0)
		{
			ParentObject.GetStat("Hitpoints").BaseValue += HPGain;
			AppendStatMessage(SB, "hitpoint", HPGain);
		}
	}

	public void AddSkillPoints(StringBuilder SB, int SPGain)
	{
		if (SPGain != 0)
		{
			ParentObject.GetStat("SP").BaseValue += SPGain;
			AppendStatMessage(SB, "Skill Point", SPGain);
		}
	}

	public void AddMutationPoints(StringBuilder SB, int MPGain)
	{
		if (MPGain != 0)
		{
			ParentObject.GainMP(MPGain);
			AppendStatMessage(SB, "Mutation Point", MPGain);
		}
	}

	public void AddAttributePoints(StringBuilder SB, int APGain)
	{
		if (APGain != 0)
		{
			ParentObject.GetStat("AP").BaseValue += APGain;
			AppendStatMessage(SB, "Attribute Point", APGain);
		}
	}

	public void AddAttributeBonus(StringBuilder SB, int ABGain)
	{
		if (ABGain != 0)
		{
			ParentObject.GetStat("Strength").BaseValue += ABGain;
			ParentObject.GetStat("Intelligence").BaseValue += ABGain;
			ParentObject.GetStat("Willpower").BaseValue += ABGain;
			ParentObject.GetStat("Agility").BaseValue += ABGain;
			ParentObject.GetStat("Toughness").BaseValue += ABGain;
			ParentObject.GetStat("Ego").BaseValue += ABGain;
			AppendStatMessage(SB, "to each attribute", ABGain, Pluralize: false);
		}
	}

	public bool UseDetailedPrompt()
	{
		if (!ParentObject.IsPlayer())
		{
			if (PlayerLedPrompt)
			{
				return ParentObject.IsPlayerLed();
			}
			return false;
		}
		return true;
	}

	public void LevelUp(GameObject Kill = null, GameObject InfluencedBy = null)
	{
		int num = ++ParentObject.GetStat("Level").BaseValue;
		StringBuilder stringBuilder = (UseDetailedPrompt() ? Event.NewStringBuilder() : null);
		stringBuilder?.Append(ParentObject.IsPlayer() ? "You" : ParentObject.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutEpithet: true)).Append(ParentObject.Has).Append(" gained a level!")
			.CompoundItVerb(ParentObject, "are")
			.Append(" now level {{C|")
			.Append(num)
			.Append("}}!");
		GetEntryDice(out var BaseHPGain, out var BaseSPGain, out var BaseMPGain);
		GetLevelUpDiceEvent.GetFor(ParentObject, num, ref BaseHPGain, ref BaseSPGain, ref BaseMPGain);
		bool num2 = ParentObject.IsMutant();
		int HitPoints = RollHP(BaseHPGain);
		int SkillPoints = RollSP(BaseSPGain);
		int MutationPoints = (num2 ? RollMP(BaseMPGain) : 0);
		int AttributePoints = ((num % 3 == 0 && num % 6 != 0) ? 1 : 0);
		int AttributeBonus = ((num % 3 == 0 && num % 6 == 0) ? 1 : 0);
		int RapidAdvancement = ((num2 && (num + 5) % 10 == 0 && !ParentObject.IsEsper()) ? 3 : 0);
		GetLevelUpPointsEvent.GetFor(ParentObject, num, ref HitPoints, ref SkillPoints, ref MutationPoints, ref AttributePoints, ref AttributeBonus, ref RapidAdvancement);
		AddHitpoints(stringBuilder, HitPoints);
		AddSkillPoints(stringBuilder, SkillPoints);
		AddMutationPoints(stringBuilder, MutationPoints);
		AddAttributePoints(stringBuilder, AttributePoints);
		AddAttributeBonus(stringBuilder, AttributeBonus);
		ParentObject.FireEvent("AfterLevelGainedEarly");
		PlayWorldSound(ParentObject.IsPlayer() ? "Level_Up_Player" : "Level_Up_Other");
		RenderParticlesAt(ParentObject.CurrentCell);
		if (stringBuilder != null)
		{
			Popup.Show(stringBuilder.ToString());
		}
		else
		{
			DidX("gain", "a level", "!", null, ParentObject);
		}
		this.RapidAdvancement(RapidAdvancement);
		if (ParentObject.IsPlayer())
		{
			SifrahInsights();
			ParentObject.CurrentZone?.FireEvent("PlayerGainedLevel");
		}
		ParentObject.FireEvent("AfterLevelGained");
		ParentObject.FireEvent("AfterLevelGainedLate");
		ItemNaming.Opportunity(ParentObject, Kill, InfluencedBy, "Level", 6, 0, 0, 3);
	}

	public void RenderParticlesAt(Cell C)
	{
		if (C != null && C.IsVisible())
		{
			ParticleManager particleManager = XRLCore.ParticleManager;
			particleManager.AddSinusoidal("&W*", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
			particleManager.AddSinusoidal("&Y*", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
			particleManager.AddSinusoidal("&W\r", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
			particleManager.AddSinusoidal("&Y\u000e", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999);
		}
	}

	public void RapidAdvancement(int Amount)
	{
		if (Amount <= 0)
		{
			return;
		}
		string @for = GetMutationTermEvent.GetFor(ParentObject);
		bool flag = ParentObject.IsPlayer() && ParentObject.Stat("MP") >= 4;
		bool flag2 = false;
		if (flag && Popup.ShowYesNo("Your genome enters an excited state! Would you like to spend {{rules|4}} mutation points to buy " + Grammar.A(@for) + " before rapidly mutating?", AllowEscape: false) == DialogResult.Yes)
		{
			flag2 = MutationsAPI.BuyRandomMutation(ParentObject, 4, Confirm: false, @for);
		}
		List<BaseMutation> list = (from m in ParentObject.GetPhysicalMutations()
			where m.CanLevel()
			select m).ToList();
		if (list.Count > 0)
		{
			if (!flag && ParentObject.IsPlayer())
			{
				Popup.Show("Your genome enters an excited state!");
			}
			if (ParentObject.IsPlayer())
			{
				string[] options = list.Select((BaseMutation m) => m.DisplayName + " ({{C|" + m.Level + "}})").ToArray();
				int index = Popup.ShowOptionList("Choose a physical " + @for + " to rapidly advance.", options);
				Popup.Show("You have rapidly advanced " + list[index].DisplayName + " by " + Grammar.Cardinal(Amount) + " ranks to rank {{C|" + (list[index].Level + Amount) + "}}!");
				list[index].RapidLevel(Amount);
			}
			else
			{
				list.GetRandomElement().RapidLevel(Amount);
			}
		}
		else if (flag2)
		{
			Popup.Show("You have no physical " + Grammar.Pluralize(@for) + " to rapidly advance!");
		}
	}

	public void SifrahInsights()
	{
		if (SifrahGame.Installed)
		{
			if (ParentObject.HasSkill("Tinkering_Tinker1") && 10.in100())
			{
				TinkeringSifrah.AwardInsight();
			}
			if (ParentObject.HasSkill("Persuasion_Proselytize") && 10.in100())
			{
				SocialSifrah.AwardInsight();
			}
			if (ParentObject.HasSkill("Customs_Sharer") && 10.in100())
			{
				RitualSifrah.AwardInsight();
			}
		}
	}
}
