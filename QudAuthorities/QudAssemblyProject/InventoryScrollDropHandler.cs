using Qud.API;
using QupKit;
using UnityEngine;
using UnityEngine.EventSystems;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

public class InventoryScrollDropHandler : MonoBehaviour, IDropHandler, IEventSystemHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		if (!(ItemLineManager.itemBeingDragged != null))
		{
			return;
		}
		XRL.World.GameObject itemToEquip = ItemLineManager.itemBeingDragged.go;
		GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
		{
			EquipmentAPI.UnequipObject(itemToEquip);
			QudItemList newList = ObjectPool<QudItemList>.Checkout();
			newList.Add(XRLCore.Core.Game.Player.Body.GetPart<Inventory>().Objects);
			newList.eqWeight = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetWeight();
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				InventoryView.instance.UpdateObjectList(newList);
			});
		});
		ItemLineManager.skipClick = true;
	}
}
