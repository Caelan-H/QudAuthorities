using System.Collections.Generic;

namespace XRL.Help;

public class XRLManualPage
{
	public string Topic;

	public List<string> Lines = new List<string>();

	public List<string> LinesStripped = new List<string>();

	public XRLManualPage(string Data)
	{
		string[] array = Data.Split('\n');
		foreach (string text in array)
		{
			Lines.Add(text.Replace("\r", ""));
			LinesStripped.Add(text.Replace("\r", "").Replace("{", "").Replace("}", ""));
		}
	}
}
