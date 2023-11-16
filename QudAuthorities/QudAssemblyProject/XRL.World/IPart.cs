using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConsoleLib.Console;
using XRL.Core;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World;

[Serializable]
public class IPart : IComponent<GameObject>
{
	public GameObject _ParentObject;

	[NonSerialized]
	private string _Name;

	[NonSerialized]
	public static Dictionary<TypeField, FieldSaveVersion> typeFieldSaveVersionInfo = new Dictionary<TypeField, FieldSaveVersion>(new TypeFieldComparer());

	[NonSerialized]
	private static readonly Dictionary<IPart, Dictionary<FieldInfo, Guid>> _FinalizeLoadAbilities = new Dictionary<IPart, Dictionary<FieldInfo, Guid>>();

	[NonSerialized]
	private static readonly Dictionary<IPart, Dictionary<FieldInfo, AACopyFinalEntry>> _FinalizeCopyAbilities = new Dictionary<IPart, Dictionary<FieldInfo, AACopyFinalEntry>>();

	[NonSerialized]
	private StatShifter _StatShifter;

	public virtual GameObject ParentObject
	{
		get
		{
			return _ParentObject;
		}
		set
		{
			if (_StatShifter != null)
			{
				_StatShifter.Owner = value;
			}
			_ParentObject = value;
		}
	}

	public string Name
	{
		get
		{
			if (_Name == null)
			{
				_Name = ModManager.ResolveTypeName(GetType());
			}
			return _Name;
		}
		set
		{
			if (value == Name)
			{
				MetricsManager.LogCallingModError("You do not need to set the name of the part " + Name + ", please remove the attempt to set it");
				return;
			}
			MetricsManager.LogCallingModError("You cannot set the name of the part " + Name + " to " + value + ", please remove the attempt to set it");
		}
	}

	private Dictionary<FieldInfo, Guid> FinalizeLoadAbilities
	{
		get
		{
			if (_FinalizeLoadAbilities.TryGetValue(this, out var value))
			{
				return value;
			}
			value = new Dictionary<FieldInfo, Guid>();
			_FinalizeLoadAbilities.Add(this, value);
			return value;
		}
	}

	private Dictionary<FieldInfo, AACopyFinalEntry> FinalizeCopyAbilities
	{
		get
		{
			if (_FinalizeCopyAbilities.TryGetValue(this, out var value))
			{
				return value;
			}
			value = new Dictionary<FieldInfo, AACopyFinalEntry>();
			_FinalizeCopyAbilities.Add(this, value);
			return value;
		}
	}

	public StatShifter StatShifter
	{
		get
		{
			if (_StatShifter == null)
			{
				_StatShifter = new StatShifter(ParentObject, null);
			}
			return _StatShifter;
		}
	}

	public override GameObject GetComponentBasis()
	{
		return _ParentObject;
	}

	public virtual void UpdateImposter(QudScreenBufferExtra extra)
	{
	}

	public virtual void OnPaint(ScreenBuffer buffer)
	{
	}

	public virtual bool AllowStaticRegistration()
	{
		return false;
	}

	public virtual bool CanGenerateStacked()
	{
		return SameAs(this);
	}

	public virtual bool IsPoolabe()
	{
		return false;
	}

	public virtual bool PoolReset()
	{
		_ParentObject = null;
		_Name = null;
		return true;
	}

	public virtual void Register(GameObject Object)
	{
	}

	public virtual void Attach()
	{
	}

	public virtual void Initialize()
	{
	}

	public virtual void AddedAfterCreation()
	{
	}

	public virtual void Remove()
	{
	}

	public virtual void ObjectLoaded()
	{
	}

	public virtual string[] GetStaticEvents()
	{
		return null;
	}

	public void RegisterStaticEvents(GameObject GO)
	{
		string[] staticEvents = GetStaticEvents();
		if (staticEvents != null)
		{
			int i = 0;
			for (int num = staticEvents.Length; i < num; i++)
			{
				GO.RegisterPartEvent(this, staticEvents[i]);
			}
		}
	}

