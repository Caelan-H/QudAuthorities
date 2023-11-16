using XRL.World.Capabilities;

namespace XRL.World.Parts;

public class IsTechScannableUnlessAlive : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AnimateEvent.ID)
		{
			return ID == GetScanTypeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject && E.ScanType == Scanning.Scan.Structure && !E.Object.IsAlive && E.Object.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0)
		{
			E.ScanType = Scanning.Scan.Tech;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		E.WantToRemove(this);
		return base.HandleEvent(E);
	}
}
