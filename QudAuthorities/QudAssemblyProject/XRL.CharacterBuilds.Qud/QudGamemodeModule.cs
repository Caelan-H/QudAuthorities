using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Rules;
using XRL.UI;

namespace XRL.CharacterBuilds.Qud;

public class QudGamemodeModule : EmbarkBuilderModule<QudGamemodeModuleData>
{
	public class GameModeDescriptor
	{
		public string ID;

		public string Title;

		public string IconTile;

		public string IconForeground;

		public string IconDetail;

		public string Description;

		public bool Editable = true;

		public Dictionary<string, string> stringGameStates = new Dictionary<string, string>();

		public Dictionary<string, int> intGameStates = new Dictionary<string, int>();

		public Dictionary<string, long> int64GameStates = new Dictionary<string, long>();

		public Dictionary<string, bool> boolGameStates = new Dictionary<string, bool>();

		public List<string> gameSystems = new List<string>();
	}

	public Dictionary<string, GameModeDescriptor> GameModes = new Dictionary<string, GameModeDescriptor>();

	protected GameModeDescriptor CurrentReadingGameModeDescriptor;

	public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
	{
		get
		{
			Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
			xmlNodes.Add("modes", HandleModesNode);
			return xmlNodes;
		}
	}

	public Dictionary<string, Action<XmlDataHelper>> XmlModesNodes => new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "mode", HandleModeNode },
		{ "icon", HandleModeIconNode },
		{ "description", HandleModeDescriptionNode },
		{ "stringgamestate", HandleStringGameStateNode },
		{ "boolgamestate", HandleBoolGameStateNode },
		{ "intgamestate", HandleIntGameStateNode },
		{ "int64gamestate", HandleInt64GameStateNode },
		{ "gamesystem", HandleGameSystemNode }
	};

	public GameModeDescriptor selectedMode => GameModes[base.data.Mode];

	public override bool IncludeInBuildCodes()
	{
		return false;
	}

	public override string DataErrors()
	{
		if (base.data?.Mode == null)
		{
			return "No game mode selected.";
		}
		return null;
	}

	public void SelectMode(string mode)
	{
		QudGamemodeModuleData qudGamemodeModuleData = new QudGamemodeModuleData();
		qudGamemodeModuleData.Mode = mode;
		setData(qudGamemodeModuleData);
		if (mode == "_Quickstart")
		{
			builder.info.GameSeed = Guid.NewGuid().ToString();
			Stat.ReseedFrom(builder.info.GameSeed);
			EmbarkBuilderModuleWindowDescriptor activeWindow = builder.activeWindow;
			do
			{
				builder.advance();
				if (builder.activeWindow != null && builder.activeWindow != activeWindow)
				{
					activeWindow = builder.activeWindow;
					activeWindow.window.DebugQuickstart(mode);
					continue;
				}
				break;
			}
			while (builder?.activeWindow != null);
		}
		else if (mode == "Daily")
		{
			builder.info.GameSeed = DateTime.Now.ToLongDateString();
			Stat.ReseedFrom(builder.info.GameSeed);
			EmbarkBuilderModuleWindowDescriptor activeWindow2 = builder.activeWindow;
			do
			{
				builder.advance();
				if (builder.activeWindow == null || builder.activeWindow == activeWindow2)
				{
					break;
				}
				activeWindow2 = builder.activeWindow;
				activeWindow2.window.DailySelection();
			}
			while (builder?.activeWindow?.viewID != "Chargen/BuildSummary");
			builder.GetModule<QudChooseStartingLocationModule>().setData(new QudChooseStartingLocationModuleData(builder.GetModule<QudChooseStartingLocationModule>().startingLocations.Keys.GetRandomElement()));
		}
		else
		{
			builder.advance();
		}
	}

	public string GetMode()
	{
		return base.data?.Mode;
	}

	public GameModeDescriptor GetModeDescriptor()
	{
		GameModeDescriptor value = null;
		GameModes.TryGetValue(GetMode() ?? "", out value);
		return value;
	}

	public override object handleUIEvent(string id, object element)
	{
		if (id == EmbarkBuilder.EventNames.EditableGameModeQuery)
		{
			bool? flag = GetModeDescriptor()?.Editable;
			if (!flag.HasValue)
			{
				return element;
			}
			return flag.GetValueOrDefault();
		}
		return base.handleUIEvent(id, element);
	}

	public void Back()
	{
		builder.back();
	}

	public override void Init()
	{
		base.Init();
		if (Options.ShowQuickstartOption && !GameModes.Any((KeyValuePair<string, GameModeDescriptor> m) => m.Key == "_Quickstart"))
		{
			GameModes.Add("_Quickstart", new GameModeDescriptor
			{
				Description = "_Quickstart",
				IconDetail = "y",
				IconForeground = "Y",
				IconTile = "Text/26.bmp",
				ID = "_Quickstart",
				Title = "_Quickstart"
			});
		}
	}

	public void HandleModesNode(XmlDataHelper xml)
	{
		xml.HandleNodes(XmlModesNodes);
	}

	protected void HandleModeNode(XmlDataHelper xml)
	{
		string attribute = xml.GetAttribute("ID");
		if (!GameModes.TryGetValue(attribute, out CurrentReadingGameModeDescriptor))
		{
			CurrentReadingGameModeDescriptor = new GameModeDescriptor
			{
				ID = attribute
			};
			GameModes.Add(attribute, CurrentReadingGameModeDescriptor);
		}
		CurrentReadingGameModeDescriptor.Title = xml.GetAttribute("Title");
		CurrentReadingGameModeDescriptor.Editable = xml.GetAttributeBool("Editable", defaultValue: true);
		xml.HandleNodes(XmlModesNodes);
		CurrentReadingGameModeDescriptor = null;
	}

	protected void HandleModeIconNode(XmlDataHelper xml)
	{
		CurrentReadingGameModeDescriptor.IconTile = xml.GetAttribute("Tile");
		CurrentReadingGameModeDescriptor.IconDetail = xml.GetAttributeString("Detail", "W");
		CurrentReadingGameModeDescriptor.IconForeground = xml.GetAttributeString("Foreground", "y");
		xml.DoneWithElement();
	}

	protected void HandleModeDescriptionNode(XmlDataHelper xml)
	{
		CurrentReadingGameModeDescriptor.Description = xml.GetTextNode();
	}

	protected void HandleGameSystemNode(XmlDataHelper xml)
	{
		CurrentReadingGameModeDescriptor.gameSystems.Add(xml.GetAttribute("Class"));
	}

	protected void HandleStringGameStateNode(XmlDataHelper xml)
	{
		string attribute = xml.GetAttribute("Name");
		string attribute2 = xml.GetAttribute("Value");
		if (attribute == null)
		{
			xml.ParseWarning("No Name provided for stringgamestate node.");
			xml.DoneWithElement();
		}
		else if (attribute2 == null)
		{
			xml.ParseWarning("No Value provided for stringgamestate node Name=" + attribute + ".");
			xml.DoneWithElement();
		}
		else
		{
			CurrentReadingGameModeDescriptor.stringGameStates[attribute] = attribute2;
			xml.DoneWithElement();
		}
	}

	protected void HandleBoolGameStateNode(XmlDataHelper xml)
	{
		string attribute = xml.GetAttribute("Name");
		string attribute2 = xml.GetAttribute("Value");
		if (attribute == null)
		{
			xml.ParseWarning("No Name provided for boolgamestate node.");
			xml.DoneWithElement();
		}
		else if (attribute2 == null)
		{
			xml.ParseWarning("No Value provided for boolgamestate node Name=" + attribute + ".");
			xml.DoneWithElement();
		}
		else
		{
			CurrentReadingGameModeDescriptor.boolGameStates[attribute] = attribute2.EqualsNoCase("TRUE");
			xml.DoneWithElement();
		}
	}

	protected void HandleIntGameStateNode(XmlDataHelper xml)
	{
		string attribute = xml.GetAttribute("Name");
		string attribute2 = xml.GetAttribute("Value");
		int result;
		if (attribute == null)
		{
			xml.ParseWarning("No Name provided for intgamestate node.");
			xml.DoneWithElement();
		}
		else if (attribute2 == null)
		{
			xml.ParseWarning("No Value provided for intgamestate node Name=" + attribute + ".");
			xml.DoneWithElement();
		}
		else if (!int.TryParse(attribute2, out result))
		{
			xml.ParseWarning("Failed parsing '" + attribute2 + "' provided for Int32GameState '" + attribute + "'.");
			xml.DoneWithElement();
		}
		else
		{
			CurrentReadingGameModeDescriptor.intGameStates[attribute] = result;
			xml.DoneWithElement();
		}
	}

	protected void HandleInt64GameStateNode(XmlDataHelper xml)
	{
		string attribute = xml.GetAttribute("Name");
		string attribute2 = xml.GetAttribute("Value");
		long result;
		if (attribute == null)
		{
			xml.ParseWarning("No Name provided for int64gamestate node.");
			xml.DoneWithElement();
		}
		else if (attribute2 == null)
		{
			xml.ParseWarning("No Value provided for int64gamestate node Name=" + attribute + ".");
			xml.DoneWithElement();
		}
		else if (!long.TryParse(attribute2, out result))
		{
			xml.ParseWarning("Failed parsing '" + attribute2 + "' provided for Int64GameState '" + attribute + "'.");
			xml.DoneWithElement();
		}
		else
		{
			CurrentReadingGameModeDescriptor.int64GameStates[attribute] = result;
			xml.DoneWithElement();
		}
	}

	public override void InitFromSeed(string seed)
	{
		throw new NotImplementedException();
	}

	public override void bootGame(XRLGame game, EmbarkInfo info)
	{
		game.gameMode = info.getData<QudGamemodeModuleData>().Mode;
		foreach (KeyValuePair<string, string> stringGameState in selectedMode.stringGameStates)
		{
			game.SetStringGameState(stringGameState.Key, stringGameState.Value);
		}
		foreach (KeyValuePair<string, bool> boolGameState in selectedMode.boolGameStates)
		{
			game.SetBooleanGameState(boolGameState.Key, boolGameState.Value);
		}
		foreach (KeyValuePair<string, int> intGameState in selectedMode.intGameStates)
		{
			game.SetIntGameState(intGameState.Key, intGameState.Value);
		}
		foreach (KeyValuePair<string, long> int64GameState in selectedMode.int64GameStates)
		{
			game.SetInt64GameState(int64GameState.Key, int64GameState.Value);
		}
		foreach (string gameSystem in selectedMode.gameSystems)
		{
			game.AddSystem(gameSystem);
		}
	}
}
