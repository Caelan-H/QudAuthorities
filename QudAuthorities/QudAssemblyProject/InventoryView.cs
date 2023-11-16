using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.API;
using QupKit;
using UnityEngine;
using UnityEngine.UI;
using XRL.Core;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

[UIView("Inventory", false, true, false, "Charactersheet,Menu", "Inventory", false, 0, false)]
public class InventoryView : BaseInventoryView
{
	public const int VIEW_COLUMNS = 5;

	public const int VIEW_ROWS = 20;

	public List<UnityEngine.GameObject> EquipmentButtons = new List<UnityEngine.GameObject>();

	public static string Title = "";

	public new static string Text = "";

	public static Action action1 = null;

	public static Action action2 = null;

	public static Action action3 = null;

	public Vector2 originalDialogSize = Vector2.zero;

	public Vector2 originalTextSize = Vector2.zero;

	public static InventoryView instance;

	public string idEquipmentContentControl = "Panel/Equipment Scroll/Viewport/Content";

	public string idEquipmentScrollViewControl = "Panel/Equipment Scroll";

	private UnityEngine.GameObject previousSelectedItem;

	private float lastScrollPosition = 1f;

	public override void OnCreate()
	{
		filterBar = base.rootObject.GetComponent<InventoryViewBehaviour>().filterBar;
		filterBar.inventory = this;
		instance = this;
		base.OnCreate();
	}

