using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Qud.UI;

public class MoveableSceneFrameHeader : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDragHandler
{
	public GameObject highlightTarget;

	public GameObject dragTarget;

	public Image backgroundImage;

	public UnityEvent<int> backgroundOpacityToggled = new UnityEvent<int>();

	public UnityEvent<bool> sideMovePinToggled = new UnityEvent<bool>();

	public bool sideMovePin;

	public Image sideMoveDisplay;

	public string prefsKey = "MessageLog";

	public int BackgroundOpacity = 1;

	public string OpacityID => prefsKey + "-Opacity";

	public string SidePinID => prefsKey + "-SidePin";

	public virtual void Init()
	{
		ToggleBackgroundOpacity(PlayerPrefs.GetInt(OpacityID));
		ToggleSideMovePin(PlayerPrefs.GetInt(SidePinID, 0) == 1);
		backgroundOpacityToggled.AddListener(UpdateBackgroundOpacity);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		dragTarget.GetComponent<RectTransform>().anchoredPosition += new Vector2(eventData.delta.x, eventData.delta.y) / UIManager.scale;
	}

	public void UpdateBackgroundOpacity(int opacityLevel)
	{
		PlayerPrefs.SetInt(OpacityID, opacityLevel);
	}

	public void ToggleBackgroundOpacity()
	{
		BackgroundOpacity++;
		if (BackgroundOpacity > 2)
		{
			BackgroundOpacity = 0;
		}
		ToggleBackgroundOpacity(BackgroundOpacity);
	}

	public void ToggleBackgroundOpacity(int setTo)
	{
		BackgroundOpacity = setTo;
		Color color = backgroundImage.color;
		color.a = 0.1f;
		if (setTo == 0)
		{
			color.a = 0f;
		}
		if (setTo == 1)
		{
			color.a = 0.1f;
		}
		if (setTo == 2)
		{
			color.a = 1f;
		}
		backgroundImage.color = color;
		backgroundOpacityToggled.Invoke(setTo);
	}

	public void ToggleSideMovePin()
	{
		ToggleSideMovePin(!sideMovePin);
	}

	public void ToggleSideMovePin(bool setTo)
	{
		PlayerPrefs.SetInt(SidePinID, setTo ? 1 : 0);
		sideMovePin = setTo;
		if (setTo)
		{
			sideMoveDisplay.color = Color.red;
		}
		else
		{
			sideMoveDisplay.color = Color.white;
		}
		sideMovePinToggled.Invoke(setTo);
	}
}
