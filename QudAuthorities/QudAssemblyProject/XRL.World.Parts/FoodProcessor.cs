using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class FoodProcessor : IPoweredPart
{
	public FoodProcessor()
	{
		ChargeUse = 500;
		WorksOnInventory = true;
		NameForStatus = "FoodProcessing";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EndTurn");
		base.Register(Object);
	}

	private bool ProcessFoodItem(GameObject obj)
	{
		if (obj.GetPart("Butcherable") is Butcherable butcherable && butcherable.AttemptButcher(ParentObject, Automatic: false, SkipSkill: true, IntoInventory: true))
		{
			return false;
		}
		if (obj.GetPart("PreservableItem") is PreservableItem)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(ParentObject.The).Append(ParentObject.DisplayNameOnly).Append(ParentObject.GetVerb("preserve"))
				.Append(' ');
			if (Campfire.PerformPreserve(obj, stringBuilder, ParentObject, Capitalize: false))
			{
				if (Visible())
				{
					stringBuilder.Append('.');
					IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
				}
				return false;
			}
		}
		return true;
	}

	public bool ProcessFood()
	{
		bool num = ForeachActivePartSubjectWhile(ProcessFoodItem, MayMoveAddOrDestroy: true);
		if (!num)
		{
			ConsumeCharge();
		}
		return num;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ProcessFood();
		}
		return base.FireEvent(E);
	}
}
