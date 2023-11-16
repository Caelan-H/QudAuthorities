using System;

namespace XRL.World.Parts;

[Serializable]
public class RemoveWhenSlain : IPart
{
	public string GameState;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDieEvent.ID && ID != ZoneActivatedEvent.ID)
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		The.Game.SetBooleanGameState(GameState ?? (ParentObject.Blueprint + "Killed"), Value: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (The.Game.GetBooleanGameState(GameState ?? (ParentObject.Blueprint + "Killed")) && ParentObject.IsValid())
		{
			ParentObject.Destroy(null, Silent: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (The.Game.GetBooleanGameState(GameState ?? (ParentObject.Blueprint + "Killed")))
		{
			ParentObject.Destroy(null, Silent: true);
		}
		return base.HandleEvent(E);
	}
}
