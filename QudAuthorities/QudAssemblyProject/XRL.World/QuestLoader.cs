using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRL.Core;
using XRL.UI;

namespace XRL.World;

[Serializable]
[HasModSensitiveStaticCache]
public class QuestLoader
{
	public Dictionary<string, Quest> QuestsByID;

	[ModSensitiveStaticCache(false)]
	private static QuestLoader _Loader;

	public static QuestLoader Loader
	{
		get
		{
			CheckInit();
			return _Loader;
		}
	}

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (_Loader == null)
		{
			_Loader = new QuestLoader();
			Loading.LoadTask("Loading Quests.xml", _Loader.LoadQuests);
		}
	}

	public void LoadQuests()
	{
		List<string> Paths = new List<string>();
		Paths.Add(DataManager.FilePath("Quests.xml"));
		Paths.AddRange(Directory.GetFiles(DataManager.FilePath("."), "Quests_*.xml", SearchOption.AllDirectories));
		ModManager.ForEachFile("Quests.xml", delegate(string path)
		{
			Paths.Add(path);
		});
		QuestsByID = new Dictionary<string, Quest>();
		foreach (string item in Paths)
		{
			using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(item);
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "quests")
				{
					LoadQuestsNode(xmlTextReader);
				}
			}
			xmlTextReader.Close();
		}
	}

	private void LoadQuestsNode(XmlTextReader Reader)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "quest")
			{
				Quest quest = LoadQuestNode(Reader);
				QuestsByID.Add(quest.ID, quest);
			}
		}
	}

	private Quest LoadQuestNode(XmlTextReader Reader)
	{
		Quest quest = new Quest();
		quest.StepsByID = new Dictionary<string, QuestStep>();
		quest.ID = Reader.GetAttribute("ID");
		quest.Name = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(quest.ID))
		{
			quest.ID = quest.Name;
		}
		quest.Level = Convert.ToInt32(Reader.GetAttribute("Level"));
		quest.Accomplishment = Reader.GetAttribute("Accomplishment");
		quest.Achievement = Reader.GetAttribute("Achievement");
		quest.Hagiograph = Reader.GetAttribute("Hagiograph");
		quest.HagiographCategory = Reader.GetAttribute("HagiographCategory");
		quest.BonusAtLevel = Reader.GetAttribute("BonusAtLevel");
		quest.Factions = Reader.GetAttribute("Factions");
		quest.Reputation = Reader.GetAttribute("Reputation");
		quest.System = Reader.GetAttribute("System");
		string attribute = Reader.GetAttribute("Manager");
		if (!string.IsNullOrEmpty(attribute))
		{
			string text = "XRL.World.QuestManagers." + attribute;
			Type type = ModManager.ResolveType(text);
			if (type == null)
			{
				XRLCore.LogError("Unknown quest manager " + text + "!");
				return null;
			}
			quest.Manager = (QuestManager)Activator.CreateInstance(type);
		}
		if (Reader.NodeType == XmlNodeType.EndElement)
		{
			return quest;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "step")
			{
				QuestStep questStep = LoadQuestStepNode(Reader);
				quest.StepsByID.Add(questStep.ID, questStep);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "quest")
			{
				return quest;
			}
		}
		return quest;
	}

	private QuestStep LoadQuestStepNode(XmlTextReader Reader)
	{
		QuestStep questStep = new QuestStep();
		questStep.ID = Reader.GetAttribute("ID");
		questStep.Name = Reader.GetAttribute("Name");
		if (string.IsNullOrEmpty(questStep.ID))
		{
			questStep.ID = questStep.Name;
		}
		questStep.XP = Convert.ToInt32(Reader.GetAttribute("XP"));
		if (Reader.NodeType == XmlNodeType.EndElement)
		{
			return questStep;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "text")
			{
				Reader.Read();
				questStep.Text = Reader.Value;
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "step")
			{
				return questStep;
			}
		}
		return questStep;
	}
}
