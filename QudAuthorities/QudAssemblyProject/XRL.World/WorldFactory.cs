using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using XRL.Core;
using XRL.UI;

namespace XRL.World;

public class WorldFactory
{
	public const string ANY_PLANE = "*";

	public static WorldFactory _Factory;

	private List<WorldBlueprint> worlds = new List<WorldBlueprint>();

	private Dictionary<string, WorldBlueprint> Worlds = new Dictionary<string, WorldBlueprint>();

	private Dictionary<string, CellBlueprint> Cells = new Dictionary<string, CellBlueprint>();

	public Dictionary<string, string> ZoneIDToDisplay = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneDisplayToID = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneIDToDisplayWithIndefiniteArticle = new Dictionary<string, string>();

	public Dictionary<string, string> ZoneDisplayToIDWithIndefiniteArticle = new Dictionary<string, string>();

	public List<IWorldBuilderExtension> worldBuilderExtensions;

	public static WorldFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				_Factory = new WorldFactory();
			}
			return _Factory;
		}
	}

	public int countWorlds()
	{
		int num = Worlds.Keys.Count;
		if (XRLCore.Core != null && XRLCore.Core.Game != null && XRLCore.Core.Game.HasObjectGameState("AdditionalWorld"))
		{
			Dictionary<string, WorldBlueprint> dictionary = XRLCore.Core.Game.GetObjectGameState("AdditionalWorld") as Dictionary<string, WorldBlueprint>;
			num += dictionary.Keys.Count;
		}
		return num;
	}

	public bool hasWorld(string id)
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null && XRLCore.Core.Game.HasObjectGameState("AdditionalWorld") && (XRLCore.Core.Game.GetObjectGameState("AdditionalWorld") as Dictionary<string, WorldBlueprint>).ContainsKey(id))
		{
			return true;
		}
		return Worlds.ContainsKey(id);
	}

	public void addAdditionalWorld(string id, WorldBlueprint world)
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null)
		{
			if (!XRLCore.Core.Game.HasObjectGameState("AdditionalWorld"))
			{
				XRLCore.Core.Game.ObjectGameState.Add("AdditionalWorlds", new Dictionary<string, WorldBlueprint>());
			}
			Dictionary<string, WorldBlueprint> dictionary = XRLCore.Core.Game.GetObjectGameState("AdditionalWorld") as Dictionary<string, WorldBlueprint>;
			if (dictionary.ContainsKey(id))
			{
				dictionary[id] = world;
			}
			else
			{
				dictionary.Add(id, world);
			}
		}
	}

	public List<WorldBlueprint> getWorlds()
	{
		worlds.Clear();
		if (XRLCore.Core != null && XRLCore.Core.Game != null && XRLCore.Core.Game.HasObjectGameState("AdditionalWorld") && XRLCore.Core.Game.GetObjectGameState("AdditionalWorld") is Dictionary<string, WorldBlueprint> dictionary)
		{
			worlds.AddRange(dictionary.Values);
		}
		worlds.AddRange(Worlds.Values);
		return worlds;
	}

	public WorldBlueprint getWorld(string id)
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null && XRLCore.Core.Game.HasObjectGameState("AdditionalWorld") && (XRLCore.Core.Game.GetObjectGameState("AdditionalWorld") as Dictionary<string, WorldBlueprint>).TryGetValue(id, out var value))
		{
			return value;
		}
		if (Worlds.TryGetValue(id, out value))
		{
			return value;
		}
		return null;
	}

	public void BuildWorlds()
	{
		foreach (string key in Worlds.Keys)
		{
			BuildWorld(key);
		}
	}

	public void BuildWorld(string world)
	{
		if (worldBuilderExtensions == null)
		{
			worldBuilderExtensions = ModManager.GetInstancesWithAttribute<IWorldBuilderExtension>(typeof(WorldBuilderExtension));
		}
		foreach (ZoneBuilderBlueprint builder in Worlds[world].Builders)
		{
			string text = "XRL.World.WorldBuilders." + builder.Class;
			Type type = ModManager.ResolveType(text);
			if (type == null)
			{
				XRLCore.LogError("Unknown world builder " + text + "!");
				break;
			}
			object NewBuilder = Activator.CreateInstance(type);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (builder.Parameters == null)
				{
					builder.Parameters = new Dictionary<string, object>();
				}
				if (builder.Parameters.ContainsKey(fieldInfo.Name))
				{
					if (fieldInfo.FieldType == typeof(bool))
					{
						fieldInfo.SetValue(NewBuilder, Convert.ToBoolean(builder.Parameters[fieldInfo.Name]));
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						fieldInfo.SetValue(NewBuilder, Convert.ToInt32(builder.Parameters[fieldInfo.Name]));
					}
					else if (fieldInfo.FieldType == typeof(short))
					{
						fieldInfo.SetValue(NewBuilder, Convert.ToInt16(builder.Parameters[fieldInfo.Name]));
					}
					else
					{
						fieldInfo.SetValue(NewBuilder, builder.Parameters[fieldInfo.Name]);
					}
				}
			}
			worldBuilderExtensions.ForEach(delegate(IWorldBuilderExtension e)
			{
				e.OnBeforeBuild(world, NewBuilder);
			});
			MethodInfo method = type.GetMethod("BuildWorld");
			if (method != null && !(bool)method.Invoke(NewBuilder, new object[1] { world }))
			{
				break;
			}
			worldBuilderExtensions.ForEach(delegate(IWorldBuilderExtension e)
			{
				e.OnAfterBuild(world, NewBuilder);
			});
			NewBuilder = null;
			MemoryHelper.GCCollect();
		}
	}

	public string ZoneDisplayName(string ID)
	{
		if (ZoneIDToDisplay.TryGetValue(ID, out var value))
		{
			return value;
		}
		value = The.ZoneManager.GetZoneDisplayName(ID);
		ZoneIDToDisplay.Add(ID, value);
		return value;
	}

	public string ZoneDisplayNameWithIndefiniteArticle(string ID)
	{
		if (ZoneIDToDisplayWithIndefiniteArticle.TryGetValue(ID, out var value))
		{
			return value;
		}
		value = The.ZoneManager.GetZoneDisplayName(ID, WithIndefiniteArticle: true);
		ZoneIDToDisplayWithIndefiniteArticle.Add(ID, value);
		return value;
	}

	public void UpdateZoneDisplayName(string ID, string Name)
	{
		if (ZoneIDToDisplay.TryGetValue(ID, out var value))
		{
			ZoneIDToDisplay[ID] = Name;
			string value2 = null;
			if (ZoneDisplayToID.TryGetValue(value, out value2) && value2 == ID)
			{
				ZoneDisplayToID.Remove(value);
				ZoneDisplayToID.Set(Name, ID);
			}
		}
		if (ZoneIDToDisplayWithIndefiniteArticle.TryGetValue(ID, out value))
		{
			ZoneIDToDisplayWithIndefiniteArticle[ID] = Name;
			string value3 = null;
			if (ZoneDisplayToIDWithIndefiniteArticle.TryGetValue(value, out value3) && value3 == ID)
			{
				ZoneDisplayToIDWithIndefiniteArticle.Remove(value);
				ZoneDisplayToIDWithIndefiniteArticle.Set(Name, ID);
			}
		}
	}

	public void BuildZoneNameMap()
	{
		ZoneIDToDisplay = new Dictionary<string, string>();
		ZoneDisplayToID = new Dictionary<string, string>();
		ZoneIDToDisplayWithIndefiniteArticle = new Dictionary<string, string>();
		ZoneDisplayToIDWithIndefiniteArticle = new Dictionary<string, string>();
		if (Options.EnableWishRegionNames)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder(512);
		StringBuilder stringBuilder2 = new StringBuilder(512);
		StringBuilder stringBuilder3 = new StringBuilder(512);
		StringBuilder stringBuilder4 = new StringBuilder(512);
		StringBuilder stringBuilder5 = new StringBuilder(512);
		int num = 0;
		foreach (string key in Factory.Worlds.Keys)
		{
			num++;
			for (int i = 0; i < 80; i++)
			{
				if (i % 2 == 0)
				{
					WorldCreationProgress.StepProgress("Initializing world " + num);
				}
				stringBuilder.Length = 0;
				stringBuilder.Append(key);
				stringBuilder.Append(".");
				stringBuilder.Append(i);
				stringBuilder.Append(".");
				for (int j = 0; j < 25; j++)
				{
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder);
					stringBuilder2.Append(j);
					stringBuilder2.Append(".");
					for (int k = 0; k < 3; k++)
					{
						stringBuilder3.Length = 0;
						stringBuilder3.Append(stringBuilder2);
						stringBuilder3.Append(k);
						stringBuilder3.Append(".");
						for (int l = 0; l < 3; l++)
						{
							stringBuilder4.Length = 0;
							stringBuilder4.Append(stringBuilder3);
							stringBuilder4.Append(l);
							stringBuilder4.Append(".");
							for (int m = 10; m <= 10; m++)
							{
								stringBuilder5.Length = 0;
								stringBuilder5.Append(stringBuilder4);
								stringBuilder5.Append(m);
								string text = stringBuilder5.ToString();
								string zoneDisplayName = The.ZoneManager.GetZoneDisplayName(text, key, i, j, k, l, m);
								if (!ZoneDisplayToID.ContainsKey(zoneDisplayName))
								{
									ZoneDisplayToID.Add(zoneDisplayName, text);
								}
							}
						}
					}
				}
			}
		}
	}

	public void Init()
	{
		Worlds = new Dictionary<string, WorldBlueprint>();
		Cells = new Dictionary<string, CellBlueprint>();
		List<string> Paths = new List<string>();
		Paths.Add(DataManager.FilePath("Worlds.xml"));
		ModManager.ForEachFile("Worlds.xml", delegate(string path)
		{
			Paths.Add(path);
		});
		foreach (string item in Paths)
		{
			XmlTextReader streamingAssetsXMLStream = DataManager.GetStreamingAssetsXMLStream(item);
			streamingAssetsXMLStream.WhitespaceHandling = WhitespaceHandling.None;
			while (streamingAssetsXMLStream.Read())
			{
				if (streamingAssetsXMLStream.Name == "worlds")
				{
					LoadWorldsNode(streamingAssetsXMLStream);
				}
			}
			streamingAssetsXMLStream.Close();
		}
	}

	public void LoadWorldsNode(XmlTextReader Reader)
	{
		while (Reader.Read())
		{
			if (!(Reader.Name == "world"))
			{
				continue;
			}
			WorldBlueprint worldBlueprint = LoadWorldNode(Reader);
			if (Worlds.ContainsKey(worldBlueprint.Name))
			{
				foreach (ZoneBuilderBlueprint Builder in worldBlueprint.Builders)
				{
					if (Builder.Class.StartsWith("-"))
					{
						string builderToRemove = Builder.Class.Substring(1);
						Worlds[worldBlueprint.Name].Builders.RemoveAll((ZoneBuilderBlueprint b) => b.Class == builderToRemove);
					}
					else
					{
						Worlds[worldBlueprint.Name].Builders.RemoveAll((ZoneBuilderBlueprint b) => b.Class == Builder.Class);
						Worlds[worldBlueprint.Name].Builders.Add(Builder);
					}
				}
				foreach (string key in worldBlueprint.CellBlueprintsByName.Keys)
				{
					if (Worlds[worldBlueprint.Name].CellBlueprintsByName.ContainsKey(key))
					{
						Worlds[worldBlueprint.Name].CellBlueprintsByApplication.Remove(Worlds[worldBlueprint.Name].CellBlueprintsByName[key].ApplyTo);
						Worlds[worldBlueprint.Name].CellBlueprintsByName[key] = worldBlueprint.CellBlueprintsByName[key];
						if (!Worlds[worldBlueprint.Name].CellBlueprintsByApplication.ContainsKey(worldBlueprint.CellBlueprintsByName[key].ApplyTo))
						{
							Worlds[worldBlueprint.Name].CellBlueprintsByApplication.Add(worldBlueprint.CellBlueprintsByName[key].ApplyTo, null);
						}
						Worlds[worldBlueprint.Name].CellBlueprintsByApplication[worldBlueprint.CellBlueprintsByName[key].ApplyTo] = worldBlueprint.CellBlueprintsByName[key];
					}
					else
					{
						Worlds[worldBlueprint.Name].CellBlueprintsByName.Add(key, worldBlueprint.CellBlueprintsByName[key]);
						Worlds[worldBlueprint.Name].CellBlueprintsByApplication.Add(worldBlueprint.CellBlueprintsByName[key].ApplyTo, worldBlueprint.CellBlueprintsByName[key]);
					}
				}
			}
			else
			{
				Worlds.Add(worldBlueprint.Name, worldBlueprint);
			}
		}
	}

	public WorldBlueprint LoadWorldNode(XmlTextReader Reader)
	{
		WorldBlueprint worldBlueprint = new WorldBlueprint();
		worldBlueprint.DisplayName = Reader.GetAttribute("DisplayName");
		worldBlueprint.Name = Reader.GetAttribute("Name");
		worldBlueprint.Map = Reader.GetAttribute("Map");
		worldBlueprint.ZoneFactory = Reader.GetAttribute("ZoneFactory");
		worldBlueprint.ZoneFactoryRegex = Reader.GetAttribute("ZoneFactoryRegex");
		string attribute = Reader.GetAttribute("Plane");
		if (!string.IsNullOrEmpty(attribute))
		{
			worldBlueprint.Plane = attribute;
		}
		string attribute2 = Reader.GetAttribute("Protocol");
		if (!string.IsNullOrEmpty(attribute2))
		{
			worldBlueprint.Protocol = attribute2;
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return worldBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "builder")
			{
				ZoneBuilderBlueprint item = LoadBuilderNode(Reader, "builder");
				worldBlueprint.Builders.Add(item);
			}
			if (Reader.Name == "cell")
			{
				CellBlueprint cellBlueprint = LoadCellNode(Reader, worldBlueprint);
				worldBlueprint.CellBlueprintsByApplication[cellBlueprint.ApplyTo] = cellBlueprint;
				worldBlueprint.CellBlueprintsByName[cellBlueprint.Name] = cellBlueprint;
				Cells[worldBlueprint.Name + cellBlueprint.Name] = cellBlueprint;
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "world"))
			{
				return worldBlueprint;
			}
		}
		return worldBlueprint;
	}

	private void ParseRangeSpec(string Spec, out int Start, out int End)
	{
		if (Spec.IndexOf('-') != -1)
		{
			string[] array = Spec.Split('-');
			Start = Convert.ToInt32(array[0]);
			End = Convert.ToInt32(array[1]);
		}
		else
		{
			Start = Convert.ToInt32(Spec);
			End = Convert.ToInt32(Spec);
		}
	}

	public CellBlueprint LoadCellNode(XmlTextReader Reader, WorldBlueprint World)
	{
		CellBlueprint cellBlueprint = new CellBlueprint();
		cellBlueprint.LandingZone = "1,1";
		cellBlueprint.Name = Reader.GetAttribute("Name");
		cellBlueprint.Inherits = Reader.GetAttribute("Inherits");
		cellBlueprint.ApplyTo = Reader.GetAttribute("ApplyTo");
		string attribute = Reader.GetAttribute("Mutable");
		if (attribute != null && !attribute.EqualsNoCase("true"))
		{
			cellBlueprint.Mutable = false;
		}
		string attribute2 = Reader.GetAttribute("LandingZone");
		if (attribute2 != null)
		{
			cellBlueprint.LandingZone = attribute2;
		}
		string attribute3 = Reader.GetAttribute("Tier");
		if (attribute3.IsNullOrEmpty() || !int.TryParse(attribute3, out var result))
		{
			result = 0;
		}
		if (cellBlueprint.Inherits != null && Cells.TryGetValue(World.Name + cellBlueprint.Inherits, out var value) && value != null)
		{
			cellBlueprint.CopyFrom(value);
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return cellBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "zone")
			{
				ZoneBlueprint zoneBlueprint = LoadZoneNode(Reader);
				if (result > 0 && zoneBlueprint.Tier <= 0)
				{
					zoneBlueprint.Tier = result;
				}
				int Start = 0;
				int End = 0;
				int Start2 = 0;
				int End2 = 0;
				int Start3 = 0;
				int End3 = 0;
				ParseRangeSpec(zoneBlueprint.Level, out Start, out End);
				ParseRangeSpec(zoneBlueprint.x, out Start2, out End2);
				ParseRangeSpec(zoneBlueprint.y, out Start3, out End3);
				for (int i = Start2; i <= End2; i++)
				{
					for (int j = Start3; j <= End3; j++)
					{
						for (int k = Start; k <= End; k++)
						{
							cellBlueprint.LevelBlueprint[i, j, k] = zoneBlueprint;
						}
					}
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "cell"))
			{
				return cellBlueprint;
			}
		}
		return cellBlueprint;
	}

	public ZoneBlueprint LoadZoneNode(XmlTextReader Reader)
	{
		ZoneBlueprint zoneBlueprint = new ZoneBlueprint(null);
		zoneBlueprint.Level = Reader.GetAttribute("Level");
		zoneBlueprint.x = Reader.GetAttribute("x");
		zoneBlueprint.y = Reader.GetAttribute("y");
		zoneBlueprint.disableForcedConnections = Reader.GetAttribute("DisableForcedConnections").EqualsNoCase("Yes");
		zoneBlueprint.ProperName = Reader.GetAttribute("ProperName").EqualsNoCase("true");
		zoneBlueprint.NameContext = Reader.GetAttribute("NameContext");
		zoneBlueprint.IndefiniteArticle = Reader.GetAttribute("IndefiniteArticle");
		zoneBlueprint.DefiniteArticle = Reader.GetAttribute("DefiniteArticle");
		zoneBlueprint.IncludeContextInZoneDisplay = (Reader.GetAttribute("IncludeContextInZoneDisplay") ?? "true").EqualsNoCase("true");
		zoneBlueprint.IncludeStratumInZoneDisplay = (Reader.GetAttribute("IncludeStratumInZoneDisplay") ?? "true").EqualsNoCase("true");
		zoneBlueprint.HasWeather = Reader.GetAttribute("HasWeather").EqualsNoCase("true");
		zoneBlueprint.WindSpeed = Reader.GetAttribute("WindSpeed");
		zoneBlueprint.WindDirections = Reader.GetAttribute("WindDirections");
		zoneBlueprint.WindDuration = Reader.GetAttribute("WindDuration");
		string attribute = Reader.GetAttribute("GroundLiquid");
		if (attribute != null)
		{
			zoneBlueprint.GroundLiquid = attribute;
		}
		string attribute2 = Reader.GetAttribute("Name");
		if (attribute2 != null)
		{
			zoneBlueprint.Name = attribute2;
		}
		string attribute3 = Reader.GetAttribute("Tier");
		if (!attribute3.IsNullOrEmpty() && int.TryParse(attribute3, out var result))
		{
			zoneBlueprint.Tier = result;
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "population")
			{
				ZoneBuilderBlueprint item = LoadPopulationNode(Reader, "population");
				zoneBlueprint.Builders.Add(item);
			}
			if (Reader.Name == "builder")
			{
				ZoneBuilderBlueprint item2 = LoadBuilderNode(Reader, "builder");
				zoneBlueprint.Builders.Add(item2);
			}
			if (Reader.Name == "postbuilder")
			{
				ZoneBuilderBlueprint item3 = LoadBuilderNode(Reader, "postbuilder");
				zoneBlueprint.PostBuilders.Add(item3);
			}
			if (Reader.Name == "encounter")
			{
				ZoneEncounterBlueprint item4 = LoadEncounterNode(Reader);
				zoneBlueprint.Encounters.Add(item4);
			}
			if (Reader.Name == "map")
			{
				ZoneMapBlueprint item5 = LoadMapNode(Reader);
				zoneBlueprint.Maps.Add(item5);
			}
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "zone"))
			{
				return zoneBlueprint;
			}
		}
		return zoneBlueprint;
	}

	public ZoneMapBlueprint LoadMapNode(XmlTextReader Reader)
	{
		ZoneMapBlueprint zoneMapBlueprint = new ZoneMapBlueprint();
		zoneMapBlueprint.File = Reader.GetAttribute("FileName");
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneMapBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "map"))
			{
				return zoneMapBlueprint;
			}
		}
		return zoneMapBlueprint;
	}

	public ZoneFeature LoadFeatureNode(XmlTextReader Reader)
	{
		ZoneFeature zoneFeature = new ZoneFeature();
		zoneFeature.Name = Reader.GetAttribute("Name");
		while (Reader.MoveToNextAttribute())
		{
			zoneFeature.Properties.Add(Reader.Name, Reader.Value);
		}
		Reader.MoveToElement();
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneFeature;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == "feature"))
			{
				return zoneFeature;
			}
		}
		return zoneFeature;
	}

	public ZoneBuilderBlueprint LoadPopulationNode(XmlTextReader Reader, string openTag)
	{
		ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint();
		zoneBuilderBlueprint.Class = "Population";
		while (Reader.MoveToNextAttribute())
		{
			zoneBuilderBlueprint.AddParameter(Reader.Name, Reader.Value);
		}
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneBuilderBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == openTag))
			{
				return zoneBuilderBlueprint;
			}
		}
		return zoneBuilderBlueprint;
	}

	public ZoneBuilderBlueprint LoadBuilderNode(XmlTextReader Reader, string openTag)
	{
		ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint();
		zoneBuilderBlueprint.Class = Reader.GetAttribute("Class");
		while (Reader.MoveToNextAttribute())
		{
			zoneBuilderBlueprint.AddParameter(Reader.Name, Reader.Value);
		}
		Reader.MoveToElement();
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneBuilderBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.EndElement && (Reader.Name == "" || Reader.Name == openTag))
			{
				return zoneBuilderBlueprint;
			}
		}
		return zoneBuilderBlueprint;
	}

	public ZoneEncounterBlueprint LoadEncounterNode(XmlTextReader Reader)
	{
		ZoneEncounterBlueprint zoneEncounterBlueprint = new ZoneEncounterBlueprint();
		zoneEncounterBlueprint.Table = Reader.GetAttribute("Table");
		zoneEncounterBlueprint.Amount = Reader.GetAttribute("Amount");
		while (Reader.MoveToNextAttribute())
		{
			zoneEncounterBlueprint.Parameters.Add(Reader.Name, Reader.Value);
		}
		Reader.MoveToElement();
		if (Reader.NodeType == XmlNodeType.EndElement || Reader.IsEmptyElement)
		{
			return zoneEncounterBlueprint;
		}
		while (Reader.Read())
		{
			if (Reader.Name == "feature")
			{
				zoneEncounterBlueprint.Features.Add(LoadFeatureNode(Reader));
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "encounter")
			{
				return zoneEncounterBlueprint;
			}
		}
		return zoneEncounterBlueprint;
	}
}
