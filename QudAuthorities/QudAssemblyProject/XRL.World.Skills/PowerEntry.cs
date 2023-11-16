using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
public class PowerEntry
{
	public string Name;

	public int Cost;

	public string Attribute;

	public string Minimum;

	public string Class;

	public string Description;

	public string Requires = "";

	public string Prereq = "";

	public string Exclusion = "";

	public string Snippet = "";

	public SkillEntry ParentSkill;

	[NonSerialized]
	private List<PowerEntryRequirement> _requirements;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	private List<PowerEntryRequirement> requirements
	{
		get
		{
			InitRequirements();
			return _requirements;
		}
	}

	public void InitRequirements()
	{
		if (_requirements != null)
		{
			return;
		}
		_requirements = new List<PowerEntryRequirement>();
		if (string.IsNullOrEmpty(Attribute) || string.IsNullOrEmpty(Minimum))
		{
			return;
		}
		string[] array = Attribute.Split('|');
		string[] array2 = Minimum.Split('|');
		for (int i = 0; i < array.Length; i++)
		{
			PowerEntryRequirement powerEntryRequirement = new PowerEntryRequirement();
			string[] array3 = array[i].Split(',');
			string[] array4 = array2[i].Split(',');
			for (int j = 0; j < array3.Length; j++)
			{
				powerEntryRequirement.Attributes.Add(array3[j]);
				powerEntryRequirement.Minimums.Add(Convert.ToInt32(array4[j]));
			}
			_requirements.Add(powerEntryRequirement);
		}
	}

