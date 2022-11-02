using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleJSON;

public class JSONNode
{
	public string Key;

	public virtual JSONNode this[int aIndex]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual JSONNode this[string aKey]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual string Value
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public virtual int Count => 0;

	public virtual IEnumerable<JSONNode> Childs
	{
		get
		{
			yield break;
		}
	}

	public IEnumerable<JSONNode> DeepChilds
	{
		get
		{
			foreach (JSONNode child in Childs)
			{
				foreach (JSONNode deepChild in child.DeepChilds)
				{
					yield return deepChild;
				}
			}
		}
	}

	public virtual int AsInt
	{
		get
		{
			int result = 0;
			if (int.TryParse(Value, out result))
			{
				return result;
			}
			return 0;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual float AsFloat
	{
		get
		{
			float result = 0f;
			if (float.TryParse(Value, out result))
			{
				return result;
			}
			return 0f;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual double AsDouble
	{
		get
		{
			double result = 0.0;
			if (double.TryParse(Value, out result))
			{
				return result;
			}
			return 0.0;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual bool AsBool
	{
		get
		{
			bool result = false;
			if (bool.TryParse(Value, out result))
			{
				return result;
			}
			return !string.IsNullOrEmpty(Value);
		}
		set
		{
			Value = (value ? "true" : "false");
		}
	}

	public virtual JSONArray AsArray => this as JSONArray;

	public virtual JSONClass AsObject => this as JSONClass;

	public virtual void Add(string aKey, JSONNode aItem)
	{
	}

	public virtual void Add(JSONNode aItem)
	{
		Add("", aItem);
	}

	public virtual JSONNode Remove(string aKey)
	{
		return null;
	}

	public virtual JSONNode Remove(int aIndex)
	{
		return null;
	}

	public virtual JSONNode Remove(JSONNode aNode)
	{
		return aNode;
	}

	public override string ToString()
	{
		return "JSONNode";
	}

	public virtual string ToString(string aPrefix)
	{
		return "JSONNode";
	}

	public static implicit operator JSONNode(string s)
	{
		return new JSONData(s);
	}

	public static implicit operator string(JSONNode d)
	{
		if (!(d == null))
		{
			return d.Value;
		}
		return null;
	}

	public static bool operator ==(JSONNode a, object b)
	{
		if (b == null && a is JSONLazyCreator)
		{
			return true;
		}
		return (object)a == b;
	}

	public static bool operator !=(JSONNode a, object b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		return (object)this == obj;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal static string Escape(string aText)
	{
		string text = "";
		for (int i = 0; i < aText.Length; i++)
		{
			char c = aText[i];
			text = c switch
			{
				'\\' => text + "\\\\", 
				'"' => text + "\\\"", 
				'\n' => text + "\\n", 
				'\r' => text + "\\r", 
				'\t' => text + "\\t", 
				'\b' => text + "\\b", 
				'\f' => text + "\\f", 
				_ => text + c, 
			};
		}
		return text;
	}

	public static JSONNode Parse(string aJSON)
	{
		Stack<JSONNode> stack = new Stack<JSONNode>();
		JSONNode jSONNode = null;
		int i = 0;
		string text = "";
		string text2 = "";
		bool flag = false;
		for (; i < aJSON.Length; i++)
		{
			switch (aJSON[i])
			{
			case '{':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				stack.Push(new JSONClass());
				if (jSONNode != null)
				{
					text2 = text2.Trim();
					if (jSONNode is JSONArray)
					{
						jSONNode.Add(stack.Peek());
					}
					else if (text2 != "")
					{
						jSONNode.Add(text2, stack.Peek());
					}
				}
				text2 = "";
				text = "";
				jSONNode = stack.Peek();
				break;
			case '[':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				stack.Push(new JSONArray());
				if (jSONNode != null)
				{
					text2 = text2.Trim();
					if (jSONNode is JSONArray)
					{
						jSONNode.Add(stack.Peek());
					}
					else if (text2 != "")
					{
						jSONNode.Add(text2, stack.Peek());
					}
				}
				text2 = "";
				text = "";
				jSONNode = stack.Peek();
				break;
			case ']':
			case '}':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				if (stack.Count == 0)
				{
					throw new Exception("JSON Parse: Too many closing brackets");
				}
				stack.Pop();
				if (text != "")
				{
					text2 = text2.Trim();
					if (jSONNode is JSONArray)
					{
						jSONNode.Add(text);
					}
					else if (text2 != "")
					{
						jSONNode.Add(text2, text);
					}
				}
				text2 = "";
				text = "";
				if (stack.Count > 0)
				{
					jSONNode = stack.Peek();
				}
				break;
			case ':':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				text2 = text;
				text = "";
				break;
			case '"':
				flag = !flag;
				break;
			case ',':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				if (text != "")
				{
					if (jSONNode is JSONArray)
					{
						jSONNode.Add(text);
					}
					else if (text2 != "")
					{
						jSONNode.Add(text2, text);
					}
				}
				text2 = "";
				text = "";
				break;
			case '\t':
			case ' ':
				if (flag)
				{
					text += aJSON[i];
				}
				break;
			case '\\':
				i++;
				if (flag)
				{
					char c = aJSON[i];
					switch (c)
					{
					case 't':
						text += "\t";
						break;
					case 'r':
						text += "\r";
						break;
					case 'n':
						text += "\n";
						break;
					case 'b':
						text += "\b";
						break;
					case 'f':
						text += "\f";
						break;
					case 'u':
					{
						string s = aJSON.Substring(i + 1, 4);
						text += (char)int.Parse(s, NumberStyles.AllowHexSpecifier);
						i += 4;
						break;
					}
					default:
						text += c;
						break;
					}
				}
				break;
			default:
				text += aJSON[i];
				break;
			case '\n':
			case '\r':
				break;
			}
		}
		if (flag)
		{
			throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
		}
		return jSONNode;
	}

	public virtual void Serialize(BinaryWriter aWriter)
	{
	}

	public void SaveToStream(Stream aData)
	{
		BinaryWriter aWriter = new BinaryWriter(aData);
		Serialize(aWriter);
	}

	public void SaveToCompressedStream(Stream aData)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public void SaveToCompressedFile(string aFileName)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public string SaveToCompressedBase64()
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public void SaveToFile(string aFileName)
	{
		Directory.CreateDirectory(new FileInfo(aFileName).Directory.FullName);
		using FileStream aData = File.OpenWrite(aFileName);
		SaveToStream(aData);
	}

	public string SaveToBase64()
	{
		using MemoryStream memoryStream = new MemoryStream();
		SaveToStream(memoryStream);
		memoryStream.Position = 0L;
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static JSONNode Deserialize(BinaryReader aReader)
	{
		JSONBinaryTag jSONBinaryTag = (JSONBinaryTag)aReader.ReadByte();
		switch (jSONBinaryTag)
		{
		case JSONBinaryTag.Array:
		{
			int num2 = aReader.ReadInt32();
			JSONArray jSONArray = new JSONArray();
			for (int j = 0; j < num2; j++)
			{
				jSONArray.Add(Deserialize(aReader));
			}
			return jSONArray;
		}
		case JSONBinaryTag.Class:
		{
			int num = aReader.ReadInt32();
			JSONClass jSONClass = new JSONClass();
			for (int i = 0; i < num; i++)
			{
				string text = aReader.ReadString();
				JSONNode jSONNode = Deserialize(aReader);
				jSONNode.Key = text;
				jSONClass.Add(text, jSONNode);
			}
			return jSONClass;
		}
		case JSONBinaryTag.Value:
			return new JSONData(aReader.ReadString());
		case JSONBinaryTag.IntValue:
			return new JSONData(aReader.ReadInt32());
		case JSONBinaryTag.DoubleValue:
			return new JSONData(aReader.ReadDouble());
		case JSONBinaryTag.BoolValue:
			return new JSONData(aReader.ReadBoolean());
		case JSONBinaryTag.FloatValue:
			return new JSONData(aReader.ReadSingle());
		default:
			throw new Exception("Error deserializing JSON. Unknown tag: " + jSONBinaryTag);
		}
	}

	public static JSONNode LoadFromCompressedFile(string aFileName)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static JSONNode LoadFromCompressedStream(Stream aData)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static JSONNode LoadFromCompressedBase64(string aBase64)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static JSONNode LoadFromStream(Stream aData)
	{
		using BinaryReader aReader = new BinaryReader(aData);
		return Deserialize(aReader);
	}

	public static JSONNode LoadFromFile(string aFileName)
	{
		using FileStream aData = File.OpenRead(aFileName);
		return LoadFromStream(aData);
	}

	public static JSONNode LoadFromBase64(string aBase64)
	{
		return LoadFromStream(new MemoryStream(Convert.FromBase64String(aBase64))
		{
			Position = 0L
		});
	}
}
