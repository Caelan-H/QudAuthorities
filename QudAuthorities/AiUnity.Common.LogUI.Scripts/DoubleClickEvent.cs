using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts;

public class DoubleClickEvent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public UnityEvent doubleClickEvent;

	public void OnPointerClick(PointerEventData eventData)
	{
		_ = eventData.clickCount;
		_ = 1;
	}
}
