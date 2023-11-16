using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World;

[Serializable]
public class DynamicQuestRewardElement_GameObject : DynamicQuestRewardElement
{
	public string cacheID;

	public DynamicQuestRewardElement_GameObject()
	{
	}

	public DynamicQuestRewardElement_GameObject(GameObject go)
		: this()
	{
		cacheID = XRLCore.Core.Game.ZoneManager.CacheObject(go);
	}

	public override string getRewardConversationType()
	{
		return null;
	}

	public override string getRewardAcceptQuestText()
	{
		return null;
	}

	public override void award()
	{
		GameObject gameObject = XRLCore.Core.Game.ZoneManager.PullCachedObject(cacheID);
		gameObject.MakeUnderstood();
		Popup.Show("You receive " + gameObject.a + gameObject.ShortDisplayName + ".");
		The.Player.TakeObject(gameObject, Silent: true, 0);
	}
}
