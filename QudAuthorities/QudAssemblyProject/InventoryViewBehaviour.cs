using UnityEngine;
using UnityEngine.UI;

public class InventoryViewBehaviour : MonoBehaviour
{
	public static InventoryViewBehaviour instance;

	public InventoryFilterBarBehaviour filterBar;

	public TotalWeightPanelBehavior totalWeight;

	public ScrollRect inventoryScroll;

	public ScrollRect equipmentScroll;

	public Text totalWeightText;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
	}
}
