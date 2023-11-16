using System.Collections.Generic;
using TMPro;

public class MessageLogElement : PooledScrollRectElement<string>
{
	public TextMeshProUGUI text;

	public override void Setup(int placement, List<string> allData)
	{
		text.text = allData[placement];
	}
}
