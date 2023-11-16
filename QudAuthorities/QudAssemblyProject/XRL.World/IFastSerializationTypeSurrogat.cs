using System;

namespace XRL.World;

/// <summary>
///             Interface to allow helper classes to be used to serialize objects
///             that are not directly supported by SerializationWriter/SerializationReader
///             </summary>
public interface IFastSerializationTypeSurrogate
{
	bool SupportsType(Type type);

	/// <summary>
	///             FastSerializes the object into the SerializationWriter.
	///             </summary><param name="writer">The SerializationWriter into which the object is to be serialized.</param><param name="value">The object to serialize.</param>
	void Serialize(SerializationWriter writer, object value);

	object Deserialize(SerializationReader reader, Type type);
}
