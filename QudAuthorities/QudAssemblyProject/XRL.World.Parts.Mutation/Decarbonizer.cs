using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Decarbonizer : BaseMutation
{
	public new Guid ActivatedAbilityID;

	public int Aiming;

	public GlobalLocation TargetedFrom = new GlobalLocation();

	public GlobalLocation TargetedTo = new GlobalLocation();

	[NonSerialized]
	private List<Cell> beamCells = new List<Cell>(16);

	[NonSerialized]
	private static GameObject _Projectile;

	private static GameObject Projectile
	{
		get
		{
			if (!GameObject.validate(ref _Projectile))
			{
				_Projectile = GameObject.createUnmodified("ProjectileDecarbonizer");
			}
			return _Projectile;
		}
	}

	public Decarbonizer()
	{
		DisplayName = "Decarbonizer";
		Type = "Physical";
	}

	public bool IsTargetingActive()
	{
		if (Aiming > 0)
		{
			return beamCells.Count > 0;
		}
		return false;
	}

	public bool ValidateTargeting()
	{
		if (Aiming <= 0)
		{
			beamCells.Clear();
			return false;
		}
		if (!IsTargetingValid())
		{
			ShutDownTargeting();
			return false;
		}
		return true;
	}

	public bool ShutDownTargeting()
	{
		beamCells.Clear();
		if (!IsTargetingActive())
		{
			return false;
		}
		Aiming = 0;
		if (IComponent<GameObject>.Visible(ParentObject))
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("molecular cannon") + " goes offline.", ColorCoding.ConsequentialColor(null, ParentObject));
			PlayWorldSound("whine_down", 1f, 0f, combat: true);
		}
		return true;
	}

	public void AlertBeamCells()
	{
		if (beamCells == null)
		{
			return;
		}
		int i = 0;
		for (int count = beamCells.Count; i < count; i++)
		{
			Cell cell = beamCells[i];
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				GameObject gameObject = cell.Objects[j];
				if (gameObject.IsPlayer())
				{
					AutoAct.Interrupt("you are in the path of a decarbonizer's molecular cannon", cell);
				}
				else if (gameObject.IsPotentiallyMobile())
				{
					gameObject.pBrain.PushGoal(new FleeLocation(cell, 2));
				}
			}
		}
	}

	public bool IsTargetingValid()
	{
		if (!IsTargetingActive())
		{
			return false;
		}
		if (IsEMPed())
		{
			return false;
		}
		if (ParentObject.HasEffect("Stun"))
		{
			return false;
		}
		if (ParentObject.GetConfusion() > 0)
		{
			return false;
		}
		GlobalLocation targetedFrom = TargetedFrom;
		if (targetedFrom == null || !targetedFrom.Is(ParentObject.CurrentCell))
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CommandEvent.ID && ID != CommandTakeActionEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && ID != EnteredCellEvent.ID && ID != GeneralAmnestyEvent.ID)
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandDecarbonizer")
		{
			if (IsTargetingActive())
			{
				ShutDownTargeting();
			}
			else
			{
				PickBeamTarget();
				if (beamCells.Count <= 0)
				{
					return false;
				}
				Aiming = GetTurnsToCharge();
				ParentObject.UseEnergy(1000, "Physical Mutation Decarbonizer");
				if (!ParentObject.IsPlayer())
				{
					DidX("spin", "up " + ParentObject.its + " molecular cannon", "!", null, ParentObject);
				}
				AlertBeamCells();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (Aiming > 0)
		{
			if (!IsTargetingActive())
			{
				PickBeamTarget();
			}
			if (ValidateTargeting())
			{
				Aiming--;
				ParentObject.UseEnergy(1000, "Physical Mutation Decarbonizer");
				if (Aiming <= 0)
				{
					fireBeam(beamCells);
				}
				else
				{
					AlertBeamCells();
				}
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		ValidateTargeting();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		ValidateTargeting();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ValidateTargeting();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		ShutDownTargeting();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("might", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "StopFighting");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (Aiming <= 0 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.GetIntParameter("Distance") <= GetBeamDistance(base.Level))
			{
				E.AddAICommand("CommandDecarbonizer");
			}
		}
		else if (E.ID == "StopFighting")
		{
			ShutDownTargeting();
		}
		return base.FireEvent(E);
	}

	private bool ValidGazeTarget(GameObject obj)
	{
		return obj?.HasPart("Combat") ?? false;
	}

	public List<Cell> adjustBeamPath(List<Cell> adjPath)
	{
		List<Cell> list = Event.NewCellList();
		int num = 0;
		for (num = 0; num < adjPath.Count; num++)
		{
			if (adjPath[num].ParentZone != adjPath[0].ParentZone)
			{
				num--;
				break;
			}
		}
		if (num == adjPath.Count)
		{
			num--;
		}
		float num2 = (float)(adjPath[num].X - adjPath[0].X) / (float)adjPath[num].RealDistanceTo(adjPath[0]);
		float num3 = (float)(adjPath[num].Y - adjPath[0].Y) / (float)adjPath[num].RealDistanceTo(adjPath[0]);
		Cell cell = adjPath[adjPath.Count - 1];
		Cell cell2 = adjPath[0];
		foreach (Cell item in adjPath)
		{
			if (item.IsSolidFor(Projectile, ParentObject))
			{
				break;
			}
			cell2 = item;
			list.Add(item);
		}
		float xp = cell2.X;
		float yp = cell2.Y;
		if (num2 != 0f || num3 != 0f)
		{
			while (list.Count < GetBeamDistance(base.Level))
			{
				Cell cellFromDelta = cell.GetCellFromDelta(ref xp, ref yp, num2, num3);
				if (cellFromDelta == null)
				{
					break;
				}
				string directionFromCell = cell.GetDirectionFromCell(cellFromDelta);
				if (cellFromDelta.IsSolidFor(Projectile, ParentObject))
				{
					if (directionFromCell.Contains("N") || directionFromCell.Contains("S"))
					{
						num3 = 0f - num3;
					}
					if (directionFromCell.Contains("W") || directionFromCell.Contains("E"))
					{
						num2 = 0f - num2;
					}
				}
				cell = cellFromDelta;
				list.Add(cell);
			}
		}
		return list;
	}

	public bool PickBeamTarget()
	{
		if (Aiming <= 0)
		{
			int beamDistance = GetBeamDistance(base.Level);
			List<Cell> list = PickLine(beamDistance, AllowVis.OnlyVisible, ValidGazeTarget);
			if (list == null || list.Count <= 0)
			{
				return false;
			}
			beamCells.Clear();
			beamCells.AddRange(list);
			beamCells.Insert(0, ParentObject.CurrentCell);
			if (beamCells.Count > 1)
			{
				beamDistance++;
				bool flag = false;
				while (!flag && beamCells.Count < beamDistance)
				{
					for (int i = 0; i < beamCells.Count - 1; i++)
					{
						if (beamCells.Count >= beamDistance)
						{
							break;
						}
						string directionFromCell = beamCells[i].GetDirectionFromCell(beamCells[i + 1]);
						Cell cellFromDirection = beamCells[beamCells.Count - 1].GetCellFromDirection(directionFromCell);
						if (cellFromDirection == null)
						{
							flag = true;
							break;
						}
						beamCells.Add(cellFromDirection);
					}
				}
			}
			for (int j = 0; j < beamCells.Count; j++)
			{
				if (beamCells[j].IsSolidFor(Projectile, ParentObject))
				{
					beamCells.RemoveRange(j, beamCells.Count - j);
					break;
				}
			}
			if (beamCells.Count <= 0)
			{
				return false;
			}
			PlayWorldSound("ominous_powerup", 1f, 0f, combat: true);
			List<Cell> collection = adjustBeamPath(beamCells);
			beamCells.Clear();
			beamCells.AddRange(collection);
			TargetedFrom.SetCell(ParentObject.CurrentCell);
			TargetedTo.SetCell(beamCells[beamCells.Count - 1]);
		}
		return true;
	}

	public string pathDirectionAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return ".";
		}
		n %= path.Count;
		return path[n].GetDirectionFromCell(path[(n + 1) % path.Count]);
	}

	public Cell cellAtStep(int n, List<Cell> path)
	{
		if (path.Count <= 1)
		{
			return path[0];
		}
		if (n < path.Count)
		{
			return path[n];
		}
		Cell cell = path[path.Count - 1];
		int num = n % path.Count;
		n %= path.Count;
		for (int i = 0; i < n; i++)
		{
			cell = cell.GetCellFromDirection(pathDirectionAtStep(num, path), BuiltOnly: false);
			num++;
		}
		return cell;
	}

	public int GetHowManyDismemberedLimbs()
	{
		return Stat.Random(1, 4);
	}

	public void fireBeam(List<Cell> beamPath, bool bDoEffect = true)
	{
		Aiming = 0;
		if (!beamPath.Any((Cell c) => c.IsVisible()))
		{
			bDoEffect = false;
		}
		int num = 0;
		for (num = 0; num < beamPath.Count; num++)
		{
			if (beamPath[num].ParentZone != beamPath[0].ParentZone)
			{
				num--;
				break;
			}
		}
		if (num == beamPath.Count)
		{
			num--;
		}
		float num2 = (float)(beamPath[num].X - beamPath[0].X) / (float)beamPath[num].RealDistanceTo(beamPath[0]);
		float num3 = (float)(beamPath[num].Y - beamPath[0].Y) / (float)beamPath[num].RealDistanceTo(beamPath[0]);
		Cell cell = beamPath[beamPath.Count - 1];
		Cell cell2 = beamPath[0];
		foreach (Cell item in beamPath)
		{
			if (item.IsSolidFor(Projectile, ParentObject))
			{
				break;
			}
			cell2 = item;
		}
		float xp = cell2.X;
		float yp = cell2.Y;
		if (num2 != 0f || num3 != 0f)
		{
			while (beamPath.Count < GetBeamDistance(base.Level))
			{
				Cell cellFromDelta = cell.GetCellFromDelta(ref xp, ref yp, num2, num3);
				if (cellFromDelta == null)
				{
					break;
				}
				string directionFromCell = cell.GetDirectionFromCell(cellFromDelta);
				if (cellFromDelta.IsSolidFor(Projectile, ParentObject))
				{
					if (directionFromCell.Contains("N") || directionFromCell.Contains("S"))
					{
						num3 = 0f - num3;
					}
					if (directionFromCell.Contains("W") || directionFromCell.Contains("E"))
					{
						num2 = 0f - num2;
					}
				}
				cell = cellFromDelta;
				beamPath.Add(cell);
			}
		}
		PlayWorldSound("burn_blast", 1f, 0f, combat: true);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		for (int i = 0; i < beamPath.Count; i++)
		{
			foreach (GameObject item2 in beamPath[i].GetObjectsWithPart("Body"))
			{
				if (item2 == ParentObject || item2.MakeSave("Agility", 20, ParentObject, "Agility", "Decarbonizer Beam"))
				{
					continue;
				}
				int howManyDismemberedLimbs = GetHowManyDismemberedLimbs();
				for (int j = 0; j < howManyDismemberedLimbs; j++)
				{
					Axe_Dismember.Dismember(ParentObject, item2, null, null, null, null, assumeDecapitate: true);
					if (!ParentObject.IsValid() || ParentObject.IsNowhere())
					{
						break;
					}
				}
			}
			if (beamPath[i] != null && beamPath[i].ParentZone == beamPath[0].ParentZone && beamPath[i].IsVisible())
			{
				scrapBuffer.RenderBase();
				if (i > 0)
				{
					scrapBuffer.Goto(beamPath[i - 1].X, beamPath[i - 1].Y);
					scrapBuffer.Write("&b*");
				}
				if (i > 1)
				{
					scrapBuffer.Goto(beamPath[i - 2].X, beamPath[i - 2].Y);
					scrapBuffer.Write("&K*");
				}
				scrapBuffer.Goto(beamPath[i].X, beamPath[i].Y);
				scrapBuffer.Write("&B*");
				scrapBuffer.Draw();
				Thread.Sleep(10);
			}
		}
		beamCells.Clear();
		CooldownMyActivatedAbility(ActivatedAbilityID, 200);
		ParentObject.UseEnergy(1000, "Physical Mutation");
	}

	public int GetTurnsToCharge()
	{
		return 3;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		E.WantsToPaint = true;
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (IsTargetingActive() && ParentObject.CurrentCell != null)
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
		if (IsTargetingActive() && ParentObject.CurrentCell != null)
		{
			int num = 500;
			int num2 = num / beamCells.Count;
			int num3 = (int)(IComponent<GameObject>.frameTimerMS % num / num2);
			for (int i = 0; i < beamCells.Count; i++)
			{
				if (i > 0 && i < beamCells.Count && beamCells[i].ParentZone == ParentObject.pPhysics.CurrentCell.ParentZone && beamCells[i].IsVisible())
				{
					buffer.Goto(beamCells[i].X, beamCells[i].Y);
					if (i == num3)
					{
						buffer.Buffer[beamCells[i].X, beamCells[i].Y].TileForeground = ColorUtility.ColorMap['R'];
						buffer.Buffer[beamCells[i].X, beamCells[i].Y].Detail = ColorUtility.ColorMap['R'];
					}
					else
					{
						buffer.Buffer[beamCells[i].X, beamCells[i].Y].TileForeground = ColorUtility.ColorMap['r'];
						buffer.Buffer[beamCells[i].X, beamCells[i].Y].Detail = ColorUtility.ColorMap['r'];
					}
					buffer.Buffer[beamCells[i].X, beamCells[i].Y].SetForeground('r');
					if (AutoAct.IsActive())
					{
						AutoAct.Interrupt("you see a decarbonizer targeting beam", beamCells[i]);
					}
				}
			}
		}
		base.OnPaint(buffer);
	}

	public override string GetDescription()
	{
		return "You extract carbon from materials.";
	}

	public int GetBeamDistance(int Level)
	{
		return 40 + Level;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Decarbonize", "CommandDecarbonizer", "Physical Mutation", null, "ê");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