	public void UpdateObjectList(QudItemList newList)
	{
		int num = -1;
		int num2 = -1;
		if (previousSelectedItem != null && InventoryButtons.Contains(previousSelectedItem))
		{
			num = InventoryButtons.IndexOf(previousSelectedItem);
		}
		if (previousSelectedItem != null && EquipmentButtons.Contains(previousSelectedItem.transform.parent.gameObject))
		{
			num2 = EquipmentButtons.IndexOf(previousSelectedItem.transform.parent.gameObject);
		}
		if (num == -1 && num2 == -1)
		{
			num = 1;
		}
		string text = null;
		if (filterBar.toggledButton != null)
		{
			text = filterBar.toggledButton.category;
		}
		foreach (KeyValuePair<string, ObjectToggler> toggle in toggles)
		{
			BaseInventoryView.toggleState[toggle.Key] = toggle.Value.toggled;
		}
		if (currentList != null && currentList != newList)
		{
			currentList.Clear();
			ObjectPool<QudItemList>.Return(currentList);
			currentList = null;
		}
		currentList = newList;
		Clear();
		currentList.Categorize();
		UnityEngine.GameObject childGameObject = GetChildGameObject(idInventoryContent);
		int num3 = 0;
		InventoryViewBehaviour.instance.filterBar.AddCategory("*");
		int num4 = 0;
		for (int i = 0; i < currentList.categoryNames.Count; i++)
		{
			num3++;
			List<QudItemListElement> list = currentList.categories[currentList.categoryNames[i]];
			list.Sort(BaseInventoryView.displayNameSorter);
			UnityEngine.GameObject gameObject = PooledPrefabManager.Instantiate("ListCollapserLine");
			string text2 = currentList.categoryNames[i];
			gameObject.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = text2;
			gameObject.transform.SetParent(childGameObject.transform);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.name = currentList.categoryNames[i];
			gameObject.GetComponent<Button>().onClick.AddListener(delegate
			{
				GameManager.Instance.uiQueue.queueSingletonTask("InventoryUpdate", delegate
				{
					instance.QueueInventoryUpdate();
				});
			});
			toggles.Add(currentList.categoryNames[i], gameObject.GetComponent<ObjectToggler>());
			InventoryButtons.Add(gameObject);
			gameObject.transform.SetSiblingIndex(num3);
			InventoryViewBehaviour.instance.filterBar.AddCategory(currentList.categoryNames[i]);
			int num5 = 0;
			for (int j = 0; j < list.Count; j++)
			{
				num3++;
				UnityEngine.GameObject gameObject2 = PooledPrefabManager.Instantiate("InventoryItemLine");
				ItemLineManager component = gameObject2.GetComponent<ItemLineManager>();
				component.SetMode("Inventory");
				component.SetGameObject(list[j]);
				if (component.onClick != null)
				{
					Debug.LogError("onClick isn't null!");
				}
				InventoryButtons.Add(gameObject2);
				gameObject2.transform.SetParent(childGameObject.transform);
				gameObject2.transform.localScale = new Vector3(1f, 1f, 1f);
				gameObject2.transform.SetSiblingIndex(num3);
				gameObject.GetComponent<ObjectToggler>().objects.Add(gameObject2);
				num5 += component.weight;
				if (j == 0 && i == 0)
				{
					Select(gameObject2);
				}
			}
			gameObject.transform.Find("RightText").GetComponent<UnityEngine.UI.Text>().text = num5 + " lbs";
			num4 += num5;
		}
		num4 += newList.eqWeight;
		InventoryViewBehaviour.instance.totalWeight.SetWeight(num4, InventoryScreen.currentMaxWeight);
		UnityEngine.GameObject childGameObject2 = GetChildGameObject(idEquipmentContentControl);
		List<BodyPart> parts = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetParts();
		Dictionary<string, List<BodyPart>> dictionary = new Dictionary<string, List<BodyPart>>();
		foreach (BodyPart item in parts)
		{
			if (!dictionary.ContainsKey(item.Type))
			{
				dictionary.Add(item.Type, new List<BodyPart>());
			}
			dictionary[item.Type].Add(item);
		}
		BodyPart[,] array = new BodyPart[5, 20];
		int num6 = 0;
		int num7 = num6;
		if (dictionary.ContainsKey("Head"))
		{
			bool flag = false;
			foreach (BodyPart item2 in dictionary["Head"])
			{
				if ((((uint)item2.Laterality & (true ? 1u : 0u)) != 0 || ((uint)item2.Laterality & 2u) != 0) && item2.Parts != null && item2.Parts.Count > 1)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = false;
			bool flag3 = false;
			foreach (BodyPart item3 in dictionary["Head"])
			{
				bool flag4 = true;
				int num8 = 2;
				int num9 = 3;
				int num10 = 1;
				if (((uint)item3.Laterality & (true ? 1u : 0u)) != 0)
				{
					num8 = 1;
					num9 = 0;
					num10 = 2;
					flag2 = true;
					flag4 = false;
				}
				else if (((uint)item3.Laterality & 2u) != 0)
				{
					num8 = 3;
					num9 = 4;
					num10 = 2;
					flag3 = true;
					flag4 = false;
				}
				array[num8, num6] = item3;
				if (item3.Parts != null && item3.Parts.Count > 0)
				{
					int num11 = num6;
					if (flag || item3.Parts.Count == 1)
					{
						array[num9, num6] = item3.Parts[0];
						for (int k = 1; k < item3.Parts.Count; k++)
						{
							num11++;
							num7 = Math.Max(num7, num11);
							array[num8, num11] = item3.Parts[k];
						}
					}
					else
					{
						array[num10, num6] = item3.Parts[0];
						array[num9, num6] = item3.Parts[1];
						for (int l = 2; l < item3.Parts.Count; l++)
						{
							num11++;
							num7 = Math.Max(num7, num11);
							array[num8, num11] = item3.Parts[l];
						}
					}
				}
				if (flag4)
				{
					num6 = num7 + 1;
				}
				else if (flag2 && flag3)
				{
					num6 = num7 + 1;
					flag2 = false;
					flag3 = false;
				}
			}
		}
		if (dictionary.ContainsKey("Body"))
		{
			foreach (BodyPart item4 in dictionary["Body"])
			{
				BodyPart bodyPart = (array[2, num6] = item4);
				List<BodyPart> list2 = new List<BodyPart>(1);
				bodyPart.GetPart("Back", list2);
				for (int m = 0; m < list2.Count; m++)
				{
					array[1 - m, num6] = list2[m];
				}
				num6++;
			}
		}
		int num12 = num6;
		int num13 = 0;
		if (dictionary.ContainsKey("Hands"))
		{
			foreach (BodyPart item5 in dictionary["Hands"])
			{
				BodyPart bodyPart2 = (array[2, num12] = item5);
				num12++;
				num13++;
			}
		}
		int num14 = num6;
		int num15 = num6;
		if (dictionary.ContainsKey("Arm"))
		{
			List<BodyPart> list3 = dictionary["Arm"];
			for (int n = 0; n < list3.Count; n++)
			{
				int num16;
				int num17;
				int num18;
				if (((uint)list3[n].Laterality & (true ? 1u : 0u)) != 0)
				{
					num16 = 1;
					num17 = 0;
					num18 = num14;
					num14++;
				}
				else
				{
					num16 = 3;
					num17 = 4;
					num18 = num15;
					num15++;
				}
				array[num16, num18] = list3[n];
				if (list3[n].Parts != null && list3[n].Parts.Count > 0)
				{
					array[num17, num18] = list3[n].Parts[0];
				}
			}
		}
		if (dictionary.ContainsKey("Hand"))
		{
			List<BodyPart> list4 = dictionary["Hand"];
			for (int num19 = 0; num19 < list4.Count; num19++)
			{
				bool flag5 = false;
				for (int num20 = 0; num20 < 5; num20++)
				{
					if (flag5)
					{
						break;
					}
					for (int num21 = 0; num21 < 20; num21++)
					{
						if (flag5)
						{
							break;
						}
						if (array[num20, num21] == list4[num19])
						{
							flag5 = true;
						}
					}
				}
				if (!flag5)
				{
					int num22;
					int num23;
					if (((uint)list4[num19].Laterality & (true ? 1u : 0u)) != 0)
					{
						num22 = 1;
						num23 = num14;
						num14++;
					}
					else
					{
						num22 = 3;
						num23 = num15;
						num15++;
					}
					array[num22, num23] = list4[num19];
				}
			}
		}
		if (dictionary.ContainsKey("Leg"))
		{
			List<BodyPart> list5 = dictionary["Leg"];
			for (int num24 = 0; num24 < list5.Count; num24++)
			{
				int num25;
				int num26;
				int num27;
				if (((uint)list5[num24].Laterality & (true ? 1u : 0u)) != 0)
				{
					num25 = 1;
					num26 = 0;
					num27 = num14;
					num14++;
				}
				else
				{
					num25 = 3;
					num26 = 4;
					num27 = num15;
					num15++;
				}
				array[num25, num27] = list5[num24];
				if (list5[num24].Parts != null && list5[num24].Parts.Count > 0)
				{
					array[num26, num27] = list5[num24].Parts[0];
				}
			}
		}
		num6 = Math.Max(num12, Math.Max(num14, num15));
		if (dictionary.ContainsKey("Feet"))
		{
			int num28 = Math.Max(0, 3 - dictionary["Feet"].Count);
			foreach (BodyPart item6 in dictionary["Feet"])
			{
				BodyPart bodyPart3 = (array[num28, num6] = item6);
				num28++;
				if (num28 >= 5)
				{
					num28 = 0;
					num6++;
				}
			}
			num6++;
		}
		if (dictionary.ContainsKey("Roots"))
		{
			int num29 = Math.Max(0, 3 - dictionary["Roots"].Count);
			foreach (BodyPart item7 in dictionary["Roots"])
			{
				BodyPart bodyPart4 = (array[num29, num6] = item7);
				num29++;
				if (num29 >= 5)
				{
					num29 = 0;
					num6++;
				}
			}
			num6++;
		}
		if (dictionary.ContainsKey("Tread"))
		{
			int num30 = Math.Max(0, 3 - dictionary["Tread"].Count);
			foreach (BodyPart item8 in dictionary["Tread"])
			{
				BodyPart bodyPart5 = (array[num30, num6] = item8);
				num30++;
				if (num30 >= 5)
				{
					num30 = 0;
					num6++;
				}
			}
			num6++;
		}
		if (dictionary.ContainsKey("Foot"))
		{
			int num31 = Math.Max(0, 3 - dictionary["Foot"].Count);
			foreach (BodyPart item9 in dictionary["Foot"])
			{
				BodyPart bodyPart6 = (array[num31, num6] = item9);
				num31++;
				if (num31 >= 5)
				{
					num31 = 0;
					num6++;
				}
			}
			num6++;
		}
		if (dictionary.ContainsKey("Tail"))
		{
			int num32 = 3 - dictionary["Tail"].Count;
			foreach (BodyPart item10 in dictionary["Tail"])
			{
				BodyPart bodyPart7 = (array[num32, num6] = item10);
				num32++;
			}
			num6++;
		}
		num7 = num6;
		if (dictionary.ContainsKey("Missile Weapon"))
		{
			int num33 = num6;
			for (int num34 = 0; num34 < dictionary["Missile Weapon"].Count; num34++)
			{
				if ((num34 + 1) % 3 == 0)
				{
					num33++;
					num7 = Math.Max(num7, num33);
				}
				array[num34, num33] = dictionary["Missile Weapon"][num34];
			}
		}
		if (dictionary.ContainsKey("Floating Nearby"))
		{
			int num35 = num6;
			for (int num36 = 0; num36 < dictionary["Floating Nearby"].Count; num36++)
			{
				if (num36 > 0)
				{
					num35++;
					num7 = Math.Max(num7, num35);
				}
				array[3, num35] = dictionary["Floating Nearby"][num36];
			}
		}
		if (dictionary.ContainsKey("Thrown Weapon"))
		{
			int num37 = num6;
			for (int num38 = 0; num38 < dictionary["Thrown Weapon"].Count; num38++)
			{
				if (num38 > 0)
				{
					num37++;
					num7 = Math.Max(num7, num37);
				}
				array[4, num37] = dictionary["Thrown Weapon"][num38];
			}
		}
		num6 = num7 + 1;
		num3 = 0;
		for (int num39 = 0; num39 < num6; num39++)
		{
			for (int num40 = 0; num40 < 5; num40++)
			{
				BodyPart bodyPart8 = array[num40, num39];
				if (bodyPart8 == null)
				{
					UnityEngine.GameObject gameObject3 = new UnityEngine.GameObject();
					gameObject3.AddComponent<RectTransform>();
					gameObject3.transform.SetParent(childGameObject2.transform);
					gameObject3.transform.SetSiblingIndex(num3);
					EquipmentButtons.Add(gameObject3);
				}
				else
				{
					UnityEngine.GameObject gameObject4 = PooledPrefabManager.Instantiate("BodyLine");
					gameObject4.transform.Find("PartName").GetComponent<UnityEngine.UI.Text>().text = Sidebar.FormatToRTF(bodyPart8.Primary ? (bodyPart8.GetCardinalDescription() + "&y(&G*&y)") : bodyPart8.GetCardinalDescription());
					QudItemListElement qudItemListElement = new QudItemListElement();
					bool flag6 = false;
					if (bodyPart8.Equipped != null)
					{
						qudItemListElement.InitFrom(bodyPart8.Equipped);
					}
					else if (bodyPart8.DefaultBehavior != null)
					{
						qudItemListElement.InitFrom(bodyPart8.DefaultBehavior);
						flag6 = true;
					}
					else
					{
						qudItemListElement.InitFrom(null);
					}
					gameObject4.transform.Find("EquippedItem").SendMessage("SetMode", "Equipment");
					gameObject4.transform.Find("EquippedItem").SendMessage("SetDefault", flag6);
					gameObject4.transform.Find("EquippedItem").SendMessage("SetBodyPart", bodyPart8);
					gameObject4.transform.Find("EquippedItem").SendMessage("SetGameObject", qudItemListElement);
					gameObject4.transform.Find("EquippedItem/Text").GetComponent<UnityEngine.UI.Text>().text = qudItemListElement.displayName;
					EquipmentButtons.Add(gameObject4);
					gameObject4.transform.SetParent(childGameObject2.transform);
					gameObject4.transform.SetSiblingIndex(num3);
					gameObject4.transform.localScale = new Vector3(1f, 1f, 1f);
				}
				num3++;
			}
		}
		if (text != null && InventoryViewBehaviour.instance.filterBar.categoryButtons.ContainsKey(text) && text != "*")
		{
			InventoryViewBehaviour.instance.filterBar.categoryButtons[text].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		else if (text == null || !InventoryViewBehaviour.instance.filterBar.categoryButtons.ContainsKey(text))
		{
			InventoryViewBehaviour.instance.filterBar.categoryButtons["*"].GetComponent<InventoryFilterBarButton>().Clicked();
		}
		foreach (KeyValuePair<string, bool> item11 in BaseInventoryView.toggleState)
		{
			if (toggles.ContainsKey(item11.Key) && toggles[item11.Key].toggled != item11.Value)
			{
				toggles[item11.Key].Toggle();
			}
		}
		if (num >= 0)
		{
			bool flag7 = false;
			for (int num41 = num; num41 < InventoryButtons.Count; num41++)
			{
				if (num41 < InventoryButtons.Count && InventoryButtons[num41].gameObject.activeInHierarchy)
				{
					Select(InventoryButtons[num41]);
					flag7 = true;
					break;
				}
			}
			if (!flag7)
			{
				for (int num42 = num; num42 >= 0; num42--)
				{
					if (num42 < InventoryButtons.Count && InventoryButtons[num42].gameObject.activeInHierarchy)
					{
						Select(InventoryButtons[num42]);
						flag7 = true;
						break;
					}
				}
			}
		}
		else if (num2 >= 0 && num2 < EquipmentButtons.Count)
		{
			Select(EquipmentButtons[num2].transform.Find("EquippedItem").gameObject);
			InventoryViewBehaviour.instance.inventoryScroll.verticalNormalizedPosition = lastScrollPosition;
		}
		filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = false;
		filterBar.transform.parent.transform.parent.transform.parent.GetComponent<CanvasGroup>().interactable = true;
		Canvas.ForceUpdateCanvases();
		Canvas.ForceUpdateCanvases();
	}

	public void QueueInventoryUpdate()
	{
		GameManager.Instance.gameQueue.queueTask(delegate
		{
			QudItemList newList = ObjectPool<QudItemList>.Checkout();
			newList.Add(XRLCore.Core.Game.Player.Body.GetPart<Inventory>().Objects.Where((XRL.World.GameObject o) => !o.HasTag("HiddenInInventory")));
			newList.eqWeight = XRLCore.Core.Game.Player.Body.GetPart<Body>().GetWeight();
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				UpdateObjectList(newList);
			});
		});
	}

