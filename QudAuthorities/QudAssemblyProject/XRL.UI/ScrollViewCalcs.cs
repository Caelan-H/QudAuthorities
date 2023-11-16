using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI;

public class ScrollViewCalcs
{
	public ScrollRect scroller;

	public float contentHeight;

	public float scrollHeight;

	public float contentTop;

	public float contentBottom;

	public float scrollTop;

	public float scrollBottom;

	public float scrollPercent;

	public bool isAnyAboveView => scrollTop > contentTop;

	public bool isAnyBelowView => scrollBottom < contentBottom;

	public bool isAnyInView
	{
		get
		{
			if (!(contentBottom > scrollTop))
			{
				return contentTop > scrollBottom;
			}
			return true;
		}
	}

	public bool isInFullView
	{
		get
		{
			if (!isAnyAboveView)
			{
				return !isAnyBelowView;
			}
			return false;
		}
	}

	public static ScrollViewCalcs GetScrollViewCalcs(RectTransform rt, ScrollViewCalcs reuse = null)
	{
		ScrollViewCalcs scrollViewCalcs = reuse ?? new ScrollViewCalcs();
		if (scrollViewCalcs.scroller == null)
		{
			scrollViewCalcs.scroller = rt.GetComponentInParent<ScrollRect>();
		}
		if (scrollViewCalcs.scroller != null && scrollViewCalcs.scroller.vertical)
		{
			Vector3[] array = new Vector3[4];
			Vector3[] array2 = new Vector3[4];
			scrollViewCalcs.scroller.content.GetWorldCorners(array);
			rt.GetWorldCorners(array2);
			scrollViewCalcs.contentHeight = scrollViewCalcs.scroller.content.rect.height * (float)Options.StageScale;
			scrollViewCalcs.scrollHeight = scrollViewCalcs.scroller.viewport.rect.height * (float)Options.StageScale;
			scrollViewCalcs.contentTop = array[2].y - array2[2].y;
			scrollViewCalcs.contentBottom = array[2].y - array2[0].y;
			scrollViewCalcs.scrollPercent = scrollViewCalcs.scroller.verticalNormalizedPosition;
			scrollViewCalcs.scrollTop = (1f - scrollViewCalcs.scrollPercent) * (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			scrollViewCalcs.scrollBottom = scrollViewCalcs.scrollTop + scrollViewCalcs.scrollHeight;
		}
		return scrollViewCalcs;
	}

	public static void ScrollIntoView(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollRect componentInParent = rectTransform.GetComponentInParent<ScrollRect>();
		if (!(componentInParent != null) || !componentInParent.vertical)
		{
			return;
		}
		ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
		if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
		{
			if (scrollViewCalcs.isAnyAboveView)
			{
				componentInParent.verticalNormalizedPosition = 1f - scrollViewCalcs.contentTop / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
			else if (scrollViewCalcs.isAnyBelowView)
			{
				componentInParent.verticalNormalizedPosition = 1f - (scrollViewCalcs.contentBottom - scrollViewCalcs.scrollHeight) / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
		}
	}

	public static void ScrollToTopOfRect(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollRect componentInParent = rectTransform.GetComponentInParent<ScrollRect>();
		if (componentInParent != null && componentInParent.vertical)
		{
			ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
			if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
			{
				componentInParent.verticalNormalizedPosition = 1f - scrollViewCalcs.contentTop / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
		}
	}

	public static void ScrollToBottomOfRect(RectTransform rectTransform, ScrollViewCalcs reuse = null)
	{
		ScrollRect componentInParent = rectTransform.GetComponentInParent<ScrollRect>();
		if (componentInParent != null && componentInParent.vertical)
		{
			ScrollViewCalcs scrollViewCalcs = GetScrollViewCalcs(rectTransform, reuse);
			if (scrollViewCalcs.scrollHeight < scrollViewCalcs.contentHeight)
			{
				componentInParent.verticalNormalizedPosition = 1f - (scrollViewCalcs.contentBottom - scrollViewCalcs.scrollHeight) / (scrollViewCalcs.contentHeight - scrollViewCalcs.scrollHeight);
			}
		}
	}
}
