using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
[HasModSensitiveStaticCache]
public class SkillFactory : IPart
{
	public Dictionary<string, SkillEntry> SkillList = new Dictionary<string, SkillEntry>();

	public Dictionary<string, SkillEntry> SkillByClass = new Dictionary<string, SkillEntry>();

	public Dictionary<string, PowerEntry> PowersByClass = new Dictionary<string, PowerEntry>();

	[NonSerialized]
	[ModSensitiveStaticCache(false)]
	private static List<PowerEntry> Powers;

	[ModSensitiveStaticCache(false)]
	private static SkillFactory _Factory;

	public static SkillFactory Factory
	{
		get
		{
			if (_Factory == null)
			{
				_Factory = new SkillFactory();
				Loading.LoadTask("Loading Skills.xml", _Factory.LoadSkills);
			}
			return _Factory;
		}
	}

	public static List<PowerEntry> getUnknownPowersFor(GameObject obj)
	{
		List<PowerEntry> list = new List<PowerEntry>();
		list.AddRange(from kv in Factory.PowersByClass
			where !obj.HasPart(kv.Key)
			select kv.Value);
		return list;
	}

	public static List<PowerEntry> getLearnablePowersFor(GameObject obj)
	{
		List<PowerEntry> list = new List<PowerEntry>();
		list.AddRange(from kv in Factory.PowersByClass.Where(delegate(KeyValuePair<string, PowerEntry> kv)
			{
				if (obj.HasPart(kv.Key))
				{
					return false;
				}
				SkillEntry parentSkill = kv.Value.ParentSkill;
				if (parentSkill != null && parentSkill.Initiatory == true)
				{
					if (!kv.Value.MeetsRequirements(obj))
					{
						return false;
					}
					foreach (PowerEntry value in kv.Value.ParentSkill.Powers.Values)
					{
						if (value == kv.Value)
						{
							break;
						}
						if (!obj.HasPart(value.Class))
						{
							return false;
						}
					}
				}
				return true;
			})
			select kv.Value);
		return list;
	}

