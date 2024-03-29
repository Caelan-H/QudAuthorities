using System;

namespace XRL.World.Parts;

[Serializable]
public class DynamicQuestTarget : IPart
{
	public override void Initialize()
	{
		ParentObject.SetIntProperty("NoAIEquip", 1);
		ParentObject.SetIntProperty("QuestItem", 1);
		ParentObject.SetEpistemicStatus(2);
		if (!string.IsNullOrEmpty(ParentObject.pPhysics?.Category))
		{
			ParentObject.SetStringProperty("OriginalCategory", ParentObject.pPhysics.Category);
		}
		ParentObject.pPhysics.Category = "Quest Items";
		base.Initialize();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID)
		{
			return ID == AllowTradeWithNoInventoryEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(2.0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return false;
	}
}
