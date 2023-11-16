using System.Collections.Generic;
using XRL.Core;

namespace XRL.UI;

public class FactionRepComparer : IComparer<string>
{
	public int Compare(string f1, string f2)
	{
		int num = XRLCore.Core.Game.PlayerReputation.get(f2).CompareTo(XRLCore.Core.Game.PlayerReputation.get(f1));
		if (num == 0)
		{
			return f1.CompareTo(f2);
		}
		return num;
	}
}
