using System;
using System.Collections.Generic;

namespace XRL.World.Encounters;

public class EncounterTableXmlParser
{
	private EncounterFactory factory;

	private Dictionary<string, Action<XmlDataHelper>> Nodes;

	private Dictionary<string, Action<XmlDataHelper>> TableSubNodes;

	private Dictionary<string, Action<XmlDataHelper>> ObjectsSubNodes;

	private EncounterTable CurrentTable;

	public EncounterTableXmlParser(EncounterFactory factory)
	{
		this.factory = factory;
		Nodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "encountertables", HandleNodes },
			{ "encountertable", HandleEncounterTableNode }
		};
		TableSubNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "objects", HandleObjectsNode },
			{
				"mergetables",
				delegate(XmlDataHelper xml)
				{
					xml.HandleNodes(TableSubNodes);
				}
			},
			{ "mergetable", HandleMergeTableNode }
		};
		ObjectsSubNodes = new Dictionary<string, Action<XmlDataHelper>>
		{
			{ "tableobject", HandleTableObjectNode },
			{ "object", HandleObjectNode },
			{ "builderobject", HandleBuilderObjectNode },
			{ "population", HandlePopulationNode }
		};
	}

	public void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(Nodes);
	}

	public void HandleEncounterTableNode(XmlDataHelper xml)
	{
		EncounterTable encounterTable = LoadEncounterTableNode(xml);
		if (factory.EncounterTables.TryGetValue(encounterTable.Name, out var value))
		{
			if (encounterTable.Load == "Merge")
			{
				value.MergeWith(encounterTable);
			}
			else
			{
				factory.EncounterTables[encounterTable.Name] = encounterTable;
			}
		}
		else
		{
			factory.EncounterTables.Add(encounterTable.Name, encounterTable);
		}
	}

	private EncounterTable LoadEncounterTableNode(XmlDataHelper Reader)
	{
		EncounterTable encounterTable = new EncounterTable();
		encounterTable.Name = Reader.GetAttribute("Name");
		encounterTable.Density = Reader.GetAttribute("Density");
		encounterTable.Load = Reader.GetAttribute("Load");
		EncounterTable currentTable = CurrentTable;
		CurrentTable = encounterTable;
		Reader.HandleNodes(TableSubNodes);
		CurrentTable = currentTable;
		return encounterTable;
	}

	private void HandleObjectsNode(XmlDataHelper Reader)
	{
		Reader.HandleNodes(ObjectsSubNodes);
	}

	private void HandleTableObjectNode(XmlDataHelper Reader)
	{
		EncounterTableObject encounterTableObject = LoadTableObjectNode(Reader);
		CurrentTable.Objects.Add(encounterTableObject);
		CurrentTable.MaxChance += Convert.ToInt32(encounterTableObject.Chance);
	}

	private void HandleObjectNode(XmlDataHelper Reader)
	{
		EncounterObject encounterObject = LoadObjectNode(Reader);
		CurrentTable.Objects.Add(encounterObject);
		CurrentTable.MaxChance += Convert.ToInt32(encounterObject.Chance);
		Reader.DoneWithElement();
	}

	private void HandleBuilderObjectNode(XmlDataHelper Reader)
	{
		EncounterBuilderObject item = LoadBuilderObjectNode(Reader);
		CurrentTable.Objects.Add(item);
		Reader.DoneWithElement();
	}

	private void HandlePopulationNode(XmlDataHelper Reader)
	{
		EncounterPopulation item = LoadPopulationNode(Reader);
		CurrentTable.Objects.Add(item);
		Reader.DoneWithElement();
	}

	private EncounterTableObject LoadTableObjectNode(XmlDataHelper Reader)
	{
		EncounterTableObject result = new EncounterTableObject
		{
			Chance = Reader.GetAttribute("Chance"),
			Table = Reader.GetAttribute("Table")
		};
		Reader.DoneWithElement();
		return result;
	}

	private EncounterBuilderObject LoadBuilderObjectNode(XmlDataHelper Reader)
	{
		EncounterBuilderObject result = new EncounterBuilderObject
		{
			Chance = Reader.GetAttribute("Chance"),
			Builder = Reader.GetAttribute("Builder")
		};
		Reader.DoneWithElement();
		return result;
	}

	private EncounterPopulation LoadPopulationNode(XmlDataHelper Reader)
	{
		EncounterPopulation result = new EncounterPopulation
		{
			Chance = Reader.GetAttribute("Chance"),
			Table = Reader.GetAttribute("Table"),
			Number = Reader.GetAttribute("Number")
		};
		Reader.DoneWithElement();
		return result;
	}

	private EncounterObject LoadObjectNode(XmlDataHelper Reader)
	{
		EncounterObject result = new EncounterObject
		{
			Chance = Reader.GetAttribute("Chance"),
			Number = Reader.GetAttribute("Number"),
			Blueprint = Reader.GetAttribute("Blueprint"),
			Builder = Reader.GetAttribute("Builder"),
			Aquatic = (Reader.GetAttribute("Aquatic") == null || Reader.GetAttribute("Aquatic") == "true"),
			LivesOnWalls = (Reader.GetAttribute("LivesOnWalls") == null || Reader.GetAttribute("LivesOnWalls") == "true")
		};
		Reader.DoneWithElement();
		return result;
	}

	private void HandleMergeTableNode(XmlDataHelper Reader)
	{
		EncounterMergeTable newTable = LoadMergeTableNode(Reader);
		CurrentTable.AddMergeTable(newTable);
	}

	private EncounterMergeTable LoadMergeTableNode(XmlDataHelper Reader)
	{
		EncounterMergeTable encounterMergeTable = new EncounterMergeTable();
		encounterMergeTable.Table = Reader.GetAttribute("Table");
		string attribute = Reader.GetAttribute("Weight");
		encounterMergeTable.Weight = 1;
		switch (attribute)
		{
		case "minimum":
			encounterMergeTable.Weight = Weights.Minimum;
			break;
		case "low":
			encounterMergeTable.Weight = Weights.Low;
			break;
		case "very low":
			encounterMergeTable.Weight = Weights.VeryLow;
			break;
		case "medium":
			encounterMergeTable.Weight = Weights.Medium;
			break;
		case "high":
			encounterMergeTable.Weight = Weights.High;
			break;
		case "very high":
			encounterMergeTable.Weight = Weights.VeryHigh;
			break;
		case "maximum":
			encounterMergeTable.Weight = Weights.Maximuum;
			break;
		default:
			Reader.ParseWarning("Unhandled weight value");
			break;
		}
		Reader.DoneWithElement();
		return encounterMergeTable;
	}
}
