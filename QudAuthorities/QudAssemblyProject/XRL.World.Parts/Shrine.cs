using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Shrine : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetPointsOfInterestEvent.ID && ID != IdleQueryEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			string baseDisplayName = ParentObject.BaseDisplayName;
			string key = "Shrine " + baseDisplayName;
			bool flag = true;
			PointOfInterest pointOfInterest = E.Find(key);
			if (pointOfInterest != null)
			{
				if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
				{
					E.Remove(pointOfInterest);
				}
				else
				{
					flag = false;
					pointOfInterest.Explanation = "nearest";
				}
			}
			if (flag)
			{
				E.Add(ParentObject, baseDisplayName, null, key, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Pray", "pray", "Pray", null, 'y', FireOnActor: false, -10);
		if (!ParentObject.HasPart("ModDesecrated"))
		{
			E.AddAction("Desecrate", "desecrate", "Desecrate", null, 'D', FireOnActor: false, -10);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Pray")
		{
			if (PrayAtShrine(E.Actor, FromDialog: true))
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "Desecrate" && DesecrateShrine(E.Actor, FromDialog: true))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (E.Actor.HasPart("Brain") && Stat.Random(1, 10000) == 1)
		{
			GameObject who = E.Actor;
			if (who.pBrain.GetPrimaryFaction() == ParentObject.pPhysics.Owner)
			{
				who.pBrain.PushGoal(new DelegateGoal(delegate(GoalHandler h)
				{
					if (who.isAdjacentTo(who))
					{
						PrayAtShrine(who, FromDialog: false);
					}
					h.FailToParent();
				}));
				who.pBrain.PushGoal(new MoveTo(ParentObject));
			}
			else if (ParentObject.pPhysics.Owner != null && Factions.GetFeelingFactionToObject(ParentObject.pPhysics.Owner, who) < 0)
			{
				who.pBrain.PushGoal(new DelegateGoal(delegate(GoalHandler h)
				{
					if (who.isAdjacentTo(who))
					{
						DesecrateShrine(who, FromDialog: false);
					}
					h.FailToParent();
				}));
				who.pBrain.PushGoal(new MoveTo(ParentObject));
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage" && E.GetParameter("Attacker") is GameObject gameObject && gameObject != ParentObject)
		{
			ParentObject.pPhysics.BroadcastForHelp(gameObject);
			if (ParentObject.isDamaged(0.75) && !ParentObject.HasPart("ModDesecrated"))
			{
				PerformDesecration(gameObject, gameObject.IsPlayer());
			}
		}
		return base.FireEvent(E);
	}

	public bool PrayAtShrine(GameObject who, bool FromDialog)
	{
		IComponent<GameObject>.XDidYToZ(who, "voice", "a short prayer beneath", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog);
		who.UseEnergy(1000, "Item");
		who.FireEvent(Event.New("Prayed", "Object", ParentObject));
		return true;
	}

	public void PerformDesecration(GameObject who, bool FromDialog)
	{
		IComponent<GameObject>.XDidYToZ(who, "desecrate", ParentObject, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog);
		ParentObject.AddPart(new ModDesecrated());
		ParentObject.pPhysics.BroadcastForHelp(who);
		who.FireEvent(Event.New("Desecrated", "Object", ParentObject));
	}

	public bool DesecrateShrine(GameObject who, bool FromDialog)
	{
		if (who == null)
		{
			return false;
		}
		if (who.IsPlayer())
		{
			int freeDrams = who.GetFreeDrams("blood");
			int freeDrams2 = who.GetFreeDrams("putrid");
			if (freeDrams <= 0 && freeDrams2 <= 0)
			{
				Popup.Show("You need a dram of either {{r|blood}} or {{putrid|putrescence}} to desecrate a shrine.");
				return false;
			}
			List<string> list = new List<string>(2);
			List<string> list2 = new List<string>(2);
			if (freeDrams > 0)
			{
				list.Add("blood");
				list2.Add("blood");
			}
			if (freeDrams2 > 0)
			{
				list.Add("putrescence");
				list2.Add("putrid");
			}
			if (list.Count == 0)
			{
				Popup.Show("You need a dram of either {{r|blood}} or {{putrid|putrescence}} to desecrate a shrine.");
				Debug.LogError("internal inconsistency");
				return true;
			}
			int num = Popup.ShowOptionList("Choose a desecration liquid", list.ToArray(), null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			who.UseDrams(1, list2[num]);
		}
		PerformDesecration(who, FromDialog);
		who.UseEnergy(1000, "Item");
		return true;
	}
}
