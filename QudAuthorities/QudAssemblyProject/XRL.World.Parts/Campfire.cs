using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using UnityEngine;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Skill;
using XRL.World.Skills.Cooking;

namespace XRL.World.Parts;

[Serializable]
public class Campfire : IActivePart
{
	public const string COOKING_PREFIX = "ProceduralCookingIngredient_";

	public string ExtinguishBlueprint;

	public string PresetMeals = "";

	public List<CookingRecipe> specificProcgenMeals;

	[NonSerialized]
	protected List<CookingRecipe> _presetMeals;

	public static readonly string COOK_COMMAND_RECIPE = "CookFromRecipe";

	public static readonly string COOK_COMMAND_WHIP_UP = "CookWhipUp";

	public static readonly string COOK_COMMAND_CHOOSE = "CookChooseIngredients";

	public static readonly string COOK_COMMAND_PRESERVE = "CookPreserve";

	public static readonly string COOK_COMMAND_PRESERVE_EXOTIC = "CookPreserveExotic";

	public static readonly string COOK_COMMAND_PRESET_MEAL = "CookPresetMeal:";

	public List<CookingRecipe> presetMeals => _presetMeals ?? (_presetMeals = ParsePresetMeals());

	public static Stomach pStomach => IComponent<GameObject>.ThePlayer.GetPart<Stomach>();

	public static bool hasSkill => IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering");

	public Campfire()
	{
		MarkParentTypeFieldsWithSaveVersion(typeof(IActivePart), 113);
		WorksOnSelf = true;
	}

