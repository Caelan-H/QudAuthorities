namespace XRL.World;

public interface IOwnedDataSerializable
{
	/// <summary>
	///             Lets the implementing class store internal data directly into a SerializationWriter.
	///             </summary><param name="writer">The SerializationWriter to use</param><param name="context">Optional context to use as a hint as to what to store (BitVector32 is useful)</param>
	void SerializeOwnedData(SerializationWriter writer, object context);

	void DeserializeOwnedData(SerializationReader reader, object context);
}
