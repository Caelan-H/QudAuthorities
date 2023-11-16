using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XRL.World;

public class GamePartBlueprint
{
	public interface IFieldInitializer
	{
		void Initialize(IPart P);
	}

	public class FieldInitializationValue<T> : IFieldInitializer
	{
		public FieldInfo F;

		public T Value;

		public FieldInitializationValue(FieldInfo Field, T InitialValue)
		{
			F = Field;
			Value = InitialValue;
		}

		public void Initialize(IPart P)
		{
			F.SetValue(P, Value);
		}
	}

	public class PropertyInitializationValue<T> : IFieldInitializer
	{
		public PropertyInfo Pr;

		public T Value;

		public PropertyInitializationValue(PropertyInfo Property, T InitialValue)
		{
			Pr = Property;
			Value = InitialValue;
		}

		public void Initialize(IPart P)
		{
			Pr.SetValue(P, Value, null);
		}
	}

	public string Name = "";

	public Dictionary<string, string> Parameters = new Dictionary<string, string>();

	private Type _T;

	public MethodInfo finalizeBuild;

	public FieldInfo[] TFields;

	public PropertyInfo[] TProperties;

	public List<IFieldInitializer> FieldValues;

	public List<IFieldInitializer> PropertyValues;

	public Type T
	{
		get
		{
			if (_T == null)
			{
				string typeID = "XRL.World.Parts." + Name;
				_T = ModManager.ResolveType(typeID);
				if (_T == null)
				{
					return null;
				}
				TFields = _T.GetFields();
				TProperties = _T.GetProperties();
				if (_T != null)
				{
					finalizeBuild = _T.GetMethod("FinalizeBuild");
				}
			}
			return _T;
		}
	}

	public GamePartBlueprint()
	{
	}

	public GamePartBlueprint(string Name)
	{
		this.Name = Name;
	}

	public string GetParameter(string Parameter, string Default = null)
	{
		if (Parameters.TryGetValue(Parameter, out var value))
		{
			return value;
		}
		return Default;
	}

	public void InitializePartInstance(IPart NewPart)
	{
		if (FieldValues == null)
		{
			FieldValues = new List<IFieldInitializer>();
			PropertyValues = new List<IFieldInitializer>();
			if (TProperties != null)
			{
				int i = 0;
				for (int num = TProperties.Count(); i < num; i++)
				{
					PropertyInfo propertyInfo = TProperties[i];
					if (propertyInfo.Name != "Name" && Parameters.TryGetValue(propertyInfo.Name, out var value) && propertyInfo.CanWrite)
					{
						if (propertyInfo.PropertyType == typeof(bool))
						{
							PropertyValues.Add(new PropertyInitializationValue<bool>(propertyInfo, Convert.ToBoolean(value)));
						}
						else if (propertyInfo.PropertyType == typeof(int))
						{
							PropertyValues.Add(new PropertyInitializationValue<int>(propertyInfo, int.Parse(value)));
						}
						else if (propertyInfo.PropertyType == typeof(long))
						{
							PropertyValues.Add(new PropertyInitializationValue<long>(propertyInfo, long.Parse(value)));
						}
						else if (propertyInfo.PropertyType == typeof(short))
						{
							PropertyValues.Add(new PropertyInitializationValue<short>(propertyInfo, short.Parse(value)));
						}
						else if (propertyInfo.PropertyType == typeof(float))
						{
							PropertyValues.Add(new PropertyInitializationValue<float>(propertyInfo, Convert.ToSingle(value)));
						}
						else if (propertyInfo.PropertyType == typeof(double))
						{
							PropertyValues.Add(new PropertyInitializationValue<double>(propertyInfo, Convert.ToDouble(value)));
						}
						else if (propertyInfo.PropertyType.IsEnum)
						{
							object obj = Enum.Parse(propertyInfo.PropertyType, value);
							object obj2 = Activator.CreateInstance(typeof(PropertyInitializationValue<>).MakeGenericType(propertyInfo.PropertyType), propertyInfo, obj);
							PropertyValues.Add(obj2 as IFieldInitializer);
						}
						else
						{
							PropertyValues.Add(new PropertyInitializationValue<object>(propertyInfo, value));
						}
					}
				}
			}
			if (TFields != null)
			{
				int j = 0;
				for (int num2 = TFields.Length; j < num2; j++)
				{
					FieldInfo fieldInfo = TFields[j];
					if (!Parameters.TryGetValue(fieldInfo.Name, out var value2))
					{
						continue;
					}
					if (fieldInfo.FieldType == typeof(Color))
					{
						try
						{
							string[] array = value2.Split(',');
							FieldValues.Add(new FieldInitializationValue<Color>(fieldInfo, new Color(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), 1f)));
						}
						catch
						{
							FieldValues.Add(new FieldInitializationValue<Color>(fieldInfo, Color.black));
						}
					}
					else if (fieldInfo.FieldType == typeof(bool))
					{
						FieldValues.Add(new FieldInitializationValue<bool>(fieldInfo, Convert.ToBoolean(value2)));
					}
					else if (fieldInfo.FieldType == typeof(long))
					{
						FieldValues.Add(new FieldInitializationValue<long>(fieldInfo, Convert.ToInt32(value2)));
					}
					else if (fieldInfo.FieldType == typeof(int))
					{
						FieldValues.Add(new FieldInitializationValue<int>(fieldInfo, Convert.ToInt32(value2)));
					}
					else if (fieldInfo.FieldType == typeof(short))
					{
						FieldValues.Add(new FieldInitializationValue<short>(fieldInfo, Convert.ToInt16(value2)));
					}
					else if (fieldInfo.FieldType == typeof(double))
					{
						FieldValues.Add(new FieldInitializationValue<double>(fieldInfo, Convert.ToDouble(value2)));
					}
					else if (fieldInfo.FieldType == typeof(float))
					{
						FieldValues.Add(new FieldInitializationValue<float>(fieldInfo, Convert.ToSingle(value2)));
					}
					else if (fieldInfo.FieldType.IsEnum)
					{
						object obj4 = Enum.Parse(fieldInfo.FieldType, value2);
						object obj5 = Activator.CreateInstance(typeof(FieldInitializationValue<>).MakeGenericType(fieldInfo.FieldType), fieldInfo, obj4);
						FieldValues.Add(obj5 as IFieldInitializer);
					}
					else
					{
						FieldValues.Add(new FieldInitializationValue<object>(fieldInfo, value2));
					}
				}
			}
		}
		int k = 0;
		for (int count = FieldValues.Count; k < count; k++)
		{
			FieldValues[k].Initialize(NewPart);
		}
		int l = 0;
		for (int count2 = PropertyValues.Count; l < count2; l++)
		{
			PropertyValues[l].Initialize(NewPart);
		}
	}

	public void CopyFrom(GamePartBlueprint Source)
	{
		foreach (KeyValuePair<string, string> parameter in Source.Parameters)
		{
			Parameters[parameter.Key] = parameter.Value;
		}
	}
}