	public override void Enter()
	{
		base.Enter();
		QueueInventoryUpdate();
	}

	public override void Clear()
	{
		base.Clear();
		for (int i = 0; i < EquipmentButtons.Count; i++)
		{
			PooledPrefabManager.Return(EquipmentButtons[i]);
		}
		EquipmentButtons.Clear();
		filterBar.Clear();
	}

	public override void Overlapped()
	{
		previousSelectedItem = base.EventSystemManager.currentSelectedGameObject;
	}

	public override void OnLeave()
	{
		previousSelectedItem = null;
		Clear();
		filterBar.toggledButton = null;
		if (currentList != null)
		{
			currentList.Clear();
			ObjectPool<QudItemList>.Return(currentList);
			currentList = null;
		}
	}

	public static bool EnsureVisible(ScrollRect scrollRect, UnityEngine.GameObject child)
	{
		Rect target = child.GetComponent<RectTransform>().RectRelativeTo(scrollRect.GetComponent<RectTransform>());
		if (scrollRect.GetComponent<RectTransform>().rect.Contains(target))
		{
			return true;
		}
		float num = scrollRect.GetComponent<RectTransform>().anchorMin.y - child.GetComponent<RectTransform>().anchoredPosition.y;
		num += (float)child.GetComponent<RectTransform>().transform.GetSiblingIndex() / (float)scrollRect.content.transform.childCount;
		num /= 1000f;
		num = (scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1f - num));
		return false;
	}

	public override void Update()
	{
		lastScrollPosition = InventoryViewBehaviour.instance.inventoryScroll.verticalNormalizedPosition;
		base.Update();
		if (Input.GetKeyDown(UnityEngine.KeyCode.D) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			UnityEngine.GameObject currentSelectedGameObject = base.EventSystemManager.currentSelectedGameObject;
			if (currentSelectedGameObject != null && InventoryButtons.Contains(currentSelectedGameObject))
			{
				ItemLineManager component = currentSelectedGameObject.GetComponent<ItemLineManager>();
				if (component != null && component.go != null)
				{
					XRL.World.GameObject goToDrop = component.go;
					GameManager.Instance.gameQueue.queueTask(delegate
					{
						EquipmentAPI.DropObject(goToDrop);
						GameManager.Instance.uiQueue.queueSingletonTask("InventoryUpdate", delegate
						{
							instance.QueueInventoryUpdate();
						});
					});
				}
			}
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.A) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			UnityEngine.GameObject currentSelectedGameObject2 = base.EventSystemManager.currentSelectedGameObject;
			if (currentSelectedGameObject2 != null && InventoryButtons.Contains(currentSelectedGameObject2))
			{
				ItemLineManager component2 = currentSelectedGameObject2.GetComponent<ItemLineManager>();
				if (component2 != null && component2.go != null)
				{
					XRL.World.GameObject goToEat = component2.go;
					GameManager.Instance.gameQueue.queueTask(delegate
					{
						goToEat.HandleEvent(InventoryActionEvent.FromPool(goToEat.InInventory, goToEat, "Eat"));
						GameManager.Instance.uiQueue.queueSingletonTask("InventoryUpdate", delegate
						{
							instance.QueueInventoryUpdate();
						});
					});
				}
			}
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.R) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			UnityEngine.GameObject currentSelectedGameObject3 = base.EventSystemManager.currentSelectedGameObject;
			if (currentSelectedGameObject3 != null && InventoryButtons.Contains(currentSelectedGameObject3))
			{
				ItemLineManager component3 = currentSelectedGameObject3.GetComponent<ItemLineManager>();
				if (component3 != null && component3.go != null)
				{
					XRL.World.GameObject goToEat2 = component3.go;
					GameManager.Instance.gameQueue.queueTask(delegate
					{
						goToEat2.HandleEvent(InventoryActionEvent.FromPool(goToEat2.InInventory, goToEat2, "Drink"));
						GameManager.Instance.uiQueue.queueSingletonTask("InventoryUpdate", delegate
						{
							instance.QueueInventoryUpdate();
						});
					});
				}
			}
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.P) && (Input.GetKey(UnityEngine.KeyCode.LeftControl) || Input.GetKey(UnityEngine.KeyCode.RightControl)))
		{
			UnityEngine.GameObject currentSelectedGameObject4 = base.EventSystemManager.currentSelectedGameObject;
			if (currentSelectedGameObject4 != null && InventoryButtons.Contains(currentSelectedGameObject4))
			{
				ItemLineManager component4 = currentSelectedGameObject4.GetComponent<ItemLineManager>();
				if (component4 != null && component4.go != null)
				{
					XRL.World.GameObject goToEat3 = component4.go;
					GameManager.Instance.gameQueue.queueTask(delegate
					{
						goToEat3.HandleEvent(InventoryActionEvent.FromPool(goToEat3.InInventory, goToEat3, "Apply"));
						GameManager.Instance.uiQueue.queueSingletonTask("InventoryUpdate", delegate
						{
							instance.QueueInventoryUpdate();
						});
					});
				}
			}
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.KeypadMinus))
		{
			foreach (ObjectToggler value in toggles.Values)
			{
				if (value.toggled)
				{
					value.Toggle();
				}
			}
			SelectNearestInventoryToCurrent();
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.KeypadPlus))
		{
			foreach (ObjectToggler value2 in toggles.Values)
			{
				if (!value2.toggled)
				{
					value2.Toggle();
				}
			}
			SelectNearestInventoryToCurrent();
		}
		int num = 0;
		if (Input.GetKeyDown(UnityEngine.KeyCode.PageDown))
		{
			num = 8;
		}
		if (Input.GetKeyDown(UnityEngine.KeyCode.PageUp))
		{
			num = -8;
		}
		if (num == 0 || !(base.EventSystemManager.currentSelectedGameObject != null) || !InventoryButtons.Contains(base.EventSystemManager.currentSelectedGameObject))
		{
			return;
		}
		int num2 = InventoryButtons.IndexOf(base.EventSystemManager.currentSelectedGameObject) + num;
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 >= InventoryButtons.Count)
		{
			num2 = InventoryButtons.Count - 1;
		}
		if (InventoryButtons[num2].activeInHierarchy)
		{
			Select(InventoryButtons[num2]);
			return;
		}
		for (int i = 0; i < InventoryButtons.Count; i++)
		{
			if (num2 + i < InventoryButtons.Count && InventoryButtons[num2 + i].activeInHierarchy)
			{
				Select(InventoryButtons[num2 + i]);
				break;
			}
			if (num2 - i > 0 && InventoryButtons[num2 - i].activeInHierarchy)
			{
				Select(InventoryButtons[num2 - i]);
				break;
			}
		}
	}

	public override void OnCommand(string Command)
	{
		if (Command.StartsWith("Prev"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad7, '7'));
		}
		if (Command.StartsWith("Next"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Keypad9, '9'));
		}
		if (Command.StartsWith("Back"))
		{
			Keyboard.PushKey(new Keyboard.XRLKeyEvent(UnityEngine.KeyCode.Escape));
		}
		base.OnCommand(Command);
	}
}
