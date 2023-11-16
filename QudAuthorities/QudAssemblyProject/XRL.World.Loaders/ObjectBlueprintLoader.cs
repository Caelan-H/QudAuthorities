using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XRL.World.Loaders;

public class ObjectBlueprintLoader
{
	public class ObjectBlueprintXMLChildNode
	{
		public string NodeName;

		public Dictionary<string, string> Attributes = new Dictionary<string, string>();

		public string Name
		{
			get
			{
				if (Attributes.ContainsKey("Name"))
				{
					return Attributes["Name"];
				}
				return null;
			}
		}

		public ObjectBlueprintXMLChildNode(string NodeName)
		{
			this.NodeName = NodeName;
		}

		public string GetAttribute(string name)
		{
			if (Attributes.ContainsKey(name))
			{
				return Attributes[name];
			}
			return null;
		}

		public void Merge(ObjectBlueprintXMLChildNode other)
		{
			NodeName = other.NodeName;
			foreach (KeyValuePair<string, string> attribute in other.Attributes)
			{
				Attributes[attribute.Key] = attribute.Value;
			}
		}

		public ObjectBlueprintXMLChildNode Clone()
		{
			ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = new ObjectBlueprintXMLChildNode(NodeName);
			foreach (KeyValuePair<string, string> attribute in Attributes)
			{
				objectBlueprintXMLChildNode.Attributes[attribute.Key] = attribute.Value;
			}
			return objectBlueprintXMLChildNode;
		}

