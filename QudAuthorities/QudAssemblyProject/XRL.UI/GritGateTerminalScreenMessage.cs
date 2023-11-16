using System;
using XRL.Core;
using XRL.Messages;

namespace XRL.UI;

public class GritGateTerminalScreenMessage : GritGateTerminalScreen
{
	private GritGateTerminalScreen lastScreen;

	private Action after;

	public GritGateTerminalScreenMessage(string message, GritGateTerminalScreen lastScreen, Action after = null)
	{
		this.lastScreen = lastScreen;
		this.after = after;
		mainText = message;
		ClearOptions();
		Options.Add("Exit.");
		optionActions.Add(delegate
		{
			MessageQueue.AddPlayerMessage("Alarms blare across the enclave.");
			XRLCore.Core.Game.FinishQuestStep("Grave Thoughts", "Investigate the Rumbling");
			base.terminal.currentScreen = null;
		});
	}

	public override void Back()
	{
		base.terminal.currentScreen = lastScreen;
		if (after != null)
		{
			after();
		}
	}

	public override void Activate()
	{
		optionActions[base.terminal.nSelected]();
	}
}
