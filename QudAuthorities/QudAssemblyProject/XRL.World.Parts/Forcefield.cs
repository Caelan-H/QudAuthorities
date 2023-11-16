using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class Forcefield : IPart
{
	public bool RejectOwner = true;

	public GameObject Creator;

	[NonSerialized]
	public List<GameObject> AllowPassage;

	public string AllowPassageTag;

	public bool Unwalkable;

	public bool MissileOpaque;

	public bool MovesWithOwner;

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		if (AllowPassage == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(1);
		Writer.WriteGameObjectList(AllowPassage);
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		if (Reader.ReadInt32() == 1)
		{
			AllowPassage = new List<GameObject>(1);
			Reader.ReadGameObjectList(AllowPassage);
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforePhysicsRejectObjectEntringCell");
		Object.ModIntProperty("Electromagnetic", 1);
		base.Register(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BlocksRadarEvent.ID && ID != GetMaximumLiquidExposureEvent.ID && ID != InterruptAutowalkEvent.ID && ID != OkayToDamageEvent.ID && ID != RealityStabilizeEvent.ID)
		{
			if (ID == TookDamageEvent.ID)
			{
				return MovesWithOwner;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(OkayToDamageEvent E)
	{
		if (MovesWithOwner && GameObject.validate(ref Creator) && ParentObject.DistanceTo(Creator) <= 1 && GameObject.validate(E.Actor) && !Creator.IsHostileTowards(E.Actor) && Creator.FireEvent(Event.New("CanBeAngeredByPropertyCrime", "Attacker", E.Actor, "Object", ParentObject)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (MovesWithOwner && E.Damage.Amount > 0 && GameObject.validate(ref Creator) && ParentObject.DistanceTo(Creator) <= 1 && GameObject.validate(E.Actor) && !Creator.IsHostileTowards(E.Actor) && Creator.FireEvent(Event.New("CanBeAngeredByPropertyCrime", "Attacker", E.Actor, "Object", ParentObject)))
		{
			Creator.GetAngryAt(E.Actor, -5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (E.Check(CanDestroy: true))
		{
			Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
			if (randomLocalAdjacentCell != null)
			{
				ParentObject.Discharge(randomLocalAdjacentCell, "1d4".RollCached(), "1d6", E.Effect.Owner);
			}
			ParentObject.TileParticleBlip("items/sw_quills.bmp", "&B", "K", 10);
			DidX("collapse", "under the pressure of normality", null, null, null, ParentObject);
			ParentObject.Destroy();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InterruptAutowalkEvent E)
	{
		GameObject.validate(ref Creator);
		if ((!MovesWithOwner || E.Actor != Creator) && !CanPass(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BlocksRadarEvent E)
	{
		return false;
	}

	public void AddAllowPassage(GameObject obj)
	{
		if (AllowPassage == null)
		{
			AllowPassage = new List<GameObject>(1) { obj };
		}
		else if (!AllowPassage.Contains(obj))
		{
			AllowPassage.Add(obj);
		}
	}

	public void RemoveAllowPassage(GameObject obj)
	{
		if (AllowPassage != null)
		{
			AllowPassage.Remove(obj);
			if (AllowPassage.Count == 0)
			{
				AllowPassage = null;
			}
		}
	}

	public bool CanPass(GameObject obj)
	{
		GameObject.validate(ref Creator);
		if (obj == null)
		{
			return false;
		}
		if (obj.HasTagOrProperty("IgnoresForceWall"))
		{
			return true;
		}
		if (obj.HasTagOrProperty("ForcefieldNullifier"))
		{
			return true;
		}
		if (MovesWithOwner && obj == Creator)
		{
			return true;
		}
		if (Unwalkable)
		{
			return false;
		}
		if (AllowPassage != null && AllowPassage.Contains(obj))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(AllowPassageTag) && obj.HasTagOrStringProperty(AllowPassageTag))
		{
			return true;
		}
		if (!RejectOwner && obj == Creator)
		{
			return true;
		}
		return false;
	}

	public bool CanMissilePassFrom(GameObject obj, GameObject projectile = null)
	{
		GameObject.validate(ref Creator);
		if (obj == null)
		{
			return false;
		}
		if (projectile != null && projectile.HasTagOrProperty("IgnoresForceWall"))
		{
			return true;
		}
		if (MissileOpaque)
		{
			return false;
		}
		if (AllowPassage != null && AllowPassage.Contains(obj))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(AllowPassageTag) && obj.HasTagOrStringProperty(AllowPassageTag))
		{
			return true;
		}
		if (!RejectOwner && obj == Creator)
		{
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforePhysicsRejectObjectEntringCell" && CanPass(E.GetGameObjectParameter("Object")))
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
