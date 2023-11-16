using System;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

namespace XRL;

[Serializable]
public class WanderSystem : IGameSystem
{
	public static int WXU
	{
		get
		{
			int level = The.Player.Level;
			int num = Leveler.GetXPForLevel(level + 1) - Leveler.GetXPForLevel(level);
			num = ((level <= 2) ? (num / 2) : ((level <= 5) ? (num / 3) : ((level <= 10) ? (num / 5) : ((level <= 20) ? (num / 6) : ((level > 30) ? (num / 10) : (num / 8))))));
			num = (int)Math.Round((float)num / 10f) * 10;
			return Math.Max(10, num);
		}
	}

	public static bool WanderEnabled()
	{
		XRLGame xRLGame = XRLCore.Core.Game;
		if (!(xRLGame.GetStringGameState("GameMode") == "Wander"))
		{
			return xRLGame.GetStringGameState("WanderEnabled") == "yes";
		}
		return true;
	}

	public static int AwardWXU(int n)
	{
		if (WanderEnabled())
		{
			int num = WXU * n;
			The.Player.AwardXP(num);
			return num;
		}
		return 0;
	}

	public override void PlayerEmbarking()
	{
		if (!WanderEnabled())
		{
			return;
		}
		foreach (Faction item in Factions.loop())
		{
			if (The.Game.PlayerReputation.get(item) < 0 && item.Name != "Playerhater")
			{
				The.Game.PlayerReputation.set(item, 0);
			}
		}
	}

	public override void QuestCompleted(Quest q)
	{
		if (WanderEnabled())
		{
			AwardWXU(2);
		}
	}

	public override void LocationDiscovered(string locationName)
	{
		if (WanderEnabled())
		{
			AwardWXU(1);
		}
	}

	public override void WaterRitualPerformed(GameObject go)
	{
		if (WanderEnabled() && go != null && !go.HasStringProperty("WaterRitualWXU"))
		{
			go.SetStringProperty("WaterRitualWXU", "yes");
			AwardWXU(1);
		}
	}

	public override bool AwardingXP(ref GameObject Actor, ref int Amount, ref int Tier, ref int Minimum, ref int Maximum, ref GameObject Kill, ref GameObject InfluencedBy, ref GameObject PassedUpFrom, ref GameObject PassedDownFrom, ref string Deed)
	{
		if (WanderEnabled() && Actor != null && Actor.IsPlayer() && Kill != null)
		{
			Amount = 0;
			return false;
		}
		return base.AwardingXP(ref Actor, ref Amount, ref Tier, ref Minimum, ref Maximum, ref Kill, ref InfluencedBy, ref PassedUpFrom, ref PassedDownFrom, ref Deed);
	}
}
