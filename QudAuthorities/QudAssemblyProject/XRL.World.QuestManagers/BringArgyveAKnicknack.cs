using System;
using XRL.Core;
using XRL.World.Parts;

namespace XRL.World.QuestManagers;

[Serializable]
public class BringArgyveAKnicknack : QuestManager
{
	public override void OnQuestAdded()
	{
		XRLCore.Core.Game.Player.Body.Inventory.ForeachObject(delegate(GameObject GO)
		{
			if (GO.HasPart("Examiner") && GO.HasPart("TinkerItem") && GO.GetPart<Examiner>().Complexity > 0)
			{
				XRLCore.Core.Game.FinishQuestStep("Fetch Argyve a Knickknack", "Find a Knickknack");
				return false;
			}
			return true;
		});
		IComponent<GameObject>.ThePlayer.AddPart(this);
		IComponent<GameObject>.ThePlayer.RegisterPartEvent(this, "Took");
	}

	public override void OnQuestComplete()
	{
		IComponent<GameObject>.ThePlayer.RemovePart(this);
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Argyve");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter.HasPart("Examiner") && gameObjectParameter.HasPart("TinkerItem") && gameObjectParameter.GetPart<Examiner>().Complexity > 0)
			{
				XRLCore.Core.Game.FinishQuestStep("Fetch Argyve a Knickknack", "Find a Knickknack");
			}
		}
		return base.FireEvent(E);
	}
}
