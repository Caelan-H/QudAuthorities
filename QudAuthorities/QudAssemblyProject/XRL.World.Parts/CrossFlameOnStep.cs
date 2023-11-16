using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CrossFlameOnStep : IActivePart
{
	public int CooldownLeft;

	public string Cooldown = "1d6";

	public string DamageTriggerTypes = "Heat;Fire";

	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public int Level = 4;

	public string Length = "4";

	public string CooldownColorString = "&r";

	public new string ReadyColorString = "&R";

	public string CooldownTileColor = "&w";

	public string ReadyTileColor = "&W";

	public string CooldownDetailColor = "r";

	public new string ReadyDetailColor = "R";

	public static ScreenBuffer Buffer;

	public CrossFlameOnStep()
	{
		WorksOnCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		CrossFlameOnStep crossFlameOnStep = p as CrossFlameOnStep;
		if (crossFlameOnStep.Chance != Chance)
		{
			return false;
		}
		if (crossFlameOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (crossFlameOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (crossFlameOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (crossFlameOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (crossFlameOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (crossFlameOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (crossFlameOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		if (crossFlameOnStep.Level != Level)
		{
			return false;
		}
		if (crossFlameOnStep.Length != Length)
		{
			return false;
		}
		if (crossFlameOnStep.CooldownColorString != CooldownColorString)
		{
			return false;
		}
		if (crossFlameOnStep.ReadyColorString != ReadyColorString)
		{
			return false;
		}
		if (crossFlameOnStep.CooldownTileColor != CooldownTileColor)
		{
			return false;
		}
		if (crossFlameOnStep.ReadyTileColor != ReadyTileColor)
		{
			return false;
		}
		if (crossFlameOnStep.CooldownDetailColor != CooldownDetailColor)
		{
			return false;
		}
		if (crossFlameOnStep.ReadyDetailColor != ReadyDetailColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (CooldownLeft > 0)
		{
			CooldownLeft--;
			if (CooldownLeft <= 0)
			{
				SyncColor();
			}
		}
		CheckActivate();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetNavigationWeightEvent.ID && ID != InterruptAutowalkEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		GameObject actor = E.Actor;
		if (actor == null || actor.PhaseAndFlightMatches(ParentObject))
		{
			E.MinWeight(98);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		return false;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		SyncColor();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		CheckActivate();
		return base.HandleEvent(E);
	}

	public void CheckActivate()
	{
		if (CooldownLeft > 0 || GetActivePartFirstSubject(ValidStepTarget) == null || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) || !Chance.in100())
		{
			return;
		}
		foreach (GameObject activePartSubject in GetActivePartSubjects(ValidStepTarget))
		{
			if (string.IsNullOrEmpty(SaveStat) || activePartSubject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) == TriggerOnSaveSuccess)
			{
				Discharge();
				break;
			}
		}
	}

	public void SyncColor()
	{
		if (CooldownLeft > 0)
		{
			if (!string.IsNullOrEmpty(CooldownColorString))
			{
				ParentObject.pRender.ColorString = CooldownColorString;
			}
			if (!string.IsNullOrEmpty(CooldownTileColor))
			{
				ParentObject.pRender.TileColor = CooldownTileColor;
			}
			if (!string.IsNullOrEmpty(CooldownDetailColor))
			{
				ParentObject.pRender.DetailColor = CooldownDetailColor;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(ReadyColorString))
			{
				ParentObject.pRender.ColorString = ReadyColorString;
			}
			if (!string.IsNullOrEmpty(ReadyTileColor))
			{
				ParentObject.pRender.TileColor = ReadyTileColor;
			}
			if (!string.IsNullOrEmpty(ReadyDetailColor))
			{
				ParentObject.pRender.DetailColor = ReadyDetailColor;
			}
		}
	}

	public bool ValidStepTarget(GameObject obj)
	{
		if (obj.IsCombatObject())
		{
			return ParentObject.PhaseAndFlightMatches(obj);
		}
		return false;
	}

	public void Discharge()
	{
		if (ParentObject.CurrentCell == null || CooldownLeft > 0)
		{
			return;
		}
		CooldownLeft = Cooldown.RollCached();
		string dice = Level + "d4+1";
		List<Cell> list = new List<Cell>();
		int num = Length.RollCached();
		string[] cardinalDirectionList = Directions.CardinalDirectionList;
		foreach (string direction in cardinalDirectionList)
		{
			Cell localCellFromDirection = ParentObject.pPhysics.CurrentCell.GetLocalCellFromDirection(direction);
			for (int j = 0; j < num; j++)
			{
				if (localCellFromDirection == null)
				{
					break;
				}
				list.Add(localCellFromDirection);
				localCellFromDirection = localCellFromDirection.GetLocalCellFromDirection(direction);
			}
		}
		bool flag = false;
		if (Buffer == null)
		{
			flag = true;
			Buffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
			XRLCore.Core.RenderMapToBuffer(Buffer);
		}
		bool flag2 = false;
		foreach (Cell item in list)
		{
			if (item != null)
			{
				foreach (GameObject item2 in item.GetObjectsInCell())
				{
					if (item2.PhaseMatches(ParentObject))
					{
						item2.TemperatureChange(310 + 25 * Level, ParentObject);
						int num2 = Stat.Random(1, 3);
						if (num2 == 1)
						{
							item2.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 0f, 1);
						}
						if (num2 == 2)
						{
							item2.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 0f, 1);
						}
						if (num2 == 3)
						{
							item2.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 0f, 1);
						}
					}
				}
				foreach (GameObject item3 in item.GetObjectsWithPartReadonly("Combat"))
				{
					if (item3.PhaseMatches(ParentObject) && !item3.HasPart("CrossFlameOnStep"))
					{
						Damage damage = new Damage(dice.RollCached());
						damage.AddAttribute("Fire");
						damage.AddAttribute("Heat");
						Event @event = Event.New("TakeDamage");
						@event.AddParameter("Damage", damage);
						@event.AddParameter("Owner", ParentObject);
						@event.AddParameter("Attacker", ParentObject);
						@event.AddParameter("Message", "from %o flames!");
						item3.FireEvent(@event);
					}
				}
			}
			if (item.IsVisible() && Buffer != null)
			{
				flag2 = true;
				Buffer.Goto(item.X, item.Y);
				string text = "&C";
				int num3 = Stat.Random(1, 3);
				if (num3 == 1)
				{
					text = "&R";
				}
				if (num3 == 2)
				{
					text = "&r";
				}
				if (num3 == 3)
				{
					text = "&W";
				}
				int num4 = Stat.Random(1, 3);
				if (num4 == 1)
				{
					text += "^R";
				}
				if (num4 == 2)
				{
					text += "^r";
				}
				if (num4 == 3)
				{
					text += "^W";
				}
				if (item.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
				{
					Stat.Random(1, 3);
					Buffer.Write(text + (char)(219 + Stat.Random(0, 4)));
					Popup._TextConsole.DrawBuffer(Buffer);
				}
			}
		}
		if (Buffer != null && flag2)
		{
			Thread.Sleep(10);
		}
		list.ForEach(delegate(Cell c)
		{
			c.GetObjectsWithPartReadonly("CrossFlameOnStep").ForEach(delegate(GameObject o)
			{
				o.GetPart<CrossFlameOnStep>().Discharge();
			});
		});
		ConsumeChargeIfOperational();
		SyncColor();
		if (flag)
		{
			Buffer = null;
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeTookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTookDamage" && !string.IsNullOrEmpty(DamageTriggerTypes) && ParentObject.CurrentCell != null && E.GetParameter("Damage") is Damage damage && damage.Attributes.Any((string s) => DamageTriggerTypes.Contains(s)))
		{
			Discharge();
		}
		return base.FireEvent(E);
	}
}
