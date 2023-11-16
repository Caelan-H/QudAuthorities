using XRL.World;

namespace XRL.UI;

public class TradeEntry
{
	public GameObject GO;

	public string CategoryName = "";

	public TradeEntry(string CategoryName)
	{
		this.CategoryName = CategoryName;
	}

	public TradeEntry(GameObject GO)
	{
		this.GO = GO;
	}
}
