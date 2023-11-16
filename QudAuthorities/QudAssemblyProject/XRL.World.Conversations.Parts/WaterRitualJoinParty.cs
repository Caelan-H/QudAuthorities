using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualJoinParty : IWaterRitualPart
{
	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override void Awake()
	{
		if (WaterRitual.RecordFaction.WaterRitualJoin && !The.Speaker.IsPlayerLed() && !(The.Speaker.GetxTag("WaterRitual", "NoJoin") == "true"))
		{
			Reputation = Math.Max(50, 200 + (The.Speaker.Stat("Level") - The.Player.Stat("Level")) * 12);
			Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Join", Reputation);
			Visible = true;
		}
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			Brain pBrain = The.Speaker.pBrain;
			pBrain.BecomeCompanionOf(The.Player);
			if (pBrain.GetFeeling(The.Player) < 0)
			{
				pBrain.SetFeeling(The.Player, 5);
			}
			if (The.Speaker.GetEffect("Lovesick") is Lovesick lovesick)
			{
				lovesick.PreviousLeader = The.Player;
			}
			Popup.Show(The.Speaker.Does("join", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you!");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|-" + Reputation + "}} reputation]}}";
		return false;
	}
}