	public Campfire(List<CookingRecipe> specificProcgenMeals)
		: this()
	{
		this.specificProcgenMeals = specificProcgenMeals;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID && ID != GetCookingActionsEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetPointsOfInterestEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == RadiatesHeatEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			string propertyOrTag = ParentObject.GetPropertyOrTag("PointOfInterestKey");
			bool flag = true;
			string explanation = null;
			if (!string.IsNullOrEmpty(propertyOrTag))
			{
				PointOfInterest pointOfInterest = E.Find(propertyOrTag);
				if (pointOfInterest != null)
				{
					if (ParentObject.DistanceTo(E.Actor) < pointOfInterest.GetDistanceTo(E.Actor))
					{
						E.Remove(pointOfInterest);
						explanation = "nearest";
					}
					else
					{
						flag = false;
						pointOfInterest.Explanation = "nearest";
					}
				}
			}
			if (flag)
			{
				E.Add(ParentObject, ParentObject.BaseDisplayName, explanation, propertyOrTag, null, null, null, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (!ParentObject.HasTagOrProperty("CampfireHeatSelfOnly") && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(ExtinguishBlueprint))
		{
			GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("LiquidVolume", CanExtinguish);
			if (firstObjectWithPart != null)
			{
				if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " extinguished by " + firstObjectWithPart.t() + ".");
				}
				ParentObject.ReplaceWith(ExtinguishBlueprint);
				return true;
			}
		}
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ParentObject.pPhysics != null && ParentObject.pPhysics.Temperature < 600 && Stat.Random5(1, 100) <= 10)
			{
				ParentObject.TemperatureChange(150, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, 5);
			}
			if (cell.Objects.Count > 1 && !ParentObject.HasTagOrProperty("CampfireHeatSelfOnly"))
			{
				int phase = ParentObject.GetPhase();
				int i = 0;
				for (int count = cell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = cell.Objects[i];
					if (gameObject != ParentObject)
					{
						Physics pPhysics = gameObject.pPhysics;
						if (pPhysics != null && pPhysics.Temperature < 600 && (!gameObject.HasPart("Combat") || Stat.Random5(1, 100) <= 10))
						{
							gameObject.TemperatureChange(150, ParentObject, Radiant: false, MinAmbient: false, MaxAmbient: false, phase);
						}
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(ExtinguishBlueprint) && ParentObject.IsFrozen())
		{
			ParentObject.ReplaceWith(ExtinguishBlueprint);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Cook", "cook", "Cook", null, 'c', FireOnActor: false, 10);
		if (!string.IsNullOrEmpty(ExtinguishBlueprint))
		{
			E.AddAction("Extinguish", "extinguish", "ExtinguishCampfire", null, 'x', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	protected List<CookingRecipe> ParsePresetMeals()
	{
		List<CookingRecipe> list = new List<CookingRecipe>();
		if (!string.IsNullOrEmpty(PresetMeals))
		{
			string[] array = PresetMeals.Split(',');
			foreach (string text in array)
			{
				CookingRecipe item = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Skills.Cooking." + text)) as CookingRecipe;
				list.Add(item);
			}
		}
		else if (specificProcgenMeals != null)
		{
			list.AddRange(specificProcgenMeals);
		}
		return list;
	}

	public static string EnabledDisplay(bool Enabled, string Display)
	{
		return (Enabled ? "" : "&K") + Display;
	}

	public override bool HandleEvent(GetCookingActionsEvent E)
	{
		bool flag = CookingGamestate.instance.knownRecipies.Count > 0;
		bool flag2 = IsHungry(The.Player);
		bool flag3 = IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering");
		List<GameObject> inventoryDirect = IComponent<GameObject>.ThePlayer.GetInventoryDirect((GameObject go) => go.HasPart("PreservableItem") && !go.HasTag("ChooseToPreserve") && !go.IsTemporary && !go.IsImportant());
		List<GameObject> inventoryDirect2 = IComponent<GameObject>.ThePlayer.GetInventoryDirect((GameObject go) => go.HasPart("PreservableItem") && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood());
		if (presetMeals.Count > 0)
		{
			for (int i = 0; i < presetMeals.Count; i++)
			{
				CookingRecipe cookingRecipe = presetMeals[i];
				string display = ParentObject.GetTag("PresetMealMessage") ?? ("Eat " + cookingRecipe?.GetDisplayName().Replace("&W", "&Y").Replace("{{W|", "{{Y|"));
				E.AddAction(COOK_COMMAND_PRESET_MEAL + i, Command: COOK_COMMAND_PRESET_MEAL + i, Key: (i >= 5) ? ' ' : ((char)(97 + i)), Display: EnabledDisplay(flag2, display), PreferToHighlight: null, FireOnActor: false, Default: 100 + presetMeals.Count - i, Priority: 100 + presetMeals.Count - i);
			}
		}
		E.AddAction(COOK_COMMAND_WHIP_UP, Command: COOK_COMMAND_WHIP_UP, Display: EnabledDisplay(flag2, "Whip up a meal."), PreferToHighlight: null, Key: 'm', FireOnActor: false, Default: 10, Priority: 90);
		E.AddAction(COOK_COMMAND_CHOOSE, EnabledDisplay(flag3 && flag2, "Choose ingredients to cook with."), COOK_COMMAND_CHOOSE, null, 'i', FireOnActor: false, 0, 80);
		E.AddAction(COOK_COMMAND_RECIPE, EnabledDisplay(flag3 && flag && flag2, "Cook from a recipe."), COOK_COMMAND_RECIPE, null, 'r', FireOnActor: false, 0, 70);
		E.AddAction(COOK_COMMAND_PRESERVE, EnabledDisplay(flag3 && inventoryDirect.Count > 0, "Preserve your fresh foods."), COOK_COMMAND_PRESERVE, null, 'f', FireOnActor: false, 0, 60);
		if (flag3 && inventoryDirect2.Count > 0)
		{
			E.AddAction(COOK_COMMAND_PRESERVE_EXOTIC, EnabledDisplay(Enabled: true, "Preserve your exotic foods."), COOK_COMMAND_PRESERVE_EXOTIC, null, 'x', FireOnActor: false, 0, 50);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Cook")
		{
			if (!Cook())
			{
				return false;
			}
		}
		else if (E.Command == "ExtinguishCampfire")
		{
			if (!string.IsNullOrEmpty(ExtinguishBlueprint) && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, "extinguish", ParentObject);
				ParentObject.ReplaceWith(GameObject.create(ExtinguishBlueprint));
			}
		}
		else
		{
			if (E.Command == COOK_COMMAND_WHIP_UP)
			{
				if (CookFromIngredients(random: true))
				{
					E.RequestInterfaceExit();
				}
				return true;
			}
			if (E.Command == COOK_COMMAND_CHOOSE)
			{
				if (CookFromIngredients(random: false))
				{
					E.RequestInterfaceExit();
				}
				return true;
			}
			if (E.Command == COOK_COMMAND_RECIPE)
			{
				if (CookFromRecipe())
				{
					E.RequestInterfaceExit();
				}
				return true;
			}
			if (E.Command == COOK_COMMAND_PRESERVE)
			{
				Preserve();
				return true;
			}
			if (E.Command == COOK_COMMAND_PRESERVE_EXOTIC)
			{
				PreserveExotic();
				return true;
			}
			if (E.Command.StartsWith(COOK_COMMAND_PRESET_MEAL))
			{
				int index = Convert.ToInt32(E.Command.Substring(COOK_COMMAND_PRESET_MEAL.Length));
				CookPresetMeal(index);
				E.RequestInterfaceExit();
				return true;
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanSmartUse");
		Object.RegisterPartEvent(this, "CommandSmartUse");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanSmartUse")
		{
			return false;
		}
		if (E.ID == "CommandSmartUse" && Cook())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool IsHalfIngredient(string type)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type];
		if (string.IsNullOrEmpty(gameObjectBlueprint.GetTag("Triggers")) && string.IsNullOrEmpty(gameObjectBlueprint.GetTag("Actions")))
		{
			return true;
		}
		return false;
	}

	public static bool HasTriggers(string type)
	{
		if (string.IsNullOrEmpty(GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Triggers")))
		{
			return false;
		}
		return true;
	}

	public static bool HasActions(string type)
	{
		if (string.IsNullOrEmpty(GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Actions")))
		{
			return false;
		}
		return true;
	}

	public static bool HasUnits(string type)
	{
		if (string.IsNullOrEmpty(GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type].GetTag("Units")))
		{
			return false;
		}
		return true;
	}

	public ProceduralCookingEffect GenerateEffectFromTypeList(List<string> selectedIngredientTypes)
	{
		ProceduralCookingEffect result = null;
		IEnumerable<string[]> enumerable = Algorithms.GeneratePermutations(selectedIngredientTypes);
		List<List<string>> list = new List<List<string>>();
		foreach (string[] item2 in enumerable)
		{
			List<string> list2 = new List<string>();
			string[] array = item2;
			foreach (string item in array)
			{
				list2.Add(item);
			}
			list.Add(list2);
		}
		list.Add(null);
		list.ShuffleInPlace();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] != null && list[j].Count == 0)
			{
				throw new InvalidOperationException("There weren't any types in the selected ingredient type permutations? (should be impossible)");
			}
			if (list[j] == null || list[j].Count == 1)
			{
				result = ProceduralCookingEffect.CreateJustUnits(selectedIngredientTypes);
				break;
			}
			if (list[j].Count == 2)
			{
				if (HasTriggers(list[j][0]) && HasActions(list[j][1]))
				{
					result = ProceduralCookingEffect.CreateTriggeredAction(list[j][0], list[j][1]);
					break;
				}
			}
			else if (HasUnits(list[j][0]) && HasTriggers(list[j][1]) && HasActions(list[j][2]))
			{
				result = ProceduralCookingEffect.CreateBaseAndTriggeredAction(list[j][0], list[j][1], list[j][2]);
				break;
			}
		}
		return result;
	}

	public List<ProceduralCookingEffect> GenerateEffectsFromTypeList(List<string> selectedIngredientTypes, int n)
	{
		List<ProceduralCookingEffect> list = new List<ProceduralCookingEffect>();
		IEnumerable<string[]> enumerable = Algorithms.GeneratePermutations(selectedIngredientTypes);
		List<List<string>> list2 = new List<List<string>>();
		foreach (string[] item2 in enumerable)
		{
			List<string> list3 = new List<string>();
			string[] array = item2;
			foreach (string item in array)
			{
				list3.Add(item);
			}
			list2.Add(list3);
		}
		list2.Add(null);
		list2.ShuffleInPlace();
		for (int j = 0; j < list2.Count; j++)
		{
			if (list2[j] != null && list2[j].Count == 0)
			{
				throw new InvalidOperationException("There weren't any types in the selected ingredient type permutations? (should be impossible)");
			}
			if (list2[j] == null || list2[j].Count == 1)
			{
				ProceduralCookingEffect effect = ProceduralCookingEffect.CreateJustUnits(selectedIngredientTypes);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect)))
				{
					list.Add(effect);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
			else if (list2[j].Count == 2)
			{
				if (!HasTriggers(list2[j][0]) || !HasActions(list2[j][1]))
				{
					continue;
				}
				ProceduralCookingEffectWithTrigger effect2 = ProceduralCookingEffect.CreateTriggeredAction(list2[j][0], list2[j][1]);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect2)))
				{
					list.Add(effect2);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
			else
			{
				if (!HasUnits(list2[j][0]) || !HasTriggers(list2[j][1]) || !HasActions(list2[j][2]))
				{
					continue;
				}
				ProceduralCookingEffectWithTrigger effect3 = ProceduralCookingEffect.CreateBaseAndTriggeredAction(list2[j][0], list2[j][1], list2[j][2]);
				if (!list.Any((ProceduralCookingEffect e) => e.SameAs(effect3)))
				{
					list.Add(effect3);
					n--;
					if (n <= 0)
					{
						return list;
					}
				}
			}
		}
		return list;
	}

	private static void MakeFromStoredByPlayer(GameObject obj)
	{
		obj.SetIntProperty("FromStoredByPlayer", 1);
	}

	public static bool PerformPreserve(GameObject go, StringBuilder sb, GameObject who, bool Capitalize = true, bool Single = false)
	{
		bool num = go.GetIntProperty("StoredByPlayer") > 0;
		Action<GameObject> afterObjectCreated = null;
		if (num)
		{
			afterObjectCreated = MakeFromStoredByPlayer;
		}
		int count = go.Count;
		if (Single && count > 1)
		{
			go = go.RemoveOne();
			count = go.Count;
		}
		if (count == 1)
		{
			sb.Append(Capitalize ? go.A : go.a).Append(go.DisplayNameOnlyDirect);
		}
		else if (go.IsPlural)
		{
			sb.Append(count).Append(' ').Append(go.DisplayNameOnlyDirect);
		}
		else
		{
			sb.Append(count).Append(' ').Append(Grammar.Pluralize(go.DisplayNameOnlyDirect));
		}
		sb.Append(" into ");
		string result = go.GetPart<PreservableItem>().Result;
		int num2 = 0;
		PreservableItem part = go.GetPart<PreservableItem>();
		PreparedCookingIngredient part2 = go.GetPart<PreparedCookingIngredient>();
		int num3 = 1;
		if (part2 != null)
		{
			num3 = part2.charges;
		}
		if (part != null)
		{
			num3 = part.Number;
		}
		num3 *= go.Count;
		who.TakeObject(part.Result, num3, Silent: true, 0, null, 0, 0, null, null, afterObjectCreated);
		num2 += num3;
		GameObject gameObject = GameObject.createSample(result);
		string tagOrStringProperty = gameObject.GetTagOrStringProperty("ServingType", "serving");
		string tagOrStringProperty2 = gameObject.GetTagOrStringProperty("ServingName", gameObject.DisplayNameOnlyDirect);
		sb.Append(num2).Append(' ').Append((num2 == 1) ? tagOrStringProperty : Grammar.Pluralize(tagOrStringProperty))
			.Append(" of ")
			.Append(tagOrStringProperty2);
		go.Obliterate();
		return true;
	}

	public static bool IsValidCookingIngredient(GameObject obj)
	{
		try
		{
			if (obj.IsTemporary && !obj.HasPropertyOrTag("CanCookTemporary"))
			{
				return false;
			}
			if (obj.IsImportant())
			{
				return false;
			}
			if (obj.HasPart("PreparedCookingIngredient"))
			{
				return true;
			}
			LiquidVolume liquidVolume = obj.LiquidVolume;
			if (liquidVolume != null && !liquidVolume.EffectivelySealed() && !string.IsNullOrEmpty(liquidVolume.GetPreparedCookingIngredient()) && obj.GetEpistemicStatus() != 0)
			{
				return true;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Campfire::IsValidCookingIngredient", x);
		}
		return false;
	}

	public static List<GameObject> GetValidCookingIngredients(GameObject who)
	{
		return who.GetInventoryDirectAndEquipment(IsValidCookingIngredient);
	}

	public static List<GameObject> GetValidCookingIngredients()
	{
		return GetValidCookingIngredients(IComponent<GameObject>.ThePlayer);
	}

	public static bool IsHungry(GameObject Object)
	{
		if (Object.GetPart("Stomach") is Stomach stomach && (stomach.HungerLevel > 0 || stomach.CookCount < 3))
		{
			return !Object.HasPropertyOrTag("Robot");
		}
		return false;
	}

	public bool Preserve()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		List<GameObject> inventoryDirect = IComponent<GameObject>.ThePlayer.GetInventoryDirect((GameObject go) => go.HasPart("PreservableItem") && !go.HasTag("ChooseToPreserve") && !go.IsTemporary && !go.IsImportant());
		if (inventoryDirect.Count == 0)
		{
			Popup.Show("You don't have anything to preserve.");
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("You preserved:\n\n");
		int num = 0;
		int i = 0;
		for (int count = inventoryDirect.Count; i < count; i++)
		{
			PerformPreserve(inventoryDirect[i], stringBuilder, IComponent<GameObject>.ThePlayer);
			stringBuilder.Append('.');
			if (i < count - 1)
			{
				stringBuilder.Append('\n');
			}
			num++;
		}
		Popup.Show(stringBuilder.ToString());
		return true;
	}

	public bool PreserveExotic()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		List<GameObject> inventoryDirect = IComponent<GameObject>.ThePlayer.GetInventoryDirect((GameObject go) => go.HasPart("PreservableItem") && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood());
		if (inventoryDirect.Count == 0)
		{
			Popup.Show("You don't have anything to preserve.");
			return false;
		}
		for (; inventoryDirect.Count != 0; inventoryDirect = IComponent<GameObject>.ThePlayer.GetInventoryDirect((GameObject go) => go.HasPart("PreservableItem") && go.HasTag("ChooseToPreserve") && !go.IsTemporary && go.Understood()))
		{
			StringBuilder stringBuilder = new StringBuilder();
			int defaultSelected = 0;
			bool[] array = new bool[inventoryDirect.Count];
			string[] array2 = new string[inventoryDirect.Count];
			IRenderable[] array3 = new IRenderable[inventoryDirect.Count];
			int num = 0;
			foreach (GameObject item in inventoryDirect)
			{
				array[num] = false;
				array2[num] = item.DisplayName;
				array3[num] = item.RenderForUI();
				num++;
			}
			IRenderable[] icons = array3;
			int num2 = Popup.ShowOptionList("Choose exotic foods to preserve.", array2, null, 0, null, 60, RespectOptionNewlines: false, AllowEscape: true, defaultSelected, "", null, null, icons);
			if (num2 < 0)
			{
				return true;
			}
			GameObject gameObject = inventoryDirect[num2];
			if (!gameObject.ConfirmUseImportant(null, "preserve"))
			{
				continue;
			}
			int num3 = 1;
			if (gameObject.Count > 1)
			{
				int? num4 = Popup.AskNumber(gameObject.DisplayNameOnlyDirect + ": how many do you want to preserve? (max = " + gameObject.Count + ")", gameObject.Count, 0, gameObject.Count);
				if (!num4.HasValue || num4 == 0)
				{
					continue;
				}
				try
				{
					num3 = Convert.ToInt32(num4);
					if (num3 > gameObject.Count)
					{
						num3 = gameObject.Count;
					}
				}
				catch
				{
					continue;
				}
				if (num3 <= 0)
				{
					continue;
				}
			}
			stringBuilder.Append("You preserved:\n\n");
			PerformPreserve(gameObject.Split(num3), stringBuilder, IComponent<GameObject>.ThePlayer);
			stringBuilder.Append('.');
			Popup.Show(stringBuilder.ToString());
		}
		return true;
	}

	public bool CookPresetMeal(int index)
	{
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		IComponent<GameObject>.PlayUISound("Human_Eating");
		Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
		IComponent<GameObject>.ThePlayer.FireEvent("ClearFoodEffects");
		IComponent<GameObject>.ThePlayer.CleanEffects();
		presetMeals[index].ApplyEffectsTo(IComponent<GameObject>.ThePlayer);
		pStomach.CookCount++;
		ClearHunger();
		IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
		return true;
	}

	public static int IngredientSort(GameObject a, GameObject b)
	{
		LiquidVolume liquidVolume = a.LiquidVolume;
		LiquidVolume liquidVolume2 = b.LiquidVolume;
		string text = liquidVolume?.GetPreparedCookingIngredient();
		string text2 = liquidVolume2?.GetPreparedCookingIngredient();
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			int num = ProducesLiquidEvent.Check(a, text).CompareTo(ProducesLiquidEvent.Check(b, text2));
			if (num != 0)
			{
				return -num;
			}
			int num2 = WantsLiquidCollectionEvent.Check(a, IComponent<GameObject>.ThePlayer, text).CompareTo(WantsLiquidCollectionEvent.Check(b, IComponent<GameObject>.ThePlayer, text2));
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = liquidVolume.Volume.CompareTo(liquidVolume2.Volume);
			if (num3 != 0)
			{
				return -num3;
			}
		}
		int num4 = a.HasTagOrProperty("WaterContainer").CompareTo(b.HasTagOrProperty("WaterContainer"));
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = a.Value.CompareTo(b.Value);
		if (num5 != 0)
		{
			return num5;
		}
		return a.Blueprint.CompareTo(b.Blueprint);
	}

	public bool CookFromIngredients(bool random)
	{
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		bool flag = pStomach.HungerLevel > 0;
		int bonus = 0;
		List<GameObject> validCookingIngredients = GetValidCookingIngredients();
		validCookingIngredients.Sort(IngredientSort);
		List<GameObject> list = new List<GameObject>(validCookingIngredients.Count);
		foreach (GameObject go in validCookingIngredients)
		{
			if (!IComponent<GameObject>.ThePlayer.HasPart("Carnivorous") || (!go.HasTag("Plant") && !go.HasTag("Fungus")))
			{
				string liquidIngredient1 = go.LiquidVolume?.GetPreparedCookingIngredient();
				if (!list.Exists(delegate(GameObject o)
				{
					string text2 = o.LiquidVolume?.GetPreparedCookingIngredient();
					return (!string.IsNullOrEmpty(liquidIngredient1) && !string.IsNullOrEmpty(text2)) ? (liquidIngredient1 == text2) : (o.GetCachedDisplayNameStripped() == go.GetCachedDisplayNameStripped());
				}))
				{
					list.Add(go);
				}
			}
		}
		foreach (GameObject item in validCookingIngredients)
		{
			item.ResetNameCache();
		}
		int chance = 0;
		int num = 3;
		List<string> list2 = new List<string>();
		List<GameObject> list3 = Event.NewGameObjectList();
		List<GameObject> list4 = Event.NewGameObjectList();
		if (!random && !IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering"))
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		if (random)
		{
			if (!IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering"))
			{
				List<string> list5 = new List<string>();
				list.ShuffleInPlace();
				foreach (GameObject item2 in list)
				{
					List<string> list6 = ((!item2.HasPart("PreparedCookingIngredient")) ? new List<string>(item2.LiquidVolume.GetPreparedCookingIngredient().CachedCommaExpansion()) : item2.GetPart<PreparedCookingIngredient>().GetTypeOptions());
					if (list6.Count <= 0)
					{
						continue;
					}
					list6.ShuffleInPlace();
					if (!list5.Contains(list6[0]) && chance.in100() && !list2.Contains(list6[0]))
					{
						list2.Add(list6[0]);
						list3.Add(item2);
						if (list2.Count >= num)
						{
							break;
						}
					}
					list5.Add(list6[0]);
				}
			}
		}
		else
		{
			int num2 = 2;
			if (IComponent<GameObject>.ThePlayer.HasSkill("CookingAndGathering_Spicer"))
			{
				num2++;
			}
			int defaultSelected = 0;
			List<bool> list7 = new List<bool>();
			int num3 = 0;
			list.Sort(new GameObject.DisplayNameSort());
			foreach (GameObject item3 in list)
			{
				_ = item3;
				list7.Add(item: false);
			}
			while (true)
			{
				List<string> list8 = new List<string>();
				List<IRenderable> list9 = new List<IRenderable>();
				string text = "C";
				if (num3 > num2)
				{
					text = "R";
				}
				list8.Add("{{W|Cook with the {{" + text + "|" + num3 + "}} selected ingredients.}}");
				list9.Add(null);
				string intro = ((num3 > 0) ? ("Selected {{" + text + "|" + num3 + "}} of " + num2 + " possible ingredients.") : "");
				int num4 = 0;
				foreach (GameObject item4 in list)
				{
					string displayName = item4.GetDisplayName(1120);
					if (list7[num4])
					{
						list8.Add("{{y|[{{G|X}}]}}   " + displayName);
					}
					else
					{
						list8.Add("[ ]   " + displayName);
					}
					list9.Add(item4.RenderForUI());
					num4++;
				}
				string[] options = list8.ToArray();
				IRenderable[] icons = list9.ToArray();
				int num5 = Popup.ShowOptionList("Choose ingredients to cook with.", options, null, 0, intro, 60, RespectOptionNewlines: false, AllowEscape: true, defaultSelected, "", null, null, icons, null, null, centerIntro: false, centerIntroIcon: true, 6);
				if (num5 < 0)
				{
					return false;
				}
				if (num5 > 0)
				{
					defaultSelected = num5;
					list7[num5 - 1] = !list7[num5 - 1];
					num3 = ((!list7[num5 - 1]) ? (num3 - 1) : (num3 + 1));
				}
				else if (num5 != 0 || num3 <= num2)
				{
					break;
				}
			}
			for (int i = 0; i < list7.Count; i++)
			{
				if (!list7[i])
				{
					continue;
				}
				GameObject gameObject = null;
				string type;
				if (list[i].HasPart("PreparedCookingIngredient"))
				{
					PreparedCookingIngredient part = list[i].GetPart<PreparedCookingIngredient>();
					type = list[i].GetPart<PreparedCookingIngredient>().GetTypeInstance();
					if (part.type == "random")
					{
						while (list2.Contains(type))
						{
							type = list[i].GetPart<PreparedCookingIngredient>().GetTypeInstance();
						}
						gameObject = EncountersAPI.GetAnObjectNoExclusions((GameObjectBlueprint ob) => (EncountersAPI.IsEligibleForDynamicEncounters(ob) && ob.HasPart("PreparedCookingIngredient") && ob.GetPartParameter("PreparedCookingIngredient", "type").Contains(type)) || (ob.HasTag("LiquidCookingIngredient") && ob.createSample().LiquidVolume.GetPreparedCookingIngredient().Contains(type)));
					}
				}
				else
				{
					type = list[i].LiquidVolume.GetPreparedCookingIngredient().Split(',').GetRandomElement();
				}
				if (!list2.Contains(type))
				{
					list2.Add(type);
				}
				list3.Add(list[i]);
				if (gameObject != null)
				{
					list4.Add(gameObject);
				}
				else
				{
					list4.Add(list[i]);
				}
			}
		}
		if (list2.Count > 0)
		{
			IComponent<GameObject>.ThePlayer.FireEvent("ClearFoodEffects");
			IComponent<GameObject>.ThePlayer.CleanEffects();
			ProceduralCookingEffect proceduralCookingEffect = null;
			if (!random && IComponent<GameObject>.ThePlayer.HasEffect("Inspired"))
			{
				List<ProceduralCookingEffect> list10 = GenerateEffectsFromTypeList(list2, 3);
				List<string> list11 = new List<string>();
				foreach (ProceduralCookingEffect item5 in list10)
				{
					list11.Add(ProcessEffectDescription(item5.GetTemplatedProceduralEffectDescription(), IComponent<GameObject>.ThePlayer));
				}
				Popup.Show(DescribeMeal(list2, list3));
				if (flag)
				{
					if (!RollTasty(bonus, IComponent<GameObject>.ThePlayer.HasPart("Carnivorous"), ForceTastyBasedOnIngredients(list2)))
					{
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				}
				IComponent<GameObject>.PlayUISound("HarmonicaRiff_GoodFood_3");
				int index = Popup.ShowOptionList("You let inspiration guide you toward a mouthwatering dish.", list11.ToArray(), null, 1);
				proceduralCookingEffect = list10[index];
				CookingRecipe cookingRecipe = CookingRecipe.FromIngredients(list4, proceduralCookingEffect, IComponent<GameObject>.ThePlayer.BaseDisplayName);
				CookingGamestate.LearnRecipe(cookingRecipe);
				Popup.Show("You create a new recipe for {{|" + cookingRecipe.GetDisplayName() + "}}!");
				JournalAPI.AddAccomplishment("You invented a mouthwatering dish called {{|" + cookingRecipe.GetDisplayName() + "}}.", "In a moment of divine inspiration, the Carbide Chef =name= invented the mouthwatering dish called {{|" + cookingRecipe.GetDisplayName() + "}}.", "general", JournalAccomplishment.MuralCategory.CreatesSomething, JournalAccomplishment.MuralWeight.Low, null, -1L);
				AchievementManager.IncrementAchievement("ACH_100_RECIPES");
				IComponent<GameObject>.ThePlayer.RemoveEffect("Inspired");
				IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
			}
			else
			{
				proceduralCookingEffect = GenerateEffectFromTypeList(list2);
				Popup.Show(DescribeMeal(list2, list3));
				if (flag)
				{
					if (!RollTasty(bonus, IComponent<GameObject>.ThePlayer.HasPart("Carnivorous"), ForceTastyBasedOnIngredients(list2)))
					{
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				}
				IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
			}
			pStomach.CookCount++;
			ClearHunger();
			if (proceduralCookingEffect != null)
			{
				proceduralCookingEffect.Init(IComponent<GameObject>.ThePlayer);
				Popup.Show("You start to metabolize the meal, gaining the following effect for the rest of the day:\n\n&W" + ProcessEffectDescription(proceduralCookingEffect.GetProceduralEffectDescription(), IComponent<GameObject>.ThePlayer));
				proceduralCookingEffect.Duration = 1;
				IComponent<GameObject>.ThePlayer.ApplyEffect(proceduralCookingEffect);
			}
		}
		else
		{
			pStomach.CookCount++;
			ClearHunger();
			Popup.Show(DescribeMeal(list2, list3));
			if (flag)
			{
				if (!RollTasty(bonus, IComponent<GameObject>.ThePlayer.HasPart("Carnivorous")))
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
				}
			}
			else
			{
				IComponent<GameObject>.PlayUISound("Human_Eating");
				Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
				IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
			}
		}
		Event e = Event.New("UsedAsIngredient", "Actor", The.Player);
		foreach (GameObject item6 in list3)
		{
			item6.FireEvent(e);
			if (item6.HasPart("PreparedCookingIngredient"))
			{
				PreparedCookingIngredient part2 = item6.GetPart<PreparedCookingIngredient>();
				if (part2.HasTag("AlwaysStack"))
				{
					item6.Destroy();
					continue;
				}
				item6.SplitFromStack();
				part2.charges--;
				if (part2.charges <= 0)
				{
					item6.Destroy();
				}
				else
				{
					item6.CheckStack();
				}
			}
			else if (item6.HasPart("LiquidVolume"))
			{
				item6.LiquidVolume.UseDram();
			}
		}
		return true;
	}

	public bool CookFromRecipe()
	{
		if (!hasSkill)
		{
			Popup.Show("You don't have the Cooking and Gathering skill.");
			return false;
		}
		if (!IsHungry(The.Player))
		{
			Popup.Show("You aren't hungry. Instead, you relax by the warmth of the fire.");
			return false;
		}
		bool flag = pStomach.HungerLevel > 0;
		int bonus = 0;
		CookingGamestate.ResetInventorySnapshot();
		bool flag2 = false;
		int defaultSelected = 0;
		while (true)
		{
			int num = 0;
			List<Tuple<string, CookingRecipe>> list = new List<Tuple<string, CookingRecipe>>();
			foreach (CookingRecipe knownRecipy in CookingGamestate.instance.knownRecipies)
			{
				if (!knownRecipy.Hidden && (!IComponent<GameObject>.ThePlayer.HasPart("Carnivorous") || (!knownRecipy.HasPlants() && !knownRecipy.HasFungi())))
				{
					if (!flag2 && !knownRecipy.CheckIngredients())
					{
						num++;
					}
					else
					{
						list.Add(new Tuple<string, CookingRecipe>(knownRecipy.GetCampfireDescription() + "\n\n", knownRecipy));
					}
				}
			}
			if (list.Count <= 0)
			{
				if (num > 0)
				{
					flag2 = true;
					continue;
				}
				Popup.Show("You don't know any recipes.");
				return false;
			}
			list.Sort((Tuple<string, CookingRecipe> a, Tuple<string, CookingRecipe> b) => ConsoleLib.Console.ColorUtility.CompareExceptFormattingAndCase(a.Item1, b.Item1));
			string text = "";
			if (num > 0)
			{
				text = text + "&K< " + num + " hidden for missing ingredients >";
				list.Add(new Tuple<string, CookingRecipe>("Show " + num + " hidden recipes missing ingredients", null));
			}
			int num2 = Popup.ShowOptionList("Choose a recipe", TupleUtilities<string, CookingRecipe>.GetFirstArray(list), null, 1, text, 72, RespectOptionNewlines: true, AllowEscape: true, defaultSelected, Popup.SPACING_DARK_LINE.Replace('=', '÷'));
			if (num2 >= 0 && num2 < list.Count && list[num2].Item2 == null)
			{
				flag2 = true;
				continue;
			}
			if (num2 < 0)
			{
				break;
			}
			defaultSelected = num2;
			while (true)
			{
				string text2 = "Add to favorite recipes";
				if (list[num2].Item2.Favorite)
				{
					text2 = "Remove from favorite recipes";
				}
				int num3 = Popup.ShowOptionList("", new string[4] { "Cook", text2, "Forget", "Back" }, null, 0, list[num2].Item2.GetCampfireDescription(), 72, RespectOptionNewlines: true, AllowEscape: true);
				if (num3 < 0 || num3 == 3)
				{
					break;
				}
				if (num3 == 1)
				{
					list[num2].Item2.Favorite = !list[num2].Item2.Favorite;
					continue;
				}
				if (num3 == 2)
				{
					if (Popup.ShowYesNo("Are you sure you want to forget this recipe?") == DialogResult.Yes)
					{
						list[num2].Item2.Hidden = true;
					}
					break;
				}
				if (!list[num2].Item2.CheckIngredients(displayMessage: true))
				{
					break;
				}
				List<GameObject> list2 = new List<GameObject>();
				if (!list[num2].Item2.UseIngredients(list2))
				{
					break;
				}
				IComponent<GameObject>.ThePlayer.FireEvent("ClearFoodEffects");
				IComponent<GameObject>.ThePlayer.CleanEffects();
				if (flag)
				{
					if (!RollTasty(bonus, IComponent<GameObject>.ThePlayer.HasPart("Carnivorous"), ForceTastyBasedOnIngredients(list2)))
					{
						IComponent<GameObject>.PlayUISound("Human_Eating");
						Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
						IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
					}
				}
				else
				{
					IComponent<GameObject>.PlayUISound("Human_Eating");
					Popup.Show(HistoricStringExpander.ExpandString("<spice.cooking.ate.!random>"));
					IComponent<GameObject>.ThePlayer.FireEvent(Event.New("CookedAt", "Object", ParentObject));
				}
				if (list[num2].Item2.ApplyEffectsTo(IComponent<GameObject>.ThePlayer))
				{
					pStomach.CookCount++;
					ClearHunger();
				}
				return true;
			}
		}
		return false;
	}

	public bool Cook()
	{
		if (!IComponent<GameObject>.ThePlayer.CheckFrozen())
		{
			return false;
		}
		if (IComponent<GameObject>.ThePlayer.AreHostilesNearby())
		{
			Popup.Show("You can't cook with hostile creatures nearby.");
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != 0)
		{
			switch (activePartStatus)
			{
			case ActivePartStatus.SwitchedOff:
				Popup.Show(ParentObject.Does("are") + " turned off.");
				break;
			case ActivePartStatus.Unpowered:
				Popup.Show(ParentObject.Does("do") + " not have enough charge to operate.");
				break;
			case ActivePartStatus.NotHanging:
				Popup.Show(ParentObject.Does("need") + " to be hung up first.");
				break;
			default:
				Popup.Show(ParentObject.Does("do") + " not seem to be working.");
				break;
			}
			return false;
		}
		if (!IComponent<GameObject>.ThePlayer.CanChangeMovementMode("Cooking", ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			UnityEngine.GameObject.Find("CampfireSounds").GetComponent<CampfireSounds>().Open();
		});
		try
		{
			InventoryAction inventoryAction;
			do
			{
				Dictionary<string, InventoryAction> dictionary = new Dictionary<string, InventoryAction>(16);
				GetCookingActionsEvent.SendToActorAndObject(IComponent<GameObject>.ThePlayer, ParentObject, dictionary);
				inventoryAction = EquipmentAPI.ShowInventoryActionMenu(dictionary, IComponent<GameObject>.ThePlayer, ParentObject, Distant: false, TelekineticOnly: false, "{{W|The fire breathes its warmth on your bones.}}", new InventoryAction.Comparer
				{
					priorityFirst = true
				});
				if (inventoryAction == null)
				{
					return true;
				}
			}
			while (!inventoryAction.Process(ParentObject, IComponent<GameObject>.ThePlayer).InterfaceExitRequested());
			return true;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Campfire", x);
		}
		finally
		{
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				UnityEngine.GameObject.Find("CampfireSounds").GetComponent<CampfireSounds>().Close();
			});
		}
		return true;
	}

	private static bool CanExtinguish(GameObject obj)
	{
		LiquidVolume liquidVolume = obj.LiquidVolume;
		if (liquidVolume.Volume >= 10)
		{
			return liquidVolume.IsOpenVolume();
		}
		return false;
	}

	public static GameObject FindExtinguishingPool(Cell C)
	{
		GameObject gameObject = C?.GetFirstObjectWithPart("LiquidVolume", CanExtinguish);
		if (gameObject == null)
		{
			return null;
		}
		if (C.HasBridge())
		{
			return null;
		}
		return gameObject;
	}

	public static GameObject FindExtinguishingPool(GameObject obj)
	{
		return FindExtinguishingPool(obj?.CurrentCell);
	}

	public GameObject FindExtinguishingPool()
	{
		return FindExtinguishingPool(ParentObject);
	}

	public void ClearHunger()
	{
		Stomach part = IComponent<GameObject>.ThePlayer.GetPart<Stomach>();
		if (part != null)
		{
			part.HungerLevel = 0;
			part.CookingCounter = 0;
		}
		IComponent<GameObject>.ThePlayer.RemoveEffect("Famished");
	}

	public static bool ForceTastyBasedOnIngredients(List<string> ingredients)
	{
		return ingredients.Contains("tastyMinor");
	}

	public static bool ForceTastyBasedOnIngredients(List<GameObject> ingredientObjects)
	{
		foreach (GameObject ingredientObject in ingredientObjects)
		{
			if (ingredientObject.GetPart("PreparedCookingIngredient") is PreparedCookingIngredient preparedCookingIngredient && preparedCookingIngredient.HasTypeOption("tastyMinor"))
			{
				return true;
			}
			LiquidVolume liquidVolume = ingredientObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.GetPreparedCookingIngredient().Contains("tastyMinor"))
			{
				return true;
			}
		}
		return false;
	}

	private static Effect RandomTastyEffect(string tastyMessage)
	{
		return Stat.Random(0, 6) switch
		{
			0 => new BasicCookingEffect_Hitpoints(tastyMessage), 
			1 => new BasicCookingEffect_MA(tastyMessage), 
			2 => new BasicCookingEffect_MS(tastyMessage), 
			3 => new BasicCookingEffect_Quickness(tastyMessage), 
			4 => new BasicCookingEffect_RandomStat(tastyMessage), 
			5 => new BasicCookingEffect_Regeneration(tastyMessage), 
			6 => new BasicCookingEffect_ToHit(tastyMessage), 
			_ => null, 
		};
	}

	public static bool RollTasty(int Bonus = 0, bool bCarnivore = false, bool bAlwaysSucceed = false)
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return false;
		}
		if (bAlwaysSucceed || (10 + Bonus).in100())
		{
			string tastyMessage = ((!bCarnivore) ? "You eat the meal. It's tastier than usual." : "You gorge on the succulent meat. It's tastier than usual.");
			IComponent<GameObject>.PlayUISound("Human_Eating_WithGulp");
			IComponent<GameObject>.ThePlayer.ApplyEffect(RandomTastyEffect(tastyMessage));
			return true;
		}
		return false;
	}

	public static string DescribeMeal(List<string> mealTypes, List<GameObject> mealObjects)
	{
		string text = "unknown";
		try
		{
			string objectTypeForZone = ZoneManager.GetObjectTypeForZone(XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.ParentZone.ZoneID);
			string tag = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("Terrain");
			string tag2 = GameObjectFactory.Factory.Blueprints[objectTypeForZone].GetTag("LairOwnerTable", "GenericLairOwner");
			int num = 0;
			GameObject gameObject;
			do
			{
				num++;
				string objectBlueprint = ((num >= 20) ? PopulationManager.RollOneFrom("LairOwners_Jungle").Blueprint : PopulationManager.RollOneFrom(tag2).Blueprint);
				gameObject = GameObjectFactory.Factory.CreateObject(objectBlueprint);
			}
			while (!Axe_Dismember.HasAnyDismemberableBodyPart(gameObject, IComponent<GameObject>.ThePlayer, null, assumeDecapitate: true));
			string displayNameOnlyStripped = gameObject.DisplayNameOnlyStripped;
			string ordinalName = Axe_Dismember.GetDismemberableBodyPart(gameObject, IComponent<GameObject>.ThePlayer).GetOrdinalName();
			string displayNameOnlyStripped2 = GameObjectFactory.Factory.CreateObject(EncountersAPI.GetARandomDescendentOf("Book")).DisplayNameOnlyStripped;
			string displayNameOnlyStripped3 = GameObjectFactory.Factory.CreateObject(EncountersAPI.GetARandomDescendentOf("Gas")).DisplayNameOnlyStripped;
			string displayNameOnlyDirect = GameObjectFactory.Factory.CreateObject(EncountersAPI.GetARandomDescendentOf("Tonic")).DisplayNameOnlyDirect;
			string text2 = "";
			int num2 = Stat.Random(3, 4);
			for (int i = 0; i <= num2 - 1; i++)
			{
				if (i < mealObjects.Count)
				{
					text = mealObjects[i].DebugName;
				}
				if (i == num2 - 1)
				{
					text2 += " and ";
				}
				else if (i > 0)
				{
					text2 += " ";
				}
				if (i <= mealObjects.Count - 1)
				{
					LiquidVolume liquidVolume = mealObjects[i].LiquidVolume;
					text2 = ((liquidVolume == null || !liquidVolume.HasPreparedCookingIngredient()) ? (text2 + mealObjects[i].an(int.MaxValue, null, "CookingIngredient", AsIfKnown: false, Single: true, NoConfusion: true)) : (text2 + "a dram of " + liquidVolume.GetLiquidName()));
				}
				else
				{
					text2 += HistoricStringExpander.ExpandString("<spice.cooking.ingredient.!random>", null, null, new Dictionary<string, string>
					{
						{ "$terrain", tag },
						{
							"$creaturePossessive",
							Grammar.MakePossessive(displayNameOnlyStripped)
						},
						{ "$creatureBodyPart", ordinalName },
						{
							"$bookName",
							"{{|" + displayNameOnlyStripped2 + "}}"
						},
						{
							"$gasName",
							"{{|" + displayNameOnlyStripped3 + "}}"
						},
						{
							"$tonicName",
							"{{|" + displayNameOnlyDirect + "}}"
						}
					});
				}
				if (i != num2 - 1)
				{
					text2 += ",";
				}
			}
			return Grammar.ConvertAtoAn(HistoricStringExpander.ExpandString("<spice.cooking.cookTemplate.!random>").Replace("*ingredients*", text2));
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			MetricsManager.LogError("meal description", ex);
			MetricsManager.LogError("meal description", "{invalid meal description: " + text + "}");
			return "{invalid meal description: " + text + "}";
		}
	}

	public static string ProcessEffectDescription(string description, GameObject go)
	{
		description = description.TrimEnd('\r', '\n');
		if (go == null || go.IsPlayer())
		{
			return Grammar.CapAfterNewlines(Grammar.InitCap(description.Replace("@thisCreature", "you").Replace("@s", "").Replace("@es", "")
				.Replace("@is", "are")
				.Replace("@they", "you")
				.Replace("@their", "your")
				.Replace("@them", "you")));
		}
		return Grammar.CapAfterNewlines(Grammar.InitCap(description.Replace("@thisCreature", go.IsPlural ? "these creatures" : "this creature").Replace("@s", go.IsPlural ? "" : "s").Replace("@es", go.IsPlural ? "" : "es")
			.Replace("@is", go.Is)
			.Replace("@they", go.it)
			.Replace("@their", go.its)
			.Replace("@them", go.them)));
	}
}
