using System.Collections.Generic;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.UI;

namespace Qud.UI;

public static class RTF
{
	private static Dictionary<Thread, StringBuilder> builders = new Dictionary<Thread, StringBuilder>();

	public static string FormatToRTF(string s, string opacity = "FF", int blockWrap = -1, bool stripFormatting = false)
	{
		if (!builders.TryGetValue(Thread.CurrentThread, out var value))
		{
			value = new StringBuilder();
			builders.Add(Thread.CurrentThread, value);
		}
		if (stripFormatting)
		{
			s = ColorUtility.StripFormatting(s);
		}
		value.Clear();
		if (blockWrap > 0)
		{
			s = BlockWrap(s, blockWrap, 5000);
		}
		Sidebar.FormatToRTF(s, value, opacity);
		return value.ToString();
	}

	public static string BlockWrap(string s, int width, int maxLines)
	{
		return new TextBlock(s, width, maxLines).GetStringBuilder().ToString();
	}
}
