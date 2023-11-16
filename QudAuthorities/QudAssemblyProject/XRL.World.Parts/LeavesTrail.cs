using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class LeavesTrail : IPart
{
	public string TrailBlueprint = "SmallSlimePuddle";

	public string TrailPopTable;

	public int TrailChance = 100;

	public int TemporaryChance;

	public string TemporaryDuration = "2d3";

	public string TemporaryTurnInto;

	public int TemporaryTurnIntoChance = 100;

	public bool PassAttitudes;

	public bool VillageDeactivate = true;

	public bool Active = true;

	public bool OnEnter = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != EnteredCellEvent.ID || !OnEnter || !Active))
		{
			if (ID == LeftCellEvent.ID && !OnEnter)
			{
				return Active;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (OnEnter && Active)
		{
			LeaveTrailIn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (!OnEnter && Active)
		{
			LeaveTrailIn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit" && VillageDeactivate)
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public GameObject LeaveTrailIn(Cell C)
	{
		if (C == null || C.OnWorldMap())
		{
			return null;
		}
		if (string.IsNullOrEmpty(TrailBlueprint) && string.IsNullOrEmpty(TrailPopTable))
		{
			return null;
		}
		if (!TrailChance.in100())
		{
			return null;
		}
		GameObject gameObject = (string.IsNullOrEmpty(TrailPopTable) ? GameObject.create(TrailBlueprint) : GameObject.create(PopulationManager.RollOneFrom(TrailPopTable, new Dictionary<string, string> { 
		{
			"zonetier",
			C.ParentZone.NewTier.ToString()
		} }).Blueprint));
		if (gameObject != null)
		{
			if (TemporaryChance.in100())
			{
				Temporary p = ((string.IsNullOrEmpty(TemporaryTurnInto) || !TemporaryTurnIntoChance.in100()) ? new Temporary(TemporaryDuration.RollCached()) : new Temporary(TemporaryDuration.RollCached(), TemporaryTurnInto));
				gameObject.AddPart(p);
			}
			if (PassAttitudes)
			{
				gameObject.TakeOnAttitudesOf(ParentObject);
			}
			C.AddObject(gameObject);
		}
		return gameObject;
	}
}
