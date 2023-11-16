using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using XRL.UI;

namespace XRL.World.Conversations;

[Serializable]
[HasModSensitiveStaticCache]
public class ConversationLoader
{
	public Dictionary<string, Conversation> ConversationsByID;

	public Dictionary<string, ConversationChoice> ChoicesByID;

	[ModSensitiveStaticCache(false)]
	private static ConversationLoader _Loader;

	private Action<object> LogHandler = MetricsManager.LogError;

	[NonSerialized]
	private static StringBuilder builder = new StringBuilder();

	[NonSerialized]
	private static char[] trimChars = new char[2] { ' ', '\t' };

	public static ConversationLoader Loader
	{
		get
		{
			CheckOldInit();
			return _Loader;
		}
	}

	public static void CheckOldInit()
	{
		if (_Loader == null)
		{
			_Loader = new ConversationLoader();
			Loading.LoadTask("Loading legacy Conversations.xml", _Loader.LoadOldConversations);
		}
	}

	[PreGameCacheInit]
	public static void CheckInit()
	{
		if (Dialogue._Blueprints == null)
		{
			Loading.LoadTask("Loading Conversations.xml", LoadConversations);
		}
	}

	private static void ReadConversation(string Path, BuildContext Context)
	{
		string text = null;
		using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(Path);
		xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
		while (xmlTextReader.Read())
		{
			if (xmlTextReader.Name == "conversations")
			{
				text = (Context.Namespace = xmlTextReader.GetAttribute("Namespace"));
			}
			else if (xmlTextReader.Name == "conversation")
			{
				string text2 = xmlTextReader.GetAttribute("Namespace");
				if (!text2.IsNullOrEmpty() && !text.IsNullOrEmpty())
				{
					text2 = text + "." + text2;
				}
				Context.Namespace = text2 ?? text;
				ConversationXMLBlueprint conversationXMLBlueprint = new ConversationXMLBlueprint
				{
					Inherits = "BaseConversation"
				};
				conversationXMLBlueprint.Read(xmlTextReader, Context);
				if (Dialogue._Blueprints.TryGetValue(conversationXMLBlueprint.ID, out var value) && conversationXMLBlueprint.Load == 0)
				{
					value.Merge(conversationXMLBlueprint);
				}
				else
				{
					Dialogue._Blueprints[conversationXMLBlueprint.ID] = conversationXMLBlueprint;
				}
			}
		}
		xmlTextReader.Close();
	}

	private static void LoadConversations()
	{
		Dictionary<string, ConversationXMLBlueprint> dictionary = (Dialogue._Blueprints = new Dictionary<string, ConversationXMLBlueprint>());
		BuildContext context = new BuildContext();
		ReadConversation("Conversations.xml", context);
		ModManager.ForEachFile("Conversations.xml", delegate(string p, ModInfo m)
		{
			context.Mod = m;
			ReadConversation(p, context);
		});
		context.Next.AddRange(dictionary.Values);
		int num = 20;
		while (num > 0 && context.Next.Count > 0)
		{
			int count = context.Next.Count;
			context.Advance();
			foreach (ConversationXMLBlueprint item in context.Current)
			{
				if (!item.Bake(context))
				{
					context.Next.Add(item);
				}
			}
			if (count == context.Next.Count)
			{
				num--;
			}
		}
		foreach (string error in context.Errors)
		{
			MetricsManager.LogError(error);
		}
		context.Clear();
		foreach (ConversationXMLBlueprint value in dictionary.Values)
		{
			value.DistributeChildren(context);
		}
	}

