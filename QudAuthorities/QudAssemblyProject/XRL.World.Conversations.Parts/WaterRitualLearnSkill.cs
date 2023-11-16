using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World.Conversations.Parts;

public class WaterRitualLearnSkill : IWaterRitualPart
{
	public string Name;

	public SkillEntry Skill;

	public PowerEntry Power;

	public int Points;

	public bool Initiatory;

	public bool Disambiguate;

	public string DisplayName => Skill?.Name ?? Power?.Name ?? "<error: missing skill/power>";

	public string InitiatoryKey => Skill.Class + "_Initiated_By_" + The.Speaker.id;

	public override bool Available => The.Player.Stat("SP") >= Points;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != EnteredElementEvent.ID)
		{
			return ID == PrepareTextEvent.ID;
		}
		return true;
	}

	public override void Awake()
	{
		if (The.Speaker.HasProperty("WaterRitualNoSellSkill"))
		{
			return;
		}
		Name = The.Speaker.GetStringProperty("WaterRitual_Skill") ?? The.Speaker.GetxTag("WaterRitual", "SellSkill") ?? (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualAltSkill : WaterRitual.RecordFaction.WaterRitualSkill);
		if (Name.IsNullOrEmpty())
		{
			return;
		}
		if (!SkillFactory.Factory.SkillByClass.TryGetValue(Name, out Skill))
		{
			if (!SkillFactory.Factory.PowersByClass.TryGetValue(Name, out Power))
			{
				return;
			}
			SkillEntry parentSkill = Power.ParentSkill;
			if ((parentSkill != null && parentSkill.Initiatory == true) || Power.Cost == 0)
			{
				Skill = Power.ParentSkill;
				Power = null;
				Name = Skill.Class;
			}
		}
		SkillEntry skill = Skill;
		Initiatory = skill != null && skill.Initiatory == true;
		if (Initiatory)
		{
			if (!CanUnlockInitiatoryPower())
			{
				return;
			}
			ConversationText conversationText = ParentElement.Texts?.FirstOrDefault((ConversationText t) => t.ID == "InitiatoryText");
			if (conversationText != null)
			{
				conversationText.Priority = 2;
			}
		}
		else if (The.Player.HasSkill(Name))
		{
			return;
		}
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Skill", Power?.Cost ?? Skill.Cost);
		Disambiguate = ShouldDisambiguatePower();
		Visible = true;
	}

	public bool CanUnlockInitiatoryPower()
	{
		if (The.Player.GetIntProperty(InitiatoryKey) > 0)
		{
			return false;
		}
		if (The.Player.HasSkill(Skill.Class) && Skill.Powers.Values.All((PowerEntry p) => The.Player.HasSkill(p.Class) || !p.MeetsRequirements(The.Player)))
		{
			return false;
		}
		Points = Skill.Cost;
		return true;
	}

	public void UnlockInitiatorySkill()
	{
		if (UseReputation())
		{
			The.Player.AddSkill(Name);
			string text = Skill.Powers.Values.FirstOrDefault((PowerEntry p) => The.Player.HasSkill(p.Class))?.Name;
			Popup.Show(The.Speaker.Does("lead", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you through a rite of ancient mystery, one not for profane eyes or ears. You have begun your journey upon " + DisplayName + ((text == null) ? "" : (" with initiation into " + text)) + ".");
		}
	}

	public void UnlockInitiatoryPower()
	{
		PowerEntry powerEntry = Skill.Powers.Values.FirstOrDefault((PowerEntry p) => !The.Player.HasSkill(p.Class) && p.MeetsRequirements(The.Player));
		if (powerEntry == null)
		{
			Popup.ShowFail("You have completed " + DisplayName + ".");
		}
		else if (UseReputation())
		{
			The.Player.AddSkill(powerEntry.Class);
			bool flag = Skill.Powers.Values.Any((PowerEntry p) => !The.Player.HasSkill(p.Class) && p.MeetsRequirements(The.Player));
			Popup.Show(The.Speaker.Does("lead", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you through a mysterious rite. Your journey upon " + DisplayName + (flag ? " continues" : " has reached completion") + (powerEntry.Name.IsNullOrEmpty() ? "" : (" with initiation into " + powerEntry.Name)) + ".");
		}
	}

	public void UnlockInitiatory()
	{
		if (!The.Player.HasSkill(Name))
		{
			UnlockInitiatorySkill();
		}
		else
		{
			UnlockInitiatoryPower();
		}
		The.Player.SetIntProperty(InitiatoryKey, The.Player.Stat("Level"));
	}

	public void Unlock()
	{
		if (UseReputation())
		{
			Popup.Show(The.Speaker.Does("teach", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you {{W|" + DisplayName + "}}!");
			The.Player.AddSkill(Name);
			if (Name == "Acrobatics_Jump" && The.Speaker.Blueprint.Contains("frog", CompareOptions.IgnoreCase))
			{
				AchievementManager.SetAchievement("ACH_LEARN_JUMP");
			}
		}
	}

	public bool ShouldDisambiguatePower()
	{
		if (Power?.ParentSkill == null)
		{
			return false;
		}
		return SkillFactory.Factory.PowersByClass.Count((KeyValuePair<string, PowerEntry> Pair) => Pair.Value.Name == Power.Name) > 1;
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (The.Player.Stat("SP") < Points)
		{
			Popup.ShowFail("You don't have enough skill points.");
			return false;
		}
		if (Initiatory)
		{
			UnlockInitiatory();
		}
		else
		{
			Unlock();
		}
		if (Points > 0)
		{
			The.Player.GetStat("SP").Penalty += Points;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Text.Replace("=skill.name=", DisplayName);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{").Append(Lowlight).Append("|[");
		if (!Initiatory)
		{
			stringBuilder.Append("learn {{W|").Append(DisplayName);
			if (Disambiguate)
			{
				stringBuilder.Compound('(').Append(Power.ParentSkill.Name).Append(')');
			}
			stringBuilder.Append("}}: ");
		}
		stringBuilder.Append("{{").Append(Numeric).Append("|-")
			.Append(Reputation)
			.Append("}} reputation");
		if (Points > 0)
		{
			stringBuilder.Append(", {{C|-").Append(Points).Append("}} SP");
		}
		E.Tag = stringBuilder.Append("]}}").ToString();
		return false;
	}
}
