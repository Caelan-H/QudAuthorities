using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using XRL.World.Parts;

namespace XRL.World;

/// <summary>
///              A SerializationReader instance is used to read stored values and objects from a byte array.
///
///              Once an instance is created, use the various methods to read the required data.
///              The data read MUST be exactly the same type and in the same order as it was written.
///              </summary>
public sealed class SerializationReader : BinaryReader
{
	private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);

	internal const ushort GameObjectStartValue = 64206;

	internal const ushort GameObjectEndValue = 44203;

	internal const ushort GameObjectFinishValue = 52958;

	public int Errors;

	private string[] stringTokenList;

	private object[] objectTokens;

	private int endPosition;

	public int FileVersion;

	public static Guid ImmutableObject = new Guid("00000000-0000-0000-0000-000000000002");

	public static Guid PlayerGuid = new Guid("00000000-0000-0000-0000-000000000001");

	private Dictionary<Guid, GameObject> Objects = new Dictionary<Guid, GameObject>();

	private static BinaryFormatter formatter = null;

	public static ResolveEventHandler modAssemblyResolveHandler = delegate(object sender, ResolveEventArgs args)
	{
		AssemblyName assemblyName = new AssemblyName(args.Name);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyName.Name);
		foreach (ModInfo mod in ModManager.Mods)
		{
			if (assemblyName.Name == mod.Assembly?.GetName()?.Name)
			{
				return mod.Assembly;
			}
			if (fileNameWithoutExtension == mod.ID && !mod.IsEnabled)
			{
				MetricsManager.LogModError(mod, "Required mod " + mod.DisplayTitleStripped + " has status: " + Enum.GetName(typeof(ModState), mod.State));
				return null;
			}
		}
		MetricsManager.LogError("Unable to resolve mod assembly: " + assemblyName.Name);
		return null;
	};

	public static ResolveEventHandler assemblyResolveHandler = (object a, ResolveEventArgs b) => Assembly.GetExecutingAssembly();

	public int BytesRemaining => endPosition - (int)BaseStream.Position;

	public SerializationReader(byte[] data)
		: this(new MemoryStream(data))
	{
		formatter = null;
	}

	protected override void Dispose(bool disposing)
	{
		formatter = null;
		base.Dispose(disposing);
	}

	/// <summary>
	///             Creates a SerializationReader based on the passed Stream.
	///             </summary><param name="stream">The stream containing the serialized data</param>
	public SerializationReader(Stream stream)
		: base(stream)
	{
		endPosition = ReadInt32();
		stream.Position = endPosition;
		stringTokenList = new string[ReadOptimizedInt32()];
		for (int i = 0; i < stringTokenList.Length; i++)
		{
			stringTokenList[i] = base.ReadString();
		}
		objectTokens = new object[ReadOptimizedInt32()];
		for (int j = 0; j < objectTokens.Length; j++)
		{
			objectTokens[j] = ReadObject();
		}
		stream.Position = 4L;
		formatter = null;
	}

	/// <summary>
	///             Returns an ArrayList or null from the stream.
	///             </summary><returns>An ArrayList instance.</returns>
	public ArrayList ReadArrayList()
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return new ArrayList(ReadOptimizedObjectArray());
	}

	public BitArray ReadBitArray()
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return ReadOptimizedBitArray();
	}

	/// <summary>
	///             Returns a BitVector32 value from the stream.
	///             </summary><returns>A BitVector32 value.</returns>
	public BitVector32 ReadBitVector32()
	{
		return new BitVector32(ReadInt32());
	}

	public byte[] ReadBytesDirect(int count)
	{
		return base.ReadBytes(count);
	}

	/// <summary>
	///             Returns a DateTime value from the stream.
	///             </summary><returns>A DateTime value.</returns>
	public DateTime ReadDateTime()
	{
		return DateTime.FromBinary(ReadInt64());
	}

	public Guid ReadGuid()
	{
		return new Guid(ReadBytes(16));
	}

	/// <summary>
	///             Returns an object based on the SerializedType read next from the stream.
	///             </summary><returns>An object instance.</returns>
	public object ReadObject()
	{
		return processObject((SerializedType)ReadByte());
	}

	public void ReadGameObjectList(List<GameObject> List, string Forensics = null)
	{
		List.Clear();
		int num = ReadInt32();
		for (int i = 0; i < num; i++)
		{
			List.Add(ReadGameObject(Forensics));
		}
	}

	public GameObject ReadGameObject(string Forensics = null)
	{
		Guid guid = ReadGuid();
		if (guid == Guid.Empty)
		{
			return null;
		}
		if (guid == PlayerGuid)
		{
			return The.Player;
		}
		if (!Objects.TryGetValue(guid, out var value))
		{
			if (guid == ImmutableObject)
			{
				ReadGuid();
				value = GameObjectFactory.Factory.CreateObject(ReadString());
			}
			else
			{
				Objects.Add(guid, value = GetGameObject());
			}
		}
		return value;
	}

	public bool UnspoolTo(ushort Value)
	{
		byte[] bytes = BitConverter.GetBytes(Value);
		int num = 0;
		int num2 = bytes.Length;
		for (int num3 = BytesRemaining; num3 > 0; num3--)
		{
			if (ReadByte() == bytes[num])
			{
				num++;
				if (num >= num2)
				{
					return true;
				}
			}
			else
			{
				num = 0;
			}
		}
		return false;
	}

	public void ReadEndVal()
	{
		if (FileVersion >= 250 && ReadUInt16() != 44203)
		{
			throw new SerializationException("Invalid game object end value.");
		}
	}

	public bool ReadStartVal()
	{
		if (FileVersion >= 250)
		{
			return ReadUInt16() switch
			{
				64206 => true, 
				52958 => false, 
				_ => throw new SerializationException("Invalid game object start value."), 
			};
		}
		return ReadInt32() != 0;
	}

	public GameObject GetGameObject()
	{
		if (GameObjectFactory.Factory.gameObjectPool.Count > 0)
		{
			return GameObjectFactory.Factory.gameObjectPool.Dequeue();
		}
		return new GameObject();
	}

	public void ReadGameObjects()
	{
		GameObject value = null;
		for (long num = -1L; num < endPosition; value = null)
		{
			try
			{
				if (!ReadStartVal())
				{
					break;
				}
				Guid guid = ReadGuid();
				if (guid == PlayerGuid)
				{
					ReadEndVal();
					num = -1L;
					continue;
				}
				if (!Objects.TryGetValue(guid, out value))
				{
					value = (Objects[guid] = GetGameObject());
				}
				value.Load(this);
				ReadEndVal();
				num = -1L;
				continue;
			}
			catch (Exception ex)
			{
				Errors++;
				FieldInfo fieldInfo = ex.Data["Field"] as FieldInfo;
				Type type = (ex.Data["Type"] as Type) ?? fieldInfo?.DeclaringType;
				string text = ex.Data["TypeName"] as string;
				StringBuilder stringBuilder = Event.NewStringBuilder();
				if (type != null)
				{
					stringBuilder.Append("Type: ").Append(type.FullName);
				}
				else if (text != null)
				{
					stringBuilder.Compound("Type: ", ", ").Append(text);
				}
				if (fieldInfo != null)
				{
					stringBuilder.Compound("Field: ", ", ").Append(fieldInfo.Name);
				}
				stringBuilder.Compound(ex.ToString(), "\n\n");
				if (value != null)
				{
					value.RequirePart<Render>().DisplayName = "{{R|[CORRUPT OBJECT: " + value.Blueprint + "]}}";
					value.RequirePart<Description>().Short = stringBuilder.ToString();
					value.RequirePart<CorruptObject>();
				}
				if (num >= 0 && BaseStream.CanSeek)
				{
					BaseStream.Position = num;
					num = -1L;
				}
				if (UnspoolTo(44203))
				{
					num = BaseStream.Position;
					stringBuilder.Insert(0, "Recovered from game object deserialization error.\n");
					ModInfo Mod;
					StackFrame Frame;
					if (type != null)
					{
						MetricsManager.LogAssemblyError(type, stringBuilder.ToString());
					}
					else if (ModManager.TryGetStackMod(ex, out Mod, out Frame))
					{
						Mod.Error(stringBuilder.ToString());
					}
					else
					{
						MetricsManager.LogError(stringBuilder.ToString());
					}
					continue;
				}
				throw;
			}
		}
		foreach (Guid key in Objects.Keys)
		{
			if (!(key == PlayerGuid))
			{
				Objects[key].FinalizeLoad();
			}
		}
		IPart.LoadComplete();
		UnityEngine.Debug.Log("Objects loaded: " + Objects.Count);
		UnityEngine.Debug.Log("StatsLoaded loaded: " + GameObject.StatsLoaded);
		UnityEngine.Debug.Log("PartsLoaded loaded: " + GameObject.PartsLoaded);
		UnityEngine.Debug.Log("EffectsLoaded loaded: " + GameObject.EffectsLoaded);
		UnityEngine.Debug.Log("PartEventsLoaded loaded: " + GameObject.PartEventsLoaded);
		UnityEngine.Debug.Log("EffectEventsLoaded loaded: " + GameObject.EffectEventsLoaded);
		GameObject.StatsLoaded = 0;
		GameObject.PartsLoaded = 0;
		GameObject.EffectsLoaded = 0;
		GameObject.PartEventsLoaded = 0;
		GameObject.EffectsLoaded = 0;
		if (GameObject.LoadedEvents != null)
		{
			GameObject.LoadedEvents.Clear();
		}
	}

	public override string ReadString()
	{
		return ReadOptimizedString();
	}

	/// <summary>
	///             Returns a string value from the stream.
	///             </summary><returns>A string value.</returns>
	public string ReadStringDirect()
	{
		return base.ReadString();
	}

	public TimeSpan ReadTimeSpan()
	{
		return new TimeSpan(ReadInt64());
	}

	/// <summary>
	///             Returns a Type or null from the stream.
	///
	///             Throws an exception if the Type cannot be found.
	///             </summary><returns>A Type instance.</returns>
	public Type ReadType()
	{
		return ReadType(throwOnError: true);
	}

	public Type ReadType(bool throwOnError)
	{
		if (readTypeCode() == SerializedType.NullType)
		{
			return null;
		}
		return Type.GetType(ReadOptimizedString(), throwOnError);
	}

	/// <summary>
	///             Returns an ArrayList from the stream that was stored optimized.
	///             </summary><returns>An ArrayList instance.</returns>
	public ArrayList ReadOptimizedArrayList()
	{
		return new ArrayList(ReadOptimizedObjectArray());
	}

	public BitArray ReadOptimizedBitArray()
	{
		int num = ReadOptimizedInt32();
		if (num == 0)
		{
			return FullyOptimizableTypedArray;
		}
		return new BitArray(base.ReadBytes((num + 7) / 8))
		{
			Length = num
		};
	}

	/// <summary>
	///             Returns a BitVector32 value from the stream that was stored optimized.
	///             </summary><returns>A BitVector32 value.</returns>
	public BitVector32 ReadOptimizedBitVector32()
	{
		return new BitVector32(Read7BitEncodedInt());
	}

	public DateTime ReadOptimizedDateTime()
	{
		BitVector32 bitVector = new BitVector32(ReadByte() | (ReadByte() << 8) | (ReadByte() << 16));
		DateTime dateTime = new DateTime(bitVector[SerializationWriter.DateYearMask], bitVector[SerializationWriter.DateMonthMask], bitVector[SerializationWriter.DateDayMask]);
		if (bitVector[SerializationWriter.DateHasTimeOrKindMask] == 1)
		{
			byte b = ReadByte();
			DateTimeKind dateTimeKind = (DateTimeKind)(b & 3);
			b = (byte)(b & 0xFCu);
			if (dateTimeKind != 0)
			{
				dateTime = DateTime.SpecifyKind(dateTime, dateTimeKind);
			}
			if (b == 0)
			{
				ReadByte();
			}
			else
			{
				dateTime = dateTime.Add(decodeTimeSpan(b));
			}
		}
		return dateTime;
	}

	/// <summary>
	///             Returns a Decimal value from the stream that was stored optimized.
	///             </summary><returns>A Decimal value.</returns>
	public decimal ReadOptimizedDecimal()
	{
		byte b = ReadByte();
		int lo = 0;
		int mid = 0;
		int hi = 0;
		byte scale = 0;
		if ((b & 2u) != 0)
		{
			scale = ReadByte();
		}
		if ((b & 4) == 0)
		{
			lo = (((b & 0x20) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		if ((b & 8) == 0)
		{
			mid = (((b & 0x40) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		if ((b & 0x10) == 0)
		{
			hi = (((b & 0x80) == 0) ? ReadInt32() : ReadOptimizedInt32());
		}
		return new decimal(lo, mid, hi, (b & 1) != 0, scale);
	}

	public int ReadOptimizedInt32()
	{
		int num = 0;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (b & 0x7F) << num2;
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	/// <summary>
	///             Returns an Int16 value from the stream that was stored optimized.
	///             </summary><returns>An Int16 value.</returns>
	public short ReadOptimizedInt16()
	{
		return (short)ReadOptimizedInt32();
	}

	public long ReadOptimizedInt64()
	{
		long num = 0L;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (long)(((ulong)b & 0x7FuL) << num2);
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	/// <summary>
	///             Returns an object[] from the stream that was stored optimized.
	///             </summary><returns>An object[] instance.</returns>
	public object[] ReadOptimizedObjectArray()
	{
		return ReadOptimizedObjectArray(null);
	}

	public object[] ReadOptimizedObjectArray(Type elementType)
	{
		int num = ReadOptimizedInt32();
		object[] array = (object[])((elementType == null) ? new object[num] : Array.CreateInstance(elementType, num));
		for (int i = 0; i < array.Length; i++)
		{
			SerializedType serializedType = (SerializedType)ReadByte();
			switch (serializedType)
			{
			case SerializedType.NullSequenceType:
				i += ReadOptimizedInt32();
				break;
			case SerializedType.DuplicateValueSequenceType:
			{
				object obj = (array[i] = ReadObject());
				int num3 = ReadOptimizedInt32();
				while (num3-- > 0)
				{
					array[++i] = obj;
				}
				break;
			}
			case SerializedType.DBNullSequenceType:
			{
				int num2 = ReadOptimizedInt32();
				array[i] = DBNull.Value;
				while (num2-- > 0)
				{
					array[++i] = DBNull.Value;
				}
				break;
			}
			default:
				array[i] = processObject(serializedType);
				break;
			case SerializedType.NullType:
				break;
			}
		}
		return array;
	}

	/// <summary>
	///             Returns a pair of object[] arrays from the stream that were stored optimized.
	///             </summary><returns>A pair of object[] arrays.</returns>
	public void ReadOptimizedObjectArrayPair(out object[] values1, out object[] values2)
	{
		values1 = ReadOptimizedObjectArray(null);
		values2 = new object[values1.Length];
		for (int i = 0; i < values2.Length; i++)
		{
			SerializedType serializedType = (SerializedType)ReadByte();
			switch (serializedType)
			{
			case SerializedType.DuplicateValueSequenceType:
			{
				values2[i] = values1[i];
				int num2 = ReadOptimizedInt32();
				while (num2-- > 0)
				{
					values2[++i] = values1[i];
				}
				break;
			}
			case SerializedType.DuplicateValueType:
				values2[i] = values1[i];
				break;
			case SerializedType.NullSequenceType:
				i += ReadOptimizedInt32();
				break;
			case SerializedType.DBNullSequenceType:
			{
				int num = ReadOptimizedInt32();
				values2[i] = DBNull.Value;
				while (num-- > 0)
				{
					values2[++i] = DBNull.Value;
				}
				break;
			}
			default:
				values2[i] = processObject(serializedType);
				break;
			case SerializedType.NullType:
				break;
			}
		}
	}

	public string ReadOptimizedString()
	{
		SerializedType serializedType = readTypeCode();
		if ((int)serializedType < 128)
		{
			return readTokenizedString((int)serializedType);
		}
		return serializedType switch
		{
			SerializedType.NullType => null, 
			SerializedType.YStringType => "Y", 
			SerializedType.NStringType => "N", 
			SerializedType.SingleCharStringType => char.ToString(ReadChar()), 
			SerializedType.SingleSpaceType => " ", 
			SerializedType.EmptyStringType => string.Empty, 
			_ => throw new InvalidOperationException("Unrecognized TypeCode, expected a type of string instead got: " + serializedType), 
		};
	}

	/// <summary>
	///             Returns a TimeSpan value from the stream that was stored optimized.
	///             </summary><returns>A TimeSpan value.</returns>
	public TimeSpan ReadOptimizedTimeSpan()
	{
		return decodeTimeSpan(ReadByte());
	}

	public Type ReadOptimizedType()
	{
		return ReadOptimizedType(throwOnError: true);
	}

	/// <summary>
	///             Returns a Type from the stream.
	///
	///             Throws an exception if the Type cannot be found and throwOnError is true.
	///             </summary><returns>A Type instance.</returns>
	public Type ReadOptimizedType(bool throwOnError)
	{
		return Type.GetType(ReadOptimizedString(), throwOnError);
	}

	[CLSCompliant(false)]
	public ushort ReadOptimizedUInt16()
	{
		return (ushort)ReadOptimizedUInt32();
	}

	/// <summary>
	///             Returns a UInt32 value from the stream that was stored optimized.
	///             </summary><returns>A UInt32 value.</returns>
	[CLSCompliant(false)]
	public uint ReadOptimizedUInt32()
	{
		uint num = 0u;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= (uint)((b & 0x7F) << num2);
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	[CLSCompliant(false)]
	public ulong ReadOptimizedUInt64()
	{
		ulong num = 0uL;
		int num2 = 0;
		byte b;
		do
		{
			b = ReadByte();
			num |= ((ulong)b & 0x7FuL) << num2;
			num2 += 7;
		}
		while ((b & 0x80u) != 0);
		return num;
	}

	/// <summary>
	///             Returns a typed array from the stream.
	///             </summary><returns>A typed array.</returns>
	public Array ReadTypedArray()
	{
		return (Array)processArrayTypes(readTypeCode(), null);
	}

	public Dictionary<K, V> ReadDictionary<K, V>()
	{
		int num = ReadInt32();
		Dictionary<K, V> dictionary = new Dictionary<K, V>(num);
		for (int i = 0; i < num; i++)
		{
			dictionary.Add((K)ReadObject(), (V)ReadObject());
		}
		return dictionary;
	}

	/// <summary>
	///             Populates a pre-existing generic dictionary with keys and values from the stream.
	///             This allows a generic dictionary to be created without using the default constructor.
	///             </summary><typeparam name="K">The key Type.</typeparam><typeparam name="V">The value Type.</typeparam>
	public void ReadDictionary<K, V>(Dictionary<K, V> dictionary)
	{
		K[] array = (K[])processArrayTypes(readTypeCode(), typeof(K));
		V[] array2 = (V[])processArrayTypes(readTypeCode(), typeof(V));
		if (dictionary == null)
		{
			dictionary = new Dictionary<K, V>(array.Length);
		}
		for (int i = 0; i < array.Length; i++)
		{
			dictionary.Add(array[i], array2[i]);
		}
	}

	public List<T> ReadList<T>()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		List<T> list = new List<T>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((T)ReadObject());
		}
		return list;
	}

	public List<string> ReadStringList()
	{
		int num = ReadInt32();
		if (num == -1)
		{
			return null;
		}
		List<string> list = new List<string>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((string)ReadObject());
		}
		return list;
	}

	/// <summary>
	///             Returns a Nullable struct from the stream.
	///             The value returned must be cast to the correct Nullable type.
	///             Synonym for ReadObject();
	///             </summary><returns>A struct value or null</returns>
	public ValueType ReadNullable()
	{
		return (ValueType)ReadObject();
	}

	public bool? ReadNullableBoolean()
	{
		return (bool?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable Byte from the stream.
	///             </summary><returns>A Nullable Byte.</returns>
	public byte? ReadNullableByte()
	{
		return (byte?)ReadObject();
	}

	public char? ReadNullableChar()
	{
		return (char?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable DateTime from the stream.
	///             </summary><returns>A Nullable DateTime.</returns>
	public DateTime? ReadNullableDateTime()
	{
		return (DateTime?)ReadObject();
	}

	public decimal? ReadNullableDecimal()
	{
		return (decimal?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable Double from the stream.
	///             </summary><returns>A Nullable Double.</returns>
	public double? ReadNullableDouble()
	{
		return (double?)ReadObject();
	}

	public Guid? ReadNullableGuid()
	{
		return (Guid?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable Int16 from the stream.
	///             </summary><returns>A Nullable Int16.</returns>
	public short? ReadNullableInt16()
	{
		return (short?)ReadObject();
	}

	public int? ReadNullableInt32()
	{
		return (int?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable Int64 from the stream.
	///             </summary><returns>A Nullable Int64.</returns>
	public long? ReadNullableInt64()
	{
		return (long?)ReadObject();
	}

	[CLSCompliant(false)]
	public sbyte? ReadNullableSByte()
	{
		return (sbyte?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable Single from the stream.
	///             </summary><returns>A Nullable Single.</returns>
	public float? ReadNullableSingle()
	{
		return (float?)ReadObject();
	}

	public TimeSpan? ReadNullableTimeSpan()
	{
		return (TimeSpan?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable UInt16 from the stream.
	///             </summary><returns>A Nullable UInt16.</returns>
	[CLSCompliant(false)]
	public ushort? ReadNullableUInt16()
	{
		return (ushort?)ReadObject();
	}

	[CLSCompliant(false)]
	public uint? ReadNullableUInt32()
	{
		return (uint?)ReadObject();
	}

	/// <summary>
	///             Returns a Nullable UInt64 from the stream.
	///             </summary><returns>A Nullable UInt64.</returns>
	[CLSCompliant(false)]
	public ulong? ReadNullableUInt64()
	{
		return (ulong?)ReadObject();
	}

	public byte[] ReadByteArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new byte[0], 
			_ => readByteArray(), 
		};
	}

	/// <summary>
	///             Returns a Char[] from the stream.
	///             </summary><returns>A Char[] value; or null.</returns>
	public char[] ReadCharArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new char[0], 
			_ => readCharArray(), 
		};
	}

	public double[] ReadDoubleArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new double[0], 
			_ => readDoubleArray(), 
		};
	}

	/// <summary>
	///             Returns a Guid[] from the stream.
	///             </summary><returns>A Guid[] instance; or null.</returns>
	public Guid[] ReadGuidArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new Guid[0], 
			_ => readGuidArray(), 
		};
	}

	public short[] ReadInt16Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new short[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			short[] array = new short[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt16();
				}
				else
				{
					array[i] = ReadOptimizedInt16();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	///             Returns an object[] or null from the stream.
	///             </summary><returns>A DateTime value.</returns>
	public object[] ReadObjectArray()
	{
		return ReadObjectArray(null);
	}

	public object[] ReadObjectArray(Type elementType)
	{
		switch (readTypeCode())
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyObjectArrayType:
			if (!(elementType == null))
			{
				return (object[])Array.CreateInstance(elementType, 0);
			}
			return new object[0];
		case SerializedType.EmptyTypedArrayType:
			throw new Exception();
		default:
			return ReadOptimizedObjectArray(elementType);
		}
	}

	/// <summary>
	///             Returns a Single[] from the stream.
	///             </summary><returns>A Single[] instance; or null.</returns>
	public float[] ReadSingleArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new float[0], 
			_ => readSingleArray(), 
		};
	}

	[CLSCompliant(false)]
	public sbyte[] ReadSByteArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new sbyte[0], 
			_ => readSByteArray(), 
		};
	}

	/// <summary>
	///             Returns a string[] or null from the stream.
	///             </summary><returns>An string[] instance.</returns>
	public string[] ReadStringArray()
	{
		return (string[])ReadObjectArray(typeof(string));
	}

	[CLSCompliant(false)]
	public ushort[] ReadUInt16Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new ushort[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			ushort[] array = new ushort[ReadOptimizedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt16();
				}
				else
				{
					array[i] = ReadOptimizedUInt16();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	///             Returns a Boolean[] from the stream.
	///             </summary><returns>A Boolean[] instance; or null.</returns>
	public bool[] ReadBooleanArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new bool[0], 
			_ => readBooleanArray(), 
		};
	}

	public DateTime[] ReadDateTimeArray()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new DateTime[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			DateTime[] array = new DateTime[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadDateTime();
				}
				else
				{
					array[i] = ReadOptimizedDateTime();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	///             Returns a Decimal[] from the stream.
	///             </summary><returns>A Decimal[] instance; or null.</returns>
	public decimal[] ReadDecimalArray()
	{
		return readTypeCode() switch
		{
			SerializedType.NullType => null, 
			SerializedType.EmptyTypedArrayType => new decimal[0], 
			_ => readDecimalArray(), 
		};
	}

	public int[] ReadInt32Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new int[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			int[] array = new int[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt32();
				}
				else
				{
					array[i] = ReadOptimizedInt32();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	///             Returns an Int64[] from the stream.
	///             </summary><returns>An Int64[] instance; or null.</returns>
	public long[] ReadInt64Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new long[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			long[] array = new long[ReadOptimizedInt64()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadInt64();
				}
				else
				{
					array[i] = ReadOptimizedInt64();
				}
			}
			return array;
		}
		}
	}

	public string[] ReadOptimizedStringArray()
	{
		return (string[])ReadOptimizedObjectArray(typeof(string));
	}

	/// <summary>
	///             Returns a TimeSpan[] from the stream.
	///             </summary><returns>A TimeSpan[] instance; or null.</returns>
	public TimeSpan[] ReadTimeSpanArray()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new TimeSpan[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			TimeSpan[] array = new TimeSpan[ReadOptimizedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadTimeSpan();
				}
				else
				{
					array[i] = ReadOptimizedTimeSpan();
				}
			}
			return array;
		}
		}
	}

	[CLSCompliant(false)]
	public uint[] ReadUInt32Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new uint[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			uint[] array = new uint[ReadOptimizedUInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt32();
				}
				else
				{
					array[i] = ReadOptimizedUInt32();
				}
			}
			return array;
		}
		}
	}

	/// <summary>
	///             Returns a UInt64[] from the stream.
	///             </summary><returns>A UInt64[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ulong[] ReadUInt64Array()
	{
		SerializedType serializedType = readTypeCode();
		switch (serializedType)
		{
		case SerializedType.NullType:
			return null;
		case SerializedType.EmptyTypedArrayType:
			return new ulong[0];
		default:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(serializedType);
			ulong[] array = new ulong[ReadOptimizedInt64()];
			for (int i = 0; i < array.Length; i++)
			{
				if (bitArray == null || (bitArray != FullyOptimizableTypedArray && !bitArray[i]))
				{
					array[i] = ReadUInt64();
				}
				else
				{
					array[i] = ReadOptimizedUInt64();
				}
			}
			return array;
		}
		}
	}

	public bool[] ReadOptimizedBooleanArray()
	{
		return ReadBooleanArray();
	}

	/// <summary>
	///             Returns a DateTime[] from the stream.
	///             </summary><returns>A DateTime[] instance; or null.</returns>
	public DateTime[] ReadOptimizedDateTimeArray()
	{
		return ReadDateTimeArray();
	}

	public decimal[] ReadOptimizedDecimalArray()
	{
		return ReadDecimalArray();
	}

	/// <summary>
	///             Returns a Int16[] from the stream.
	///             </summary><returns>An Int16[] instance; or null.</returns>
	public short[] ReadOptimizedInt16Array()
	{
		return ReadInt16Array();
	}

	public int[] ReadOptimizedInt32Array()
	{
		return ReadInt32Array();
	}

	/// <summary>
	///             Returns a Int64[] from the stream.
	///             </summary><returns>A Int64[] instance; or null.</returns>
	public long[] ReadOptimizedInt64Array()
	{
		return ReadInt64Array();
	}

	public TimeSpan[] ReadOptimizedTimeSpanArray()
	{
		return ReadTimeSpanArray();
	}

	/// <summary>
	///             Returns a UInt16[] from the stream.
	///             </summary><returns>A UInt16[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ushort[] ReadOptimizedUInt16Array()
	{
		return ReadUInt16Array();
	}

	[CLSCompliant(false)]
	public uint[] ReadOptimizedUInt32Array()
	{
		return ReadUInt32Array();
	}

	/// <summary>
	///             Returns a UInt64[] from the stream.
	///             </summary><returns>A UInt64[] instance; or null.</returns>
	[CLSCompliant(false)]
	public ulong[] ReadOptimizedUInt64Array()
	{
		return ReadUInt64Array();
	}

	public void ReadOwnedData(IOwnedDataSerializable target, object context)
	{
		target.DeserializeOwnedData(this, context);
	}

	/// <summary>
	///             Returns the object associated with the object token read next from the stream.
	///             </summary><returns>An object.</returns>
	public object ReadTokenizedObject()
	{
		return objectTokens[ReadOptimizedInt32()];
	}

	private TimeSpan decodeTimeSpan(byte initialByte)
	{
		long num = 0L;
		BitVector32 bitVector = new BitVector32(initialByte | (ReadByte() << 8));
		bool flag = bitVector[SerializationWriter.HasTimeSection] == 1;
		bool flag2 = bitVector[SerializationWriter.HasSecondsSection] == 1;
		bool flag3 = bitVector[SerializationWriter.HasMillisecondsSection] == 1;
		if (flag3)
		{
			bitVector = new BitVector32(bitVector.Data | (ReadByte() << 16) | (ReadByte() << 24));
		}
		else if (flag2 && flag)
		{
			bitVector = new BitVector32(bitVector.Data | (ReadByte() << 16));
		}
		if (flag)
		{
			num += bitVector[SerializationWriter.HoursSection] * 36000000000L;
			num += (long)bitVector[SerializationWriter.MinutesSection] * 600000000L;
		}
		if (flag2)
		{
			num += (long)bitVector[(!flag && !flag3) ? SerializationWriter.MinutesSection : SerializationWriter.SecondsSection] * 10000000L;
		}
		if (flag3)
		{
			num += (long)bitVector[SerializationWriter.MillisecondsSection] * 10000L;
		}
		if (bitVector[SerializationWriter.HasDaysSection] == 1)
		{
			num += ReadOptimizedInt32() * 864000000000L;
		}
		if (bitVector[SerializationWriter.IsNegativeSection] == 1)
		{
			num = -num;
		}
		return new TimeSpan(num);
	}

	/// <summary>
	///             Creates a BitArray representing which elements of a typed array
	///             are serializable.
	///             </summary><param name="serializedType">The type of typed array.</param><returns>A BitArray denoting which elements are serializable.</returns>
	private BitArray readTypedArrayOptimizeFlags(SerializedType serializedType)
	{
		BitArray result = null;
		switch (serializedType)
		{
		case SerializedType.FullyOptimizedTypedArrayType:
			result = FullyOptimizableTypedArray;
			break;
		case SerializedType.PartiallyOptimizedTypedArrayType:
			result = ReadOptimizedBitArray();
			break;
		}
		return result;
	}

	private object processObject(SerializedType typeCode)
	{
		if (typeCode == SerializedType.NullType)
		{
			return null;
		}
		if (typeCode == SerializedType.Int32Type)
		{
			return ReadInt32();
		}
		if (typeCode == SerializedType.EmptyStringType)
		{
			return string.Empty;
		}
		if ((int)typeCode < 128)
		{
			return readTokenizedString((int)typeCode);
		}
		switch (typeCode)
		{
		case SerializedType.BooleanFalseType:
			return false;
		case SerializedType.ZeroInt32Type:
			return 0;
		case SerializedType.OptimizedInt32Type:
			return ReadOptimizedInt32();
		case SerializedType.OptimizedInt32NegativeType:
			return -ReadOptimizedInt32() - 1;
		case SerializedType.DecimalType:
			return ReadOptimizedDecimal();
		case SerializedType.ZeroDecimalType:
			return 0m;
		case SerializedType.YStringType:
			return "Y";
		case SerializedType.DateTimeType:
			return ReadDateTime();
		case SerializedType.OptimizedDateTimeType:
			return ReadOptimizedDateTime();
		case SerializedType.SingleCharStringType:
			return char.ToString(ReadChar());
		case SerializedType.SingleSpaceType:
			return " ";
		case SerializedType.OneInt32Type:
			return 1;
		case SerializedType.OptimizedInt16Type:
			return ReadOptimizedInt16();
		case SerializedType.OptimizedInt16NegativeType:
			return -ReadOptimizedInt16() - 1;
		case SerializedType.OneDecimalType:
			return 1m;
		case SerializedType.BooleanTrueType:
			return true;
		case SerializedType.NStringType:
			return "N";
		case SerializedType.DBNullType:
			return DBNull.Value;
		case SerializedType.ObjectArrayType:
			return ReadOptimizedObjectArray();
		case SerializedType.EmptyObjectArrayType:
			return new object[0];
		case SerializedType.MinusOneInt32Type:
			return -1;
		case SerializedType.MinusOneInt64Type:
			return -1L;
		case SerializedType.MinusOneInt16Type:
			return (short)(-1);
		case SerializedType.MinDateTimeType:
			return DateTime.MinValue;
		case SerializedType.GuidType:
			return ReadGuid();
		case SerializedType.EmptyGuidType:
			return Guid.Empty;
		case SerializedType.TimeSpanType:
			return ReadTimeSpan();
		case SerializedType.MaxDateTimeType:
			return DateTime.MaxValue;
		case SerializedType.ZeroTimeSpanType:
			return TimeSpan.Zero;
		case SerializedType.OptimizedTimeSpanType:
			return ReadOptimizedTimeSpan();
		case SerializedType.DoubleType:
			return ReadDouble();
		case SerializedType.ZeroDoubleType:
			return 0.0;
		case SerializedType.Int64Type:
			return ReadInt64();
		case SerializedType.ZeroInt64Type:
			return 0L;
		case SerializedType.OptimizedInt64Type:
			return ReadOptimizedInt64();
		case SerializedType.OptimizedInt64NegativeType:
			return -ReadOptimizedInt64() - 1;
		case SerializedType.Int16Type:
			return ReadInt16();
		case SerializedType.ZeroInt16Type:
			return (short)0;
		case SerializedType.SingleType:
			return ReadSingle();
		case SerializedType.ZeroSingleType:
			return 0f;
		case SerializedType.ByteType:
			return ReadByte();
		case SerializedType.ZeroByteType:
			return (byte)0;
		case SerializedType.OtherType:
		{
			AppDomain.CurrentDomain.AssemblyResolve += assemblyResolveHandler;
			object result2 = null;
			try
			{
				result2 = createBinaryFormatter().Deserialize(BaseStream);
				return result2;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Exception deserializing an unknown type: " + ex);
				return result2;
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolveHandler;
			}
		}
		case SerializedType.OtherModType:
		{
			AppDomain.CurrentDomain.AssemblyResolve += modAssemblyResolveHandler;
			object result = null;
			try
			{
				result = createBinaryFormatter().Deserialize(BaseStream);
				return result;
			}
			catch
			{
				return result;
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= modAssemblyResolveHandler;
			}
		}
		case SerializedType.UInt16Type:
			return ReadUInt16();
		case SerializedType.ZeroUInt16Type:
			return (ushort)0;
		case SerializedType.UInt32Type:
			return ReadUInt32();
		case SerializedType.ZeroUInt32Type:
			return 0u;
		case SerializedType.OptimizedUInt32Type:
			return ReadOptimizedUInt32();
		case SerializedType.UInt64Type:
			return ReadUInt64();
		case SerializedType.ZeroUInt64Type:
			return 0uL;
		case SerializedType.OptimizedUInt64Type:
			return ReadOptimizedUInt64();
		case SerializedType.BitVector32Type:
			return ReadBitVector32();
		case SerializedType.CharType:
			return ReadChar();
		case SerializedType.ZeroCharType:
			return '\0';
		case SerializedType.SByteType:
			return ReadSByte();
		case SerializedType.ZeroSByteType:
			return (sbyte)0;
		case SerializedType.OneByteType:
			return (byte)1;
		case SerializedType.OneDoubleType:
			return 1.0;
		case SerializedType.OneCharType:
			return '\u0001';
		case SerializedType.OneInt16Type:
			return (short)1;
		case SerializedType.OneInt64Type:
			return 1L;
		case SerializedType.OneUInt16Type:
			return (ushort)1;
		case SerializedType.OptimizedUInt16Type:
			return ReadOptimizedUInt16();
		case SerializedType.OneUInt32Type:
			return 1u;
		case SerializedType.OneUInt64Type:
			return 1uL;
		case SerializedType.OneSByteType:
			return (sbyte)1;
		case SerializedType.OneSingleType:
			return 1f;
		case SerializedType.BitArrayType:
			return ReadOptimizedBitArray();
		case SerializedType.TypeType:
			return Type.GetType(ReadOptimizedString(), throwOnError: false);
		case SerializedType.ArrayListType:
			return ReadOptimizedArrayList();
		case SerializedType.StringListType:
			return ReadStringList();
		case SerializedType.SingleInstanceType:
			try
			{
				return Activator.CreateInstance(ModManager.ResolveType(ReadStringDirect()), nonPublic: true);
			}
			catch
			{
				return null;
			}
		case SerializedType.OwnedDataSerializableAndRecreatableType:
		{
			object obj2 = Activator.CreateInstance(ReadOptimizedType());
			ReadOwnedData((IOwnedDataSerializable)obj2, null);
			return obj2;
		}
		case SerializedType.OptimizedEnumType:
		{
			Type enumType2 = ReadOptimizedType();
			Type underlyingType2 = Enum.GetUnderlyingType(enumType2);
			if (underlyingType2 == typeof(int) || underlyingType2 == typeof(uint) || underlyingType2 == typeof(long) || underlyingType2 == typeof(ulong))
			{
				return Enum.ToObject(enumType2, ReadOptimizedUInt64());
			}
			return Enum.ToObject(enumType2, ReadUInt64());
		}
		case SerializedType.EnumType:
		{
			Type enumType = ReadOptimizedType();
			Type underlyingType = Enum.GetUnderlyingType(enumType);
			if (underlyingType == typeof(int))
			{
				return Enum.ToObject(enumType, ReadInt32());
			}
			if (underlyingType == typeof(byte))
			{
				return Enum.ToObject(enumType, ReadByte());
			}
			if (underlyingType == typeof(short))
			{
				return Enum.ToObject(enumType, ReadInt16());
			}
			if (underlyingType == typeof(uint))
			{
				return Enum.ToObject(enumType, ReadUInt32());
			}
			if (underlyingType == typeof(long))
			{
				return Enum.ToObject(enumType, ReadInt64());
			}
			if (underlyingType == typeof(sbyte))
			{
				return Enum.ToObject(enumType, ReadSByte());
			}
			if (underlyingType == typeof(ushort))
			{
				return Enum.ToObject(enumType, ReadUInt16());
			}
			return Enum.ToObject(enumType, ReadUInt64());
		}
		case SerializedType.SurrogateHandledType:
		{
			Type type = ReadOptimizedType();
			return SerializationWriter.findSurrogateForType(type).Deserialize(this, type);
		}
		default:
		{
			object obj = processArrayTypes(typeCode, null);
			if (obj != null)
			{
				return obj;
			}
			throw new InvalidOperationException("Unrecognized TypeCode: " + typeCode);
		}
		}
	}

	private static IFormatter createBinaryFormatter()
	{
		if (formatter == null)
		{
			Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
			formatter = new BinaryFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			formatter.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}
		return formatter;
	}

	/// <summary>
	///             Determine whether the passed-in type code refers to an array type
	///             and deserializes the array if it is.
	///             Returns null if not an array type.
	///             </summary><param name="typeCode">The SerializedType to check.</param><param name="defaultElementType">The Type of array element; null if to be read from stream.</param><returns />
	private object processArrayTypes(SerializedType typeCode, Type defaultElementType)
	{
		switch (typeCode)
		{
		case SerializedType.StringArrayType:
			return ReadOptimizedStringArray();
		case SerializedType.Int32ArrayType:
			return ReadInt32Array();
		case SerializedType.Int64ArrayType:
			return ReadInt64Array();
		case SerializedType.DecimalArrayType:
			return readDecimalArray();
		case SerializedType.TimeSpanArrayType:
			return ReadTimeSpanArray();
		case SerializedType.UInt32ArrayType:
			return ReadUInt32Array();
		case SerializedType.UInt64ArrayType:
			return ReadUInt64Array();
		case SerializedType.DateTimeArrayType:
			return ReadDateTimeArray();
		case SerializedType.BooleanArrayType:
			return readBooleanArray();
		case SerializedType.ByteArrayType:
			return readByteArray();
		case SerializedType.CharArrayType:
			return readCharArray();
		case SerializedType.DoubleArrayType:
			return readDoubleArray();
		case SerializedType.SingleArrayType:
			return readSingleArray();
		case SerializedType.GuidArrayType:
			return readGuidArray();
		case SerializedType.SByteArrayType:
			return readSByteArray();
		case SerializedType.Int16ArrayType:
			return ReadInt16Array();
		case SerializedType.UInt16ArrayType:
			return ReadUInt16Array();
		case SerializedType.EmptyTypedArrayType:
			return Array.CreateInstance((defaultElementType != null) ? defaultElementType : ReadOptimizedType(), 0);
		case SerializedType.OtherTypedArrayType:
			return ReadOptimizedObjectArray(ReadOptimizedType());
		case SerializedType.ObjectArrayType:
			return ReadOptimizedObjectArray(defaultElementType);
		case SerializedType.NonOptimizedTypedArrayType:
		case SerializedType.FullyOptimizedTypedArrayType:
		case SerializedType.PartiallyOptimizedTypedArrayType:
		{
			BitArray bitArray = readTypedArrayOptimizeFlags(typeCode);
			int num = ReadOptimizedInt32();
			if (defaultElementType == null)
			{
				defaultElementType = ReadOptimizedType();
			}
			Array array = Array.CreateInstance(defaultElementType, num);
			for (int i = 0; i < num; i++)
			{
				if (bitArray == null)
				{
					array.SetValue(ReadObject(), i);
				}
				else if (bitArray == FullyOptimizableTypedArray || !bitArray[i])
				{
					IOwnedDataSerializable ownedDataSerializable = (IOwnedDataSerializable)Activator.CreateInstance(defaultElementType);
					ReadOwnedData(ownedDataSerializable, null);
					array.SetValue(ownedDataSerializable, i);
				}
			}
			return array;
		}
		default:
			return null;
		}
	}

	private string readTokenizedString(int bucket)
	{
		return stringTokenList[(ReadOptimizedInt32() << 7) + bucket];
	}

	/// <summary>
	///             Returns the SerializedType read next from the stream.
	///             </summary><returns>A SerializedType value.</returns>
	private SerializedType readTypeCode()
	{
		return (SerializedType)ReadByte();
	}

	private bool[] readBooleanArray()
	{
		BitArray bitArray = ReadOptimizedBitArray();
		bool[] array = new bool[bitArray.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = bitArray[i];
		}
		return array;
	}

	/// <summary>
	///             Internal implementation returning a Byte[].
	///             </summary><returns>A Byte[].</returns>
	private byte[] readByteArray()
	{
		return base.ReadBytes(ReadOptimizedInt32());
	}

	private char[] readCharArray()
	{
		return base.ReadChars(ReadOptimizedInt32());
	}

	/// <summary>
	///             Internal implementation returning a Decimal[].
	///             </summary><returns>A Decimal[].</returns>
	private decimal[] readDecimalArray()
	{
		decimal[] array = new decimal[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadOptimizedDecimal();
		}
		return array;
	}

	private double[] readDoubleArray()
	{
		double[] array = new double[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadDouble();
		}
		return array;
	}

	/// <summary>
	///             Internal implementation returning a Guid[].
	///             </summary><returns>A Guid[].</returns>
	private Guid[] readGuidArray()
	{
		Guid[] array = new Guid[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadGuid();
		}
		return array;
	}

	private sbyte[] readSByteArray()
	{
		sbyte[] array = new sbyte[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadSByte();
		}
		return array;
	}

	/// <summary>
	///             Internal implementation returning a Single[].
	///             </summary><returns>A Single[].</returns>
	private float[] readSingleArray()
	{
		float[] array = new float[ReadOptimizedInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadSingle();
		}
		return array;
	}

	[Conditional("DEBUG")]
	public void DumpStringTables(ArrayList list)
	{
		list.AddRange(stringTokenList);
	}
}