	private void LoadOldConversations()
	{
		ConversationsByID = new Dictionary<string, Conversation>();
		ChoicesByID = new Dictionary<string, ConversationChoice>();
		List<string> Paths = new List<string>();
		List<ModInfo> modInfos = new List<ModInfo>();
		Paths.Add(DataManager.FilePath("Conversations.xml"));
		modInfos.Add(null);
		Paths.AddRange(Directory.GetFiles(DataManager.FilePath("."), "Conversations_*.xml", SearchOption.AllDirectories));
		ModManager.ForEachFile("Conversations.xml", delegate(string path, ModInfo ModInfo)
		{
			Paths.Add(path);
			modInfos.Add(ModInfo);
		});
		for (int i = 0; i < Paths.Count; i++)
		{
			string fileName = Paths[i];
			if (modInfos[i] != null)
			{
				LogHandler = modInfos[i].Error;
			}
			else
			{
				LogHandler = MetricsManager.LogError;
			}
			using XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream(fileName);
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "conversations")
				{
					LoadConversationsNode(xmlTextReader);
				}
			}
			xmlTextReader.Close();
		}
	}

	private void LoadConversationsNode(XmlTextReader Reader)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "conversation")
			{
				Conversation conversation = LoadConversationNode(Reader);
				ConversationsByID[conversation.ID] = conversation;
			}
		}
	}

	private Conversation LoadConversationNode(XmlTextReader Reader)
	{
		Conversation conversation = new Conversation();
		conversation.ID = Reader.GetAttribute("ID");
		if (Reader.NodeType == XmlNodeType.EndElement)
		{
			return conversation;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "node")
			{
				ConversationNode conversationNode = LoadConversationNodeNode(Reader, conversation);
				conversationNode.ParentConversation = conversation;
				if (conversationNode.ID == "Start")
				{
					conversation.StartNodes.Add(conversationNode);
				}
				else
				{
					try
					{
						conversation.NodesByID.Add(conversationNode.ID, conversationNode);
					}
					catch (Exception)
					{
						LogHandler("Duplicate node ID: " + conversationNode.ID);
					}
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "conversation")
			{
				return conversation;
			}
		}
		return conversation;
	}

	private ConversationNode LoadConversationNodeNode(XmlTextReader Reader, Conversation ParentConversation)
	{
		ConversationNode conversationNode = new ConversationNode();
		conversationNode.ID = Reader.GetAttribute("ID");
		conversationNode.ParentConversation = ParentConversation;
		conversationNode.AddIntState = Reader.GetAttribute("AddIntState");
		conversationNode.ClearOwner = Reader.GetAttribute("ClearOwner");
		conversationNode.CompleteQuestStep = Reader.GetAttribute("CompleteQuestStep");
		conversationNode.Filter = Reader.GetAttribute("Filter");
		conversationNode.FinishQuest = Reader.GetAttribute("FinishQuest");
		conversationNode.GiveItem = Reader.GetAttribute("GiveItem");
		conversationNode.GiveOneItem = Reader.GetAttribute("GiveOneItem");
		conversationNode.IfFinishedQuest = Reader.GetAttribute("IfFinishedQuest");
		conversationNode.IfFinishedQuestStep = Reader.GetAttribute("IfFinishedQuestStep");
		conversationNode.IfHasBlueprint = Reader.GetAttribute("IfHasBlueprint");
		conversationNode.IfHaveItemWithID = Reader.GetAttribute("IfHaveItemWithID");
		conversationNode.IfHaveObservation = Reader.GetAttribute("IfHaveObservation");
		conversationNode.IfNotHaveObservation = Reader.GetAttribute("IfNotHaveObservation");
		conversationNode.IfHaveObservationWithTag = Reader.GetAttribute("IfHaveObservationWithTag");
		conversationNode.IfHaveVillageNote = Reader.GetAttribute("IfHaveVillageNote");
		conversationNode.IfHavePart = Reader.GetAttribute("IfHavePart");
		conversationNode.IfHaveQuest = Reader.GetAttribute("IfHaveQuest");
		conversationNode.IfHaveState = Reader.GetAttribute("IfHaveState");
		conversationNode.IfHaveSultanNoteWithTag = Reader.GetAttribute("IfHaveSultanNoteWithTag");
		conversationNode.IfLevelLessOrEqual = Reader.GetAttribute("IfLevelLessOrEqual");
		conversationNode.IfNotFinishedQuest = Reader.GetAttribute("IfNotFinishedQuest");
		conversationNode.IfNotFinishedQuestStep = Reader.GetAttribute("IfNotFinishedQuestStep");
		conversationNode.IfNotHavePart = Reader.GetAttribute("IfNotHavePart");
		conversationNode.IfNotHaveQuest = Reader.GetAttribute("IfNotHaveQuest");
		conversationNode.IfNotHaveState = Reader.GetAttribute("IfNotHaveState");
		conversationNode.IfNotTrueKin = Reader.GetAttribute("IfNotTrueKin").EqualsNoCase("true");
		conversationNode.IfTestState = Reader.GetAttribute("IfTestState");
		conversationNode.IfTrueKin = Reader.GetAttribute("IfTrueKin").EqualsNoCase("true");
		conversationNode.IfWearingBlueprint = Reader.GetAttribute("IfWearingBlueprint");
		conversationNode.RevealMapNoteId = Reader.GetAttribute("RevealMapNoteId");
		conversationNode.SetBooleanState = Reader.GetAttribute("SetBooleanState");
		conversationNode.SetIntState = Reader.GetAttribute("SetIntState");
		conversationNode.SetStringState = Reader.GetAttribute("SetStringState");
		conversationNode.SpecialRequirement = Reader.GetAttribute("SpecialRequirement");
		conversationNode.StartQuest = Reader.GetAttribute("StartQuest");
		conversationNode.TakeItem = Reader.GetAttribute("TakeItem");
		conversationNode.ToggleBooleanState = Reader.GetAttribute("ToggleBooleanState");
		conversationNode.TradeNote = Reader.GetAttribute("TradeNote") == "show";
		string attribute = Reader.GetAttribute("Closable");
		if (attribute != null && attribute == "false")
		{
			conversationNode.bCloseable = false;
		}
		if (Reader.NodeType == XmlNodeType.EndElement)
		{
			return conversationNode;
		}
		int num = 0;
		while (Reader.Read())
		{
			if (Reader.Name == "choice")
			{
				ConversationChoice conversationChoice = LoadConversationChoiceNode(Reader, ParentConversation.ID);
				conversationChoice.ParentNode = conversationNode;
				conversationChoice.Ordinal = ((conversationChoice.Ordinal == int.MinValue) ? num++ : conversationChoice.Ordinal);
				conversationNode.Choices.Add(conversationChoice);
			}
			if (Reader.NodeType == XmlNodeType.Element && Reader.Name == "text")
			{
				Reader.Read();
				conversationNode.Text = ProcessLines(Reader.Value);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "node")
			{
				return conversationNode;
			}
		}
		return conversationNode;
	}

	private static string ProcessLines(string Text, bool skipInitialNewline = false)
	{
		string[] array = Text.Replace("\r\n", "\n").Split('\n');
		int num = ((skipInitialNewline && array.Length != 0 && array[0] == "") ? 1 : 0);
		for (int i = num; i < array.Length; i++)
		{
			array[i] = array[i].TrimStart(trimChars).Replace("/_", " ");
		}
		builder.Length = 0;
		for (int j = num; j < array.Length; j++)
		{
			if (j > num)
			{
				builder.Append('\n');
			}
			builder.Append(array[j]);
		}
		return builder.ToString();
	}

	private ConversationChoice LoadConversationChoiceNode(XmlTextReader Reader, string ConversationID)
	{
		ConversationChoice conversationChoice = new ConversationChoice();
		if (Reader.NodeType == XmlNodeType.EndElement)
		{
			return conversationChoice;
		}
		string text = null;
		string attribute = Reader.GetAttribute("UseID");
		if (!string.IsNullOrEmpty(attribute))
		{
			string choiceKey = GetChoiceKey(ConversationID, attribute);
			if (ChoicesByID.TryGetValue(choiceKey, out var value))
			{
				conversationChoice.Copy(value);
				conversationChoice.ID = Reader.GetAttribute("ID") ?? "";
				string attribute2;
				if ((attribute2 = Reader.GetAttribute("Achievement")) != null)
				{
					conversationChoice.Achievement = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("AddIntState")) != null)
				{
					conversationChoice.AddIntState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfFinishedQuest")) != null)
				{
					conversationChoice.IfFinishedQuest = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfFinishedQuestStep")) != null)
				{
					conversationChoice.IfFinishedQuestStep = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfHasBlueprint")) != null)
				{
					conversationChoice.IfHasBlueprint = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfHaveItemWithID")) != null)
				{
					conversationChoice.IfHaveItemWithID = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfHavePart")) != null)
				{
					conversationChoice.IfHavePart = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfHaveQuest")) != null)
				{
					conversationChoice.IfHaveQuest = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfHaveState")) != null)
				{
					conversationChoice.IfHaveState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotFinishedQuest")) != null)
				{
					conversationChoice.IfNotFinishedQuest = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotFinishedQuestStep")) != null)
				{
					conversationChoice.IfNotFinishedQuestStep = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotHavePart")) != null)
				{
					conversationChoice.IfNotHavePart = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotHaveQuest")) != null)
				{
					conversationChoice.IfNotHaveQuest = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotHaveState")) != null)
				{
					conversationChoice.IfNotHaveState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfNotTrueKin")) != null)
				{
					conversationChoice.IfTrueKin = attribute2.EqualsNoCase("true");
				}
				if ((attribute2 = Reader.GetAttribute("IfTestState")) != null)
				{
					conversationChoice.IfTestState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("IfTrueKin")) != null)
				{
					conversationChoice.IfTrueKin = attribute2.EqualsNoCase("true");
				}
				if ((attribute2 = Reader.GetAttribute("IfWearingBlueprint")) != null)
				{
					conversationChoice.IfWearingBlueprint = Reader.GetAttribute("IfWearingBlueprint");
				}
				if ((attribute2 = Reader.GetAttribute("SetBooleanState")) != null)
				{
					conversationChoice.SetBooleanState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("SetIntState")) != null)
				{
					conversationChoice.SetIntState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("SetStringState")) != null)
				{
					conversationChoice.SetStringState = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("SpecialRequirement")) != null)
				{
					conversationChoice.SpecialRequirement = attribute2;
				}
				if ((attribute2 = Reader.GetAttribute("ToggleBooleanState")) != null)
				{
					conversationChoice.ToggleBooleanState = attribute2;
				}
			}
			else
			{
				LogHandler("no choice " + attribute + " in conversation " + ConversationID + ", line " + Reader.LineNumber);
			}
		}
		else
		{
			string obj = Reader.GetAttribute("ID") ?? "";
			text = obj;
			conversationChoice.ID = obj;
			conversationChoice.GotoID = Reader.GetAttribute("GotoID");
			conversationChoice.Achievement = Reader.GetAttribute("Achievement");
			conversationChoice.AddIntState = Reader.GetAttribute("AddIntState");
			conversationChoice.CallScript = Reader.GetAttribute("CallScript");
			conversationChoice.ClearOwner = Reader.GetAttribute("ClearOwner");
			conversationChoice.CompleteQuestStep = Reader.GetAttribute("CompleteQuestStep");
			conversationChoice.Execute = Reader.GetAttribute("Execute");
			conversationChoice.Filter = Reader.GetAttribute("Filter");
			conversationChoice.FinishQuest = Reader.GetAttribute("FinishQuest");
			conversationChoice.GiveItem = Reader.GetAttribute("GiveItem");
			conversationChoice.GiveOneItem = Reader.GetAttribute("GiveOneItem");
			conversationChoice.IdGift = Reader.GetAttribute("IdGift");
			conversationChoice.IfFinishedQuest = Reader.GetAttribute("IfFinishedQuest");
			conversationChoice.IfFinishedQuestStep = Reader.GetAttribute("IfFinishedQuestStep");
			conversationChoice.IfHasBlueprint = Reader.GetAttribute("IfHasBlueprint");
			conversationChoice.IfHaveItemWithID = Reader.GetAttribute("IfHaveItemWithID");
			conversationChoice.IfHaveObservation = Reader.GetAttribute("IfHaveObservation");
			conversationChoice.IfNotHaveObservation = Reader.GetAttribute("IfNotHaveObservation");
			conversationChoice.IfHaveObservationWithTag = Reader.GetAttribute("IfHaveObservationWithTag");
			conversationChoice.IfHaveVillageNote = Reader.GetAttribute("IfHaveVillageNote");
			conversationChoice.IfHavePart = Reader.GetAttribute("IfHavePart");
			conversationChoice.IfHaveQuest = Reader.GetAttribute("IfHaveQuest");
			conversationChoice.IfHaveState = Reader.GetAttribute("IfHaveState");
			conversationChoice.IfHaveSultanNoteWithTag = Reader.GetAttribute("IfHaveSultanNoteWithTag");
			conversationChoice.IfNotFinishedQuest = Reader.GetAttribute("IfNotFinishedQuest");
			conversationChoice.IfNotFinishedQuestStep = Reader.GetAttribute("IfNotFinishedQuestStep");
			conversationChoice.IfNotHavePart = Reader.GetAttribute("IfNotHavePart");
			conversationChoice.IfNotHaveQuest = Reader.GetAttribute("IfNotHaveQuest");
			conversationChoice.IfNotHaveState = Reader.GetAttribute("IfNotHaveState");
			conversationChoice.IfNotTrueKin = Reader.GetAttribute("IfNotTrueKin").EqualsNoCase("true");
			conversationChoice.IfTestState = Reader.GetAttribute("IfTestState");
			conversationChoice.IfTrueKin = Reader.GetAttribute("IfTrueKin").EqualsNoCase("true");
			conversationChoice.IfWearingBlueprint = Reader.GetAttribute("IfWearingBlueprint");
			conversationChoice.RevealMapNoteId = Reader.GetAttribute("RevealMapNoteId");
			conversationChoice.RevealObservation = Reader.GetAttribute("RevealObservation");
			conversationChoice.SetBooleanState = Reader.GetAttribute("SetBooleanState");
			conversationChoice.SetIntState = Reader.GetAttribute("SetIntState");
			conversationChoice.SetStringState = Reader.GetAttribute("SetStringState");
			conversationChoice.SpecialRequirement = Reader.GetAttribute("SpecialRequirement");
			conversationChoice.StartQuest = Reader.GetAttribute("StartQuest");
			conversationChoice.TakeBlueprint = Reader.GetAttribute("TakeBlueprint");
			conversationChoice.TakeItem = Reader.GetAttribute("TakeItem");
			conversationChoice.ToggleBooleanState = Reader.GetAttribute("ToggleBooleanState");
			conversationChoice.LineNumber = Reader.LineNumber;
			if (int.TryParse(Reader.GetAttribute("Ordinal"), out var result))
			{
				conversationChoice.Ordinal = result;
			}
			else
			{
				conversationChoice.Ordinal = int.MinValue;
			}
			Reader.Read();
			conversationChoice.Text = ProcessLines(Reader.Value, skipInitialNewline: true);
			if (!string.IsNullOrEmpty(text))
			{
				string choiceKey2 = GetChoiceKey(ConversationID, text);
				if (ChoicesByID.ContainsKey(choiceKey2))
				{
					LogHandler("Duplicate conversation choice ID " + text + " at line " + Reader.LineNumber + " (conflicts with line " + ChoicesByID[choiceKey2].LineNumber + ")");
				}
				else
				{
					ChoicesByID.Add(choiceKey2, conversationChoice);
					if (!ChoicesByID.ContainsKey(text))
					{
						ChoicesByID.Add(text, conversationChoice);
					}
				}
			}
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "choice")
			{
				return conversationChoice;
			}
		}
		return conversationChoice;
	}

	private static string GetChoiceKey(string ConversationID, string ChoiceID)
	{
		return ConversationID + "|" + ChoiceID;
	}
}
