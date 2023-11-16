using System;
using ConsoleLib.Console;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL;

[Serializable]
public class MutationEntry
{
	public string MutationCode;

	public MutationCategory Category;

	public string Class;

	public string Exclusions;

	public string Constructor;

	public string Type;

	public int Cost;

	public bool Ranked;

	public string DisplayName;

	public string Help = "";

	public string Stat = "";

	public string Tile;

	public string Foreground = "w";

	public string Detail = "W";

	public string Property = "";

	public string ForceProperty;

	private string[] _Exclusions;

	public int Maximum = 1;

	public string BearerDescription = "";

	public int MaxLevel = 10;

	public bool Prerelease;

	public BaseMutation _Mutation;

	private bool? hasvariants;

	public BaseMutation Mutation
	{
		get
		{
			if (_Mutation == null)
			{
				_Mutation = CreateInstance();
			}
			return _Mutation;
		}
	}

	public string GetStat()
	{
		if (!string.IsNullOrEmpty(Stat))
		{
			return Stat;
		}
		return Category?.Stat;
	}

	public string GetProperty()
	{
		if (!string.IsNullOrEmpty(Property))
		{
			return Property;
		}
		return Category?.Property;
	}

	public string GetCategoryForceProperty()
	{
		return Category?.ForceProperty;
	}

	public string GetForceProperty()
	{
		if (string.IsNullOrEmpty(ForceProperty))
		{
			ForceProperty = "MutationBonus_" + Class;
		}
		return ForceProperty;
	}

	public string[] GetExclusions()
	{
		if (_Exclusions == null)
		{
			if (!string.IsNullOrEmpty(Exclusions))
			{
				_Exclusions = Exclusions.Split(',');
			}
			else
			{
				_Exclusions = new string[0];
			}
		}
		return _Exclusions;
	}

	public void MergeWith(MutationEntry newEntry)
	{
		if (!string.IsNullOrEmpty(newEntry.MutationCode))
		{
			MutationCode = newEntry.MutationCode;
		}
		if (!string.IsNullOrEmpty(newEntry.Class))
		{
			Class = newEntry.Class;
		}
		if (!string.IsNullOrEmpty(newEntry.Exclusions))
		{
			Exclusions = newEntry.Exclusions;
		}
		if (!string.IsNullOrEmpty(newEntry.Constructor))
		{
			Constructor = newEntry.Constructor;
		}
		if (!string.IsNullOrEmpty(newEntry.DisplayName))
		{
			DisplayName = newEntry.DisplayName;
		}
		if (!string.IsNullOrEmpty(newEntry.Help))
		{
			Help = newEntry.Help;
		}
		if (!string.IsNullOrEmpty(newEntry.Stat))
		{
			Stat = newEntry.Stat;
		}
		if (!string.IsNullOrEmpty(newEntry.Property))
		{
			Property = newEntry.Property;
		}
		if (!string.IsNullOrEmpty(newEntry.ForceProperty))
		{
			ForceProperty = newEntry.ForceProperty;
		}
		if (!string.IsNullOrEmpty(newEntry.BearerDescription))
		{
			BearerDescription = newEntry.BearerDescription;
		}
		if (newEntry.Prerelease)
		{
			Prerelease = newEntry.Prerelease;
		}
		if (newEntry.Cost != -999)
		{
			Cost = newEntry.Cost;
		}
		if (newEntry.Maximum != -999)
		{
			Maximum = newEntry.Maximum;
		}
		if (newEntry.MaxLevel != -999)
		{
			MaxLevel = newEntry.MaxLevel;
		}
		Ranked = newEntry.Ranked;
	}

	public bool HasVariants()
	{
		if (!hasvariants.HasValue)
		{
			BaseMutation baseMutation = CreateInstance();
			if (baseMutation == null)
			{
				return false;
			}
			hasvariants = baseMutation.GetVariants() != null;
		}
		return hasvariants.GetValueOrDefault();
	}

	public BaseMutation CreateInstance()
	{
		if (string.IsNullOrEmpty(Class))
		{
			return null;
		}
		string text = "XRL.World.Parts.Mutation." + Class;
		Type type = ModManager.ResolveType(text);
		if (type == null)
		{
			throw new TypeLoadException("Unknown Class " + text);
		}
		if (string.IsNullOrEmpty(Constructor))
		{
			return (BaseMutation)Activator.CreateInstance(type);
		}
		object[] args = Constructor.Split(',');
		return (BaseMutation)Activator.CreateInstance(type, args);
	}

	public IRenderable GetRenderable()
	{
		if (Tile != null)
		{
			return new Renderable
			{
				Tile = Tile,
				ColorString = "&" + Foreground,
				DetailColor = Detail[0]
			};
		}
		return null;
	}

	public bool OkWith(MutationEntry Entry, bool CheckOther = true, bool allowMultipleDefects = false)
	{
		if (Entry == this || Entry == null)
		{
			return true;
		}
		if (CheckOther && !Entry.OkWith(this, CheckOther: false, allowMultipleDefects))
		{
			return false;
		}
		if (!allowMultipleDefects && !Options.DisableDefectLimit && Entry.IsDefect() && IsDefect())
		{
			return false;
		}
		string[] exclusions = GetExclusions();
		foreach (string text in exclusions)
		{
			if (text == Entry.DisplayName)
			{
				return false;
			}
			if (text.Length > 0 && text[0] == '*' && text.Length == Entry.Category.Name.Length + 1 && text.EndsWith(Entry.Category.Name))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsDefect()
	{
		MutationCategory category = Category;
		if (category == null)
		{
			return false;
		}
		return category.Name?.Contains("Defect") == true;
	}

	public bool IsMental()
	{
		BaseMutation mutation = Mutation;
		if (mutation == null)
		{
			return false;
		}
		return mutation.GetMutationType()?.Contains("Mental") == true;
	}

	public bool IsPhysical()
	{
		BaseMutation mutation = Mutation;
		if (mutation == null)
		{
			return false;
		}
		return mutation.GetMutationType()?.Contains("Physical") == true;
	}
}
