using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AloePorta : IActivePart
{
	public string GroupKey = "AloePorta";

	public int CooldownLeft;

	public string Cooldown = "1";

	public int Chance = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs;

	public bool TriggerOnSaveSuccess = true;

	public bool DoCooldownRecolor;

	public string CooldownColorString = "&w";

	public new string ReadyColorString = "&W";

	public string CooldownTileColor = "&w";

	public string ReadyTileColor = "&W";

	public string CooldownDetailColor = "b";

	public new string ReadyDetailColor = "B";

	public AloePorta()
	{
		WorksOnCellContents = true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		Object.RegisterPartEvent(this, "ObjectEnteredCell");
		Object.RegisterPartEvent(this, "ObjectCreated");
		base.Register(Object);
	}

	public override bool SameAs(IPart p)
	{
		DischargeOnStep dischargeOnStep = p as DischargeOnStep;
		if (dischargeOnStep.Chance != Chance)
		{
			return false;
		}
		if (dischargeOnStep.SaveStat != SaveStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (dischargeOnStep.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (dischargeOnStep.SaveVs != SaveVs)
		{
			return false;
		}
		if (dischargeOnStep.CooldownLeft != CooldownLeft)
		{
			return false;
		}
		if (dischargeOnStep.TriggerOnSaveSuccess != TriggerOnSaveSuccess)
		{
			return false;
		}
		if (dischargeOnStep.Cooldown != Cooldown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void SyncColor()
	{
		if (!DoCooldownRecolor)
		{
			return;
		}
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
		if (obj.HasPart("Combat"))
		{
			return obj.FlightMatches(ParentObject);
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && E.ID == "EndTurn" && CooldownLeft > 0)
		{
			CooldownLeft--;
			if (CooldownLeft <= 0)
			{
				SyncColor();
			}
		}
		if (E.ID == "ObjectEnteredCell")
		{
			if (CooldownLeft <= 0 && GetActivePartFirstSubject(ValidStepTarget) != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && (Chance >= 100 || Stat.Random(1, 100) < Chance))
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects(ValidStepTarget))
				{
					if (activePartSubject.HasStringProperty("aloevoltatarget") || (!string.IsNullOrEmpty(SaveStat) && activePartSubject.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat) != TriggerOnSaveSuccess))
					{
						continue;
					}
					List<GameObject> objects = ParentObject.pPhysics.CurrentCell.ParentZone.GetObjects(delegate(GameObject o)
					{
						if (o != ParentObject && o.HasPart("AloePorta") && o.GetPart<AloePorta>().GroupKey == GroupKey && o.GetPart<AloePorta>().CooldownLeft <= 0)
						{
							if (o.pPhysics.CurrentCell.IsSolid())
							{
								return false;
							}
							if (o.pPhysics.CurrentCell.IsPassable(o))
							{
								return true;
							}
						}
						return false;
					});
					if (objects.Count > 0)
					{
						GameObject randomElement = objects.GetRandomElement();
						CooldownLeft = Stat.Roll(Cooldown);
						randomElement.GetPart<AloePorta>().CooldownLeft = Stat.Roll(randomElement.GetPart<AloePorta>().Cooldown);
						activePartSubject.TeleportSwirl();
						activePartSubject.TeleportTo(randomElement.pPhysics.CurrentCell, 0);
						activePartSubject.TeleportSwirl();
					}
				}
			}
		}
		else if (E.ID == "ObjectCreated")
		{
			SyncColor();
		}
		return base.FireEvent(E);
	}
}
