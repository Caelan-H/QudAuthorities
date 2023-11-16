using System;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public abstract class IProgrammableRecoiler : IPoweredPart
{
	public bool Reprogrammable;

	public int TimesProgrammed;

	public IProgrammableRecoiler()
	{
		ChargeUse = 10000;
		IsRealityDistortionBased = true;
		MustBeUnderstood = true;
		WorksOnCarrier = true;
		WorksOnHolder = true;
	}

	public override bool SameAs(IPart p)
	{
		IProgrammableRecoiler programmableRecoiler = p as IProgrammableRecoiler;
		if (programmableRecoiler.Reprogrammable != Reprogrammable)
		{
			return false;
		}
		if (programmableRecoiler.TimesProgrammed != TimesProgrammed)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public virtual void ProgrammedForLocation(Zone Z, Cell C)
	{
	}

	public static bool ProgramObjectForLocation(GameObject obj, Zone Z, Cell C = null, IProgrammableRecoiler pPR = null)
	{
		if (Z == null)
		{
			return false;
		}
		if (Z.IsWorldMap())
		{
			return false;
		}
		ITeleporter partDescendedFrom = obj.GetPartDescendedFrom<ITeleporter>();
		if (partDescendedFrom == null)
		{
			return false;
		}
		if (pPR == null)
		{
			pPR = obj.GetPartDescendedFrom<IProgrammableRecoiler>();
			if (pPR == null)
			{
				return false;
			}
		}
		pPR.TimesProgrammed++;
		partDescendedFrom.DestinationZone = Z.ZoneID;
		partDescendedFrom.DestinationX = C?.X ?? (-1);
		partDescendedFrom.DestinationY = C?.Y ?? (-1);
		pPR.ProgrammedForLocation(Z, C);
		return true;
	}

	public static bool ProgramObjectForLocation(GameObject obj, Cell C)
	{
		return ProgramObjectForLocation(obj, C.ParentZone, C);
	}

	public static bool ProgramObjectForLocation(GameObject obj)
	{
		return ProgramObjectForLocation(obj, obj.CurrentCell);
	}

	public bool ProgramForLocation(Zone Z, Cell C = null)
	{
		return ProgramObjectForLocation(ParentObject, Z, C, this);
	}

	public bool ProgramForLocation(Cell C)
	{
		if (C == null)
		{
			return false;
		}
		return ProgramForLocation(C.ParentZone, C);
	}

	public bool ProgramForLocation(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		return ProgramForLocation(obj.CurrentCell);
	}

	public bool ProgramForLocation()
	{
		return ProgramForLocation(ParentObject);
	}

	public bool ProgramRecoiler(GameObject who, IEvent FromEvent = null)
	{
		if (!IsObjectActivePartSubject(who))
		{
			return false;
		}
		Cell cell = who.CurrentCell;
		if ((!Reprogrammable && TimesProgrammed > 0) || cell == null || cell.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			if (who.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + " merely" + ParentObject.GetVerb("click") + ".");
			}
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != 0)
		{
			if (activePartStatus == ActivePartStatus.Unpowered)
			{
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("do") + " not have enough charge to be imprinted with the current location.");
				}
			}
			else if (who.IsPlayer())
			{
				Popup.Show(ParentObject.The + ParentObject.DisplayNameOnly + " merely" + ParentObject.GetVerb("click") + ".");
			}
			return false;
		}
		if (IsRealityDistortionBased && !who.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", who, "Device", ParentObject), FromEvent))
		{
			return false;
		}
		if (who.IsPlayer())
		{
			Popup.Show(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("vibrate") + " as the current location is imprinted in " + ParentObject.its + " geospatial core.");
		}
		else if (ParentObject.InInventory == who || ParentObject.Equipped == who)
		{
			if (IComponent<GameObject>.Visible(who))
			{
				IComponent<GameObject>.AddPlayerMessage(Grammar.MakePossessive(who.The + who.DisplayNameOnly) + " " + ParentObject.DisplayNameOnly + ParentObject.GetVerb("vibrate") + " as the current location is imprinted in " + ParentObject.its + " geospatial core.");
			}
		}
		else if (IComponent<GameObject>.Visible(ParentObject))
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.DisplayNameOnly + ParentObject.GetVerb("vibrate") + " as the current location is imprinted in " + ParentObject.its + " geospatial core.");
		}
		ProgramForLocation(cell);
		return true;
	}
}
