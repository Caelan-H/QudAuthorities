using XRL.World;

namespace XRL;

public interface IGamestatePostsave
{
	void OnGamestatePostsave(XRLGame game, SerializationWriter writer);
}
