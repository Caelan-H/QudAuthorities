using System;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Berate : BaseSkill
{
	public const int VOCAL_RANGE = 8;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandPersuasionBerate");
		base.Register(Object);
	}

	public void ApplyBerate(Cell C, bool IsMissingTongue)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		string text = "1d8";
		int num = ParentObject.StatMod("Ego");
		if (num > 0)
		{
			text = text + "+" + num;
		}
		else if (num < 0)
		{
			text += num;
		}
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (!item.HasPart("Brain"))
				{
					continue;
				}
				if (IsMissingTongue && !ParentObject.CanMakeTelepathicContactWith(item))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("{{r|Without a tongue, you cannot berate " + item.t() + ".}}");
					}
					continue;
				}
				if (!ParentObject.CheckFrozen(Telepathic: true, Telekinetic: false, Silent: true, item))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("{{r|Frozen solid, you cannot berate " + item.t() + ".}}");
					}
					continue;
				}
				if (ParentObject.DistanceTo(item) > 8 && !ParentObject.CanMakeTelepathicContactWith(item))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("{{r|You cannot make telepathic contact with " + item.t() + ", and " + item.itis + " out of normal range (" + 8 + " squares).}}");
					}
					continue;
				}
				int duration = "6d6".RollCached();
				if (CanApplyEffectEvent.Check(item, "Shamed", duration) && item.ApplyEffect(new Shamed(duration)))
				{
					if (item.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You are shamed!", 'R');
					}
					else if (ParentObject.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("Your words shame " + item.t() + ".", 'G');
					}
				}
				else if (item.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You hold your head high!", 'g');
				}
				else if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(item.T() + item.GetVerb("won't") + " be shamed by your words.", 'R');
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
		if (E.ID == "CommandPersuasionBerate")
		{
			bool flag = ParentObject.IsMissingTongue();
			if (flag && !ParentObject.HasPart("Telepathy"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowBlock("You cannot berate without a tongue.");
				}
				return true;
			}
			if (!ParentObject.CheckFrozen(Telepathic: true))
			{
				return true;
			}
			Cell cell = PickDestinationCell(80, AllowVis.OnlyVisible, Locked: true, IgnoreSolid: false, IgnoreLOS: true, RequireCombat: true, PickTarget.PickStyle.EmptyCell, null, Snap: true);
			if (cell == null)
			{
				return true;
			}
			if (ParentObject.DistanceTo(cell) > 8 && !ParentObject.HasPart("Telepathy"))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.Show("That is out of range! (" + 8 + " squares)");
				}
				return true;
			}
			if (cell != null)
			{
				int intProperty = ParentObject.GetIntProperty("Horrifying");
				int turns = Math.Max(50 - intProperty * 10, 10);
				CooldownMyActivatedAbility(ActivatedAbilityID, turns);
				ApplyBerate(cell, flag);
				UseEnergy(1000, "Skill Persuasion Berate");
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Berate", "CommandPersuasionBerate", "Skill", null, "\u0014");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
