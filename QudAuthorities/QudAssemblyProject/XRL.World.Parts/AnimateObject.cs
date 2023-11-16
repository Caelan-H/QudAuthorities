using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class AnimateObject : IPoweredPart
{
	public AnimateObject()
	{
		WorksOnSelf = true;
		ChargeUse = 5000;
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
		E.AddAction("Activate", "activate", "ActivateAnimateObject", null, 'a');
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateAnimateObject")
		{
			Cell cell = null;
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				cell = E.Actor.pPhysics.PickDirection();
			}
			if (cell != null)
			{
				List<GameObject> list = new List<GameObject>();
				List<string> list2 = new List<string>();
				char c = 'a';
				List<char> list3 = new List<char>();
				foreach (GameObject @object in cell.Objects)
				{
					if (CanBeAnimated(@object))
					{
						list.Add(@object);
						list2.Add(@object.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true));
						list3.Add((c <= 'z') ? c++ : ' ');
					}
				}
				if (list.Count == 0)
				{
					Popup.Show("There's nothing viable to animate here.");
				}
				else
				{
					int num = Popup.ShowOptionList("Choose an object to animate.", list2.ToArray(), list3.ToArray(), 0, null, 60, RespectOptionNewlines: false, AllowEscape: true);
					if (num >= 0)
					{
						GameObject gameObject = list[num];
						if (gameObject.HasPart("Brain"))
						{
							if (E.Actor.IsPlayer())
							{
								Popup.Show("You can't animate an object that already has a brain.");
							}
						}
						else if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
						{
							if (E.Actor.IsPlayer())
							{
								Popup.Show(ParentObject.Does("are") + " unresponsive.");
							}
						}
						else
						{
							E.Actor.UseEnergy(1000, "Item Animate");
							if (E.Actor.IsPlayer())
							{
								Popup.Show("You imbue " + gameObject.t() + " with life.");
								JournalAPI.AddAccomplishment("You imbued " + gameObject.an() + " with life. Why?", "While traveling in " + Grammar.GetProsaicZoneName(The.Player.CurrentZone) + ", =name= performed a sacred ritual with " + gameObject.a + gameObject.ShortDisplayName + ", imbuing " + gameObject.them + " with life and arranging " + gameObject.them + " " + HistoricStringExpander.ExpandString("<spice.elements." + IComponent<GameObject>.ThePlayerMythDomain + ".babeTrait.!random>") + ". Many of the local denizens declared it a miracle. Some weren't so sure.", "general", JournalAccomplishment.MuralCategory.CommitsFolly, JournalAccomplishment.MuralWeight.Medium, null, -1L);
							}
							Animate(gameObject, E.Actor, ParentObject);
							E.RequestInterfaceExit();
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public static bool CanAnimate(GameObject frankenObject)
	{
		return frankenObject.HasTagOrProperty("Animatable");
	}

	public static void Animate(GameObject frankenObject, GameObject Actor = null, GameObject Using = null)
	{
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint("Creature");
		frankenObject.pPhysics.Category = "Creature";
		frankenObject.pPhysics.Solid = false;
		frankenObject.pPhysics.Takeable = false;
		frankenObject.SetIntProperty("Creature", 1);
		frankenObject.SetIntProperty("UseFullDisplayNameForCreatureType", 1);
		frankenObject.pRender.RenderIfDark = false;
		foreach (KeyValuePair<string, Statistic> stat in blueprint.Stats)
		{
			if (!frankenObject.HasStat(stat.Key))
			{
				Statistic statistic = new Statistic(stat.Value);
				frankenObject.Statistics.Add(stat.Key, statistic);
				statistic.Owner = frankenObject;
			}
		}
		frankenObject.RequirePart<Brain>();
		frankenObject.RequirePart<AnimatedObject>();
		frankenObject.pBrain.Wanders = true;
		frankenObject.pBrain.FactionMembership.Clear();
		frankenObject.pBrain.FactionMembership.Add("Newly Sentient Beings", 100);
		if (frankenObject.Body == null)
		{
			frankenObject.AddPart(new Body()).Anatomy = frankenObject.GetTag("BodyType", "Humanoid");
		}
		frankenObject.RequirePart<Combat>();
		frankenObject.RequirePart<Experience>();
		frankenObject.RequirePart<Inventory>();
		frankenObject.RequirePart<Mutations>();
		frankenObject.RequirePart<Skills>();
		frankenObject.RequirePart<ActivatedAbilities>();
		frankenObject.RequirePart<Leveler>();
		frankenObject.RequirePart<Stomach>();
		frankenObject.RequirePart<Corpse>();
		if (!frankenObject.HasPart("ConversationScript"))
		{
			frankenObject.AddPart(new ConversationScript("NewlySentientBeings"));
		}
		if (frankenObject.GetIntProperty("ForceEffects") == 0)
		{
			frankenObject.SetIntProperty("ForceEffects", 1);
		}
		frankenObject.RemoveIntProperty("Anchoring");
		string propertyOrTag = frankenObject.GetPropertyOrTag("AnimatedSkills");
		if (!string.IsNullOrEmpty(propertyOrTag))
		{
			string[] array = propertyOrTag.Split(',');
			foreach (string @class in array)
			{
				frankenObject.AddSkill(@class);
			}
		}
		if (frankenObject.GetGender().Subjective == "it")
		{
			frankenObject.SetGender("nonspecific");
		}
		if (50.in100())
		{
			frankenObject.SetPronounSet("any");
		}
		else if (50.in100())
		{
			frankenObject.SetPronounSet("generate");
		}
		if (frankenObject.HasTag("AnimatedInventoryPopulationTable"))
		{
			frankenObject.TakePopulation(frankenObject.GetTag("AnimatedInventoryPopulationTable"));
		}
		AnimateEvent.Send(Actor, frankenObject, Using);
		if (!frankenObject.IsNowhere())
		{
			frankenObject.MakeActive();
		}
		if (Actor != null && Actor.IsPlayer())
		{
			AchievementManager.IncrementAchievement("ACH_BESTOW_LIFE_20");
		}
	}

	public static bool CanBeAnimated(GameObject obj)
	{
		if (!obj.HasTagOrProperty("Animatable"))
		{
			return false;
		}
		if (obj.GetMatterPhase() != 1)
		{
			return false;
		}
		if (obj.IsTemporary)
		{
			return false;
		}
		return true;
	}
}
