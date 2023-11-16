using System;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

public class TradeAmountViewBehavior : MonoBehaviour
{
	public static TradeAmountViewBehavior instance;

	public ItemLineManager selectedItem;

	public ItemLineManager itemDisplay;

	public UnityEngine.UI.Text titleText;

	public InputField field;

	public static void Show(ItemLineManager item)
	{
		LegacyViewManager.Instance.SetActiveView("Popup:TradeAmount", bHideOldView: false);
		if (TradeUI.GetSideOfObject(item.go) == 0)
		{
			instance.titleText.text = "How many do you want to buy?";
		}
		else
		{
			instance.titleText.text = "How many do you want to sell?";
		}
		instance.selectedItem = item;
		instance.itemDisplay.CopyFrom(instance.selectedItem);
		instance.field.text = TradeUI.GetNumberSelected(item.go).ToString();
	}

	public void More()
	{
		int num = Convert.ToInt32(field.text);
		num++;
		if (num < 0)
		{
			num = 0;
		}
		if (num > selectedItem.go.Count)
		{
			num = selectedItem.go.Count;
		}
		field.text = num.ToString();
	}

	public void Less()
	{
		int num = Convert.ToInt32(field.text);
		num--;
		if (num < 0)
		{
			num = 0;
		}
		if (num > selectedItem.go.Count)
		{
			num = selectedItem.go.Count;
		}
		field.text = num.ToString();
	}

	public void All()
	{
		field.text = selectedItem.go.Count.ToString();
	}

	public void None()
	{
		field.text = "0";
	}

	public void Accept()
	{
		int num = Convert.ToInt32(field.text);
		if (num < 0)
		{
			num = 0;
		}
		if (num > selectedItem.go.Count)
		{
			num = selectedItem.go.Count;
		}
		TradeUI.SetNumberSelected(selectedItem.go, num);
		LegacyViewManager.Instance.SetActiveView("Trade");
		TradeView.instance.QueueInventoryUpdate();
	}

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