	public bool ShowRequirementsFailurePopup(GameObject GO)
	{
		if (!string.IsNullOrEmpty(Attribute) && !string.IsNullOrEmpty(Minimum))
		{
			InitRequirements();
			foreach (PowerEntryRequirement requirement in requirements)
			{
				if (requirement.ShowFailurePopup(GO, this))
				{
					return true;
				}
			}
		}
		if (!string.IsNullOrEmpty(Exclusion))
		{
			foreach (string item in Exclusion.CachedCommaExpansion())
			{
				if (SkillFactory.Factory.PowersByClass.TryGetValue(item, out var value))
				{
					if (GO.HasSkill(item))
					{
						Popup.Show("You may not learn this skill if you already have " + value.Name + ".");
						return true;
					}
				}
				else if (MutationFactory.HasMutation(item))
				{
					MutationEntry mutationEntryByName = MutationFactory.GetMutationEntryByName(item);
					if (GO.HasPart(item))
					{
						Popup.Show("You may not learn this skill if you have " + mutationEntryByName.DisplayName + ".");
						return true;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(Prereq))
		{
			foreach (string item2 in Prereq.CachedCommaExpansion())
			{
				if (SkillFactory.Factory.PowersByClass.TryGetValue(item2, out var value2))
				{
					if (!GO.HasSkill(item2))
					{
						Popup.Show("You may not learn this skill until you have " + value2.Name + ".");
						return true;
					}
				}
				else if (MutationFactory.HasMutation(item2))
				{
					MutationEntry mutationEntryByName2 = MutationFactory.GetMutationEntryByName(item2);
					if (!GO.HasPart(item2))
					{
						Popup.Show("You may not learn this skill until you have " + mutationEntryByName2.DisplayName + ".");
						return true;
					}
				}
			}
		}
		SkillEntry parentSkill = ParentSkill;
		if (parentSkill != null && parentSkill.Initiatory == true && !GO.HasSkill(ParentSkill.Class))
		{
			Popup.Show("You must be initiated into this skill in order to learn it.");
			return true;
		}
		return false;
	}

	public bool MeetsRequirements(GameObject GO)
	{
		if (!string.IsNullOrEmpty(Attribute) && !string.IsNullOrEmpty(Minimum))
		{
			InitRequirements();
			bool flag = false;
			foreach (PowerEntryRequirement requirement in requirements)
			{
				if (requirement.MeetsRequirement(GO))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (!string.IsNullOrEmpty(Exclusion))
		{
			foreach (string item in Exclusion.CachedCommaExpansion())
			{
				if (SkillFactory.Factory.PowersByClass.TryGetValue(item, out var _))
				{
					if (GO.HasSkill(item))
					{
						return false;
					}
				}
				else if (MutationFactory.HasMutation(item) && GO.HasPart(item))
				{
					return false;
				}
			}
		}
		if (!string.IsNullOrEmpty(Prereq))
		{
			foreach (string item2 in Prereq.CachedCommaExpansion())
			{
				if (SkillFactory.Factory.PowersByClass.ContainsKey(item2))
				{
					if (!GO.HasSkill(item2))
					{
						return false;
					}
					continue;
				}
				if (MutationFactory.HasMutation(item2))
				{
					if (!GO.HasPart(item2))
					{
						return false;
					}
					continue;
				}
				return false;
			}
		}
		SkillEntry parentSkill = ParentSkill;
		if (parentSkill != null && parentSkill.Initiatory == true && !GO.HasSkill(ParentSkill.Class))
		{
			return false;
		}
		return true;
	}

	public void ShowAttributeFailurePopup(GameObject GO)
	{
		if (string.IsNullOrEmpty(Attribute) || string.IsNullOrEmpty(Minimum))
		{
			return;
		}
		InitRequirements();
		using List<PowerEntryRequirement>.Enumerator enumerator = requirements.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.ShowFailurePopup(GO, this))
		{
		}
	}

	public bool MeetsAttributeMinimum(GameObject GO)
	{
		if (string.IsNullOrEmpty(Attribute))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(Minimum))
		{
			return true;
		}
		InitRequirements();
		foreach (PowerEntryRequirement requirement in requirements)
		{
			if (requirement.MeetsRequirement(GO))
			{
				return true;
			}
		}
		return false;
	}

	public string Render(GameObject GO)
	{
		if (SB == null)
		{
			SB = new StringBuilder();
		}
		else
		{
			SB.Clear();
		}
		if (MeetsRequirements(GO))
		{
			if (Cost <= GO.Stat("SP"))
			{
				SB.Append(" - [{{C|" + Cost + "}}sp] {{g|" + Name + "}}");
			}
			else
			{
				SB.Append(" - [{{R|" + Cost + "}}sp] {{g|" + Name + "}}");
			}
		}
		else
		{
			SB.Append(" - [{{K|" + Cost + "}}sp] {{K|" + Name + "}}");
		}
		InitRequirements();
		int num = 0;
		foreach (PowerEntryRequirement requirement in requirements)
		{
			if (num == 0)
			{
				SB.Append(' ');
			}
			else
			{
				SB.Append(" or ");
			}
			num++;
			requirement.Render(GO, SB);
		}
		return SB.ToString();
	}

	public void MergeWith(PowerEntry NewEntry)
	{
		if (NewEntry.Name != null)
		{
			Name = NewEntry.Name;
		}
		if (NewEntry.Cost != -999)
		{
			Cost = NewEntry.Cost;
		}
		if (NewEntry.Minimum != null)
		{
			Minimum = NewEntry.Minimum;
		}
		if (NewEntry.Attribute != null)
		{
			Attribute = NewEntry.Attribute;
		}
		if (NewEntry.Class != null)
		{
			Class = NewEntry.Class;
		}
		if (NewEntry.Description != null)
		{
			Description = NewEntry.Description;
		}
		if (NewEntry.Requires != null)
		{
			Requires = NewEntry.Requires;
		}
		if (NewEntry.Prereq != null)
		{
			Prereq = NewEntry.Prereq;
		}
		if (NewEntry.Exclusion != null)
		{
			Exclusion = NewEntry.Exclusion;
		}
		if (NewEntry.Snippet != null)
		{
			Snippet = NewEntry.Snippet;
		}
	}
}
