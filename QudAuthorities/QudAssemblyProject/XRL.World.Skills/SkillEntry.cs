using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XRL.World.Skills;

[Serializable]
public class SkillEntry
{
	public string Name;

	public string Class;

	public string Description;

	public string Attribute;

	public int Cost;

	public string Snippet = "";

	[OptionalField]
	public bool? Initiatory;

	public Dictionary<string, PowerEntry> Powers = new Dictionary<string, PowerEntry>();

	public bool ShowRequirementsFailurePopup(GameObject GO)
	{
		foreach (PowerEntry value in Powers.Values)
		{
			if (value.Cost == 0 && value.ShowRequirementsFailurePopup(GO))
			{
				return true;
			}
		}
		return false;
	}

	public bool MeetsRequirements(GameObject GO)
	{
		foreach (PowerEntry value in Powers.Values)
		{
			if (value.Cost == 0 && !value.MeetsRequirements(GO))
			{
				return false;
			}
		}
		return true;
	}

	public void MergeWith(SkillEntry NewEntry)
	{
		if (NewEntry.Name != null)
		{
			Name = NewEntry.Name;
		}
		if (NewEntry.Class != null)
		{
			Class = NewEntry.Class;
		}
		if (NewEntry.Description != null)
		{
			Description = NewEntry.Description;
		}
		if (NewEntry.Attribute != null)
		{
			Attribute = NewEntry.Attribute;
		}
		if (NewEntry.Cost != -999)
		{
			Cost = NewEntry.Cost;
		}
		if (NewEntry.Snippet != null)
		{
			Snippet = NewEntry.Snippet;
		}
		if (NewEntry.Initiatory.HasValue)
		{
			Initiatory = NewEntry.Initiatory;
		}
		foreach (string key in NewEntry.Powers.Keys)
		{
			if (key[0] == '-')
			{
				if (Powers.ContainsKey(key.Substring(1)))
				{
					Powers.Remove(key.Substring(1));
				}
			}
			else if (Powers.ContainsKey(NewEntry.Powers[key].Name))
			{
				Powers[NewEntry.Powers[key].Name].MergeWith(NewEntry.Powers[key]);
			}
			else
			{
				Powers.Add(NewEntry.Powers[key].Name, NewEntry.Powers[key]);
				NewEntry.Powers[key].ParentSkill = this;
			}
		}
	}
}
