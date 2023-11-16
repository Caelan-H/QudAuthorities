using System;

namespace XRL.World.QuestManagers;

[Serializable]
public class MoreThanAWillingSpirit : QuestManager
{
	public override void OnQuestAdded()
	{
		foreach (GameObject @object in The.Player.Inventory.GetObjects())
		{
			if (@object.Blueprint == "Scrapped Waydroid")
			{
				The.Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
				The.Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
			}
			if (@object.Blueprint == "Dormant Waydroid")
			{
				The.Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
				The.Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
				The.Game.FinishQuestStep("More Than a Willing Spirit", "Repair the Waydroid");
			}
		}
		The.Player.AddPart(this);
	}

	public override void OnQuestComplete()
	{
		The.Player.RemovePart("MoreThanAWillingSpirit");
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Mafeo");
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "Took");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took")
		{
			foreach (GameObject item in The.Player.Inventory.GetObjectsDirect())
			{
				if (item.Blueprint == "Scrapped Waydroid")
				{
					The.Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
					The.Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
				}
				else if (item.Blueprint == "Dormant Waydroid")
				{
					The.Game.FinishQuestStep("More Than a Willing Spirit", "Travel to Golgotha");
					The.Game.FinishQuestStep("More Than a Willing Spirit", "Find a Dysfunctional Waydroid");
					The.Game.FinishQuestStep("More Than a Willing Spirit", "Repair the Waydroid");
				}
			}
		}
		return base.FireEvent(E);
	}
}
