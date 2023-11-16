using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XRL.UI.Framework;

[RequireComponent(typeof(FrameworkContext))]
public class FrameworkClickToAccept : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public NavigationContext context => GetComponent<FrameworkContext>()?.context;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (context != null && context.Activate())
		{
			NavigationController.instance.FireInputButtonEvent(InputButtonTypes.AcceptButton, new Dictionary<string, object> { { "PointerEventData", eventData } });
		}
	}
}