		public static ObjectBlueprintXMLChildNode ReadChildNode(XmlTextReader reader)
		{
			ObjectBlueprintXMLChildNode objectBlueprintXMLChildNode = new ObjectBlueprintXMLChildNode(reader.Name);
			if (reader.HasAttributes)
			{
				reader.MoveToFirstAttribute();
				do
				{
					objectBlueprintXMLChildNode.Attributes[reader.Name] = reader.Value;
				}
				while (reader.MoveToNextAttribute());
				reader.MoveToElement();
			}
			if (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement)
			{
				return objectBlueprintXMLChildNode;
			}
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.EndElement)
				{
					if (reader.Name != "" && reader.Name != objectBlueprintXMLChildNode.NodeName)
					{
						throw new Exception("Unexpected end node for " + reader.Name);
					}
					return objectBlueprintXMLChildNode;
				}
			}
			return objectBlueprintXMLChildNode;
		}
	}

	public class ObjectBlueprintXMLChildNodeCollection
	{
		public Dictionary<string, ObjectBlueprintXMLChildNode> Named;

		public List<ObjectBlueprintXMLChildNode> Unnamed;

		public void Add(ObjectBlueprintXMLChildNode node, XmlTextReader reader)
		{
			if (string.IsNullOrEmpty(node.Name))
			{
				if (Unnamed == null)
				{
					Unnamed = new List<ObjectBlueprintXMLChildNode>(1);
				}
				Unnamed.Add(node);
				return;
			}
			if (Named == null)
			{
				Named = new Dictionary<string, ObjectBlueprintXMLChildNode>(1);
			}
			if (Named.ContainsKey(node.Name) && reader != null)
			{
				handleError($"{reader.BaseURI}: Duplicate {node.NodeName} Name='{node.Name}' found at line {reader.LineNumber}");
				Named[node.Name].Merge(node);
			}
			else
			{
				Named[node.Name] = node;
			}
		}

		public ObjectBlueprintXMLChildNodeCollection Clone()
		{
			ObjectBlueprintXMLChildNodeCollection objectBlueprintXMLChildNodeCollection = new ObjectBlueprintXMLChildNodeCollection();
			if (Named != null)
			{
				objectBlueprintXMLChildNodeCollection.Named = new Dictionary<string, ObjectBlueprintXMLChildNode>(Named.Count);
				foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in Named)
				{
					objectBlueprintXMLChildNodeCollection.Named[item.Key] = item.Value.Clone();
				}
			}
			if (Unnamed != null)
			{
				objectBlueprintXMLChildNodeCollection.Unnamed = new List<ObjectBlueprintXMLChildNode>(Unnamed.Count);
				{
					foreach (ObjectBlueprintXMLChildNode item2 in Unnamed)
					{
						objectBlueprintXMLChildNodeCollection.Unnamed.Add(item2.Clone());
					}
					return objectBlueprintXMLChildNodeCollection;
				}
			}
			return objectBlueprintXMLChildNodeCollection;
		}

		public override string ToString()
		{
			string text = "";
			if (Named != null)
			{
				text = text + "Named: " + Named.Count + "\n";
				foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in Named)
				{
					text = text + "  [" + item.Key + " ";
					foreach (KeyValuePair<string, string> attribute in item.Value.Attributes)
					{
						text = text + attribute.Key + "=\"" + attribute.Value + "\"";
					}
					text += "]\n";
				}
			}
			if (Unnamed != null)
			{
				text = text + "Unnamed: " + Unnamed.Count + "\n";
				{
					foreach (ObjectBlueprintXMLChildNode item2 in Unnamed)
					{
						text += "  [";
						foreach (KeyValuePair<string, string> attribute2 in item2.Attributes)
						{
							text = text + attribute2.Key + "=\"" + attribute2.Value + "\"";
						}
						text += "]\n";
					}
					return text;
				}
			}
			return text;
		}

		public void Merge(ObjectBlueprintXMLChildNodeCollection other)
		{
			if (other.Named != null)
			{
				if (Named == null)
				{
					Named = other.Named;
				}
				else
				{
					foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in other.Named)
					{
						if (Named.TryGetValue(item.Key, out var value))
						{
							value.Merge(item.Value);
						}
						else
						{
							Add(item.Value, null);
						}
					}
				}
			}
			if (other.Unnamed != null)
			{
				if (Unnamed == null)
				{
					Unnamed = other.Unnamed;
				}
				else
				{
					Unnamed.AddRange(other.Unnamed);
				}
			}
		}
	}

	public class ObjectBlueprintXMLData
	{
		public string Name;

		public string Inherits;

		public string Load;

		public ModInfo Mod;

		public Dictionary<string, ObjectBlueprintXMLChildNodeCollection> Children = new Dictionary<string, ObjectBlueprintXMLChildNodeCollection>();

		public IEnumerable<KeyValuePair<string, ObjectBlueprintXMLChildNode>> NamedNodes(string type)
		{
			if (Children.ContainsKey(type))
			{
				if (Children[type].Unnamed != null)
				{
					MetricsManager.LogWarning("Unnamed " + type + " nodes detected in " + Name);
				}
				return Children[type].Named.AsEnumerable();
			}
			return Enumerable.Empty<KeyValuePair<string, ObjectBlueprintXMLChildNode>>();
		}

		public IEnumerable<ObjectBlueprintXMLChildNode> UnnamedNodes(string type)
		{
			if (Children.ContainsKey(type))
			{
				if (Children[type].Named != null)
				{
					MetricsManager.LogWarning("Named " + type + " nodes detected in " + Name);
				}
				return Children[type].Unnamed.AsEnumerable();
			}
			return Enumerable.Empty<ObjectBlueprintXMLChildNode>();
		}

		public void Merge(ObjectBlueprintXMLData other)
		{
			if (!string.IsNullOrEmpty(other.Inherits))
			{
				Inherits = other.Inherits;
			}
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in other.Children)
			{
				if (Children.TryGetValue(child.Key, out var value))
				{
					value.Merge(child.Value);
				}
				else
				{
					Children[child.Key] = child.Value;
				}
			}
			if (other.Mod != null)
			{
				Mod = other.Mod;
			}
		}

		public static ObjectBlueprintXMLData ReadObjectNode(XmlTextReader reader)
		{
			ObjectBlueprintXMLData objectBlueprintXMLData = new ObjectBlueprintXMLData();
			objectBlueprintXMLData.Name = reader.GetAttribute("Name");
			objectBlueprintXMLData.Inherits = reader.GetAttribute("Inherits");
			objectBlueprintXMLData.Load = reader.GetAttribute("Load");
			if (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement)
			{
				return objectBlueprintXMLData;
			}
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Comment || reader.NodeType == XmlNodeType.Text)
				{
					continue;
				}
				if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "object")
				{
					return objectBlueprintXMLData;
				}
				if (reader.NodeType == XmlNodeType.Element)
				{
					string name = reader.Name;
					if (!KnownNodes.Contains(name))
					{
						handleError($"{reader.BaseURI}: Unknown object element {reader.Name} at line {reader.LineNumber}");
					}
					ObjectBlueprintXMLChildNode node = ObjectBlueprintXMLChildNode.ReadChildNode(reader);
					if (!objectBlueprintXMLData.Children.ContainsKey(name))
					{
						objectBlueprintXMLData.Children[name] = new ObjectBlueprintXMLChildNodeCollection();
					}
					objectBlueprintXMLData.Children[name].Add(node, reader);
				}
				else
				{
					handleError($"{reader.BaseURI}: Unknown problem reading object: {reader.NodeType}");
				}
			}
			return objectBlueprintXMLData;
		}

		public override string ToString()
		{
			string text = "[ObjectBlueprintXMLData " + Name + " Inherits=" + Inherits + " Load=" + Load + "]\n";
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in Children)
			{
				text = text + "[" + child.Key + "]\n";
				text += child.Value;
			}
			return text;
		}
	}

	private Dictionary<string, ObjectBlueprintXMLData> Objects = new Dictionary<string, ObjectBlueprintXMLData>();

	private Dictionary<string, ObjectBlueprintXMLData> Finalized = new Dictionary<string, ObjectBlueprintXMLData>();

	protected static Action<object> handleError = MetricsManager.LogError;

	public static HashSet<string> KnownNodes = new HashSet<string>
	{
		"builder", "intproperty", "inventory", "inventoryobject", "mutation", "part", "property", "removepart", "skill", "stag",
		"stat", "tag", "xtag", "xtagGrammar", "xtagTextFragments", "xtagWaterRitual"
	};

	private ObjectBlueprintXMLData Bake(ObjectBlueprintXMLData obj)
	{
		if (Finalized.TryGetValue(obj.Name, out var value))
		{
			return value;
		}
		ObjectBlueprintXMLData objectBlueprintXMLData = new ObjectBlueprintXMLData();
		objectBlueprintXMLData.Name = obj.Name;
		objectBlueprintXMLData.Inherits = obj.Inherits;
		objectBlueprintXMLData.Mod = obj.Mod;
		if (!string.IsNullOrEmpty(obj.Inherits))
		{
			ObjectBlueprintXMLData objectBlueprintXMLData2;
			if (!Objects.TryGetValue(obj.Inherits, out var value2))
			{
				MetricsManager.LogPotentialModError(obj.Mod, "blueprint \"" + obj.Inherits + "\" inherited by " + obj.Name + " not found");
				objectBlueprintXMLData2 = new ObjectBlueprintXMLData();
			}
			else
			{
				objectBlueprintXMLData2 = Bake(value2);
			}
			foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child in objectBlueprintXMLData2.Children)
			{
				objectBlueprintXMLData.Children[child.Key] = child.Value.Clone();
			}
			if (objectBlueprintXMLData.Children.TryGetValue("tag", out var value3))
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, ObjectBlueprintXMLChildNode> item in value3.Named)
				{
					if (item.Value.Attributes.ContainsValue("*noinherit"))
					{
						list.Add(item.Key);
					}
				}
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					value3.Named.Remove(list[i]);
				}
			}
		}
		foreach (KeyValuePair<string, ObjectBlueprintXMLChildNodeCollection> child2 in obj.Children)
		{
			if (objectBlueprintXMLData.Children.TryGetValue(child2.Key, out var value4))
			{
				if (child2.Key.StartsWith("xtag"))
				{
					value4.Unnamed[0].Merge(child2.Value.Unnamed[0]);
				}
				else
				{
					value4.Merge(child2.Value.Clone());
				}
			}
			else
			{
				objectBlueprintXMLData.Children[child2.Key] = child2.Value.Clone();
			}
		}
		if (objectBlueprintXMLData.Children.ContainsKey("removepart"))
		{
			foreach (string key in objectBlueprintXMLData.Children["removepart"].Named.Keys)
			{
				if (objectBlueprintXMLData.Children.ContainsKey("part") && objectBlueprintXMLData.Children["part"].Named.ContainsKey(key))
				{
					objectBlueprintXMLData.Children["part"].Named.Remove(key);
				}
			}
			objectBlueprintXMLData.Children.Remove("removepart");
		}
		return objectBlueprintXMLData;
	}

	public IEnumerable<ObjectBlueprintXMLData> BakedBlueprints()
	{
		foreach (string key in Objects.Keys)
		{
			yield return Bake(Objects[key]);
		}
	}

	public int ReadObjectsNode(XmlTextReader reader, ModInfo modInfo = null)
	{
		int num = 0;
		while (reader.Read())
		{
			if (reader.Name == "object")
			{
				ObjectBlueprintXMLData objectBlueprintXMLData = ObjectBlueprintXMLData.ReadObjectNode(reader);
				objectBlueprintXMLData.Mod = modInfo;
				num++;
				if (objectBlueprintXMLData.Load == "Merge")
				{
					if (!Objects.ContainsKey(objectBlueprintXMLData.Name))
					{
						handleError(reader.BaseURI + ": Attempt to merge with " + objectBlueprintXMLData.Name + " which is an unknown blueprint, node discarded");
					}
					Objects[objectBlueprintXMLData.Name].Merge(objectBlueprintXMLData);
				}
				else
				{
					Objects[objectBlueprintXMLData.Name] = objectBlueprintXMLData;
				}
			}
			else if (reader.NodeType != XmlNodeType.Comment)
			{
				if (reader.Name == "objects" && reader.NodeType == XmlNodeType.EndElement)
				{
					return num;
				}
				throw new Exception("Unknown node '" + reader.Name + "'");
			}
		}
		return num;
	}

	public void ReadObjectBlueprintsXML(XmlTextReader reader, ModInfo modInfo = null)
	{
		if (handleError == null)
		{
			handleError = MetricsManager.LogError;
		}
		bool flag = false;
		try
		{
			reader.WhitespaceHandling = WhitespaceHandling.None;
			while (reader.Read())
			{
				if (reader.Name == "objects")
				{
					flag = true;
					ReadObjectsNode(reader, modInfo);
				}
			}
		}
		catch (Exception innerException)
		{
			throw new Exception($"File: {reader.BaseURI}, Line: {reader.LineNumber}:{reader.LinePosition}", innerException);
		}
		finally
		{
			reader.Close();
		}
		if (!flag)
		{
			handleError("No <objects> tag found in ObjectBlueprints.XML");
		}
	}

	public void LoadMainBlueprints()
	{
		try
		{
			handleError = MetricsManager.LogError;
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Creatures.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Data.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Foods.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Furniture.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Items.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/PhysicalPhenomena.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/RootObjects.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Staging.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Walls.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/Widgets.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/WorldTerrain.xml"));
			ReadObjectBlueprintsXML(DataManager.GetStreamingAssetsXMLStream("ObjectBlueprints/ZoneTerrain.xml"));
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Main ObjectBlueprints.xml: ", x);
		}
	}

	public void LoadModdedBlueprints()
	{
		ModManager.ForEachFile("ObjectBlueprints.xml", delegate(string path, ModInfo modInfo)
		{
			XmlTextReader reader = new XmlTextReader(path);
			handleError = modInfo.Error;
			ReadObjectBlueprintsXML(reader, modInfo);
		});
		handleError = MetricsManager.LogError;
	}

	public void LoadAllBlueprints()
	{
		LoadMainBlueprints();
		LoadModdedBlueprints();
	}
}
