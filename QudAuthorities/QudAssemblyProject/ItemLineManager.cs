using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Kobold;
using Qud.API;
using QupKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

public class ItemLineManager : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
	[NonSerialized]
	public XRL.World.GameObject go;

	[NonSerialized]
	public BodyPart bodyPart;

	public UIThreeColorProperties colorProperties;

	public Image image;

	public UnityEngine.UI.Text text;

	public UnityEngine.UI.Text rightText;

	public int weight;

	public float HoldTime;

	public static bool skipClick;

	public bool holding;

	public string mode = "ItemList";

	public Action<ItemLineManager> onClick;

	public Selectable button;

	private bool isDefault;

	public static UnityEngine.GameObject dragObject;

	public static ItemLineManager itemBeingDragged;

	public static Vector3 dragStart;

	public static float startAlptha;

	public static bool dragging;

	public static bool wasDragging;

	public static bool cancelDrag;

	public static Canvas mainCanvas;

	public void SetOnClick(Action<ItemLineManager> action)
	{
		onClick = action;
	}

	public void PoolReset()
	{
		go = null;
		bodyPart = null;
		onClick = null;
		button.interactable = false;
		button.interactable = true;
	}

	public void StopHold()
	{
		holding = false;
	}

	public void SetDefault(bool value)
	{
		isDefault = value;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		holding = true;
	}

	public void Held()
	{
		if (mode == "Trade")
		{
			TradeUI.SetSelectedObject(go);
			Keyboard.PushKey(UnityEngine.KeyCode.Tab);
			cancelDrag = true;
		}
	}

	public void Clicked()
	{
		if (onClick != null)
		{
			onClick(this);
		}
		else if (mode == "Trade")
		{
			TradeAmountViewBehavior.Show(this);
			LegacyViewManager.Instance.SetActiveView("Popup:TradeAmount", bHideOldView: false);
		}
		else
		{
			if (!(mode != "PickItem"))
			{
				return;
			}
			if (go != null && !isDefault)
			{
				GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
				{
					EquipmentAPI.TwiddleObject(go);
				});
			}
			else if (mode == "Equipment" && bodyPart != null)
			{
				GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
				{
					EquipmentScreen.ShowBodypartEquipUI(XRLCore.Core.Game.Player.Body, bodyPart);
				});
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!skipClick && !eventData.dragging && !wasDragging && HoldTime <= 0.5f && holding)
		{
			if (GetComponent<Button>() != null)
			{
				GetComponent<Button>().Select();
			}
			holding = false;
			HoldTime = 0f;
			if (mode == "Trade")
			{
				return;
			}
			if (mode == "Nearby")
			{
				Clicked();
			}
			else
			{
				if (mode == "Inventory" || mode == "Equipment" || mode == "PickItem")
				{
					return;
				}
				if (eventData.button == PointerEventData.InputButton.Left)
				{
					GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
					{
						if (go.FireEvent(XRL.World.Event.New("CommandSmartUseEarly", "User", XRLCore.Core.Game.Player.Body)))
						{
							if (go.HasPart("ConversationScript"))
							{
								if (go.GetPart("ConversationScript") is ConversationScript conversationScript)
								{
									if (go.pPhysics.Temperature <= go.pPhysics.BrittleTemperature)
									{
										Popup.Show("You hear a muffled grunting coming from inside the block of ice.");
									}
									else
									{
										conversationScript.FireEvent(XRL.World.Event.New("HaveConversation"));
									}
								}
							}
							else
							{
								Dictionary<string, List<IPart>> registeredPartEvents = go.RegisteredPartEvents;
								if (registeredPartEvents != null && registeredPartEvents.ContainsKey("CommandSmartUse"))
								{
									go.FireEvent(XRL.World.Event.New("CommandSmartUse", "User", XRLCore.Core.Game.Player.Body));
								}
								else if (go.IsTakeable())
								{
									XRLCore.Core.Game.Player.Body.FireEvent(XRL.World.Event.New("CommandTakeObject", "Object", go));
								}
							}
						}
					});
				}
				else
				{
					Clicked();
				}
			}
		}
		skipClick = false;
		holding = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (mode == "Nearby")
		{
			ShowTooltip(bStayOpen: false);
		}
	}

	public void Update()
	{
		if (holding && !dragging)
		{
			if (HoldTime == 0f)
			{
				cancelDrag = false;
			}
			HoldTime += Time.deltaTime;
			if (HoldTime > 0.5f)
			{
				HoldTime = 0f;
				skipClick = true;
				holding = false;
				Held();
			}
		}
		if (!dragging && wasDragging)
		{
			wasDragging = false;
		}
	}

	public void SetMode(string newMode)
	{
		mode = newMode;
	}

	public void CopyFrom(ItemLineManager source)
	{
		text.text = source.text.text;
		rightText.text = source.rightText.text;
		base.gameObject.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta = source.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta;
		image.sprite = source.image.sprite;
		colorProperties.SetColors(source.colorProperties.Foreground, source.colorProperties.Detail, source.colorProperties.Background);
	}

	public void SetGameObject(QudItemListElement item)
	{
		if (item == null)
		{
			go = null;
			bodyPart = null;
			return;
		}
		text.text = item.displayName;
		if (mode == "Trade")
		{
			int numberSelected = TradeUI.GetNumberSelected(item.go);
			if (numberSelected > 0)
			{
				item.rightText = " [x" + numberSelected + "] $" + $"{TradeUI.GetValue(item.go):0.##}";
			}
			else
			{
				item.rightText = " $" + $"{TradeUI.GetValue(item.go):0.##}";
			}
		}
		if (rightText != null)
		{
			rightText.text = item.rightText;
		}
		weight = item.weight;
		go = item.go;
		if (mode == "Nearby")
		{
			base.gameObject.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta = new Vector2(16f, 24f);
			image.sprite = item.GenerateSprite();
			colorProperties.SetColors(item.foregroundColor, item.detailColor, item.backgroundColor);
		}
		else if (mode == "Equipment")
		{
			if (go == null || isDefault)
			{
				base.gameObject.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 48f);
				if (bodyPart != null)
				{
					if (bodyPart.Type == "Body")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/body.png");
					}
					if (bodyPart.Type == "Arm")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/arm.png");
					}
					if (bodyPart.Type == "Hand")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/hand2.png");
					}
					if (bodyPart.Type == "Head")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/head.png");
					}
					if (bodyPart.Type == "Face")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/face.png");
					}
					if (bodyPart.Type == "Hands")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/hands.png");
					}
					if (bodyPart.Type == "Back")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/back.png");
					}
					if (bodyPart.Type == "Feet")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/feet.png");
					}
					if (bodyPart.Type == "Floating Nearby")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/float.png");
					}
					if (bodyPart.Type == "Missile Weapon")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/missile.png");
					}
					if (bodyPart.Type == "Thrown Weapon")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/thrown.png");
					}
					if (bodyPart.Type == "Foot")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/hand1.png");
					}
					if (bodyPart.Type == "Tail")
					{
						image.sprite = SpriteManager.GetUnitySprite("UI/hand1.png");
					}
				}
				colorProperties.SetColors(new Color(0f, 0f, 0f, 0f), new Color(5f / 64f, 7f / 32f, 5f / 64f, 1f), new Color(0f, 0f, 0f, 0f));
			}
			else
			{
				base.gameObject.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 72f);
				image.sprite = item.GenerateSprite();
				colorProperties.SetColors(item.foregroundColor, item.detailColor, item.backgroundColor);
			}
		}
		else
		{
			base.gameObject.transform.Find("3CImage").GetComponent<RectTransform>().sizeDelta = new Vector2(32f, 48f);
			image.sprite = item.GenerateSprite();
			colorProperties.SetColors(item.foregroundColor, item.detailColor, item.backgroundColor);
		}
	}

	private void SetBodyPart(BodyPart part)
	{
		bodyPart = part;
	}

	public void OnScroll(PointerEventData eventData)
	{
		holding = false;
		base.transform.parent.parent.parent.SendMessage("OnScroll", eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		holding = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (cancelDrag)
		{
			eventData.dragging = false;
			return;
		}
		if (mode == "PickItem" || mode == "Nearby")
		{
			eventData.dragging = false;
			return;
		}
		mainCanvas = LegacyViewManager.Instance.MainCanvas;
		dragObject = UnityEngine.Object.Instantiate(base.transform.Find("3CImage")).gameObject;
		dragObject.transform.SetParent(mainCanvas.transform);
		itemBeingDragged = this;
		startAlptha = GetComponent<CanvasGroup>().alpha;
		GetComponent<CanvasGroup>().alpha = 0.3f;
		dragging = true;
		wasDragging = true;
		holding = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (dragging)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, eventData.position, GameManager.cameraMainCamera, out var localPoint);
			dragObject.transform.position = mainCanvas.transform.TransformPoint(localPoint);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		UnityEngine.Object.Destroy(dragObject);
		GetComponent<CanvasGroup>().alpha = startAlptha;
		dragging = false;
		dragObject = null;
		itemBeingDragged = null;
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (mode == "Trade")
		{
			base.gameObject.transform.parent.SendMessageUpwards("OnDrop", eventData);
			return;
		}
		if (mode == "Equipment")
		{
			XRL.World.GameObject itemToEquip = itemBeingDragged.go;
			GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
			{
				EquipmentAPI.EquipObjectToPlayer(itemToEquip, bodyPart);
				QudItemList newList2 = ObjectPool<QudItemList>.Checkout();
				newList2.Add(XRLCore.Core.Game.Player.Body.GetPart<Inventory>().Objects);
				newList2.eqWeight = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetWeight();
				GameManager.Instance.uiQueue.queueTask(delegate
				{
					InventoryView.instance.UpdateObjectList(newList2);
				});
			});
			skipClick = true;
		}
		if (!(mode == "Inventory"))
		{
			return;
		}
		XRL.World.GameObject itemToEquip2 = itemBeingDragged.go;
		GameManager.Instance.gameQueue.queueSingletonTask("sidebarcommand", delegate
		{
			EquipmentAPI.UnequipObject(itemToEquip2);
			QudItemList newList = ObjectPool<QudItemList>.Checkout();
			newList.Add(XRLCore.Core.Game.Player.Body.GetPart<Inventory>().Objects);
			newList.eqWeight = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetWeight();
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				InventoryView.instance.UpdateObjectList(newList);
			});
		});
		skipClick = true;
	}

	public void ShowTooltip(bool bStayOpen)
	{
		if (go != null)
		{
			Look.QueueItemTooltip(Input.mousePosition, go, bStayOpen);
		}
		else if (bodyPart != null)
		{
			if (bodyPart.Equipped != null)
			{
				Look.QueueItemTooltip(Input.mousePosition, bodyPart.Equipped, bStayOpen);
			}
			else if (bodyPart.DefaultBehavior != null)
			{
				Look.QueueItemTooltip(Input.mousePosition, bodyPart.DefaultBehavior, bStayOpen);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (mode == "Trade")
			{
				TradeUI.SetSelectedObject(go);
				Keyboard.PushKey(UnityEngine.KeyCode.Tab);
			}
			else
			{
				ShowTooltip(bStayOpen: false);
			}
		}
		else if (onClick != null)
		{
			if (mode == "Nearby")
			{
				ShowTooltip(bStayOpen: false);
			}
			else
			{
				onClick(this);
			}
		}
	}
}
