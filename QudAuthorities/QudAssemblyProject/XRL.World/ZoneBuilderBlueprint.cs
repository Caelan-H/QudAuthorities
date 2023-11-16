using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class ZoneBuilderBlueprint
{
	public string Class;

	public Dictionary<string, object> Parameters;

	public ZoneBuilderBlueprint()
	{
	}

	public ZoneBuilderBlueprint(string _Class)
	{
		Class = _Class;
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value)
	{
		Class = _Class;
		AddParameter(Name, Value);
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value, string Name2, object Value2)
	{
		Class = _Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3)
	{
		Class = _Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4)
	{
		Class = _Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4, string Name5, object Value5)
	{
		Class = _Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
		AddParameter(Name5, Value5);
	}

	public ZoneBuilderBlueprint(string _Class, string Name, object Value, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4, string Name5, object Value5, string Name6, object Value6)
	{
		Class = _Class;
		AddParameter(Name, Value);
		AddParameter(Name2, Value2);
		AddParameter(Name3, Value3);
		AddParameter(Name4, Value4);
		AddParameter(Name5, Value5);
		AddParameter(Name6, Value6);
	}

	public void AddParameter(string nam, object val)
	{
		if (Parameters == null)
		{
			Parameters = new Dictionary<string, object>();
		}
		Parameters.Add(nam, val);
	}

	public override string ToString()
	{
		return "ZoneBuilder<" + Class + ">";
	}

	public static ZoneBuilderBlueprint Load(SerializationReader reader)
	{
		ZoneBuilderBlueprint zoneBuilderBlueprint = new ZoneBuilderBlueprint();
		zoneBuilderBlueprint.Class = reader.ReadString();
		int num = reader.ReadInt32();
		if (num > -1)
		{
			for (int i = 0; i < num; i++)
			{
				zoneBuilderBlueprint.AddParameter(reader.ReadString(), reader.ReadObject());
			}
		}
		return zoneBuilderBlueprint;
	}

	public void Save(SerializationWriter writer)
	{
		writer.Write(Class);
		if (Parameters == null)
		{
			writer.Write(-1);
			return;
		}
		writer.Write(Parameters.Keys.Count);
		foreach (string key in Parameters.Keys)
		{
			writer.Write(key);
			writer.WriteObject(Parameters[key]);
		}
	}
}
