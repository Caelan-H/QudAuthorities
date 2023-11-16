using System;
using System.Collections.Generic;
using System.Text;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Body : IPart
{
	[Serializable]
	public class DismemberedPart
	{
		public BodyPart Part;

		public int ParentID;

		public DismemberedPart()
		{
		}

		public DismemberedPart(BodyPart P, BodyPart ParentPart)
			: this()
		{
			Part = P;
			if (ParentPart != null)
			{
				ParentID = ParentPart.ID;
			}
		}

		public void Save(SerializationWriter Writer)
		{
			Part.Save(Writer);
			Writer.Write(ParentID);
		}

		public void Load(SerializationReader Reader)
		{
			Part = BodyPart.Load(Reader, null);
			ParentID = Reader.ReadInt32();
		}

		public bool IsReattachable(Body ParentBody)
		{
			return ParentBody.GetPartByID(ParentID) != null;
		}

		public void Reattach(Body ParentBody)
		{
			(ParentBody.GetPartByID(ParentID) ?? throw new Exception("cannot reattach, parent " + ParentID + " missing")).AddPart(Part, Part.Position, DoUpdate: false);
		}

		public void CheckRenumberingOnPositionAssignment(BodyPart Parent, int AssignedPosition, BodyPart ExceptPart = null)
		{
			if (Parent.idMatch(ParentID) && Part.Position >= AssignedPosition && Part != ExceptPart)
			{
				Part.Position++;
			}
		}

		public bool HasPosition(BodyPart Parent, int Position, BodyPart ExceptPart = null)
		{
			if (Parent.idMatch(ParentID) && Part.Position == Position)
			{
				return Part != ExceptPart;
			}
			return false;
		}
	}

	public const int MAXIMUM_MOBILITY_MOVE_SPEED_PENALTY = 60;

	public const int BASIC_FULL_MOBILITY = 2;

	public int MobilitySpeedPenaltyApplied;

	[NonSerialized]
	public BodyPart _Body;

	public bool built;

	[NonSerialized]
	public List<DismemberedPart> DismemberedParts;

	public string _Anatomy;

	[NonSerialized]
	private static Event eBodypartsUpdated = new ImmutableEvent("BodypartsUpdated");

	[NonSerialized]
	private static Event eDecorateDefaultEquipment = new ImmutableEvent("DecorateDefaultEquipment");

	[NonSerialized]
	public static Dictionary<GameObject, BodyPart> DeepCopyEquipMap = null;

	public int WeightCache = -1;

	[NonSerialized]
	public static List<BodyPart> partStatic = new List<BodyPart>();

	[NonSerialized]
	private static List<BodyPart> parts2 = new List<BodyPart>();

	[NonSerialized]
	private List<BodyPart> _bodyParts = new List<BodyPart>();

	private static List<BodyPart> AllBodyParts = new List<BodyPart>(16);

	private static List<BodyPart> SomeBodyParts = new List<BodyPart>(4);

	private static List<BodyPart> OtherBodyParts = new List<BodyPart>(4);

	public string Anatomy
	{
		get
		{
			return _Anatomy;
		}
		set
		{
			Anatomies.GetAnatomyOrFail(value).ApplyTo(this);
		}
	}

	public override void SaveData(SerializationWriter Writer)
	{
		_Body.Save(Writer);
		if (DismemberedParts == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(DismemberedParts.Count);
			if (DismemberedParts.Count > 0)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					item.Save(Writer);
				}
			}
		}
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		_Body = BodyPart.Load(Reader, this);
		int num = Reader.ReadInt32();
		if (num > 0)
		{
			DismemberedParts = new List<DismemberedPart>(num);
			for (int i = 0; i < num; i++)
			{
				DismemberedPart dismemberedPart = new DismemberedPart();
				dismemberedPart.Load(Reader);
				DismemberedParts.Add(dismemberedPart);
			}
		}
		else
		{
			DismemberedParts = null;
		}
		base.LoadData(Reader);
	}

	public void Clear()
	{
		_Body.Clear();
	}

	[Obsolete("version with MapInv argument should always be called")]
	public override IPart DeepCopy(GameObject Parent)
	{
		return base.DeepCopy(Parent);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		DeepCopyEquipMap = new Dictionary<GameObject, BodyPart>(8);
		Body body = base.DeepCopy(Parent, MapInv) as Body;
		body.built = false;
		body._Body = _Body.DeepCopy(Parent, body, MapInv);
		body.built = true;
		body.UpdateBodyParts();
		return body;
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		if (ParentObject.DeepCopyInventoryObjectMap != null)
		{
			foreach (GameObject key in DeepCopyEquipMap.Keys)
			{
				if (!key.IsValid())
				{
					continue;
				}
				if (key.IsImplant)
				{
					if (key.HasTag("CyberneticsUsesEqSlot"))
					{
						key.pPhysics._Equipped = ParentObject;
						DeepCopyEquipMap[key].DoEquip(key);
					}
					DeepCopyEquipMap[key].Implant(key, ForDeepCopy: true);
				}
				else
				{
					key.pPhysics._Equipped = ParentObject;
					DeepCopyEquipMap[key].DoEquip(key, Silent: false, ForDeepCopy: true);
				}
			}
		}
		DeepCopyEquipMap = null;
	}

	public int GetWeight()
	{
		if (WeightCache == -1)
		{
			return RecalculateWeight();
		}
		return WeightCache;
	}

	public int RecalculateWeight()
	{
		WeightCache = _Body.GetWeight();
		return WeightCache;
	}

	public void FlushWeightCache()
	{
		WeightCache = -1;
	}

	public string GetPrimaryLimbType()
	{
		return ParentObject.GetPropertyOrTag("PrimaryLimbType", "Hand");
	}

	public GameObject GetShield(Predicate<GameObject> Filter = null, GameObject Attacker = null)
	{
		GameObject obj = null;
		_Body.GetShield(ref obj, Filter, Attacker, ParentObject);
		return obj;
	}

	public GameObject GetMainWeapon(out int PossibleWeapons, out BodyPart primaryWeaponPart, bool NeedPrimary = false, bool FailDownFromPrimary = false)
	{
		BodyPart equippedOn = null;
		GameObject Weapon = null;
		bool HadPrimary = false;
		int PickPriority = 0;
		PossibleWeapons = 0;
		_Body.ScanForWeapon(NeedPrimary, ref Weapon, ref equippedOn, ref PickPriority, ref PossibleWeapons, ref HadPrimary);
		primaryWeaponPart = equippedOn;
		if (NeedPrimary && !FailDownFromPrimary && !HadPrimary)
		{
			return null;
		}
		return Weapon;
	}

	public GameObject GetMainWeapon(bool NeedPrimary = false, bool FailDownFromPrimary = false)
	{
		BodyPart primaryWeaponPart = null;
		int PossibleWeapons;
		return GetMainWeapon(out PossibleWeapons, out primaryWeaponPart, NeedPrimary, FailDownFromPrimary);
	}

	public bool HasWeaponOfType(string Type, bool NeedPrimary = false)
	{
		return _Body.HasWeaponOfType(Type, NeedPrimary);
	}

	public bool HasPrimaryWeaponOfType(string Type)
	{
		return HasWeaponOfType(Type, NeedPrimary: true);
	}

	public bool HasWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary = false)
	{
		return _Body.HasWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
	}

	public GameObject GetWeaponOfType(string Type, bool NeedPrimary = false, bool PreferPrimary = false)
	{
		if (PreferPrimary && !NeedPrimary)
		{
			GameObject weaponOfType = _Body.GetWeaponOfType(Type, NeedPrimary: true);
			if (weaponOfType != null)
			{
				return weaponOfType;
			}
		}
		return _Body.GetWeaponOfType(Type, NeedPrimary);
	}

	public GameObject GetPrimaryWeaponOfType(string Type)
	{
		return GetWeaponOfType(Type, NeedPrimary: true);
	}

	public GameObject GetPrimaryWeaponOfType(string Type, bool AcceptFirstHandForNonHandPrimary)
	{
		GameObject weaponOfType = GetWeaponOfType(Type, NeedPrimary: true);
		if (weaponOfType != null)
		{
			return weaponOfType;
		}
		if (AcceptFirstHandForNonHandPrimary && GetPrimaryLimbType() != "Hand")
		{
			BodyPart firstPart = GetFirstPart("Hand");
			if (firstPart != null)
			{
				weaponOfType = firstPart.ThisPartWeaponOfType(Type, NeedPrimary: false);
				if (weaponOfType != null)
				{
					return weaponOfType;
				}
			}
		}
		return null;
	}

	public GameObject GetWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary = false, bool PreferPrimary = false)
	{
		if (PreferPrimary && !NeedPrimary)
		{
			GameObject weaponOfTypeOnBodyPartOfType = _Body.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary: true);
			if (weaponOfTypeOnBodyPartOfType != null)
			{
				return weaponOfTypeOnBodyPartOfType;
			}
		}
		return _Body.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
	}

	public GameObject GetPrimaryWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType)
	{
		return GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary: true);
	}

	public void ClearShieldBlocks()
	{
		_Body.ClearShieldBlocks();
	}

	public List<GameObject> StripAllEquipment()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.StripAllEquipment(list);
		return list;
	}

	public List<GameObject> GetPrimaryHandEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.GetPrimaryHandEquippedObjects(list);
		return list;
	}

	public List<GameObject> GetPrimaryEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>();
		_Body.GetPrimaryEquippedObjects(list);
		return list;
	}

	public void GetEquippedObjects(List<GameObject> Return)
	{
		_Body.GetEquippedObjects(Return);
	}

	public List<GameObject> GetEquippedObjects()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount());
		_Body.GetEquippedObjects(list);
		return list;
	}

	public List<GameObject> GetEquippedObjectsReadonly()
	{
		return _Body.GetEquippedObjectsReadonly();
	}

	public List<GameObject> GetEquippedObjects(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(pFilter));
		_Body.GetEquippedObjects(list, pFilter);
		return list;
	}

	public void GetEquippedObjectsExceptNatural(List<GameObject> Return)
	{
		_Body.GetEquippedObjectsExceptNatural(Return);
	}

	public List<GameObject> GetEquippedObjectsExceptNatural()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount());
		_Body.GetEquippedObjectsExceptNatural(list);
		return list;
	}

	public List<GameObject> GetEquippedObjectsExceptNatural(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(pFilter));
		_Body.GetEquippedObjectsExceptNatural(list, pFilter);
		return list;
	}

	public void ForeachEquippedObject(Action<GameObject> aProc)
	{
		_Body.ForeachEquippedObject(aProc);
	}

	public void SafeForeachEquippedObject(Action<GameObject> aProc)
	{
		_Body.SafeForeachEquippedObject(aProc);
	}

	public void GetInstalledCybernetics(List<GameObject> Return)
	{
		_Body.GetInstalledCybernetics(Return);
	}

	public List<GameObject> GetInstalledCybernetics()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetInstalledCyberneticsCount());
		_Body.GetInstalledCybernetics(list);
		return list;
	}

	public List<GameObject> GetInstalledCyberneticsReadonly()
	{
		return _Body.GetInstalledCyberneticsReadonly();
	}

	public List<GameObject> GetInstalledCybernetics(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetInstalledCyberneticsCount(pFilter));
		_Body.GetInstalledCybernetics(list, pFilter);
		return list;
	}

	public bool AnyInstalledCybernetics()
	{
		return _Body.AnyInstalledCybernetics();
	}

	public bool AnyInstalledCybernetics(Predicate<GameObject> pFilter)
	{
		return _Body.AnyInstalledCybernetics(pFilter);
	}

	public void ForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		_Body.ForeachInstalledCybernetics(aProc);
	}

	public void SafeForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		_Body.SafeForeachInstalledCybernetics(aProc);
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return)
	{
		_Body.GetEquippedObjectsAndInstalledCybernetics(Return);
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCybernetics()
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount() + _Body.GetInstalledCyberneticsCount());
		_Body.GetEquippedObjectsAndInstalledCybernetics(list);
		return list;
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCyberneticsReadonly()
	{
		return _Body.GetEquippedObjectsAndInstalledCyberneticsReadonly();
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCybernetics(Predicate<GameObject> Filter)
	{
		List<GameObject> list = new List<GameObject>(_Body.GetEquippedObjectCount(Filter) + _Body.GetInstalledCyberneticsCount(Filter));
		_Body.GetEquippedObjectsAndInstalledCybernetics(list, Filter);
		return list;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public bool HasPrimaryHand()
	{
		foreach (BodyPart item in GetPart("Hand"))
		{
			if (item.Primary)
			{
				return true;
			}
		}
		return false;
	}

	public BodyPart GetBody()
	{
		return _Body;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		_Body.ToString(stringBuilder);
		return stringBuilder.ToString();
	}

	public bool AnyRegisteredEvent(string ID)
	{
		return _Body.AnyRegisteredEvent(ID);
	}

	public BodyPart GetPartByManager(string Manager, bool EvenIfDismembered = false)
	{
		BodyPart bodyPart = _Body.FindByManager(Manager);
		if (bodyPart != null)
		{
			return bodyPart;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num = DismemberedParts.Count - 1; num >= 0; num--)
			{
				if (DismemberedParts[num].Part.Manager == Manager)
				{
					return DismemberedParts[num].Part;
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByManager(string Manager, string Type, bool EvenIfDismembered = false)
	{
		BodyPart bodyPart = _Body.FindByManager(Manager, Type);
		if (bodyPart != null)
		{
			return bodyPart;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num = DismemberedParts.Count - 1; num >= 0; num--)
			{
				if (DismemberedParts[num].Part.Manager == Manager && DismemberedParts[num].Part.Type == Type)
				{
					return DismemberedParts[num].Part;
				}
			}
		}
		return null;
	}

	public void GetPartsByManager(string Manager, List<BodyPart> Store, bool EvenIfDismembered = false)
	{
		_Body.FindByManager(Manager, Store);
		if (!EvenIfDismembered || DismemberedParts == null)
		{
			return;
		}
		for (int num = DismemberedParts.Count - 1; num >= 0; num--)
		{
			if (DismemberedParts[num].Part.Manager == Manager)
			{
				Store.Add(DismemberedParts[num].Part);
			}
		}
	}

	public void GetPartsByManager(string Manager, string Type, List<BodyPart> Store, bool EvenIfDismembered = false)
	{
		_Body.FindByManager(Manager, Store);
		if (!EvenIfDismembered || DismemberedParts == null)
		{
			return;
		}
		for (int num = DismemberedParts.Count - 1; num >= 0; num--)
		{
			if (DismemberedParts[num].Part.Manager == Manager && DismemberedParts[num].Part.Type == Type)
			{
				Store.Add(DismemberedParts[num].Part);
			}
		}
	}

	public int RemovePartsByManager(string Manager, bool EvenIfDismembered = false)
	{
		int num = 0;
		BodyPart partByManager;
		while ((partByManager = GetPartByManager(Manager)) != null)
		{
			RemovePart(partByManager);
			num++;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			for (int num2 = DismemberedParts.Count - 1; num2 >= 0; num2--)
			{
				if (DismemberedParts[num2].Part.Manager == Manager)
				{
					DismemberedParts.RemoveAt(num2);
					num++;
				}
			}
		}
		return num;
	}

	public void FindPartsEquipping(GameObject GO, List<BodyPart> Return)
	{
		_Body.FindPartsEquipping(GO, Return);
	}

	public List<BodyPart> FindPartsEquipping(GameObject GO)
	{
		List<BodyPart> list = new List<BodyPart>();
		FindPartsEquipping(GO, list);
		return list;
	}

	public BodyPart FindParentPartOf(BodyPart FindPart)
	{
		return _Body.FindParentPartOf(FindPart);
	}

	public BodyPart FindEquippedItem(GameObject GO)
	{
		return _Body.FindEquippedItem(GO);
	}

	public BodyPart FindEquippedItem(string Blueprint)
	{
		return _Body.FindEquippedItem(Blueprint);
	}

	public BodyPart FindDefaultOrEquippedItem(GameObject GO)
	{
		return _Body.FindDefaultOrEquippedItem(GO);
	}

	public GameObject FindEquipmentOrDefaultByID(string ID)
	{
		return _Body.FindEquipmentOrDefaultByID(ID);
	}

	public GameObject FindEquipmentByEvent(string ID)
	{
		return _Body.FindEquipmentByEvent(ID);
	}

	public GameObject FindEquipmentByEvent(Event E)
	{
		return _Body.FindEquipmentByEvent(E);
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(string ID)
	{
		return _Body.FindEquipmentOrCyberneticsByEvent(ID);
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(Event E)
	{
		return _Body.FindEquipmentOrCyberneticsByEvent(E);
	}

	public bool HasEquippedItem(GameObject GO)
	{
		return _Body.HasEquippedItem(GO);
	}

	public bool HasEquippedItem(string Blueprint)
	{
		return _Body.HasEquippedItem(Blueprint);
	}

	public bool IsItemEquippedOnLimbType(GameObject GO, string FindType)
	{
		return _Body.IsItemEquippedOnLimbType(GO, FindType);
	}

	public BodyPart FindCybernetics(GameObject GO)
	{
		return _Body.FindCybernetics(GO);
	}

	public bool IsItemImplantedInLimbType(GameObject GO, string FindType)
	{
		return _Body.IsItemImplantedInLimbType(GO, FindType);
	}

	public bool IsADefaultBehavior(GameObject obj)
	{
		return _Body.IsADefaultBehavior(obj);
	}

	public BodyPart FindDefaultBehavior(GameObject GO)
	{
		return _Body.FindDefaultBehavior(GO);
	}

	public GameObject FindObject(Predicate<GameObject> pFilter)
	{
		return _Body.GetPart((BodyPart P) => P.Equipped != null && pFilter(P.Equipped))?.Equipped;
	}

	public GameObject FindObjectByBlueprint(string Blueprint)
	{
		return _Body.GetPart((BodyPart P) => P.Equipped != null && P.Equipped.Blueprint == Blueprint)?.Equipped;
	}

	public void ForeachPart(Action<BodyPart> aProc)
	{
		_Body.ForeachPart(aProc);
	}

	public bool ForeachPart(Predicate<BodyPart> pProc)
	{
		return _Body.ForeachPart(pProc);
	}

	public List<BodyPart> GetEquippedParts()
	{
		int partCount = GetPartCount((BodyPart P) => P.Equipped != null);
		List<BodyPart> Return = new List<BodyPart>(partCount);
		if (partCount > 0)
		{
			_Body.ForeachPart(delegate(BodyPart P)
			{
				if (P.Equipped != null)
				{
					Return.Add(P);
				}
			});
		}
		return Return;
	}

	public void GetParts(List<BodyPart> Return)
	{
		_Body.GetParts(Return);
	}

	public void GetParts(List<BodyPart> Return, bool EvenIfDismembered)
	{
		_Body.GetParts(Return);
		if (!EvenIfDismembered || DismemberedParts == null || DismemberedParts.Count <= 0)
		{
			return;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			Return.Add(dismemberedPart.Part);
		}
		Return.Sort(BodyPart.Sort);
	}

	public List<BodyPart> GetParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		GetParts(list);
		return list;
	}

	public List<BodyPart> GetParts(bool EvenIfDismembered)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		GetParts(list, EvenIfDismembered);
		return list;
	}

	public IEnumerable<BodyPart> LoopParts()
	{
		foreach (BodyPart item in _Body.LoopParts())
		{
			yield return item;
		}
	}

	public void GetConcreteParts(List<BodyPart> Return)
	{
		_Body.GetConcreteParts(Return);
	}

	public List<BodyPart> GetConcreteParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetConcretePartCount());
		GetConcreteParts(list);
		return list;
	}

	public void GetAbstractParts(List<BodyPart> Return)
	{
		_Body.GetAbstractParts(Return);
	}

	public List<BodyPart> GetAbstractParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetAbstractPartCount());
		GetAbstractParts(list);
		return list;
	}

	public int GetPartCount()
	{
		return _Body.GetPartCount();
	}

	public int GetPartCount(string RequiredType)
	{
		return _Body.GetPartCount(RequiredType);
	}

	public int GetPartCount(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetPartCount(RequiredType, RequiredLaterality);
	}

	public int GetPartCount(Predicate<BodyPart> Filter)
	{
		return _Body.GetPartCount(Filter);
	}

	public int GetConcretePartCount()
	{
		return _Body.GetConcretePartCount();
	}

	public int GetAbstractPartCount()
	{
		return _Body.GetAbstractPartCount();
	}

	public bool AnyCategoryParts(int FindCategory)
	{
		return _Body.AnyCategoryParts(FindCategory);
	}

	public int GetCategoryPartCount(int FindCategory)
	{
		return _Body.GetCategoryPartCount(FindCategory);
	}

	public int GetCategoryPartCount(int FindCategory, string FindType)
	{
		return _Body.GetCategoryPartCount(FindCategory, FindType);
	}

	public int GetCategoryPartCount(int FindCategory, Predicate<BodyPart> pFilter)
	{
		return _Body.GetCategoryPartCount(FindCategory, pFilter);
	}

	public int GetNativePartCount()
	{
		return _Body.GetNativePartCount();
	}

	public int GetNativePartCount(string RequiredType)
	{
		return _Body.GetNativePartCount(RequiredType);
	}

	public int GetNativePartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetNativePartCount(pFilter);
	}

	public int GetAddedPartCount()
	{
		return _Body.GetAddedPartCount();
	}

	public int GetAddedPartCount(string RequiredType)
	{
		return _Body.GetAddedPartCount(RequiredType);
	}

	public int GetAddedPartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetAddedPartCount(pFilter);
	}

	public int GetMortalPartCount()
	{
		return _Body.GetMortalPartCount();
	}

	public int GetMortalPartCount(string RequiredType)
	{
		return _Body.GetMortalPartCount(RequiredType);
	}

	public int GetMortalPartCount(Predicate<BodyPart> pFilter)
	{
		return _Body.GetMortalPartCount(pFilter);
	}

	public bool AnyMortalParts()
	{
		return _Body.AnyMortalParts();
	}

	public bool IsEquippedOnType(GameObject FindObj, string FindType)
	{
		return _Body.IsEquippedOnType(FindObj, FindType);
	}

	public bool IsEquippedOnCategory(GameObject FindObj, int FindCategory)
	{
		return _Body.IsEquippedOnCategory(FindObj, FindCategory);
	}

	public bool IsEquippedOnPrimary(GameObject FindObj)
	{
		return _Body.IsEquippedOnPrimary(FindObj);
	}

	public bool IsImplantedInCategory(GameObject FindObj, int FindCategory)
	{
		return _Body.IsImplantedInCategory(FindObj, FindCategory);
	}

	public void GetMobilityProvidingParts(List<BodyPart> Return)
	{
		_Body.GetMobilityProvidingParts(Return);
	}

	public List<BodyPart> GetMobilityProvidingParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetMobilityProvidingParts(list);
		return list;
	}

	public void GetConcreteMobilityProvidingParts(List<BodyPart> Return)
	{
		_Body.GetConcreteMobilityProvidingParts(Return);
	}

	public List<BodyPart> GetConcreteMobilityProvidingParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		GetConcreteMobilityProvidingParts(list);
		return list;
	}

	public int GetTotalMobility()
	{
		return _Body.GetTotalMobility();
	}

	public int GetBodyMobility()
	{
		return _Body.Mobility;
	}

	public void MarkAllNative()
	{
		_Body.MarkAllNative();
	}

	public void CategorizeAll(int ApplyCategory)
	{
		_Body.CategorizeAll(ApplyCategory);
	}

	public void CategorizeAllExcept(int ApplyCategory, int SkipCategory)
	{
		_Body.CategorizeAllExcept(ApplyCategory, SkipCategory);
	}

	public void GetPartsEquippedOn(GameObject obj, List<BodyPart> Result)
	{
		_Body.GetPartsEquippedOn(obj, Result);
	}

	public List<BodyPart> GetPartsEquippedOn(GameObject obj)
	{
		return _Body.GetPartsEquippedOn(obj);
	}

	public int GetPartCountEquippedOn(GameObject obj)
	{
		return _Body.GetPartCountEquippedOn(obj);
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType)
	{
		List<BodyPart> list = new List<BodyPart>();
		_Body.GetUnequippedPart(RequiredType, list);
		return list;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, int RequiredLaterality)
	{
		List<BodyPart> list = new List<BodyPart>();
		_Body.GetUnequippedPart(RequiredType, RequiredLaterality, list);
		return list;
	}

	public int GetUnequippedPartCount(string RequiredType)
	{
		return _Body.GetUnequippedPartCount(RequiredType);
	}

	public int GetUnequippedPartCount(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetUnequippedPartCount(RequiredType, RequiredLaterality);
	}

	public int GetUnequippedPartCountExcept(string RequiredType, BodyPart ExceptPart)
	{
		return _Body.GetUnequippedPartCountExcept(RequiredType, ExceptPart);
	}

	public int GetUnequippedPartCountExcept(string RequiredType, int RequiredLaterality, BodyPart ExceptPart)
	{
		return _Body.GetUnequippedPartCountExcept(RequiredType, RequiredLaterality, ExceptPart);
	}

	public List<BodyPart> GetPartStatic(string RequiredType)
	{
		if (partStatic == null)
		{
			partStatic = new List<BodyPart>(4);
		}
		else
		{
			partStatic.Clear();
		}
		_Body.GetPart(RequiredType, partStatic);
		return partStatic;
	}

	public List<BodyPart> GetPart(string RequiredType)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType));
		_Body.GetPart(RequiredType, list);
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType);
		}
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType, RequiredLaterality));
		_Body.GetPart(RequiredType, RequiredLaterality, list);
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, bool EvenIfDismembered)
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType));
		_Body.GetPart(RequiredType, list);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType)
				{
					list.Add(dismemberedPart.Part);
				}
			}
			return list;
		}
		return list;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType, EvenIfDismembered);
		}
		List<BodyPart> list = new List<BodyPart>(GetPartCount(RequiredType, RequiredLaterality));
		_Body.GetPart(RequiredType, RequiredLaterality, list);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if ((dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType) && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
				{
					list.Add(dismemberedPart.Part);
				}
			}
			return list;
		}
		return list;
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType)
	{
		foreach (BodyPart item in _Body.LoopPart(RequiredType))
		{
			yield return item;
		}
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType, int RequiredLaterality)
	{
		foreach (BodyPart item in _Body.LoopPart(RequiredType, RequiredLaterality))
		{
			yield return item;
		}
	}

	public BodyPart GetFirstPart()
	{
		return _Body;
	}

	public BodyPart GetFirstPart(string RequiredType)
	{
		return _Body.GetFirstPart(RequiredType);
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetFirstPart(RequiredType, RequiredLaterality);
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(RequiredType, Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstPart(RequiredType, RequiredLaterality, Filter);
	}

	public BodyPart GetFirstPart(string RequiredType, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num)
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstPart(RequiredType, RequiredLaterality, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality) && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType)
	{
		return _Body.GetFirstVariantPart(RequiredType);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality)
	{
		return _Body.GetFirstVariantPart(RequiredType, RequiredLaterality);
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(RequiredType, Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return _Body.GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
	}

	public BodyPart GetFirstVariantPart(string RequiredType, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num)
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		BodyPart result = _Body.GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
		if (EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.VariantType == RequiredType && dismemberedPart.Part.Position < num && Laterality.Match(dismemberedPart.Part, RequiredLaterality) && Filter(dismemberedPart.Part))
					{
						result = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return result;
			}
		}
		return result;
	}

	public bool HasPart(string RequiredType)
	{
		return GetFirstPart(RequiredType) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality)
	{
		return GetFirstPart(RequiredType, RequiredLaterality) != null;
	}

	public bool HasPart(Predicate<BodyPart> Filter)
	{
		return GetFirstPart(Filter) != null;
	}

	public bool HasPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return GetFirstPart(RequiredType, Filter) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, Filter) != null;
	}

	public bool HasPart(string RequiredType, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, EvenIfDismembered) != null;
	}

	public bool HasPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(Filter, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, Filter, EvenIfDismembered) != null;
	}

	public bool HasPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstPart(RequiredType, RequiredLaterality, Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType)
	{
		return GetFirstVariantPart(RequiredType) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality) != null;
	}

	public bool HasVariantPart(Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(RequiredType, Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, Filter) != null;
	}

	public bool HasVariantPart(string RequiredType, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, Filter, EvenIfDismembered) != null;
	}

	public bool HasVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return GetFirstVariantPart(RequiredType, RequiredLaterality, Filter, EvenIfDismembered) != null;
	}

	public BodyPart GetPartByName(string RequiredPart)
	{
		return _Body.GetPartByName(RequiredPart);
	}

	public BodyPart GetPartByName(string RequiredPart, bool EvenIfDismembered)
	{
		BodyPart bodyPart = _Body.GetPartByName(RequiredPart);
		if (bodyPart == null && EvenIfDismembered && DismemberedParts != null)
		{
			int num = int.MaxValue;
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.Type == RequiredPart && dismemberedPart.Part.Position < num)
					{
						bodyPart = dismemberedPart.Part;
						num = dismemberedPart.Part.Position;
					}
				}
				return bodyPart;
			}
		}
		return bodyPart;
	}

	public BodyPart GetPartByNameStartsWith(string RequiredPart)
	{
		return _Body.GetPartByNameStartsWith(RequiredPart);
	}

	public BodyPart GetPartByNameWithoutCybernetics(string RequiredPart)
	{
		return _Body.GetPartByNameWithoutCybernetics(RequiredPart);
	}

	public BodyPart GetPartByDescription(string RequiredPart)
	{
		return _Body.GetPartByDescription(RequiredPart);
	}

	public BodyPart GetPartByDescriptionStartsWith(string RequiredPart)
	{
		return _Body.GetPartByDescriptionStartsWith(RequiredPart);
	}

	public bool RemoveUnmanagedPartsByVariantPrefix(string Prefix)
	{
		if (_Body.RemoveUnmanagedPartsByVariantPrefix(Prefix))
		{
			UpdateBodyParts();
			RecalculateArmor();
			return true;
		}
		return false;
	}

	public bool RemovePart(BodyPart removePart, bool DoUpdate = true)
	{
		bool result = false;
		if (_Body.RemovePart(removePart, DoUpdate))
		{
			result = true;
		}
		if (DismemberedParts != null)
		{
			bool flag = false;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part == removePart)
				{
					flag = true;
					result = true;
					break;
				}
			}
			if (flag)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					if (item.Part == removePart)
					{
						DismemberedParts.Remove(item);
					}
					else if (item.ParentID == removePart.ID)
					{
						RemovePart(item.Part, DoUpdate: false);
					}
				}
				return result;
			}
		}
		return result;
	}

	public bool RemovePartByID(int removeID)
	{
		bool result = false;
		if (_Body.RemovePartByID(removeID))
		{
			return true;
		}
		if (DismemberedParts != null)
		{
			bool flag = false;
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.idMatch(removeID))
				{
					flag = true;
					result = true;
					break;
				}
			}
			if (flag)
			{
				foreach (DismemberedPart item in new List<DismemberedPart>(DismemberedParts))
				{
					if (item.Part.idMatch(removeID))
					{
						DismemberedParts.Remove(item);
					}
				}
				return result;
			}
		}
		return result;
	}

	public BodyPart GetPartByID(int findID, bool EvenIfDismembered = false)
	{
		BodyPart partByID = _Body.GetPartByID(findID);
		if (partByID != null)
		{
			return partByID;
		}
		if (EvenIfDismembered && DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.idMatch(findID))
				{
					return dismemberedPart.Part;
				}
			}
		}
		return null;
	}

	public List<BodyPart> GetPartsBySupportsDependent(string findSupportsDependent)
	{
		_Body.GetPartBySupportsDependent(findSupportsDependent, parts2);
		return parts2;
	}

	public BodyPart GetPartBySupportsDependent(string findSupportsDependent)
	{
		return _Body.GetPartBySupportsDependent(findSupportsDependent);
	}

	public bool CheckSlotEquippedMatch(GameObject obj, string SlotSpec)
	{
		if (SlotSpec.IndexOf(',') != -1)
		{
			return _Body.CheckSlotEquippedMatch(obj, SlotSpec.CachedCommaExpansion());
		}
		return _Body.CheckSlotEquippedMatch(obj, SlotSpec);
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, string SlotSpec)
	{
		if (SlotSpec.IndexOf(',') != -1)
		{
			return _Body.CheckSlotCyberneticsMatch(obj, SlotSpec.CachedCommaExpansion());
		}
		return _Body.CheckSlotCyberneticsMatch(obj, SlotSpec);
	}

	public bool HasReadyMissileWeapon()
	{
		return _Body.HasReadyMissileWeapon();
	}

	public bool HasMissileWeapon()
	{
		return _Body.HasMissileWeapon();
	}

	public void GetMissileWeapons(List<GameObject> List)
	{
		_Body.GetMissileWeapons(ref List);
	}

	public List<GameObject> GetMissileWeapons()
	{
		List<GameObject> List = null;
		_Body.GetMissileWeapons(ref List);
		return List;
	}

	public BodyPart GetDismemberedPartByID(int ID)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.idMatch(ID))
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartByType(string RequiredType)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.Type == RequiredType || dismemberedPart.Part.VariantType == RequiredType)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartBySupportsDependent(string RequiredSupportsDependent)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.SupportsDependent == RequiredSupportsDependent)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public BodyPart GetDismemberedPartByDependsOn(string RequiredDependsOn)
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.DependsOn == RequiredDependsOn)
			{
				return dismemberedPart.Part;
			}
		}
		return null;
	}

	public bool ValidateDismemberedParentPart(BodyPart Parent, BodyPart Child)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part == Child && Parent.idMatch(dismemberedPart.ParentID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart GetFirstDismemberedPartByType(BodyPart Parent, string RequiredType)
	{
		BodyPart bodyPart = null;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == RequiredType && (bodyPart == null || dismemberedPart.Part.Position < bodyPart.Position))
				{
					bodyPart = dismemberedPart.Part;
				}
			}
			return bodyPart;
		}
		return bodyPart;
	}

	public BodyPart GetFirstDismemberedPartByTypeAndLaterality(BodyPart Parent, string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstDismemberedPartByType(Parent, RequiredType);
		}
		BodyPart bodyPart = null;
		if (DismemberedParts != null)
		{
			if (RequiredLaterality == 0)
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == RequiredType && dismemberedPart.Part.Laterality == 0 && (bodyPart == null || dismemberedPart.Part.Position < bodyPart.Position))
					{
						bodyPart = dismemberedPart.Part;
					}
				}
				return bodyPart;
			}
			{
				foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
				{
					if (Parent.idMatch(dismemberedPart2.ParentID) && dismemberedPart2.Part.Type == RequiredType && (dismemberedPart2.Part.Laterality & RequiredLaterality) == RequiredLaterality && (bodyPart == null || dismemberedPart2.Part.Position < bodyPart.Position))
					{
						bodyPart = dismemberedPart2.Part;
					}
				}
				return bodyPart;
			}
		}
		return bodyPart;
	}

	public bool IsDismembered(BodyPart checkPart)
	{
		if (DismemberedParts == null)
		{
			return false;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part == checkPart)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPartOrDismemberedPart(string partName)
	{
		if (GetPartByName(partName) != null)
		{
			return true;
		}
		if (HasDismemberedPartNamed(partName))
		{
			return true;
		}
		return false;
	}

	public bool HasDismemberedPartNamed(string RequiredPart)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Name == RequiredPart || dismemberedPart.Part.Description == RequiredPart)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyDismemberedMortalParts()
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.Mortal)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetFirstDismemberedPartPosition(BodyPart Parent, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position < num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
			return num;
		}
		return num;
	}

	public int GetLastDismemberedPartPosition(BodyPart Parent, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position > num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
			return num;
		}
		return num;
	}

	public int GetFirstDismemberedPartTypePosition(BodyPart Parent, string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == Type && dismemberedPart.Part != ExceptPart && (num == -1 || dismemberedPart.Part.Position < num))
				{
					num = dismemberedPart.Part.Position;
				}
			}
			return num;
		}
		return num;
	}

	public int GetLastDismemberedPartTypePosition(BodyPart Parent, string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (Parent.idMatch(dismemberedPart.ParentID) && dismemberedPart.Part.Type == Type && dismemberedPart.Part != ExceptPart && dismemberedPart.Part.Position > num)
				{
					num = dismemberedPart.Part.Position;
				}
			}
			return num;
		}
		return num;
	}

	public DismemberedPart FindRegenerablePart()
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.IsRegenerable() && dismemberedPart.IsReattachable(this))
				{
					return dismemberedPart;
				}
			}
		}
		return null;
	}

	public DismemberedPart FindRegenerablePart(int ParentID)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.IsRegenerable() && dismemberedPart.ParentID == ParentID && dismemberedPart.IsReattachable(this))
				{
					return dismemberedPart;
				}
			}
		}
		return null;
	}

	public List<DismemberedPart> FindRecoverableParts()
	{
		if (DismemberedParts == null)
		{
			return null;
		}
		List<DismemberedPart> list = null;
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.IsRecoverable(this) && dismemberedPart.IsReattachable(this))
			{
				if (list == null)
				{
					list = new List<DismemberedPart>(1) { dismemberedPart };
				}
				else
				{
					list.Add(dismemberedPart);
				}
			}
		}
		return list;
	}

	public int DismemberAndPreventRegenerationBySupportsDependent(string SupportsDependent, string Because)
	{
		int num = 0;
		foreach (BodyPart part in GetParts())
		{
			if (part.SupportsDependent == SupportsDependent)
			{
				part.PreventRegenerationBecause = Because;
				Dismember(part);
				num++;
			}
		}
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.SupportsDependent == SupportsDependent)
				{
					dismemberedPart.Part.PreventRegenerationBecause = Because;
					num++;
				}
			}
			return num;
		}
		return num;
	}

	public int RestoreRegenerationBySupportsDependentAndReason(string SupportsDependent, string Because)
	{
		int num = 0;
		foreach (BodyPart part in GetParts())
		{
			if (part.SupportsDependent == SupportsDependent && part.PreventRegenerationBecause == Because)
			{
				part.PreventRegenerationBecause = null;
				num++;
			}
		}
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.SupportsDependent == SupportsDependent && dismemberedPart.Part.PreventRegenerationBecause == Because)
				{
					dismemberedPart.Part.PreventRegenerationBecause = null;
					num++;
				}
			}
			return num;
		}
		return num;
	}

	public int RestoreRegenerationByReason(string Because)
	{
		int num = 0;
		foreach (BodyPart part in GetParts())
		{
			if (part.PreventRegenerationBecause == Because)
			{
				part.PreventRegenerationBecause = null;
				num++;
			}
		}
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.Part.PreventRegenerationBecause == Because)
				{
					dismemberedPart.Part.PreventRegenerationBecause = null;
					num++;
				}
			}
			return num;
		}
		return num;
	}

	public void CheckDismemberedPartRenumberingOnPositionAssignment(BodyPart Parent, int AssignedPosition, BodyPart ExceptPart = null)
	{
		if (DismemberedParts == null)
		{
			return;
		}
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			dismemberedPart.CheckRenumberingOnPositionAssignment(Parent, AssignedPosition, ExceptPart);
		}
	}

	public bool DismemberedPartHasPosition(BodyPart Parent, int Position, BodyPart ExceptPart = null)
	{
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart in DismemberedParts)
			{
				if (dismemberedPart.HasPosition(Parent, Position, ExceptPart))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetTypeArmorInfo(string ForType, ref GameObject First, ref int Count, ref int AV, ref int DV)
	{
		_Body.GetTypeArmorInfo(ForType, ref First, ref Count, ref AV, ref DV);
	}

	public void RecalculateArmor()
	{
		if (built)
		{
			_Body.RecalculateArmor();
		}
	}

	public void RecalculateArmorExcept(GameObject obj)
	{
		if (built)
		{
			_Body.RecalculateArmorExcept(obj);
		}
	}

	public void RecalculateTypeArmor(string ForType)
	{
		if (built)
		{
			_Body.RecalculateTypeArmor(ForType);
		}
	}

	public void RecalculateTypeArmorExcept(string ForType, GameObject obj)
	{
		if (built)
		{
			_Body.RecalculateTypeArmorExcept(ForType, obj);
		}
	}

	public void RecalculateFirsts()
	{
		if (built)
		{
			bool PrimarySet = false;
			bool HasPreferredPrimary = false;
			BodyPart CurrentPrimaryPart = null;
			ForeachPart(delegate(BodyPart p)
			{
				p.Primary = false;
				return true;
			});
			ForeachPart(delegate(BodyPart p)
			{
				p.DefaultPrimary = false;
				return true;
			});
			_Body.SetPrimaryScan(GetPrimaryLimbType(), ref PrimarySet, ref HasPreferredPrimary, ref CurrentPrimaryPart);
			if (CurrentPrimaryPart == null)
			{
				CurrentPrimaryPart = _Body;
				_Body.DefaultPrimary = true;
				_Body.Primary = true;
			}
			if (!HasPreferredPrimary && CurrentPrimaryPart != null)
			{
				CurrentPrimaryPart.PreferedPrimary = true;
			}
			ParentObject.FireEvent("PrimaryLimbRecalculated");
		}
	}

	public void CheckUnsupportedPartLoss()
	{
		List<BodyPart> list = _Body.FindUnsupportedParts();
		if (list == null)
		{
			return;
		}
		foreach (BodyPart item in list)
		{
			if (ParentObject.IsPlayer())
			{
				string ordinalName = item.GetOrdinalName();
				CutAndQueueForRegeneration(item);
				if (item.IsConcretelyDependent())
				{
					IComponent<GameObject>.AddPlayerMessage("You have lost all of your " + ordinalName + ".", 'R');
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("You have lost the use of your " + ordinalName + ".", 'R');
				}
			}
			else
			{
				CutAndQueueForRegeneration(item);
			}
		}
	}

	public void CheckPartRecovery()
	{
		List<DismemberedPart> list = FindRecoverableParts();
		if (list == null)
		{
			return;
		}
		foreach (DismemberedPart item in list)
		{
			item.Reattach(this);
			DismemberedParts.Remove(item);
			if (ParentObject.IsPlayer() && item.Part.IsAbstractlyDependent())
			{
				IComponent<GameObject>.AddPlayerMessage("You have recovered the use of your " + item.Part.GetOrdinalName() + ".", 'G');
			}
		}
	}

	public void RegenerateDefaultEquipment()
	{
		GetParts(_bodyParts);
		try
		{
			foreach (BodyPart part in GetParts())
			{
				if (part.DefaultBehavior != null)
				{
					GameObject defaultBehavior = part.DefaultBehavior;
					part.DefaultBehavior = null;
					defaultBehavior.Obliterate();
				}
				if (part.DefaultBehaviorBlueprint != null)
				{
					part.DefaultBehavior = GameObject.createUnmodified(part.DefaultBehaviorBlueprint);
				}
			}
			ParentObject.FireEvent(Event.New("RegenerateDefaultEquipment", "Body", this));
			ParentObject.FireEvent(eDecorateDefaultEquipment);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("RegenerateDefaultEquipment", x);
		}
		finally
		{
			_bodyParts.Clear();
		}
	}

	public void UpdateBodyParts(int Depth = 0)
	{
		if (built)
		{
			RegenerateDefaultEquipment();
			RecalculateFirsts();
			UpdateMobilitySpeedPenalty();
			CheckUnsupportedPartLoss();
			CheckPartRecovery();
			FireEventOnBodyparts(eBodypartsUpdated);
		}
	}

	public void CutAndQueueForRegeneration(BodyPart Part)
	{
		BodyPart parentPart = Part.GetParentPart();
		DismemberedPart dismemberedPart = (Part.Extrinsic ? null : new DismemberedPart(Part, parentPart));
		if (Part.Parts != null && Part.Parts.Count > 0)
		{
			if (Part.Parts.Count > 1)
			{
				foreach (BodyPart item in new List<BodyPart>(Part.Parts))
				{
					CutAndQueueForRegeneration(item);
				}
			}
			else
			{
				CutAndQueueForRegeneration(Part.Parts[0]);
			}
			Part.Parts = null;
		}
		parentPart?.RemovePart(Part, DoUpdate: false);
		if (dismemberedPart != null)
		{
			if (DismemberedParts == null)
			{
				DismemberedParts = new List<DismemberedPart>(1) { dismemberedPart };
			}
			else
			{
				DismemberedParts.Add(dismemberedPart);
			}
		}
		Part.ParentBody = null;
	}

	/// Determines the speed penalty we should have based on our current
	/// intact and dismembered mobility-providing limbs.
	public int CalculateMobilitySpeedPenalty(out bool AnyDismembered)
	{
		AnyDismembered = false;
		if (ParentObject.IsFlying)
		{
			return 0;
		}
		int totalMobility = GetTotalMobility();
		if (totalMobility == 0)
		{
			if (DismemberedParts != null)
			{
				foreach (DismemberedPart dismemberedPart in DismemberedParts)
				{
					if (dismemberedPart.Part.GetTotalMobility() > 0)
					{
						AnyDismembered = true;
						break;
					}
				}
			}
			return 60;
		}
		int num = 0;
		if (DismemberedParts != null)
		{
			foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
			{
				num += dismemberedPart2.Part.GetTotalMobility();
			}
		}
		if (num > 0)
		{
			AnyDismembered = true;
		}
		if (totalMobility >= 2)
		{
			if (num == 0)
			{
				return 0;
			}
			return 60 * num / (totalMobility + num + 2);
		}
		if (num == 0)
		{
			return 60 * totalMobility / 2;
		}
		return 60 * num / (totalMobility + num);
	}

	public int CalculateMobilitySpeedPenalty()
	{
		bool AnyDismembered;
		return CalculateMobilitySpeedPenalty(out AnyDismembered);
	}

	public void UpdateMobilitySpeedPenalty()
	{
		bool AnyDismembered;
		int num = CalculateMobilitySpeedPenalty(out AnyDismembered);
		if (num == MobilitySpeedPenaltyApplied || !ParentObject.Statistics.ContainsKey("MoveSpeed"))
		{
			return;
		}
		ParentObject.Statistics["MoveSpeed"].Bonus -= MobilitySpeedPenaltyApplied;
		ParentObject.Statistics["MoveSpeed"].Bonus += num;
		MobilitySpeedPenaltyApplied = num;
		if (MobilitySpeedPenaltyApplied == 0)
		{
			ParentObject.RemoveEffect("MobilityImpaired");
		}
		else if (AnyDismembered)
		{
			if (ParentObject.HasEffect("MobilityImpaired"))
			{
				(ParentObject.GetEffect("MobilityImpaired") as MobilityImpaired).Amount = MobilitySpeedPenaltyApplied;
			}
			else
			{
				ParentObject.ApplyEffect(new MobilityImpaired(MobilitySpeedPenaltyApplied));
			}
		}
	}

	public GameObject Dismember(BodyPart Part, Cell Where = null, bool obliterate = false)
	{
		if (!ParentObject.FireEvent(Event.New("Dismember", "Part", Part)) && !obliterate)
		{
			return null;
		}
		ParentObject.StopMoving();
		GameObject gameObject = null;
		GameObject gameObject2 = Part.Unimplant();
		Part.UnequipPartAndChildren(Silent: false, Where);
		string text = null;
		string text2 = null;
		bool flag = false;
		Cell cell = Where ?? ParentObject.GetDropCell();
		if (!Part.Extrinsic && (cell == null || !cell.IsGraveyard()) && !obliterate)
		{
			BodyPartType bodyPartType = Part.TypeModel();
			gameObject = GameObject.create(ParentObject.GetPropertyOrTag(bodyPartType.LimbBlueprintProperty, bodyPartType.LimbBlueprintDefault), 0, 0, "Dismember");
			string displayName = ParentObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: true, NoColor: true, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: false, BaseOnly: true);
			if (!gameObject.HasPropertyOrTag("SeveredLimbKeepAppearance"))
			{
				Render pRender = gameObject.pRender;
				text = Part.Name;
				text2 = Part.GetOrdinalName();
				flag = Part.Plural;
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(Grammar.MakePossessive(displayName)).Append(' ');
				string color = BodyPartCategory.GetColor(Part.Category);
				if (color != null)
				{
					stringBuilder.Append("{{").Append(color).Append('|');
				}
				stringBuilder.Append(text);
				if (color != null)
				{
					stringBuilder.Append("}}");
				}
				pRender.DisplayName = stringBuilder.ToString();
				gameObject.GetPart<Description>().Short = ParentObject.A + Grammar.MakePossessive(displayName) + " severed " + text + ".";
				if (ParentObject.HasPropertyOrTag("SeveredLimbColorString"))
				{
					gameObject.pRender.ColorString = ParentObject.GetPropertyOrTag("SeveredLimbColorString");
				}
				if (ParentObject.HasPropertyOrTag("SeveredLimbTileColor"))
				{
					gameObject.pRender.TileColor = ParentObject.GetPropertyOrTag("SeveredLimbTileColor");
				}
				if (ParentObject.HasPropertyOrTag("SeveredLimbDetailColor"))
				{
					gameObject.pRender.DetailColor = ParentObject.GetPropertyOrTag("SeveredLimbDetailColor");
				}
			}
			gameObject.SetStringProperty("LimbSourceGameObjectID", ParentObject.id);
			gameObject.SetStringProperty("LimbSourceBodyPartType", Part.Type);
			gameObject.SetIntProperty("LimbSourceBodyPartID", Part.ID);
			gameObject.SetIntProperty("LimbSourceBodyPartCategory", Part.Category);
			gameObject.SetStringProperty("LimbSourceBodyPartCategoryName", BodyPartCategory.GetName(Part.Category));
			if (Part.Category == _Body.Category)
			{
				string genotype = ParentObject.GetGenotype();
				if (!string.IsNullOrEmpty(genotype))
				{
					gameObject.SetStringProperty("FromGenotype", genotype);
				}
			}
			if (ParentObject.IsPlayer())
			{
				gameObject.AddPart(new EatenAchievement("ACH_EAT_OWN_LIMB"));
			}
			if (Part.Category == 18)
			{
				gameObject.AddPart(new CookedAchievement("ACH_COOKED_EXTRADIMENSIONAL"));
			}
			if (gameObject2 != null)
			{
				gameObject2.RemoveFromContext();
				gameObject.AddPart(new CyberneticsButcherableCybernetic(gameObject2));
				gameObject.RemovePart("Food");
			}
			if (Part.Type == "Face")
			{
				Armor armor = gameObject.RequirePart<Armor>();
				armor.WornOn = "Face";
				if (ParentObject.IsPlayer() || ParentObject.HasProperty("PlayerCopy"))
				{
					gameObject.AddPart(new EquippedAchievement("ACH_WEAR_OWN_FACE"));
				}
				int value = ParentObject.Statistics["Level"]._Value;
				int num = (armor.Ego = ((value < 20) ? 1 : ((value >= 35) ? 3 : 2)));
				foreach (string key in ParentObject.pBrain.FactionMembership.Keys)
				{
					if (Factions.get(key).Visible)
					{
						AddsRep.AddModifier(gameObject, key, -500);
					}
				}
			}
			Temporary.CarryOver(ParentObject, gameObject);
			Phase.carryOver(ParentObject, gameObject);
			cell?.AddObject(gameObject);
		}
		if (ParentObject.IsPlayer() && text != null && !obliterate)
		{
			Popup.Show("Your " + text2 + " " + (flag ? "are" : "is") + " dismembered!");
			JournalAPI.AddAccomplishment("Your " + text2 + " " + (flag ? "were" : "was") + " dismembered.", "While fighting a battle to protect the practice of " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".practices.!random>") + ", =name= valorously had " + The.Player.GetPronounProvider().PossessiveAdjective + " " + text2 + " dismembered.", "general", JournalAccomplishment.MuralCategory.BodyExperienceBad, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		if (!obliterate)
		{
			CutAndQueueForRegeneration(Part);
		}
		UpdateBodyParts();
		RecalculateArmor();
		return gameObject;
	}

	public bool RegenerateLimb(bool WholeLimb = false, DismemberedPart Part = null, bool DoUpdate = true)
	{
		if (Part == null)
		{
			Part = FindRegenerablePart();
			if (Part == null)
			{
				return false;
			}
		}
		Part.Reattach(this);
		DismemberedParts.Remove(Part);
		if (ParentObject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You regenerate your " + Part.Part.GetOrdinalName() + "!", 'G');
			AchievementManager.SetAchievement("ACH_REGENERATE_LIMB");
		}
		else if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("regenerate") + " " + ParentObject.its + " " + Part.Part.GetOrdinalName() + "!", ColorCoding.ConsequentialColor(ParentObject));
		}
		if (WholeLimb && Part.Part.HasID())
		{
			DismemberedPart dismemberedPart = null;
			while ((dismemberedPart = FindRegenerablePart(Part.Part.ID)) != null)
			{
				RegenerateLimb(WholeLimb, dismemberedPart, DoUpdate: false);
			}
		}
		if (DoUpdate)
		{
			UpdateBodyParts();
		}
		return true;
	}

	private List<string> SummarizeMissingBodyParts(List<BodyPart> Parts)
	{
		Dictionary<string, int> Counts = new Dictionary<string, int>(Parts.Count);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>(Parts.Count);
		foreach (BodyPart Part in Parts)
		{
			string name = Part.Name;
			if (Counts.ContainsKey(name))
			{
				Counts[name]++;
				continue;
			}
			Counts.Add(name, 1);
			dictionary[name] = Part.Plural;
		}
		List<string> list = new List<string>(Counts.Keys);
		list.Sort(delegate(string n1, string n2)
		{
			int num2 = Counts[n1].CompareTo(Counts[n2]);
			return (num2 != 0) ? (-num2) : n1.CompareTo(n2);
		});
		List<string> list2 = new List<string>(list.Count);
		foreach (string item in list)
		{
			int num = Counts[item];
			if (num == 1)
			{
				if (GetPartByName(item) == null)
				{
					list2.Add(ParentObject.its + " " + item);
				}
				else if (dictionary[item])
				{
					list2.Add("a set of " + item);
				}
				else
				{
					list2.Add(Grammar.A(item));
				}
			}
			else if (dictionary[item])
			{
				list2.Add(Grammar.Cardinal(num) + " sets of " + item);
			}
			else
			{
				list2.Add(Grammar.Cardinal(num) + " " + Grammar.Pluralize(item));
			}
		}
		return list2;
	}

	public bool GetMissingLimbsDescription(StringBuilder SB, bool PrependNewlineIfContent = false)
	{
		if (DismemberedParts == null)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (DismemberedPart dismemberedPart in DismemberedParts)
		{
			if (dismemberedPart.Part.Abstract)
			{
				num2++;
			}
			else if (dismemberedPart.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart.Part.SupportsDependent) == null)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return false;
		}
		if (PrependNewlineIfContent)
		{
			SB.Append('\n');
		}
		if (num2 > 0)
		{
			List<BodyPart> list = new List<BodyPart>(num);
			List<BodyPart> list2 = new List<BodyPart>(num2);
			foreach (DismemberedPart dismemberedPart2 in DismemberedParts)
			{
				if (dismemberedPart2.Part.Abstract)
				{
					list2.Add(dismemberedPart2.Part);
				}
				else if (dismemberedPart2.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart2.Part.SupportsDependent) == null)
				{
					list.Add(dismemberedPart2.Part);
				}
			}
			List<string> words = SummarizeMissingBodyParts(list);
			List<string> words2 = SummarizeMissingBodyParts(list2);
			SB.Append(ParentObject.Itis).Append(" missing ").Append(Grammar.MakeAndList(words))
				.Append(", and so ")
				.Append(ParentObject.it)
				.Append(ParentObject.GetVerb("do", PrependSpace: true, PronounAntecedent: true))
				.Append(" not have the use of ")
				.Append(Grammar.MakeOrList(words2))
				.Append('.');
		}
		else
		{
			List<BodyPart> list3 = new List<BodyPart>(num);
			foreach (DismemberedPart dismemberedPart3 in DismemberedParts)
			{
				if (!dismemberedPart3.Part.Abstract && (dismemberedPart3.Part.SupportsDependent == null || GetDismemberedPartByDependsOn(dismemberedPart3.Part.SupportsDependent) == null))
				{
					list3.Add(dismemberedPart3.Part);
				}
			}
			List<string> words3 = SummarizeMissingBodyParts(list3);
			SB.Append(ParentObject.Itis).Append(" missing ").Append(Grammar.MakeAndList(words3))
				.Append('.');
		}
		return true;
	}

	public string GetMissingLimbsDescription(bool PrependNewlineIfContent = false)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		GetMissingLimbsDescription(stringBuilder, PrependNewlineIfContent);
		if (stringBuilder.Length != 0)
		{
			return stringBuilder.ToString();
		}
		return null;
	}

	public bool Rebuild(string AsAnatomy)
	{
		List<BodyPart> parts = GetParts();
		List<GameObject> list = Event.NewGameObjectList();
		Dictionary<GameObject, string> dictionary = new Dictionary<GameObject, string>(parts.Count);
		List<GameObject> list2 = Event.NewGameObjectList();
		Dictionary<GameObject, bool> dictionary2 = new Dictionary<GameObject, bool>(parts.Count);
		List<GameObject> list3 = Event.NewGameObjectList();
		List<GameObject> list4 = null;
		foreach (BodyPart item in parts)
		{
			GameObject cybernetics = item.Cybernetics;
			if (cybernetics != null && !list.Contains(cybernetics))
			{
				if (cybernetics.IsStackable())
				{
					cybernetics.SetIntProperty("NeverStack", 1);
				}
				list.Add(cybernetics);
				dictionary.Add(cybernetics, item.Type);
				item.Unimplant();
			}
		}
		foreach (BodyPart item2 in parts)
		{
			GameObject equipped = item2.Equipped;
			if (equipped == null || equipped.IsInGraveyard())
			{
				continue;
			}
			if (equipped.IsStackable())
			{
				equipped.SetIntProperty("NeverStack", 1);
				if (item2.TryUnequip(Silent: true, SemiForced: true))
				{
					list2.Add(equipped);
					dictionary2.Add(equipped, item2.Type == "Hand");
					list3.Add(equipped);
				}
				else
				{
					equipped.RemoveIntProperty("NeverStack");
				}
			}
			else if (item2.TryUnequip(Silent: true, SemiForced: true))
			{
				list2.Add(equipped);
				dictionary2.Add(equipped, item2.Type == "Hand");
			}
		}
		List<BaseMutation> list5 = ParentObject.GetPart<Mutations>()?.ActiveMutationList;
		List<BaseMutation> list6 = null;
		if (list5 != null)
		{
			foreach (BaseMutation item3 in list5)
			{
				if (item3.AffectsBodyParts() || item3.GeneratesEquipment())
				{
					item3.Unmutate(item3.ParentObject);
					if (list6 == null)
					{
						list6 = new List<BaseMutation>(3) { item3 };
					}
					else
					{
						list6.Add(item3);
					}
				}
			}
		}
		foreach (BodyPart part2 in GetParts())
		{
			GameObject equipped2 = part2.Equipped;
			if (equipped2 == null || equipped2.IsInGraveyard() || list2.Contains(equipped2))
			{
				continue;
			}
			bool flag = equipped2.IsStackable();
			if (flag)
			{
				equipped2.SetIntProperty("NeverStack", 1);
			}
			if (part2.ForceUnequip(Silent: true))
			{
				list2.Add(equipped2);
				dictionary2.Add(equipped2, part2.Type == "Hand");
				if (flag)
				{
					list3.Add(equipped2);
				}
				if (!equipped2.HasPropertyOrTag("CursedBodyRebuildAllowUnequip"))
				{
					if (list4 == null)
					{
						list4 = new List<GameObject>(3) { equipped2 };
					}
					else
					{
						list4.Add(equipped2);
					}
				}
			}
			else if (flag)
			{
				equipped2.RemoveIntProperty("NeverStack");
			}
		}
		Anatomy = AsAnatomy;
		if (list6 != null)
		{
			ActivatedAbilities part = ParentObject.GetPart<ActivatedAbilities>();
			bool flag2 = false;
			if (part != null)
			{
				flag2 = part.Silent;
				if (!flag2)
				{
					part.Silent = true;
				}
			}
			try
			{
				foreach (BaseMutation item4 in list6)
				{
					item4.Mutate(item4.ParentObject, item4.Level);
				}
			}
			finally
			{
				if (part != null && !flag2)
				{
					part.Silent = false;
				}
			}
		}
		if (list.Count > 0)
		{
			List<GameObject> list7 = null;
			foreach (GameObject item5 in list)
			{
				if (item5.IsInGraveyard())
				{
					continue;
				}
				bool flag3 = false;
				foreach (BodyPart item6 in GetPart(dictionary[item5]))
				{
					if (item6.Cybernetics == null)
					{
						item5.RemoveFromContext();
						item6.Implant(item5);
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					if (list7 == null)
					{
						list7 = new List<GameObject>(list.Count) { item5 };
					}
					else
					{
						list7.Add(item5);
					}
				}
			}
			if (list7 != null)
			{
				foreach (GameObject item7 in list7)
				{
					string[] array = item7.GetPart<CyberneticsBaseItem>().Slots.Split(',');
					int num = 0;
					while (true)
					{
						if (num < array.Length)
						{
							string requiredType = array[num];
							foreach (BodyPart item8 in GetPart(requiredType))
							{
								if (item8.Cybernetics == null)
								{
									item7.RemoveFromContext();
									item8.Implant(item7);
									goto end_IL_0507;
								}
							}
							num++;
							continue;
						}
						if (item7.HasTag("CyberneticsNoRemove") || item7.HasTag("CyberneticsDestroyOnRemoval"))
						{
							item7.Destroy();
						}
						break;
						continue;
						end_IL_0507:
						break;
					}
				}
			}
		}
		if (list2.Count > 0)
		{
			foreach (GameObject item9 in list2)
			{
				ParentObject.AutoEquip(item9, Forced: true, dictionary2[item9], Silent: true);
			}
		}
		foreach (GameObject item10 in list3)
		{
			if (!item10.IsInvalid() && !item10.IsInGraveyard())
			{
				item10.RemoveIntProperty("NeverStack");
				item10.CheckStack();
			}
		}
		if (list4 != null)
		{
			foreach (GameObject item11 in list4)
			{
				if (!item11.IsInGraveyard() && (item11.pPhysics == null || item11.pPhysics.Equipped == null))
				{
					item11.Destroy();
				}
			}
		}
		foreach (BodyPart item12 in parts)
		{
			item12._Equipped = null;
			item12._Cybernetics = null;
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetDefensiveItemList");
		Object.RegisterPartEvent(this, "AIGetOffensiveItemList");
		Object.RegisterPartEvent(this, "Dismember");
		base.Register(Object);
	}

	public bool FireEventOnBodyparts(Event E)
	{
		return _Body.FireEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		switch (E.ID)
		{
		case "EndTurn":
		case "AIGetOffensiveItemList":
		case "AIGetDefensiveItemList":
		case "BeforeDeathRemoval":
		case "Dismember":
			if (!FireEventOnBodyparts(E))
			{
				return false;
			}
			break;
		}
		return base.FireEvent(E);
	}

	public override bool WantTurnTick()
	{
		return _Body.WantTurnTick();
	}

	public override void TurnTick(long TurnNumber)
	{
		_Body.TurnTick(TurnNumber);
	}

	public override bool WantTenTurnTick()
	{
		return _Body.WantTenTurnTick();
	}

	public override void TenTurnTick(long TurnNumber)
	{
		_Body.TenTurnTick(TurnNumber);
	}

	public override bool WantHundredTurnTick()
	{
		return _Body.WantHundredTurnTick();
	}

	public override void HundredTurnTick(long TurnNumber)
	{
		_Body.HundredTurnTick(TurnNumber);
	}

	public void TypeDump(StringBuilder SB)
	{
		_Body.TypeDump(SB);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (base.WantEvent(ID, cascade) || ID == BeforeDeathRemovalEvent.ID || ID == EffectAppliedEvent.ID || ID == EffectRemovedEvent.ID || ID == GenericNotifyEvent.ID || ID == GenericQueryEvent.ID || ID == GetContentsEvent.ID || ID == GetExtrinsicWeightEvent.ID || ID == GetExtrinsicValueEvent.ID || ID == GetShortDescriptionEvent.ID || ID == QuerySlotListEvent.ID || ID == StripContentsEvent.ID)
		{
			return true;
		}
		if (MinEvent.CascadeTo(cascade, 1) && _Body.WantEvent(ID, cascade))
		{
			return true;
		}
		return false;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		UpdateMobilitySpeedPenalty();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		UpdateMobilitySpeedPenalty();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(MinEvent E)
	{
		if (!base.HandleEvent(E))
		{
			return false;
		}
		if (E.CascadeTo(1) && !_Body.HandleEvent(E))
		{
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!ParentObject.HasTag("NoDropOnDeath") && !ParentObject.IsTemporary && _Body != null)
		{
			_Body.UnequipPartAndChildren(Silent: true, null, ForDeath: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicValueEvent E)
	{
		if (!_Body.ProcessExtrinsicValue(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetExtrinsicWeightEvent E)
	{
		if (!_Body.ProcessExtrinsicWeight(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QuerySlotListEvent E)
	{
		if (!_Body.ProcessQuerySlotList(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (DismemberedParts != null && DismemberedParts.Count > 0)
		{
			GetMissingLimbsDescription(E.Postfix, PrependNewlineIfContent: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StripContentsEvent E)
	{
		List<GameObject> list = Event.NewGameObjectList();
		GetEquippedObjects(list);
		List<GameObject> list2 = Event.NewGameObjectList();
		GetInstalledCybernetics(list2);
		foreach (GameObject item in list)
		{
			if ((!E.KeepNatural || !item.IsNatural()) && (item.pPhysics == null || item.pPhysics.IsReal) && !list2.Contains(item))
			{
				list2.Add(item);
			}
		}
		foreach (GameObject item2 in list2)
		{
			item2.Obliterate(null, E.Silent);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContentsEvent E)
	{
		_Body.GetContents(E.Objects);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericNotifyEvent E)
	{
		if (E.Notify == "RegenerateLimb")
		{
			RegenerateLimb();
		}
		else if (E.Notify == "RegenerateWholeLimb")
		{
			RegenerateLimb(WholeLimb: true);
		}
		else if (E.Notify == "RegenerateAllLimbs")
		{
			int num = 0;
			while (RegenerateLimb(WholeLimb: true) && ++num < 100)
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "AnyRegenerableLimbs" && FindRegenerablePart() != null)
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public bool WantsEndTurnEvent()
	{
		return _Body.WantsEndTurnEvent();
	}

	public int GetPartDepth(BodyPart Part)
	{
		return _Body.GetPartDepth(Part, 0);
	}

	public void GetBodyPartsImplying(out List<BodyPart> Result, BodyPart Part, bool EvenIfDismembered = true)
	{
		Result = null;
		BodyPartType bodyPartType = Part.VariantTypeModel();
		if (string.IsNullOrEmpty(bodyPartType.ImpliedBy))
		{
			return;
		}
		int impliedPer = bodyPartType.GetImpliedPer();
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == bodyPartType.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == bodyPartType.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			Result = new List<BodyPart>(SomeBodyParts);
		}
		else
		{
			Result = new List<BodyPart>(impliedPer);
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							Result.Add(SomeBodyParts[num]);
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		if (EvenIfDismembered || Result == null || Result.Count <= 0 || DismemberedParts == null || DismemberedParts.Count <= 0)
		{
			return;
		}
		SomeBodyParts.Clear();
		SomeBodyParts.AddRange(Result);
		foreach (BodyPart someBodyPart in SomeBodyParts)
		{
			if (IsDismembered(someBodyPart))
			{
				Result.Remove(someBodyPart);
			}
		}
	}

	public List<BodyPart> GetBodyPartsImplying(BodyPart Part, bool EvenIfDismembered = true)
	{
		GetBodyPartsImplying(out var Result, Part, EvenIfDismembered);
		return Result;
	}

	public int GetBodyPartCountImplying(BodyPart Part)
	{
		if (string.IsNullOrEmpty(Part.VariantTypeModel().ImpliedBy))
		{
			return 0;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return GetBodyPartCountImplyingInternal(Part);
	}

	private int GetBodyPartCountImplyingInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (string.IsNullOrEmpty(Type.ImpliedBy))
			{
				return 0;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			return SomeBodyParts.Count;
		}
		int num = 0;
		int num2 = 0;
		int j = 0;
		for (int count2 = OtherBodyParts.Count; j < count2; j++)
		{
			if (OtherBodyParts[j] == Part)
			{
				for (int k = 0; k < impliedPer; k++)
				{
					if (num2 < SomeBodyParts.Count)
					{
						num++;
					}
					num2++;
				}
			}
			else
			{
				num2 += impliedPer;
			}
		}
		return num;
	}

	public bool AnyBodyPartsImplying(BodyPart Part)
	{
		if (string.IsNullOrEmpty(Part.VariantTypeModel().ImpliedBy))
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return AnyBodyPartsImplyingInternal(Part);
	}

	private bool AnyBodyPartsImplyingInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (string.IsNullOrEmpty(Type.ImpliedBy))
			{
				return false;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Count > 0)
			{
				return true;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							return true;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return false;
	}

	public bool ShouldRemoveDueToLackOfImplication(BodyPart Part)
	{
		if (string.IsNullOrEmpty(Part.VariantTypeModel().ImpliedBy))
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return ShouldRemoveDueToLackOfImplicationInternal(Part);
	}

	private bool ShouldRemoveDueToLackOfImplicationInternal(BodyPart Part, BodyPartType Type = null)
	{
		if (Type == null)
		{
			Type = Part.VariantTypeModel();
			if (string.IsNullOrEmpty(Type.ImpliedBy))
			{
				return false;
			}
		}
		int impliedPer = Type.GetImpliedPer();
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == Part.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy)
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == Part && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Count > 0)
			{
				return false;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == Part)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count)
						{
							return false;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return true;
	}

	public bool DoesPartImplyPart(BodyPart PartMaybeImplying, BodyPart PartMaybeImplied)
	{
		if (string.IsNullOrEmpty(PartMaybeImplied.VariantTypeModel().ImpliedBy))
		{
			return false;
		}
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return DoesPartImplyPartInternal(PartMaybeImplied, PartMaybeImplying);
	}

	private bool DoesPartImplyPartInternal(BodyPart PartMaybeImplying, BodyPart PartMaybeImplied, BodyPartType Type = null, bool TypeOnly = false)
	{
		if (Type == null)
		{
			Type = PartMaybeImplied.VariantTypeModel();
			if (string.IsNullOrEmpty(Type.ImpliedBy))
			{
				return false;
			}
		}
		SomeBodyParts.Clear();
		OtherBodyParts.Clear();
		int impliedPer = Type.GetImpliedPer();
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart.VariantType == Type.ImpliedBy && bodyPart.Laterality == PartMaybeImplied.Laterality)
			{
				SomeBodyParts.Add(bodyPart);
			}
			if (bodyPart.VariantType == Type.Type || (!TypeOnly && bodyPart.VariantType == bodyPart.VariantTypeModel().ImpliedBy))
			{
				OtherBodyParts.Add(bodyPart);
			}
		}
		if (OtherBodyParts.Count == 1 && OtherBodyParts[0] == PartMaybeImplied && SomeBodyParts.Count <= impliedPer)
		{
			if (SomeBodyParts.Contains(PartMaybeImplying))
			{
				return true;
			}
		}
		else
		{
			int num = 0;
			int j = 0;
			for (int count2 = OtherBodyParts.Count; j < count2; j++)
			{
				if (OtherBodyParts[j] == PartMaybeImplied)
				{
					for (int k = 0; k < impliedPer; k++)
					{
						if (num < SomeBodyParts.Count && SomeBodyParts[num] == PartMaybeImplying)
						{
							return true;
						}
						num++;
					}
				}
				else
				{
					num += impliedPer;
				}
			}
		}
		return false;
	}

	public void GetBodyPartsImpliedBy(out List<BodyPart> Result, BodyPart Part)
	{
		Result = null;
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart != Part && DoesPartImplyPartInternal(Part, bodyPart))
			{
				if (Result == null)
				{
					Result = new List<BodyPart>();
				}
				Result.Add(bodyPart);
			}
		}
	}

	public List<BodyPart> GetBodyPartsImpliedBy(BodyPart Part)
	{
		GetBodyPartsImpliedBy(out var Result, Part);
		return Result;
	}

	public BodyPart GetBodyPartOfTypeImpliedBy(BodyPart Part, BodyPartType Type)
	{
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		return GetBodyPartOfTypeImpliedByInternal(Part, Type);
	}

	private BodyPart GetBodyPartOfTypeImpliedByInternal(BodyPart Part, BodyPartType Type)
	{
		int i = 0;
		for (int count = AllBodyParts.Count; i < count; i++)
		{
			BodyPart bodyPart = AllBodyParts[i];
			if (bodyPart != Part && bodyPart.VariantType == Type.Type && DoesPartImplyPartInternal(Part, bodyPart, Type, TypeOnly: true))
			{
				return bodyPart;
			}
		}
		return null;
	}

	public void CheckImpliedParts(int Depth = 0)
	{
		AllBodyParts.Clear();
		GetParts(AllBodyParts, EvenIfDismembered: true);
		bool flag = false;
		int i = 0;
		for (int count = Anatomies.BodyPartTypeList.Count; i < count; i++)
		{
			BodyPartType bodyPartType = Anatomies.BodyPartTypeList[i];
			if (string.IsNullOrEmpty(bodyPartType.ImpliedBy))
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			int num3 = -1;
			BodyPart bodyPart = null;
			int j = 0;
			for (int count2 = AllBodyParts.Count; j < count2; j++)
			{
				BodyPart bodyPart2 = AllBodyParts[j];
				if (bodyPart2.VariantType == bodyPartType.ImpliedBy)
				{
					num++;
				}
				if (bodyPart2.VariantType == bodyPartType.Type)
				{
					num2++;
					if (bodyPart2.Position > num3)
					{
						num3 = bodyPart2.Position;
						bodyPart = bodyPart2;
					}
				}
			}
			if (num <= 0)
			{
				continue;
			}
			int impliedPer = bodyPartType.GetImpliedPer();
			int num4 = num / impliedPer;
			if (num % impliedPer != 0)
			{
				num4++;
			}
			for (int k = num2; k < num4; k++)
			{
				BodyPart bodyPart3 = null;
				int l = 0;
				for (int count3 = AllBodyParts.Count; l < count3; l++)
				{
					BodyPart bodyPart4 = AllBodyParts[l];
					if (bodyPart4.VariantType == bodyPartType.ImpliedBy && GetBodyPartOfTypeImpliedByInternal(bodyPart4, bodyPartType) == null)
					{
						bodyPart3 = bodyPart4;
						break;
					}
				}
				if (bodyPart3 != null)
				{
					BodyPart bodyPart5;
					if (bodyPart != null)
					{
						BodyPart body = _Body;
						BodyPartType @base = bodyPartType;
						int laterality = bodyPart3.Laterality;
						bodyPart5 = body.AddPartAt(bodyPart, @base, laterality, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, DoUpdate: false);
					}
					else
					{
						bodyPart5 = _Body.AddPartAt(Base: bodyPartType, Laterality: bodyPart3.Laterality, InsertBefore: new string[3] { "Feet", "Roots", "Thrown Weapon" }, DefaultBehavior: null, SupportsDependent: null, DependsOn: null, RequiresType: null, Manager: null, Category: null, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, DoUpdate: false);
					}
					bodyPart = bodyPart5;
					flag = true;
				}
			}
		}
		List<BodyPart> list = null;
		int m = 0;
		for (int count4 = AllBodyParts.Count; m < count4; m++)
		{
			if (ShouldRemoveDueToLackOfImplicationInternal(AllBodyParts[m]))
			{
				if (list == null)
				{
					list = new List<BodyPart>();
				}
				list.Add(AllBodyParts[m]);
			}
		}
		if (list != null)
		{
			int n = 0;
			for (int count5 = list.Count; n < count5; n++)
			{
				BodyPart removePart = list[n];
				RemovePart(removePart, DoUpdate: false);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateBodyParts(Depth + 1);
			RecalculateArmor();
		}
	}
}
