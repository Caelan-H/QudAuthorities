using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Stomach : IPart
{
	public const int MIN_WATER = 0;

	public const int PARCHED = 10000;

	public const int THIRSTY = 20000;

	public const int QUENCHED = 30000;

	public const int TUMESCENT = 40000;

	public const int MAX_WATER = 50000;

	public int CookCount;

	public int Water = 30000;

	public int RegenCounter;

	[NonSerialized]
	private static Event eCalculatingThirst = new Event("CalculatingThirst", "Amount", 0);

	[NonSerialized]
	private static Event eRegenerating = new Event("Regenerating", "Amount", 0);

	public int HungerLevel;

	public int _CookingCounter;

	public const int CookingIncrement = 1200;

	[NonSerialized]
	private static List<GameObject> containers = new List<GameObject>();

	public int HitCounter;

	public int WasOnWorldMap;

	public int CookingCounter
	{
		get
		{
			return _CookingCounter;
		}
		set
		{
			_CookingCounter = value;
			UpdateHunger();
		}
	}

	public void GetHungry()
	{
		CookingCounter = CalculateCookingIncrement();
		UpdateHunger();
	}

	public int CalculateCookingIncrement()
	{
		int num = 1200;
		if (ParentObject.HasSkill("Discipline_FastingWay"))
		{
			num *= 2;
		}
		if (ParentObject.HasSkill("Discipline_MindOverBody"))
		{
			num *= 6;
		}
		return num;
	}

	public string FoodStatus()
	{
		if (HungerLevel == 0)
		{
			return "{{g|Sated}}";
		}
		if (HungerLevel == 1)
		{
			return "{{W|Hungry}}";
		}
		if (ParentObject.HasPart("PhotosyntheticSkin"))
		{
			return "{{R|Wilted!}}";
		}
		return "{{R|Famished!}}";
	}

	public string WaterStatus()
	{
		if (ParentObject.HasPart("Amphibious"))
		{
			if (Water <= 0)
			{
				return "{{R|Desiccated!}}";
			}
			if (Water <= 10000)
			{
				return "{{r|Dry}}";
			}
			if (Water <= 20000)
			{
				return "{{c|Moist}}";
			}
			if (Water <= 30000)
			{
				return "{{b|Wet}}";
			}
			return "{{B|Soaked}}";
		}
		if (Water <= 0)
		{
			return "{{R|Dehydrated!}}";
		}
		if (Water <= 10000)
		{
			return "{{r|Parched}}";
		}
		if (Water <= 20000)
		{
			return "{{Y|Thirsty}}";
		}
		if (Water <= 30000)
		{
			return "{{g|Quenched}}";
		}
		return "{{G|Tumescent}}";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID && ID != EnteredCellEvent.ID && ID != GetDebugInternalsEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (ParentObject.IsPlayer())
		{
			if (!ParentObject.OnWorldMap())
			{
				if (WasOnWorldMap > 0)
				{
					if (WasOnWorldMap > 2 && HungerLevel < 1)
					{
						CookingCounter = CalculateCookingIncrement();
						ParentObject.FireEvent("BecameHungry");
					}
					WasOnWorldMap = 0;
				}
				else
				{
					ParentObject.SetLongProperty("OnWorldMapSince", XRLCore.CurrentTurn);
				}
			}
			else
			{
				WasOnWorldMap++;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		bool flag = false;
		if (ParentObject.IsPlayer())
		{
			if (!ParentObject.OnWorldMap())
			{
				if (WasOnWorldMap > 0)
				{
					WasOnWorldMap = 0;
					CookingCounter = CalculateCookingIncrement();
					int turns = (int)Math.Min(XRLCore.CurrentTurn - ParentObject.GetLongProperty("OnWorldMapSince"), 100000L);
					foreach (GameObject item in GameObject.findAll((GameObject who) => who.IsPlayerLed()))
					{
						if (item.GetPart("Stomach") is Stomach stomach && stomach.HitCounter <= 5)
						{
							stomach.ProcessNaturalHealing(turns);
						}
					}
				}
				CookingCounter++;
			}
			if (!ParentObject.HasEffect("Asleep"))
			{
				if (ParentObject.HasPart("FattyHump"))
				{
					if (Water > 0)
					{
						Water--;
					}
				}
				else if (ParentObject.Speed != 0)
				{
					int num = 1;
					if (ParentObject.HasPart("Discipline_FastingWay"))
					{
						num *= 2;
					}
					if (ParentObject.HasPart("Discipline_MindOverBody"))
					{
						num *= 6;
					}
					int value = ParentObject.Speed / (ParentObject.HasPart("Amphibious") ? 3 : 5) / num;
					eCalculatingThirst.SetParameter("Amount", value);
					ParentObject.FireEvent(eCalculatingThirst);
					value = eCalculatingThirst.GetIntParameter("Amount");
					Water -= value;
				}
				else
				{
					Water -= 20;
				}
				if (Water < 0)
				{
					Water = 0;
				}
			}
			if (Water < 0)
			{
				Water = 0;
			}
			if (Options.AutoSip)
			{
				int num2 = 20000;
				string autoSipLevel = Options.AutoSipLevel;
				if (!string.IsNullOrEmpty(autoSipLevel))
				{
					switch (autoSipLevel)
					{
					case "Dehydrated":
						num2 = 0;
						break;
					case "Parched":
						num2 = 10000;
						break;
					case "Thirsty":
						num2 = 20000;
						break;
					case "Quenched":
						num2 = 30000;
						break;
					case "Tumescent":
						num2 = 40000;
						break;
					}
				}
				if (Water < num2)
				{
					flag = true;
				}
			}
		}
		else
		{
			int num3 = 20000;
			if (Water < num3)
			{
				flag = true;
			}
		}
		if (flag && !ParentObject.IsFrozen() && ParentObject.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true))
		{
			containers.Clear();
			bool flag2 = ParentObject.HasPart("Amphibious");
			if (ParentObject.UseDrams(1, "water", null, null, null, containers, !flag2))
			{
				GameObject gameObject = ((containers.Count > 0) ? containers[0] : null);
				if (flag2)
				{
					if (gameObject == null)
					{
						DidXToY("pour", "fresh water over", ParentObject, null, null, null, ParentObject);
					}
					else
					{
						DidXToYWithZ("pour", "fresh water from", gameObject, "over", ParentObject, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, indefiniteDirectObject: false, indefiniteIndirectObject: false, indefiniteDirectObjectForOthers: true, indefiniteIndirectObjectForOthers: false, possessiveDirectObject: false, possessiveIndirectObject: false, null, ParentObject);
					}
					ParentObject.ApplyEffect(new LiquidCovered("water", 1));
				}
				else
				{
					if (gameObject == null)
					{
						DidX("take", "a sip of fresh water", null, null, ParentObject);
					}
					else
					{
						ParentObject.FireEvent(Event.New("DrinkingFrom", "Container", gameObject));
						DidXToY("take", "a sip of fresh water from", gameObject, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, ParentObject);
					}
					FireEvent(Event.New("AddWater", "Amount", 10000));
				}
			}
			else if (E.Traveling && !E.TravelMessagesSuppressed)
			{
				E.TravelMessagesSuppressed = true;
				if (Popup.ShowYesNo("You have run out of {{B|water}}! Do you want to stop travelling?") == DialogResult.Yes)
				{
					return false;
				}
			}
			containers.Clear();
		}
		bool flag3 = ProcessNaturalHealing();
		if (IsPlayer())
		{
			if (!flag3 && Stat.RollPenetratingSuccesses("1d" + ParentObject.Stat("Toughness"), 2) <= 0)
			{
				if (E.Traveling)
				{
					if (!E.TravelMessagesSuppressed)
					{
						E.TravelMessagesSuppressed = true;
						if (ParentObject.HasPart("Amphibious"))
						{
							if (Popup.ShowYesNo("You are drying out! Do you want to stop travelling?") == DialogResult.Yes)
							{
								return false;
							}
						}
						else if (Popup.ShowYesNo("You are dying of thirst! Do you want to stop travelling?") == DialogResult.Yes)
						{
							return false;
						}
					}
				}
				else if (ParentObject.HasPart("Amphibious"))
				{
					Popup.Show("You are drying out!");
				}
				else
				{
					Popup.Show("You are dying of thirst!");
				}
				ParentObject.pPhysics.LastDamagedBy = null;
				XRLCore.Core.Game.DeathReason = "You died of thirst.";
				ParentObject.GetStat("Hitpoints").Penalty += 2;
			}
			if (XRLCore.Core.IDKFA)
			{
				ParentObject.GetStat("Hitpoints").Penalty = 0;
				Water = 49000;
				ParentObject.pPhysics.Temperature = 25;
				ParentObject.UpdateVisibleStatusColor();
				Sidebar.Update();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		HitCounter = 10;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Water", Water);
		E.AddEntry(this, "HungerLevel", HungerLevel);
		E.AddEntry(this, "CookCount", CookCount);
		E.AddEntry(this, "CookingCounter", CookingCounter);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AddFood");
		Object.RegisterPartEvent(this, "AddWater");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AddWater")
		{
			int num = E.GetIntParameter("Amount");
			bool flag = E.HasFlag("Forced");
			bool flag2 = E.HasFlag("External");
			if (ParentObject.HasPart("Amphibious"))
			{
				if (num < 0 && IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("The moisture is sucked out of your body.");
				}
				if (!flag2)
				{
					num /= 10;
				}
				Water += num;
				if (Water > 50000)
				{
					Water = 50000;
				}
				else if (Water < 0)
				{
					Water = 0;
				}
			}
			else
			{
				if (num < 0 && IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("The moisture is sucked out of your throat.");
				}
				if (!flag && Water + num > 50000 && IsPlayer() && Popup.ShowYesNo("Drinking that much will probably make you sick, do you want to continue?") != 0)
				{
					return false;
				}
				Water += num;
				if (Water > 50000)
				{
					if (flag && ParentObject.Energy.Value <= 0 && ParentObject.GetLongProperty("VomitedOnTurn") > XRLCore.CurrentTurn - 3)
					{
						Water = 50000;
					}
					else
					{
						Water = Stat.Random(20000, 30000);
						if (IsPlayer())
						{
							Popup.Show("{{R|You drank way too much! You vomit!}}");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + " drank too much and" + ParentObject.GetVerb("vomit") + " everywhere!", 'R');
						}
						ParentObject.ApplyEffect(new LiquidCovered("putrid", 2));
						GameObject gameObject = GameObject.create("VomitPool");
						if (ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
						{
							ParentObject.CurrentCell.AddObject(gameObject);
						}
						else
						{
							gameObject.Obliterate();
						}
						ParentObject.SetLongProperty("VomitedOnTurn", XRLCore.CurrentTurn);
						ParentObject.UseEnergy(1000, "Vomit");
						E.RequestInterfaceExit();
					}
				}
				else if (Water < 0)
				{
					Water = 0;
				}
			}
			if (ParentObject.HasRegisteredEvent("AfterDrank"))
			{
				ParentObject.FireEvent(Event.New("AfterDrank", "Amount", num, "Forced", flag ? 1 : 0, "External", flag2 ? 1 : 0));
			}
			if (num < 0)
			{
				CheckCompanionThirstNotify(HighPriority: true);
			}
		}
		else if (E.ID == "AddFood")
		{
			bool flag3 = HungerLevel > 0;
			string stringParameter = E.GetStringParameter("Satiation", "Snack");
			bool flag4 = E.HasFlag("Meat");
			bool num2 = ParentObject.HasPart("Carnivorous");
			if (!num2 || flag4)
			{
				if (stringParameter == "Snack")
				{
					CookingCounter -= Food.smallHungerAmount;
				}
				else if (stringParameter == "Meal")
				{
					CookingCounter = 0;
				}
			}
			if (num2)
			{
				if (flag4)
				{
					if (flag3)
					{
						Campfire.RollTasty(10, bCarnivore: true);
					}
				}
				else if (Stat.Random(0, 1) == 0)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.Show("Ugh, you feel sick.");
					}
					ParentObject.ApplyEffect(new Ill(100, 1));
				}
			}
		}
		return base.FireEvent(E);
	}

	public bool IsFamished()
	{
		return HungerLevel >= 2;
	}

	public void UpdateHunger()
	{
		int num = CalculateCookingIncrement();
		if (CookingCounter < num || ParentObject.HasPropertyOrTag("Robot"))
		{
			if (HungerLevel != 0)
			{
				ParentObject.RemoveEffect("Famished");
				HungerLevel = 0;
				CookCount = 0;
			}
		}
		else if (CookingCounter >= num * 2)
		{
			if (HungerLevel == 2)
			{
				return;
			}
			HungerLevel = 2;
			if (!ParentObject.HasEffect("Famished") && !ParentObject.HasPart("Discipline_MindOverBody"))
			{
				ParentObject.ApplyEffect(new Famished());
				if (IsPlayer())
				{
					if (ParentObject.HasPart("PhotosyntheticSkin"))
					{
						Popup.Show("You have wilted! You'll move and regenerate slower until you eat or bask in the sunlight again.");
					}
					else
					{
						Popup.Show("You are famished! You'll move slower until you eat again.");
					}
					if (AutoAct.IsInterruptable())
					{
						AutoAct.Interrupt();
					}
				}
			}
			ParentObject.FireEvent("BecameFamished");
			CookCount = 0;
		}
		else
		{
			if (CookingCounter < num || HungerLevel == 1)
			{
				return;
			}
			if (IsPlayer())
			{
				if (!Prefs.HasString("Tutorial_WasEverHungry"))
				{
					Popup.Show("You're hungry. Make a campfire to cook.\nPress {{W|a}}, choose {{W|Make Camp}}, then interact with the campfire.");
					Prefs.SetString("Tutorial_WasEverHungry", "true");
					if (AutoAct.IsInterruptable())
					{
						AutoAct.Interrupt();
					}
				}
				else if (AutoAct.IsInterruptable())
				{
					AutoAct.Interrupt("you are hungry");
				}
			}
			ParentObject.RemoveEffect("Famished");
			HungerLevel = 1;
			ParentObject.FireEvent("BecameHungry");
			CookCount = 0;
		}
	}

	public bool ProcessNaturalHealing(int turns = 1)
	{
		if (HitCounter > 0)
		{
			HitCounter -= turns;
		}
		bool flag = ParentObject.HasPart("Regeneration");
		if (HitCounter <= 0 || flag)
		{
			int value = (20 + 2 * ParentObject.StatMod("Toughness") + 2 * ParentObject.StatMod("Willpower")) * turns;
			eRegenerating.ID = "Regenerating";
			eRegenerating.SetParameter("Amount", value);
			ParentObject.FireEvent(eRegenerating);
			eRegenerating.ID = "Regenerating2";
			ParentObject.FireEvent(eRegenerating);
			value = eRegenerating.GetIntParameter("Amount");
			if (value < 0)
			{
				value = 0;
			}
			if (HitCounter > 0)
			{
				value /= 2;
			}
			if (ParentObject.HasEffect("Meditating"))
			{
				value *= 3;
			}
			if (ParentObject.HasPart("LuminousInfection") && IsDay() && ParentObject.CurrentZone != null && ParentObject.CurrentZone.Z <= 10)
			{
				value = value * 85 / 100;
			}
			RegenCounter += value;
			if (RegenCounter > 100)
			{
				int num = (int)Math.Floor((double)RegenCounter / 100.0);
				RegenCounter %= 100;
				if (Water > 0)
				{
					ParentObject.GetStat("Hitpoints").Penalty -= num;
					ParentObject.UpdateVisibleStatusColor();
					if (ParentObject.IsPlayer())
					{
						Sidebar.Update();
					}
				}
				else
				{
					if (IsPlayer())
					{
						return false;
					}
					CheckCompanionThirstNotify();
				}
			}
		}
		return true;
	}

	public void CheckCompanionThirstNotify(bool HighPriority = false)
	{
		if (Water <= 0 && !ParentObject.IsPlayer() && !ParentObject.IsTrifling && ParentObject.IsPlayerLed() && IComponent<GameObject>.Visible(ParentObject) && XRLCore.Core.Game.Turns - ParentObject.GetLongProperty("LastCompanionThirstNotify") >= 100 && ParentObject.GetFreeDrams() < 1)
		{
			string message = "{{R|" + ParentObject.T() + ParentObject.Is + " dehydrated and will be unable to heal naturally until " + ParentObject.it + ParentObject.GetVerb("get", PrependSpace: true, PronounAntecedent: true) + " water to " + (ParentObject.HasPart("Amphibious") ? ("douse " + ParentObject.itself + " with") : "drink") + ".}}";
			if (HighPriority)
			{
				Popup.Show(message);
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage(message);
			}
			ParentObject.SetLongProperty("LastCompanionThirstNotify", XRLCore.Core.Game.Turns);
		}
	}
}
