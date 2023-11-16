namespace XRL.UI.Framework;

public class HorizontalScrollerScroller : HorizontalScroller
{
	public override ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return new ScrollChildContext
		{
			proxyTo = GetPrefabForIndex(index).GetComponent<IFrameworkControl>().GetNavigationContext()
		};
	}

	public void UpdateHighlightText()
	{
		int num = scrollContext.selectedPosition;
		FrameworkScroller component = GetPrefabForIndex(num).GetComponent<FrameworkScroller>();
		if (descriptionText != null)
		{
			string description = scrollContext.data[num].Description;
			if (component != null)
			{
				int index = component.scrollContext.selectedPosition;
				description = component.scrollContext.data[index].Description;
			}
			descriptionText?.SetText(description);
		}
	}

	public override void UpdateSelection()
	{
		base.UpdateSelection();
		UpdateHighlightText();
	}
}
