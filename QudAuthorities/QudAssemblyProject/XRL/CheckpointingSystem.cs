using System;
using XRL.Core;
using XRL.UI;
using XRL.World;

namespace XRL;

[Serializable]
public class CheckpointingSystem : IGameSystem
{
	public string lastZone;

	public string lastCheckpointKey;

	public static bool ShowDeathMessage(string message)
	{
		if (!IsCheckpointingEnabled())
		{
			Popup.ShowSpace(message);
			return false;
		}
		if (Options.AllowReallydie && Popup.ShowYesNo("DEBUG: Do you really want to die?", AllowEscape: true, DialogResult.No) != 0)
		{
			The.Player.RestorePristineHealth();
			return true;
		}
		while (true)
		{
			switch (Popup.ShowOptionList("", new string[4] { "Reload from checkpoint", "View final messages", "Retire character", "Quit to main menu" }, null, 0, message + "\n"))
			{
			case 0:
				if (The.Core.Game.RestoreCheckpoint())
				{
					return true;
				}
				break;
			case 1:
				XRLCore.Core.Game.Player.Messages.Show();
				break;
			case 2:
			{
				string text = Popup.AskString("If you retire this character, your score will be recorded and your character will be lost. Are you sure you want to RETIRE THIS CHARACTER FOREVER? Type 'RETIRE' to confirm.", "", 7);
				if (text != null && text.ToUpper() == "RETIRE")
				{
					return false;
				}
				break;
			}
			case 3:
				XRLCore.Core.Game.DeathReason = "<nodeath>";
				XRLCore.Core.Game.forceNoDeath = true;
				XRLCore.Core.Game.Running = false;
				return true;
			default:
				return true;
			}
		}
	}

	public static bool IsPlayerInCheckpoint()
	{
		return (The.Game.Player?.Body?.CurrentZone?.IsCheckpoint()).GetValueOrDefault();
	}

	public static bool IsCheckpointingEnabled()
	{
		XRLGame xRLGame = The.Game;
		if (xRLGame.GetStringGameState("Checkpointing") != "Enabled")
		{
			string stringGameState = xRLGame.GetStringGameState("GameMode");
			if (stringGameState != "Wander" && stringGameState != "Roleplay")
			{
				return false;
			}
		}
		return true;
	}

	public static void DoCheckpoint()
	{
		The.Game.Checkpoint();
	}

	public static void ManualCheckpoint(Zone Z, string Key)
	{
		if (!IsCheckpointingEnabled() || !The.Game.Running)
		{
			return;
		}
		CheckpointingSystem system = The.Game.GetSystem<CheckpointingSystem>();
		if (system != null)
		{
			if (Z.ZoneID == system.lastZone || Key == system.lastCheckpointKey)
			{
				return;
			}
			system.lastZone = Z.ZoneID;
			system.lastCheckpointKey = Key;
		}
		QueueCheckpoint();
	}

	public override void ZoneActivated(Zone Z)
	{
		if (IsCheckpointingEnabled() && base.game.Running && !(Z.ZoneID == lastZone))
		{
			string checkpointKey = Z.GetCheckpointKey();
			if ((checkpointKey != null || lastCheckpointKey != null) && checkpointKey != lastCheckpointKey)
			{
				QueueCheckpoint();
			}
			lastZone = Z.ZoneID;
			lastCheckpointKey = checkpointKey;
		}
	}

	public static void QueueCheckpoint()
	{
		GameManager.Instance.gameQueue.queueTask(The.Game.Checkpoint);
	}
}
