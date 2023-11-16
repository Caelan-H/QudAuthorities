using XRL.World;

namespace XRL;

public interface IObjectGamestateCustomSerializer
{
	IGamestateSingleton GameLoad(SerializationReader reader);

	void GameSave(SerializationWriter writer);
}
