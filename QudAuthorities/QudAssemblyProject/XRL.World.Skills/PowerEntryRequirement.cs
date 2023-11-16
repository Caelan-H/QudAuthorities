using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Skills;

[Serializable]
public class PowerEntryRequirement
{
	public List<string> Attributes = new List<string>();

	public List<int> Minimums = new List<int>();

	public bool MeetsRequirement(GameObject GO)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			if (GO.BaseStat(Attributes[i]) < Minimums[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool ShowFailurePopup(GameObject GO, PowerEntry power)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			if (GO.BaseStat(Attributes[i]) < Minimums[i])
			{
				Popup.Show("Your " + Attributes[i] + " isn't high enough to buy " + power.Name + "!");
				return true;
			}
		}
		return false;
	}

	public void Render(GameObject GO, StringBuilder sb)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			string value = "R";
			if (GO.BaseStat(Attributes[i]) >= Minimums[i])
			{
				value = "G";
			}
			if (i > 0)
			{
				sb.Append(", ");
			}
			sb.Append("{{C|").Append(Minimums[i]).Append("}} {{")
				.Append(value)
				.Append('|')
				.Append(Attributes[i])
				.Append("}}");
		}
	}
}
