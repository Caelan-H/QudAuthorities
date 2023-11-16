using System;

namespace XRL.World.Parts;

[Serializable]
public class Eskhind : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (The.Game.HasGameState("EskhindMoved") && !ParentObject.HasProperty("RealEskhind"))
		{
			ParentObject.CurrentCell.ParentZone.FindObject("Meyehind")?.Destroy();
			ParentObject.CurrentCell.ParentZone.FindObject("Liihart")?.Destroy();
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}
