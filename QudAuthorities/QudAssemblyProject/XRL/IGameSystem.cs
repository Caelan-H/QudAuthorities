using System;
using XRL.Core;
using XRL.World;

namespace XRL;

[Serializable]
public class IGameSystem
{
	public bool flaggedForRemoval;

	public string id;

	public XRLGame game => XRLCore.Core.Game;

	public GamePlayer player => XRLCore.Core.Game.Player;

	public virtual void FireEvent(Event e)
	{
	}

	public virtual int GetPriority()
	{
		return 0;
	}

	public virtual void OnAdded()
	{
	}

	public virtual void OnRemoved()
	{
	}

	public virtual int GetDaylightRadius(int radius)
	{
		return radius;
	}

	public virtual void PlayerEmbarking()
	{
	}

	public virtual void BeginPlayerTurn()
	{
	}

	public virtual void EndPlayerTurn()
	{
	}

	public virtual void BeginSegment()
	{
	}

	public virtual void EndTurn()
	{
	}

	public virtual void QuestCompleted(Quest q)
	{
	}

	public virtual void LocationDiscovered(string locationName)
	{
	}

	public virtual void WaterRitualPerformed(GameObject go)
	{
	}

	public virtual void PlayerReputationChanged(string faction, int oldValue, int newValue, string because)
	{
	}

	public virtual void NewZoneGenerated(Zone zone)
	{
	}

	public virtual void ZoneActivated(Zone zone)
	{
	}

	public virtual void LoadGame(SerializationReader reader)
	{
	}

	public virtual void SaveGame(SerializationWriter writer)
	{
	}

	public virtual bool AwardingXP(ref GameObject Actor, ref int Amount, ref int Tier, ref int Minimum, ref int Maximum, ref GameObject Kill, ref GameObject InfluencedBy, ref GameObject PassedUpFrom, ref GameObject PassedDownFrom, ref string Deed)
	{
		return true;
	}
}
