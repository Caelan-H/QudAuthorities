using System;
using System.Collections.Generic;
using System.Reflection;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Wish;

namespace XRL.World;

[Serializable]
[HasWishCommand]
public class Effect : IComponent<GameObject>
{
	public const int TYPE_GENERAL = 1;

	public const int TYPE_MENTAL = 2;

	public const int TYPE_METABOLIC = 4;

	public const int TYPE_RESPIRATORY = 8;

	public const int TYPE_CIRCULATORY = 16;

	public const int TYPE_CONTACT = 32;

	public const int TYPE_FIELD = 64;

	public const int TYPE_ACTIVITY = 128;

	public const int TYPE_DIMENSIONAL = 256;

	public const int TYPE_CHEMICAL = 512;

	public const int TYPE_STRUCTURAL = 1024;

	public const int TYPE_SONIC = 2048;

	public const int TYPE_TEMPORAL = 4096;

	public const int TYPE_NEUROLOGICAL = 8192;

	public const int TYPE_DISEASE = 16384;

	public const int TYPE_PSIONIC = 32768;

	public const int TYPE_MINOR = 16777216;

	public const int TYPE_NEGATIVE = 33554432;

	public const int TYPE_REMOVABLE = 67108864;

	public const int TYPE_VOLUNTARY = 134217728;

	public const int TYPES_MECHANISM = 16777215;

	public const int TYPES_CLASS = 251658240;

	public const int DURATION_INDEFINITE = 9999;

	public static List<string> NegativeEffects;

	public string _ClassName;

	public string _DisplayName;

	public Guid ID = Guid.NewGuid();

	public int _Duration;

	public int MaxDuration;

	[NonSerialized]
	public GameObject _Object;

	[NonSerialized]
	private StatShifter _StatShifter;

