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
public class Disintegration : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public Disintegration()
	{
		DisplayName = "Disintegration";
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
		E.Add("chance", 1);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandDisintegration");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You disintegrate nearby matter.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text += "Area: 7x7 around self\n";
		text = text + "Damage to non-structural objects: {{rules|" + Level + "d10+" + 2 * Level + "}}\n";
		text = text + "Damage to structural objects: {{rules|" + Level + "d100+20}}\n";
		text += "You are exhausted for 3 rounds after using this power\n";
		return text + "Cooldown: 75 rounds";
	}

	public static void Disintegrate(Cell C, int Radius, int Level, GameObject immunity, GameObject owner = null, GameObject source = null, bool lowPrecision = false, bool indirect = false)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		if (owner == null)
		{
			owner = immunity;
		}
		int phase = Phase.getPhase(source ?? owner);
		List<Cell> list = new List<Cell>();
		C.GetAdjacentCells(Radius, list, LocalOnly: false);
		bool flag = false;
		if (C.ParentZone != null && C.ParentZone.IsActive())
		{
			for (int i = 0; i < Radius; i++)
			{
				foreach (Cell item in list)
				{
					if (item.ParentZone == C.ParentZone && item.IsVisible())
					{
						flag = true;
						if (Radius < 3 || (item.PathDistanceTo(C) <= i - Stat.Random(0, 1) && item.PathDistanceTo(C) > i - Stat.Random(2, 3)))
						{
							scrapBuffer.Goto(item.X, item.Y);
							scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase) + (char)Stat.Random(191, 198));
						}
					}
				}
				if (flag)
				{
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(75);
				}
			}
		}
		if (list.Count > 0 && owner != null && owner.pPhysics != null)
		{
			owner.pPhysics.PlayWorldSound("disintegration", 0.5f, 0f, combat: true);
		}
		string text = Level + "d10+" + 2 * Level;
		string text2 = Level + "d100";
		bool flag2 = lowPrecision;
		if (!flag2 && owner != null)
		{
			foreach (Cell item2 in list)
			{
				if (item2.HasObject(owner.IsRegardedWithHostilityBy))
				{
					flag2 = true;
					break;
				}
			}
		}
		foreach (Cell item3 in list)
		{
			foreach (GameObject item4 in item3.GetObjectsInCell())
			{
				if (item4 != immunity && item4.PhaseMatches(phase) && item4.GetMatterPhase() <= 3 && item4.GetIntProperty("Electromagnetic") <= 0)
				{
					string dice = (item4.HasPart("Inorganic") ? text2 : text);
					int amount = dice.RollCached();
					bool accidental = flag2 && owner != null && !item4.IsHostileTowards(owner);
					item4.TakeDamage(amount, "from %t disintegration!", "Disintegration", null, null, owner, null, source, null, accidental, Environmental: false, indirect);
				}
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 3 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				Cell cell = ParentObject.CurrentCell;
				if (cell != null)
				{
					bool flag = false;
					bool flag2 = true;
					if (ParentObject.pBrain != null)
					{
						foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 4, "Combat", ParentObject))
						{
							if (item != ParentObject)
							{
								if (ParentObject.pBrain.GetFeeling(item) >= 0)
								{
									flag2 = false;
									break;
								}
								flag = true;
							}
						}
					}
					if (flag && flag2)
					{
						E.AddAICommand("CommandDisintegration");
					}
				}
			}
		}
		else if (E.ID == "CommandDisintegration")
		{
			Cell cell2 = ParentObject.GetCurrentCell();
			if (cell2 == null)
			{
				return false;
			}
			Disintegrate(cell2, 3, base.Level, ParentObject);
			CooldownMyActivatedAbility(ActivatedAbilityID, 75);
			ParentObject.ApplyEffect(new Exhausted(3));
			UseEnergy(1000);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Disintegration", "CommandDisintegration", "Mental Mutation", null, "ê");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
