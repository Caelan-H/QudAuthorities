using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Consumer : IPart
{
	public int Chance = 100;

	public int WeightThresholdPercentage = 90;

	public bool SuppressCorpseDrops = true;

	public bool bActive = true;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeginMoveLate");
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMoveLate" && bActive && string.IsNullOrEmpty(E.GetStringParameter("Type")))
		{
			return TryToConsumeObjectsIn(E.GetParameter("DestinationCell") as Cell);
		}
		if (E.ID == "VillageInit")
		{
			bActive = false;
		}
		return base.FireEvent(E);
	}

	public bool TryToConsumeObjectsIn(Cell C)
	{
		if (C == null || C.OnWorldMap())
		{
			return true;
		}
		if (!Chance.in100())
		{
			return true;
		}
		Cell cell = ParentObject.CurrentCell;
		List<GameObject> list = Event.NewGameObjectList();
		int consumeWeightCapacity = GetConsumeWeightCapacity();
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (!ShouldIgnore(gameObject))
			{
				if (!CanConsume(gameObject, consumeWeightCapacity))
				{
					list.Clear();
					ParentObject.SetIntProperty("AIKeepMoving", 1);
					ParentObject.FireEvent(Event.New("CommandAttackCell", "Cell", C));
					return false;
				}
				list.Add(gameObject);
			}
		}
		foreach (GameObject item in list)
		{
			Consume(item);
		}
		if (ParentObject.CurrentCell != cell)
		{
			return false;
		}
		return true;
	}

	public void Consume(GameObject obj)
	{
		DidXToY("consume", obj, null, "!");
		BeingConsumedEvent.Send(ParentObject, obj);
		bool flag = false;
		if (SuppressCorpseDrops)
		{
			obj.ModIntProperty("SuppressCorpseDrops", 1);
			flag = true;
		}
		try
		{
			if (obj.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_SWALLOWED_WHOLE");
			}
			if (obj.Count > 1)
			{
				obj.Obliterate("You were consumed whole by " + ParentObject.a + ParentObject.ShortDisplayName + ".", Silent: false, obj.It + obj.GetVerb("were") + " @@consumed whole by " + ParentObject.a + ParentObject.ShortDisplayName + ".");
			}
			else
			{
				obj.Die(ParentObject, null, "You were consumed whole by " + ParentObject.a + ParentObject.ShortDisplayName + ".", obj.It + obj.GetVerb("were") + " @@consumed whole by " + ParentObject.a + ParentObject.ShortDisplayName + ".");
			}
		}
		finally
		{
			if (flag && GameObject.validate(ref obj) && !obj.IsNowhere())
			{
				obj.ModIntProperty("SuppressCorpseDrops", -1, RemoveIfZero: true);
			}
		}
	}

	public static bool CanConsume(GameObject obj, int WeightThreshold)
	{
		if (obj.Weight > 1000)
		{
			Debug.LogError("CHECKING " + obj.DebugName + " " + obj.Weight + " AGAINST " + WeightThreshold);
		}
		return obj.Weight < WeightThreshold;
	}

	public bool CanConsume(GameObject obj)
	{
		return CanConsume(obj, GetConsumeWeightCapacity());
	}

	public int GetConsumeWeightCapacity()
	{
		return ParentObject.Weight * WeightThresholdPercentage / 100;
	}

	public bool ShouldIgnore(GameObject obj)
	{
		if (!obj.IsReal)
		{
			return true;
		}
		if (obj.IsScenery)
		{
			return true;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return true;
		}
		if (obj.GetMatterPhase() >= 3)
		{
			return true;
		}
		if (obj.HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return true;
		}
		if (!ParentObject.PhaseAndFlightMatches(obj))
		{
			return true;
		}
		return false;
	}

	public bool WouldConsume(GameObject obj)
	{
		if (!ShouldIgnore(obj))
		{
			return CanConsume(obj);
		}
		return false;
	}

	public bool AnythingToConsume(Cell C)
	{
		foreach (GameObject @object in C.Objects)
		{
			if (WouldConsume(@object))
			{
				return true;
			}
		}
		return false;
	}
}
