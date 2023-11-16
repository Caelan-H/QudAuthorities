using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;

namespace XRL;

[Serializable]
public class CatacombsAnchorSystem : IGameSystem
{
	public string nextAnchorCallDuration = "300";

	public int nextAnchorCall = 300;

	public int anchorAnnouncementCall = 20;

	public static HashSet<string> catacombsAnchorZonesSet = new HashSet<string> { "JoppaWorld.53.3.0.0.11", "JoppaWorld.53.3.0.1.11", "JoppaWorld.53.3.0.2.11", "JoppaWorld.53.3.1.0.11", "JoppaWorld.53.3.1.2.11", "JoppaWorld.53.3.2.0.11", "JoppaWorld.53.3.2.1.11", "JoppaWorld.53.3.2.2.11" };

	public static List<string> catacombsAnchorZones = new List<string> { "JoppaWorld.53.3.0.0.11", "JoppaWorld.53.3.0.1.11", "JoppaWorld.53.3.0.2.11", "JoppaWorld.53.3.1.0.11", "JoppaWorld.53.3.1.2.11", "JoppaWorld.53.3.2.0.11", "JoppaWorld.53.3.2.1.11", "JoppaWorld.53.3.2.2.11" };

	public static List<string> catacombsAllowedStairsZones = new List<string> { "JoppaWorld.53.3.0.0.11", "JoppaWorld.53.3.0.1.11", "JoppaWorld.53.3.1.0.11", "JoppaWorld.53.3.1.2.11", "JoppaWorld.53.3.2.0.11", "JoppaWorld.53.3.2.1.11", "JoppaWorld.53.3.2.2.11" };

	public bool playerInCatacombs()
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
		return catacombsAnchorZonesSet.Contains(XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID);
	}

	public void anchorCall()
	{
		if (!base.player.Body.HasMarkOfDeath())
		{
			return;
		}
		string currentZoneID = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID;
		string randomElement = catacombsAnchorZones.Where((string zid) => zid != currentZoneID).GetRandomElement();
		Zone zone = XRLCore.Core.Game.ZoneManager.GetZone(randomElement);
		if (!base.player.Body.pPhysics.CurrentCell.HasObject("AnchorRoomTile"))
		{
			Cell cell = (from c in zone.GetCellsWithObject("AnchorRoomTile")
				where c.IsEmpty()
				select c).GetRandomElement();
			if (cell == null)
			{
				cell = zone.GetCellsWithObject("AnchorRoomTile").GetRandomElement().GetCellOrFirstConnectedSpawnLocation();
			}
			if (base.player.Body.HasEffect("LatchedOnto"))
			{
				foreach (Effect item in base.player.Body.Effects.Where((Effect e) => e.ClassName == "LatchedOnto").ToList())
				{
					if ((item as LatchedOnto).LatchedOnWeapon != null)
					{
						(item as LatchedOnto).LatchedOnWeapon.Equipped?.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
					}
				}
			}
			SoundManager.PlaySound("Mark of Death_F_Filtered");
			base.player.Body.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
			if (!base.player.Body.CurrentCell.HasObject("MopangoHideoutTile"))
			{
				base.player.Body.CurrentCell.GetCellOrFirstConnectedSpawnLocation().AddObject("Bone Worm").SetActive();
			}
			MessageQueue.AddPlayerMessage("You've been recalled to a resting place.", 'M');
			zone.SetActive();
			base.player.Body.TeleportSwirl();
		}
		else
		{
			MessageQueue.AddPlayerMessage("You were not recalled as you're already in a resting place.", 'M');
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
			if (playerInCatacombs())
			{
				anchorCall();
			}
			nextAnchorCall = Stat.Roll(nextAnchorCallDuration);
		}
		else if (nextAnchorCall % anchorAnnouncementCall == 0 && playerInCatacombs())
		{
			MessageQueue.AddPlayerMessage("&MThe Bell of Rest tolls! The dead will be recalled in " + nextAnchorCall + " rounds.");
		}
	}
}
