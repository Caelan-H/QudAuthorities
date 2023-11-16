using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.CharacterBuilds.Qud;

public class QudChooseStartingLocationModule : EmbarkBuilderModule<QudChooseStartingLocationModuleData>
{
	private StartingLocationData currentReadingStartingLocationData;

	public Dictionary<string, StartingLocationData> startingLocations = new Dictionary<string, StartingLocationData>();

	public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
	{
		get
		{
			Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
			xmlNodes.Add("locations", delegate(XmlDataHelper xml)
			{
				xml.HandleNodes(XmlNodeHandlers);
			});
			return xmlNodes;
		}
	}

	public Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
	{
		{
			"location",
			delegate(XmlDataHelper xml)
			{
				string attribute = xml.GetAttribute("ID");
				if (!startingLocations.TryGetValue(attribute, out currentReadingStartingLocationData))
				{
					currentReadingStartingLocationData = new StartingLocationData
					{
						Id = attribute
					};
					startingLocations.Add(attribute, currentReadingStartingLocationData);
				}
				currentReadingStartingLocationData.Name = xml.GetAttribute("Name");
				currentReadingStartingLocationData.Location = xml.GetAttribute("Location");
				xml.HandleNodes(XmlNodeHandlers);
				currentReadingStartingLocationData = null;
			}
		},
		{
			"description",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.Description = xml.GetTextNode();
			}
		},
		{
			"stringgamestate",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.stringGameStates[xml.GetAttribute("Name")] = xml.GetAttribute("Value");
				xml.DoneWithElement();
			}
		},
		{
			"item",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.items.Add(new StartingLocationItem
				{
					Blueprint = xml.GetAttribute("Blueprint"),
					Number = ((xml.GetAttribute("Number") == null) ? 1 : int.Parse(xml.GetAttribute("Number")))
				});
				xml.DoneWithElement();
			}
		},
		{
			"skill",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.skills.Add(new StartingLocationSkill
				{
					Class = xml.GetAttribute("Class")
				});
				xml.DoneWithElement();
			}
		},
		{
			"reputation",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.reputations.Add(new StartingLocationReputation
				{
					Faction = xml.GetAttribute("Faction"),
					Modifier = ((xml.GetAttribute("Modifier") != null) ? int.Parse(xml.GetAttribute("Modifier")) : 0)
				});
				xml.DoneWithElement();
			}
		},
		{
			"grid",
			delegate(XmlDataHelper xml)
			{
				currentReadingStartingLocationData.grid[xml.GetAttribute("Position")] = new StartingLocationGridElement
				{
					Tile = xml.GetAttribute("Tile"),
					Background = xml.GetAttribute("Background"),
					Detail = xml.GetAttribute("Detail"),
					Foreground = xml.GetAttribute("Foreground")
				};
				xml.DoneWithElement();
			}
		}
	};

	public StartingLocationData startingLocation => startingLocations.Values.Where((StartingLocationData s) => s.Id == base.data.StartingLocation).FirstOrDefault();

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
	}

	public override bool shouldBeEditable()
	{
		return builder?.GetModule<QudChartypeModule>()?.data?.type != "Daily";
	}

	public override void InitFromSeed(string seed)
	{
	}

	public override void bootGame(XRLGame game, EmbarkInfo info)
	{
	}

	private static void AddItem(GameObject GO, GameObject player)
	{
		GO.Seen();
		GO.MakeUnderstood();
		GO.GetPart<EnergyCellSocket>()?.Cell?.MakeUnderstood();
		player.ReceiveObject(GO);
	}

	private static void AddItem(string Blueprint, GameObject player)
	{
		try
		{
			AddItem(GameObject.create(Blueprint), player);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	private static void AddItem(string Blueprint, int number, GameObject player)
	{
		try
		{
			for (int i = 0; i < number; i++)
			{
				AddItem(GameObject.create(Blueprint), player);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception creating " + Blueprint, x);
		}
	}

	private static void AddSkill(string Class, GameObject player)
	{
		object obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + Class));
		(player.GetPart("Skills") as Skills).AddSkill(obj as BaseSkill);
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element)
	{
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject player = element as GameObject;
			foreach (StartingLocationSkill skill in startingLocation.skills)
			{
				AddSkill(skill.Class, player);
			}
			foreach (StartingLocationItem item in startingLocation.items)
			{
				for (int i = 0; i < item.Number; i++)
				{
					AddItem(item.Blueprint, player);
				}
			}
			foreach (StartingLocationReputation reputation in startingLocation.reputations)
			{
				The.Game.PlayerReputation.modify(reputation.Faction, reputation.Modifier, null, null, silent: true);
			}
		}
		if (id == QudGameBootModule.BOOTEVENT_BEFOREINITIALIZEHISTORY)
		{
			game.SetStringGameState("embark", info.getData<QudSubtypeModuleData>()?.Entry?.StartingLocation ?? info.getData<QudGenotypeModuleData>()?.Entry?.StartingLocation ?? base.data.StartingLocation);
			foreach (KeyValuePair<string, string> stringGameState in startingLocation.stringGameStates)
			{
				game.SetStringGameState(stringGameState.Key, stringGameState.Value);
			}
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTSTARTINGLOCATION)
		{
			string text = info.getData<QudSubtypeModuleData>()?.Entry?.StartingLocation ?? info.getData<QudGenotypeModuleData>()?.Entry?.StartingLocation;
			if (!text.IsNullOrEmpty())
			{
				return new GlobalLocation(text);
			}
			text = startingLocation.Location;
			if (!text.IsNullOrEmpty())
			{
				if (text.StartsWith("GlobalLocation:"))
				{
					return new GlobalLocation(text.Split(':')[1]);
				}
				if (text.StartsWith("StringGameState:"))
				{
					return new GlobalLocation(game.GetStringGameState(text.Split(':')[1], "JoppaWorld.11.22.1.1.10@37,22"));
				}
			}
			MetricsManager.LogError("unknown starting location specification:" + text);
			throw new ArgumentException("Starting location was not properly defined for QudChooseStartingLocationModule.");
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public string getSelected()
	{
		return base.data?.StartingLocation;
	}
}
public class QudChooseStartingLocationModuleData : AbstractEmbarkBuilderModuleData
{
	public string StartingLocation;

	public QudChooseStartingLocationModuleData()
	{
	}

	public QudChooseStartingLocationModuleData(string startingLocation)
	{
		StartingLocation = startingLocation;
	}
}
