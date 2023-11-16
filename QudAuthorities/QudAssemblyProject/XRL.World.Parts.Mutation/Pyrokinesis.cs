using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Pyrokinesis : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Pyrokinesis()
	{
		DisplayName = "Pyrokinesis";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandPyrokinesis");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You heat a nearby area with your mind.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		text = "{{rules|" + Level + "d3}} divided by 2";
		text2 = "{{rules|" + Level + "d4}} divided by 2";
		text3 = "{{rules|" + Level + "d6}} divided by 2";
		string text4 = "";
		text4 = ((Level != base.Level) ? "{{rules|Increased toast temperature intensity}}\n" : "Toasts affected area over 3 rounds\n");
		text4 += "Range: 8\n";
		text4 += "Area: 3x3\n";
		text4 = text4 + "Round 1 Damage: " + text + "\n";
		text4 = text4 + "Round 2 Damage: " + text2 + "\n";
		text4 = text4 + "Round 3 Damage: " + text3 + "\n";
		return text4 + "Cooldown: 50 rounds";
	}

	public static List<GameObject> Pyro(GameObject Actor, int Level, List<Cell> Cells, int Duration = 3)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		List<GameObject> list = new List<GameObject>(Cells.Count);
		foreach (Cell Cell in Cells)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject("Shimmering Heat");
			if (gameObject.GetPart("PyroZone") is PyroZone pyroZone)
			{
				pyroZone.Level = Level;
				pyroZone.Owner = Actor;
				pyroZone.Duration = Duration;
			}
			list.Add(Cell.AddObject(gameObject));
			if (Cell.IsVisible())
			{
				scrapBuffer.Goto(Cell.X, Cell.Y);
				if (Stat.Random(1, 3) == 1)
				{
					scrapBuffer.Write("&Y*");
				}
				else if (Stat.Random(1, 3) == 1)
				{
					scrapBuffer.Write("&W*");
				}
				else
				{
					scrapBuffer.Write("&R*");
				}
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(25);
			}
		}
		return list;
	}

	public static bool Cast(Pyrokinesis mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new Pyrokinesis();
			mutation.Level = Stat.Roll(level);
			mutation.ParentObject = XRLCore.Core.Game.Player.Body;
		}
		List<Cell> list = mutation.PickBurst(1, 8, bLocked: false, AllowVis.OnlyVisible);
		if (list == null)
		{
			return true;
		}
		foreach (Cell item in list)
		{
			if (item.DistanceTo(mutation.ParentObject) > 9)
			{
				if (mutation.ParentObject.IsPlayer())
				{
					Popup.Show("That is out of range! (8 squares)");
				}
				return true;
			}
		}
		Pyro(mutation.ParentObject, mutation.Level, list);
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 50);
		mutation.UseEnergy(1000, "Mental Mutation");
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 8 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandPyrokinesis");
			}
		}
		else if (E.ID == "CommandPyrokinesis" && !Cast(this))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Toast", "CommandPyrokinesis", "Mental Mutation", null, "*");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
