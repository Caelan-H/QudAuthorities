using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsAirCurrentMicrosensor : IPart
{
	[NonSerialized]
	public Zone lastZone;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GenericDeepNotifyEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		E.Implantee.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.UnregisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericDeepNotifyEvent E)
	{
		if ((E.Notify == "MemoriesEaten" || E.Notify == "AmnesiaTriggered") && E.Subject == ParentObject.Implantee)
		{
			RevealStairs();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			GameObject implantee = ParentObject.Implantee;
			if (implantee != null && implantee.CurrentCell != null && implantee.CurrentZone != lastZone)
			{
				RevealStairs();
			}
		}
		return base.FireEvent(E);
	}

	public int RevealStairs()
	{
		int num = 0;
		lastZone = ParentObject.Implantee?.CurrentZone;
		if (lastZone != null)
		{
			foreach (Cell cell in lastZone.GetCells())
			{
				if (cell.HasObjectWithPart("StairsUp") || cell.HasObjectWithPart("StairsDown"))
				{
					cell.SetExplored(State: true);
					num++;
				}
			}
			return num;
		}
		return num;
	}
}
