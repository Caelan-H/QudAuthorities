using System;

namespace XRL.World.Capabilities;

public static class SmartUse
{
	[NonSerialized]
	private static Event eCanSmartUse = new Event("CanSmartUse", "User", (object)null);

	[NonSerialized]
	private static Event ePreventSmartUse = new Event("PreventSmartUse", "User", (object)null);

	[NonSerialized]
	private static Event eCommandSmartUseEarly = new Event("CommandSmartUseEarly", "User", (object)null);

	[NonSerialized]
	private static Event eCommandSmartUse = new Event("CommandSmartUse", "User", (object)null);

	public static GameObject FindSmartUseObject(Cell TargetCell, GameObject who)
	{
		return TargetCell.GetHighestRenderLayerInteractableObjectFor(who, (GameObject o) => CanSmartUse(o, who));
	}

	public static GameObject FindPlayerSmartUseObject(Cell TargetCell)
	{
		return TargetCell.GetHighestRenderLayerInteractableObjectFor(The.Player, CanPlayerSmartUse);
	}

	public static bool CanSmartUse(GameObject GO, GameObject who)
	{
		if (GO == who || GO.HasTag("NoSmartUse"))
		{
			return false;
		}
		if (GO.pRender != null && !GO.pRender.Visible)
		{
			return false;
		}
		if (GO.HasTag("ForceSmartUse"))
		{
			return true;
		}
		bool flag = false;
		if (GO.HasRegisteredEvent(eCanSmartUse.ID))
		{
			eCanSmartUse.SetParameter("User", who);
			if (!GO.FireEvent(eCanSmartUse))
			{
				flag = true;
			}
		}
		bool flag2 = false;
		if (GO.WantEvent(CanSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CanSmartUseEvent.FromPool(who, GO)))
		{
			flag2 = true;
		}
		if (!flag && !flag2)
		{
			return false;
		}
		if (GO.HasRegisteredEvent(ePreventSmartUse.ID))
		{
			ePreventSmartUse.SetParameter("User", who);
			if (!GO.FireEvent(ePreventSmartUse))
			{
				return false;
			}
		}
		if (GO.WantEvent(PreventSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(PreventSmartUseEvent.FromPool(who, GO)))
		{
			return false;
		}
		Cell currentCell = GO.CurrentCell;
		if (currentCell != null && who.CurrentCell != currentCell && !GO.ConsiderSolidFor(who) && currentCell.IsSolidFor(who))
		{
			return false;
		}
		return true;
	}

	public static bool CanPlayerSmartUse(GameObject GO)
	{
		return CanSmartUse(GO, The.Player);
	}

	public static bool CanTake(GameObject GO, GameObject who)
	{
		if (GO == who)
		{
			return false;
		}
		if (GO.pRender != null && !GO.pRender.Visible)
		{
			return false;
		}
		return GO.IsTakeable();
	}

	public static bool CanPlayerTake(GameObject GO)
	{
		return CanTake(GO, The.Player);
	}

	public static bool PerformSmartUse(GameObject GO, GameObject who)
	{
		if (GO == null)
		{
			return false;
		}
		if (GO.HasRegisteredEvent(eCommandSmartUseEarly.ID))
		{
			eCommandSmartUseEarly.SetParameter("User", who);
			if (!GO.FireEvent(eCommandSmartUseEarly))
			{
				return false;
			}
		}
		if (GO.WantEvent(CommandSmartUseEarlyEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CommandSmartUseEarlyEvent.FromPool(who, GO)))
		{
			return false;
		}
		if (GO.HasRegisteredEvent(eCommandSmartUse.ID))
		{
			eCommandSmartUse.SetParameter("User", who);
			if (!GO.FireEvent(eCommandSmartUse))
			{
				return false;
			}
		}
		if (GO.WantEvent(CommandSmartUseEvent.ID, MinEvent.CascadeLevel) && !GO.HandleEvent(CommandSmartUseEvent.FromPool(who, GO)))
		{
			return false;
		}
		return true;
	}

	public static bool PlayerPerformSmartUse(GameObject GO)
	{
		return PerformSmartUse(GO, The.Player);
	}

	public static bool PlayerPerformSmartUse(Cell TargetCell)
	{
		return PlayerPerformSmartUse(FindPlayerSmartUseObject(TargetCell));
	}
}
