using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class KeybindRow : MonoBehaviour, IFrameworkControl
{
	public Image background;

	public GameObject bindingDisplay;

	public GameObject categoryDisplay;

	public UITextSkin categoryDescription;

	public UITextSkin categoryExpander;

	public UITextSkin description;

	public KeybindBox box1;

	public KeybindBox box2;

	public ScrollContext<KeybindType, NavigationContext> subscroller = new ScrollContext<KeybindType, NavigationContext>();

	public NavigationContext categoryContext = new NavigationContext();

	public FrameworkContext frameworkContext;

	public bool selectedMode;

	public KeybindDataRow dataRow = new KeybindDataRow
	{
		KeyDescription = "Interact Nearby",
		Bind1 = "Ctrl+Space",
		Bind2 = null
	};

	public KeybindCategoryRow categoryRow;

	public bool editorUpdateDataRow;

	public UnityEvent<KeybindDataRow, KeybindType, KeybindRow> onRebind = new UnityEvent<KeybindDataRow, KeybindType, KeybindRow>();

	private bool? wasSelected;

	public NavigationContext GetNavigationContext()
	{
		if (frameworkContext.context is ScrollChildContext scrollChildContext)
		{
			if (categoryRow != null)
			{
				scrollChildContext.proxyTo = categoryContext;
			}
			else
			{
				box1.context.RequireContext<ScrollChildContext>();
				box1.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					XRL.UI.Framework.Event.Helpers.Handle(delegate
					{
						onRebind?.Invoke(dataRow, KeybindType.Primary, this);
					})
				} };
				box2.context.RequireContext<ScrollChildContext>();
				box2.context.context.buttonHandlers = new Dictionary<InputButtonTypes, Action> { 
				{
					InputButtonTypes.AcceptButton,
					XRL.UI.Framework.Event.Helpers.Handle(delegate
					{
						onRebind?.Invoke(dataRow, KeybindType.Alternate, this);
					})
				} };
				scrollChildContext.proxyTo = subscroller;
				subscroller.SetAxis(InputAxisTypes.NavigationXAxis);
				subscroller.wraps = false;
				if (subscroller.data.Count == 0)
				{
					subscroller.data.Add(KeybindType.Primary);
					subscroller.data.Add(KeybindType.Alternate);
					subscroller.contexts.Add(box1.context.context);
					subscroller.contexts.Add(box2.context.context);
				}
			}
		}
		return frameworkContext.context;
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is KeybindDataRow keybindDataRow)
		{
			categoryDisplay.SetActive(value: false);
			bindingDisplay.SetActive(value: true);
			categoryRow = null;
			dataRow = keybindDataRow;
			description.text = "{{C|" + keybindDataRow.KeyDescription + "}}";
			description.Apply();
			box1.boxText = (string.IsNullOrEmpty(keybindDataRow.Bind1) ? "{{c|None}}" : ("{{w|" + keybindDataRow.Bind1 + "}}"));
			box1.forceUpdate = true;
			box2.boxText = (string.IsNullOrEmpty(keybindDataRow.Bind2) ? "{{c|None}}" : ("{{w|" + keybindDataRow.Bind2 + "}}"));
			box2.forceUpdate = true;
		}
		else if (data is KeybindCategoryRow keybindCategoryRow)
		{
			categoryDisplay.SetActive(value: true);
			bindingDisplay.SetActive(value: false);
			categoryRow = keybindCategoryRow;
			dataRow = null;
			categoryDescription.text = "{{C|" + categoryRow.CategoryDescription.ToUpper() + "}}";
			categoryDescription.Apply();
			if (keybindCategoryRow.Collapsed)
			{
				categoryExpander.SetText("{{C|[+]}}");
			}
			else
			{
				categoryExpander.SetText("{{C|[-]}}");
			}
		}
	}

	public void Update()
	{
		if (editorUpdateDataRow)
		{
			editorUpdateDataRow = false;
			if (dataRow != null && !string.IsNullOrEmpty(dataRow.KeyDescription))
			{
				setData(dataRow);
			}
			else if (categoryRow != null)
			{
				setData(categoryRow);
			}
		}
		bool? flag = GetNavigationContext()?.IsActive();
		if (wasSelected != flag)
		{
			wasSelected = flag;
			bool flag2 = (background.enabled = flag.GetValueOrDefault());
			selectedMode = flag2;
		}
	}
}
