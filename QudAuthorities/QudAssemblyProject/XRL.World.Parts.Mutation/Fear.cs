using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Fear : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Fear()
	{
		DisplayName = "Fear";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandFear");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public void ApplyFear(Cell C)
	{
		if (C == null)
		{
			return;
		}
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		string dice = (int)((double)base.Level / 2.0 + 3.0) + "d6";
		int magnitude = (int)(((double)base.Level - 1.0) / 2.0 + 1.0);
		foreach (GameObject item in C.GetObjectsInCell())
		{
			if (item.pBrain != null)
			{
				PerformMentalAttack((MentalAttackEvent E) => Terrified.Attack(E, ParentObject, null, Panicked: true, Psionic: true), ParentObject, item, null, "Terrify Fear", dice, 8388609, magnitude, int.MinValue, ParentObject.StatMod("Ego"));
			}
		}
		if (C.IsVisible())
		{
			scrapBuffer.Goto(C.X, C.Y);
			scrapBuffer.Write("&Y#");
			textConsole.DrawBuffer(scrapBuffer);
			Thread.Sleep(50);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 5 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target"), IncludeSolid: false))
			{
				E.AddAICommand("CommandFear");
			}
		}
		else if (E.ID == "CommandFear")
		{
			Cell cell = PickDestinationCell(5, AllowVis.OnlyVisible);
			if (cell == null)
			{
				return false;
			}
			if (cell.DistanceTo(ParentObject) > 5)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("That is out of range! (5 squares)");
				}
				return false;
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, 25);
			UseEnergy(1000, "Mental Mutation Fear");
			ApplyFear(cell);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Fear", "CommandFear", "Mental Mutation", null, "®");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
