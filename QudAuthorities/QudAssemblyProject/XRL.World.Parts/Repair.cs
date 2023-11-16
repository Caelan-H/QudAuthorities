using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class Repair : IPart, IRepairSifrahHandler
{
	public const string OWNER_ANGER_SUPPRESS = "DontWarnOnRepair";

	public string FixedBlueprint;

	public int Difficulty;

	public override bool SameAs(IPart p)
	{
		Repair repair = p as Repair;
		if (repair.FixedBlueprint != FixedBlueprint)
		{
			return false;
		}
		if (repair.Difficulty != Difficulty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != IsRepairableEvent.ID)
		{
			return ID == RepairedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Repair", "repair", "Repair", null, 'r', FireOnActor: false, 20);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Repair")
		{
			if (E.Actor.GetTotalConfusion() > 0)
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You could not possibly attempt that in your confusion.");
				}
			}
			else if (!E.Actor.FlightMatches(ParentObject))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You cannot reach " + ParentObject.t() + " to repair " + ParentObject.them + ".");
				}
			}
			else if (!E.Actor.PhaseMatches(ParentObject))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail(ParentObject.Does("are") + " insubstantial; you cannot repair " + ParentObject.them + ".");
				}
			}
			else if (E.Actor.AreHostilesNearby() && E.Actor.FireEvent("CombatPreventsRepair"))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You can't perform repairs with hostiles nearby!");
				}
			}
			else if (Options.SifrahRepair)
			{
				if (E.Actor.IsPlayer())
				{
					if (!string.IsNullOrEmpty(ParentObject.Owner) && !ParentObject.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(ParentObject.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: true) + " not owned by you, and trying to repair " + ParentObject.them + " risks damaging " + ParentObject.them + ". Are you sure you want to do so?") != 0)
					{
						return false;
					}
					GameObject inInventory = ParentObject.InInventory;
					if (inInventory != null && inInventory != E.Actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnRepair") && Popup.ShowYesNoCancel(inInventory.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: true) + " not owned by you, and trying to repair " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " inside " + inInventory.them + " risks causing damage. Are you sure you want to do so?") != 0)
					{
						return false;
					}
				}
				RepairSifrah repairSifrah = new RepairSifrah(ParentObject, ParentObject.GetComplexity(), Difficulty, E.Actor.Stat("Intelligence"));
				repairSifrah.HandlerID = ParentObject.id;
				repairSifrah.HandlerPartName = base.Name;
				repairSifrah.Play(ParentObject);
				if (repairSifrah.InterfaceExitRequested)
				{
					E.RequestInterfaceExit();
				}
			}
			else if (Difficulty <= 0 || E.Actor.MakeSave("Intelligence", Difficulty, null, null, "Tinkering Repair", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
			{
				RepairResultSuccess(E.Actor, ParentObject);
			}
			else
			{
				RepairResultFailure(E.Actor, ParentObject);
			}
			E.Actor.UseEnergy(1000, "Item Repair");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		ParentObject.ReplaceWith(GameObject.createUnmodified(FixedBlueprint));
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void RepairResultSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.ShowBlock("You repair " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
		RepairedEvent.Send(who, obj);
	}

	public void RepairResultExceptionalSuccess(GameObject who, GameObject obj)
	{
		RepairResultSuccess(who, obj);
		string randomBits = BitType.GetRandomBits("3d6".Roll(), obj.GetComplexity());
		if (!string.IsNullOrEmpty(randomBits))
		{
			who.RequirePart<BitLocker>().AddBits(randomBits);
			if (who.IsPlayer())
			{
				Popup.Show("You receive tinkering bits <{{|" + BitType.GetDisplayString(randomBits) + "}}>");
			}
		}
	}

	public void RepairResultPartialSuccess(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You make some progress repairing " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultFailure(GameObject who, GameObject obj)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You can't figure out how to fix " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		}
	}

	public void RepairResultCriticalFailure(GameObject who, GameObject obj)
	{
		if (!RepairCriticalFailureEvent.Check(who, obj))
		{
			return;
		}
		if (obj.IsBroken())
		{
			if (!obj.HasPropertyOrTag("CantDestroyOnRepair"))
			{
				obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
				IComponent<GameObject>.XDidYToZ(who, "accidentally", "destroy", obj, null, "!", null, null, who);
				obj.Destroy();
			}
			else
			{
				RepairResultFailure(who, obj);
			}
			return;
		}
		string message = "You think you broke " + obj.them + "...";
		if (obj.ApplyEffect(new Broken()) && obj.IsBroken())
		{
			if (who.IsPlayer())
			{
				Popup.Show(message);
			}
			obj.PotentiallyAngerOwner(who, "DontWarnOnRepair");
		}
		else
		{
			RepairResultFailure(who, obj);
		}
	}
}
