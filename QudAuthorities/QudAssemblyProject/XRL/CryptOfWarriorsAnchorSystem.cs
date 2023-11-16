using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;

namespace XRL;

[Serializable]
public class CryptOfWarriorsAnchorSystem : IGameSystem
{
	public string nextAnchorCallDuration = "300";

	public int nextAnchorCall = 300;

	public int anchorAnnouncementCall = 20;

	public static HashSet<string> warriorsAnchorZoneSet = new HashSet<string> { "JoppaWorld.53.3.0.0.8", "JoppaWorld.53.3.0.1.8", "JoppaWorld.53.3.0.2.8", "JoppaWorld.53.3.1.0.8", "JoppaWorld.53.3.1.2.8", "JoppaWorld.53.3.2.0.8", "JoppaWorld.53.3.2.1.8", "JoppaWorld.53.3.2.2.8" };

	public static List<string> warriorsAnchorZones = new List<string> { "JoppaWorld.53.3.0.0.8", "JoppaWorld.53.3.0.1.8", "JoppaWorld.53.3.0.2.8", "JoppaWorld.53.3.1.0.8", "JoppaWorld.53.3.1.2.8", "JoppaWorld.53.3.2.0.8", "JoppaWorld.53.3.2.1.8", "JoppaWorld.53.3.2.2.8" };

	public static List<string> warriorsPullZones = new List<string> { "JoppaWorld.53.3.0.0.8", "JoppaWorld.53.3.0.1.8", "JoppaWorld.53.3.0.2.8", "JoppaWorld.53.3.1.0.8", "JoppaWorld.53.3.1.2.8", "JoppaWorld.53.3.2.0.8", "JoppaWorld.53.3.2.1.8", "JoppaWorld.53.3.2.2.8" };

	public static List<string> warriorsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.8", "JoppaWorld.53.3.0.1.8", "JoppaWorld.53.3.0.2.8", "JoppaWorld.53.3.1.0.8", "JoppaWorld.53.3.1.2.8", "JoppaWorld.53.3.2.0.8", "JoppaWorld.53.3.2.1.8", "JoppaWorld.53.3.2.2.8" };

	public bool playerInCryptOfWarriors()
	{
		if (XRLCore.Core == null)
		{
			return false;
		}
		if (XRLCore.Core.Game == null)
		{
			return false;
		}
		if (XRLCore.Core.Game.Player == null)
		{
			return false;
		}
		if (XRLCore.Core.Game.Player.Body == null)
		{
			return false;
		}
		if (XRLCore.Core.Game.Player.Body.pPhysics == null)
		{
			return false;
		}
		if (XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell == null)
		{
			return false;
		}
		return warriorsPullZones.Contains(XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID);
	}

	public void anchorCall()
	{
		if (base.player.Body.HasMarkOfDeath())
		{
			string currentZoneID = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID;
			string randomElement = warriorsAnchorZones.Where((string zid) => zid != currentZoneID).GetRandomElement();
			CryptOfPriestsAnchorSystem.cryptRecall(XRLCore.Core.Game.ZoneManager.GetZone(randomElement));
		}
	}

	public override void EndTurn()
	{
		if (The.Game.HasIntGameState("BellOfRestDestroyed"))
		{
			return;
		}
		nextAnchorCall--;
		if (nextAnchorCall <= 0)
		{
			if (playerInCryptOfWarriors())
			{
				anchorCall();
			}
			nextAnchorCall = Stat.Roll(nextAnchorCallDuration);
		}
		else if (nextAnchorCall % anchorAnnouncementCall == 0 && playerInCryptOfWarriors())
		{
			MessageQueue.AddPlayerMessage("&MThe Bell of Rest tolls! The dead will be recalled in " + nextAnchorCall + " rounds.");
		}
	}
}