	public bool IsStaticEvent(string ID)
	{
		string[] staticEvents = GetStaticEvents();
		if (staticEvents == null)
		{
			return false;
		}
		int i = 0;
		for (int num = staticEvents.Length; i < num; i++)
		{
			if (staticEvents[i] == ID)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void LoadBlueprint()
	{
	}

	public virtual bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	public virtual bool Render(RenderEvent E)
	{
		return true;
	}

	public virtual bool OverlayRender(RenderEvent E)
	{
		return true;
	}

	public virtual bool FinalRender(RenderEvent E, bool bAlt)
	{
		return true;
	}

	protected static void MarkTypeFieldWithSaveVersion(TypeField typeField, FieldSaveVersion saveVersion)
	{
		if (typeFieldSaveVersionInfo.ContainsKey(typeField))
		{
			if (typeFieldSaveVersionInfo[typeField].minimumSaveVersion < saveVersion.minimumSaveVersion)
			{
				typeFieldSaveVersionInfo[typeField] = saveVersion;
			}
		}
		else
		{
			typeFieldSaveVersionInfo.Add(typeField, saveVersion);
		}
	}

	protected static void MarkTypeFieldWithSaveVersion(Type type, FieldInfo field, FieldSaveVersion saveVersion)
	{
		MarkTypeFieldWithSaveVersion(new TypeField(type, field), saveVersion);
	}

	protected static void MarkTypeFieldWithSaveVersion(Type type, string field, FieldSaveVersion saveVersion)
	{
		MarkTypeFieldWithSaveVersion(new TypeField(type, field), saveVersion);
	}

	protected static void MarkTypeFieldWithSaveVersion(Type type, FieldInfo field, int saveVersion)
	{
		MarkTypeFieldWithSaveVersion(new TypeField(type, field), new FieldSaveVersion(saveVersion));
	}

	protected static void MarkTypeFieldWithSaveVersion(Type type, string field, int saveVersion)
	{
		MarkTypeFieldWithSaveVersion(new TypeField(type, field), new FieldSaveVersion(saveVersion));
	}

	protected static void MarkParentTypeFieldsWithSaveVersion(Type parentType, Type childType, FieldSaveVersion saveVersion)
	{
		FieldInfo[] fields = parentType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			MarkTypeFieldWithSaveVersion(childType, fieldInfo.Name, saveVersion);
		}
	}

	protected static void MarkParentTypeFieldsWithSaveVersion(Type parentType, Type childType, int saveVersion)
	{
		MarkParentTypeFieldsWithSaveVersion(parentType, childType, new FieldSaveVersion(saveVersion));
	}

	protected void MarkParentTypeFieldsWithSaveVersion(Type parentType, int saveVersion)
	{
		MarkParentTypeFieldsWithSaveVersion(parentType, GetType(), new FieldSaveVersion(saveVersion));
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(GetType().FullName);
		Writer.WriteGameObject(ParentObject);
		if (_StatShifter == null)
		{
			Writer.Write(value: false);
		}
		else
		{
			Writer.Write(value: true);
			_StatShifter.Save(Writer);
		}
		SaveData(Writer);
	}

	public virtual void SaveData(SerializationWriter Writer)
	{
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsStatic || fieldInfo.IsNotSerialized || fieldInfo.FieldType.FullName == "XRL.World.Parts.Physics" || fieldInfo.FieldType.FullName == "XRL.World.BodyPart" || fieldInfo.Name == "Objects" || fieldInfo.Name == "MutationList")
			{
				continue;
			}
			if (fieldInfo.FieldType.FullName == "XRL.World.Cell")
			{
				Cell cell = (Cell)fieldInfo.GetValue(this);
				if (cell == null || cell.ParentZone.ZoneID == null)
				{
					Writer.Write(value: true);
					continue;
				}
				Writer.Write(value: false);
				Writer.Write(cell.ParentZone.ZoneID);
				Writer.Write(cell.X);
				Writer.Write(cell.Y);
			}
			else if (fieldInfo.FieldType.FullName.Contains("ActivatedAbilityEntry"))
			{
				if (fieldInfo.GetValue(this) == null)
				{
					Writer.Write(Guid.Empty);
				}
				else
				{
					Writer.Write(((ActivatedAbilityEntry)fieldInfo.GetValue(this)).ID);
				}
			}
			else if (fieldInfo.FieldType.FullName == "XRL.World.GameObject")
			{
				GameObject gameObject = (GameObject)fieldInfo.GetValue(this);
				if (gameObject != null && gameObject.IsPlayer())
				{
					Writer.Write(value: true);
					continue;
				}
				Writer.Write(value: false);
				Writer.WriteGameObject((GameObject)fieldInfo.GetValue(this));
			}
			else
			{
				Writer.WriteObject(fieldInfo.GetValue(this));
			}
		}
	}

	public static IPart Load(SerializationReader Reader)
	{
		string text = Reader.ReadString();
		try
		{
			Type type = ModManager.ResolveType(text);
			IPart part = (IPart)GameObjectFactory.Factory.CreateInstance(type);
			part.ParentObject = Reader.ReadGameObject();
			if (Reader.ReadBoolean())
			{
				part._StatShifter = StatShifter.Load(Reader, part.ParentObject);
			}
			part.LoadData(Reader);
			return part;
		}
		catch (Exception ex)
		{
			ex.Data["TypeName"] = text;
			throw;
		}
	}

	public virtual void LoadData(SerializationReader Reader)
	{
		FieldInfo fieldInfo = null;
		Type type = null;
		try
		{
			type = GetType();
			TypeField typeField = new TypeField(type);
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo fieldInfo2 in fields)
			{
				fieldInfo = fieldInfo2;
				if (fieldInfo2.IsStatic || fieldInfo2.IsNotSerialized || fieldInfo2.Name == "Objects" || fieldInfo2.Name == "MutationList")
				{
					continue;
				}
				string fullName = fieldInfo2.FieldType.FullName;
				if (fullName == "XRL.World.Parts.Physics" || fullName == "XRL.World.BodyPart")
				{
					continue;
				}
				FieldSaveVersion value = null;
				if (!IComponent<GameObject>.fieldSaveVersionInfo.TryGetValue(fieldInfo2, out value))
				{
					value = (FieldSaveVersion)Attribute.GetCustomAttribute(fieldInfo2, typeof(FieldSaveVersion));
					IComponent<GameObject>.fieldSaveVersionInfo.Add(fieldInfo2, value);
				}
				typeField.F = fieldInfo2;
				FieldSaveVersion value2 = null;
				if (typeFieldSaveVersionInfo.TryGetValue(typeField, out value2) && (value == null || value2.minimumSaveVersion > value.minimumSaveVersion))
				{
					value = value2;
				}
				if (value != null && value.minimumSaveVersion > -1 && value.minimumSaveVersion > Reader.FileVersion)
				{
					continue;
				}
				if (fullName == "XRL.World.Cell")
				{
					if (Reader.ReadBoolean())
					{
						fieldInfo2.SetValue(this, null);
						continue;
					}
					string zoneID = Reader.ReadString();
					int x = Reader.ReadInt32();
					int y = Reader.ReadInt32();
					fieldInfo2.SetValue(this, XRLCore.Core.Game.ZoneManager.GetZone(zoneID).GetCell(x, y));
				}
				else if (fullName.Contains("ActivatedAbilityEntry"))
				{
					Guid guid = Reader.ReadGuid();
					if (guid != Guid.Empty)
					{
						FinalizeLoadAbilities.Add(fieldInfo2, guid);
					}
				}
				else if (fullName == "XRL.World.GameObject")
				{
					if (Reader.ReadBoolean())
					{
						fieldInfo2.SetValue(this, XRLCore.Core.Game.Player.Body);
					}
					else
					{
						fieldInfo2.SetValue(this, Reader.ReadGameObject((fieldInfo2.Name == "_ParentObject" || fieldInfo2.Name == "_InInventory") ? null : (GetType().Name + "::" + fieldInfo2.Name)));
					}
				}
				else if (fieldInfo2.IsLiteral)
				{
					Reader.ReadObject();
				}
				else
				{
					fieldInfo2.SetValue(this, Reader.ReadObject());
				}
			}
		}
		catch (Exception ex)
		{
			if ((object)type != null)
			{
				ex.Data["Type"] = type;
			}
			if ((object)fieldInfo != null)
			{
				ex.Data["Field"] = fieldInfo;
			}
			throw;
		}
	}

	public virtual void FinalizeLoad()
	{
		if (FinalizeLoadAbilities == null)
		{
			return;
		}
		ActivatedAbilities activatedAbilities = ParentObject.GetPart("ActivatedAbilities") as ActivatedAbilities;
		foreach (FieldInfo key in FinalizeLoadAbilities.Keys)
		{
			if (activatedAbilities != null && key != null && activatedAbilities.AbilityByGuid.ContainsKey(FinalizeLoadAbilities[key]))
			{
				key.SetValue(this, activatedAbilities.AbilityByGuid[FinalizeLoadAbilities[key]]);
			}
		}
		_FinalizeLoadAbilities.Remove(this);
	}

	public static void LoadComplete()
	{
		if (_FinalizeLoadAbilities != null)
		{
			_FinalizeCopyAbilities.Clear();
		}
	}

	public virtual IPart DeepCopy(GameObject Parent)
	{
		IPart part = (IPart)Activator.CreateInstance(GetType());
		Dictionary<string, string> source = new Dictionary<string, string>();
		Dictionary<string, string> dest = new Dictionary<string, string>();
		source.ToList().ForEach(delegate(KeyValuePair<string, string> kv)
		{
			dest.Add(kv.Key, kv.Value);
		});
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) != 0 || fieldInfo.IsLiteral)
			{
				continue;
			}
			if (fieldInfo.FieldType.FullName.Contains("ActivatedAbilityEntry"))
			{
				if (fieldInfo.GetValue(this) != null)
				{
					part.FinalizeCopyAbilities.Add(fieldInfo, new AACopyFinalEntry(((ActivatedAbilityEntry)fieldInfo.GetValue(this)).ID, part));
				}
			}
			else
			{
				fieldInfo.SetValue(part, fieldInfo.GetValue(this));
			}
		}
		part.ParentObject = Parent;
		return part;
	}

	public virtual IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		return DeepCopy(Parent);
	}

	public virtual void FinalizeCopyEarly(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		if (FinalizeCopyAbilities == null || !(ParentObject.GetPart("ActivatedAbilities") is ActivatedAbilities activatedAbilities))
		{
			return;
		}
		foreach (FieldInfo key in FinalizeCopyAbilities.Keys)
		{
			key.SetValue(FinalizeCopyAbilities[key].Part, activatedAbilities.AbilityByGuid[FinalizeCopyAbilities[key].ID]);
		}
		_FinalizeCopyAbilities.Remove(this);
	}

	public virtual void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
	}

	public virtual void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
	}

	public override string ToString()
	{
		return Name;
	}

	public virtual bool SameAs(IPart p)
	{
		return Name == p.Name;
	}

	public void ForceDropSelf()
	{
		if (ParentObject.CurrentCell != null)
		{
			return;
		}
		if (ParentObject.Equipped != null)
		{
			BodyPart bodyPart = ParentObject.Equipped.Body.FindEquippedItem(ParentObject);
			if (bodyPart != null)
			{
				ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart));
			}
		}
		if (ParentObject.InInventory != null)
		{
			ParentObject.InInventory.FireEvent(Event.New("CommandDropObject", "Object", ParentObject));
		}
	}

	public bool ShouldUsePsychometry(GameObject who)
	{
		if (!(who.GetPart("Psychometry") is Psychometry psychometry))
		{
			return false;
		}
		return psychometry.Advisable(ParentObject);
	}

	public bool ShouldUsePsychometry()
	{
		return ShouldUsePsychometry(IComponent<GameObject>.ThePlayer);
	}

	public bool UsePsychometry(GameObject who)
	{
		if (!(who.GetPart("Psychometry") is Psychometry psychometry))
		{
			return false;
		}
		return psychometry.Activate(ParentObject);
	}

	public bool UsePsychometry()
	{
		return UsePsychometry(IComponent<GameObject>.ThePlayer);
	}
}
