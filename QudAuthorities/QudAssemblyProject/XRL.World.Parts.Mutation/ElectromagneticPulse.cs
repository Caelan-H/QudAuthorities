using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ElectromagneticPulse : BaseMutation
{
	public int nCharges = 5;

	public int nTurnCounter;

	public Guid DischargeActivatedAbilityID = Guid.Empty;

	public ElectromagneticPulse()
	{
		DisplayName = "Electromagnetic Pulse";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandElectromagneticPulse");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You generate an electromagnetic pulse that disables nearby artifacts and machines.";
	}

	public static int GetRadius(int Level)
	{
		if (Level < 5)
		{
			return 2;
		}
		if (Level < 9)
		{
			return 5;
		}
		return 9;
	}

	public override string GetLevelText(int Level)
	{
		int num = GetRadius(Level) * 2 + 1;
		string text = "Area: {{rules|" + num + "x" + num + "}} centered around yourself\n";
		text = text + "Duration: {{rules|" + (4 + Level * 2) + "-" + (13 + Level * 2) + "}} rounds\n";
		return text + "Cooldown: 200 rounds";
	}

	public static void EMP(Cell C, int Radius, int Duration, bool IncludeBaseCell = true, int Phase = 1)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		List<Cell> list = new List<Cell>();
		C.GetAdjacentCells(Radius, list, LocalOnly: false);
		foreach (Cell item in list)
		{
			if (!(item != C || IncludeBaseCell))
			{
				continue;
			}
			foreach (GameObject item2 in item.GetObjectsWithPart("Metal"))
			{
				if (item2.PhaseMatches(Phase))
				{
					item2.ForceApplyEffect(new ElectromagneticPulsed(Duration));
				}
			}
			foreach (GameObject item3 in item.GetObjectsWithPart("Combat"))
			{
				if (item3.PhaseMatches(Phase))
				{
					item3.ApplyEffect(new ElectromagneticPulsed(Duration));
				}
			}
		}
		bool flag = false;
		for (int i = 0; i < Radius; i++)
		{
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			foreach (Cell item4 in list)
			{
				if (item4.ParentZone == C.ParentZone && (Radius < 3 || (item4.PathDistanceTo(C) <= i - Stat.Random(0, 1) && item4.PathDistanceTo(C) > i - Stat.Random(2, 3))) && item4.IsVisible())
				{
					flag = true;
					scrapBuffer.Goto(item4.X, item4.Y);
					scrapBuffer.Write("&" + XRL.World.Capabilities.Phase.getRandomElectromagneticPulseColor(Phase) + (char)Stat.Random(191, 198));
				}
			}
			if (flag)
			{
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(25);
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (nCharges > 0 && IsMyActivatedAbilityAIUsable(DischargeActivatedAbilityID) && E.GetIntParameter("Distance") <= base.Level)
			{
				E.AddAICommand("CommandElectromagneticPulse");
			}
		}
		else if (E.ID == "CommandElectromagneticPulse")
		{
			if (nCharges == 0)
			{
				return false;
			}
			Cell cell = ParentObject.CurrentCell;
			if (cell == null)
			{
				return false;
			}
			UseEnergy(1000, "Physical Mutation Electromagnetic Pulse");
			EMP(cell, GetRadius(base.Level), Stat.Random(1, 10) + 5 + base.Level, IncludeBaseCell: false, ParentObject.GetPhase());
			CooldownMyActivatedAbility(DischargeActivatedAbilityID, 200);
			DidX("emit", "an electromagnetic pulse", null, null, ParentObject);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		DischargeActivatedAbilityID = AddMyActivatedAbility("Emit Pulse", "CommandElectromagneticPulse", "Physical Mutation", null, "î");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref DischargeActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