	public string ClassName
	{
		get
		{
			if (_ClassName == null)
			{
				_ClassName = GetType().Name;
			}
			return _ClassName;
		}
	}

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				_DisplayName = ClassName;
			}
			return _DisplayName;
		}
		set
		{
			_DisplayName = value;
		}
	}

	public string DisplayNameStripped => ConsoleLib.Console.ColorUtility.StripFormatting(DisplayName);

	public int Duration
	{
		get
		{
			return _Duration;
		}
		set
		{
			_Duration = value;
			if (_Duration > MaxDuration)
			{
				MaxDuration = _Duration;
			}
		}
	}

	public GameObject Object
	{
		get
		{
			return _Object;
		}
		set
		{
			_Object = value;
			if (_StatShifter != null)
			{
				_StatShifter.Owner = _Object;
			}
		}
	}

	public StatShifter StatShifter
	{
		get
		{
			if (_StatShifter == null)
			{
				_StatShifter = new StatShifter(Object, GetDescription());
			}
			return _StatShifter;
		}
	}

	public override GameObject GetComponentBasis()
	{
		return Object;
	}

	public virtual bool allowCopyOnNoEffectDeepCopy()
	{
		return false;
	}

	public virtual int GetEffectType()
	{
		return 1;
	}

	public bool IsOfType(int mask)
	{
		return GetEffectType().HasBit(mask);
	}

	public bool IsOfTypes(int mask)
	{
		return GetEffectType().HasAllBits(mask);
	}

	public virtual bool IsTonic()
	{
		return false;
	}

	public virtual string GetDescription()
	{
		if (DisplayName.Contains("<"))
		{
			return null;
		}
		return DisplayName;
	}

	public virtual bool SuppressInLookDisplay()
	{
		return false;
	}

	public static bool CanEffectTypeBeAppliedTo(int type, GameObject obj)
	{
		if (type.HasBit(2) && obj.pBrain == null)
		{
			return false;
		}
		if ((type.HasBit(4) || type.HasBit(8)) && !obj.HasPart("Stomach"))
		{
			return false;
		}
		if (type.HasBit(16) && obj.GetIntProperty("Bleeds") <= 0)
		{
			return false;
		}
		if (type.HasBit(128) && !obj.HasPart("Body"))
		{
			return false;
		}
		switch (obj.GetMatterPhase())
		{
		case 1:
			if (!CanEffectTypeBeAppliedToSolid(type))
			{
				return false;
			}
			break;
		case 2:
			if (!CanEffectTypeBeAppliedToLiquid(type))
			{
				return false;
			}
			break;
		case 3:
			if (!CanEffectTypeBeAppliedToGas(type))
			{
				return false;
			}
			break;
		case 4:
			if (!CanEffectTypeBeAppliedToPlasma(type))
			{
				return false;
			}
			break;
		}
		return true;
	}

	public bool CanBeAppliedTo(GameObject obj)
	{
		return CanEffectTypeBeAppliedTo(GetEffectType(), obj);
	}

	public static bool CanEffectTypeBeAppliedToSolid(int type, GameObject obj = null)
	{
		return true;
	}

	public static bool CanEffectTypeBeAppliedToLiquid(int type, GameObject obj = null)
	{
		if (type.HasBit(32))
		{
			return false;
		}
		if (type.HasBit(1024))
		{
			return false;
		}
		return true;
	}

	public static bool CanEffectTypeBeAppliedToGas(int type, GameObject obj = null)
	{
		if (type.HasBit(32))
		{
			return false;
		}
		if (type.HasBit(4))
		{
			return false;
		}
		if (type.HasBit(1024))
		{
			return false;
		}
		return true;
	}

	public static bool CanEffectTypeBeAppliedToPlasma(int type, GameObject obj = null)
	{
		if (type.HasBit(32))
		{
			return false;
		}
		if (type.HasBit(4))
		{
			return false;
		}
		if (type.HasBit(64))
		{
			return false;
		}
		if (type.HasBit(512))
		{
			return false;
		}
		if (type.HasBit(1024))
		{
			return false;
		}
		if (type.HasBit(2048))
		{
			return false;
		}
		return true;
	}

	public virtual bool SameAs(Effect e)
	{
		if (e.ClassName != ClassName)
		{
			return false;
		}
		if (e.DisplayName != DisplayName)
		{
			return false;
		}
		if (e.Duration != Duration)
		{
			return false;
		}
		return true;
	}

	public virtual string GetDetails()
	{
		return "[effect details]";
	}

	public virtual void OnPaint(ScreenBuffer Buffer)
	{
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

	public virtual bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(GetType().FullName);
		Writer.WriteGameObject(Object);
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

	public virtual Effect DeepCopy(GameObject Parent)
	{
		Effect effect = (Effect)Activator.CreateInstance(GetType());
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0 && !fieldInfo.IsLiteral)
			{
				fieldInfo.SetValue(effect, fieldInfo.GetValue(this));
			}
		}
		effect.Object = Parent;
		return effect;
	}

	public virtual Effect DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		return DeepCopy(Parent);
	}

	public virtual void SaveData(SerializationWriter Writer)
	{
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
			{
				continue;
			}
			if (fieldInfo.IsNotSerialized)
			{
				Writer.Write(0);
			}
			else if (fieldInfo.FieldType.FullName == "XRL.World.Parts.Physics" || fieldInfo.FieldType.FullName == "XRL.World.BodyPart" || fieldInfo.Name == "Objects" || fieldInfo.Name == "MutationList")
			{
				Writer.Write(0);
			}
			else if (fieldInfo.FieldType.FullName == "XRL.World.Cell")
			{
				if (!(fieldInfo.GetValue(this) is Cell cell) || cell.ParentZone.ZoneID == null)
				{
					Writer.Write(4);
					continue;
				}
				Writer.Write(3);
				Writer.Write(cell.ParentZone.ZoneID);
				Writer.Write(cell.X);
				Writer.Write(cell.Y);
			}
			else if (fieldInfo.FieldType.FullName == "XRL.World.GameObject")
			{
				GameObject gameObject = fieldInfo.GetValue(this) as GameObject;
				if (gameObject != null && gameObject.IsPlayer())
				{
					Writer.Write(5);
					continue;
				}
				Writer.Write(2);
				Writer.WriteGameObject(gameObject);
			}
			else
			{
				Writer.Write(1);
				Writer.WriteObject(fieldInfo.GetValue(this));
			}
		}
	}

	public static Effect Load(SerializationReader Reader)
	{
		string text = Reader.ReadString();
		try
		{
			Type type = ModManager.ResolveType(text);
			if (type == null)
			{
				Debug.LogError("Couldnt find effect type: " + text);
			}
			Effect effect = Activator.CreateInstance(type) as Effect;
			effect.Object = Reader.ReadGameObject();
			if (Reader.ReadBoolean())
			{
				effect._StatShifter = StatShifter.Load(Reader, effect.Object);
			}
			effect.LoadData(Reader);
			return effect;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Effect load type:" + text, x);
		}
		return null;
	}

	public virtual void LoadData(SerializationReader Reader)
	{
		Type type = GetType();
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
			{
				continue;
			}
			FieldSaveVersion value = null;
			if (!IComponent<GameObject>.fieldSaveVersionInfo.TryGetValue(fieldInfo, out value))
			{
				value = (FieldSaveVersion)Attribute.GetCustomAttribute(fieldInfo, typeof(FieldSaveVersion));
				IComponent<GameObject>.fieldSaveVersionInfo.Add(fieldInfo, value);
			}
			if (value != null && value.minimumSaveVersion > -1 && value.minimumSaveVersion > Reader.FileVersion)
			{
				continue;
			}
			switch (Reader.ReadInt32())
			{
			case 5:
				fieldInfo.SetValue(this, XRLCore.Core.Game.Player.Body);
				break;
			case 3:
			{
				string zoneID = Reader.ReadString();
				int x = Reader.ReadInt32();
				int y = Reader.ReadInt32();
				fieldInfo.SetValue(this, XRLCore.Core.Game.ZoneManager.GetZone(zoneID).GetCell(x, y));
				break;
			}
			case 4:
				fieldInfo.SetValue(this, null);
				break;
			case 2:
				fieldInfo.SetValue(this, Reader.ReadGameObject(type.Name + "::" + fieldInfo.Name));
				break;
			case 1:
			{
				object obj = Reader.ReadObject();
				try
				{
					fieldInfo.SetValue(this, obj);
				}
				catch (ArgumentException ex)
				{
					if (obj is string)
					{
						Debug.LogError("Error restoring field " + type.Name + "::" + fieldInfo.Name + " with string value \"" + obj?.ToString() + "\"");
					}
					else if (obj is int)
					{
						Debug.LogError("Error restoring field " + type.Name + "::" + fieldInfo.Name + " with int value \"" + obj?.ToString() + "\"");
					}
					else
					{
						Debug.LogError("Error restoring field " + type.Name + "::" + fieldInfo.Name);
					}
					throw ex;
				}
				break;
			}
			}
		}
	}

	public virtual bool Apply(GameObject Object)
	{
		return true;
	}

	public virtual void Remove(GameObject Object)
	{
	}

	public virtual bool CanApplyToStack()
	{
		return false;
	}

	public virtual void WasUnstackedFrom(GameObject obj)
	{
	}

	public virtual void Register(GameObject Object)
	{
	}

	public virtual void Unregister(GameObject Object)
	{
	}

	public virtual void Expired()
	{
	}

	public virtual bool UseStandardDurationCountdown()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDebugInternalsEvent.ID && (ID != BeforeBeginTakeActionEvent.ID || !UseStandardDurationCountdown() || Object?.pBrain == null))
		{
			if (ID == EndTurnEvent.ID && UseStandardDurationCountdown())
			{
				return Object?.pBrain == null;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (UseStandardDurationCountdown() && Object?.pBrain != null && Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (UseStandardDurationCountdown() && Object?.pBrain == null && Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Duration", Duration);
		return base.HandleEvent(E);
	}
}
