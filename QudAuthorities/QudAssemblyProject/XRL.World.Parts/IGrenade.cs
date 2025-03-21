using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public abstract class IGrenade : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterThrownEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != TookDamageEvent.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.HasIntProperty("Primed"))
		{
			ParentObject.RemoveIntProperty("Primed");
			Detonate();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		if (Detonate(ParentObject.CurrentCell, E.Actor, E.ApparentTarget))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (ParentObject.Understood())
		{
			E.AddAction("Detonate", "detonate", "Detonate", null, 'n', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Detonate")
		{
			if (base.OnWorldMap)
			{
				Popup.ShowFail("You cannot do that on the world map.");
			}
			else
			{
				E.Item.SplitFromStack();
				MissileWeapon.SetupProjectile(E.Item, E.Actor);
				if (Detonate(null, E.Actor))
				{
					E.Actor.UseEnergy(1000, "Item Grenade Detonate");
					E.RequestInterfaceExit();
				}
				else
				{
					MissileWeapon.CleanupProjectile(E.Item);
					E.Item.CheckStack();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		ParentObject.SplitFromStack();
		if (!Detonate(null, E.Actor, null, Indirect: true))
		{
			return false;
		}
		ParentObject.CheckStack();
		return base.HandleEvent(E);
	}

	protected abstract bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false);

	public bool Detonate(Cell InCell = null, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		Cell cell = InCell ?? ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		if (ParentObject.CurrentCell != cell)
		{
			ParentObject.RemoveFromContext();
			cell.AddObject(ParentObject, Forced: true, System: false, IgnoreGravity: true);
		}
		if (!BeforeDetonateEvent.Check(ParentObject, Actor, ApparentTarget, Indirect))
		{
			return false;
		}
		bool num = DoDetonate(cell, Actor, ApparentTarget, Indirect);
		if (!num)
		{
			ParentObject.Gravitate();
		}
		return num;
	}
}
