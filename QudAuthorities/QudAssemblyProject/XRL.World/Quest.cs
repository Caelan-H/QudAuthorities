using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;

namespace XRL.World;

[Serializable]
public class Quest
{
	public string ID;

	public string Name;

	public string Accomplishment;

	public string Achievement;

	public string BonusAtLevel;

	public int Level;

	public string Factions;

	public string Reputation;

	public string Hagiograph;

	public string HagiographCategory;

	public bool Finished;

	public DynamicQuestReward _dynamicReward;

	public Dictionary<string, QuestStep> StepsByID;

	public string System;

	public QuestManager _Manager;

	public DynamicQuestReward dynamicReward
	{
		get
		{
			return _dynamicReward;
		}
		set
		{
			_dynamicReward = value;
			if (value == null)
			{
				return;
			}
			int count = StepsByID.Count;
			foreach (QuestStep value2 in StepsByID.Values)
			{
				value2.XP = value.StepXP / count;
			}
		}
	}

	public QuestManager Manager
	{
		get
		{
			return _Manager;
		}
		set
		{
			_Manager = value;
			value.MyQuestID = ID;
		}
	}

	public string DisplayName => "{{W|" + Name + "}}";

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[Quest " + ID + "]");
		stringBuilder.AppendLine("Name=" + Name);
		stringBuilder.AppendLine("Accomplishment=" + Accomplishment);
		stringBuilder.AppendLine("Achievement=" + Achievement);
		stringBuilder.AppendLine("BonusAtLevel=" + BonusAtLevel);
		stringBuilder.AppendLine("Hagiograph=" + Hagiograph);
		stringBuilder.AppendLine("HagiographCategory=" + HagiographCategory);
		stringBuilder.AppendLine("Factions=" + Factions);
		stringBuilder.AppendLine("Reputation=" + Reputation);
		stringBuilder.AppendLine("Level=" + Level);
		stringBuilder.AppendLine("Finished=" + Finished);
		stringBuilder.AppendLine("System=" + System);
		if (_dynamicReward == null)
		{
			stringBuilder.Append("DynamicReward=none");
		}
		else
		{
			stringBuilder.Append("DynamicReward=" + _dynamicReward.ToString());
		}
		stringBuilder.Append("nSteps=" + StepsByID.Count);
		foreach (KeyValuePair<string, QuestStep> item in StepsByID)
		{
			stringBuilder.Append(" step " + item.Key + " = " + item.Value.ToString());
		}
		return stringBuilder.ToString();
	}

	public bool ReadyToTurnIn()
	{
		foreach (QuestStep value in StepsByID.Values)
		{
			if (!value.Finished)
			{
				return false;
			}
		}
		return true;
	}

	public void FinishPost()
	{
		if (dynamicReward != null)
		{
			dynamicReward.postaward();
		}
	}

	public void Fail()
	{
	}

	public void Finish()
	{
		if (dynamicReward != null)
		{
			dynamicReward.award();
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.Write(Name);
		Writer.Write(Level);
		Writer.Write(Finished);
		Writer.Write(Accomplishment);
		Writer.Write(Achievement);
		Writer.Write(Hagiograph);
		Writer.Write(HagiographCategory);
		Writer.Write(BonusAtLevel);
		Writer.Write(Factions);
		Writer.Write(Reputation);
		Writer.WriteObject(_dynamicReward);
		Writer.WriteObject(StepsByID);
		if (Manager == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(1);
			Manager.Save(Writer);
		}
		Writer.Write(System);
	}

	public static Quest Load(SerializationReader Reader)
	{
		Quest quest = new Quest();
		quest.ID = Reader.ReadString();
		quest.Name = Reader.ReadString();
		quest.Level = Reader.ReadInt32();
		quest.Finished = Reader.ReadBoolean();
		quest.Accomplishment = Reader.ReadString();
		Quest value;
		if (Reader.FileVersion >= 233)
		{
			quest.Achievement = Reader.ReadString();
		}
		else if (QuestLoader.Loader.QuestsByID.TryGetValue(quest.ID, out value))
		{
			quest.Achievement = value.Achievement;
		}
		quest.Hagiograph = Reader.ReadString();
		quest.HagiographCategory = Reader.ReadString();
		quest.BonusAtLevel = Reader.ReadString();
		if (Reader.FileVersion >= 161)
		{
			quest.Factions = Reader.ReadString();
			quest.Reputation = Reader.ReadString();
		}
		quest._dynamicReward = Reader.ReadObject() as DynamicQuestReward;
		quest.StepsByID = (Dictionary<string, QuestStep>)Reader.ReadObject();
		if (Reader.ReadInt32() != 0)
		{
			quest.Manager = (QuestManager)IPart.Load(Reader);
		}
		quest.System = Reader.ReadString();
		return quest;
	}

	public Quest Copy()
	{
		Quest quest = new Quest();
		quest.ID = ID;
		quest.Name = Name;
		quest.Accomplishment = Accomplishment;
		quest.Achievement = Achievement;
		quest.Hagiograph = Hagiograph;
		quest.HagiographCategory = HagiographCategory;
		quest.BonusAtLevel = BonusAtLevel;
		quest.Factions = Factions;
		quest.Reputation = Reputation;
		quest.Level = Level;
		quest.Finished = Finished;
		quest._Manager = _Manager;
		quest.System = System;
		quest._dynamicReward = _dynamicReward;
		quest.StepsByID = new Dictionary<string, QuestStep>();
		foreach (string key in StepsByID.Keys)
		{
			QuestStep questStep = new QuestStep();
			questStep.Finished = false;
			questStep.ID = StepsByID[key].ID;
			questStep.Name = StepsByID[key].Name;
			questStep.Text = StepsByID[key].Text;
			questStep.XP = StepsByID[key].XP;
			quest.StepsByID.Add(key, questStep);
		}
		return quest;
	}

	public static string Consider(Quest Q)
	{
		int num = XRLCore.Core.Game.Player.Body.Statistics["Level"].Value - Q.Level;
		if (num <= -15)
		{
			return "[{{R|Impossible}}]";
		}
		if (num <= -10)
		{
			return "[{{r|Very Tough}}]";
		}
		if (num <= -5)
		{
			return "[{{W|Tough}}]";
		}
		if (num < 5)
		{
			return "[{{w|Average}}]";
		}
		if (num <= 10)
		{
			return "[{{g|Easy}}]";
		}
		return "[{{G|Trivial}}]";
	}
}
