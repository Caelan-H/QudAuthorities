using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete("save compat")]
public class BethesdaColdZone : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
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
		Zone parentZone = E.Cell.ParentZone;
		if (parentZone != null)
		{
			parentZone.BaseTemperature = 25 - (parentZone.Z - 10) * 9;
			for (int i = 0; i < parentZone.Width; i++)
			{
				for (int j = 0; j < parentZone.Height; j++)
				{
					Cell cell = parentZone.Map[i][j];
					int k = 0;
					for (int count = cell.Objects.Count; k < count; k++)
					{
						GameObject gameObject = cell.Objects[k];
						if (gameObject.pPhysics != null)
						{
							if (gameObject.HasProperty("StartFrozen"))
							{
								gameObject.pPhysics.Temperature = gameObject.pPhysics.BrittleTemperature - 30;
							}
							else if (gameObject.Stat("ColdResistance") < 100)
							{
								gameObject.pPhysics.Temperature = gameObject.pPhysics.AmbientTemperature;
							}
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}
