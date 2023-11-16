using ConsoleLib.Console;
using Rewired;
using UnityEngine;

public class PopupItemBehaviour : MonoBehaviour
{
	public GameObject takeAll;

	public GameObject store;

	public TotalWeightPanelBehavior totalWeight;

	public static PopupItemBehaviour instance;

	public InventoryPanelBehaviour inventoryPanel;

	public void Awake()
	{
		instance = this;
	}

	public void EnableTakeAll(bool state)
	{
		takeAll.SetActive(state);
	}

	public void EnableStore(bool state)
	{
		store.SetActive(state);
	}

	private void Update()
	{
		if (ReInput.players.GetPlayer(0).GetButtonDown("Cancel"))
		{
			ConsoleLib.Console.Keyboard.PushKey(UnityEngine.KeyCode.Escape);
		}
	}
}
