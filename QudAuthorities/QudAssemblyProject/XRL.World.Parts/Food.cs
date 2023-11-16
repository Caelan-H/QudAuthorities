using System;
using ConsoleLib.Console;
using HistoryKit;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Food : IPart
{
	public static readonly int smallHungerAmount = 200;

	public int Thirst;

	public string Satiation = "None";

	public bool Gross;

	public bool IllOnEat;

	public string Healing = "0";

	public string Message = "That hits the spot!";

	public override bool SameAs(IPart p)
	{
		Food food = p as Food;
		if (food.Thirst != Thirst)
		{
			return false;
		}
		if (food.Satiation != Satiation)
		{
			return false;
		}
		if (food.Gross != Gross)
		{
			return false;
		}
		if (food.IllOnEat != IllOnEat)
		{
			return false;
		}
		if (food.Healing != Healing)
		{
			return false;
		}
		if (food.Message != Message)
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
		int @default = 20;
		if (Healing == "0" && !ParentObject.HasPart("HealOnEat") && !ParentObject.HasPart("GeometricHealOnEat"))
		{
			@default = 0;
		}
		E.AddAction("Eat", "eat", "Eat", null, 'a', FireOnActor: false, @default);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Eat")
		{
			if (!E.Actor.CanMoveExtremities("Eat", ShowMessage: true))
			{
				return false;
			}
			Stomach stomach = E.Actor.GetPart("Stomach") as Stomach;
			if (Gross && (!E.Actor.HasPart("Carnivorous") || !ParentObject.HasTag("Meat")) && (stomach == null || (stomach.HungerLevel < 2 && !E.Actor.HasSkill("Discipline_MindOverBody"))))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You're not hungry enough to bring " + E.Actor.itself + " to eat that.");
				}
				return true;
			}
			Event e = Event.New("OnEat", "Eater", E.Actor);
			bool num = ParentObject.FireEvent(e);
			E.ProcessChildEvent(e);
			if (num)
			{
				_ = stomach.HungerLevel;
				if (E.Actor.FireEvent(Event.New("Eating", "Food", ParentObject)))
				{
					E.Actor.FireEvent(Event.New("AddWater", "Amount", Thirst));
					E.Actor.Heal(Healing.RollCached(), Message: false, FloatText: true, RandomMinimum: true);
					if (E.Actor.HasPart("Stomach"))
					{
						if (!E.Actor.HasPart("Carnivorous") || ParentObject.HasTag("Meat"))
						{
							if (Satiation == "Snack")
							{
								stomach.CookingCounter -= smallHungerAmount;
							}
							if (Satiation == "Meal")
							{
								stomach.CookingCounter = 0;
							}
						}
						if (E.Actor.IsPlayer())
						{
							string text = Message ?? "";
							if (text != "")
							{
								if (text.Contains("~"))
								{
									text = text.Split('~')[0];
								}
								if (!text.EndsWith("!") && !text.EndsWith(".") && !text.EndsWith("?"))
								{
									text += ".";
								}
								text = Markup.Wrap(text);
								text += "\n";
							}
							Popup.Show("You eat " + ParentObject.the + ParentObject.DisplayNameSingle + ".\n" + text + "You are now {{|" + stomach.FoodStatus() + "}} and {{|" + stomach.WaterStatus() + "}}.");
						}
					}
					ParentObject.FireEvent(Event.New("Eaten", "Eater", E.Actor));
					if ((IllOnEat || (!ParentObject.HasTag("Meat") && E.Actor.HasPart("Carnivorous") && If.d100(50))) && (!E.Actor.HasPart("Carnivorous") || !ParentObject.HasTag("Meat")))
					{
						if (E.Actor.IsPlayer())
						{
							Popup.Show("Ugh, you feel sick.");
						}
						E.Actor.ApplyEffect(new Ill(100, 1));
					}
					E.Actor.UseEnergy(1000, "Item Eat");
					E.RequestInterfaceExit();
					ParentObject.Destroy();
				}
			}
		}
		return base.HandleEvent(E);
	}
}
