using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class StickyTongue : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public int TongueCharging;

	public bool PullSameCreatureType = true;

	public bool PullHostileOnly;

	public StickyTongue()
	{
		DisplayName = "Sticky Tongue";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID && ID != BeginTakeActionEvent.ID)
		{
			return ID == CommandEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (TongueCharging == 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			CommandEvent.Send(E.Actor, "CommandStickyTongue");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (TongueCharging > 0)
		{
			TongueCharging++;
			ParentObject.UseEnergy(1000, "Physical Mutation");
			if (TongueCharging >= 2)
			{
				if (HarpoonNearest(ParentObject, GetRange(), "&M", 1, PullSameCreatureType, PullHostileOnly) > 0)
				{
					CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
				}
				else
				{
					TakeMyActivatedAbilityOffCooldown(ActivatedAbilityID);
				}
				TongueCharging = 0;
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandStickyTongue")
		{
			TongueCharging = 1;
			ParentObject.UseEnergy(1000, "Physical Mutation Sticky Tongue");
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(base.Level));
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You capture prey with your sticky tongue.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You pull the nearest creature toward you.\n" + "Range: " + GetRange(Level) + "\n", "Cooldown: ", GetCooldown(Level).ToString(), " rounds");
	}

	public override bool Render(RenderEvent E)
	{
		if (TongueCharging == 1)
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 45)
			{
				E.Tile = null;
				E.RenderString = "*";
				E.ColorString = "&M";
			}
		}
		return base.Render(E);
	}

	public static int GetRange(int Level)
	{
		return 10 + Level * 2;
	}

	public int GetRange()
	{
		return GetRange(base.Level);
	}

	public static int GetCooldown(int Level)
	{
		return 22 - Level;
	}

	public int GetCooldown()
	{
		return GetCooldown(base.Level);
	}

	public static int HarpoonNearest(GameObject Actor, int Range, string harpoonColor = "&y", int Count = 1, bool PullSameCreatureType = true, bool PullHostileOnly = false)
	{
		int num = 0;
		Cell cell = Actor.CurrentCell;
		if (cell == null)
		{
			return num;
		}
		List<GameObject> objects = cell.ParentZone.GetObjects(delegate(GameObject o)
		{
			if (!o.IsCombatObject())
			{
				return false;
			}
			if (o == Actor)
			{
				return false;
			}
			int num4 = Actor.DistanceTo(o);
			if (num4 <= 1 || num4 > Range)
			{
				return false;
			}
			if (!PullSameCreatureType && o.Blueprint == Actor.Blueprint)
			{
				return false;
			}
			if (PullHostileOnly && !Actor.IsHostileTowards(o))
			{
				return false;
			}
			if (!Actor.PhaseMatches(o))
			{
				return false;
			}
			return Actor.HasLOSTo(o) ? true : false;
		});
		if (objects.Count < 1)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("There are no creatures in range.");
			}
			return num;
		}
		if (objects.Count > 1)
		{
			objects.Sort((GameObject a, GameObject b) => a.DistanceTo(Actor).CompareTo(b.DistanceTo(Actor)));
		}
		for (int i = 0; i < objects.Count && i < Count; i++)
		{
			GameObject defender = objects[i];
			Cell cell2 = defender.CurrentCell;
			List<Cell> localEmptyAdjacentCells = cell.GetLocalEmptyAdjacentCells();
			if (localEmptyAdjacentCells.Count <= 0)
			{
				break;
			}
			localEmptyAdjacentCells.Sort((Cell a, Cell b) => a.DistanceTo(defender).CompareTo(b.DistanceTo(defender)));
			List<Tuple<Cell, char>> lineTo = Actor.GetLineTo(defender);
			if (lineTo[0].Item1 != cell)
			{
				lineTo.Reverse();
			}
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			string text = harpoonColor;
			text = ((cell.X == cell2.X) ? (text + "|") : ((cell.Y == cell2.Y) ? (text + "-") : ((cell.Y < cell2.Y) ? ((cell.X <= cell2.X) ? (text + "\\") : (text + "/")) : ((cell.X <= cell2.X) ? (text + "/") : (text + "\\")))));
			int num2 = 0;
			Cell cell3 = defender.CurrentCell;
			int num3 = lineTo.Count - 2;
			while (num3 >= 1 && defender.CurrentCell == cell3)
			{
				Cell cell4 = cell.ParentZone.GetCell(lineTo[num3].Item1.X, lineTo[num3].Item1.Y);
				if (cell4 == null || !cell4.IsAdjacentTo(cell3))
				{
					break;
				}
				string directionFromCell = cell3.GetDirectionFromCell(cell4);
				if (!defender.Move(directionFromCell, Forced: true, System: false, IgnoreGravity: true, NoStack: false, Actor))
				{
					break;
				}
				cell3 = cell4;
				num2++;
				bool flag = false;
				scrapBuffer.RenderBase();
				for (int j = 1; j < num3 - 1; j++)
				{
					if (lineTo[j].Item1.IsVisible())
					{
						scrapBuffer.Goto(lineTo[j].Item1.X, lineTo[j].Item1.Y);
						scrapBuffer.Write(text);
						flag = true;
					}
				}
				if (flag)
				{
					scrapBuffer.Draw();
					Thread.Sleep(50);
				}
				num3--;
			}
			if (Actor != null && defender != null)
			{
				if (num2 == 0)
				{
					IComponent<GameObject>.XDidYToZ(Actor, "try", "to pull", defender, "toward " + Actor.them + ", but cannot", "!", null, null, Actor);
				}
				else if (Actor.DistanceTo(defender) <= 1)
				{
					IComponent<GameObject>.XDidYToZ(Actor, "pull", defender, "to " + Actor.them, "!", null, null, defender);
				}
				else
				{
					IComponent<GameObject>.XDidYToZ(Actor, "pull", defender, "toward " + Actor.them, "!", null, null, defender);
				}
			}
			defender.Gravitate();
			num++;
		}
		return num;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList" && TongueCharging == 0)
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter > 1 && intParameter <= GetRange() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandStickyTongue");
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Tongue", "CommandStickyTongue", "Physical Mutation", GetDescription(), "­");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
