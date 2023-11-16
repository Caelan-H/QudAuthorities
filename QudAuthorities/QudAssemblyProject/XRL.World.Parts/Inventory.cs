using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Inventory : IPart
{
	public bool DropOnDeath = true;

	[NonSerialized]
	public List<GameObject> Objects = new List<GameObject>();

	[NonSerialized]
	private static Event eCommandRemoveObject = new Event("CommandRemoveObject", "Object", (object)null, "ForEquip", 0);

	[NonSerialized]
	private static Event eCommandFreeTakeObject = new Event("CommandTakeObject", "Object", null, "Context", null, "EnergyCost", 0);

	private int WeightCache = -1;

	[NonSerialized]
	private static List<GameObject> TurnTickObjects = new List<GameObject>(8);

	[NonSerialized]
	private static bool TurnTickObjectsInUse = false;

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.WriteGameObjectList(Objects);
		base.SaveData(Writer);
	}

	public int Count(string blueprint)
	{
		int num = 0;
		for (int num2 = Objects.Count - 1; num2 >= 0; num2--)
		{
			if (Objects[num2].Blueprint == blueprint)
			{
				num += Objects[num2].Count;
			}
		}
		return num;
	}

	public override void LoadData(SerializationReader Reader)
	{
		Reader.ReadGameObjectList(Objects);
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			if (Objects[num] == null)
			{
				Objects.RemoveAt(num);
			}
		}
		base.LoadData(Reader);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void Validate()
	{
		List<GameObject> list = null;
		if (Objects != null)
		{
			for (int num = Objects.Count - 1; num >= 0; num--)
			{
				if (!Objects[num].IsValid())
				{
					if (list == null)
					{
						list = new List<GameObject>(1) { Objects[num] };
					}
					else
					{
						list.Add(Objects[num]);
					}
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (GameObject item in list)
		{
			Objects.Remove(item);
		}
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
	}

	public void AddObject(List<GameObject> GOs)
	{
		foreach (GameObject GO in GOs)
		{
			AddObject(GO, Silent: false, NoStack: false, FlushTickCache: false);
		}
		FlushWantTurnTickCache();
	}

	public GameObject AddObjectNoStack(GameObject GO)
	{
		return AddObject(GO, Silent: false, NoStack: true);
	}

	public GameObject AddObject(string blueprint, bool bSilent = false)
	{
		return AddObject(GameObject.create(blueprint), bSilent);
	}

	public GameObject AddObject(GameObject GO, bool Silent = false, bool NoStack = false, bool FlushTickCache = true, IEvent ParentEvent = null)
	{
		bool num = Objects.Count == 0;
		Objects.Add(GO);
		Cell cell = GO.CurrentCell;
		GO.pPhysics.InInventory = ParentObject;
		if (cell != null)
		{
			cell.RemoveObject(GO);
			Cell cell2 = GO.CurrentCell;
			if (cell2 != null && cell2.Objects.Contains(GO))
			{
				cell2.Objects.Remove(GO);
			}
			GO.pPhysics.CurrentCell = null;
		}
		FlushWeightCache();
		if (FlushTickCache)
		{
			FlushWantTurnTickCache();
		}
		if (num)
		{
			CheckNonEmptyState();
		}
		AddedToInventoryEvent.Send(ParentObject, GO, Silent, NoStack, ParentEvent);
		ParentObject.FireEvent("EncumbranceChanged");
		return GO;
	}

	public void RemoveObject(GameObject GO, IEvent ParentEvent = null)
	{
		Objects.Remove(GO);
		GO.pPhysics.InInventory = null;
		FlushWeightCache();
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		ParentObject.FireEvent("EncumbranceChanged");
	}

	public void Clear()
	{
		Objects.Clear();
		FlushWeightCache();
		ParentObject.FireEvent("EncumbranceChanged");
	}

	[Obsolete("version with MapInv argument should always be called")]
	public override IPart DeepCopy(GameObject Parent)
	{
		return base.DeepCopy(Parent);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Inventory inventory = (Inventory)base.DeepCopy(Parent, MapInv);
		inventory.Objects = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			GameObject gameObject = MapInv?.Invoke(Objects[i]) ?? Objects[i].DeepCopy(CopyEffects: false, CopyID: false, MapInv);
			if (gameObject != null)
			{
				gameObject.pPhysics.InInventory = Parent;
				inventory.Objects.Add(gameObject);
			}
		}
		return inventory;
	}

	public override void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyLate(Source, CopyEffects, CopyID, MapInv);
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
	}

	public int GetWeight()
	{
		if (WeightCache != -1)
		{
			return WeightCache;
		}
		return RecalculateWeight();
	}

	public void FlushWeightCache()
	{
		WeightCache = -1;
		GameObject inInventory = ParentObject.InInventory;
		if (inInventory != null)
		{
			Inventory inventory = inInventory.Inventory;
			if (inventory != null && inventory.WeightCache != -1)
			{
				inventory.FlushWeightCache();
			}
		}
	}

	private int RecalculateWeight()
	{
		WeightCache = 0;
		for (int num = Objects.Count - 1; num >= 0; num--)
		{
			WeightCache += Objects[num].Weight;
		}
		return WeightCache;
	}

	public GameObject FindObject(Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectByBlueprint(string Blueprint)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint)
			{
				return Objects[i];
			}
		}
		return null;
	}

	public GameObject FindObjectByBlueprint(string Blueprint, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].Blueprint == Blueprint && pFilter(Objects[i]))
			{
				return Objects[i];
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsDirect()
	{
		FlushWeightCache();
		return Objects;
	}

	public List<GameObject> GetObjectsDirect(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjects()
	{
		return GetObjectsDirect((GameObject obj) => !obj.HasTag("HiddenInInventory"));
	}

	public void GetObjectsDirect(List<GameObject> store)
	{
		store.AddRange(Objects);
	}

	public void GetObjects(List<GameObject> store)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				store.Add(Objects[i]);
			}
		}
	}

	public void GetObjects(List<GameObject> store, Predicate<GameObject> pFilter)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (pFilter(Objects[i]) && !Objects[i].HasTag("HiddenInInventory"))
			{
				store.Add(Objects[i]);
			}
		}
	}

	public List<GameObject> GetObjects(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(Objects.Count);
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
			{
				list.Add(Objects[i]);
			}
		}
		return list;
	}

	public List<GameObject> GetObjectsWithTag(string Name)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasTag(Name) && !Objects[i].HasTag("HiddenInInventory"))
			{
				num++;
			}
		}
		List<GameObject> list = new List<GameObject>(num);
		int j = 0;
		for (int count2 = Objects.Count; j < count2; j++)
		{
			if (Objects[j].HasTag(Name) && !Objects[j].HasTag("HiddenInInventory"))
			{
				list.Add(Objects[j]);
			}
		}
		return list;
	}

	public GameObject GetFirstObject()
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObject(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public GameObject GetFirstObjectDirect()
	{
		if (Objects == null || Objects.Count <= 0)
		{
			return null;
		}
		return Objects[0];
	}

	public GameObject GetFirstObjectDirect(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (pFilter(Objects[i]))
				{
					return Objects[i];
				}
			}
		}
		return null;
	}

	public int GetObjectCountDirect()
	{
		if (Objects == null)
		{
			return 0;
		}
		return Objects.Count;
	}

	public int GetObjectCountDirect(Predicate<GameObject> pFilter)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (pFilter(Objects[i]))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetObjectCount()
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetObjectCount(Predicate<GameObject> pFilter)
	{
		int num = 0;
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasObject()
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObject(GameObject obj)
	{
		if (Objects.Contains(obj))
		{
			return !obj.HasTag("HiddenInInventory");
		}
		return false;
	}

	public bool HasObject(string Blueprint)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].Blueprint == Blueprint && !Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObject(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectDirect(GameObject obj)
	{
		return Objects.Contains(obj);
	}

	public bool HasObjectDirect(string Blueprint)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].Blueprint == Blueprint && !Objects[i].HasTag("HiddenInInventory"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectDirect(Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (pFilter(Objects[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachObject(Action<GameObject> aProc)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && pFilter(Objects[i]) && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc)
	{
		if (Objects.Count == 0)
		{
			return;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory"))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject))
				{
					aProc(gameObject);
				}
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory"))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject) && !pProc(gameObject))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachObject(Action<GameObject> aProc, Predicate<GameObject> pFilter)
	{
		if (Objects.Count == 0)
		{
			return;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory") && pFilter(gameObject))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject))
				{
					aProc(gameObject);
				}
			}
		}
	}

	public bool SafeForeachObject(Predicate<GameObject> pProc, Predicate<GameObject> pFilter)
	{
		if (Objects.Count == 0)
		{
			return true;
		}
		List<GameObject> list = null;
		for (int i = 0; i < (list?.Count ?? Objects.Count); i++)
		{
			GameObject gameObject = ((list == null) ? Objects[i] : list[i]);
			if (!gameObject.HasTag("HiddenInInventory") && pFilter(gameObject))
			{
				if (list == null)
				{
					list = new List<GameObject>(Objects);
				}
				if (Objects.Contains(gameObject) && !pProc(gameObject))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachObjectWithRegisteredEvent(string EventName, Action<GameObject> aProc)
	{
		if (Objects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory") && Objects[i].HasRegisteredEvent(EventName))
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObjectWithRegisteredEvent(string EventName, Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!Objects[i].HasTag("HiddenInInventory") && Objects[i].HasRegisteredEvent(EventName) && !pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ForeachObjectDirect(Action<GameObject> aProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				aProc(Objects[i]);
			}
		}
	}

	public bool ForeachObjectDirect(Predicate<GameObject> pProc)
	{
		if (Objects != null)
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (!pProc(Objects[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsItemSlotAppropriate(GameObject go, string SlotType)
	{
		if (go.HasTagOrProperty("CannotEquip") || go.HasTagOrProperty("NoEquip"))
		{
			return false;
		}
		QueryEquippableListEvent queryEquippableListEvent = QueryEquippableListEvent.FromPool(IComponent<GameObject>.ThePlayer, go, SlotType);
		go.HandleEvent(queryEquippableListEvent);
		return queryEquippableListEvent.List.Contains(go);
	}

	private QueryEquippableListEvent GetEquipmentListEvent(string SlotType)
	{
		QueryEquippableListEvent queryEquippableListEvent = null;
		foreach (GameObject @object in Objects)
		{
			if (@object.WantEvent(QueryEquippableListEvent.ID, MinEvent.CascadeLevel) && !@object.HasTagOrProperty("CannotEquip") && !@object.HasTagOrProperty("NoEquip"))
			{
				if (queryEquippableListEvent == null)
				{
					queryEquippableListEvent = QueryEquippableListEvent.FromPool(ParentObject, @object, SlotType);
				}
				if (ParentObject.IsPlayer() || !@object.HasPropertyOrTag("NoAIEquip"))
				{
					@object.HandleEvent(queryEquippableListEvent);
				}
			}
		}
		return queryEquippableListEvent;
	}

	public void GetEquipmentListForSlot(List<GameObject> Return, string SlotType)
	{
		QueryEquippableListEvent equipmentListEvent = GetEquipmentListEvent(SlotType);
		if (equipmentListEvent != null)
		{
			Return.AddRange(equipmentListEvent.List);
			if (Return.Count > 1)
			{
				Return.Sort(new Brain.GearSorter(ParentObject));
			}
		}
	}

	public List<GameObject> GetEquipmentListForSlot(string SlotType)
	{
		QueryEquippableListEvent equipmentListEvent = GetEquipmentListEvent(SlotType);
		if (equipmentListEvent == null)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>(equipmentListEvent.List);
		if (list.Count > 1)
		{
			list.Sort(new Brain.GearSorter(ParentObject));
		}
		return list;
	}

	public void CheckEmptyState()
	{
		if (ParentObject?.pRender != null)
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("EmptyTile");
			if (propertyOrTag != null && ParentObject.pRender != null)
			{
				ParentObject.pRender.Tile = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
			propertyOrTag = ParentObject.GetPropertyOrTag("EmptyDetailColor");
			if (propertyOrTag != null && ParentObject.pRender != null)
			{
				ParentObject.pRender.DetailColor = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
		}
	}

	public void CheckNonEmptyState()
	{
		if (ParentObject?.pRender != null)
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("FullTile");
			if (propertyOrTag != null)
			{
				ParentObject.pRender.Tile = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
			propertyOrTag = ParentObject.GetPropertyOrTag("FullDetailColor");
			if (propertyOrTag != null)
			{
				ParentObject.pRender.DetailColor = propertyOrTag.CachedCommaExpansion().GetRandomElement();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		Object.RegisterPartEvent(this, "CommandDropObject");
		Object.RegisterPartEvent(this, "CommandEquipObject");
		Object.RegisterPartEvent(this, "CommandExamineObject");
		Object.RegisterPartEvent(this, "CommandForceEquipObject");
		Object.RegisterPartEvent(this, "CommandForceUnequipObject");
		Object.RegisterPartEvent(this, "CommandGet");
		Object.RegisterPartEvent(this, "CommandGetFrom");
		Object.RegisterPartEvent(this, "CommandRemoveObject");
		Object.RegisterPartEvent(this, "CommandTakeObject");
		Object.RegisterPartEvent(this, "CommandUnequipObject");
		Object.RegisterPartEvent(this, "HasBlueprint");
		Object.RegisterPartEvent(this, "PerformDrop");
		Object.RegisterPartEvent(this, "PerformEquip");
		Object.RegisterPartEvent(this, "PerformTake");
		Object.RegisterPartEvent(this, "PerformUnequip");
		base.Register(Object);
	}

	public bool CheckOverburdened()
	{
		if (IsOverburdened())
		{
			if (!ParentObject.HasEffectByClass("Overburdened"))
			{
				ParentObject.ApplyEffect(new Overburdened());
			}
			return true;
		}
		ParentObject.RemoveEffectByExactClass("Overburdened");
		return false;
	}

	public static List<GameObject> GetInteractableObjects(GameObject who, Cell CC, Cell C, bool DoInteractNearby)
	{
		List<GameObject> list = ((C == CC || !C.IsSolidFor(who)) ? C.GetObjectsInCell() : C.GetSolidObjectsFor(who));
		if (list.Count > 1)
		{
			list.Sort(new SortGORenderLayer());
		}
		List<GameObject> list2 = Event.NewGameObjectList();
		int num = (Options.DebugInternals ? (-1) : 0);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject != who && (gameObject.pRender == null || (gameObject.pRender.Visible && gameObject.pRender.RenderLayer > num)) && (gameObject.IsTakeable() || (DoInteractNearby && EquipmentAPI.CanBeTwiddled(gameObject, who))))
			{
				list2.Add(gameObject);
			}
		}
		return list2;
	}

	public bool InventoryWantsEndTurnEvent()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].HasRegisteredEvent("EndTurn"))
			{
				return true;
			}
		}
		return false;
	}

	public bool WantsEndTurnEvent()
	{
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (InventoryWantsEndTurnEvent())
			{
				List<GameObject> list = Event.NewGameObjectList();
				List<GameObject> list2 = Event.NewGameObjectList();
				list.AddRange(Objects);
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					if (list[i] != null)
					{
						list[i].FireEvent(E);
						if (list[i].CleanEffects())
						{
							list2.Add(list[i]);
						}
					}
				}
				list.Clear();
				if (list2.Count > 0)
				{
					int j = 0;
					for (int count2 = list2.Count; j < count2; j++)
					{
						list2[j].CheckStack();
					}
					list2.Clear();
				}
			}
			CheckOverburdened();
		}
		else if (E.ID == "AIGetOffensiveItemList" || E.ID == "AIGetDefensiveItemList")
		{
			int k = 0;
			for (int count3 = Objects.Count; k < count3; k++)
			{
				Objects[k].FireEvent(E);
			}
		}
		else
		{
			if (E.ID == "HasBlueprint")
			{
				string stringParameter = E.GetStringParameter("Blueprint");
				int l = 0;
				for (int count4 = Objects.Count; l < count4; l++)
				{
					if (Objects[l].Blueprint == stringParameter)
					{
						return true;
					}
				}
				return false;
			}
			if (E.ID == "CommandGet" || E.ID == "CommandGetFrom")
			{
				bool DoInteractNearby = E.ID == "CommandGetFrom";
				bool flag = !DoInteractNearby;
				if (flag && E.HasParameter("GetOne"))
				{
					flag = false;
				}
				if (flag && Options.AskForOneItem)
				{
					flag = false;
				}
				XRLCore.Core.RenderBaseToBuffer(Popup._ScreenBuffer);
				Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer);
				GameObject Opener = ParentObject;
				Cell CC = Opener.CurrentCell;
				Cell C = E.GetParameter("TargetCell") as Cell;
				if (C == null)
				{
					if (DoInteractNearby && ParentObject.IsPlayer())
					{
						string text = XRL.UI.PickDirection.ShowPicker();
						if (text == null)
						{
							return true;
						}
						C = CC.GetCellFromDirection(text);
					}
					else
					{
						C = CC;
					}
					if (C == null)
					{
						return true;
					}
				}
				List<GameObject> interactableObjects = GetInteractableObjects(Opener, CC, C, DoInteractNearby);
				if (interactableObjects.Count == 1 && flag)
				{
					GameObject gameObject = interactableObjects[0];
					Event @event = Event.New("CommandTakeObject");
					@event.SetParameter("Object", gameObject);
					@event.SetParameter("Context", E.GetStringParameter("Context"));
					@event.SetSilent(Silent: false);
					if (Opener.FireEvent(@event))
					{
						C.RemoveObject(gameObject);
					}
				}
				else if (interactableObjects.Count > 0)
				{
					bool RequestInterfaceExit = false;
					PickItem.ShowPicker(interactableObjects, ref RequestInterfaceExit, null, PickItem.PickItemDialogStyle.GetItemDialog, Opener, null, C, null, PreserveOrder: false, () => GetInteractableObjects(Opener, CC, C, DoInteractNearby));
					if (RequestInterfaceExit)
					{
						E.RequestInterfaceExit();
					}
				}
				else if (Opener.IsPlayer())
				{
					if (DoInteractNearby)
					{
						Popup.ShowFail("There's nothing you can interact with there.");
					}
					else
					{
						Popup.ShowFail("There's nothing to take.");
					}
				}
			}
			else if (E.ID == "CommandExamineObject")
			{
				E.GetGameObjectParameter("Object").FireEvent(E);
			}
			else if (E.ID == "PerformEquip")
			{
				GameObject gameObject2 = E.GetGameObjectParameter("Object");
				BodyPart bodyPart = E.GetParameter("BodyPart") as BodyPart;
				bool flag2 = E.IsSilent();
				int intParameter = E.GetIntParameter("AutoEquipTry");
				string stringParameter2 = E.GetStringParameter("FailureMessage");
				List<GameObject> list3 = E.GetParameter("WasUnequipped") as List<GameObject>;
				int intParameter2 = E.GetIntParameter("DestroyOnUnequipDeclined");
				if (bodyPart == null)
				{
					return false;
				}
				if (gameObject2.Count > 1)
				{
					gameObject2 = gameObject2.RemoveOne();
				}
				GameObject obj = bodyPart.Equipped;
				if (obj != null)
				{
					Event event2 = Event.New("CommandUnequipObject", "BodyPart", bodyPart);
					if (flag2)
					{
						event2.SetSilent(Silent: true);
					}
					if (intParameter > 0)
					{
						event2.SetParameter("AutoEquipTry", intParameter);
					}
					if (!string.IsNullOrEmpty(stringParameter2))
					{
						event2.SetParameter("FailureMessage", stringParameter2);
					}
					if (intParameter2 > 0)
					{
						event2.SetParameter("DestroyOnUnequipDeclined", intParameter2);
					}
					if (!ParentObject.FireEvent(event2))
					{
						string stringParameter3 = event2.GetStringParameter("FailureMessage");
						if (!string.IsNullOrEmpty(stringParameter3) && stringParameter3 != stringParameter2)
						{
							stringParameter2 = stringParameter3;
							E.SetParameter("FailureMessage", stringParameter2);
						}
						int intParameter3 = event2.GetIntParameter("DestroyOnUnequipDeclined");
						if (intParameter3 > intParameter2)
						{
							intParameter2 = intParameter3;
							E.SetParameter("DestroyOnUnequipDeclined", intParameter2);
						}
						return false;
					}
					if (list3 != null && GameObject.validate(ref obj) && !list3.Contains(obj))
					{
						list3.Add(obj);
					}
				}
				_ = gameObject2.InInventory;
				if (!flag2)
				{
					DidXToY("equip", gameObject2, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: true);
				}
				string FailureMessage = null;
				if (!bodyPart.DoEquip(gameObject2, ref FailureMessage, flag2, ForDeepCopy: false, UnequipOthers: true, intParameter, list3))
				{
					if (!string.IsNullOrEmpty(FailureMessage) && string.IsNullOrEmpty(stringParameter2))
					{
						stringParameter2 = FailureMessage;
						E.SetParameter("FailureMessage", stringParameter2);
					}
					return false;
				}
				EquippedEvent.Send(ParentObject, gameObject2, bodyPart);
				EquipperEquippedEvent.Send(ParentObject, gameObject2, bodyPart);
			}
			else if (E.ID == "PerformUnequip")
			{
				if (!(E.GetParameter("BodyPart") is BodyPart bodyPart2))
				{
					return false;
				}
				if (bodyPart2.Equipped == null)
				{
					return true;
				}
				GameObject equipped = bodyPart2.Equipped;
				bodyPart2.Unequip();
				ParentObject.FireEvent(Event.New("EquipperUnequipped", "Object", equipped));
			}
			else if (E.ID == "PerformTake")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				GameObject gameObjectParameter2 = E.GetGameObjectParameter("Container");
				string text2 = null;
				if (gameObjectParameter2 != null && !gameObjectParameter2.IsCreature && gameObjectParameter2.CurrentCell != null && gameObjectParameter2.CurrentCell != ParentObject.CurrentCell)
				{
					text2 = ((!ParentObject.IsPlayer()) ? ParentObject.DescribeRelativeDirectionToward(gameObjectParameter2) : ParentObject.DescribeDirectionToward(gameObjectParameter2));
				}
				else if (gameObjectParameter2 == null)
				{
					Cell cell = gameObjectParameter.GetCurrentCell();
					Cell cell2 = ParentObject.GetCurrentCell();
					if (cell != null && cell2 != null && cell != cell2)
					{
						string directionFromCell = cell2.GetDirectionFromCell(cell);
						text2 = ((!ParentObject.IsPlayer()) ? ("from " + ParentObject.its + " " + Directions.GetExpandedDirection(directionFromCell)) : ("from the " + Directions.GetExpandedDirection(directionFromCell)));
					}
				}
				if (!Objects.Contains(gameObjectParameter))
				{
					if (ParentObject.IsPlayer() && gameObjectParameter.InInventory != ParentObject && gameObjectParameter.Equipped != ParentObject && gameObjectParameter.IsOwned())
					{
						if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to take it?") != 0)
						{
							return false;
						}
						gameObjectParameter.pPhysics.BroadcastForHelp(ParentObject);
					}
					gameObjectParameter.RemoveFromContext(E);
					FlushWeightCache();
					if (!E.IsSilent())
					{
						GameObject gameObjectParameter3 = E.GetGameObjectParameter("PutBy");
						if (gameObjectParameter3 != null)
						{
							if (ParentObject.CurrentCell != null)
							{
								text2 = ((!gameObjectParameter3.IsPlayer()) ? gameObjectParameter3.DescribeRelativeDirectionToward(ParentObject) : gameObjectParameter3.DescribeDirectionToward(ParentObject));
							}
							IComponent<GameObject>.WDidXToYWithZ(gameObjectParameter3, "put", gameObjectParameter, "in", ParentObject, text2, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: true, indefiniteIndirectObjectForOthers: true);
						}
						else if (gameObjectParameter2 != null)
						{
							DidXToYWithZ("take", gameObjectParameter, "from", gameObjectParameter2, text2);
						}
						else if (text2 != null)
						{
							DidXToY("take", gameObjectParameter, text2);
						}
						else
						{
							DidXToY("take", gameObjectParameter);
						}
					}
					AddObject(gameObjectParameter, Silent: false, NoStack: false, FlushTickCache: true, E);
					if (!gameObjectParameter.IsValid())
					{
						return false;
					}
					if (gameObjectParameter.HasRegisteredEvent("Taken"))
					{
						Event event3 = Event.New("Taken");
						event3.SetParameter("TakingObject", ParentObject);
						event3.SetParameter("Context", E.GetStringParameter("Context"));
						gameObjectParameter.FireEvent(event3, E);
					}
					if (gameObjectParameter.WantEvent(TakenEvent.ID, MinEvent.CascadeLevel))
					{
						gameObjectParameter.HandleEvent(TakenEvent.FromPool(ParentObject, gameObjectParameter, E.GetStringParameter("Context")));
					}
					if (ParentObject.HasRegisteredEvent("Took"))
					{
						ParentObject.FireEvent(Event.New("Took", "Object", gameObjectParameter));
					}
				}
			}
			else if (E.ID == "PerformDrop")
			{
				GameObject gameObjectParameter4 = E.GetGameObjectParameter("Object");
				if (Objects.Contains(gameObjectParameter4))
				{
					RemoveObject(gameObjectParameter4);
					FlushWeightCache();
					if (!E.IsSilent())
					{
						DidXToY("drop", gameObjectParameter4);
					}
					gameObjectParameter4.HandleEvent(DroppedEvent.FromPool(ParentObject, gameObjectParameter4, E.HasFlag("Forced")));
				}
			}
			else if (E.ID == "CommandRemoveObject")
			{
				GameObject gameObjectParameter5 = E.GetGameObjectParameter("Object");
				if (gameObjectParameter5 != null)
				{
					if (!ParentObject.HasRegisteredEvent("BeginDrop") || ParentObject.FireEvent(Event.New("BeginDrop", "Object", gameObjectParameter5, "ForEquip", E.GetIntParameter("ForEquip"))))
					{
						if (!gameObjectParameter5.HasRegisteredEvent("BeginBeingDropped") || gameObjectParameter5.FireEvent(Event.New("BeginBeingDropped", "TakingObject", ParentObject)))
						{
							Event event4 = Event.New("PerformDrop");
							event4.SetParameter("Object", gameObjectParameter5);
							if (E.HasFlag("Forced"))
							{
								event4.SetFlag("Forced", State: true);
							}
							if (E.IsSilent())
							{
								event4.SetSilent(Silent: true);
							}
							return ParentObject.FireEvent(event4);
						}
						return false;
					}
					return false;
				}
			}
			else if (E.ID == "CommandDropObject")
			{
				GameObject gameObjectParameter6 = E.GetGameObjectParameter("Object");
				if (gameObjectParameter6 != null)
				{
					bool flag3 = E.HasFlag("Forced");
					bool flag4 = E.HasFlag("ForEquip");
					if (!flag3 && !flag4 && gameObjectParameter6.HasPart("Stacker"))
					{
						Stacker stacker = gameObjectParameter6.GetPart("Stacker") as Stacker;
						if (stacker.Number > 1 && ParentObject.IsPlayer())
						{
							int? num = Popup.AskNumber("How many do you want to drop? (max=" + stacker.Number + ")", stacker.Number, 0, stacker.Number);
							if (!num.HasValue || num == 0)
							{
								return false;
							}
							int number = stacker.Number;
							try
							{
								number = num.Value;
							}
							catch
							{
								number = stacker.Number;
							}
							if (number <= 0)
							{
								return true;
							}
							if (number >= stacker.Number)
							{
								number = stacker.Number;
							}
							else
							{
								gameObjectParameter6.SplitStack(number, XRLCore.Core.Game.Player.Body);
							}
						}
					}
					if (!ParentObject.HasRegisteredEvent("BeginDrop") || ParentObject.FireEvent(Event.New("BeginDrop", "Object", gameObjectParameter6)) || flag3)
					{
						if (!gameObjectParameter6.HasRegisteredEvent("BeginBeingDropped") || gameObjectParameter6.FireEvent(Event.New("BeginBeingDropped", "TakingObject", ParentObject)) || flag3)
						{
							Event event5 = Event.New("PerformDrop", "Object", gameObjectParameter6);
							if (flag3)
							{
								event5.SetFlag("Forced", State: true);
							}
							event5.bSilent = E.bSilent;
							if (ParentObject.FireEvent(event5) || flag3)
							{
								Cell cell3 = E.GetParameter("Where") as Cell;
								if (cell3 == null)
								{
									cell3 = ParentObject.GetCurrentCell() ?? IComponent<GameObject>.ThePlayer?.GetCurrentCell();
								}
								if (cell3 != null)
								{
									if (ParentObject.IsPlayer())
									{
										gameObjectParameter6.SetIntProperty("DroppedByPlayer", 1);
									}
									cell3.AddObject(gameObjectParameter6);
								}
								return true;
							}
							return false;
						}
						return false;
					}
					return false;
				}
			}
			else if (E.ID == "CommandEquipObject" || E.ID == "CommandForceEquipObject")
			{
				GameObject obj3 = E.GetGameObjectParameter("Object");
				BodyPart bodyPart3 = E.GetParameter("BodyPart") as BodyPart;
				bool flag5 = E.ID == "CommandForceEquipObject";
				bool flag6 = E.HasFlag("SemiForced");
				bool flag7 = E.IsSilent();
				int intParameter4 = E.GetIntParameter("AutoEquipTry");
				string text3 = E.GetStringParameter("FailureMessage");
				List<GameObject> list4 = (E.GetParameter("WasUnequipped") as List<GameObject>) ?? Event.NewGameObjectList();
				int num2 = E.GetIntParameter("DestroyOnUnequipDeclined");
				if (!flag5 && !flag6)
				{
					if (ParentObject.HasEffect("Stuck"))
					{
						if (!flag7 && ParentObject.IsPlayer())
						{
							Popup.ShowFail("You may not equip items while stuck!");
						}
						return false;
					}
					if (!ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true, flag7))
					{
						return false;
					}
				}
				Cell cell4 = obj3.CurrentCell;
				GameObject inInventory = obj3.InInventory;
				int num3 = 0;
				bool flag8 = false;
				try
				{
					if (!flag5 && inInventory != ParentObject && obj3.Equipped != ParentObject)
					{
						obj3.SplitFromStack();
						if (!ParentObject.ReceiveObject(obj3))
						{
							obj3.CheckStack();
							return false;
						}
					}
					if (obj3 != null)
					{
						GameObject inInventory2 = obj3.InInventory;
						if (inInventory2 != null && inInventory2 != ParentObject && !inInventory2.FireEvent(Event.New("BeforeContentsTaken", "Taker", ParentObject)))
						{
							return false;
						}
					}
					if (ParentObject.IsPlayer() && obj3.InInventory != ParentObject && obj3.Equipped != ParentObject && (obj3.CurrentCell != null || obj3.InInventory != null) && !string.IsNullOrEmpty(obj3.Owner))
					{
						if (E.HasFlag("OwnershipViolationDeclined"))
						{
							return false;
						}
						if (!E.HasFlag("OwnershipViolationConfirmed"))
						{
							E.SetFlag("OwnershipViolationChecked", State: true);
							if (Popup.ShowYesNoCancel("That is not owned by you. Are you sure you want to take it?") != 0)
							{
								E.SetFlag("OwnershipViolationDeclined", State: true);
								return false;
							}
							E.SetFlag("OwnershipViolationConfirmed", State: true);
							obj3.pPhysics.BroadcastForHelp(ParentObject);
						}
					}
					if (obj3 != null)
					{
						GameObject inInventory3 = obj3.InInventory;
						if (inInventory3 != null && !inInventory3.FireEvent(Event.New("AfterContentsTaken", "Taker", ParentObject)))
						{
							return false;
						}
					}
					List<BodyPart> @for = QuerySlotListEvent.GetFor(ParentObject, obj3);
					if (!flag5 && !flag6)
					{
						if (@for.Count == 0)
						{
							if (!E.IsSilent() && ParentObject.IsPlayer())
							{
								if (intParameter4 > 0)
								{
									if (string.IsNullOrEmpty(text3))
									{
										text3 = "You cannot equip " + obj3.t() + ".";
										E.SetParameter("FailureMessage", text3);
									}
								}
								else
								{
									Popup.ShowFail("You cannot equip " + obj3.t() + ".");
								}
							}
							return false;
						}
						if (bodyPart3 != null && !@for.Contains(bodyPart3))
						{
							if (!E.IsSilent() && ParentObject.IsPlayer())
							{
								if (intParameter4 > 0)
								{
									if (string.IsNullOrEmpty(text3))
									{
										text3 = "You cannot equip " + obj3.t() + " on your " + bodyPart3.GetOrdinalName() + ".";
										E.SetParameter("FailureMessage", text3);
									}
								}
								else
								{
									Popup.ShowFail("You cannot equip " + obj3.t() + " on your " + bodyPart3.GetOrdinalName() + ".");
								}
							}
							return false;
						}
					}
					if (bodyPart3 == null && ParentObject.IsPlayer())
					{
						List<BodyPart> list5 = new List<BodyPart>(@for.Count);
						List<string> list6 = new List<string>(@for.Count);
						List<char> list7 = new List<char>(@for.Count);
						char c = 'a';
						foreach (BodyPart item in @for)
						{
							list5.Add(item);
							list6.Add(item.ToString());
							list7.Add(c);
							c = (char)(c + 1);
						}
						int defaultSelected = 0;
						if (obj3.HasTag("MeleeWeapon") && list5.Any((BodyPart p) => p.Primary))
						{
							defaultSelected = list5.IndexOf(list5.First((BodyPart p) => p.Primary));
						}
						int num4 = Popup.ShowOptionList("", list6.ToArray(), list7.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, defaultSelected);
						if (num4 == -1)
						{
							return false;
						}
						bodyPart3 = list5[num4];
					}
					int @default = 1000;
					if (bodyPart3 != null && bodyPart3.Type == "Thrown Weapon")
					{
						@default = 0;
					}
					else if (ParentObject.CurrentCell == null)
					{
						@default = 0;
					}
					num3 = E.GetIntParameter("EnergyCost", @default);
					if (bodyPart3 == null || obj3 == null)
					{
						return false;
					}
					if (obj3.Count > 1)
					{
						obj3 = obj3.RemoveOne();
					}
					if (ParentObject.HasRegisteredEvent("BeginEquip"))
					{
						Event event6 = Event.New("BeginEquip", "Object", obj3, "BodyPart", bodyPart3);
						if (flag7)
						{
							event6.SetSilent(Silent: true);
						}
						if (intParameter4 > 0)
						{
							event6.SetParameter("AutoEquipTry", intParameter4);
						}
						if (!string.IsNullOrEmpty(text3))
						{
							event6.SetParameter("FailureMessage", text3);
						}
						if (list4 != null)
						{
							event6.SetParameter("WasUnequipped", list4);
						}
						if (num2 > 0)
						{
							event6.SetParameter("DestroyOnUnequipDeclined", num2);
						}
						bool num5 = ParentObject.FireEvent(event6);
						string stringParameter4 = event6.GetStringParameter("FailureMessage");
						if (!string.IsNullOrEmpty(stringParameter4) && stringParameter4 != text3)
						{
							text3 = stringParameter4;
							E.SetParameter("FailureMessage", text3);
						}
						int intParameter5 = event6.GetIntParameter("DestroyOnUnequipDeclined");
						if (intParameter5 > num2)
						{
							num2 = intParameter5;
							E.SetParameter("DestroyOnUnequipDeclined", num2);
						}
						if (!num5 && !flag5)
						{
							return false;
						}
					}
					if (obj3.HasRegisteredEvent("BeginBeingEquipped"))
					{
						Event event7 = Event.New("BeginBeingEquipped", "EquippingObject", ParentObject, "Equipper", ParentObject, "BodyPart", bodyPart3);
						if (flag7)
						{
							event7.SetSilent(Silent: true);
						}
						if (intParameter4 > 0)
						{
							event7.SetParameter("AutoEquipTry", intParameter4);
						}
						if (!string.IsNullOrEmpty(text3))
						{
							event7.SetParameter("FailureMessage", text3);
						}
						if (list4 != null)
						{
							event7.SetParameter("WasUnequipped", list4);
						}
						if (num2 > 0)
						{
							event7.SetParameter("DestroyOnUnequipDeclined", num2);
						}
						bool num6 = obj3.FireEvent(event7);
						string stringParameter5 = event7.GetStringParameter("FailureMessage");
						if (!string.IsNullOrEmpty(stringParameter5) && stringParameter5 != text3)
						{
							text3 = stringParameter5;
							E.SetParameter("FailureMessage", text3);
						}
						int intParameter6 = event7.GetIntParameter("DestroyOnUnequipDeclined");
						if (intParameter6 > num2)
						{
							num2 = intParameter6;
							E.SetParameter("DestroyOnUnequipDeclined", num2);
						}
						if (!num6 && !flag5)
						{
							return false;
						}
					}
					eCommandRemoveObject.SetParameter("Object", obj3);
					eCommandRemoveObject.SetFlag("ForEquip", State: true);
					eCommandRemoveObject.SetSilent(Silent: true);
					if (!ParentObject.FireEvent(eCommandRemoveObject) && !flag5)
					{
						return false;
					}
					if (obj3.CurrentCell != null && !obj3.CurrentCell.RemoveObject(obj3))
					{
						return false;
					}
					Event event8 = Event.New("PerformEquip", "Object", obj3);
					event8.SetParameter("BodyPart", bodyPart3);
					if (flag7)
					{
						event8.SetSilent(Silent: true);
					}
					if (intParameter4 > 0)
					{
						event8.SetParameter("AutoEquipTry", intParameter4);
					}
					if (!string.IsNullOrEmpty(text3))
					{
						event8.SetParameter("FailureMessage", text3);
					}
					if (list4 != null)
					{
						event8.SetParameter("WasUnequipped", list4);
					}
					if (num2 > 0)
					{
						event8.SetParameter("DestroyOnUnequipDeclined", num2);
					}
					bool num7 = ParentObject.FireEvent(event8);
					string stringParameter6 = event8.GetStringParameter("FailureMessage");
					if (!string.IsNullOrEmpty(stringParameter6) && stringParameter6 != text3)
					{
						text3 = stringParameter6;
						E.SetParameter("FailureMessage", text3);
					}
					int intParameter7 = event8.GetIntParameter("DestroyOnUnequipDeclined");
					if (intParameter7 > num2)
					{
						num2 = intParameter7;
						E.SetParameter("DestroyOnUnequipDeclined", num2);
					}
					if (!num7 && !flag5)
					{
						Event event9 = Event.New("CommandTakeObject");
						event9.SetParameter("Object", obj3);
						event9.SetParameter("Context", E.GetStringParameter("Context"));
						event9.SetSilent(Silent: true);
						if (!ParentObject.FireEvent(event9))
						{
							if (ParentObject.CurrentCell != null)
							{
								ParentObject.CurrentCell.AddObject(obj3);
							}
							else if (ParentObject.IsPlayer())
							{
								Popup.ShowFail("Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
							}
							else
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.DisplayName + "] Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
							}
						}
						if (!flag7 && intParameter4 <= 0 && ParentObject.IsPlayer())
						{
							string text4 = null;
							if (!string.IsNullOrEmpty(text3))
							{
								text4 = text3;
							}
							if (list4 != null && list4.Count > 0)
							{
								string text5 = ParentObject.DescribeUnequip(list4);
								if (!string.IsNullOrEmpty(text5))
								{
									if (text4 == null)
									{
										text4 = "";
									}
									if (text4 != null)
									{
										text4 += "\n\n";
									}
									text4 += text5;
								}
							}
							if (!string.IsNullOrEmpty(text4))
							{
								Popup.ShowFail(text4);
							}
						}
						return false;
					}
					flag8 = true;
				}
				finally
				{
					if (!flag8)
					{
						if (cell4 != null)
						{
							if (GameObject.validate(ref obj3) && obj3.CurrentCell != cell4)
							{
								obj3.RemoveFromContext(E);
								cell4.AddObject(obj3);
							}
						}
						else if (inInventory?.Inventory != null && GameObject.validate(ref obj3) && obj3.InInventory != inInventory)
						{
							obj3.RemoveFromContext(E);
							inInventory.Inventory.AddObject(obj3);
						}
					}
				}
				if (flag8 && num3 > 0)
				{
					ParentObject.UseEnergy(num3, "Equip Item");
				}
			}
			else if (E.ID == "CommandUnequipObject" || E.ID == "CommandForceUnequipObject")
			{
				bool flag9 = E.ID.Contains("Force");
				bool flag10 = E.HasFlag("SemiForced");
				bool flag11 = E.IsSilent();
				int intParameter8 = E.GetIntParameter("AutoEquipTry");
				string stringParameter7 = E.GetStringParameter("FailureMessage");
				int intParameter9 = E.GetIntParameter("DestroyOnUnequipDeclined");
				BodyPart bodyPart4 = E.GetParameter("BodyPart") as BodyPart;
				if (bodyPart4 == null)
				{
					if (!E.HasParameter("Object"))
					{
						return true;
					}
					bodyPart4 = E.GetGameObjectParameter("Object").EquippedOn();
					if (bodyPart4 == null)
					{
						return true;
					}
				}
				GameObject obj4 = bodyPart4.Equipped;
				if (obj4 == null)
				{
					return true;
				}
				if (!flag9 && !flag10)
				{
					if (ParentObject.HasEffect("Stuck"))
					{
						stringParameter7 = "You may not remove items while stuck!";
						E.SetParameter("FailureMessage", stringParameter7);
						if (!flag11 && intParameter8 <= 0 && ParentObject.IsPlayer())
						{
							Popup.ShowFail(stringParameter7);
						}
						return false;
					}
					if (!ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true, Silent: true))
					{
						stringParameter7 = "You are frozen solid!";
						E.SetParameter("FailureMessage", stringParameter7);
						if (!flag11 && intParameter8 <= 0 && ParentObject.IsPlayer())
						{
							Popup.ShowFail(stringParameter7);
						}
						return false;
					}
				}
				Event event10 = Event.New("BeginUnequip", "BodyPart", bodyPart4);
				event10.SetFlag("Forced", flag9);
				event10.SetFlag("SemiForced", flag10);
				if (flag11)
				{
					event10.SetSilent(Silent: true);
				}
				if (intParameter8 > 0)
				{
					event10.SetParameter("AutoEquipTry", intParameter8);
				}
				if (!string.IsNullOrEmpty(stringParameter7))
				{
					event10.SetParameter("FailureMessage", stringParameter7);
				}
				if (intParameter9 > 0)
				{
					event10.SetParameter("DestroyOnUnequipDeclined", intParameter9);
				}
				if (!ParentObject.FireEvent(event10))
				{
					string stringParameter8 = event10.GetStringParameter("FailureMessage");
					if (!string.IsNullOrEmpty(stringParameter8) && stringParameter8 != stringParameter7)
					{
						stringParameter7 = stringParameter8;
						E.SetParameter("FailureMessage", stringParameter8);
					}
					int intParameter10 = event10.GetIntParameter("DestroyOnUnequipDeclined");
					if (intParameter10 > intParameter9)
					{
						intParameter9 = intParameter10;
						E.SetParameter("DestroyOnUnequipDeclined", intParameter9);
					}
					return false;
				}
				Event event11 = Event.New("BeginBeingUnequipped");
				event11.SetParameter("Owner", ParentObject);
				event11.SetParameter("BodyPart", bodyPart4);
				event11.SetFlag("Forced", flag9);
				event11.SetFlag("SemiForced", flag10);
				if (flag11)
				{
					event11.SetSilent(Silent: true);
				}
				if (intParameter8 > 0)
				{
					event11.SetParameter("AutoEquipTry", intParameter8);
				}
				if (!string.IsNullOrEmpty(stringParameter7))
				{
					event11.SetParameter("FailureMessage", stringParameter7);
				}
				if (intParameter9 > 0)
				{
					event11.SetParameter("DestroyOnUnequipDeclined", intParameter9);
				}
				if (!obj4.FireEvent(event11))
				{
					string stringParameter9 = event11.GetStringParameter("FailureMessage");
					if (!string.IsNullOrEmpty(stringParameter9) && stringParameter9 != stringParameter7)
					{
						stringParameter7 = stringParameter9;
						E.SetParameter("FailureMessage", stringParameter9);
					}
					int intParameter11 = event11.GetIntParameter("DestroyOnUnequipDeclined");
					if (intParameter11 > intParameter9)
					{
						intParameter9 = intParameter11;
						E.SetParameter("DestroyOnUnequipDeclined", intParameter9);
					}
					return false;
				}
				Event event12 = Event.New("PerformUnequip");
				event12.SetParameter("BodyPart", bodyPart4);
				event12.SetFlag("Forced", flag9);
				event12.SetFlag("SemiForced", flag10);
				if (flag11)
				{
					event12.SetSilent(Silent: true);
				}
				if (intParameter8 > 0)
				{
					event12.SetParameter("AutoEquipTry", intParameter8);
				}
				if (!string.IsNullOrEmpty(stringParameter7))
				{
					event12.SetParameter("FailureMessage", stringParameter7);
				}
				if (intParameter9 > 0)
				{
					event12.SetParameter("DestroyOnUnequipDeclined", intParameter9);
				}
				if (!ParentObject.FireEvent(event12))
				{
					string stringParameter10 = event12.GetStringParameter("FailureMessage");
					if (!string.IsNullOrEmpty(stringParameter10) && stringParameter10 != stringParameter7)
					{
						stringParameter7 = stringParameter10;
						E.SetParameter("FailureMessage", stringParameter10);
					}
					int intParameter12 = event12.GetIntParameter("DestroyOnUnequipDeclined");
					if (intParameter12 > intParameter9)
					{
						intParameter9 = intParameter12;
						E.SetParameter("DestroyOnUnequipDeclined", intParameter9);
					}
					return false;
				}
				if (GameObject.validate(ref obj4))
				{
					if (!flag11 && ParentObject.HasPart("Combat"))
					{
						DidXToY("unequip", obj4);
					}
					obj4.pPhysics.Equipped = null;
					if (GameObject.validate(ref obj4) && !E.HasParameter("NoTake") && !obj4.IsInGraveyard())
					{
						eCommandFreeTakeObject.SetParameter("Object", obj4);
						eCommandFreeTakeObject.SetSilent(Silent: true);
						if (obj4 != null && obj4.HasTag("DestroyWhenUnequipped"))
						{
							obj4.Destroy();
						}
						else
						{
							if (ParentObject.FireEvent(eCommandFreeTakeObject))
							{
								return true;
							}
							if (ParentObject.CurrentCell != null)
							{
								ParentObject.CurrentCell.AddObject(obj4);
							}
							else if (ParentObject.IsPlayer())
							{
								Popup.ShowFail("Error dropping object, removing to graveyard zone! (Inventory.cs:CommandEquipObject)");
							}
						}
					}
				}
			}
			else if (E.ID == "CommandTakeObject")
			{
				GameObject gameObjectParameter7 = E.GetGameObjectParameter("Object");
				if (gameObjectParameter7 == null)
				{
					return false;
				}
				int intParameter13 = E.GetIntParameter("EnergyCost", 1000);
				if (gameObjectParameter7.MovingIntoWouldCreateContainmentLoop(ParentObject))
				{
					MetricsManager.LogError(ParentObject.DebugName + " taking " + gameObjectParameter7.DebugName + " would create containment loop");
					return false;
				}
				if (intParameter13 > 0 && !ParentObject.CheckFrozen(Telepathic: false, Telekinetic: true))
				{
					return false;
				}
				if (gameObjectParameter7.IsInStasis())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot budge " + gameObjectParameter7.t() + ".");
					}
					return false;
				}
				GameObject inInventory4 = gameObjectParameter7.InInventory;
				if (inInventory4 != null && !inInventory4.FireEvent(Event.New("BeforeContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				if (inInventory4 != null && inInventory4.IsOwned() && ParentObject.IsPlayer() && inInventory4.HasTagOrProperty("DontWarnOnOpen") && gameObjectParameter7.GetIntProperty("StoredByPlayer") <= 0 && gameObjectParameter7.GetIntProperty("FromStoredByPlayer") <= 0 && Popup.ShowYesNo("You don't own " + inInventory4.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: false, inInventory4.indicativeProximal) + ". Are you sure you want to take " + gameObjectParameter7.t() + "?") != 0)
				{
					return false;
				}
				if (inInventory4 != null && !inInventory4.FireEvent(Event.New("AfterContentsTaken", "Taker", ParentObject)))
				{
					return false;
				}
				GameObject gameObjectParameter8 = E.GetGameObjectParameter("PutBy");
				string stringParameter11 = E.GetStringParameter("Context");
				Event event13 = Event.New("BeginTake");
				event13.SetParameter("Object", gameObjectParameter7);
				event13.SetParameter("PutBy", gameObjectParameter8);
				event13.SetParameter("Container", inInventory4);
				event13.SetParameter("Context", stringParameter11);
				event13.SetSilent(E.IsSilent());
				if (!ParentObject.FireEvent(event13, E))
				{
					return false;
				}
				Event event14 = Event.New("BeginBeingTaken");
				event14.SetParameter("TakingObject", ParentObject);
				event14.SetParameter("PutBy", gameObjectParameter8);
				event14.SetParameter("Container", inInventory4);
				event14.SetParameter("Context", stringParameter11);
				event14.SetSilent(E.IsSilent());
				if (!gameObjectParameter7.FireEvent(event14, E))
				{
					return false;
				}
				Event event15 = Event.New("PerformTake");
				event15.SetParameter("Object", gameObjectParameter7);
				event15.SetParameter("PutBy", E.GetGameObjectParameter("PutBy"));
				event15.SetParameter("Container", inInventory4);
				event15.SetParameter("Context", stringParameter11);
				event15.SetSilent(E.IsSilent());
				if (!ParentObject.FireEvent(event15, E))
				{
					return false;
				}
				if (intParameter13 > 0)
				{
					ParentObject.UseEnergy(intParameter13, "Take", stringParameter11);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool WantTurnTick()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantTurnTick())
			{
				return true;
			}
		}
		return false;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (TurnTickObjectsInUse)
		{
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			GameObject gameObject3 = null;
			List<GameObject> list = null;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				GameObject gameObject4 = Objects[i];
				if (gameObject4.WantTurnTick())
				{
					if (list != null)
					{
						list.Add(gameObject4);
						continue;
					}
					if (gameObject == null)
					{
						gameObject = gameObject4;
						continue;
					}
					if (gameObject2 == null)
					{
						gameObject2 = gameObject4;
						continue;
					}
					if (gameObject3 == null)
					{
						gameObject3 = gameObject4;
						continue;
					}
					list = new List<GameObject>(4) { gameObject, gameObject2, gameObject3, gameObject4 };
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].TurnTick(TurnNumber);
				}
			}
			else if (gameObject != null)
			{
				gameObject.TurnTick(TurnNumber);
				if (gameObject2 != null)
				{
					gameObject2.TurnTick(TurnNumber);
					gameObject3?.TurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickObjectsInUse = true;
		try
		{
			TurnTickObjects.Clear();
			int k = 0;
			for (int count3 = Objects.Count; k < count3; k++)
			{
				GameObject gameObject5 = Objects[k];
				if (gameObject5.WantTurnTick())
				{
					TurnTickObjects.Add(gameObject5);
				}
			}
			int l = 0;
			for (int count4 = TurnTickObjects.Count; l < count4; l++)
			{
				TurnTickObjects[l].TurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickObjectsInUse = false;
		}
	}

	public override bool WantTenTurnTick()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantTenTurnTick())
			{
				return true;
			}
		}
		return false;
	}

	public override void TenTurnTick(long TurnNumber)
	{
		if (TurnTickObjectsInUse)
		{
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			GameObject gameObject3 = null;
			List<GameObject> list = null;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				GameObject gameObject4 = Objects[i];
				if (gameObject4.WantTenTurnTick())
				{
					if (list != null)
					{
						list.Add(gameObject4);
						continue;
					}
					if (gameObject == null)
					{
						gameObject = gameObject4;
						continue;
					}
					if (gameObject2 == null)
					{
						gameObject2 = gameObject4;
						continue;
					}
					if (gameObject3 == null)
					{
						gameObject3 = gameObject4;
						continue;
					}
					list = new List<GameObject>(4) { gameObject, gameObject2, gameObject3, gameObject4 };
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].TenTurnTick(TurnNumber);
				}
			}
			else if (gameObject != null)
			{
				gameObject.TenTurnTick(TurnNumber);
				if (gameObject2 != null)
				{
					gameObject2.TenTurnTick(TurnNumber);
					gameObject3?.TenTurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickObjectsInUse = true;
		try
		{
			TurnTickObjects.Clear();
			int k = 0;
			for (int count3 = Objects.Count; k < count3; k++)
			{
				GameObject gameObject5 = Objects[k];
				if (gameObject5.WantTurnTick())
				{
					TurnTickObjects.Add(gameObject5);
				}
			}
			int l = 0;
			for (int count4 = TurnTickObjects.Count; l < count4; l++)
			{
				TurnTickObjects[l].TenTurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickObjectsInUse = false;
		}
	}

	public override bool WantHundredTurnTick()
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i].WantHundredTurnTick())
			{
				return true;
			}
		}
		return false;
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		if (TurnTickObjectsInUse)
		{
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			GameObject gameObject3 = null;
			List<GameObject> list = null;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				GameObject gameObject4 = Objects[i];
				if (gameObject4.WantHundredTurnTick())
				{
					if (list != null)
					{
						list.Add(gameObject4);
						continue;
					}
					if (gameObject == null)
					{
						gameObject = gameObject4;
						continue;
					}
					if (gameObject2 == null)
					{
						gameObject2 = gameObject4;
						continue;
					}
					if (gameObject3 == null)
					{
						gameObject3 = gameObject4;
						continue;
					}
					list = new List<GameObject>(4) { gameObject, gameObject2, gameObject3, gameObject4 };
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].HundredTurnTick(TurnNumber);
				}
			}
			else if (gameObject != null)
			{
				gameObject.HundredTurnTick(TurnNumber);
				if (gameObject2 != null)
				{
					gameObject2.HundredTurnTick(TurnNumber);
					gameObject3?.HundredTurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickObjectsInUse = true;
		try
		{
			TurnTickObjects.Clear();
			int k = 0;
			for (int count3 = Objects.Count; k < count3; k++)
			{
				GameObject gameObject5 = Objects[k];
				if (gameObject5.WantTurnTick())
				{
					TurnTickObjects.Add(gameObject5);
				}
			}
			int l = 0;
			for (int count4 = TurnTickObjects.Count; l < count4; l++)
			{
				TurnTickObjects[l].HundredTurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickObjectsInUse = false;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GameObject @object in Objects)
		{
			stringBuilder.Append(@object.DisplayName);
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}

	public bool IsOverburdened()
	{
		if (!ParentObject.IsPlayerControlled())
		{
			return false;
		}
		return GetWeight() + (ParentObject.Body?.GetWeight() ?? 0) > ParentObject.GetMaxCarriedWeight();
	}

	public bool WouldBeOverburdened(int Weight)
	{
		if (!ParentObject.IsPlayerControlled())
		{
			return false;
		}
		return Weight + GetWeight() + (ParentObject.Body?.GetWeight() ?? 0) > ParentObject.GetMaxCarriedWeight();
	}

	public bool WouldBeOverburdened(GameObject GO)
	{
		if (!ParentObject.IsPlayerControlled())
		{
			return false;
		}
		return GO.Weight + GetWeight() + (ParentObject.Body?.GetWeight() ?? 0) > ParentObject.GetMaxCarriedWeight();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (base.WantEvent(ID, cascade))
		{
			return true;
		}
		if (ID == AddedToInventoryEvent.ID)
		{
			return true;
		}
		if (ID == GetExtrinsicWeightEvent.ID && Objects.Count > 0)
		{
			return true;
		}
		if (ID == GetExtrinsicValueEvent.ID && Objects.Count > 0)
		{
			return true;
		}
		if (ID == AfterObjectCreatedEvent.ID)
		{
			return true;
		}
		if (ID == BeforeDeathRemovalEvent.ID)
		{
			return true;
		}
		if (ID == StripContentsEvent.ID)
		{
			return true;
		}
		if (ID == GetContentsEvent.ID)
		{
			return true;
		}
		if (ID == StatChangeEvent.ID)
		{
			return true;
		}
		if (ID == CarryingCapacityChangedEvent.ID)
		{
			return true;
		}
		if (ID == InventoryActionEvent.ID)
		{
			return true;
		}
		if (MinEvent.CascadeTo(cascade, 2))
		{
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].WantEvent(ID, cascade))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(2))
		{
			int num = -1;
			int i = 0;
			for (int count = Objects.Count; i < count; i++)
			{
				if (Objects[i].WantEvent(E.ID, E.GetCascadeLevel()))
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				List<GameObject> list = Event.NewGameObjectList();
				list.AddRange(Objects);
				int j = num;
				for (int count2 = list.Count; j < count2; j++)
				{
					if (!list[j].HandleEvent(E))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "EmptyForDisassemble")
		{
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(Objects);
			ParentObject.GetContext(out var ObjectContext, out var CellContext);
			if (ObjectContext != null)
			{
				ObjectContext.TakeObject(list, Silent: true, 0);
			}
			else if (CellContext != null)
			{
				foreach (GameObject item in list)
				{
					item.RemoveFromContext(E);
					CellContext.AddObject(item);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			E.Value += Objects[i].Value;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (!Objects[i].HasTag("HiddenInInventory"))
			{
				E.Weight += Objects[i].GetWeight();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AddedToInventoryEvent E)
	{
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (Objects.Count == 0)
		{
			CheckEmptyState();
		}
		else
		{
			CheckNonEmptyState();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject @object in Objects)
		{
			if ((!E.KeepNatural || !@object.IsNatural()) && (@object.pPhysics == null || @object.pPhysics.IsReal))
			{
				list.Add(@object);
			}
		}
		foreach (GameObject item in list)
		{
			item.Obliterate(null, E.Silent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		E.Objects.AddRange(Objects);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (DropOnDeath && !ParentObject.IsTemporary && ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				if (cell.IsOccluding())
				{
					cell = cell.GetFirstNonOccludingAdjacentCell() ?? cell;
				}
				if (Objects.Count > 0)
				{
					List<GameObject> list = Event.NewGameObjectList();
					list.AddRange(Objects);
					Objects.Clear();
					int i = 0;
					for (int count = list.Count; i < count; i++)
					{
						GameObject gameObject = list[i];
						gameObject.RemoveFromContext(E);
						if (gameObject.IsReal && DropOnDeathEvent.Check(gameObject, cell))
						{
							cell.AddObject(gameObject);
							gameObject.HandleEvent(DroppedEvent.FromPool(null, gameObject));
						}
					}
				}
			}
			else
			{
				Objects.Clear();
			}
			FlushWeightCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Strength" && CheckOverburdenedOnStrengthUpdateEvent.Check(ParentObject))
		{
			CheckOverburdened();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CarryingCapacityChangedEvent E)
	{
		CheckOverburdened();
		return base.HandleEvent(E);
	}

	public int GetOccurrences(GameObject obj)
	{
		int num = 0;
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			if (Objects[i] == obj)
			{
				num++;
			}
		}
		return num;
	}
}
