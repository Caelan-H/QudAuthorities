using System;

namespace XRL.World.Parts;

[Serializable]
public class Rummager : IPart
{
	[FieldSaveVersion(229)]
	public bool Active = true;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "VillageInit");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			Active = false;
		}
		if (E.ID == "BeginTakeAction")
		{
			if (CheckPickUp())
			{
				return false;
			}
		}
		else if (E.ID == "EnteredCell")
		{
			CheckPickUp();
		}
		return base.FireEvent(E);
	}

	private bool CheckPickUp()
	{
		if (Active && !ParentObject.IsPlayerControlled() && ParentObject.Target == null)
		{
			foreach (GameObject item in ParentObject.CurrentCell.GetObjectsWithPart("Physics"))
			{
				if (item != ParentObject && !item.HasPart("Combat") && item.IsTakeable() && ParentObject.GetCarriedWeight() + item.Weight < ParentObject.GetMaxCarriedWeight() && ParentObject.TakeObject(item, Silent: true, 0))
				{
					DidXToY("pick", "up", item);
					return true;
				}
			}
		}
		return false;
	}
}
