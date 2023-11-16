using XRL.World;

namespace XRL;

public interface IGamestatePostload
{
	void OnGamestatePostload(XRLGame game, SerializationReader reader);
}
