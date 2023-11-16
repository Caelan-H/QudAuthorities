using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DeployWith : IPart
{
	public string Blueprint;

	public bool SolidOkay;

	public bool SeepingOkay;

	public bool SameCell;

	public string PreferredDirection;

	public int Chance = 100;

	public bool CarryOverOwner = true;

	public override bool SameAs(IPart p)
	{
		DeployWith deployWith = p as DeployWith;
		if (deployWith.Blueprint != Blueprint)
		{
			return false;
		}
		if (deployWith.SolidOkay != SolidOkay)
		{
			return false;
		}
		if (deployWith.SeepingOkay != SeepingOkay)
		{
			return false;
		}
		if (deployWith.SameCell != SameCell)
		{
			return false;
		}
		if (deployWith.PreferredDirection != PreferredDirection)
		{
			return false;
		}
		if (deployWith.Chance != Chance)
		{
			return false;
		}
		if (deployWith.CarryOverOwner != CarryOverOwner)
		{
			return false;
		}
		return true;
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		PerformDeploy();
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && ParentObject.CurrentCell != null && ParentObject.CurrentCell.ParentZone != null && ParentObject.CurrentCell.ParentZone.Built)
		{
			PerformDeploy();
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}

	public void PerformDeploy()
	{
		if (!Chance.in100())
		{
			return;
		}
		Cell C = ParentObject.CurrentCell;
		if (C == null)
		{
			return;
		}
		Cell TC = null;
		if (SameCell)
		{
			TC = C;
		}
		else
		{
			int num = 0;
			C.ForeachLocalAdjacentCell(delegate(Cell AC)
			{
				if (SolidOkay || !AC.IsSolid(SeepingOkay))
				{
					if (!string.IsNullOrEmpty(PreferredDirection) && C.GetDirectionFromCell(AC) == PreferredDirection)
					{
						TC = AC;
						return false;
					}
					num++;
				}
				return true;
			});
			if (TC == null && num > 0)
			{
				int selected = Stat.Random(1, num);
				int pos = 0;
				C.ForeachLocalAdjacentCell(delegate(Cell AC)
				{
					if ((SolidOkay || !AC.IsSolid(SeepingOkay)) && ++pos == selected)
					{
						TC = AC;
						return false;
					}
					return true;
				});
			}
		}
		if (TC == null)
		{
			return;
		}
		if (CarryOverOwner)
		{
			string owner = ParentObject.Owner;
			GameObjectFactory.ProcessSpecification(Blueprint, delegate(GameObject GO)
			{
				if (!string.IsNullOrEmpty(owner) && GO.pPhysics != null)
				{
					GO.pPhysics.Owner = owner;
				}
				TC.AddObject(GO);
			});
		}
		else
		{
			GameObjectFactory.ProcessSpecification(Blueprint, delegate(GameObject GO)
			{
				TC.AddObject(GO);
			});
		}
	}
}
