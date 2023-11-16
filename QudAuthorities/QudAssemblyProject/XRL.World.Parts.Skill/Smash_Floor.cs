using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Smash_Floor : BaseSkill
{
	public int Range = 15;

	public int AdjacentPitChance = 10;

	public int DamageRadius = 1;

	public int MinimumDistance = 2;

	public int Windup = 3;

	public int Throwing;

	public string Damage = "4d6";

	public string DirectHitAdditionalDamage = "4d6";

	public Guid ActivatedAbilityID = Guid.Empty;

	public Location2D smashTarget;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandFloorSmash");
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Throwing > 0)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		int num = (int)(IComponent<GameObject>.frameTimerMS % 1000 / (1000 / DamageRadius)) + 1;
		if (ParentObject.CurrentCell == null || smashTarget == null)
		{
			return;
		}
		Cell cell = ParentObject.CurrentZone.GetCell(smashTarget);
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells(num))
		{
			if (localAdjacentCell.location.Distance(cell.location) == num)
			{
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].SetBackground('R');
				buffer.Buffer[localAdjacentCell.X, localAdjacentCell.Y].Detail = ColorUtility.ColorMap['R'];
			}
		}
		if (IComponent<GameObject>.frameTimerMS % 1000 <= 500)
		{
			buffer.Goto(smashTarget.x, smashTarget.y);
			buffer.Buffer[smashTarget.x, smashTarget.y].Tile = null;
			buffer.Write("&RX");
			buffer.Goto(ParentObject.pPhysics.CurrentCell.X, ParentObject.pPhysics.CurrentCell.Y);
			buffer.Buffer[ParentObject.pPhysics.CurrentCell.X, ParentObject.pPhysics.CurrentCell.Y].Tile = null;
			buffer.Write("&RX");
		}
	}

	public void SmashOpen(Cell cell)
	{
		if (!cell.HasObject("OpenShaft"))
		{
			Cell cellFromDirection = cell.GetCellFromDirection("D", BuiltOnly: false);
			cellFromDirection.ClearObjectsWithTag("Wall");
			if (!cellFromDirection.IsPassable(IComponent<GameObject>.ThePlayer))
			{
				cellFromDirection.Clear();
			}
			cell.AddObject("OpenShaft");
		}
	}

	public static string getSmokeParticle()
	{
		int num = Stat.RandomCosmetic(1, 3);
		string result = "";
		if (num == 1)
		{
			result = "&K";
		}
		if (num == 1)
		{
			result = "&y";
		}
		if (num == 1)
		{
			result = "&Y";
		}
		int num2 = Stat.RandomCosmetic(1, 3);
		if (num2 == 1)
		{
			result = "°";
		}
		if (num2 == 2)
		{
			result = "±";
		}
		if (num2 == 3)
		{
			result = "²";
		}
		return result;
	}

	public static void drawPuff(Cell c)
	{
		if (c != null)
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			scrapBuffer.RenderBase();
			scrapBuffer.Goto(c.X, c.Y);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Draw();
			Thread.Sleep(15);
			scrapBuffer.RenderBase();
			scrapBuffer.Goto(c.X - 1, c.Y - 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X, c.Y - 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X + 1, c.Y - 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X - 1, c.Y);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X + 1, c.Y);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X - 1, c.Y + 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X, c.Y + 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Goto(c.X + 1, c.Y + 1);
			scrapBuffer.Write(getSmokeParticle());
			scrapBuffer.Draw();
			Thread.Sleep(15);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && Throwing > 0 && ParentObject.CurrentZone != null && smashTarget != null)
		{
			Throwing--;
			if (Throwing <= 0)
			{
				Cell cell = ParentObject.CurrentZone.GetCell(smashTarget);
				List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells(DamageRadius);
				localAdjacentCells.Add(cell);
				cell.DustPuff();
				if (localAdjacentCells.Any((Cell c) => c.IsVisible()))
				{
					drawPuff(ParentObject.pPhysics.CurrentCell);
					drawPuff(cell);
				}
				foreach (Cell item in localAdjacentCells)
				{
					foreach (GameObject item2 in item.GetObjectsWithPart("Physics"))
					{
						if (item2 != ParentObject)
						{
							int num = Damage.RollCached();
							if (item == cell)
							{
								num += DirectHitAdditionalDamage.RollCached();
							}
							item2.TakeDamage(num, "from %t projectile.", null, null, null, null, ParentObject);
						}
					}
					if (item != cell && AdjacentPitChance.in100())
					{
						SmashOpen(item);
					}
				}
				SmashOpen(cell);
				smashTarget = null;
				Throwing = 0;
				if (Visible())
				{
					DidX("smash", "through the floor with a block of fulcrete", "!");
				}
			}
			else if (AutoAct.IsActive())
			{
				Cell cell2 = ParentObject.CurrentZone.GetCell(smashTarget);
				if (cell2 != null)
				{
					if (cell2.Objects.Any((GameObject o) => o.IsPlayerControlled() && o.IsVisible()))
					{
						AutoAct.Interrupt(null, null, ParentObject);
					}
					else
					{
						foreach (Cell localAdjacentCell in cell2.GetLocalAdjacentCells(DamageRadius))
						{
							if (localAdjacentCell.Objects.Any((GameObject o) => o.IsPlayerControlled() && o.IsVisible()))
							{
								AutoAct.Interrupt(null, null, ParentObject);
								break;
							}
						}
					}
				}
			}
			ParentObject.UseEnergy(1000, "Skill");
		}
		if (E.ID == "AIGetOffensiveMutationList")
		{
			int intParameter = E.GetIntParameter("Distance");
			if (intParameter <= Range && intParameter >= MinimumDistance && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasLOSTo(E.GetGameObjectParameter("Target")))
			{
				E.AddAICommand("CommandFloorSmash");
			}
		}
		else if (E.ID == "CommandFloorSmash")
		{
			if (ParentObject.CurrentCell == null)
			{
				return false;
			}
			if (ParentObject.Target != null || ParentObject.IsPlayer())
			{
				if (ParentObject.IsPlayer())
				{
					List<Cell> list = PickBurst(DamageRadius, Range, bLocked: true, AllowVis.OnlyVisible);
					if (list == null || list.Count == 0)
					{
						return false;
					}
					smashTarget = list[0].location;
				}
				else
				{
					smashTarget = ParentObject.Target.CurrentCell.location;
					if (ParentObject.Target.IsPlayer())
					{
						AutoAct.Interrupt(null, null, ParentObject);
					}
				}
				Throwing = 3;
				CooldownMyActivatedAbility(ActivatedAbilityID, 100);
				ParentObject.UseEnergy(1000, "Skill Smash Floor");
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Catapult", "CommandFloorSmash", "Skill", null, "\u001f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
