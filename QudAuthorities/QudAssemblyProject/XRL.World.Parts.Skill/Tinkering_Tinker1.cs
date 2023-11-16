using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Tinker1 : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandRechargeObject");
		base.Register(Object);
	}

	public bool Recharge(GameObject obj, IEvent E = null)
	{
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		bool AnyParts = false;
		bool AnyRechargeable = false;
		bool AnyRecharged = false;
		obj.ForeachPartDescendedFrom(delegate(IRechargeable rcg)
		{
			AnyParts = true;
			if (!rcg.CanBeRecharged())
			{
				return true;
			}
			AnyRechargeable = true;
			int rechargeAmount = rcg.GetRechargeAmount();
			if (rechargeAmount <= 0)
			{
				return true;
			}
			int num = rechargeAmount / 3000;
			if (num < 1)
			{
				num = 1;
			}
			int bitCount = BitLocker.GetBitCount(ParentObject, 'R');
			if (bitCount == 0)
			{
				Popup.ShowFail("You don't have any " + BitType.GetString("R") + " bits, which are required for recharging.");
				return false;
			}
			int num2 = Math.Min(bitCount, num);
			int valueOrDefault = Popup.AskNumber("It would take {{C|" + num + "}} " + BitType.GetString("R") + " " + ((num == 1) ? "bit" : "bits") + " to fully recharge " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ". You have {{C|" + bitCount + "}}. How many do you want to use?", num2, 0, num2).GetValueOrDefault();
			if (valueOrDefault > 0)
			{
				BitLocker.UseBits(ParentObject, 'R', valueOrDefault);
				obj.SplitFromStack();
				rcg.AddCharge((valueOrDefault < num) ? (valueOrDefault * 3000) : rechargeAmount);
				IComponent<GameObject>.PlayUISound("whine_up", Math.Min(1f, (float)bitCount / (float)num));
				Popup.Show("You have " + ((valueOrDefault < num) ? "partially " : "") + "recharged " + obj.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				obj.CheckStack();
				AnyRecharged = true;
			}
			return true;
		});
		if (!AnyParts)
		{
			Popup.ShowFail("That isn't an energy cell and does not have a rechargeable capacitor.");
		}
		else if (!AnyRechargeable)
		{
			Popup.ShowFail(obj.T() + " can't be recharged that way.");
		}
		if (AnyRecharged)
		{
			ParentObject.UseEnergy(1000, "Skill Tinkering Recharge");
			E?.RequestInterfaceExit();
		}
		return AnyRecharged;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRechargeObject" && ParentObject.CheckFrozen())
		{
			List<GameObject> list = Event.NewGameObjectList();
			foreach (GameObject content in ParentObject.GetContents())
			{
				if (content.Understood() && content.NeedsRecharge())
				{
					list.Add(content);
				}
			}
			if (list.Count == 0)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You have no items that require charging.");
				}
			}
			else
			{
				GameObject gameObject = Popup.PickGameObject("Select an item to charge.", list, AllowEscape: true, ShowContext: true);
				if (gameObject != null)
				{
					Recharge(gameObject, E);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.GetIntProperty("ReceivedTinker1Recipe") <= 0)
		{
			Tinkering part = GO.GetPart<Tinkering>();
			if (part != null)
			{
				part.LearnNewRecipe(0, 2);
				GO.SetIntProperty("ReceivedTinker1Recipe", 1);
			}
		}
		ActivatedAbilityID = AddMyActivatedAbility("Recharge", "CommandRechargeObject", "Skill", null, "\u009b");
		if (GO.IsPlayer())
		{
			TinkeringSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
