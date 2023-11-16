using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace XRL;

[Serializable]
public class MarkovChainData
{
	public int order;

	public List<string> OpeningWords = new List<string>();

	public List<string> keys = new List<string>();

	public List<string> values = new List<string>();

	[NonSerialized]
	public Dictionary<string, List<string>> Chain = new Dictionary<string, List<string>>();

	public void OnBeforeSerialize()
	{
		keys.Clear();
		values.Clear();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, List<string>> item in Chain)
		{
			keys.Add(item.Key);
			stringBuilder.Length = 0;
			for (int i = 0; i < item.Value.Count; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append('\u0001');
				}
				stringBuilder.Append(item.Value[i]);
			}
			values.Add(stringBuilder.ToString());
		}
	}

	public void OnAfterDeserialize()
	{
		Chain.Clear();
		if (keys.Count != values.Count)
		{
			throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
		}
		for (int i = 0; i < keys.Count; i++)
		{
			Chain.Add(keys[i], new List<string>(values[i].Split('\u0001')));
		}
		keys.Clear();
		values.Clear();
	}

	public void SaveToFile(string FileName)
	{
		OnBeforeSerialize();
		File.WriteAllText(FileName, JsonUtility.ToJson(this));
	}

	public static MarkovChainData LoadFromFile(string FileName)
	{
		MarkovChainData markovChainData = JsonUtility.FromJson<MarkovChainData>(File.ReadAllText(FileName));
		markovChainData.OnAfterDeserialize();
		return markovChainData;
	}
}
