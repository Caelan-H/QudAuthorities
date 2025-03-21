using System;

namespace XRL.World.Effects;

[Serializable]
public class ResummonGloaming : Effect
{
	public string currentZone;

	public ResummonGloaming()
	{
		base.Duration = 1;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("ResummonGloaming"))
		{
			return false;
		}
		if (Object.CurrentZone != null)
		{
			currentZone = Object.CurrentZone.ZoneID;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (base.Duration > 0 && base.Object.CurrentZone?.ZoneID != currentZone && !base.Object.OnWorldMap())
		{
			Cell connectedSpawnLocation = base.Object.CurrentCell.GetConnectedSpawnLocation();
			if (connectedSpawnLocation != null)
			{
				GameObject gameObject = connectedSpawnLocation.AddObject("Gloaming");
				gameObject.SetPartyLeader(base.Object, takeOnAttitudesOfLeader: false);
				base.Duration = 0;
				IComponent<GameObject>.XDidY(gameObject, "reappear");
			}
		}
		return base.HandleEvent(E);
	}
}
