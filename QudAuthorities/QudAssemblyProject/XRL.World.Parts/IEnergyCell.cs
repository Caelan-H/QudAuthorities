using System;

namespace XRL.World.Parts;

[Serializable]
public abstract class IEnergyCell : IRechargeable
{
	public string SlotType = "EnergyCell";

	public GameObject SlottedIn;

	public abstract bool HasAnyCharge();

	public abstract bool HasCharge(int Amount);

	public abstract int GetCharge();

	public abstract string ChargeStatus();

	public abstract void TinkerInitialize();

	public abstract void UseCharge(int Amount);

	public abstract void RandomizeCharge();

	public abstract void MaximizeCharge();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetContextEvent.ID && ID != RemoveFromContextEvent.ID && ID != ReplaceInContextEvent.ID)
		{
			return ID == TryRemoveFromContextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.validate(ref SlottedIn))
		{
			E.ObjectContext = SlottedIn;
			E.Relation = 5;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		ReplaceCell(E.Replacement);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ReplaceCell(null);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		ReplaceCell(null);
		return base.HandleEvent(E);
	}

	private void ReplaceCell(GameObject Replacement)
	{
		if (GameObject.validate(ref SlottedIn) && SlottedIn.GetPart("EnergyCellSocket") is EnergyCellSocket energyCellSocket && energyCellSocket.Cell == ParentObject)
		{
			energyCellSocket.SetCell(Replacement);
		}
	}

	public GameObject GetSlottedIn()
	{
		GameObject.validate(ref SlottedIn);
		return SlottedIn;
	}
}
