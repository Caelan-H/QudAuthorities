using System;
using XRL.UI;

namespace XRL.World.QuestManagers;

[Serializable]
public class PaxKlanqIPresume : QuestManager
{
	public const string QuestID = "Pax Klanq, I Presume?";

	public static bool Start()
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Eating");
		base.Register(Object);
	}

	public override void OnQuestAdded()
	{
		IComponent<GameObject>.ThePlayer.AddPart(this);
	}

	public override void OnStepComplete(string StepName)
	{
	}

	public override void OnQuestComplete()
	{
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Pax Klanq");
	}

	public static bool UnderConstructionMessage()
	{
		Popup.ShowSpace("You've reached the temporary end of the main questline.\n\nYou may continue to explore the world, and stay tuned for updates as we prepare to leave Early Access.");
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Zone activeZone = The.ActiveZone;
		if (activeZone.GetTerrainObject()?.Blueprint == "TerrainFungalCenter" && activeZone.X == 1 && activeZone.Y == 1 && activeZone.Z == 10)
		{
			The.Game.FinishQuestStep("Pax Klanq, I Presume?", "Seek the Heart of the Rainbow");
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eating")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Food");
			if (gameObjectParameter != null && gameObjectParameter.Blueprint == "Godshroom Cap")
			{
				The.Game.FinishQuestStep("Pax Klanq, I Presume?", "Eat the God's Flesh");
			}
		}
		return base.FireEvent(E);
	}
}
