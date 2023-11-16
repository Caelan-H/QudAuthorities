using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Capabilities;

public static class SavingThrows
{
	[NonSerialized]
	private static Dictionary<string, List<string>> VsLists = new Dictionary<string, List<string>>();

	[NonSerialized]
	private static Dictionary<string, List<string>> VsSets = new Dictionary<string, List<string>>();

	[NonSerialized]
	private static Dictionary<string, string> Descriptions = new Dictionary<string, string>();

	[NonSerialized]
	private static Dictionary<string, string> VariantDescriptions = new Dictionary<string, string>
	{
		{ "Axe", "Axe skills" },
		{ "Beam", "energy beams" },
		{ "Blade", "bladed weapons" },
		{ "Contact", "contact agents" },
		{ "Cudgel", "Cudgel skills" },
		{ "Disarm", "disarmament" },
		{ "Drag", "being dragged" },
		{ "EMP", "electromagnetic pulses" },
		{ "Fungal", "fungal infestation" },
		{ "Gas", "gases" },
		{ "Gaze", "gaze effects" },
		{ "Grab", "being grabbed" },
		{ "HookAndDrag", "Hook and Drag" },
		{ "Hologram", "holographic effects" },
		{ "Inhaled", "inhaled toxins" },
		{ "Injected", "injected toxins" },
		{ "LatchOn", "being latched onto" },
		{ "LongBlades", "Long Blades skills" },
		{ "Move", "forced movement" },
		{ "Phase", "phase effects" },
		{ "Pistol", "Pistol skills" },
		{ "Restraint", "being restrained" },
		{ "Rifle", "Bow and Rifle skills" },
		{ "RobotStop", "adversarial signage" },
		{ "ShieldSlam", "Shield Slam" },
		{ "ShortBlades", "Short Blades skills" },
		{ "Sleep", "forced sleep" },
		{ "Slip", "slipping" },
		{ "SlogGlands", "the bilge sphincter" },
		{ "Stoning", "petrification" },
		{ "Stuck", "becoming stuck" },
		{ "StunningForce", "Stunning Force" },
		{ "Swipe", "Swipe" },
		{ "Taunt", "taunting" },
		{ "Verbal", "speech effects" },
		{ "Web", "webbing" }
	};

	[NonSerialized]
	private static StringBuilder SingleDescStringBuilder = new StringBuilder();

	[NonSerialized]
	private static StringBuilder GetSaveBonusDescriptionStringBuilder = new StringBuilder();

	public static List<string> VsList(string Text)
	{
		if (VsLists.TryGetValue(Text, out var value))
		{
			return value;
		}
		value = new List<string>(Text.Split(','));
		VsLists.Add(Text, value);
		return value;
	}

	public static List<string> VsSet(string Text)
	{
		if (VsSets.TryGetValue(Text, out var value))
		{
			return value;
		}
		value = new List<string>(Text.Split(' '));
		VsSets.Add(Text, value);
		return value;
	}

	public static bool Applicable(string ApplyVs, string SaveVs)
	{
		if (string.IsNullOrEmpty(ApplyVs))
		{
			return true;
		}
		if (SaveVs == null)
		{
			return false;
		}
		if (ApplyVs.Contains(","))
		{
			return Applicable(VsList(ApplyVs), SaveVs);
		}
		if (ApplyVs.Contains(" "))
		{
			foreach (string item in VsSet(ApplyVs))
			{
				if (!SaveVs.Contains(item))
				{
					return false;
				}
			}
			return true;
		}
		return SaveVs.Contains(ApplyVs);
	}

	public static bool Applicable(string[] ApplyVs, string SaveVs)
	{
		if (ApplyVs == null || ApplyVs.Length == 0)
		{
			return true;
		}
		if (SaveVs == null)
		{
			return false;
		}
		foreach (string text in ApplyVs)
		{
			if (text.Contains(" "))
			{
				bool flag = false;
				foreach (string item in VsSet(text))
				{
					if (!SaveVs.Contains(item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			else if (SaveVs.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Applicable(List<string> ApplyVs, string SaveVs)
	{
		if (ApplyVs == null || ApplyVs.Count == 0)
		{
			return true;
		}
		if (SaveVs == null)
		{
			return false;
		}
		foreach (string ApplyV in ApplyVs)
		{
			if (ApplyV.Contains(" "))
			{
				bool flag = false;
				foreach (string item in VsSet(ApplyV))
				{
					if (!SaveVs.Contains(item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			else if (SaveVs.Contains(ApplyV))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Applicable(string ApplyVs, Event E)
	{
		return Applicable(ApplyVs, E.GetStringParameter("Vs"));
	}

	public static bool Applicable(string[] ApplyVs, Event E)
	{
		return Applicable(ApplyVs, E.GetStringParameter("Vs"));
	}

	public static bool Applicable(List<string> ApplyVs, Event E)
	{
		return Applicable(ApplyVs, E.GetStringParameter("Vs"));
	}

	public static bool Applicable(string ApplyVs, ISaveEvent E)
	{
		return Applicable(ApplyVs, E.Vs);
	}

	public static bool Applicable(string[] ApplyVs, ISaveEvent E)
	{
		return Applicable(ApplyVs, E.Vs);
	}

	public static bool Applicable(List<string> ApplyVs, ISaveEvent E)
	{
		return Applicable(ApplyVs, E.Vs);
	}

	private static string SingleDesc(string Item)
	{
		if (VariantDescriptions.TryGetValue(Item, out var value))
		{
			return value;
		}
		SingleDescStringBuilder.Clear();
		int i = 0;
		for (int length = Item.Length; i < length; i++)
		{
			char c = Item[i];
			if (char.IsUpper(c))
			{
				if (i > 0)
				{
					SingleDescStringBuilder.Append(' ');
				}
				SingleDescStringBuilder.Append(char.ToLower(c));
			}
			else
			{
				SingleDescStringBuilder.Append(c);
			}
		}
		return SingleDescStringBuilder.ToString();
	}

	public static string GetSaveBonusTypeDescription(string Text)
	{
		if (Descriptions.TryGetValue(Text, out var value))
		{
			return value;
		}
		List<string> list = new List<string>(VsList(Text));
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i] = SingleDesc(list[i]);
		}
		return Grammar.MakeAndList(list);
	}

	public static void AppendSaveBonusDescription(StringBuilder Store, int Bonus, string Text, bool HighlightNumber = false, bool Highlight = false)
	{
		if (Bonus != 0)
		{
			if (Store.Length > 0)
			{
				Store.Append('\n');
			}
			if (Highlight)
			{
				Store.Append("{{rules|");
			}
			if (Bonus > 0)
			{
				Store.Append('+');
			}
			if (HighlightNumber)
			{
				Store.Append("{{rules|");
			}
			Store.Append(Bonus);
			if (HighlightNumber)
			{
				Store.Append("}}");
			}
			Store.Append(" to saves");
			if (!string.IsNullOrEmpty(Text))
			{
				Store.Append(" vs. ").Append(GetSaveBonusTypeDescription(Text));
			}
			if (Highlight)
			{
				Store.Append("}}");
			}
		}
	}

	public static string GetSaveBonusDescription(int Bonus, string Text, bool HighlightNumber = false)
	{
		GetSaveBonusDescriptionStringBuilder.Clear();
		AppendSaveBonusDescription(GetSaveBonusDescriptionStringBuilder, Bonus, Text);
		if (GetSaveBonusDescriptionStringBuilder.Length <= 0)
		{
			return null;
		}
		return GetSaveBonusDescriptionStringBuilder.ToString();
	}
}
