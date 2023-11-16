using UnityEngine;
using UnityEngine.EventSystems;

public class TileAreaHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IMoveHandler, IPointerExitHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public bool PointerInside;

	public float hoverTimer;

	public float hoverDelay = 0.5f;

	private Camera mainCamera;

	private Vector3 lastMousePosition = Vector3.zero;

	public void OnMove(AxisEventData eventData)
	{
		hoverTimer = 0f;
	}

	public void StartHover(Vector2 position)
	{
		if (!GameManager.Instance.MouseInput)
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(position));
		foreach (RaycastHit raycastHit in array)
		{
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				GameManager.Instance.TileHover(component.x, component.y);
			}
		}
	}

	public void EndHover()
	{
		hoverTimer = 0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.dragging || !GameManager.Instance.MouseInput)
		{
			return;
		}
		RaycastHit[] array = Physics.RaycastAll(GameManager.MainCamera.GetComponent<Camera>().ScreenPointToRay(eventData.position));
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				if (eventData.button == PointerEventData.InputButton.Left)
				{
					GameManager.Instance.OnTileClicked("LeftClick", component.x, component.y);
				}
				else
				{
					GameManager.Instance.OnTileClicked("RightClick", component.x, component.y);
				}
			}
			BorderTileBehavior component2 = raycastHit.collider.gameObject.GetComponent<BorderTileBehavior>();
			if (component2 != null)
			{
				GameManager.Instance.OnControlPanelButton(component2.Direction);
			}
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		GameManager.Instance.OnScroll(eventData.scrollDelta);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerInside = true;
		hoverTimer = 0f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerInside = false;
		hoverTimer = 0f;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!GameManager.Instance.MouseInput || GameManager.MainCamera == null)
		{
			return;
		}
		if (mainCamera == null)
		{
			mainCamera = GameManager.MainCamera.GetComponent<Camera>();
		}
		if (mainCamera == null || !PointerInside)
		{
			return;
		}
		if (lastMousePosition == Input.mousePosition)
		{
			hoverTimer += Time.deltaTime;
		}
		else
		{
			EndHover();
		}
		lastMousePosition = Input.mousePosition;
		RaycastHit[] array = Physics.RaycastAll(mainCamera.ScreenPointToRay(Input.mousePosition));
		foreach (RaycastHit raycastHit in array)
		{
			TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
			if (component != null)
			{
				GameManager.Instance.OnTileOver(component.x, component.y);
			}
		}
		if (hoverTimer > hoverDelay)
		{
			StartHover(Input.mousePosition);
			hoverTimer = 0f;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		GameManager.Instance.OnDrag(eventData.delta);
	}
}
