using System;
using System.Collections.Generic;

namespace XRL.World;

[Serializable]
public class Damage
{
	public int Amount;

	public List<string> Attributes = new List<string>();

	public bool SuppressionMessageDone;

	public Damage(int Amount)
	{
		this.Amount = Amount;
	}

	public bool HasAnyAttribute(List<string> names)
	{
		if (names == null)
		{
			return false;
		}
		if (Attributes == null)
		{
			return false;
		}
		foreach (string attribute in Attributes)
		{
			if (names.Contains(attribute))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAttribute(string Name)
	{
		if (Attributes == null)
		{
			return false;
		}
		if (Attributes.Contains(Name))
		{
			return true;
		}
		return false;
	}

	public void AddAttribute(string Name)
	{
		Attributes.Add(Name);
	}

	public void AddAttributes(string List)
	{
		if (string.IsNullOrEmpty(List))
		{
			return;
		}
		if (List.Contains(" "))
		{
			string[] array = List.Split(' ');
			foreach (string name in array)
			{
				AddAttribute(name);
			}
		}
		else
		{
			AddAttribute(List);
		}
	}

	public bool IsColdDamage()
	{
		if (!HasAttribute("Cold") && !HasAttribute("Ice"))
		{
			return HasAttribute("Freeze");
		}
		return true;
	}

	public bool IsHeatDamage()
	{
		if (!HasAttribute("Fire"))
		{
			return HasAttribute("Heat");
		}
		return true;
	}

	public bool IsElectricDamage()
	{
		if (!HasAttribute("Electric") && !HasAttribute("Shock") && !HasAttribute("Lightning"))
		{
			return HasAttribute("Electricity");
		}
		return true;
	}

	public bool IsAcidDamage()
	{
		return HasAttribute("Acid");
	}

	public bool IsDisintegrationDamage()
	{
		if (!HasAttribute("Disintegrate"))
		{
			return HasAttribute("Disintegration");
		}
		return true;
	}

	public static bool IsColdDamage(string Type)
	{
		if (!(Type == "Cold") && !(Type == "Ice"))
		{
			return Type == "Freeze";
		}
		return true;
	}

	public static bool IsHeatDamage(string Type)
	{
		if (!(Type == "Fire"))
		{
			return Type == "Heat";
		}
		return true;
	}

	public static bool IsElectricDamage(string Type)
	{
		switch (Type)
		{
		default:
			return Type == "Electrical";
		case "Electric":
		case "Shock":
		case "Lightning":
		case "Electricity":
			return true;
		}
	}

	public static bool IsAcidDamage(string Type)
	{
		return Type == "Acid";
	}

	public static bool IsDisintegrationDamage(string Type)
	{
		if (!(Type == "Disintegrate"))
		{
			return Type == "Disintegration";
		}
		return true;
	}

	public static bool ContainsColdDamage(string Type)
	{
		if (!Type.Contains("Cold") && !Type.Contains("Ice"))
		{
			return Type.Contains("Freeze");
		}
		return true;
	}

	public static bool ContainsHeatDamage(string Type)
	{
		if (!Type.Contains("Fire"))
		{
			return Type.Contains("Heat");
		}
		return true;
	}

	public static bool ContainsElectricDamage(string Type)
	{
		if (!Type.Contains("Electric") && !Type.Contains("Shock") && !Type.Contains("Lightning"))
		{
			return Type.Contains("Electricity");
		}
		return true;
	}

	public static bool ContainsAcidDamage(string Type)
	{
		return Type.Contains("Acid");
	}

	public static bool ContainsDisintegrationDamage(string Type)
	{
		if (!Type.Contains("Disintegrate"))
		{
			return Type.Contains("Disintegration");
		}
		return true;
	}

	public static string GetDamageColor(string Attributes)
	{
		return "r";
	}
}
