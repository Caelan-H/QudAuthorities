using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class RunOver : IPart
{
	public string Damage = "40-60";

	public string BreakSolidAV = "30";

	public int MaxTargetDistance = 6;

	public int KnockdownSaveTarget = 35;

	public string KnockdownSaveStat = "Strength";

	public string KnockdownSaveVs = "RunOver Knockdown";

	public int DazeSaveTarget = 35;

	public string DazeSaveStat = "Toughness";

	public string DazeSaveVs = "RunOver Daze";

	public int Charging;

	public Guid ActivatedAbilityID;

	[NonSerialized]
	private List<Cell> chargeCells;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Run Over", "CommandRunOver", "Maneuvers", null, "\u00af");
		base.Initialize();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Charging > 0)
		{
			if (chargeCells == null || chargeCells.Count == 0)
			{
				Charging = 0;
			}
			else
			{
				Charging--;
				ParentObject.UseEnergy(1000, "Physical Ability RunOver");
				if (Charging > 0)
				{
					return false;
				}
				performCharge(chargeCells);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		chargeCells = null;
		Charging = 0;
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandRunOver");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRunOver")
		{
			if (ParentObject.GetCurrentCell() == null || ParentObject.GetCurrentCell().OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't do that here.");
				}
				return false;
			}
			PickChargeTarget();
			if (chargeCells == null || chargeCells.Count <= 0)
			{
				return false;
			}
			Charging = GetTurnsToCharge();
			ParentObject.UseEnergy(1000, "Physical Ability RunOver");
			DidX("stare", null, null, null, ParentObject);
			if (Visible() && AutoAct.IsInterruptable())
			{
				AutoAct.Interrupt(null, null, ParentObject);
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList" && Charging <= 0 && E.GetIntParameter("Distance") <= MaxTargetDistance && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			GameObject obj = E.GetGameObjectParameter("Target");
			if (GameObject.validate(ref obj) && ParentObject.PhaseMatches(obj) && ParentObject.FlightCanReach(obj) && ParentObject.HasLOSTo(ParentObject.Target))
			{
				E.AddAICommand("CommandRunOver", 10);
			}
		}
		return base.FireEvent(E);
	}

	private bool ValidChargeTarget(GameObject obj)
	{
		return obj?.IsCombatObject(NoBrainOnly: true) ?? false;
	}

	public void PickChargeTarget()
	{
		if (Charging > 0)
		{
			return;
		}
		chargeCells = PickLine(MaxTargetDistance, AllowVis.OnlyVisible, ValidChargeTarget, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, ParentObject);
		if (chargeCells == null)
		{
			return;
		}
		chargeCells = new List<Cell>(chargeCells);
		chargeCells.Insert(0, ParentObject.CurrentCell);
		if (chargeCells.Count <= 1 || chargeCells[0].ParentZone != chargeCells[1].ParentZone)
		{
			return;
		}
		while (chargeCells.Count <= MaxTargetDistance)
		{
			for (int i = 0; i < chargeCells.Count - 1; i++)
			{
				if (chargeCells.Count > MaxTargetDistance)
				{
					break;
				}
				if (chargeCells[i].ParentZone != chargeCells[i + 1].ParentZone)
				{
					break;
				}
				string directionFromCell = chargeCells[i].GetDirectionFromCell(chargeCells[i + 1]);
				chargeCells.Add(chargeCells[chargeCells.Count - 1].GetCellFromDirection(directionFromCell, BuiltOnly: false));
			}
		}
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
		for (int i = 0; i <= n; i++)
		{
			cell = cell.GetCellFromDirection(pathDirectionAtStep(num, path), BuiltOnly: false);
			num++;
		}
		return cell;
	}

	public void performCharge(List<Cell> chargePath, bool bDoEffect = true)
	{
		Charging = 0;
		DidX("charge", null, "!");
		int i = 1;
		for (int num = chargePath.Count; i < num; i++)
		{
			Cell cell = cellAtStep(i, chargePath);
			if (cell == null)
			{
				break;
			}
			foreach (GameObject item in Event.NewGameObjectList(cell.Objects))
			{
				if (item == ParentObject)
				{
					continue;
				}
				bool num2 = item.IsCombatObject(NoBrainOnly: true);
				if (num2)
				{
					num = Math.Max(num, i + 2);
				}
				if (num2 && ParentObject.PhaseMatches(item) && ParentObject.FlightCanReach(item))
				{
					DidXToY("run", "over", item, null, null, null, ParentObject);
					if (!item.MakeSave(KnockdownSaveStat, KnockdownSaveTarget, null, null, KnockdownSaveVs))
					{
						item.ApplyEffect(new Prone());
					}
					if (!item.MakeSave(DazeSaveStat, DazeSaveTarget, null, null, DazeSaveVs))
					{
						item.ApplyEffect(new Dazed());
					}
					int num3 = Damage.RollCached();
					if (num3 > 0)
					{
						item.TakeDamage(num3, "being run over by %O.", null, null, null, null, ParentObject);
					}
				}
				else if (item.ConsiderSolidFor(ParentObject))
				{
					if (BreakSolidAV.RollCached() < Stats.GetCombatAV(item))
					{
						DidXToY("are", "stopped in " + ParentObject.its + " tracks by", item, null, "!");
						break;
					}
					if (IComponent<GameObject>.Visible(item))
					{
						CombatJuice._cameraShake(0.25f);
						item.DustPuff();
					}
					int j = 0;
					for (int count = item.Count; j < count; j++)
					{
						item.Destroy();
					}
				}
			}
			ParentObject.DirectMoveTo(cell, 0, forced: false, ignoreCombat: true, ignoreGravity: true);
			if (cell.IsVisible())
			{
				The.Core.RenderDelay(10, Interruptible: false);
			}
		}
		if (ParentObject.ShouldShunt())
		{
			Cell cell2 = ParentObject.CurrentCell?.GetFirstEmptyAdjacentCell();
			if (cell2 != null)
			{
				ParentObject.DirectMoveTo(cell2, 0, forced: false, ignoreCombat: true, ignoreGravity: true);
			}
		}
		ParentObject.Gravitate();
	}

	public int GetTurnsToCharge()
	{
		return 1;
	}

	public override bool Render(RenderEvent E)
	{
		if (chargeCells != null && chargeCells.Count > 0 && ParentObject.CurrentCell != null)
		{
			int num = 500;
			if (IComponent<GameObject>.frameTimerMS % num < num / 2)
			{
				E.ColorString = "&r^R";
			}
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (chargeCells != null)
		{
			int num = 1000;
			int val = num / Math.Max(chargeCells.Count, 1);
			int num2 = (int)(IComponent<GameObject>.frameTimerMS % num / Math.Max(val, 1));
			if (num2 > 0 && num2 < chargeCells.Count && chargeCells[num2].ParentZone == ParentObject.CurrentZone && chargeCells[num2].IsVisible())
			{
				buffer.Goto(chargeCells[num2].X, chargeCells[num2].Y);
				buffer.Write(ParentObject.pRender.RenderString);
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Tile = ParentObject.pRender.Tile;
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].TileForeground = ColorUtility.ColorMap['r'];
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].Detail = ColorUtility.ColorMap['R'];
				buffer.Buffer[chargeCells[num2].X, chargeCells[num2].Y].SetForeground('r');
			}
			base.OnPaint(buffer);
		}
	}
}
