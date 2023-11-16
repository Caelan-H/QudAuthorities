using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class BurnToAshesIfOrganic : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.pPhysics != null && (ParentObject.pPhysics.LastDamagedByType == "Fire" || ParentObject.pPhysics.LastDamagedByType == "Light") && ParentObject.GetIntProperty("Inorganic") <= 0 && !ParentObject.HasPart("Metal") && !ParentObject.HasPart("Crysteel") && !ParentObject.HasPart("Zetachrome") && ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			GameObject inInventory = ParentObject.InInventory;
			if (inInventory != null && !inInventory.IsCreature)
			{
				GameObject gameObject = GameObject.create("Ashes");
				DoCarryOvers(ParentObject, gameObject);
				inInventory.ReceiveObject(gameObject);
			}
			else
			{
				Cell dropCell = ParentObject.GetDropCell();
				if (dropCell != null)
				{
					GameObject to = dropCell.AddObject("Ashes");
					DoCarryOvers(ParentObject, to);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void DoCarryOvers(GameObject From, GameObject To)
	{
		if (From.HasProperty("StoredByPlayer") || From.HasProperty("FromStoredByPlayer"))
		{
			To.SetIntProperty("FromStoredByPlayer", 1);
		}
		Temporary.CarryOver(From, To);
		Phase.carryOver(From, To);
	}
}
