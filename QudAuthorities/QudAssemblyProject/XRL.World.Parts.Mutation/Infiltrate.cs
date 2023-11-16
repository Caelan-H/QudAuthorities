using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Infiltrate : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public int Infiltrating;

	[NonSerialized]
	private Cell teleportDestination;

	public Infiltrate()
	{
		DisplayName = "Infiltrate";
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandInfiltrate");
		base.Register(Object);
	}

	private bool ValidGazeTarget(GameObject obj)
	{
		return obj?.HasPart("Combat") ?? false;
	}

	public void PickInfiltrateDestination()
	{
		if (Infiltrating <= 0)
		{
			if (!IsPlayer() && ParentObject.CurrentCell != null)
			{
				int num = int.MinValue;
				GameObject gameObject = null;
				foreach (GameObject item in ParentObject.CurrentCell.ParentZone.LoopObjectsWithPart("Combat"))
				{
					if (ParentObject.pBrain.GetFeeling(item) <= 0 && ParentObject.DistanceTo(item) > num)
					{
						num = ParentObject.DistanceTo(item);
						gameObject = item;
					}
				}
				if (gameObject != null)
				{
					teleportDestination = gameObject.pPhysics.CurrentCell.getClosestPassableCell();
				}
			}
			else
			{
				teleportDestination = PickDestinationCell(GetTeleportDistance(base.Level), AllowVis.Any, Locked: false);
			}
			if (teleportDestination == null)
			{
				return;
			}
			if (!teleportDestination.IsPassable())
			{
				teleportDestination = teleportDestination.GetFirstEmptyAdjacentCell(1, 80);
			}
		}
		if (teleportDestination == null)
		{
			return;
		}
		Event.NewGameObjectList().Add(ParentObject);
		foreach (Cell localAdjacentCell in ParentObject.pPhysics.CurrentCell.GetLocalAdjacentCells(GetTeleportRadius(base.Level) + GetTurnsToCharge()))
		{
			foreach (GameObject item2 in localAdjacentCell.LoopObjectsWithPart("Combat"))
			{
				if (item2 != null && item2.IsMemberOfFaction("Templar"))
				{
					item2.pBrain.PushGoal(new MoveTo(ParentObject));
					break;
				}
			}
		}
	}

	public void performInfiltrate(Cell teleportDestination, bool bDoEffect = true)
	{
		if (ParentObject.pPhysics.CurrentCell != null)
		{
			try
			{
				ParentObject.DilationSplat();
				Point2D point2D = teleportDestination.Pos2D - ParentObject.pPhysics.CurrentCell.Pos2D;
				List<GameObject> list = Event.NewGameObjectList();
				list.Add(ParentObject);
				foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(GetTeleportRadius(base.Level)))
				{
					foreach (GameObject item in localAdjacentCell.LoopObjectsWithPart("Combat"))
					{
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
				foreach (GameObject item2 in list)
				{
					Cell cell = item2.pPhysics.CurrentCell;
					if (cell != null && cell.location != null)
					{
						Point2D p = cell.Pos2D + point2D;
						if (p.x < 0)
						{
							p.x = 0;
						}
						if (p.y > 79)
						{
							p.y = 79;
						}
						if (p.x < 0)
						{
							p.x = 0;
						}
						if (p.y > 24)
						{
							p.y = 24;
						}
						Cell cell2 = cell.ParentZone.GetCell(p);
						if (cell2 != null)
						{
							if (!cell2.IsPassable())
							{
								cell2 = cell2.GetFirstEmptyAdjacentCell(1, 80);
							}
							if (cell2 != null)
							{
								item2.TeleportTo(cell2, 0);
							}
						}
					}
					ParentObject.DilationSplat();
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Infiltrator teleport", x);
			}
		}
		Infiltrating = 0;
		teleportDestination = null;
		CooldownMyActivatedAbility(ActivatedAbilityID, 100);
		ParentObject.UseEnergy(1000, "Physical Mutation");
	}

	public int GetTurnsToCharge()
	{
		return 3;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (teleportDestination != null && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				buffer.Goto(teleportDestination.X, teleportDestination.Y);
				buffer.Write("&RX");
				buffer.Buffer[teleportDestination.X, teleportDestination.Y].TileForeground = ColorUtility.ColorMap['r'];
				buffer.Buffer[teleportDestination.X, teleportDestination.Y].Detail = ColorUtility.ColorMap['r'];
			}
		}
		base.OnPaint(buffer);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Infiltrating > 0)
			{
				if (teleportDestination == null)
				{
					PickInfiltrateDestination();
				}
				if (teleportDestination == null)
				{
					Infiltrating = 0;
				}
				else
				{
					Infiltrating--;
					ParentObject.UseEnergy(1000, "Physical Mutation");
					if (Infiltrating > 0)
					{
						return false;
					}
					performInfiltrate(teleportDestination);
					teleportDestination = null;
				}
			}
		}
		else if (E.ID == "CommandInfiltrate")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't infiltrate on the world map.");
				}
				return false;
			}
			PickInfiltrateDestination();
			if (teleportDestination == null)
			{
				return false;
			}
			Infiltrating = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Mutation Infiltrate");
			DidX("focus", ParentObject.its + " baleful gaze", null, null, ParentObject);
		}
		else if (E.ID == "AIGetOffensiveMutationList" && Infiltrating <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && !ParentObject.OnWorldMap() && E.GetIntParameter("Distance") <= GetTeleportDistance(base.Level))
		{
			E.AddAICommand("CommandInfiltrate");
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "You teleport and bring things along with you.";
	}

	public int GetTeleportDistance(int Level)
	{
		return 80;
	}

	public int GetTeleportRadius(int Level)
	{
		return 3 + Level;
	}

	public override string GetLevelText(int Level)
	{
		return "You can teleport any distance and bring everything within " + GetTeleportRadius(Level) + " squares along with you.";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Infiltrate", "CommandInfiltrate", "Physical Mutation", null, "\u001d");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
