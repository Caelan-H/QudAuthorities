using UnityEngine;
using UnityEngine.EventSystems;

public class TradeScrollDropHandler : MonoBehaviour, IDropHandler, IEventSystemHandler
{
	public TradeViewBehaviour tradeView;

	public int side;

	public void OnDrop(PointerEventData eventData)
	{
		if (ItemLineManager.itemBeingDragged != null)
		{
			TradeAmountViewBehavior.Show(ItemLineManager.itemBeingDragged);
		}
	}
}
