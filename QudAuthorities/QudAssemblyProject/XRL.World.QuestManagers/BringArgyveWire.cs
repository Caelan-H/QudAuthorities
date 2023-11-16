using System;
using XRL.Core;

namespace XRL.World.QuestManagers;

[Serializable]
public class BringArgyveWire : QuestManager
{
	public int nTotalLength;

	public override void OnQuestAdded()
	{
		nTotalLength = 0;
		XRLCore.Core.Game.Player.Body.Inventory.ForeachObject(delegate(GameObject GO)
		{
			if (GO.HasPart("Wire"))
			{
				nTotalLength += GO.Count;
			}
		});
		if (nTotalLength >= 200)
		{
			The.Game.FinishQuestStep("Weirdwire Conduit... Eureka!", "Find 200 feet of copper wire");
		}
		IComponent<GameObject>.ThePlayer.AddPart(this);
		IComponent<GameObject>.ThePlayer.RegisterPartEvent(this, "Took");
	}

	public override void OnQuestComplete()
	{
		IComponent<GameObject>.ThePlayer.RemovePart("BringArgyveWire");
	}

	public override bool SameAs(IPart p)
	{
		if ((p as BringArgyveWire).nTotalLength != nTotalLength)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override GameObject GetQuestInfluencer()
	{
		return GameObject.findByBlueprint("Argyve");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Took")
		{
			int num = nTotalLength;
			nTotalLength = 0;
			XRLCore.Core.Game.Player.Body.Inventory.ForeachObject(delegate(GameObject GO)
			{
				if (GO.HasPart("Wire"))
				{
					nTotalLength += GO.Count;
				}
			});
			if (nTotalLength >= 200)
			{
				The.Game.FinishQuestStep("Weirdwire Conduit... Eureka!", "Find 200 feet of copper wire");
			}
			else if (nTotalLength != num)
			{
				IComponent<GameObject>.AddPlayerMessage("You now have " + nTotalLength + " feet of copper wire.", 'c');
			}
		}
		return base.FireEvent(E);
	}
}
