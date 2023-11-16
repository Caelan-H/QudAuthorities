using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SlowDangerousMovement : IPart
{
	public string PreparedDirection;

	public bool LinkedToConsumer;

	public string PrepMessageSelf;

	public string PrepMessageOther;

	public bool bActive = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID && ID != EnteredCellEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !string.IsNullOrEmpty(PreparedDirection);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (!string.IsNullOrEmpty(PreparedDirection) && bActive && (!ParentObject.IsMobile() || !ParentObject.CanChangeMovementMode()))
		{
			PreparedDirection = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		PreparedDirection = null;
		foreach (Cell localAdjacentCell in E.Cell.GetLocalAdjacentCells())
		{
			localAdjacentCell.ClearNavigationCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(PreparedDirection) && bActive)
		{
			E.Infix.Compound(ParentObject.Itis, "\n").Append(" preparing to move ").Append(Directions.GetIndicativeDirection(PreparedDirection));
			Consumer consumer = (LinkedToConsumer ? (ParentObject.GetPart("Consumer") as Consumer) : null);
			if (consumer != null && consumer.Chance > 0)
			{
				E.Infix.Append(", ");
				if (consumer.Chance < 100)
				{
					E.Infix.Append("potentially ");
				}
				E.Infix.Append("consuming anything in ").Append(ParentObject.its).Append(" path");
			}
			E.Infix.Append('.');
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "VillageInit");
		Object.RegisterPartEvent(this, "BeginMove");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			bActive = false;
		}
		if (E.ID == "BeginMove" && bActive && string.IsNullOrEmpty(E.GetStringParameter("Type")))
		{
			return TryToMoveTo(E.GetParameter("DestinationCell") as Cell);
		}
		return base.FireEvent(E);
	}

	public bool TryToMoveTo(Cell C)
	{
		string directionFromCell = ParentObject.CurrentCell.GetDirectionFromCell(C);
		if (!Directions.IsActualDirection(directionFromCell))
		{
			return true;
		}
		if (directionFromCell == PreparedDirection)
		{
			return true;
		}
		Consumer consumer = (LinkedToConsumer ? (ParentObject.GetPart("Consumer") as Consumer) : null);
		if (consumer != null && ParentObject.Target != null && !consumer.AnythingToConsume(C))
		{
			return true;
		}
		PreparedDirection = directionFromCell;
		string text = (ParentObject.IsPlayer() ? PrepMessageSelf : PrepMessageOther);
		if (!string.IsNullOrEmpty(text) && Visible())
		{
			if (text.Contains("=dir="))
			{
				text = text.Replace("=dir=", Directions.GetExpandedDirection(directionFromCell));
			}
			if (text.Contains("=dirward="))
			{
				text = text.Replace("=dirward=", Directions.GetIndicativeDirection(directionFromCell));
			}
			IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(text, ParentObject));
		}
		ParentObject.UseEnergy(1000, "Movement");
		CauseFleeing(C, Immediate: true);
		foreach (Cell directionAndAdjacentCell in C.GetDirectionAndAdjacentCells(directionFromCell))
		{
			CauseFleeing(directionAndAdjacentCell);
		}
		if (!ParentObject.IsPlayer())
		{
			ParentObject.SetIntProperty("AIKeepMoving", 1);
		}
		return false;
	}

	public void CauseFleeing(Cell C, bool Immediate = false)
	{
		foreach (GameObject @object in C.Objects)
		{
			CauseFleeing(@object, C, Immediate);
		}
	}

	public void CauseFleeing(GameObject obj, Cell C = null, bool Immediate = false)
	{
		if (obj?.pBrain == null || !obj.IsCreature || !obj.IsMobile() || obj.Stat("Intelligence") <= 6)
		{
			return;
		}
		if (obj.IsPlayer())
		{
			if (AutoAct.IsActive())
			{
				AutoAct.Interrupt("you are " + (Immediate ? "in" : "near") + " the path of " + ParentObject.an(), null, ParentObject);
			}
			return;
		}
		Consumer consumer = (LinkedToConsumer ? (ParentObject.GetPart("Consumer") as Consumer) : null);
		if ((consumer == null || consumer.WouldConsume(obj)) && (Immediate || !obj.IsEngagedInMelee()))
		{
			obj.pBrain.PushGoal(new FleeLocation(C ?? obj.CurrentCell, 1));
		}
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (!string.IsNullOrEmpty(PreparedDirection) && bActive)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (!string.IsNullOrEmpty(PreparedDirection) && Visible() && bActive)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Cell cellFromDirection = cell.GetCellFromDirection(PreparedDirection);
				if (cellFromDirection != null && cellFromDirection.IsVisible())
				{
					buffer.Goto(cellFromDirection.X, cellFromDirection.Y);
					if (XRLCore.CurrentFrame >= 20)
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileForeground = ColorUtility.ColorMap['r'];
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].Detail = ColorUtility.ColorMap['r'];
					}
					else
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileForeground = ColorUtility.ColorMap['R'];
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].Detail = ColorUtility.ColorMap['R'];
					}
					if (XRLCore.CurrentFrame % 20 >= 10)
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileBackground = ColorUtility.ColorMap['R'];
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].SetBackground('R');
					}
					buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].SetForeground('r');
				}
			}
		}
		base.OnPaint(buffer);
	}
}
