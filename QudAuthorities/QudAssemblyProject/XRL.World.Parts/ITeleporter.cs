using System;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public abstract class ITeleporter : IPoweredPart
{
	public string DestinationZone = "";

	public int DestinationX = 40;

	public int DestinationY = 13;

	public bool UsableInCombat;

	public bool Intraplanar;

	public bool Interplanar;

	public bool Interprotocol;

	public ITeleporter()
	{
		ChargeUse = 0;
		WorksOnCarrier = true;
		WorksOnHolder = true;
	}

	public override bool SameAs(IPart p)
	{
		ITeleporter teleporter = p as ITeleporter;
		if (teleporter.DestinationZone != DestinationZone)
		{
			return false;
		}
		if (teleporter.DestinationX != DestinationX)
		{
			return false;
		}
		if (teleporter.DestinationY != DestinationY)
		{
			return false;
		}
		if (teleporter.UsableInCombat != UsableInCombat)
		{
			return false;
		}
		if (teleporter.Intraplanar != Intraplanar)
		{
			return false;
		}
		if (teleporter.Interplanar != Interplanar)
		{
			return false;
		}
		if (teleporter.Interprotocol != Interprotocol)
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
		if (Interprotocol)
		{
			E.Add("circuitry", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		string text = null;
		string text2 = null;
		WorldBlueprint worldBlueprint = null;
		WorldBlueprint worldBlueprint2 = null;
		if (!Intraplanar)
		{
			if (text == null)
			{
				text = ZoneID.GetWorldID(DestinationZone);
			}
			if (text2 == null)
			{
				text2 = ParentObject.GetCurrentCell()?.ParentZone?.ZoneWorld;
			}
			if (text != text2)
			{
				return true;
			}
		}
		if (!Interplanar)
		{
			if (worldBlueprint == null)
			{
				if (text == null)
				{
					text = ZoneID.GetWorldID(DestinationZone);
				}
				worldBlueprint = WorldFactory.Factory.getWorld(text);
			}
			if (worldBlueprint2 == null)
			{
				if (text2 == null)
				{
					text2 = ParentObject.GetCurrentCell().ParentZone.ZoneWorld;
				}
				worldBlueprint2 = WorldFactory.Factory.getWorld(text2);
			}
			if (worldBlueprint.Plane != worldBlueprint2.Plane)
			{
				return true;
			}
		}
		if (!Interprotocol)
		{
			if (worldBlueprint == null)
			{
				if (text == null)
				{
					text = ZoneID.GetWorldID(DestinationZone);
				}
				worldBlueprint = WorldFactory.Factory.getWorld(text);
			}
			if (worldBlueprint2 == null)
			{
				if (text2 == null)
				{
					text2 = ParentObject.GetCurrentCell().ParentZone.ZoneWorld;
				}
				worldBlueprint2 = WorldFactory.Factory.getWorld(text2);
			}
			if (worldBlueprint.Protocol != worldBlueprint2.Protocol)
			{
				return true;
			}
		}
		return base.GetActivePartLocallyDefinedFailure();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		string text = null;
		string text2 = null;
		WorldBlueprint worldBlueprint = null;
		WorldBlueprint worldBlueprint2 = null;
		if (!Intraplanar)
		{
			if (text == null)
			{
				text = ZoneID.GetWorldID(DestinationZone);
			}
			if (text2 == null)
			{
				text2 = ParentObject.GetCurrentCell().ParentZone.ZoneWorld;
			}
			if (text != text2)
			{
				return "ProtocolMismatch";
			}
		}
		if (!Interplanar)
		{
			if (worldBlueprint == null)
			{
				if (text == null)
				{
					text = ZoneID.GetWorldID(DestinationZone);
				}
				worldBlueprint = WorldFactory.Factory.getWorld(text);
			}
			if (worldBlueprint2 == null)
			{
				if (text2 == null)
				{
					text2 = ParentObject.GetCurrentCell().ParentZone.ZoneWorld;
				}
				worldBlueprint2 = WorldFactory.Factory.getWorld(text2);
			}
			if (worldBlueprint.Plane != worldBlueprint2.Plane)
			{
				return "QuantumPhaseMismatch";
			}
		}
		if (!Interprotocol)
		{
			if (worldBlueprint == null)
			{
				if (text == null)
				{
					text = ZoneID.GetWorldID(DestinationZone);
				}
				worldBlueprint = WorldFactory.Factory.getWorld(text);
			}
			if (worldBlueprint2 == null)
			{
				if (text2 == null)
				{
					text2 = ParentObject.GetCurrentCell().ParentZone.ZoneWorld;
				}
				worldBlueprint2 = WorldFactory.Factory.getWorld(text2);
			}
			if (worldBlueprint.Protocol != worldBlueprint2.Protocol)
			{
				return "RelativityCompensationFailure";
			}
		}
		return base.GetActivePartLocallyDefinedFailureDescription();
	}

	public bool AttemptTeleport(GameObject who, IEvent FromEvent = null)
	{
		if (string.IsNullOrEmpty(DestinationZone))
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("Nothing happens.");
			}
			return false;
		}
		if (!UsableInCombat && who.AreHostilesNearby())
		{
			if (who.IsPlayer())
			{
				Popup.ShowFail("You can't recoil with hostiles nearby!");
			}
			return false;
		}
		int num = ParentObject.QueryCharge(LiveOnly: false, 0L);
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: true, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != 0 || !IsObjectActivePartSubject(who))
		{
			if (who.IsPlayer())
			{
				if (activePartStatus == ActivePartStatus.LocallyDefinedFailure && GetActivePartLocallyDefinedFailureDescription() == "ProtocolMismatch")
				{
					string protocol = WorldFactory.Factory.getWorld(ParentObject.GetCurrentCell().ParentZone.ZoneWorld).Protocol;
					if (string.Equals(protocol, "THIN"))
					{
						Popup.ShowFail("You have no bodily tether to recoil.");
					}
					else if (string.Equals(protocol, "CLAM"))
					{
						Popup.ShowFail("You are stuck in a remote pocket dimension and cannot recoil out.");
					}
					else
					{
						Popup.ShowFail("You cannot do that here.");
					}
				}
				else
				{
					switch (activePartStatus)
					{
					case ActivePartStatus.Rusted:
						Popup.ShowFail(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName) + " activation button is rusted in place.");
						break;
					case ActivePartStatus.Broken:
						Popup.ShowFail(ParentObject.Itis + " broken...");
						break;
					case ActivePartStatus.Booting:
						Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.Is + " still starting up.");
						break;
					case ActivePartStatus.Unpowered:
						if (num > 0 && ParentObject.QueryCharge(LiveOnly: false, 0L) < num)
						{
							Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("hum") + " for a moment, then powers down. " + ParentObject.It + ParentObject.GetVerb("don't") + " have enough charge to function.");
						}
						else
						{
							Popup.ShowFail(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("don't") + " have enough charge to function.");
						}
						break;
					default:
						Popup.ShowFail("Nothing happens.");
						break;
					}
				}
			}
			return false;
		}
		return who.ZoneTeleport(DestinationZone, DestinationX, DestinationY, FromEvent, ParentObject, who);
	}
}
