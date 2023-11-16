using System;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class KindrishProperties : LowStatBooster
{
	public static bool ReturnAward()
	{
		XRLCore.Core.Game.PlayerReputation.modify("Hindren", 400);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject("Force Bracelet", 0, 1);
		gameObject.MakeUnderstood();
		Popup.Show("You receive " + gameObject.a + gameObject.DisplayName + ".");
		IComponent<GameObject>.ThePlayer.TakeObject(gameObject, Silent: true, 0);
		return true;
	}

	public KindrishProperties()
	{
		base.AffectedStats = "Strength,Agility,Toughness,Intelligence,Willpower,Ego";
		base.Amount = 3;
		DescribeStatusForProperty = null;
	}
}
