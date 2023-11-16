using System;

namespace XRL.World.Parts;

[Serializable]
public class PetEitherOrRespawner : IPart
{
	public bool respawnEither;

	public bool respawnOr;

	public string lastZone = "";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			try
			{
				if (ParentObject.pPhysics != null && ParentObject.pPhysics.CurrentCell != null)
				{
					if (lastZone != "" && ParentObject.CurrentZone.ZoneID != lastZone && !ParentObject.CurrentZone.IsWorldMap())
					{
						if (respawnEither)
						{
							Cell connectedSpawnLocation = ParentObject.CurrentCell.GetConnectedSpawnLocation();
							if (connectedSpawnLocation != null)
							{
								GameObject gameObject = connectedSpawnLocation.AddObject("EitherPet");
								gameObject.SetActive();
								gameObject.PartyLeader = IComponent<GameObject>.ThePlayer;
								gameObject.TakeOnAttitudesOf(IComponent<GameObject>.ThePlayer);
								gameObject.IsTrifling = true;
								respawnEither = false;
							}
						}
						if (respawnOr)
						{
							Cell connectedSpawnLocation2 = ParentObject.CurrentCell.GetConnectedSpawnLocation();
							if (connectedSpawnLocation2 != null)
							{
								GameObject gameObject2 = connectedSpawnLocation2.AddObject("OrPet");
								gameObject2.SetActive();
								gameObject2.PartyLeader = IComponent<GameObject>.ThePlayer;
								gameObject2.TakeOnAttitudesOf(IComponent<GameObject>.ThePlayer);
								gameObject2.IsTrifling = true;
								respawnOr = false;
							}
						}
					}
					lastZone = ParentObject.pPhysics.CurrentCell.ParentZone.ZoneID;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("PetEitherOrRespawner", x);
			}
		}
		return true;
	}
}
