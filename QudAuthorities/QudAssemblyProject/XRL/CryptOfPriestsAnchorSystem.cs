using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL;

[Serializable]
public class CryptOfPriestsAnchorSystem : IGameSystem
{
	public string nextAnchorCallDuration = "300";

	public int nextAnchorCall = 300;

	public int anchorAnnouncementCall = 20;

	public static HashSet<string> priestsAnchorZoneSet = new HashSet<string> { "JoppaWorld.53.3.0.0.7", "JoppaWorld.53.3.0.1.7", "JoppaWorld.53.3.0.2.7", "JoppaWorld.53.3.1.0.7", "JoppaWorld.53.3.1.2.7", "JoppaWorld.53.3.2.0.7", "JoppaWorld.53.3.2.1.7", "JoppaWorld.53.3.2.2.7" };

	public static List<string> priestsAnchorZones = new List<string> { "JoppaWorld.53.3.0.0.7", "JoppaWorld.53.3.0.1.7", "JoppaWorld.53.3.0.2.7", "JoppaWorld.53.3.1.0.7", "JoppaWorld.53.3.1.2.7", "JoppaWorld.53.3.2.0.7", "JoppaWorld.53.3.2.1.7", "JoppaWorld.53.3.2.2.7" };

	public static List<string> priestsPullZones = new List<string> { "JoppaWorld.53.3.0.0.7", "JoppaWorld.53.3.0.1.7", "JoppaWorld.53.3.0.2.7", "JoppaWorld.53.3.1.0.7", "JoppaWorld.53.3.1.2.7", "JoppaWorld.53.3.2.0.7", "JoppaWorld.53.3.2.1.7", "JoppaWorld.53.3.2.2.7" };

	public static List<string> priestsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.7", "JoppaWorld.53.3.0.1.7", "JoppaWorld.53.3.1.0.7", "JoppaWorld.53.3.1.2.7", "JoppaWorld.53.3.2.0.7", "JoppaWorld.53.3.2.1.7", "JoppaWorld.53.3.2.2.7" };

	public bool playerInCryptOfPriests()
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
		return priestsPullZones.Contains(XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID);
	}

	public static void cryptRecall(Zone targetZone)
	{
		GamePlayer gamePlayer = XRLCore.Core.Game.Player;
		if (!gamePlayer.Body.pPhysics.CurrentCell.HasObject("AnchorRoomTile"))
		{
			Cell cell = targetZone.GetCellsWithObject("AnchorRoomTile").GetRandomElement();
			if (cell == null)
			{
				cell = targetZone.GetCellsWithObject("AnchorRoomTile").GetRandomElement().GetCellOrFirstConnectedSpawnLocation();
			}
			if (cell != null && cell.HasObjectWithPart("StairsDown"))
			{
				cell = cell.GetConnectedSpawnLocation();
			}
			List<GameObject> list = new List<GameObject>();
			if (gamePlayer.Body.HasEffect("LatchedOnto"))
			{
				foreach (Effect item in gamePlayer.Body.Effects.Where((Effect e) => e.ClassName == "LatchedOnto").ToList())
				{
					if ((item as LatchedOnto).LatchedOnWeapon != null)
					{
						GameObject equipped = (item as LatchedOnto).LatchedOnWeapon.Equipped;
						if (equipped != null)
						{
							equipped.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
							list.Add(equipped);
						}
					}
					gamePlayer.Body.RemoveEffect(item);
				}
			}
			SoundManager.PlaySound("Mark of Death_F_Filtered");
			gamePlayer.Body.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
			GameObject gameObject = cell.GetObjectsWithPart("Enclosing").FirstOrDefault();
			if (gameObject != null)
			{
				gameObject.GetPart<Enclosing>().EnterEnclosure(gamePlayer.Body);
				foreach (GameObject item2 in list)
				{
					gameObject.GetPart<Enclosing>().EnterEnclosure(item2);
					if (item2.HasPart("CryptFerretBehavior"))
					{
						item2.GetPart<CryptFerretBehavior>().behaviorState = "looting";
						item2.pBrain.Goals.Clear();
					}
				}
			}
			MessageQueue.AddPlayerMessage("&MYou've been recalled to a resting place.");
			gamePlayer.Body.TeleportSwirl();
		}
		else
		{
			MessageQueue.AddPlayerMessage("&MYou were not recalled as you're already in a resting place.");
		}
	}

	public void anchorCall()
	{
		if (base.player.Body.HasMarkOfDeath())
		{
			string currentZoneID = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID;
			string randomElement = priestsAnchorZones.Where((string zid) => zid != currentZoneID).GetRandomElement();
			cryptRecall(XRLCore.Core.Game.ZoneManager.GetZone(randomElement));
		}
	}

	public override void EndTurn()
	{
		nextAnchorCall--;
		if (nextAnchorCall <= 0)
		{
			if (playerInCryptOfPriests())
			{
				anchorCall();
			}
			nextAnchorCall = Stat.Roll(nextAnchorCallDuration);
		}
		else if (nextAnchorCall % anchorAnnouncementCall == 0 && playerInCryptOfPriests())
		{
			MessageQueue.AddPlayerMessage("&MThe Bell of Rest tolls! The dead will be recalled in " + nextAnchorCall + " rounds.");
		}
	}
}
