using System.Collections.Generic;

namespace XRL.UI;

public class GameOption
{
	public string ID;

	public string DisplayText;

	public string Category;

	public string Type;

	public string Default;

	public List<string> Values;

	public int Min;

	public int Max;

	public int Increment;
}
