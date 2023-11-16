using System;
using UnityEngine;

namespace XRL.World;

internal sealed class ColorSerializationSurrogate : IFastSerializationTypeSurrogate
{
	public object Deserialize(SerializationReader reader, Type type)
	{
		Color color = default(Color);
		color.r = reader.ReadSingle();
		color.g = reader.ReadSingle();
		color.b = reader.ReadSingle();
		color.a = reader.ReadSingle();
		return color;
	}

	public void Serialize(SerializationWriter writer, object value)
	{
		Color color = (Color)value;
		writer.Write(color.r);
		writer.Write(color.g);
		writer.Write(color.b);
		writer.Write(color.a);
	}

	public bool SupportsType(Type type)
	{
		return type == typeof(Color);
	}
}
