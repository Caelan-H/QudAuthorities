using System;

namespace XRL.World.Parts;

[Serializable]
public class PopulationSpawner : IPart
{
	public bool DestroyAfterSpawn = true;

	public bool spawned;

	public string Table = "SaltyWaterPuddle";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (spawned)
			{
				return true;
			}
			spawned = true;
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null)
			{
				foreach (PopulationResult item in PopulationManager.Generate(Table))
				{
					if (!string.IsNullOrEmpty(item.Blueprint))
					{
						for (int i = 0; i < item.Number; i++)
						{
							cell.AddObject(item.Blueprint);
						}
					}
				}
			}
		}
		if (DestroyAfterSpawn)
		{
			ParentObject.Destroy();
		}
		return true;
	}
}
