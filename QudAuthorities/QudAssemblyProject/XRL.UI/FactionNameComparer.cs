using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class FactionNameComparer : IComparer<string>
{
	public int Compare(string f1, string f2)
	{
		int num = Factions.get(f1).DisplayName.CompareTo(Factions.get(f2).DisplayName);
		if (num == 0)
		{
			return f1.CompareTo(f2);
		}
		return num;
	}
}
