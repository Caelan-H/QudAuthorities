using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Messages;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class BodyPart
{
	public string Type;

	public string VariantType;

	public string Description;

	public string DescriptionPrefix;

	public string Name;

	public string SupportsDependent;

	public string DependsOn;

	public string RequiresType;

	public string PreventRegenerationBecause;

	public string Manager;

	public int Category = 1;

	public int Laterality;

	public int RequiresLaterality = 65535;

	public int Mobility;

	public bool DefaultPrimary;

	public bool PreferedPrimary;

	public bool Primary;

	public bool Native;

	public bool Appendage;

	public bool Integral;

	public bool Mortal;

	public bool Abstract;

	public bool Extrinsic;

	public bool Plural;

	public bool Mass;

	public bool Contact = true;

	public bool IgnorePosition;

	public Body ParentBody;

	public string DefaultBehaviorBlueprint;

	public GameObject DefaultBehavior;

	public int Position = -1;

	public GameObject _Cybernetics;

	public GameObject _Equipped;

	public List<BodyPart> Parts;

	public static int IDPreloadSequence = -1;

	public int _ID;

	[NonSerialized]
	private static List<BodyPart> ToClearEquip = new List<BodyPart>();

	private static string[] managerSplit = new string[1] { "::" };

	[NonSerialized]
	private static List<BodyPart> SeverWork = new List<BodyPart>(8);

	public GameObject Cybernetics => _Cybernetics;

	public GameObject Equipped => _Equipped;

	public int ID
	{
		get
		{
			if (_ID == 0)
			{
				if (XRLCore.Core.Game != null)
				{
					_ID = XRLCore.Core.Game.GetIntGameState("nextId", 1);
					XRLCore.Core.Game.SetIntGameState("nextId", _ID + 1);
				}
				else
				{
					_ID = IDPreloadSequence--;
				}
			}
			return _ID;
		}
		set
		{
			_ID = value;
		}
	}

	public bool idMatch(int testID)
	{
		return _ID == testID;
	}

	public bool HasID()
	{
		return _ID != 0;
	}

	private BodyPart()
	{
	}

	private BodyPart(int Alloc)
	{
		if (Parts == null)
		{
			Parts = new List<BodyPart>(Alloc);
		}
	}

	public BodyPart(Body ParentBody)
		: this()
	{
		this.ParentBody = ParentBody;
	}

	public BodyPart(Body ParentBody, int Alloc)
		: this(Alloc)
	{
		this.ParentBody = ParentBody;
	}

	public BodyPart(BodyPartType Base, Body ParentBody)
		: this(ParentBody)
	{
		Base.ApplyTo(this);
	}

	public BodyPart(BodyPartType Base, Body ParentBody, int Alloc)
		: this(ParentBody, Alloc)
	{
		Base.ApplyTo(this);
	}

	public BodyPart(string Base, Body ParentBody)
		: this(Anatomies.GetBodyPartTypeOrFail(Base), ParentBody)
	{
	}

	public BodyPart(string Base, Body ParentBody, int Alloc)
		: this(Anatomies.GetBodyPartTypeOrFail(Base), ParentBody, Alloc)
	{
	}

	public BodyPart(string Base, int Laterality, Body ParentBody)
		: this(Base, ParentBody)
	{
		ChangeLaterality(Laterality);
	}

	public BodyPart(string Type, string Description, string Name, Body ParentBody)
		: this(ParentBody)
	{
		this.Type = Type;
		this.Description = Description ?? Type;
		this.Name = Name ?? Description.ToLower();
	}

	public BodyPart(BodyPartType Base, Body ParentBody, string DescriptionPrefix = null, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, ParentBody)
	{
		if (DescriptionPrefix != null)
		{
			this.DescriptionPrefix = DescriptionPrefix;
		}
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPart(BodyPartType Base, int Laterality, Body ParentBody, string DescriptionPrefix = null, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, ParentBody)
	{
		ChangeLaterality(Laterality);
		if (DescriptionPrefix != null)
		{
			this.DescriptionPrefix = DescriptionPrefix;
		}
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPart(string Base, int Laterality, Body ParentBody, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Mortal = null, bool? Appendage = null, bool? Integral = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null)
		: this(Base, Laterality, ParentBody)
	{
		if (DefaultBehavior != null)
		{
			DefaultBehaviorBlueprint = DefaultBehavior;
		}
		if (SupportsDependent != null)
		{
			this.SupportsDependent = SupportsDependent;
		}
		if (DependsOn != null)
		{
			this.DependsOn = DependsOn;
		}
		if (RequiresType != null)
		{
			this.RequiresType = RequiresType;
		}
		if (Manager != null)
		{
			this.Manager = Manager;
		}
		if (Category.HasValue)
		{
			this.Category = Category.Value;
		}
		if (RequiresLaterality.HasValue)
		{
			this.RequiresLaterality = RequiresLaterality.Value;
		}
		if (Mobility.HasValue)
		{
			this.Mobility = Mobility.Value;
		}
		if (Appendage.HasValue)
		{
			this.Appendage = Appendage.Value;
		}
		if (Integral.HasValue)
		{
			this.Integral = Integral.Value;
		}
		if (Mortal.HasValue)
		{
			this.Mortal = Mortal.Value;
		}
		if (Abstract.HasValue)
		{
			this.Abstract = Abstract.Value;
		}
		if (Extrinsic.HasValue)
		{
			this.Extrinsic = Extrinsic.Value;
		}
		if (Plural.HasValue)
		{
			this.Plural = Plural.Value;
		}
		if (Mass.HasValue)
		{
			this.Mass = Mass.Value;
		}
		if (Contact.HasValue)
		{
			this.Contact = Contact.Value;
		}
		if (IgnorePosition.HasValue)
		{
			this.IgnorePosition = IgnorePosition.Value;
		}
	}

	public BodyPartType TypeModel()
	{
		return Anatomies.GetBodyPartTypeOrFail(Type);
	}

	public BodyPartType VariantTypeModel()
	{
		return Anatomies.GetBodyPartTypeOrFail(VariantType ?? Type);
	}

	public static int Sort(BodyPart a, BodyPart b)
	{
		return a.Position.CompareTo(b.Position);
	}

	public bool ObjectEquippedOnThisOrAnyParent(GameObject obj)
	{
		if (obj == DefaultBehavior)
		{
			return true;
		}
		if (obj == Equipped)
		{
			return true;
		}
		if (obj == Cybernetics)
		{
			return true;
		}
		return GetParentPart()?.ObjectEquippedOnThisOrAnyParent(obj) ?? false;
	}

	public BodyPart ChangeLaterality(int NewLaterality, bool Recursive = false)
	{
		BodyPartType bodyPartType = VariantTypeModel();
		if (IsLateralityConsistent(bodyPartType))
		{
			Laterality = NewLaterality;
			Description = XRL.World.Capabilities.Laterality.WithLateralityAdjective(bodyPartType.Description, Laterality, Capitalized: true);
			Name = XRL.World.Capabilities.Laterality.WithLateralityAdjective(bodyPartType.Name, Laterality);
			RequiresLaterality = bodyPartType.RequiresLateralityFor(Laterality);
		}
		else
		{
			string @base = XRL.World.Capabilities.Laterality.StripLateralityAdjective(Description, Laterality, Capitalized: true);
			string base2 = XRL.World.Capabilities.Laterality.StripLateralityAdjective(Name, Laterality);
			Laterality = NewLaterality;
			Description = XRL.World.Capabilities.Laterality.WithLateralityAdjective(@base, Laterality, Capitalized: true);
			Name = XRL.World.Capabilities.Laterality.WithLateralityAdjective(base2, Laterality);
			if (RequiresLaterality == 65535)
			{
				RequiresLaterality = bodyPartType.RequiresLateralityFor(Laterality);
			}
			else
			{
				RequiresLaterality |= bodyPartType.RequiresLateralityFor(Laterality);
			}
		}
		if (Recursive && Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].ChangeLaterality(NewLaterality, Recursive: true);
			}
		}
		return this;
	}

	public bool IsLateralityConsistent(BodyPartType UseType = null)
	{
		if (UseType == null)
		{
			UseType = VariantTypeModel();
		}
		if (Description != XRL.World.Capabilities.Laterality.WithLateralityAdjective(UseType.Description, Laterality, Capitalized: true))
		{
			return false;
		}
		if (Name != XRL.World.Capabilities.Laterality.WithLateralityAdjective(UseType.Name, Laterality))
		{
			return false;
		}
		if (RequiresLaterality != 65535 && RequiresLaterality != UseType.RequiresLateralityFor(Laterality))
		{
			return false;
		}
		return true;
	}

	public bool IsLateralitySafeToChange(int ExpectLaterality = 0, Body BackupParentBody = null, BodyPart ChildPart = null)
	{
		if (Laterality != ExpectLaterality)
		{
			return false;
		}
		BodyPartType useType = VariantTypeModel();
		if (!IsLateralityConsistent(useType))
		{
			return false;
		}
		if (ChildPart != null)
		{
			if (!IsParentPartOf(ChildPart, BackupParentBody, EvenIfDismembered: true))
			{
				return false;
			}
			if (!ChildPart.IsLateralitySafeToChange(ExpectLaterality, BackupParentBody))
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerable<BodyPart> LoopParts()
	{
		yield return this;
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			foreach (BodyPart item in part.LoopParts())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopOtherParts()
	{
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			foreach (BodyPart item in part.LoopParts())
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopSubparts()
	{
		if (Parts == null)
		{
			yield break;
		}
		foreach (BodyPart part in Parts)
		{
			yield return part;
		}
	}

	public BodyPart GetParentPart()
	{
		return ParentBody.FindParentPartOf(this);
	}

	public BodyPart FindByManager(string Manager)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindByManager(Manager);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		if (this.Manager == Manager)
		{
			return this;
		}
		return null;
	}

	public BodyPart FindByManager(string Manager, string Type)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindByManager(Manager, Type);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		if (this.Manager == Manager && this.Type == Type)
		{
			return this;
		}
		return null;
	}

	public void FindByManager(string Manager, List<BodyPart> Store)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindByManager(Manager, Store);
			}
		}
		if (this.Manager == Manager)
		{
			Store.Add(this);
		}
	}

	public void FindByManager(string Manager, string Type, List<BodyPart> Store)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindByManager(Manager, Type, Store);
			}
		}
		if (this.Manager == Manager && this.Type == Type)
		{
			Store.Add(this);
		}
	}

	public BodyPart FindParentPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			if (Parts.Contains(FindPart))
			{
				return this;
			}
			foreach (BodyPart part in Parts)
			{
				BodyPart bodyPart = part.FindParentPartOf(FindPart);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public bool IsParentPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			return Parts.Contains(FindPart);
		}
		return false;
	}

	public bool IsParentPartOf(BodyPart FindPart, Body BackupParentBody, bool EvenIfDismembered)
	{
		if (IsParentPartOf(FindPart))
		{
			return true;
		}
		Body body = ((ParentBody != null) ? ParentBody : BackupParentBody);
		if (body != null && body.ValidateDismemberedParentPart(this, FindPart))
		{
			return true;
		}
		return false;
	}

	public bool IsAncestorPartOf(BodyPart FindPart)
	{
		if (Parts != null)
		{
			if (Parts.Contains(FindPart))
			{
				return true;
			}
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsAncestorPartOf(FindPart))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FindPartsEquipping(GameObject GO, List<BodyPart> Return)
	{
		if (Equipped == GO)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].FindPartsEquipping(GO, Return);
			}
		}
	}

	public BodyPart FindEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindEquippedItem(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindEquippedItem(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindEquippedItem(Blueprint);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public BodyPart FindDefaultOrEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return this;
		}
		if (DefaultBehavior == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindDefaultOrEquippedItem(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentByEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID) && !Equipped.FireEvent(ID))
		{
			return Equipped;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentByEvent(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrDefaultByID(string ID)
	{
		if (Equipped != null && Equipped.idmatch(ID))
		{
			return Equipped;
		}
		if (DefaultBehavior != null && DefaultBehavior.idmatch(ID))
		{
			return DefaultBehavior;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrDefaultByID(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentByEvent(Event E)
	{
		if (Equipped != null && !Equipped.FireEvent(E))
		{
			return Equipped;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentByEvent(E);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID) && !Equipped.FireEvent(ID))
		{
			return Equipped;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.HasRegisteredEvent(ID) && !Cybernetics.FireEvent(ID))
		{
			return Cybernetics;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrCyberneticsByEvent(ID);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public GameObject FindEquipmentOrCyberneticsByEvent(Event E)
	{
		if (Equipped != null && !Equipped.FireEvent(E))
		{
			return Equipped;
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Cybernetics.FireEvent(E))
		{
			return Cybernetics;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				GameObject gameObject = Parts[i].FindEquipmentOrCyberneticsByEvent(E);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public bool HasEquippedItem(GameObject GO)
	{
		if (Equipped == GO)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasEquippedItem(GO))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquippedItem(string Blueprint)
	{
		if (Equipped != null && Equipped.Blueprint == Blueprint)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].HasEquippedItem(Blueprint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsItemEquippedOnLimbType(GameObject GO, string FindType)
	{
		if (Equipped == GO && Type == FindType)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsItemEquippedOnLimbType(GO, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart FindCybernetics(GameObject GO)
	{
		if (Cybernetics == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindCybernetics(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public bool IsItemImplantedInLimbType(GameObject GO, string FindType)
	{
		if (Cybernetics == GO && Type == FindType)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].IsItemImplantedInLimbType(GO, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public BodyPart FindDefaultBehavior(GameObject GO)
	{
		if (DefaultBehavior == GO)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i].FindDefaultBehavior(GO);
				if (bodyPart != null)
				{
					return bodyPart;
				}
			}
		}
		return null;
	}

	public void FlushWantTurnTickCache()
	{
		if (ParentBody != null && ParentBody.ParentObject != null)
		{
			ParentBody.ParentObject.FlushWantTurnTickCache();
		}
	}

	public GameObject Unimplant(bool MoveToInventory = true)
	{
		GameObject cybernetics = _Cybernetics;
		if (cybernetics != null)
		{
			UnimplantedEvent.Send(ParentBody.ParentObject, cybernetics, this);
			if (_Equipped == cybernetics)
			{
				_Equipped = null;
			}
			if (MoveToInventory && ParentBody != null && ParentBody.ParentObject != null)
			{
				Inventory inventory = ParentBody.ParentObject.Inventory;
				if (inventory != null && !inventory.Objects.Contains(cybernetics))
				{
					inventory.AddObject(cybernetics);
				}
			}
			_Cybernetics = null;
			FlushWantTurnTickCache();
		}
		return cybernetics;
	}

	public GameObject Dismember(bool obliterate = false)
	{
		if (ParentBody == null)
		{
			return null;
		}
		return ParentBody.Dismember(this, null, obliterate);
	}

	public void Implant(GameObject Cybernetic, bool ForDeepCopy = false)
	{
		if (_Cybernetics != null)
		{
			Unimplant();
		}
		_Cybernetics = Cybernetic;
		if (Cybernetic.HasTag("CyberneticsUsesEqSlot"))
		{
			if (_Equipped != null && !TryUnequip())
			{
				ForceUnequip();
			}
			Equip(Cybernetic, null, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true);
		}
		ImplantedEvent.Send(ParentBody.ParentObject, Cybernetic, this, null, ForDeepCopy);
		ParentBody.RegenerateDefaultEquipment();
		if (ParentBody.ParentObject.IsPlayer())
		{
			string baseDisplayName = Cybernetic.BaseDisplayName;
			JournalAPI.AddAccomplishment("You installed a cybernetic " + baseDisplayName + " and came one step closer to the Grand Unification.", "Gaze upon the sublime body of =name=! See the glistening " + baseDisplayName + "!", "general", JournalAccomplishment.MuralCategory.BodyExperienceGood, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
	}

	public bool Equip(GameObject Item, int? EnergyCost = null, bool Silent = false, bool ForDeepCopy = false, bool Forced = false, bool SemiForced = false)
	{
		Event @event = Event.New("CommandEquipObject");
		if (Silent)
		{
			@event.SetSilent(Silent: true);
		}
		if (ForDeepCopy)
		{
			@event.SetFlag("ForDeepCopy", State: true);
		}
		@event.SetParameter("Object", Item);
		@event.SetParameter("BodyPart", this);
		if (Forced)
		{
			@event.SetFlag("Forced", State: true);
		}
		if (SemiForced)
		{
			@event.SetFlag("SemiForced", State: true);
		}
		if (EnergyCost.HasValue)
		{
			@event.SetParameter("EnergyCost", EnergyCost.Value);
		}
		return ParentBody.ParentObject.FireEvent(@event);
	}

	public bool DoEquip(GameObject GO, bool Silent = false, bool ForDeepCopy = false)
	{
		string FailureMessage = null;
		bool result = DoEquip(GO, ref FailureMessage, Silent, ForDeepCopy);
		if (!string.IsNullOrEmpty(FailureMessage) && !Silent)
		{
			Body parentBody = ParentBody;
			if (parentBody != null && parentBody.ParentObject?.IsPlayer() == true)
			{
				Popup.ShowFail(FailureMessage);
			}
		}
		return result;
	}

	public bool DoEquip(GameObject GO, ref string FailureMessage, bool Silent = false, bool ForDeepCopy = false, bool UnequipOthers = false, int AutoEquipTry = 0, List<GameObject> WasUnequipped = null)
	{
		if (ParentBody == null)
		{
			throw new Exception("cannot equip without parent body");
		}
		ParentBody.WeightCache = -1;
		string usesSlots = GO.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots) && (Type != "Thrown Weapon" || usesSlots.Contains("Thrown Weapon")) && (!(Type == "Hand") || usesSlots.Contains("Hand")))
		{
			List<string> list = usesSlots.CachedCommaExpansion();
			bool flag = true;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string text = list[i];
				string Text = text;
				int codeFromAdjective = XRL.World.Capabilities.Laterality.GetCodeFromAdjective(ref Text);
				if ((Text == Type || Text == VariantType) && (codeFromAdjective == 0 || (Laterality & codeFromAdjective) == codeFromAdjective) && flag)
				{
					flag = false;
					continue;
				}
				int num = ParentBody.GetUnequippedPartCountExcept(Text, codeFromAdjective, this);
				int num2 = 0;
				bool flag2 = true;
				foreach (string item in list)
				{
					if ((item == Type || item == VariantType) && flag2)
					{
						flag2 = false;
					}
					else if (item == Text)
					{
						num2++;
					}
				}
				if (num2 <= num)
				{
					continue;
				}
				if (UnequipOthers)
				{
					foreach (BodyPart item2 in ParentBody.GetPart(Text, codeFromAdjective))
					{
						if (item2 == this)
						{
							continue;
						}
						GameObject obj = item2.Equipped;
						if (obj == null)
						{
							continue;
						}
						Event @event = Event.New("CommandUnequipObject", "BodyPart", item2);
						if (Silent)
						{
							@event.SetSilent(Silent: true);
						}
						if (AutoEquipTry > 0)
						{
							@event.SetParameter("AutoEquipTry", AutoEquipTry);
						}
						if (!string.IsNullOrEmpty(FailureMessage))
						{
							@event.SetParameter("FailureMessage", FailureMessage);
						}
						if (ForDeepCopy)
						{
							@event.SetFlag("ForDeepCopy", State: true);
						}
						if (ParentBody.ParentObject.FireEvent(@event))
						{
							if (item2.Equipped == null)
							{
								if (WasUnequipped != null && GameObject.validate(ref obj))
								{
									WasUnequipped.Add(obj);
								}
								num++;
								if (num >= num2)
								{
									break;
								}
							}
						}
						else
						{
							string stringParameter = @event.GetStringParameter("FailureMessage");
							if (!string.IsNullOrEmpty(stringParameter) && stringParameter != FailureMessage)
							{
								FailureMessage = stringParameter;
							}
						}
					}
				}
				if (num2 > num)
				{
					if (num2 == num + 1)
					{
						FailureMessage = "You need " + ((num > 0) ? "another" : "a") + " free " + text + " slot to equip that!";
					}
					else
					{
						FailureMessage = "You need another " + Grammar.Cardinal(num2 - num) + " free " + text + " slots to equip that!";
					}
					return false;
				}
			}
			bool flag3 = false;
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				string text2 = list[j];
				string Text2 = text2;
				int codeFromAdjective2 = XRL.World.Capabilities.Laterality.GetCodeFromAdjective(ref Text2);
				List<BodyPart> part = ParentBody.GetPart(Text2, codeFromAdjective2);
				if (!flag3 && part.Contains(this))
				{
					flag3 = true;
					continue;
				}
				BodyPart bodyPart = null;
				int num3 = int.MaxValue;
				int k = 0;
				for (int count3 = part.Count; k < count3; k++)
				{
					BodyPart bodyPart2 = part[k];
					if (bodyPart2.Equipped == null && bodyPart2 != this)
					{
						int num4 = Math.Abs(bodyPart2.Position - Position);
						if (num4 < num3)
						{
							bodyPart = bodyPart2;
							num3 = num4;
						}
					}
				}
				if (bodyPart != null)
				{
					bodyPart._Equipped = GO;
				}
				else
				{
					MetricsManager.LogError("internal inconsistency, no closest " + text2 + " found for " + GO.DebugName);
				}
			}
		}
		else if (GO.UsesTwoSlotsFor(ParentBody.ParentObject) && Type != "Thrown Weapon")
		{
			List<BodyPart> part2 = ParentBody.GetPart(Type);
			BodyPart bodyPart3 = null;
			foreach (BodyPart item3 in part2)
			{
				if (item3 != this && item3.Equipped == null)
				{
					bodyPart3 = item3;
					break;
				}
			}
			if (bodyPart3 == null && UnequipOthers)
			{
				foreach (BodyPart item4 in part2)
				{
					if (item4 == this)
					{
						continue;
					}
					GameObject obj2 = item4.Equipped;
					if (obj2 == null)
					{
						continue;
					}
					Event event2 = Event.New("CommandUnequipObject", "BodyPart", item4);
					if (Silent)
					{
						event2.SetSilent(Silent: true);
					}
					if (AutoEquipTry > 0)
					{
						event2.SetParameter("AutoEquipTry", AutoEquipTry);
					}
					if (!string.IsNullOrEmpty(FailureMessage))
					{
						event2.SetParameter("FailureMessage", FailureMessage);
					}
					if (ForDeepCopy)
					{
						event2.SetFlag("ForDeepCopy", State: true);
					}
					if (ParentBody.ParentObject.FireEvent(event2))
					{
						if (item4.Equipped == null)
						{
							if (WasUnequipped != null && GameObject.validate(ref obj2))
							{
								WasUnequipped.Add(obj2);
							}
							bodyPart3 = item4;
							break;
						}
					}
					else
					{
						string stringParameter2 = event2.GetStringParameter("FailureMessage");
						if (!string.IsNullOrEmpty(stringParameter2) && stringParameter2 != FailureMessage)
						{
							FailureMessage = stringParameter2;
						}
					}
				}
			}
			if (bodyPart3 == null)
			{
				FailureMessage = "You need another free " + Anatomies.GetBodyPartType(Type).Type + " slot to equip that!";
				return false;
			}
			bodyPart3._Equipped = GO;
		}
		if (ParentBody != null)
		{
			ParentBody.WeightCache = -1;
		}
		_Equipped = GO;
		FlushWantTurnTickCache();
		return true;
	}

	private void ClearEquipped()
	{
		if (ParentBody != null)
		{
			ParentBody.WeightCache = -1;
		}
		_Equipped = null;
		FlushWantTurnTickCache();
	}

	public bool TryUnequip(bool Silent = false, bool SemiForced = false)
	{
		Event @event = Event.New("CommandUnequipObject", "BodyPart", this);
		if (SemiForced)
		{
			@event.SetParameter("SemiForced", 1);
		}
		if (Silent)
		{
			@event.SetSilent(Silent: true);
		}
		return ParentBody.ParentObject.FireEvent(@event);
	}

	public bool ForceUnequip(bool Silent = false)
	{
		Event @event = Event.New("CommandForceUnequipObject", "BodyPart", this);
		if (Silent)
		{
			@event.SetSilent(Silent: true);
		}
		return ParentBody.ParentObject.FireEvent(@event);
	}

	public void Unequip()
	{
		if (Equipped == null)
		{
			return;
		}
		GameObject equipped = Equipped;
		ToClearEquip.Clear();
		if (ParentBody == null)
		{
			FindPartsEquipping(equipped, ToClearEquip);
		}
		else
		{
			ParentBody.FindPartsEquipping(equipped, ToClearEquip);
		}
		int i = 0;
		for (int count = ToClearEquip.Count; i < count; i++)
		{
			if (ToClearEquip[i].Equipped == equipped)
			{
				ToClearEquip[i].ClearEquipped();
			}
		}
		ToClearEquip.Clear();
	}

	private GameObject MakeCopyWithMap(GameObject obj, Func<GameObject, GameObject> map)
	{
		if (map != null)
		{
			GameObject gameObject = map(obj);
			if (gameObject != null)
			{
				if (obj.hasid)
				{
					gameObject.injectId(obj.id);
				}
				return gameObject;
			}
		}
		return obj.DeepCopy(CopyEffects: false, CopyID: true, map);
	}

	public BodyPart DeepCopy(GameObject Parent, Body NewBody, Func<GameObject, GameObject> MapInv)
	{
		BodyPart bodyPart = new BodyPart();
		bodyPart.Type = Type;
		bodyPart.VariantType = VariantType;
		bodyPart.Description = Description;
		bodyPart.DescriptionPrefix = DescriptionPrefix;
		bodyPart.Name = Name;
		bodyPart.SupportsDependent = SupportsDependent;
		bodyPart.DependsOn = DependsOn;
		bodyPart.RequiresType = RequiresType;
		bodyPart.PreventRegenerationBecause = PreventRegenerationBecause;
		bodyPart.Manager = Manager;
		bodyPart.Category = Category;
		bodyPart.Laterality = Laterality;
		bodyPart.RequiresLaterality = RequiresLaterality;
		bodyPart.Mobility = Mobility;
		bodyPart.Primary = Primary;
		bodyPart.PreferedPrimary = PreferedPrimary;
		bodyPart.DefaultPrimary = DefaultPrimary;
		bodyPart.Native = Native;
		bodyPart.Appendage = Appendage;
		bodyPart.Integral = Integral;
		bodyPart.Mortal = Mortal;
		bodyPart.Abstract = Abstract;
		bodyPart.Extrinsic = Extrinsic;
		bodyPart.Plural = Plural;
		bodyPart.Mass = Mass;
		bodyPart.Contact = Contact;
		bodyPart.IgnorePosition = IgnorePosition;
		bodyPart.Position = Position;
		bodyPart._ID = _ID;
		bodyPart.ParentBody = NewBody;
		bodyPart.DefaultBehaviorBlueprint = DefaultBehaviorBlueprint;
		if (DefaultBehavior != null)
		{
			GameObject gameObject = MakeCopyWithMap(DefaultBehavior, MapInv);
			Parent.DeepCopyInventoryObjectMap.Add(DefaultBehavior, gameObject);
			gameObject.pPhysics._Equipped = Parent;
			bodyPart.DefaultBehavior = gameObject;
			if (DefaultBehavior == Equipped || DefaultBehavior == Cybernetics)
			{
				Body.DeepCopyEquipMap.Add(gameObject, bodyPart);
			}
		}
		if (Equipped != null && !Parent.DeepCopyInventoryObjectMap.ContainsKey(Equipped))
		{
			GameObject gameObject2 = MakeCopyWithMap(Equipped, MapInv);
			Parent.DeepCopyInventoryObjectMap.Add(Equipped, gameObject2);
			Body.DeepCopyEquipMap.Add(gameObject2, bodyPart);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Parent.DeepCopyInventoryObjectMap.ContainsKey(Cybernetics))
		{
			GameObject gameObject3 = MakeCopyWithMap(Cybernetics, MapInv);
			Parent.DeepCopyInventoryObjectMap.Add(Cybernetics, gameObject3);
			Body.DeepCopyEquipMap.Add(gameObject3, bodyPart);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (!part.Extrinsic)
				{
					bodyPart.AddPart(part.DeepCopy(Parent, NewBody, MapInv));
				}
			}
			return bodyPart;
		}
		return bodyPart;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(Type);
		Writer.Write(VariantType);
		Writer.Write(Description);
		Writer.Write(DescriptionPrefix);
		Writer.Write(Name);
		Writer.Write(SupportsDependent);
		Writer.Write(DependsOn);
		Writer.Write(RequiresType);
		Writer.Write(PreventRegenerationBecause);
		Writer.Write(Manager);
		Writer.Write(Category);
		Writer.Write(Laterality);
		Writer.Write(RequiresLaterality);
		Writer.Write(Mobility);
		Writer.Write(Primary);
		Writer.Write(PreferedPrimary);
		Writer.Write(DefaultPrimary);
		Writer.Write(Native);
		Writer.Write(Appendage);
		Writer.Write(Integral);
		Writer.Write(Mortal);
		Writer.Write(Abstract);
		Writer.Write(Extrinsic);
		Writer.Write(Plural);
		Writer.Write(Mass);
		Writer.Write(Contact);
		Writer.Write(IgnorePosition);
		Writer.Write(Position);
		Writer.Write(_ID);
		Writer.Write(DefaultBehaviorBlueprint);
		Writer.WriteGameObject(DefaultBehavior);
		Writer.WriteGameObject(_Equipped);
		Writer.WriteGameObject(_Cybernetics);
		if (Parts == null)
		{
			Writer.Write(-1);
			return;
		}
		Writer.Write(Parts.Count);
		foreach (BodyPart part in Parts)
		{
			part.Save(Writer);
		}
	}

	public static BodyPart Load(SerializationReader Reader, Body ParentBody)
	{
		BodyPart bodyPart = new BodyPart();
		bodyPart.Type = Reader.ReadString();
		bodyPart.VariantType = Reader.ReadString();
		bodyPart.Description = Reader.ReadString();
		bodyPart.DescriptionPrefix = Reader.ReadString();
		bodyPart.Name = Reader.ReadString();
		bodyPart.SupportsDependent = Reader.ReadString();
		bodyPart.DependsOn = Reader.ReadString();
		bodyPart.RequiresType = Reader.ReadString();
		bodyPart.PreventRegenerationBecause = Reader.ReadString();
		if (Reader.FileVersion >= 133)
		{
			bodyPart.Manager = Reader.ReadString();
		}
		bodyPart.Category = Reader.ReadInt32();
		bodyPart.Laterality = Reader.ReadInt32();
		bodyPart.RequiresLaterality = Reader.ReadInt32();
		bodyPart.Mobility = Reader.ReadInt32();
		bodyPart.Primary = Reader.ReadBoolean();
		if (Reader.FileVersion >= 214)
		{
			bodyPart.DefaultPrimary = Reader.ReadBoolean();
			bodyPart.PreferedPrimary = Reader.ReadBoolean();
		}
		else
		{
			bodyPart.PreferedPrimary = bodyPart.Primary;
		}
		bodyPart.Native = Reader.ReadBoolean();
		bodyPart.Appendage = Reader.ReadBoolean();
		bodyPart.Integral = Reader.ReadBoolean();
		bodyPart.Mortal = Reader.ReadBoolean();
		bodyPart.Abstract = Reader.ReadBoolean();
		bodyPart.Extrinsic = Reader.ReadBoolean();
		bodyPart.Plural = Reader.ReadBoolean();
		bodyPart.Mass = Reader.ReadBoolean();
		bodyPart.Contact = Reader.ReadBoolean();
		bodyPart.IgnorePosition = Reader.ReadBoolean();
		bodyPart.Position = Reader.ReadInt32();
		bodyPart._ID = Reader.ReadInt32();
		if (Reader.FileVersion >= 141)
		{
			bodyPart.DefaultBehaviorBlueprint = Reader.ReadString();
		}
		bodyPart.DefaultBehavior = Reader.ReadGameObject();
		if (Reader.FileVersion < 141)
		{
			bodyPart.DefaultBehaviorBlueprint = bodyPart.DefaultBehavior?.Blueprint;
		}
		bodyPart._Equipped = Reader.ReadGameObject();
		bodyPart._Cybernetics = Reader.ReadGameObject();
		bodyPart.ParentBody = ParentBody;
		int num = Reader.ReadInt32();
		if (num < 1)
		{
			bodyPart.Parts = null;
		}
		else
		{
			bodyPart.Parts = new List<BodyPart>(num);
		}
		for (int i = 0; i < num; i++)
		{
			bodyPart.Parts.Add(Load(Reader, ParentBody));
		}
		return bodyPart;
	}

	public int GetWeight()
	{
		int num = 0;
		if (Equipped != null && IsFirstSlotForEquipped())
		{
			CyberneticsBaseItem part = Equipped.GetPart<CyberneticsBaseItem>();
			if (part != null)
			{
				if (part.ImplantedOn == null)
				{
					num += Equipped.Weight;
				}
			}
			else
			{
				num += Equipped.Weight;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetWeight();
				}
			}
		}
		return num;
	}

	public void Clear()
	{
		DefaultBehavior = null;
		ClearEquipped();
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.Clear();
			}
		}
		Parts = null;
	}

	public void StripAllEquipment(List<GameObject> Return)
	{
		if (Equipped != null)
		{
			Return.Add(Equipped);
			BeforeUnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			UnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			Unequip();
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item.StripAllEquipment(Return);
		}
	}

	public void GetPrimaryHandEquippedObjects(List<GameObject> Return)
	{
		if (Primary && Type == "Hand" && Equipped != null)
		{
			Return.Add(Equipped);
		}
		if (Primary && Type == "Hand" && Equipped == null && DefaultBehavior != null)
		{
			Return.Add(DefaultBehavior);
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.GetPrimaryHandEquippedObjects(Return);
			}
		}
	}

	public void GetPrimaryEquippedObjects(List<GameObject> Return)
	{
		if (Primary && Equipped != null)
		{
			Return.Add(Equipped);
		}
		if (Primary && DefaultBehavior != null && Equipped == null)
		{
			Return.Add(DefaultBehavior);
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				Parts[i]?.GetPrimaryEquippedObjects(Return);
			}
		}
	}

	public void GetShield(ref GameObject obj, Predicate<GameObject> Filter, GameObject Attacker, GameObject Defender)
	{
		if (Equipped != null && Equipped.GetPart("Shield") is Shield shield && (shield.WornOn == "*" || shield.WornOn == Type) && (Filter == null || Filter(Equipped)))
		{
			if (obj == null)
			{
				obj = Equipped;
			}
			else
			{
				int @for = GetShieldBlockPreferenceEvent.GetFor(Equipped, Attacker, Defender);
				int for2 = GetShieldBlockPreferenceEvent.GetFor(obj, Attacker, Defender);
				if (@for > for2)
				{
					obj = Equipped;
				}
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetShield(ref obj, Filter, Attacker, Defender);
			}
		}
	}

	public void SetAsPreferredDefault(bool force = false)
	{
		if (!force && ParentBody.ParentObject.IsPlayer())
		{
			if (XRLCore.Core.Game.Player.Body.AreHostilesNearby())
			{
				Popup.Show("You can't switch primary limbs in combat.");
				return;
			}
			if (Abstract || Extrinsic)
			{
				Popup.Show("This body part cannot be set as your primary.");
				return;
			}
		}
		ParentBody.ForeachPart(delegate(BodyPart p)
		{
			p.PreferedPrimary = false;
			return true;
		});
		PreferedPrimary = true;
		ParentBody.RecalculateFirsts();
	}

	public void SetPrimaryScan(string WantType, ref bool PrimarySet, ref bool HasPreferredPrimary, ref BodyPart CurrentPrimaryPart)
	{
		if (PreferedPrimary)
		{
			HasPreferredPrimary = true;
		}
		if (PrimarySet)
		{
			Primary = false;
			DefaultPrimary = false;
		}
		else if (Type == WantType)
		{
			PrimarySet = true;
			DefaultPrimary = true;
			if (CurrentPrimaryPart == null)
			{
				Primary = true;
				CurrentPrimaryPart = this;
			}
		}
		if (PreferedPrimary)
		{
			Primary = true;
			if (CurrentPrimaryPart != null && CurrentPrimaryPart != this)
			{
				CurrentPrimaryPart.Primary = false;
			}
			CurrentPrimaryPart = this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].SetPrimaryScan(WantType, ref PrimarySet, ref HasPreferredPrimary, ref CurrentPrimaryPart);
			}
		}
	}

	public void ScanForWeapon(bool NeedPrimary, ref GameObject Weapon, ref BodyPart equippedOn, ref int PickPriority, ref int PossibleWeapons, ref bool HadPrimary)
	{
		GameObject gameObject = Equipped ?? DefaultBehavior;
		if (gameObject != null && gameObject != Weapon && gameObject.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.AttackFromPart(this))
		{
			PossibleWeapons++;
			int num = (Primary ? ((PreferedPrimary && !DefaultPrimary) ? 20 : ((Equipped == null) ? 8 : 10)) : (gameObject.HasTag("PreferredDefault") ? 9 : ((Type == "Hand") ? 7 : ((this == ParentBody._Body) ? 5 : 6))));
			if (gameObject == DefaultBehavior && gameObject.HasTag("UndesireableWeapon"))
			{
				num -= 4;
			}
			bool flag = false;
			if (Weapon == null || num > PickPriority)
			{
				flag = true;
			}
			else if (num == PickPriority && Brain.IsNewWeaponBetter(gameObject, Weapon, ParentBody.ParentObject))
			{
				flag = true;
			}
			if (flag)
			{
				Weapon = gameObject;
				equippedOn = this;
				PickPriority = num;
				HadPrimary = Primary;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ScanForWeapon(NeedPrimary, ref Weapon, ref equippedOn, ref PickPriority, ref PossibleWeapons, ref HadPrimary);
			}
		}
	}

	public GameObject ThisPartWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		if (!NeedPrimary || Primary || PreferedPrimary)
		{
			if (Equipped != null)
			{
				if (Equipped.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && WeaponType == meleeWeapon.Skill && meleeWeapon.AttackFromPart(this))
				{
					return Equipped;
				}
			}
			else if (DefaultBehavior != null && DefaultBehavior.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon2 && WeaponType == meleeWeapon2.Skill && meleeWeapon2.AttackFromPart(this))
			{
				return DefaultBehavior;
			}
		}
		return null;
	}

	public GameObject GetWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		GameObject gameObject = ThisPartWeaponOfType(WeaponType, NeedPrimary);
		if (gameObject != null)
		{
			return gameObject;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null)
				{
					gameObject = bodyPart.GetWeaponOfType(WeaponType, NeedPrimary);
					if (gameObject != null)
					{
						return gameObject;
					}
				}
			}
		}
		return null;
	}

	public bool HasWeaponOfType(string WeaponType, bool NeedPrimary)
	{
		if (ThisPartWeaponOfType(WeaponType, NeedPrimary) != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.HasWeaponOfType(WeaponType, NeedPrimary))
				{
					return true;
				}
			}
		}
		return false;
	}

	public GameObject GetWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary)
	{
		if (Type == BodyPartType)
		{
			GameObject gameObject = ThisPartWeaponOfType(WeaponType, NeedPrimary);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null)
				{
					GameObject weaponOfTypeOnBodyPartOfType = bodyPart.GetWeaponOfTypeOnBodyPartOfType(WeaponType, BodyPartType, NeedPrimary);
					if (weaponOfTypeOnBodyPartOfType != null)
					{
						return weaponOfTypeOnBodyPartOfType;
					}
				}
			}
		}
		return null;
	}

	public bool HasWeaponOfTypeOnBodyPartOfType(string WeaponType, string BodyPartType, bool NeedPrimary)
	{
		if (Type == BodyPartType && ThisPartWeaponOfType(WeaponType, NeedPrimary) != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.HasWeaponOfType(WeaponType, NeedPrimary))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ClearShieldBlocks()
	{
		if (Equipped != null && Equipped.GetPart("Shield") is Shield shield)
		{
			shield.Blocks = 0;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i]?.ClearShieldBlocks();
			}
		}
	}

	public int GetEquippedObjectCount()
	{
		int num = 0;
		if (Equipped != null && IsFirstSlotForEquipped())
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetEquippedObjectCount();
				}
			}
			return num;
		}
		return num;
	}

	public int GetEquippedObjectCount(Predicate<GameObject> pFilter)
	{
		int num = 0;
		if (Equipped != null && pFilter(Equipped) && IsFirstSlotForEquipped())
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetEquippedObjectCount(pFilter);
				}
			}
			return num;
		}
		return num;
	}

	public void GetEquippedObjects(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjects(Return);
		}
	}

	public List<GameObject> GetEquippedObjectsReadonly()
	{
		List<GameObject> list = null;
		if (Equipped != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Equipped);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetEquippedObjectsReadonly();
					}
					else
					{
						part.GetEquippedObjects(list);
					}
				}
			}
			return list;
		}
		return list;
	}

	public void GetEquippedObjects(List<GameObject> Return, Predicate<GameObject> pFilter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && pFilter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjects(Return, pFilter);
		}
	}

	public void GetEquippedObjectsExceptNatural(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped) && !Equipped.HasTag("NaturalGear"))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsExceptNatural(Return);
		}
	}

	public void GetEquippedObjectsExceptNatural(List<GameObject> Return, Predicate<GameObject> pFilter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && !Equipped.HasTag("NaturalGear") && pFilter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsExceptNatural(Return, pFilter);
		}
	}

	public void ForeachEquippedObject(Action<GameObject> aProc)
	{
		if (Equipped != null && IsFirstSlotForEquipped())
		{
			aProc(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.ForeachEquippedObject(aProc);
		}
	}

	public void SafeForeachEquippedObject(Action<GameObject> aProc)
	{
		if (Equipped != null && IsFirstSlotForEquipped())
		{
			aProc(Equipped);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.SafeForeachEquippedObject(aProc);
		}
	}

	public int GetInstalledCyberneticsCount()
	{
		int num = 0;
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetInstalledCyberneticsCount();
				}
			}
			return num;
		}
		return num;
	}

	public int GetInstalledCyberneticsCount(Predicate<GameObject> Filter)
	{
		int num = 0;
		if (Cybernetics != null && Filter(Cybernetics) && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetInstalledCyberneticsCount(Filter);
				}
			}
			return num;
		}
		return num;
	}

	public void GetInstalledCybernetics(List<GameObject> Return)
	{
		if (Cybernetics != null && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetInstalledCybernetics(Return);
		}
	}

	public List<GameObject> GetInstalledCyberneticsReadonly()
	{
		List<GameObject> list = null;
		if (Cybernetics != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Cybernetics);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetInstalledCyberneticsReadonly();
					}
					else
					{
						part.GetInstalledCybernetics(list);
					}
				}
			}
			return list;
		}
		return list;
	}

	public void GetInstalledCybernetics(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Cybernetics != null && !Return.Contains(Cybernetics) && Filter(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetInstalledCybernetics(Return, Filter);
		}
	}

	public bool AnyInstalledCybernetics()
	{
		if (Cybernetics != null)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.AnyInstalledCybernetics())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyInstalledCybernetics(Predicate<GameObject> Filter)
	{
		if (Cybernetics != null && Filter(Cybernetics))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && bodyPart.AnyInstalledCybernetics(Filter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			aProc(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.ForeachInstalledCybernetics(aProc);
		}
	}

	public void SafeForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		if (Cybernetics != null && ParentBody.FindCybernetics(Cybernetics) == this)
		{
			aProc(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.ForeachInstalledCybernetics(aProc);
		}
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsAndInstalledCybernetics(Return);
		}
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCyberneticsReadonly()
	{
		List<GameObject> list = null;
		if (Equipped != null)
		{
			list = Event.NewGameObjectList();
			list.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			if (list == null)
			{
				list = Event.NewGameObjectList();
			}
			list.Add(Cybernetics);
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					if (list == null)
					{
						list = part.GetEquippedObjectsAndInstalledCyberneticsReadonly();
					}
					else
					{
						part.GetEquippedObjectsAndInstalledCybernetics(list);
					}
				}
			}
			return list;
		}
		return list;
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> Return, Predicate<GameObject> Filter)
	{
		if (Equipped != null && !Return.Contains(Equipped) && Filter(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics) && Filter(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.GetEquippedObjectsAndInstalledCybernetics(Return, Filter);
		}
	}

	public bool Render(RenderEvent E)
	{
		if (Equipped != null)
		{
			Equipped.Render(E);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			Cybernetics.Render(E);
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].Render(E);
				}
			}
		}
		return true;
	}

	public virtual bool AnyRegisteredEvent(string ID)
	{
		if (Equipped != null && Equipped.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.HasRegisteredEvent(ID))
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part.AnyRegisteredEvent(ID))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool FireEvent(Event E)
	{
		bool flag = true;
		if (Equipped != null && IsFirstSlotForEquipped())
		{
			if (!Equipped.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn" && Equipped != null)
			{
				Equipped.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (Cybernetics != null && Cybernetics != Equipped && IsFirstSlotForCybernetics())
		{
			if (!Cybernetics.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn" && Cybernetics != null)
			{
				Cybernetics.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (DefaultBehavior != null)
		{
			if (!DefaultBehavior.FireEvent(E))
			{
				flag = false;
			}
			if (E.ID == "EndTurn")
			{
				DefaultBehavior.CleanEffects();
			}
		}
		if (!flag)
		{
			return false;
		}
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (bodyPart != null && !bodyPart.FireEvent(E))
				{
					flag = false;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return flag;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Markup.Wrap(Name)).Append(": ");
		if (Equipped == null && DefaultBehavior == null)
		{
			stringBuilder.Append("{{K|-}}");
		}
		else if (Equipped == null)
		{
			stringBuilder.Append("{{K|").Append(DefaultBehavior.ShortDisplayName).Append("}}");
		}
		else
		{
			stringBuilder.Append("{{C|").Append(Equipped.ShortDisplayName).Append("}}");
		}
		if (Primary)
		{
			stringBuilder.Append(" {{G|*Primary}}");
		}
		return stringBuilder.ToString();
	}

	public void UnequipPartAndChildren(bool Silent = false, Cell Where = null, bool ForDeath = false)
	{
		if (Equipped != null && Equipped.IsReal && !Equipped.IsNatural())
		{
			if (ParentBody != null)
			{
				GameObject equipped = Equipped;
				try
				{
					Event @event = Event.New("CommandForceUnequipObject", "BodyPart", this, "NoTake", 1);
					@event.SetSilent(Silent: true);
					ParentBody.ParentObject.FireEvent(@event);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("UnequipPartAndChildren unequip", x);
				}
				try
				{
					if (!ForDeath || ParentBody.ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
					{
						if (Where == null && ForDeath)
						{
							Where = ParentBody.ParentObject.GetDropCell();
						}
						if (Where == null)
						{
							Inventory inventory = ParentBody.ParentObject.Inventory;
							if (inventory != null)
							{
								equipped.RemoveFromContext();
								inventory.AddObject(equipped);
							}
							else
							{
								Where = ParentBody.ParentObject.GetDropCell();
							}
						}
						if (Where != null && (!ForDeath || DropOnDeathEvent.Check(equipped, Where, WasEquipped: true)))
						{
							Event event2 = Event.New("CommandDropObject");
							event2.SetParameter("Object", equipped);
							event2.SetParameter("Where", Where);
							event2.SetFlag("Forced", State: true);
							if (ParentBody.ParentObject.FireEvent(event2) && !Silent && equipped.CurrentCell == Where && equipped.IsVisible())
							{
								MessageQueue.AddPlayerMessage(ParentBody.ParentObject.Poss(equipped) + equipped.GetVerb("fall") + " to the ground.", ColorCoding.ConsequentialColor(null, ParentBody.ParentObject));
							}
						}
					}
				}
				catch (Exception x2)
				{
					MetricsManager.LogException("UnequipPartAndChildren drop", x2);
				}
			}
		}
		else
		{
			if (Equipped != null && ParentBody.ParentObject != null)
			{
				BeforeUnequippedEvent.Send(Equipped, ParentBody.ParentObject);
				UnequippedEvent.Send(Equipped, ParentBody.ParentObject);
			}
			Unequip();
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item.UnequipPartAndChildren(Silent, Where, ForDeath);
		}
	}

	public void ToString(StringBuilder SB)
	{
		SB.Append(ToString());
		SB.Append("\n");
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.ToString(SB);
		}
	}

	public bool RemoveUnmanagedPartsByVariantPrefix(string Prefix)
	{
		bool result = false;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].RemoveUnmanagedPartsByVariantPrefix(Prefix))
				{
					result = true;
					count = Parts.Count;
					i--;
				}
			}
		}
		if (VariantType != Type && VariantType.StartsWith(Prefix) && ParentBody != null)
		{
			ParentBody.RemovePart(this);
			result = true;
		}
		return result;
	}

	public bool RemovePart(BodyPart Part, bool DoUpdate = true)
	{
		if (Parts == null)
		{
			return false;
		}
		if (Parts.Contains(Part))
		{
			Part.UnequipPartAndChildren();
			Parts.Remove(Part);
			if (DoUpdate && ParentBody != null)
			{
				ParentBody.UpdateBodyParts();
				ParentBody.RecalculateTypeArmor(Part.Type);
			}
			return true;
		}
		foreach (BodyPart part in Parts)
		{
			if (part.RemovePart(Part, DoUpdate))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemovePartByID(int removeID, bool DoUpdate = true)
	{
		BodyPart partByID = GetPartByID(removeID);
		if (partByID == null)
		{
			return false;
		}
		return RemovePart(partByID, DoUpdate);
	}

	private bool PositionOccupied(int Position)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position == Position)
				{
					return true;
				}
			}
		}
		if (ParentBody != null && ParentBody.DismemberedPartHasPosition(this, Position))
		{
			return true;
		}
		return false;
	}

	private bool PositionOccupied(int Position, BodyPart ExceptPart)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position == Position && Parts[i] != ExceptPart)
				{
					return true;
				}
			}
		}
		if (ParentBody != null && ParentBody.DismemberedPartHasPosition(this, Position, ExceptPart))
		{
			return true;
		}
		return false;
	}

	private void AssignPosition(BodyPart Part, int Position)
	{
		if (PositionOccupied(Position, Part))
		{
			if (Parts != null)
			{
				int i = 0;
				for (int count = Parts.Count; i < count; i++)
				{
					if (Parts[i].Position >= Position)
					{
						Parts[i].Position++;
					}
				}
			}
			if (ParentBody != null)
			{
				ParentBody.CheckDismemberedPartRenumberingOnPositionAssignment(this, Position, Part);
			}
		}
		Part.Position = Position;
	}

	private int NextPosition(BodyPart ExceptPart = null)
	{
		int num = ((ParentBody != null) ? ParentBody.GetLastDismemberedPartPosition(this, ExceptPart) : (-1));
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Position > num && Parts[i] != ExceptPart)
				{
					num = Parts[i].Position;
				}
			}
		}
		return num + 1;
	}

	private int FindIndexForPosition(int Position)
	{
		if (Parts != null)
		{
			for (int i = 0; i < Parts.Count; i++)
			{
				if (Parts[i].Position >= Position)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public BodyPart AddPart(BodyPart NewPart, int Position = -1, bool DoUpdate = true)
	{
		if (Parts == null)
		{
			Parts = new List<BodyPart>();
		}
		if (Parts.IndexOf(NewPart) != -1)
		{
			throw new Exception("part for add already attached");
		}
		if (Position == -1)
		{
			Position = NextPosition(NewPart);
		}
		else
		{
			while (Position > 0 && !PositionOccupied(Position - 1, NewPart))
			{
				Position--;
			}
		}
		int num = FindIndexForPosition(Position);
		if (num == -1)
		{
			Parts.Add(NewPart);
		}
		else
		{
			Parts.Insert(num, NewPart);
		}
		AssignPosition(NewPart, Position);
		if (ParentBody != null)
		{
			NewPart.ParentBody = ParentBody;
			if (DoUpdate)
			{
				ParentBody.UpdateBodyParts();
				ParentBody.RecalculateTypeArmor(NewPart.Type);
			}
		}
		return NewPart;
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, bool DoUpdate = true)
	{
		int num = LastPartTypePosition(InsertAfter);
		if (num != -1)
		{
			num++;
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, string OrInsertBefore, bool DoUpdate = true)
	{
		if (OrInsertBefore == null)
		{
			return AddPart(NewPart, InsertAfter);
		}
		int num = LastPartTypePosition(InsertAfter);
		num = ((num != -1) ? (num + 1) : FirstPartTypePosition(OrInsertBefore));
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string[] InsertBefore, bool DoUpdate = true)
	{
		int num = -1;
		if (InsertBefore != null)
		{
			foreach (string type in InsertBefore)
			{
				num = FirstPartTypePosition(type);
				if (num != -1)
				{
					break;
				}
			}
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPart NewPart, string InsertAfter, string[] OrInsertBefore, bool DoUpdate = true)
	{
		if (OrInsertBefore == null)
		{
			return AddPart(NewPart, InsertAfter);
		}
		int num = LastPartTypePosition(InsertAfter);
		if (num == -1)
		{
			foreach (string type in OrInsertBefore)
			{
				num = FirstPartTypePosition(type);
				if (num != -1)
				{
					break;
				}
			}
		}
		else
		{
			num++;
		}
		return AddPart(NewPart, num, DoUpdate);
	}

	public BodyPart AddPart(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), -1, DoUpdate);
	}

	public BodyPart AddPart(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), -1, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string[] OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, string InsertAfter = null, string[] OrInsertBefore = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter, OrInsertBefore, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPart InsertAfter, BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter.Position + 1, DoUpdate);
	}

	public BodyPart AddPartAt(BodyPart InsertAfter, string Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertAfter.Position + 1, DoUpdate);
	}

	public BodyPart AddPartAt(string[] InsertBefore, BodyPartType Base, int Laterality = 0, string DefaultBehavior = null, string SupportsDependent = null, string DependsOn = null, string RequiresType = null, string Manager = null, int? Category = null, int? RequiresLaterality = null, int? Mobility = null, bool? Appendage = null, bool? Integral = null, bool? Mortal = null, bool? Abstract = null, bool? Extrinsic = null, bool? Plural = null, bool? Mass = null, bool? Contact = null, bool? IgnorePosition = null, bool DoUpdate = true)
	{
		return AddPart(new BodyPart(Base, Laterality, ParentBody, null, DefaultBehavior, SupportsDependent, DependsOn, RequiresType, Manager, Category, RequiresLaterality, Mobility, Mortal, Appendage, Integral, Abstract, Extrinsic, Plural, Mass, Contact, IgnorePosition), InsertBefore, DoUpdate);
	}

	public int FirstPartTypePosition(string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].Type == Type && Parts[i] != ExceptPart)
				{
					num = Parts[i].Position;
					break;
				}
			}
		}
		if (ParentBody != null)
		{
			int firstDismemberedPartTypePosition = ParentBody.GetFirstDismemberedPartTypePosition(this, Type, ExceptPart);
			if (firstDismemberedPartTypePosition != -1 && (num == -1 || firstDismemberedPartTypePosition < num))
			{
				num = firstDismemberedPartTypePosition;
			}
		}
		return num;
	}

	public int LastPartTypePosition(string Type, BodyPart ExceptPart = null)
	{
		int num = -1;
		if (Type == null)
		{
			return num;
		}
		if (Parts != null)
		{
			for (int num2 = Parts.Count - 1; num2 >= 0; num2--)
			{
				if (Parts[num2].Type == Type && Parts[num2] != ExceptPart)
				{
					num = Parts[num2].Position;
					break;
				}
			}
		}
		if (ParentBody != null)
		{
			int lastDismemberedPartTypePosition = ParentBody.GetLastDismemberedPartTypePosition(this, Type, ExceptPart);
			if (lastDismemberedPartTypePosition > num)
			{
				num = lastDismemberedPartTypePosition;
			}
		}
		return num;
	}

	public BodyPart GetPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			foreach (BodyPart part2 in Parts)
			{
				if (part2 != null)
				{
					BodyPart part = part2.GetPart(Filter);
					if (part != null)
					{
						return part;
					}
				}
			}
		}
		return null;
	}

	public bool AnyPart(Predicate<BodyPart> pFilter)
	{
		if (pFilter(this))
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyPart(pFilter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForeachPart(Action<BodyPart> aProc)
	{
		aProc(this);
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part?.ForeachPart(aProc);
		}
	}

	public bool ForeachPart(Predicate<BodyPart> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && !part.ForeachPart(pProc))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SafeForeachPart(Action<BodyPart> aProc)
	{
		aProc(this);
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart item in new List<BodyPart>(Parts))
		{
			item?.ForeachPart(aProc);
		}
	}

	public bool SafeForeachPart(Predicate<BodyPart> pProc)
	{
		if (!pProc(this))
		{
			return false;
		}
		if (Parts != null)
		{
			foreach (BodyPart item in new List<BodyPart>(Parts))
			{
				if (item != null && !item.ForeachPart(pProc))
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetPartCount()
	{
		int num = 1;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount();
				}
			}
		}
		return num;
	}

	public int GetConcretePartCount()
	{
		int num = ((!Abstract) ? 1 : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetConcretePartCount();
				}
			}
		}
		return num;
	}

	public int GetAbstractPartCount()
	{
		int num = (Abstract ? 1 : 0);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetAbstractPartCount();
				}
			}
		}
		return num;
	}

	public int GetPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(RequiredType);
				}
			}
		}
		return num;
	}

	public int GetPartCount(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPartCount(RequiredType);
		}
		int num = 0;
		if (Type == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(RequiredType, RequiredLaterality);
				}
			}
		}
		return num;
	}

	public int GetPartCount(Predicate<BodyPart> Filter)
	{
		int num = 0;
		if (Filter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCount(Filter);
				}
			}
		}
		return num;
	}

	public int GetPartNameCount(string FindName)
	{
		int num = 0;
		if (Name == FindName)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartNameCount(FindName);
				}
			}
		}
		return num;
	}

	public int GetPartDescriptionCount(string FindDescription)
	{
		int num = 0;
		if (Description == FindDescription)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartDescriptionCount(FindDescription);
				}
			}
		}
		return num;
	}

	public int GetPartDescriptionCount(string FindDescription, string FindDescriptionPrefix)
	{
		int num = 0;
		if (Description == FindDescription && DescriptionPrefix.EqualsEmptyEqualsNull(FindDescriptionPrefix))
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartDescriptionCount(FindDescription, FindDescriptionPrefix);
				}
			}
		}
		return num;
	}

	public bool AnyCategoryParts(int FindCategory)
	{
		if (FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyCategoryParts(FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetCategoryPartCount(int FindCategory)
	{
		int num = ((FindCategory == Category) ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetCategoryPartCount(FindCategory);
				}
			}
			return num;
		}
		return num;
	}

	public int GetCategoryPartCount(int FindCategory, string FindType)
	{
		int num = 0;
		if (FindCategory == Category && FindType == Type)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetCategoryPartCount(FindCategory, FindType);
			}
			return num;
		}
		return num;
	}

	public int GetCategoryPartCount(int FindCategory, Predicate<BodyPart> pFilter)
	{
		int num = 0;
		if (FindCategory == Category && pFilter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetCategoryPartCount(FindCategory, pFilter);
				}
			}
			return num;
		}
		return num;
	}

	public int GetNativePartCount()
	{
		int num = (Native ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetNativePartCount();
				}
			}
			return num;
		}
		return num;
	}

	public int GetNativePartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && Native)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetNativePartCount(RequiredType);
			}
			return num;
		}
		return num;
	}

	public int GetNativePartCount(Predicate<BodyPart> pFilter)
	{
		int num = 0;
		if (Native && pFilter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetNativePartCount(pFilter);
				}
			}
			return num;
		}
		return num;
	}

	public int GetAddedPartCount()
	{
		int num = ((!Native) ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetAddedPartCount();
				}
			}
			return num;
		}
		return num;
	}

	public int GetAddedPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && !Native)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetAddedPartCount(RequiredType);
			}
			return num;
		}
		return num;
	}

	public int GetAddedPartCount(Predicate<BodyPart> pFilter)
	{
		int num = 0;
		if (!Native && pFilter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetAddedPartCount(pFilter);
				}
			}
			return num;
		}
		return num;
	}

	public int GetMortalPartCount()
	{
		int num = (Mortal ? 1 : 0);
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetMortalPartCount();
				}
			}
			return num;
		}
		return num;
	}

	public int GetMortalPartCount(string RequiredType)
	{
		int num = 0;
		if (Type == RequiredType && Mortal)
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				num += part.GetMortalPartCount(RequiredType);
			}
			return num;
		}
		return num;
	}

	public int GetMortalPartCount(Predicate<BodyPart> pFilter)
	{
		int num = 0;
		if (Mortal && pFilter(this))
		{
			num++;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null)
				{
					num += part.GetMortalPartCount(pFilter);
				}
			}
			return num;
		}
		return num;
	}

	public bool AnyMortalParts()
	{
		if (Mortal)
		{
			return true;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part != null && part.AnyMortalParts())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnType(GameObject FindObj, string FindType)
	{
		if (FindObj == Equipped && FindType == Type)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnType(FindObj, FindType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnCategory(GameObject FindObj, int FindCategory)
	{
		if (FindObj == Equipped && FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnCategory(FindObj, FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquippedOnPrimary(GameObject FindObj)
	{
		if (FindObj == Equipped && Primary)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsEquippedOnPrimary(FindObj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsImplantedInCategory(GameObject FindObj, int FindCategory)
	{
		if (FindObj == Cybernetics && FindCategory == Category)
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].IsImplantedInCategory(FindObj, FindCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetTotalMobility()
	{
		int num = Mobility;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetTotalMobility();
				}
			}
		}
		return num;
	}

	public void MarkAllNative()
	{
		Native = true;
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].MarkAllNative();
			}
		}
	}

	public void CategorizeAll(int ApplyCategory)
	{
		Category = ApplyCategory;
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].CategorizeAll(ApplyCategory);
			}
		}
	}

	public void CategorizeAllExcept(int ApplyCategory, int SkipCategory)
	{
		if (Category != SkipCategory)
		{
			Category = ApplyCategory;
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].CategorizeAllExcept(ApplyCategory, SkipCategory);
			}
		}
	}

	public List<BodyPart> GetParts()
	{
		List<BodyPart> list = new List<BodyPart>(GetPartCount());
		list.Add(this);
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetParts(list);
				}
			}
		}
		return list;
	}

	public void GetParts(List<BodyPart> Return)
	{
		Return.Add(this);
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetParts(Return);
			}
		}
	}

	public void GetConcreteParts(List<BodyPart> Return)
	{
		if (!Abstract)
		{
			Return.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetConcreteParts(Return);
			}
		}
	}

	public void GetAbstractParts(List<BodyPart> Return)
	{
		if (Abstract)
		{
			Return.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetAbstractParts(Return);
			}
		}
	}

	public void GetPartsEquippedOn(GameObject obj, List<BodyPart> Result)
	{
		if (Equipped == obj)
		{
			Result.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetPartsEquippedOn(obj, Result);
			}
		}
	}

	public List<BodyPart> GetPartsEquippedOn(GameObject obj)
	{
		List<BodyPart> result = new List<BodyPart>(GetPartCountEquippedOn(obj));
		GetPartsEquippedOn(obj, result);
		return result;
	}

	public int GetPartCountEquippedOn(GameObject obj)
	{
		int num = 0;
		if (Equipped == obj)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetPartCountEquippedOn(obj);
				}
			}
		}
		return num;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, List<BodyPart> Return)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetUnequippedPart(RequiredType, Return);
				}
			}
		}
		return Return;
	}

	public List<BodyPart> GetUnequippedPart(string RequiredType, int RequiredLaterality, List<BodyPart> Return)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPart(RequiredType, Return);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetUnequippedPart(RequiredType, RequiredLaterality, Return);
				}
			}
		}
		return Return;
	}

	public int GetUnequippedPartCount(string RequiredType)
	{
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCount(RequiredType);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCount(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPartCount(RequiredType);
		}
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCount(RequiredType, RequiredLaterality);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCountExcept(string RequiredType, BodyPart ExceptPart)
	{
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && Equipped == null && ExceptPart != this)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCountExcept(RequiredType, ExceptPart);
				}
			}
		}
		return num;
	}

	public int GetUnequippedPartCountExcept(string RequiredType, int RequiredLaterality, BodyPart ExceptPart)
	{
		if (RequiredLaterality == 65535)
		{
			return GetUnequippedPartCountExcept(RequiredType, ExceptPart);
		}
		int num = 0;
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Equipped == null && ExceptPart != this)
		{
			num++;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					num += Parts[i].GetUnequippedPartCountExcept(RequiredType, RequiredLaterality, ExceptPart);
				}
			}
		}
		return num;
	}

	public List<BodyPart> GetPart(string RequiredType, List<BodyPart> Return)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetPart(RequiredType, Return);
				}
			}
		}
		return Return;
	}

	public List<BodyPart> GetPart(string RequiredType, int RequiredLaterality, List<BodyPart> Return)
	{
		if (RequiredLaterality == 65535)
		{
			return GetPart(RequiredType, Return);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].GetPart(RequiredType, RequiredLaterality, Return);
				}
			}
		}
		return Return;
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			yield return this;
		}
		if (Parts == null)
		{
			yield break;
		}
		int i = 0;
		for (int j = Parts.Count; i < j; i++)
		{
			if (Parts[i] == null)
			{
				continue;
			}
			foreach (BodyPart item in Parts[i].LoopPart(RequiredType))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<BodyPart> LoopPart(string RequiredType, int RequiredLaterality)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			yield return this;
		}
		if (Parts == null)
		{
			yield break;
		}
		int i = 0;
		for (int j = Parts.Count; i < j; i++)
		{
			if (Parts[i] == null)
			{
				continue;
			}
			foreach (BodyPart item in Parts[i].LoopPart(RequiredType, RequiredLaterality))
			{
				yield return item;
			}
		}
	}

	public BodyPart GetFirstPart(string RequiredType)
	{
		if (Type == RequiredType || VariantType == RequiredType)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstPart(RequiredType);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, RequiredLaterality);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		if ((Type == RequiredType || VariantType == RequiredType) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstPart(RequiredType, Filter);
		}
		if ((Type == RequiredType || VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstPart = Parts[i].GetFirstPart(RequiredType, RequiredLaterality, Filter);
					if (firstPart != null)
					{
						return firstPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType)
	{
		if (VariantType == RequiredType)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstVariantPart(RequiredType);
		}
		if (VariantType == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, RequiredLaterality);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(Predicate<BodyPart> Filter)
	{
		if (Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, Predicate<BodyPart> Filter)
	{
		if (VariantType == RequiredType && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstVariantPart(string RequiredType, int RequiredLaterality, Predicate<BodyPart> Filter)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstVariantPart(RequiredType, Filter);
		}
		if (VariantType == RequiredType && XRL.World.Capabilities.Laterality.Match(this, RequiredLaterality) && Filter(this))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart firstVariantPart = Parts[i].GetFirstVariantPart(RequiredType, RequiredLaterality, Filter);
					if (firstVariantPart != null)
					{
						return firstVariantPart;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && (Parts[i].Type == RequiredType || Parts[i].VariantType == RequiredType))
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, Body BackupParentBody, bool EvenIfDismembered)
	{
		BodyPart bodyPart = GetFirstAttachedPart(RequiredType);
		if (EvenIfDismembered)
		{
			Body body = ParentBody ?? BackupParentBody;
			if (body != null)
			{
				BodyPart firstDismemberedPartByType = body.GetFirstDismemberedPartByType(this, RequiredType);
				if (firstDismemberedPartByType != null && (bodyPart == null || firstDismemberedPartByType.Position < bodyPart.Position))
				{
					bodyPart = firstDismemberedPartByType;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, int RequiredLaterality)
	{
		if (RequiredLaterality == 65535)
		{
			return GetFirstAttachedPart(RequiredType);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && (Parts[i].Type == RequiredType || Parts[i].VariantType == RequiredType) && XRL.World.Capabilities.Laterality.Match(Parts[i], RequiredLaterality))
				{
					return Parts[i];
				}
			}
		}
		return null;
	}

	public BodyPart GetFirstAttachedPart(string RequiredType, int RequiredLaterality, Body BackupParentBody, bool EvenIfDismembered)
	{
		BodyPart bodyPart = GetFirstAttachedPart(RequiredType, RequiredLaterality);
		if (EvenIfDismembered)
		{
			Body body = ParentBody ?? BackupParentBody;
			if (body != null)
			{
				BodyPart firstDismemberedPartByTypeAndLaterality = body.GetFirstDismemberedPartByTypeAndLaterality(this, RequiredType, RequiredLaterality);
				if (firstDismemberedPartByTypeAndLaterality != null && (bodyPart == null || firstDismemberedPartByTypeAndLaterality.Position < bodyPart.Position))
				{
					bodyPart = firstDismemberedPartByTypeAndLaterality;
				}
			}
		}
		return bodyPart;
	}

	public BodyPart GetPartByName(string RequiredPart)
	{
		if (Name == RequiredPart)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByName = Parts[i].GetPartByName(RequiredPart);
					if (partByName != null)
					{
						return partByName;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByNameStartsWith(string RequiredPart)
	{
		if (Name != null && Name.StartsWith(RequiredPart))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByNameStartsWith = Parts[i].GetPartByNameStartsWith(RequiredPart);
					if (partByNameStartsWith != null)
					{
						return partByNameStartsWith;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByNameWithoutCybernetics(string RequiredPart)
	{
		if (Name == RequiredPart && Cybernetics == null)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByNameWithoutCybernetics = Parts[i].GetPartByNameWithoutCybernetics(RequiredPart);
					if (partByNameWithoutCybernetics != null)
					{
						return partByNameWithoutCybernetics;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescription(string RequiredDescription)
	{
		if (Description == RequiredDescription)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescription = Parts[i].GetPartByDescription(RequiredDescription);
					if (partByDescription != null)
					{
						return partByDescription;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescription(string RequiredDescription, string RequiredDescriptionPrefix)
	{
		if (Description == RequiredDescription && DescriptionPrefix.EqualsEmptyEqualsNull(RequiredDescriptionPrefix))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescription = Parts[i].GetPartByDescription(RequiredDescription, RequiredDescriptionPrefix);
					if (partByDescription != null)
					{
						return partByDescription;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByDescriptionStartsWith(string RequiredDescription)
	{
		if (Description != null && Description.StartsWith(RequiredDescription))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByDescriptionStartsWith = Parts[i].GetPartByDescriptionStartsWith(RequiredDescription);
					if (partByDescriptionStartsWith != null)
					{
						return partByDescriptionStartsWith;
					}
				}
			}
		}
		return null;
	}

	public BodyPart GetPartByID(int findID)
	{
		if (idMatch(findID))
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partByID = Parts[i].GetPartByID(findID);
					if (partByID != null)
					{
						return partByID;
					}
				}
			}
		}
		return null;
	}

	public void GetPartBySupportsDependent(string findSupportsDependent, List<BodyPart> parts)
	{
		if (SupportsDependent == findSupportsDependent)
		{
			parts.Add(this);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetPartBySupportsDependent(findSupportsDependent);
			}
		}
	}

	public BodyPart GetPartBySupportsDependent(string findSupportsDependent)
	{
		if (SupportsDependent == findSupportsDependent)
		{
			return this;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					BodyPart partBySupportsDependent = Parts[i].GetPartBySupportsDependent(findSupportsDependent);
					if (partBySupportsDependent != null)
					{
						return partBySupportsDependent;
					}
				}
			}
		}
		return null;
	}

	protected void GetNamePosition(BodyPart FindPart, ref int Found, ref bool Done)
	{
		if (FindPart == this)
		{
			Found++;
			Done = true;
			return;
		}
		if (FindPart.Name == Name)
		{
			Found++;
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetNamePosition(FindPart, ref Found, ref Done);
				if (Done)
				{
					break;
				}
			}
		}
	}

	public int GetNamePosition()
	{
		int Found = 0;
		bool Done = false;
		if (ParentBody != null)
		{
			ParentBody.GetBody().GetNamePosition(this, ref Found, ref Done);
		}
		else
		{
			GetNamePosition(this, ref Found, ref Done);
		}
		return Found;
	}

	protected void GetDescriptionPosition(BodyPart FindPart, ref int Found, ref bool Done)
	{
		if (FindPart == this)
		{
			Found++;
			Done = true;
			return;
		}
		if (FindPart.Description == Description && FindPart.DescriptionPrefix.EqualsEmptyEqualsNull(DescriptionPrefix))
		{
			Found++;
		}
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.GetDescriptionPosition(FindPart, ref Found, ref Done);
			if (Done)
			{
				break;
			}
		}
	}

	public int GetDescriptionPosition()
	{
		int Found = 0;
		bool Done = false;
		if (ParentBody != null)
		{
			ParentBody.GetBody().GetDescriptionPosition(this, ref Found, ref Done);
		}
		else
		{
			GetDescriptionPosition(this, ref Found, ref Done);
		}
		return Found;
	}

	public string GetCardinalName()
	{
		if (IgnorePosition)
		{
			return Name;
		}
		int namePosition = GetNamePosition();
		if (namePosition == 1)
		{
			return Name;
		}
		return Event.NewStringBuilder().Append(Name).Append(" (")
			.Append(namePosition)
			.Append(')')
			.ToString();
	}

	public string GetOrdinalName()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (IgnorePosition || GetPartNameCount(Name) == 1)
		{
			stringBuilder.Append(Name);
		}
		else
		{
			stringBuilder.Append(Grammar.Ordinal(GetNamePosition())).Append(' ').Append(Name);
		}
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public string GetCardinalDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (!string.IsNullOrEmpty(DescriptionPrefix))
		{
			stringBuilder.Append(DescriptionPrefix).Append(' ');
		}
		stringBuilder.Append(Description);
		if (!IgnorePosition)
		{
			int descriptionPosition = GetDescriptionPosition();
			if (descriptionPosition != 1)
			{
				stringBuilder.Append(" (").Append(descriptionPosition).Append(')');
			}
		}
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public string GetOrdinalDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		string text = (Abstract ? null : BodyPartCategory.GetColor(Category));
		if (text != null)
		{
			stringBuilder.Append("{{").Append(text).Append('|');
		}
		if (!IgnorePosition && GetPartDescriptionCount(Description) != 1)
		{
			int descriptionPosition = GetDescriptionPosition();
			stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(Grammar.Ordinal(descriptionPosition))).Append(' ');
		}
		if (!string.IsNullOrEmpty(DescriptionPrefix))
		{
			stringBuilder.Append(DescriptionPrefix).Append(' ');
		}
		stringBuilder.Append(Description);
		if (text != null)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}

	public bool IsConcretelyDependent()
	{
		return DependsOn != null;
	}

	public bool IsAbstractlyDependent()
	{
		return RequiresType != null;
	}

	public bool IsDependent()
	{
		if (!IsConcretelyDependent())
		{
			return IsAbstractlyDependent();
		}
		return true;
	}

	public bool IsConcretelyUnsupported(Body UseParentBody = null)
	{
		if (DependsOn == null)
		{
			return false;
		}
		if (UseParentBody != null)
		{
			return UseParentBody.GetPartBySupportsDependent(DependsOn) == null;
		}
		if (ParentBody != null)
		{
			return ParentBody.GetPartBySupportsDependent(DependsOn) == null;
		}
		return GetPartBySupportsDependent(DependsOn) == null;
	}

	public bool IsAbstractlyUnsupported(Body UseParentBody = null)
	{
		if (RequiresType == null)
		{
			return false;
		}
		if (UseParentBody != null)
		{
			return UseParentBody.GetFirstPart(RequiresType, RequiresLaterality) == null;
		}
		if (ParentBody != null)
		{
			return ParentBody.GetFirstPart(RequiresType, RequiresLaterality) == null;
		}
		return GetFirstPart(RequiresType, RequiresLaterality) == null;
	}

	public bool IsUnsupported(Body UseParentBody = null)
	{
		if (!IsConcretelyUnsupported(UseParentBody))
		{
			return IsAbstractlyUnsupported(UseParentBody);
		}
		return true;
	}

	public bool IsSeverable()
	{
		if (Abstract)
		{
			return false;
		}
		if (!Appendage)
		{
			return false;
		}
		if (Integral)
		{
			return false;
		}
		if (IsDependent())
		{
			return false;
		}
		return true;
	}

	public bool SeverRequiresDecapitate()
	{
		return Mortal;
	}

	public bool IsRegenerable()
	{
		if (IsSeverable())
		{
			return PreventRegenerationBecause == null;
		}
		return false;
	}

	public bool IsRecoverable(Body UseParentBody = null)
	{
		if (IsDependent())
		{
			return !IsUnsupported(UseParentBody);
		}
		return false;
	}

	public void FindUnsupportedParts(ref List<BodyPart> Result)
	{
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null)
				{
					Parts[i].FindUnsupportedParts(ref Result);
				}
			}
		}
		if (IsUnsupported())
		{
			if (Result == null)
			{
				Result = new List<BodyPart>(1) { this };
			}
			else
			{
				Result.Add(this);
			}
		}
	}

	public List<BodyPart> FindUnsupportedParts()
	{
		List<BodyPart> Result = null;
		FindUnsupportedParts(ref Result);
		return Result;
	}

	public bool HasLaterality(int checkLaterality)
	{
		return (Laterality & checkLaterality) == checkLaterality;
	}

	public bool IsFirstSlotForEquipped()
	{
		if (Equipped == null)
		{
			return false;
		}
		if (ParentBody != null)
		{
			return ParentBody.FindEquippedItem(Equipped) == this;
		}
		return true;
	}

	public bool IsFirstSlotForCybernetics()
	{
		if (Cybernetics == null)
		{
			return false;
		}
		if (ParentBody != null)
		{
			return ParentBody.FindCybernetics(Cybernetics) == this;
		}
		return true;
	}

	public bool IsFirstSlotForDefaultBehavior()
	{
		if (DefaultBehavior == null)
		{
			return false;
		}
		if (ParentBody != null)
		{
			return ParentBody.FindDefaultBehavior(DefaultBehavior) == this;
		}
		return true;
	}

	public void GetTypeArmorInfo(string ForType, ref GameObject First, ref int Count, ref int AV, ref int DV)
	{
		if (ForType == Type)
		{
			Count++;
			if (Equipped != null && Equipped.GetPart("Armor") is Armor armor && (armor.WornOn == Type || armor.WornOn == "*") && IsFirstSlotForEquipped())
			{
				if (First == null)
				{
					First = Equipped;
				}
				AV += armor.AV;
				DV += armor.DV;
			}
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetTypeArmorInfo(ForType, ref First, ref Count, ref AV, ref DV);
			}
		}
	}

	public void RecalculateArmor()
	{
		if (Equipped != null && Equipped.GetPart("Armor") is Armor armor && (armor.WornOn == Type || armor.WornOn == "*") && IsFirstSlotForEquipped())
		{
			armor.RecalculateArmor();
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateArmor();
			}
		}
	}

	public void RecalculateArmorExcept(GameObject obj)
	{
		if (Equipped != null && Equipped != obj && Equipped.GetPart("Armor") is Armor armor && (armor.WornOn == Type || armor.WornOn == "*") && IsFirstSlotForEquipped())
		{
			armor.RecalculateArmor();
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateArmorExcept(obj);
			}
		}
	}

	public void RecalculateTypeArmor(string ForType)
	{
		if (ForType == Type && Equipped != null && Equipped.GetPart("Armor") is Armor armor && (armor.WornOn == Type || armor.WornOn == "*") && IsFirstSlotForEquipped())
		{
			armor.RecalculateArmor();
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateTypeArmor(ForType);
			}
		}
	}

	public void RecalculateTypeArmorExcept(string ForType, GameObject obj)
	{
		if (ForType == Type && Equipped != null && Equipped != obj && Equipped.GetPart("Armor") is Armor armor && (armor.WornOn == Type || armor.WornOn == "*") && IsFirstSlotForEquipped())
		{
			armor.RecalculateArmor();
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].RecalculateTypeArmorExcept(ForType, obj);
			}
		}
	}

	public bool CheckSlotEquippedMatch(GameObject obj, List<string> Slots)
	{
		if (obj == Equipped && (Slots.Contains(Type) || Slots.Contains("*")))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotEquippedMatch(obj, Slots))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotEquippedMatch(GameObject obj, string Slot)
	{
		if (obj == Equipped && (Slot == Type || Slot == "*"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotEquippedMatch(obj, Slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, List<string> Slots)
	{
		if (obj == Cybernetics && (Slots.Contains(Type) || Slots.Contains("*")))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotCyberneticsMatch(obj, Slots))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckSlotCyberneticsMatch(GameObject obj, string Slot)
	{
		if (obj == Cybernetics && (Slot == Type || Slot == "*"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].CheckSlotCyberneticsMatch(obj, Slot))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasReadyMissileWeapon()
	{
		if (Equipped != null && Equipped.GetPart("MissileWeapon") is MissileWeapon missileWeapon && missileWeapon.FiresManually && missileWeapon.ValidSlotType(Type) && missileWeapon.ReadyToFire())
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.GetPart("MissileWeapon") is MissileWeapon missileWeapon2 && missileWeapon2.FiresManually && missileWeapon2.ValidSlotType(Type) && missileWeapon2.ReadyToFire())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].HasReadyMissileWeapon())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMissileWeapon()
	{
		if (Equipped != null && Equipped.GetPart("MissileWeapon") is MissileWeapon missileWeapon && missileWeapon.FiresManually && missileWeapon.ValidSlotType(Type))
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.GetPart("MissileWeapon") is MissileWeapon missileWeapon2 && missileWeapon2.FiresManually && missileWeapon2.ValidSlotType(Type))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i] != null && Parts[i].HasMissileWeapon())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GetMissileWeapons(ref List<GameObject> List)
	{
		if (Equipped != null && (List == null || !List.Contains(Equipped)) && Equipped.GetPart("MissileWeapon") is MissileWeapon missileWeapon && missileWeapon.FiresManually && missileWeapon.ValidSlotType(Type) && IsFirstSlotForEquipped())
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && (List == null || !List.Contains(Cybernetics)) && Cybernetics.GetPart("MissileWeapon") is MissileWeapon missileWeapon2 && missileWeapon2.FiresManually && missileWeapon2.ValidSlotType(Type) && IsFirstSlotForCybernetics())
		{
			if (List == null)
			{
				List = Event.NewGameObjectList();
			}
			List.Add(Cybernetics);
		}
		if (Parts == null)
		{
			return;
		}
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			if (Parts[i] != null)
			{
				Parts[i].GetMissileWeapons(ref List);
			}
		}
	}

	public bool IsADefaultBehavior(GameObject obj)
	{
		if (obj == DefaultBehavior)
		{
			return obj != null;
		}
		if (Parts != null)
		{
			foreach (BodyPart part in Parts)
			{
				if (part.IsADefaultBehavior(obj))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool WantTurnTick()
	{
		if (Equipped != null && Equipped.WantTurnTick())
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.WantTurnTick())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantTurnTick())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TurnTick(long TurnNumber)
	{
		if (Equipped != null)
		{
			Equipped.TurnTick(TurnNumber);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			Cybernetics.TurnTick(TurnNumber);
		}
		if (Parts == null)
		{
			return;
		}
		BodyPart bodyPart = null;
		BodyPart bodyPart2 = null;
		BodyPart bodyPart3 = null;
		List<BodyPart> list = null;
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			BodyPart bodyPart4 = Parts[i];
			if (bodyPart4.WantTurnTick())
			{
				if (list != null)
				{
					list.Add(bodyPart4);
					continue;
				}
				if (bodyPart == null)
				{
					bodyPart = bodyPart4;
					continue;
				}
				if (bodyPart2 == null)
				{
					bodyPart2 = bodyPart4;
					continue;
				}
				if (bodyPart3 == null)
				{
					bodyPart3 = bodyPart4;
					continue;
				}
				list = new List<BodyPart>(4) { bodyPart, bodyPart2, bodyPart3, bodyPart4 };
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
		else if (bodyPart != null)
		{
			bodyPart.TurnTick(TurnNumber);
			if (bodyPart2 != null)
			{
				bodyPart2.TurnTick(TurnNumber);
				bodyPart3?.TurnTick(TurnNumber);
			}
		}
	}

	public bool WantTenTurnTick()
	{
		if (Equipped != null && Equipped.WantTenTurnTick())
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.WantTenTurnTick())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantTenTurnTick())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TenTurnTick(long TurnNumber)
	{
		if (Equipped != null)
		{
			Equipped.TenTurnTick(TurnNumber);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			Cybernetics.TenTurnTick(TurnNumber);
		}
		if (Parts == null)
		{
			return;
		}
		BodyPart bodyPart = null;
		BodyPart bodyPart2 = null;
		BodyPart bodyPart3 = null;
		List<BodyPart> list = null;
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			BodyPart bodyPart4 = Parts[i];
			if (bodyPart4.WantTenTurnTick())
			{
				if (list != null)
				{
					list.Add(bodyPart4);
					continue;
				}
				if (bodyPart == null)
				{
					bodyPart = bodyPart4;
					continue;
				}
				if (bodyPart2 == null)
				{
					bodyPart2 = bodyPart4;
					continue;
				}
				if (bodyPart3 == null)
				{
					bodyPart3 = bodyPart4;
					continue;
				}
				list = new List<BodyPart>(4) { bodyPart, bodyPart2, bodyPart3, bodyPart4 };
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
		else if (bodyPart != null)
		{
			bodyPart.TenTurnTick(TurnNumber);
			if (bodyPart2 != null)
			{
				bodyPart2.TenTurnTick(TurnNumber);
				bodyPart3?.TenTurnTick(TurnNumber);
			}
		}
	}

	public bool WantHundredTurnTick()
	{
		if (Equipped != null && Equipped.WantHundredTurnTick())
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.WantHundredTurnTick())
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantHundredTurnTick())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void HundredTurnTick(long TurnNumber)
	{
		if (Equipped != null)
		{
			Equipped.HundredTurnTick(TurnNumber);
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			Cybernetics.HundredTurnTick(TurnNumber);
		}
		if (Parts == null)
		{
			return;
		}
		BodyPart bodyPart = null;
		BodyPart bodyPart2 = null;
		BodyPart bodyPart3 = null;
		List<BodyPart> list = null;
		int i = 0;
		for (int count = Parts.Count; i < count; i++)
		{
			BodyPart bodyPart4 = Parts[i];
			if (bodyPart4.WantHundredTurnTick())
			{
				if (list != null)
				{
					list.Add(bodyPart4);
					continue;
				}
				if (bodyPart == null)
				{
					bodyPart = bodyPart4;
					continue;
				}
				if (bodyPart2 == null)
				{
					bodyPart2 = bodyPart4;
					continue;
				}
				if (bodyPart3 == null)
				{
					bodyPart3 = bodyPart4;
					continue;
				}
				list = new List<BodyPart>(4) { bodyPart, bodyPart2, bodyPart3, bodyPart4 };
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
		else if (bodyPart != null)
		{
			bodyPart.HundredTurnTick(TurnNumber);
			if (bodyPart2 != null)
			{
				bodyPart2.HundredTurnTick(TurnNumber);
				bodyPart3?.HundredTurnTick(TurnNumber);
			}
		}
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (!MinEvent.CascadeTo(cascade, 16) || Type != "Thrown Weapon")
		{
			if (Equipped != null && Equipped.WantEvent(ID, cascade))
			{
				return true;
			}
			if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.WantEvent(ID, cascade))
			{
				return true;
			}
			if (DefaultBehavior != null && DefaultBehavior != Equipped && DefaultBehavior != Cybernetics && DefaultBehavior.WantEvent(ID, cascade))
			{
				return true;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantEvent(ID, cascade))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HandleEvent(MinEvent E)
	{
		if (!E.CascadeTo(16) || Type != "Thrown Weapon")
		{
			if (Equipped != null && Equipped.WantEvent(E.GetID(), E.GetCascadeLevel()) && IsFirstSlotForEquipped() && !Equipped.HandleEvent(E))
			{
				return false;
			}
			if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.WantEvent(E.GetID(), E.GetCascadeLevel()) && IsFirstSlotForCybernetics() && !Cybernetics.HandleEvent(E))
			{
				return false;
			}
			if (DefaultBehavior != null && DefaultBehavior != Equipped && DefaultBehavior != Cybernetics && DefaultBehavior.WantEvent(E.GetID(), E.GetCascadeLevel()) && IsFirstSlotForDefaultBehavior() && !DefaultBehavior.HandleEvent(E))
			{
				return false;
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (!bodyPart.HandleEvent(E))
				{
					return false;
				}
				if (count != Parts.Count)
				{
					count = Parts.Count;
					if (i < count && Parts[i] != bodyPart)
					{
						i--;
					}
				}
			}
		}
		return true;
	}

	public bool ProcessQuerySlotList(QuerySlotListEvent E)
	{
		if ((Equipped == null || Equipped.FireEvent("CanBeUnequipped")) && (Cybernetics == null || !Cybernetics.HasTag("CyberneticsUsesEqSlot") || Cybernetics.FireEvent("CanBeUnequipped")) && (DefaultBehavior == null || !DefaultBehavior.HasRegisteredEvent("CanEquipOverDefaultBehavior") || DefaultBehavior.FireEvent(Event.New("CanEquipOverDefaultBehavior", "Subject", ParentBody.ParentObject, "Object", E.Object, "Part", this))))
		{
			QueryEquippableListEvent queryEquippableListEvent = QueryEquippableListEvent.FromPool(ParentBody.ParentObject, E.Object, Type);
			E.Object.HandleEvent(queryEquippableListEvent);
			if (queryEquippableListEvent.List.Count > 0)
			{
				E.SlotList.Add(this);
			}
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				BodyPart bodyPart = Parts[i];
				if (!bodyPart.ProcessQuerySlotList(E))
				{
					return false;
				}
				if (Parts.Count != count)
				{
					count = Parts.Count;
					if (i < count && Parts[i] != bodyPart)
					{
						i--;
					}
				}
			}
		}
		return true;
	}

	public bool ProcessExtrinsicValue(GetExtrinsicValueEvent E)
	{
		if (Equipped != null)
		{
			E.Value += Equipped.Value;
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			E.Value += Cybernetics.Value;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (!Parts[i].ProcessExtrinsicValue(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ProcessExtrinsicWeight(GetExtrinsicWeightEvent E)
	{
		if (Equipped != null)
		{
			E.Weight += Equipped.GetWeight();
		}
		if (Cybernetics != null && Cybernetics != Equipped)
		{
			E.Weight += Cybernetics.GetWeight();
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (!Parts[i].ProcessExtrinsicWeight(E))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void GetCreatureFromSpec(string Blueprint, string Type, string Tag, string Base, Predicate<GameObjectBlueprint> Filter, ref GameObject Creature)
	{
		if (Blueprint != null)
		{
			Creature = GameObject.create(Blueprint);
		}
		else if (Tag != null && Base != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && BP.DescendsFrom(Base));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && BP.DescendsFrom(Base) && Filter(BP));
			}
		}
		else if (Tag != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.HasTag(Tag) && Filter(BP));
			}
		}
		else if (Base != null)
		{
			if (Filter == null)
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.DescendsFrom(Base));
			}
			else
			{
				Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && BP.DescendsFrom(Base) && Filter(BP));
			}
		}
		else if (Filter == null)
		{
			Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body"));
		}
		else
		{
			Creature = EncountersAPI.GetACreature((GameObjectBlueprint BP) => BP.HasPart("Body") && Filter(BP));
		}
	}

	public static GameObject MakeSeveredBodyPart(string Blueprint = null, string Type = null, string Tag = null, string Base = null, Predicate<GameObjectBlueprint> Filter = null, GameObject PutIn = null)
	{
		GameObject Creature = null;
		int num = 0;
		while (++num < 10)
		{
			try
			{
				GetCreatureFromSpec(Blueprint, Type, Tag, Base, Filter, ref Creature);
				if (Creature == null)
				{
					return null;
				}
				Body body = Creature.Body;
				BodyPart bodyPart;
				if (Type == null)
				{
					SeverWork.Clear();
					foreach (BodyPart item in body.LoopParts())
					{
						if (item.IsSeverable())
						{
							SeverWork.Add(item);
						}
					}
					bodyPart = SeverWork.GetRandomElement();
				}
				else
				{
					bodyPart = body.GetFirstPart(Type);
				}
				if (bodyPart != null && bodyPart.IsSeverable())
				{
					GameObject gameObject = bodyPart.Dismember();
					if (gameObject != null)
					{
						PutIn?.FireEvent(Event.New("CommandTakeObject", "Object", gameObject, "EnergyCost", 0).SetSilent(Silent: true));
						return gameObject;
					}
				}
			}
			finally
			{
				Creature?.Obliterate();
			}
			if (Blueprint != null)
			{
				return null;
			}
		}
		return null;
	}

	public static void MakeSeveredBodyParts(int Number, string Blueprint = null, string Type = null, string Tag = null, string Base = null, Predicate<GameObjectBlueprint> Filter = null, GameObject PutIn = null, List<GameObject> Return = null)
	{
		GameObject Creature = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (++num2 < 10)
		{
			while (true)
			{
				GameObject gameObject = Creature;
				GetCreatureFromSpec(Blueprint, Type, Tag, Base, Filter, ref Creature);
				if (Creature == null)
				{
					break;
				}
				Body body = Creature.Body;
				BodyPart bodyPart;
				if (Type == null)
				{
					SeverWork.Clear();
					foreach (BodyPart item in body.LoopParts())
					{
						if (item.IsSeverable())
						{
							SeverWork.Add(item);
						}
					}
					bodyPart = SeverWork.GetRandomElement();
					if (Creature == gameObject)
					{
						if (num - SeverWork.Count >= Number / 2)
						{
							Creature.Obliterate();
							Creature = null;
							continue;
						}
					}
					else
					{
						num = SeverWork.Count;
					}
				}
				else
				{
					bodyPart = body.GetFirstPart(Type);
				}
				try
				{
					if (bodyPart != null && bodyPart.IsSeverable())
					{
						GameObject gameObject2 = bodyPart.Dismember();
						if (gameObject2 != null)
						{
							PutIn?.FireEvent(Event.New("CommandTakeObject", "Object", gameObject2, "EnergyCost", 0).SetSilent(Silent: true));
							Return?.Add(gameObject2);
							if (++num3 >= Number)
							{
								break;
							}
							continue;
						}
					}
				}
				finally
				{
					if (Blueprint == null)
					{
						Creature.Obliterate();
						Creature = null;
					}
				}
				goto IL_013f;
			}
			break;
			IL_013f:
			if (Blueprint != null)
			{
				break;
			}
		}
		Creature?.Obliterate();
	}

	public void TypeDump(StringBuilder SB)
	{
		SB.Append(Type).Append(' ').Append(VariantType)
			.Append(' ')
			.Append(Name)
			.Append('\n');
		if (Parts == null)
		{
			return;
		}
		foreach (BodyPart part in Parts)
		{
			part.TypeDump(SB);
		}
	}

	public void GetMobilityProvidingParts(List<BodyPart> Return)
	{
		if (Mobility > 0)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetMobilityProvidingParts(Return);
			}
		}
	}

	public void GetConcreteMobilityProvidingParts(List<BodyPart> Return)
	{
		if (Mobility > 0 && !Abstract)
		{
			Return.Add(this);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetConcreteMobilityProvidingParts(Return);
			}
		}
	}

	public void GetContents(List<GameObject> Return)
	{
		if (Equipped != null && !Return.Contains(Equipped))
		{
			Return.Add(Equipped);
		}
		if (Cybernetics != null && Cybernetics != Equipped && !Return.Contains(Cybernetics))
		{
			Return.Add(Cybernetics);
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].GetContents(Return);
			}
		}
	}

	public bool WantsEndTurnEvent()
	{
		if (Equipped != null && Equipped.HasRegisteredEvent("EndTurn"))
		{
			return true;
		}
		if (Cybernetics != null && Cybernetics != Equipped && Cybernetics.HasRegisteredEvent("EndTurn"))
		{
			return true;
		}
		if (DefaultBehavior != null && DefaultBehavior.HasRegisteredEvent("EndTurn"))
		{
			return true;
		}
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				if (Parts[i].WantsEndTurnEvent())
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetPartDepth(BodyPart Part, int Depth)
	{
		if (Part == this)
		{
			if (!Contact && Depth == 1)
			{
				return 0;
			}
			return Depth;
		}
		if (Parts != null)
		{
			Depth++;
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				int partDepth = Parts[i].GetPartDepth(Part, Depth);
				if (partDepth != -1)
				{
					return partDepth;
				}
			}
		}
		return -1;
	}

	public void ModifyNameAndDescriptionRecursively(string NameMod, string DescMod)
	{
		Name = NameMod + "-" + Name;
		Description = DescMod + "-" + Description;
		if (Parts != null)
		{
			int i = 0;
			for (int count = Parts.Count; i < count; i++)
			{
				Parts[i].ModifyNameAndDescriptionRecursively(NameMod, DescMod);
			}
		}
	}

	public bool IsCategoryLive()
	{
		return BodyPartCategory.IsLiveCategory(Category);
	}

	public bool CanReceiveCyberneticImplant()
	{
		if (Extrinsic)
		{
			return false;
		}
		if (Category != 1)
		{
			return false;
		}
		return true;
	}
}
