using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class TeleportOnEat : IPart
{
	public bool Controlled;

	public string Level = "5-6";

	public override bool SameAs(IPart p)
	{
		if ((p as TeleportOnEat).Controlled != Controlled)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 5);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "OnEat");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObject = E.GetParameter("Eater") as GameObject;
			if (!gameObject.IsRealityDistortionUsable())
			{
				return true;
			}
			if (!Controlled)
			{
				Cell cell = gameObject.pPhysics.CurrentCell;
				if (cell != null && !cell.ParentZone.IsWorldMap() && gameObject.FireEvent("CheckRealityDistortionUsability"))
				{
					Event e = Event.New("CheckRealityDistortionAccessibility");
					List<Cell> emptyCells = cell.ParentZone.GetEmptyCells(e);
					if (emptyCells.Contains(cell))
					{
						emptyCells.Remove(cell);
					}
					if (emptyCells.Count > 0)
					{
						Cell randomElement = emptyCells.GetRandomElement();
						gameObject.ParticleBlip("&C\u000f");
						gameObject.TeleportTo(randomElement, 0);
						gameObject.TeleportSwirl();
						gameObject.ParticleBlip("&C\u000f");
						if (gameObject.IsPlayer())
						{
							Popup.Show("You are transported!");
						}
					}
				}
				return true;
			}
			GameObject subject = gameObject;
			Teleportation.Cast(null, Level, null, null, subject);
			return true;
		}
		return base.FireEvent(E);
	}
}
