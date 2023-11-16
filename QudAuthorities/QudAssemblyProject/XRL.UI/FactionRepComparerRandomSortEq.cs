using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.UI;

public class FactionRepComparerRandomSortEqual : IComparer<string>
{
	public int Compare(string f1, string f2)
	{
		if (f1 == f2)
		{
			return 1.CompareTo(1);
		}
		int num = XRLCore.Core.Game.PlayerReputation.get(f2).CompareTo(XRLCore.Core.Game.PlayerReputation.get(f1));
		if (num == 0)
		{
			return Stat.Random(-1, 1);
		}
		return num;
	}
}
