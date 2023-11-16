using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryFilterBarButton : MonoBehaviour
{
	public InventoryFilterBarBehaviour filterBar;

	public UIThreeColorProperties color;

	public Selectable selectable;

	public string _category;

	public string category
	{
		get
		{
			return _category;
		}
		set
		{
			if (value == "*")
			{
				filterBar.toggledButton = this;
				color.Foreground = new Color(57f / 128f, 93f / 128f, 0.50390625f, 1f);
				color.Detail = new Color(57f / 128f, 93f / 128f, 0.50390625f, 1f);
			}
			else
			{
				color.Foreground = new Color(0.5f, 0.5f, 0.5f, 1f);
				color.Detail = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			_category = value;
		}
	}

	public void PoolReset()
	{
		selectable.interactable = true;
	}

	public void Clicked()
	{
		if (filterBar.toggledButton != null)
		{
			filterBar.toggledButton.color.Foreground = new Color(0.5f, 0.5f, 0.5f, 1f);
			filterBar.toggledButton.color.Detail = new Color(0.5f, 0.5f, 0.5f, 1f);
		}
		filterBar.toggledButton = this;
		filterBar.toggledButton.color.Foreground = new Color(57f / 128f, 93f / 128f, 0.50390625f, 1f);
		filterBar.toggledButton.color.Detail = new Color(57f / 128f, 93f / 128f, 0.50390625f, 1f);
		if (category == "*")
		{
			foreach (KeyValuePair<string, ObjectToggler> toggle in filterBar.inventory.toggles)
			{
				toggle.Value.transform.gameObject.SetActive(value: true);
				if (!toggle.Value.toggled)
				{
					toggle.Value.Toggle();
				}
			}
			return;
		}
		foreach (KeyValuePair<string, ObjectToggler> toggle2 in filterBar.inventory.toggles)
		{
			if (toggle2.Key == category)
			{
				toggle2.Value.transform.gameObject.SetActive(value: true);
				if (!toggle2.Value.toggled)
				{
					toggle2.Value.Toggle();
				}
			}
			else
			{
				if (toggle2.Value.toggled)
				{
					toggle2.Value.Toggle();
				}
				toggle2.Value.transform.gameObject.SetActive(value: false);
			}
		}
	}
}
