using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.UI;

namespace XRL.UI.Framework;

public class CategoryMenusScroller : FrameworkScroller
{
	public UITextSkin selectedTitleText;

	public UIThreeColorProperties selectedIcon;

	public UITextSkin selectedDescriptionText;

	public RectTransform safeArea;

	public RectTransform OuterVLayout;

	public RectTransform DescriptionHLayout;

	public RectTransform DescriptionArea;

	public float lastSafeAreaWidth;

	public int descriptionBottomBreakpoint;

	public bool hasShown;

	public bool breakDescription => lastSafeAreaWidth < (float)descriptionBottomBreakpoint;

	public void UpdateDescriptions(FrameworkDataElement dataElement)
	{
		if (dataElement is PrefixMenuOption prefixMenuOption)
		{
			selectedTitleText.SetText(prefixMenuOption.Description);
			selectedDescriptionText.SetText(prefixMenuOption.LongDescription);
			selectedIcon.FromRenderable(prefixMenuOption.Renderable);
		}
	}

	public NavigationContext ContextFor(int index, int subMenuIndex)
	{
		return GetPrefabForIndex(index).GetComponent<FrameworkScroller>().scrollContext.GetContextAt(subMenuIndex);
	}

	public override void ScrollSelectedIntoView()
	{
	}

	public override void BeforeShow(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections = null)
	{
		base.BeforeShow(descriptor, selections);
		if (!hasShown)
		{
			try
			{
				UpdateDescriptions((scrollContext.GetDataAt(0) as CategoryMenuData).menuOptions[0]);
				hasShown = true;
			}
			catch
			{
			}
		}
		onHighlight.RemoveAllListeners();
		onHighlight.AddListener(UpdateDescriptions);
	}

	public override ScrollChildContext MakeContextFor(FrameworkDataElement data, int index)
	{
		return new ScrollChildContext
		{
			proxyTo = GetPrefabForIndex(index).GetComponent<FrameworkScroller>().scrollContext
		};
	}

	public override void SetupPrefab(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
	{
		base.SetupPrefab(newChild, context, data, index);
		FrameworkScroller component = newChild.GetComponent<FrameworkScroller>();
		component.onSelected = onSelected;
		component.onHighlight = onHighlight;
	}

	public override void Update()
	{
		base.Update();
		if (safeArea.rect.width != lastSafeAreaWidth)
		{
			lastSafeAreaWidth = (int)Math.Floor(safeArea.rect.width);
			if (breakDescription)
			{
				DescriptionArea.SetParent(OuterVLayout, worldPositionStays: false);
				DescriptionArea.SetSiblingIndex(DescriptionHLayout.GetSiblingIndex() + 1);
			}
			else
			{
				DescriptionArea.SetParent(DescriptionHLayout, worldPositionStays: false);
				DescriptionArea.SetSiblingIndex(2);
			}
		}
	}
}
