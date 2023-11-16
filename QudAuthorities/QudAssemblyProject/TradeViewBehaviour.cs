using ConsoleLib.Console;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class TradeViewBehaviour : MonoBehaviour
{
	public static TradeViewBehaviour instance;

	public InventoryFilterBarBehaviour filterBar1;

	public InventoryFilterBarBehaviour filterBar2;

	public TotalWeightPanelBehavior totalWeight;

	public ScrollRect inventoryScroll1;

	public ScrollRect inventoryScroll2;

	public Text totalWeightText;

	public Text infoText;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
		if (GameManager.Instance.PrereleaseInput && ReInput.players.GetPlayer(0).GetButtonDown("Cancel"))
		{
			ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Escape);
		}
	}
}
