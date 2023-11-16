using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class TimeCube : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != GetItemElementsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Activate", "activate", "ActivateTimeCube", null, 'a', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateTimeCube")
		{
			if (!E.Actor.FireEvent("CheckRealityDistortionUsability") || !ParentObject.FireEvent("CheckRealityDistortionUsability"))
			{
				IComponent<GameObject>.PlayUISound("ominous_powerup");
				Popup.Show("{{R|Fraudulent}} {{W|ONEness}} is taught by {{O|evil}} {{G|educators}}! Nothing happens!");
				E.Actor.UseEnergy(1000, "Item Failure");
			}
			else
			{
				IComponent<GameObject>.PlayUISound("time_dilation");
				AchievementManager.SetAchievement("ACH_ACTIVATE_TIMECUBE");
				Popup.ShowBlock("{{G|You are filled with the true vision! The {{B|Cubic Form}} is {{M|Infinite}}, {{W|Harmonic}} and transcends the {{R|1 Day rotation}}!}}");
				E.Actor.ApplyEffect(new TimeCubed());
				ParentObject.Destroy();
			}
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("time", 20);
		return base.HandleEvent(E);
	}
}
