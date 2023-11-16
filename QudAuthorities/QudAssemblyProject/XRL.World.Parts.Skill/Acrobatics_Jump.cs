using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Acrobatics_Jump : BaseSkill
{
	public const int DEFAULT_MAX_DISTANCE = 2;

	public int MaxDistance = 2;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetMovementMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AIGetRetreatMutationList");
		Object.RegisterPartEvent(this, "CommandAcrobaticsJump");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				int intParameter = E.GetIntParameter("Distance");
				if (intParameter > 2 && intParameter <= GetRange() + 1 && ParentObject.HasBodyPart("Feet") && FindCellToApproachTarget(ParentObject, E.GetGameObjectParameter("Target")) != null)
				{
					E.AddAICommand("CommandAcrobaticsJump");
				}
			}
		}
		else if (E.ID == "AIGetMovementMutationList" || E.ID == "AIGetRetreatMutationList")
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && ParentObject.HasBodyPart("Feet") && E.GetParameter("TargetCell") is Cell cell && cell.IsEmptyOfSolid())
			{
				int num = ParentObject.DistanceTo(cell);
				if (num > 1 && num <= GetRange() && CheckPath(ParentObject, cell, Silent: true))
				{
					E.AddAICommand("CommandAcrobaticsJump");
				}
			}
		}
		else if (E.ID == "CommandAcrobaticsJump" && Jump(ParentObject, int.MinValue, E.GetParameter("TargetCell") as Cell))
		{
			CooldownMyActivatedAbility(ActivatedAbilityID, 100);
			ParentObject.UseEnergy(1000, "Movement Jump");
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Jump", "CommandAcrobaticsJump", "Skill", null, "\u0017");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, out GameObject Over, bool Silent = false)
	{
		Over = null;
		Cell cell = Actor.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		Zone parentZone = cell.ParentZone;
		List<Point> list = Zone.Line(cell.X, cell.Y, TargetCell.X, TargetCell.Y);
		int num = 0;
		foreach (Point item in list)
		{
			Cell cell2 = parentZone.GetCell(item.X, item.Y);
			if (cell2 != cell)
			{
				int i = 0;
				for (int count = cell2.Objects.Count; i < count; i++)
				{
					GameObject gameObject = cell2.Objects[i];
					if (num == list.Count - 1)
					{
						if (gameObject.ConsiderSolidFor(Actor) || gameObject.IsCombatObject(NoBrainOnly: true))
						{
							if (!Silent && Actor.IsPlayer())
							{
								Popup.ShowFail("You can only jump into empty spaces.");
							}
							return false;
						}
					}
					else if (((gameObject.ConsiderSolidFor(Actor) && !gameObject.HasPropertyOrTag("Flyover")) || gameObject.IsCombatObject(NoBrainOnly: true)) && gameObject.PhaseAndFlightMatches(Actor))
					{
						if (!Silent && Actor.IsPlayer())
						{
							Popup.ShowFail("You can't jump over " + gameObject.the + gameObject.ShortDisplayName + ".");
						}
						return false;
					}
					if (Over == null && gameObject.IsReal)
					{
						Over = gameObject;
					}
				}
			}
			num++;
		}
		return true;
	}

	public static bool CheckPath(GameObject Actor, Cell TargetCell, bool Silent = false)
	{
		GameObject Over;
		return CheckPath(Actor, TargetCell, out Over, Silent);
	}

	public static Cell FindCellToApproachTarget(GameObject Actor, GameObject Target)
	{
		if (Target == null || Target.IsInvalid() || Target.IsInGraveyard())
		{
			return null;
		}
		Cell result = null;
		int num = int.MaxValue;
		foreach (Cell localAdjacentCell in Target.CurrentCell.GetLocalAdjacentCells())
		{
			int num2 = Actor.DistanceTo(localAdjacentCell);
			if (num2 < num && localAdjacentCell.IsEmptyOfSolid() && !localAdjacentCell.HasObjectWithPart("Combat") && CheckPath(Actor, localAdjacentCell, Silent: true))
			{
				result = localAdjacentCell;
				num = num2;
			}
		}
		return result;
	}

	public int GetRange()
	{
		return MaxDistance + ParentObject.GetIntProperty("JumpRangeModifier");
	}

	public static int GetRange(GameObject Actor)
	{
		if (Actor.GetPart("Acrobatics_Jump") is Acrobatics_Jump acrobatics_Jump)
		{
			return acrobatics_Jump.GetRange();
		}
		return 2 + Actor.GetIntProperty("JumpRangeModifier");
	}

	public static bool Jump(GameObject Actor, int Range = int.MinValue, Cell TargetCell = null, string SourceKey = null)
	{
		if (Actor.OnWorldMap())
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You cannot jump on the world map.");
			}
			return false;
		}
		if (!Actor.CheckFrozen())
		{
			return false;
		}
		if (!Actor.HasBodyPart("Feet"))
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You cannot jump without feet.");
			}
			return false;
		}
		if (Actor.IsFlying)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You cannot jump while flying.");
			}
			return false;
		}
		if (!Actor.CanChangeMovementMode("Jumping", ShowMessage: true))
		{
			return false;
		}
		if (!Actor.CanChangeBodyPosition("Jumping", ShowMessage: true))
		{
			return false;
		}
		Cell cell = Actor.CurrentCell;
		int num = ((Range == int.MinValue) ? GetRange(Actor) : Range);
		if (TargetCell == null)
		{
			TargetCell = ((!Actor.IsPlayer()) ? FindCellToApproachTarget(Actor, Actor.Target) : PickTarget.ShowPicker(PickTarget.PickStyle.Line, num, num, cell.X, cell.Y, Locked: false, AllowVis.OnlyVisible, null, null, Actor, null, "Jump where?", EnforceRange: false, UseTarget: false));
			if (TargetCell == null)
			{
				return false;
			}
		}
		if (Actor.DistanceTo(TargetCell) > num)
		{
			if (Actor.IsPlayer())
			{
				Popup.ShowFail("You may not jump more than " + Grammar.Cardinal(num) + " " + ((num == 1) ? "square" : "squares") + "!");
			}
			return false;
		}
		if (!CheckPath(Actor, TargetCell, out var Over))
		{
			return false;
		}
		Actor.MovementModeChanged("Jumping");
		Actor.BodyPositionChanged("Jumping");
		if (Over != null)
		{
			IComponent<GameObject>.XDidYToZ(Actor, "jump", "over", Over, null, "!");
		}
		else
		{
			IComponent<GameObject>.XDidY(Actor, "jump", null, "!");
		}
		if (Actor.DirectMoveTo(TargetCell, 0, forced: false, ignoreCombat: true, ignoreGravity: true))
		{
			Event @event = Event.New("Jumped");
			@event.SetParameter("Actor", Actor);
			@event.SetParameter("OriginCell", cell);
			@event.SetParameter("TargetCell", TargetCell);
			@event.SetParameter("Range", num);
			@event.SetParameter("SourceKey", SourceKey);
			Actor.FireEvent(@event);
		}
		Actor.Gravitate();
		return true;
	}
}
