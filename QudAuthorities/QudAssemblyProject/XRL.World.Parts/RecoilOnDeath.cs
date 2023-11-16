using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RecoilOnDeath : IPoweredPart
{
	public string DestinationZone = "";

	public int DestinationX = 40;

	public int DestinationY = 13;

	public RecoilOnDeath()
	{
		ChargeUse = 0;
		WorksOnEquipper = true;
		IsEMPSensitive = false;
		NameForStatus = "EmergencyMedEvac";
	}

	public override bool SameAs(IPart p)
	{
		RecoilOnDeath recoilOnDeath = p as RecoilOnDeath;
		if (recoilOnDeath.DestinationZone != DestinationZone)
		{
			return false;
		}
		if (recoilOnDeath.DestinationX != DestinationX)
		{
			return false;
		}
		if (recoilOnDeath.DestinationY != DestinationY)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		Object.RegisterPartEvent(this, "Dismember");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject")?.RegisterPartEvent(this, "BeforeDie");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject")?.UnregisterPartEvent(this, "BeforeDie");
		}
		else
		{
			if (E.ID == "Dismember")
			{
				BodyPart bodyPart = ParentObject.EquippedOn();
				if (bodyPart != null && bodyPart.ObjectEquippedOnThisOrAnyParent(ParentObject))
				{
					return false;
				}
				return true;
			}
			if (E.ID == "BeforeDie")
			{
				GameObject equipped = ParentObject.Equipped;
				if (equipped != null)
				{
					if (string.IsNullOrEmpty(DestinationZone))
					{
						if (equipped.IsPlayer())
						{
							Popup.Show("Nothing happens.");
						}
						return false;
					}
					Cell cell = equipped.pPhysics.CurrentCell;
					ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
					Cell cell2 = zoneManager.GetZone(DestinationZone).GetCell(DestinationX, DestinationY);
					if (equipped.HasStat("Hitpoints"))
					{
						equipped.Statistics["Hitpoints"].Penalty = 0;
					}
					if (DestinationX == -1 || DestinationY == -1)
					{
						try
						{
							List<Cell> emptyReachableCells = zoneManager.ActiveZone.GetEmptyReachableCells();
							cell2 = ((emptyReachableCells.Count <= 0) ? zoneManager.ActiveZone.GetCell(40, 20) : emptyReachableCells.GetRandomElement());
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							cell2 = zoneManager.ActiveZone.GetCell(40, 20);
						}
					}
					if (equipped.IsPlayer())
					{
						Popup.Show("Just before your demise, you are transported to safety! " + ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("disintegrate") + ".");
					}
					IComponent<GameObject>.XDidY(equipped, "dematerialize", "out of the local region of spacetime", null, null, equipped);
					equipped.DirectMoveTo(cell2);
					if (equipped.IsPlayer())
					{
						zoneManager.SetActiveZone(cell.ParentZone);
					}
					cell.RemoveObject(equipped);
					cell2.AddObject(equipped);
					The.ZoneManager.ProcessGoToPartyLeader();
					equipped.TeleportSwirl();
					equipped.UseEnergy(1000, "Item");
					E.RequestInterfaceExit();
					if (equipped.IsPlayer())
					{
						ParentObject.Destroy();
					}
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
