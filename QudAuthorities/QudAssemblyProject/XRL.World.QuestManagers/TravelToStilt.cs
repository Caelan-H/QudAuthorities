using System;
using XRL.Core;

namespace XRL.World.QuestManagers;

[Serializable]
public class TravelToStilt : QuestManager
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override void OnQuestAdded()
	{
		IComponent<GameObject>.ThePlayer.AddPart(this);
	}

	public override void OnQuestComplete()
	{
		IComponent<GameObject>.ThePlayer.RemovePart("TravelToStilt");
	}

	public override GameObject GetQuestInfluencer()
	{
		if (50.in100())
		{
			return GameObject.findByBlueprint("Wardens Esther");
		}
		return GameObject.findByBlueprint("Tszappur");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && (ParentObject.InZone("JoppaWorld.5.2.1.1.10") || ParentObject.InZone("JoppaWorld.5.2.1.2.10")))
		{
			XRLCore.Core.Game.FinishQuestStep("O Glorious Shekhinah!", "Make a Pilgrimage to the Six Day Stilt");
		}
		return base.FireEvent(E);
	}
}
