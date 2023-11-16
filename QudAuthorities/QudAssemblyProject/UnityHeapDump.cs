using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class UnityHeapDump
{
	private class StructOrClass
	{
		private List<StructOrClass> Children = new List<StructOrClass>();

		public int Size { get; private set; }

		public Type ParsedType { get; private set; }

		public int InstanceID { get; private set; }

		private int ArraySize { get; set; }

		private string Identifier { get; set; }

		public StructOrClass(Type type, string assemblyFolder)
		{
			StructOrClass structOrClass = this;
			ParsedType = type;
			HashSet<object> seenObjects = new HashSet<object>();
			Identifier = type.FullName;
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				ParseField(fieldInfo, null, seenObjects);
			}
			if (Size < 1)
			{
				return;
			}
			Interlocked.Increment(ref ThreadJobsPrinting);
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					string arg = structOrClass.Identifier.Replace("<", "(").Replace(">", ")").Replace(".", "_");
					using StreamWriter streamWriter = new StreamWriter($"{assemblyFolder}{structOrClass.Size}-{arg}");
					streamWriter.WriteLine("Static ({0}): {1} bytes", structOrClass.ParsedType, structOrClass.Size);
					structOrClass.Children.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
					string indent = "    ";
					foreach (StructOrClass child in structOrClass.Children)
					{
						if (child.Size < 1)
						{
							break;
						}
						child.Write(streamWriter, indent);
					}
				}
				finally
				{
					Interlocked.Increment(ref ThreadJobsDone);
				}
			});
		}

		public StructOrClass(UnityEngine.Object uObject)
		{
			InstanceID = uObject.GetInstanceID();
			ParsedType = uObject.GetType();
			Identifier = uObject.name + uObject.GetInstanceID();
			HashSet<object> seenObjects = new HashSet<object>();
			FieldInfo[] fields = ParsedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				ParseField(fieldInfo, uObject, seenObjects);
			}
			if (Size < 1)
			{
				return;
			}
			using StreamWriter streamWriter = new StreamWriter(string.Format("dump/{0}/{1}-{2}", (uObject is ScriptableObject) ? "sobjects" : "uobjects", Size, Identifier.Replace("<", "(").Replace(">", ")").Replace(".", "_")));
			streamWriter.WriteLine("{0} ({1}): {2} bytes", Identifier, ParsedType, Size);
			Children.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
			foreach (StructOrClass child in Children)
			{
				if (child.Size >= 1)
				{
					child.Write(streamWriter, "    ");
				}
			}
		}

		private StructOrClass(string name, object root, TypeData rootTypeData, HashSet<object> seenObjects)
		{
			Identifier = name;
			ParsedType = root.GetType();
			Size = rootTypeData.Size;
			if (ParsedType.IsArray)
			{
				int num = 0;
				ArraySize = GetTotalLength((Array)root);
				Type elementType = ParsedType.GetElementType();
				TypeData typeData = TypeData.Get(elementType);
				if (!elementType.IsValueType && !elementType.IsPrimitive && !elementType.IsEnum)
				{
					Size += IntPtr.Size * ArraySize;
					{
						foreach (object item in (Array)root)
						{
							ParseItem(item, num++.ToString(), seenObjects);
						}
						return;
					}
				}
				if (typeData.DynamicSizedFields != null)
				{
					foreach (object item2 in (Array)root)
					{
						StructOrClass structOrClass = new StructOrClass(num++.ToString(), item2, typeData, seenObjects);
						Size += structOrClass.Size;
						Children.Add(structOrClass);
					}
					return;
				}
				Size += typeData.Size * ArraySize;
			}
			else
			{
				if (rootTypeData.DynamicSizedFields == null)
				{
					return;
				}
				foreach (FieldInfo dynamicSizedField in rootTypeData.DynamicSizedFields)
				{
					ParseField(dynamicSizedField, root, seenObjects);
				}
			}
		}

		private void ParseField(FieldInfo fieldInfo, object root, HashSet<object> seenObjects)
		{
			if (!fieldInfo.FieldType.IsPointer)
			{
				ParseItem(fieldInfo.GetValue(root), fieldInfo.Name, seenObjects);
			}
		}

		private void ParseItem(object obj, string objName, HashSet<object> seenObjects)
		{
			if (obj == null)
			{
				return;
			}
			Type type = obj.GetType();
			if (type.IsPointer)
			{
				return;
			}
			if (type == typeof(string))
			{
				int num = 3 * IntPtr.Size + 2;
				num += ((string)obj).Length * 2;
				int num2 = num % IntPtr.Size;
				if (num2 != 0)
				{
					num += IntPtr.Size - num2;
				}
				Size += num;
				return;
			}
			TypeData typeData = TypeData.Get(type);
			if (type.IsClass || type.IsArray || typeData.DynamicSizedFields != null)
			{
				if (type.IsPrimitive || type.IsValueType || type.IsEnum || seenObjects.Add(obj))
				{
					StructOrClass structOrClass = new StructOrClass(objName, obj, typeData, seenObjects);
					Size += structOrClass.Size;
					Children.Add(structOrClass);
				}
			}
			else
			{
				Size += typeData.Size;
			}
		}

		private void Write(StreamWriter writer, string indent)
		{
			if (ParsedType.IsArray)
			{
				writer.WriteLine("{0}{1} ({2}:{3}) : {4}", indent, Identifier, ParsedType, ArraySize, Size);
			}
			else
			{
				writer.WriteLine("{0}{1} ({2}) : {3}", indent, Identifier, ParsedType, Size);
			}
			Children.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
			string indent2 = indent + "    ";
			foreach (StructOrClass child in Children)
			{
				if (child.Size >= 1)
				{
					child.Write(writer, indent2);
					continue;
				}
				break;
			}
		}

		private static int GetTotalLength(Array val)
		{
			int num = val.GetLength(0);
			for (int i = 1; i < val.Rank; i++)
			{
				num *= val.GetLength(i);
			}
			return num;
		}
	}

	public class TypeData
	{
		private static Dictionary<Type, TypeData> seenTypeData;

		private static Dictionary<Type, TypeData> seenTypeDataNested;

		public int Size { get; private set; }

		public List<FieldInfo> DynamicSizedFields { get; private set; }

		public static void Clear()
		{
			seenTypeData = null;
		}

		public static void Start()
		{
			seenTypeData = new Dictionary<Type, TypeData>();
			seenTypeDataNested = new Dictionary<Type, TypeData>();
		}

		public static TypeData Get(Type type)
		{
			if (!seenTypeData.TryGetValue(type, out var value))
			{
				value = new TypeData(type);
				seenTypeData[type] = value;
			}
			return value;
		}

		public static TypeData GetNested(Type type)
		{
			if (!seenTypeDataNested.TryGetValue(type, out var value))
			{
				value = new TypeData(type, nested: true);
				seenTypeDataNested[type] = value;
			}
			return value;
		}

		public TypeData(Type type, bool nested = false)
		{
			if (type.IsGenericType)
			{
				genericTypes.Add(type);
			}
			Type baseType = type.BaseType;
			if (baseType != null && baseType != typeof(object) && baseType != typeof(ValueType) && baseType != typeof(Array) && baseType != typeof(Enum))
			{
				TypeData nested2 = GetNested(baseType);
				Size += nested2.Size;
				if (nested2.DynamicSizedFields != null)
				{
					DynamicSizedFields = new List<FieldInfo>(nested2.DynamicSizedFields);
				}
			}
			if (type.IsPointer)
			{
				Size = IntPtr.Size;
				return;
			}
			if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				Size = ((elementType.IsValueType || elementType.IsPrimitive || elementType.IsEnum) ? 3 : 4) * IntPtr.Size;
				return;
			}
			if (type.IsPrimitive)
			{
				Size = Marshal.SizeOf(type);
				return;
			}
			if (type.IsEnum)
			{
				Size = Marshal.SizeOf(Enum.GetUnderlyingType(type));
				return;
			}
			if (!nested && type.IsClass)
			{
				Size = 2 * IntPtr.Size;
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				ProcessField(fieldInfo, fieldInfo.FieldType);
			}
			if (!nested && type.IsClass)
			{
				Size = Math.Max(3 * IntPtr.Size, Size);
				int num = Size % IntPtr.Size;
				if (num != 0)
				{
					Size += IntPtr.Size - num;
				}
			}
		}

		private void ProcessField(FieldInfo field, Type fieldType)
		{
			if (IsStaticallySized(fieldType))
			{
				Size += GetStaticSize(fieldType);
				return;
			}
			if (!fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
			{
				Size += IntPtr.Size;
			}
			if (!fieldType.IsPointer)
			{
				if (DynamicSizedFields == null)
				{
					DynamicSizedFields = new List<FieldInfo>();
				}
				DynamicSizedFields.Add(field);
			}
		}

		private static bool IsStaticallySized(Type type)
		{
			if (type.IsPointer || type.IsArray || type.IsClass || type.IsInterface)
			{
				return false;
			}
			if (type.IsPrimitive || type.IsEnum)
			{
				return true;
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (!IsStaticallySized(fields[i].FieldType))
				{
					return false;
				}
			}
			return true;
		}

		private static int GetStaticSize(Type type)
		{
			if (type.IsPrimitive)
			{
				return Marshal.SizeOf(type);
			}
			if (type.IsEnum)
			{
				return Marshal.SizeOf(Enum.GetUnderlyingType(type));
			}
			int num = 0;
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				num += GetStaticSize(fieldInfo.FieldType);
			}
			return num;
		}
	}

	private const int TYPE_MIN_SIZE_TO_PRINT = 1;

	private const int ROOT_MIN_SIZE = 1;

	private const int CHILD_MIN_SIZE = 1;

	private static int ThreadJobsPrinting = 0;

	private static int ThreadJobsDone = 0;

	private static HashSet<Type> genericTypes = new HashSet<Type>();

	public static void Create()
	{
		TypeData.Start();
		ThreadJobsPrinting = 0;
		ThreadJobsDone = 0;
		if (Directory.Exists("dump"))
		{
			Directory.Delete("dump", recursive: true);
		}
		Directory.CreateDirectory("dump");
		Directory.CreateDirectory("dump/sobjects");
		Directory.CreateDirectory("dump/uobjects");
		Directory.CreateDirectory("dump/statics");
		using (StreamWriter streamWriter = new StreamWriter("dump/log.txt"))
		{
			Dictionary<Assembly, List<StructOrClass>> dictionary = new Dictionary<Assembly, List<StructOrClass>>();
			Dictionary<Assembly, int> dictionary2 = new Dictionary<Assembly, int>();
			List<KeyValuePair<Type, Exception>> list = new List<KeyValuePair<Type, Exception>>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				string text = ((!assembly.FullName.Contains("Assembly-CSharp")) ? "dump/statics/misc/" : string.Format("dump/statics/{0}/", assembly.FullName.Replace("<", "(").Replace(">", ")").Replace(".", "_")));
				Directory.CreateDirectory(text);
				List<StructOrClass> list2 = new List<StructOrClass>();
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (!type.IsEnum && !type.IsGenericType)
					{
						try
						{
							list2.Add(new StructOrClass(type, text));
						}
						catch (Exception value)
						{
							list.Add(new KeyValuePair<Type, Exception>(type, value));
						}
					}
				}
				dictionary[assembly] = list2;
			}
			List<StructOrClass> list3 = new List<StructOrClass>();
			GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
			for (int i = 0; i < array.Length; i++)
			{
				Component[] components = array[i].GetComponents<Component>();
				foreach (Component component in components)
				{
					if (!(component == null))
					{
						try
						{
							list3.Add(new StructOrClass(component));
						}
						catch (Exception value2)
						{
							list.Add(new KeyValuePair<Type, Exception>(component.GetType(), value2));
						}
					}
				}
			}
			List<StructOrClass> list4 = new List<StructOrClass>();
			ScriptableObject[] array2 = Resources.FindObjectsOfTypeAll<ScriptableObject>();
			foreach (ScriptableObject scriptableObject in array2)
			{
				if (!(scriptableObject == null))
				{
					try
					{
						list4.Add(new StructOrClass(scriptableObject));
					}
					catch (Exception value3)
					{
						list.Add(new KeyValuePair<Type, Exception>(scriptableObject.GetType(), value3));
					}
				}
			}
			foreach (Type item in genericTypes.ToList())
			{
				try
				{
					dictionary[item.Assembly].Add(new StructOrClass(item, "dump/statics/misc/"));
				}
				catch (Exception value4)
				{
					list.Add(new KeyValuePair<Type, Exception>(item, value4));
				}
			}
			foreach (KeyValuePair<Assembly, List<StructOrClass>> item2 in dictionary)
			{
				dictionary2[item2.Key] = item2.Value.Sum((StructOrClass a) => a.Size);
				item2.Value.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
			}
			TypeData.Clear();
			List<KeyValuePair<Assembly, int>> list5 = dictionary2.ToList();
			list5.Sort((KeyValuePair<Assembly, int> a, KeyValuePair<Assembly, int> b) => b.Value - a.Value);
			list3.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
			int num = list3.Sum((StructOrClass a) => a.Size);
			bool flag = false;
			list4.Sort((StructOrClass a, StructOrClass b) => b.Size - a.Size);
			int num2 = list4.Sum((StructOrClass a) => a.Size);
			bool flag2 = false;
			streamWriter.WriteLine("Total tracked memory (including duplicates, so too high) = {0}", list5.Sum((KeyValuePair<Assembly, int> a) => a.Value) + num + num2);
			foreach (KeyValuePair<Assembly, int> item3 in list5)
			{
				Assembly key = item3.Key;
				int value5 = item3.Value;
				if (!flag && value5 < num)
				{
					flag = true;
					streamWriter.WriteLine("Unity components of total size: {0}", num);
					foreach (StructOrClass item4 in list3)
					{
						if (item4.Size >= 1)
						{
							streamWriter.WriteLine("    Type {0} (ID: {1}) of size {2}", item4.ParsedType.FullName, item4.InstanceID, item4.Size);
						}
					}
				}
				if (!flag2 && value5 < num2)
				{
					flag2 = true;
					streamWriter.WriteLine("Unity scriptableobjects of total size: {0}", num2);
					foreach (StructOrClass item5 in list4)
					{
						if (item5.Size >= 1)
						{
							streamWriter.WriteLine("    Type {0} (ID: {1}) of size {2}", item5.ParsedType.FullName, item5.InstanceID, item5.Size);
						}
					}
				}
				streamWriter.WriteLine("Assembly: {0} of total size: {1}", key, value5);
				foreach (StructOrClass item6 in dictionary[key])
				{
					if (item6.Size >= 1)
					{
						streamWriter.WriteLine("    Type: {0} of size {1}", item6.ParsedType.FullName, item6.Size);
					}
				}
			}
			foreach (KeyValuePair<Type, Exception> item7 in list)
			{
				streamWriter.WriteLine(item7);
			}
		}
		while (ThreadJobsDone < ThreadJobsPrinting)
		{
			Thread.Sleep(1);
		}
	}
}
