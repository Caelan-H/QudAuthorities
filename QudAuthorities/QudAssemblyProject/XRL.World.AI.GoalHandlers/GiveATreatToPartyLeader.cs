using System;
using XRL.World.Capabilities;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class GiveATreatToPartyLeader : GoalHandler
{
	private string treatTable;

	public GiveATreatToPartyLeader(string treatTable)
	{
		this.treatTable = treatTable;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Create()
	{
	}

	public override void TakeAction()
	{
		if (base.ParentObject.pBrain.PartyLeader == null || base.ParentObject.pBrain.ParentObject.IsInvalid())
		{
			Pop();
		}
		else if (base.ParentObject.DistanceTo(base.ParentObject.pBrain.PartyLeader) <= 1)
		{
			GameObject gameObject = GameObject.create(PopulationManager.RollOneFrom(treatTable).Blueprint);
			if (gameObject != null && base.ParentObject.pBrain.PartyLeader.IsPlayer())
			{
				Messaging.XDidY(base.ParentObject, "give", "you a treat");
				base.ParentObject.pBrain.PartyLeader.TakeObject(gameObject, Silent: false, 0);
			}
			Pop();
		}
		else
		{
			ParentBrain.PushGoal(new MoveTo(base.ParentObject.PartyLeader, careful: false, overridesCombat: false, 1));
		}
	}
}
