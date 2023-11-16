using System;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems;

[AddComponentMenu("Event/Desktop Input Module")]
public class DesktopInputModule : PointerInputModule
{
	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public enum InputMode
	{
		Mouse,
		Buttons
	}

	private float m_PrevActionTime;

	private Vector2 m_LastMoveVector;

	private int m_ConsecutiveMoveCount;

	private Vector2 m_LastMousePosition;

	private Vector2 m_MousePosition;

	[SerializeField]
	private string m_HorizontalAxis = "Horizontal";

	[SerializeField]
	private string m_VerticalAxis = "Vertical";

	[SerializeField]
	private string m_SubmitButton = "Submit";

	[SerializeField]
	private string m_CancelButton = "Cancel";

	[SerializeField]
	private float m_InputActionsPerSecond = 10f;

	[SerializeField]
	private float m_RepeatDelay = 0.5f;

	[SerializeField]
	[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
	private bool m_ForceModuleActive;

	[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
	public InputMode inputMode => InputMode.Mouse;

	[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
	public bool allowActivationOnMobileDevice
	{
		get
		{
			return m_ForceModuleActive;
		}
		set
		{
			m_ForceModuleActive = value;
		}
	}

	public bool forceModuleActive
	{
		get
		{
			return m_ForceModuleActive;
		}
		set
		{
			m_ForceModuleActive = value;
		}
	}

	public float inputActionsPerSecond
	{
		get
		{
			return m_InputActionsPerSecond;
		}
		set
		{
			m_InputActionsPerSecond = value;
		}
	}

	public float repeatDelay
	{
		get
		{
			return RewiredExtensions.delaytime;
		}
		set
		{
			m_RepeatDelay = value;
		}
	}

	public string horizontalAxis
	{
		get
		{
			return m_HorizontalAxis;
		}
		set
		{
			m_HorizontalAxis = value;
		}
	}

	public string verticalAxis
	{
		get
		{
			return m_VerticalAxis;
		}
		set
		{
			m_VerticalAxis = value;
		}
	}

	public string submitButton
	{
		get
		{
			return m_SubmitButton;
		}
		set
		{
			m_SubmitButton = value;
		}
	}

	public string cancelButton
	{
		get
		{
			return m_CancelButton;
		}
		set
		{
			m_CancelButton = value;
		}
	}

	protected DesktopInputModule()
	{
	}

	public override void UpdateModule()
	{
		m_LastMousePosition = m_MousePosition;
		m_MousePosition = Input.mousePosition;
	}

	public override bool IsModuleSupported()
	{
		return true;
	}

	public override bool ShouldActivateModule()
	{
		if (!base.ShouldActivateModule())
		{
			return false;
		}
		bool flag = m_ForceModuleActive;
		flag |= Input.GetButtonDown(m_SubmitButton);
		flag |= Input.GetButtonDown(m_CancelButton);
		flag |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0f);
		flag |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0f);
		flag |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f;
		flag |= Input.GetMouseButtonDown(0);
		if (Input.touchCount > 0)
		{
			flag = true;
		}
		return flag;
	}

	public override void ActivateModule()
	{
		base.ActivateModule();
		m_MousePosition = Input.mousePosition;
		m_LastMousePosition = Input.mousePosition;
		GameObject gameObject = base.eventSystem.currentSelectedGameObject;
		if (gameObject == null)
		{
			gameObject = base.eventSystem.firstSelectedGameObject;
		}
		base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
	}

	public override void DeactivateModule()
	{
		base.DeactivateModule();
		ClearSelection();
	}

	public override void Process()
	{
		bool flag = SendUpdateEventToSelectedObject();
		if (base.eventSystem.sendNavigationEvents)
		{
			if (!flag)
			{
				flag |= SendMoveEventToSelectedObject();
			}
			if (!flag)
			{
				SendSubmitEventToSelectedObject();
			}
		}
		if (!ProcessTouchEvents())
		{
			ProcessMouseEvent();
		}
	}

	private bool ProcessTouchEvents()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.type != TouchType.Indirect)
			{
				bool pressed;
				bool released;
				PointerEventData touchPointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
				ProcessTouchPress(touchPointerEventData, pressed, released);
				if (!released)
				{
					ProcessMove(touchPointerEventData);
					ProcessDrag(touchPointerEventData);
				}
				else
				{
					RemovePointerData(touchPointerEventData);
				}
			}
		}
		return Input.touchCount > 0;
	}

	private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
	{
		GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
		if (pressed)
		{
			pointerEvent.eligibleForClick = true;
			pointerEvent.delta = Vector2.zero;
			pointerEvent.dragging = false;
			pointerEvent.useDragThreshold = true;
			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			if (pointerEvent.pointerEnter != gameObject)
			{
				HandlePointerExitAndEnter(pointerEvent, gameObject);
				pointerEvent.pointerEnter = gameObject;
			}
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == pointerEvent.lastPress)
			{
				if (unscaledTime - pointerEvent.clickTime < 0.3f)
				{
					pointerEvent.clickCount++;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}
				pointerEvent.clickTime = unscaledTime;
			}
			else
			{
				pointerEvent.clickCount = 1;
			}
			pointerEvent.pointerPress = gameObject2;
			pointerEvent.rawPointerPress = gameObject;
			pointerEvent.clickTime = unscaledTime;
			pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (released)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (pointerEvent.pointerPress == eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			if (pointerEvent.pointerDrag != null)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.pointerDrag = null;
			ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
			pointerEvent.pointerEnter = null;
		}
	}

	protected bool SendSubmitEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		if (Input.GetButtonDown(m_SubmitButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
		}
		if (Input.GetButtonDown(m_CancelButton))
		{
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
		}
		return baseEventData.used;
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxisRaw(m_HorizontalAxis);
		zero.y = Input.GetAxisRaw(m_VerticalAxis);
		if (Input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (Input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	protected bool SendMoveEventToSelectedObject()
	{
		float unscaledTime = Time.unscaledTime;
		Vector2 rawMoveVector = GetRawMoveVector();
		if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
		{
			m_ConsecutiveMoveCount = 0;
			return false;
		}
		bool flag = Input.GetButtonDown(m_HorizontalAxis) || Input.GetButtonDown(m_VerticalAxis);
		bool flag2 = Vector2.Dot(rawMoveVector, m_LastMoveVector) > 0f;
		if (!flag)
		{
			flag = ((!flag2 || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + RewiredExtensions.repeattime) : (unscaledTime > m_PrevActionTime + m_RepeatDelay));
		}
		if (!flag)
		{
			return false;
		}
		AxisEventData axisEventData = GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
		if (!flag2)
		{
			m_ConsecutiveMoveCount = 0;
		}
		m_ConsecutiveMoveCount++;
		m_PrevActionTime = unscaledTime;
		m_LastMoveVector = rawMoveVector;
		return axisEventData.used;
	}

	protected void ProcessMouseEvent()
	{
		ProcessMouseEvent(0);
	}

	protected void ProcessMouseEvent(int id)
	{
		MouseState mousePointerEventData = GetMousePointerEventData(id);
		MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
		ProcessMousePress(eventData);
		ProcessMove(eventData.buttonData);
		ProcessDrag(eventData.buttonData);
		ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
		ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
		ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
		if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
		{
			ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
		}
	}

	protected bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	protected void ProcessMousePress(MouseButtonEventData data)
	{
		PointerEventData buttonData = data.buttonData;
		GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
		if (data.PressedThisFrame())
		{
			buttonData.eligibleForClick = true;
			buttonData.delta = Vector2.zero;
			buttonData.dragging = false;
			buttonData.useDragThreshold = true;
			buttonData.pressPosition = buttonData.position;
			buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
			GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
			if (gameObject2 == null)
			{
				gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			}
			float unscaledTime = Time.unscaledTime;
			if (gameObject2 == buttonData.lastPress)
			{
				if (unscaledTime - buttonData.clickTime < 0.3f)
				{
					buttonData.clickCount++;
				}
				else
				{
					buttonData.clickCount = 1;
				}
				buttonData.clickTime = unscaledTime;
			}
			else
			{
				buttonData.clickCount = 1;
			}
			buttonData.pointerPress = gameObject2;
			buttonData.rawPointerPress = gameObject;
			buttonData.clickTime = unscaledTime;
			buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
			if (buttonData.pointerDrag != null)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
			}
		}
		if (data.ReleasedThisFrame())
		{
			ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
			if (buttonData.pointerPress == eventHandler && buttonData.eligibleForClick)
			{
				ExecuteEvents.Execute(buttonData.pointerPress, buttonData, ExecuteEvents.pointerClickHandler);
			}
			else if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.dropHandler);
			}
			buttonData.eligibleForClick = false;
			buttonData.pointerPress = null;
			buttonData.rawPointerPress = null;
			if (buttonData.pointerDrag != null && buttonData.dragging)
			{
				ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.endDragHandler);
			}
			buttonData.dragging = false;
			buttonData.pointerDrag = null;
			if (gameObject != buttonData.pointerEnter)
			{
				HandlePointerExitAndEnter(buttonData, null);
				HandlePointerExitAndEnter(buttonData, gameObject);
			}
		}
	}
}
