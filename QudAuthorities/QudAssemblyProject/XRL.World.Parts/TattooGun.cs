using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class TattooGun : IPoweredPart
{
	public string LiquidConsumed = "ink";

	public TattooGun()
	{
		ChargeUse = 0;
		MustBeUnderstood = true;
		WorksOnEquipper = true;
		WorksOnCarrier = true;
		IsEMPSensitive = false;
		NameForStatus = "IntradermalInjector";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as TattooGun).LiquidConsumed != LiquidConsumed)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor))
		{
			E.AddAction("Tattoo", "tattoo", "Tattoo", null, 't', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Tattoo" && AttemptTattoo(E.Actor))
		{
			E.Actor.UseEnergy(10000, "Item Tattoo Gun");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!string.IsNullOrEmpty(LiquidConsumed))
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.Volume > 0)
			{
				return !liquidVolume.IsPureLiquid(LiquidConsumed);
			}
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "ProcessInputMissing";
	}

	public bool AttemptTattoo(GameObject who)
	{
		if (!who.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			return false;
		}
		if (who.AreHostilesNearby())
		{
			if (who.IsPlayer())
			{
				Popup.Show("You can't tattoo with hostiles nearby.");
			}
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != 0)
		{
			switch (activePartStatus)
			{
			case ActivePartStatus.LocallyDefinedFailure:
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.T() + ParentObject.Is + " out of {{|" + LiquidVolume.getLiquid(LiquidConsumed).GetName() + "}}.");
				}
				break;
			case ActivePartStatus.Unpowered:
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.T() + ParentObject.GetVerb("aren't") + " merely " + ParentObject.GetVerb("click") + ".");
				}
				break;
			default:
				if (who.IsPlayer())
				{
					Popup.Show(ParentObject.T() + ParentObject.GetVerb("aren't") + " working!");
				}
				break;
			}
			return false;
		}
		if (!who.IsPlayer())
		{
			return false;
		}
		string text = XRL.UI.PickDirection.ShowPicker();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (who.CurrentCell == null)
		{
			return false;
		}
		Cell cellFromDirection = who.CurrentCell.GetCellFromDirection(text);
		if (cellFromDirection == null)
		{
			return false;
		}
		GameObject firstObjectWithPart = cellFromDirection.GetFirstObjectWithPart("Body", who.GetPhase(), who, null, null, null, CheckFlight: true);
		if (firstObjectWithPart == null)
		{
			if (who.IsPlayer())
			{
				Popup.Show("There is nobody there you can tattoo.");
			}
			return false;
		}
		if (firstObjectWithPart != who && !firstObjectWithPart.IsPlayerLed())
		{
			if (who.IsPlayer())
			{
				Popup.Show("You can only tattoo " + who.itself + " and your companions.");
			}
			return false;
		}
		Body body = firstObjectWithPart.Body;
		if (body == null)
		{
			if (who.IsPlayer())
			{
				Popup.Show("You cannot tattoo " + ((firstObjectWithPart == who) ? who.itself : firstObjectWithPart.t()) + ".");
			}
			return false;
		}
		List<BodyPart> list = new List<BodyPart>(16);
		foreach (BodyPart part in body.GetParts())
		{
			if (!part.Abstract && part.Contact)
			{
				list.Add(part);
			}
		}
		if (list.Count == 0)
		{
			if (who.IsPlayer())
			{
				Popup.Show("You cannot tattoo " + ((firstObjectWithPart == who) ? who.itself : firstObjectWithPart.t()) + ".");
			}
			return false;
		}
		List<string> list2 = new List<string>(list.Count);
		List<BodyPart> list3 = new List<BodyPart>(list.Count);
		List<char> list4 = new List<char>(list.Count);
		char c = 'a';
		foreach (BodyPart item in list)
		{
			list2.Add(item.GetCardinalName());
			list3.Add(item);
			list4.Add(c);
			c = (char)(c + 1);
		}
		int num = Popup.ShowOptionList("Choose a body part to tattoo.", list2.ToArray(), list4.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
		if (num < 0)
		{
			return false;
		}
		BodyPart bodyPart = list3[num];
		if (bodyPart == null)
		{
			return false;
		}
		Tattoos.ApplyResult applyResult = Tattoos.CanApplyTattoo(who, bodyPart);
		if (applyResult != 0)
		{
			switch (applyResult)
			{
			case Tattoos.ApplyResult.TooManyTattoos:
				if (who.IsPlayer())
				{
					Popup.Show("There are too many tattoos on " + ((firstObjectWithPart == who) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + " to add more.");
				}
				break;
			case Tattoos.ApplyResult.AbstractBodyPart:
			case Tattoos.ApplyResult.NonContactBodyPart:
				if (who.IsPlayer())
				{
					Popup.Show("You can't tattoo " + ((firstObjectWithPart == who) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
				}
				break;
			case Tattoos.ApplyResult.InappropriateBodyPart:
				if (who.IsPlayer())
				{
					Popup.Show(((firstObjectWithPart == who) ? "Your" : Grammar.MakePossessive(firstObjectWithPart.T())) + " " + bodyPart.GetOrdinalName() + " can't be tattooed because " + (bodyPart.Plural ? "they don't" : "it doesn't") + " have flesh.");
				}
				break;
			default:
				if (who.IsPlayer())
				{
					Popup.Show("You can't tattoo " + ((firstObjectWithPart == who) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
				}
				break;
			}
			return false;
		}
		string text2 = Popup.AskString("Describe your tattoo. For example: \"a tiny snail\".", "", 30, 0, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -+/#()!@$%*<>");
		if (string.IsNullOrEmpty(text2))
		{
			return false;
		}
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (who.IsPlayer())
			{
				Popup.Show(ParentObject.T() + ParentObject.GetVerb("aren't") + " working!");
			}
			return false;
		}
		string text3 = Popup.ShowColorPicker("Choose a primary color.", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, 0, "", includeNone: true, includePatterns: false, allowBackground: true);
		if (text3 == null)
		{
			return false;
		}
		string text4 = null;
		if (!string.IsNullOrEmpty(text3))
		{
			text4 = Popup.ShowColorPicker("Choose a secondary color.");
			if (text4 == null)
			{
				return false;
			}
		}
		bool flag = who.HasMarkOfDeath();
		ConsumeLiquid(LiquidConsumed);
		if (Tattoos.ApplyTattoo(firstObjectWithPart, bodyPart, text2, text3, text4) != 0)
		{
			if (who.IsPlayer())
			{
				Popup.Show("Something went wrong and the tattooing fails.");
			}
			return false;
		}
		if (who.IsPlayer())
		{
			if (!flag && who.HasMarkOfDeath())
			{
				Popup.Show("You tattoo the mark of death on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
				The.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
			}
			else
			{
				Popup.Show("You tattoo " + text2 + " on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
			}
			if (firstObjectWithPart == who)
			{
				AchievementManager.SetAchievement("ACH_TATTOO_SELF");
			}
		}
		return true;
	}
}
