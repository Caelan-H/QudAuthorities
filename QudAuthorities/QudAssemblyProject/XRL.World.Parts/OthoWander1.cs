using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class OthoWander1 : IPart
{
	public long startTurn;

	public static bool begin()
	{
		XRLCore.Core.Game.Player.Body.AddPart(new PlayerOthoWander1Safeguard());
		XRLCore.Core.Game.Player.Body.GetPart<PlayerOthoWander1Safeguard>().startTurn = XRLCore.Core.Game.Turns;
		GameObject gameObject = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.FindObject("Otho");
		gameObject.SetIntProperty("AllowGlobalTraversal", 1);
		gameObject.AddPart<OthoWander1>().startTurn = XRLCore.Core.Game.Turns;
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIBoredEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (The.Game.Turns - startTurn > 150 && ParentObject.CurrentCell != null && ParentObject.CurrentZone.Z == 13)
		{
			ParentObject.RemovePart(this);
			return base.HandleEvent(E);
		}
		if (The.Game.Turns - startTurn < 150)
		{
			ParentObject.pBrain.MoveToGlobal("JoppaWorld.22.14.1.0.14", 18, 8);
		}
		else if (The.Game.Turns - startTurn > 150)
		{
			The.Game.SetIntGameState("OmonporchReady", 1);
			ParentObject.pBrain.MoveToGlobal("JoppaWorld.22.14.1.0.13", 32, 21);
		}
		else
		{
			ParentObject.UseEnergy(1000);
		}
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CheckZoneSuspend");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CheckZoneSuspend")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
