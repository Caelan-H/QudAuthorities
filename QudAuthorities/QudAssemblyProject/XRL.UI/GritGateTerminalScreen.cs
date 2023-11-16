using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

public class GritGateTerminalScreen : TerminalScreen
{
	public List<Action> optionActions = new List<Action>();

	private ElectricalPowerTransmission generator;

	public int available;

	protected override void ClearOptions()
	{
		base.ClearOptions();
		optionActions.Clear();
	}

	public override void BeforeRender(ScreenBuffer buffer)
	{
		int num = 3;
		buffer.Goto(num + 3, 24);
		if (generator == null)
		{
			GameObject gameObject = base.terminal.terminal.CurrentCell.ParentZone.FindObject((GameObject o) => o.Blueprint == "GritGateFusionPowerStation" && o.pPhysics.CurrentCell.Y >= 10);
			if (gameObject != null)
			{
				generator = gameObject.GetPart<ElectricalPowerTransmission>();
			}
		}
		if (generator == null)
		{
			available = 0;
			buffer.Write("[{{R|!!! ERROR: POWER SYSTEMS HAVE FAILED !!!}}]");
			return;
		}
		int num2 = 4000;
		int totalDraw = generator.GetTotalDraw();
		available = num2 - totalDraw;
		if (available < 0)
		{
			buffer.Write("[{{W|!!! WARNING: INSUFFICIENT POWER !!!}}]");
		}
		else
		{
			buffer.Write(" Available power: " + (float)available * 0.1f + " amps ");
		}
		GritGateAmperageImposter.display = " Available power: " + (float)available * 0.1f + " amps ";
	}
}