	private void LoadSkills()
	{
		SkillList = new Dictionary<string, SkillEntry>();
		SkillByClass = new Dictionary<string, SkillEntry>();
		PowersByClass = new Dictionary<string, PowerEntry>();
		using (XmlTextReader xmlTextReader = DataManager.GetStreamingAssetsXMLStream("Skills.xml"))
		{
			xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader.Read())
			{
				if (xmlTextReader.Name == "skills")
				{
					LoadSkillsNode(xmlTextReader);
				}
				if (xmlTextReader.NodeType == XmlNodeType.EndElement && xmlTextReader.Name == "skills")
				{
					break;
				}
			}
			xmlTextReader.Close();
		}
		ModManager.ForEachFile("Skills.xml", delegate(string s)
		{
			using XmlTextReader xmlTextReader2 = DataManager.GetStreamingAssetsXMLStream(s);
			xmlTextReader2.WhitespaceHandling = WhitespaceHandling.None;
			while (xmlTextReader2.Read())
			{
				if (xmlTextReader2.Name == "skills")
				{
					LoadSkillsNode(xmlTextReader2, bMod: true);
				}
				if (xmlTextReader2.NodeType == XmlNodeType.EndElement && xmlTextReader2.Name == "skills")
				{
					break;
				}
			}
			xmlTextReader2.Close();
		});
		foreach (string key in SkillList.Keys)
		{
			SkillByClass.Add(SkillList[key].Class, SkillList[key]);
			foreach (string key2 in SkillList[key].Powers.Keys)
			{
				if (SkillList[key].Powers[key2].Class != null)
				{
					PowersByClass.Add(SkillList[key].Powers[key2].Class, SkillList[key].Powers[key2]);
				}
			}
		}
	}

	public void LoadSkillsNode(XmlTextReader Reader, bool bMod = false)
	{
		while (Reader.Read())
		{
			if (Reader.Name == "skill")
			{
				SkillEntry skillEntry = LoadSkillNode(Reader, bMod);
				if (skillEntry.Name[0] == '-')
				{
					if (SkillList.ContainsKey(skillEntry.Name.Substring(1)))
					{
						SkillList.Remove(skillEntry.Name.Substring(1));
					}
				}
				else if (SkillList.ContainsKey(skillEntry.Name))
				{
					SkillList[skillEntry.Name].MergeWith(skillEntry);
				}
				else
				{
					SkillList.Add(skillEntry.Name, skillEntry);
				}
			}
			if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "skills")
			{
				break;
			}
		}
	}

	public SkillEntry LoadSkillNode(XmlTextReader Reader, bool bMod)
	{
		SkillEntry skillEntry = new SkillEntry();
		skillEntry.Name = Reader.GetAttribute("Name");
		skillEntry.Class = Reader.GetAttribute("Class");
		skillEntry.Description = Reader.GetAttribute("Description");
		skillEntry.Snippet = Reader.GetAttribute("Snippet");
		string attribute = Reader.GetAttribute("Cost");
		if (!string.IsNullOrEmpty(attribute))
		{
			skillEntry.Cost = Convert.ToInt32(attribute);
		}
		else
		{
			skillEntry.Cost = -999;
		}
		skillEntry.Attribute = Reader.GetAttribute("Attribute");
		string attribute2 = Reader.GetAttribute("Initiatory");
		if (!string.IsNullOrEmpty(attribute2))
		{
			skillEntry.Initiatory = Convert.ToBoolean(attribute2);
		}
		while (Reader.Read())
		{
			if (Reader.NodeType == XmlNodeType.Element)
			{
				if (Reader.Name == "power")
				{
					PowerEntry powerEntry = LoadPowerNode(Reader, bMod, skillEntry);
					if (string.IsNullOrEmpty(powerEntry.Requires))
					{
						powerEntry.Requires = skillEntry.Class;
					}
					skillEntry.Powers.Add(powerEntry.Name, powerEntry);
				}
			}
			else if (Reader.NodeType == XmlNodeType.EndElement && Reader.Name == "skill")
			{
				break;
			}
		}
		return skillEntry;
	}

	public PowerEntry LoadPowerNode(XmlTextReader Reader, bool bMod, SkillEntry NewSkillEntry)
	{
		PowerEntry powerEntry = new PowerEntry();
		powerEntry.Name = Reader.GetAttribute("Name");
		powerEntry.Class = Reader.GetAttribute("Class");
		powerEntry.Description = Reader.GetAttribute("Description");
		powerEntry.ParentSkill = NewSkillEntry;
		string attribute = Reader.GetAttribute("Cost");
		if (!string.IsNullOrEmpty(attribute))
		{
			powerEntry.Cost = Convert.ToInt32(attribute);
		}
		else
		{
			powerEntry.Cost = -999;
		}
		powerEntry.Attribute = Reader.GetAttribute("Attribute");
		powerEntry.Prereq = Reader.GetAttribute("Prereq");
		powerEntry.Exclusion = Reader.GetAttribute("Exclusion");
		powerEntry.Minimum = Reader.GetAttribute("Minimum");
		powerEntry.Requires = Reader.GetAttribute("Requires");
		powerEntry.Snippet = Reader.GetAttribute("Snippet");
		return powerEntry;
	}

	public static List<PowerEntry> GetPowers()
	{
		if (Powers == null)
		{
			Powers = new List<PowerEntry>();
			foreach (SkillEntry value in Factory.SkillList.Values)
			{
				foreach (PowerEntry value2 in value.Powers.Values)
				{
					if (!string.IsNullOrEmpty(value2.Class))
					{
						Powers.Add(value2);
					}
				}
			}
		}
		return Powers;
	}

	public static string GetRandomPowerClass()
	{
		return GetPowers().GetRandomElement()?.Class;
	}

	public static string GetSkillOrPowerName(string ClassName)
	{
		if (Factory.SkillByClass.TryGetValue(ClassName, out var value))
		{
			return value.Name;
		}
		if (Factory.PowersByClass.TryGetValue(ClassName, out var value2))
		{
			return value2.Name;
		}
		return ClassName;
	}
}
