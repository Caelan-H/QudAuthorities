using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_MenacingStare : BaseSkill
{
	public string RatingBase = "1d8";

	public int RatingOffset = 2;

	public int MaxRange = 5;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandPersuasionMenacingStare");
		base.Register(Object);
	}

	public void ApplyStare(Cell C)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if ((item != ParentObject || item.GetBodyPartCount("Head") >= 2) && item.pBrain != null)
				{
					PerformMentalAttack(Terrified.OfAttacker, ParentObject, item, null, "Terrify MenacingStare", RatingBase, 8388612, "6d4".RollCached(), int.MinValue, ParentObject.StatMod("Ego") + RatingOffset);
				}
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
		if (E.ID == "CommandPersuasionMenacingStare")
		{
			Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, null, Snap: true);
			if (cell == null)
			{
				return true;
			}
			if (cell.DistanceTo(ParentObject) > MaxRange)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("That is out of range! (" + MaxRange + " " + ((MaxRange == 1) ? "square" : "squares") + ")");
				}
				return true;
			}
			if (cell != null)
			{
				int num = 75;
				if (ParentObject.GetIntProperty("Horrifying") > 0)
				{
					num -= 10;
				}
				CooldownMyActivatedAbility(ActivatedAbilityID, num);
				ApplyStare(cell);
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Menacing Stare", "CommandPersuasionMenacingStare", "Skill", null, "ì");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
