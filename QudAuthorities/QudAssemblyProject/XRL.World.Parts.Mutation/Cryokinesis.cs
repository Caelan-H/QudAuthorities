using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Cryokinesis : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Cryokinesis()
	{
		DisplayName = "Cryokinesis";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("ice", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandCryokinesis");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You chill a nearby area with your mind.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		text = "{{rules|" + Level + "d2}} divided by 2";
		text2 = "{{rules|" + Level + "d3}} divided by 2";
		text3 = "{{rules|" + Level + "d4}} divided by 2";
		string text4 = "";
		text4 = ((Level != base.Level) ? "{{rules|Increased chill temperature intensity}}\n" : "Chills affected area over 3 rounds\n");
		text4 += "Range: 8\n";
		text4 += "Area: 3x3\n";
		text4 = text4 + "Round 1 Damage: " + text + "\n";
		text4 = text4 + "Round 2 Damage: " + text2 + "\n";
		text4 = text4 + "Round 3 Damage: " + text3 + "\n";
		return text4 + "Cooldown: 50 rounds";
	}

	public void Cryo(List<Cell> Cells)
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
		foreach (Cell Cell in Cells)
		{
			GameObject gameObject = GameObject.create("Frigid Mist");
			CryoZone obj = gameObject.GetPart("CryoZone") as CryoZone;
			obj.Level = base.Level;
			obj.Owner = ParentObject;
			Cell.AddObject(gameObject);
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
	}

	public static bool Cast(Cryokinesis mutation = null, string level = "5-6")
	{
		if (mutation == null)
		{
			mutation = new Cryokinesis();
			mutation.Level = Stat.Roll(level);
			mutation.ParentObject = IComponent<GameObject>.ThePlayer;
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
		if (list != null)
		{
			mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 50);
			mutation.UseEnergy(1000, "Mental Mutation");
			mutation.Cryo(list);
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 2 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandCryokinesis");
			}
		}
		else if (E.ID == "CommandCryokinesis" && !Cast(this))
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
		ActivatedAbilityID = AddMyActivatedAbility("Chill", "CommandCryokinesis", "Mental Mutation", null, "\u000f");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
