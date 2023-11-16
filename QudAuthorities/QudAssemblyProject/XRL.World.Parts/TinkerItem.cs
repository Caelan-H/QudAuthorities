using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.Language;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
[WantLoadBlueprint]
[HasGameBasedStaticCache]
public class TinkerItem : IPart
{
	public bool CanDisassemble = true;

	public bool CanBuild;

	public int BuildTier = 1;

	public int NumberMade = 1;

	public string Ingredient = "";

	public string SubstituteBlueprint;

	public string RepairCost;

	public string RustedRepairCost;

	[NonSerialized]
	[GameBasedStaticCache]
	public static Dictionary<string, string> BitCostMap = new Dictionary<string, string>();

	[NonSerialized]
	private static BitCost Cost = new BitCost();

	public UnityEngine.GameObject[] objectsToDisable;

	public UnityEngine.GameObject[] objectsToEnable;

	public string Bits
	{
		get
		{
			if (BitCostMap.TryGetValue(ActiveBlueprint, out var value))
			{
				Cost.Clear();
				Cost.Import(value);
				if (GlobalConfig.GetBoolSetting("IncludeModBitsInItemBits"))
				{
					int intProperty = ParentObject.GetIntProperty("nMods");
					if (intProperty > 0)
					{
						int num = 0;
						List<IModification> partsDescendedFrom = ParentObject.GetPartsDescendedFrom<IModification>();
						int i = 0;
						for (int count = partsDescendedFrom.Count; i < count; i++)
						{
							IModification modification = partsDescendedFrom[i];
							if (!ModificationFactory.ModsByPart.TryGetValue(modification.Name, out var value2))
							{
								continue;
							}
							int j = 0;
							for (int count2 = TinkerData.TinkerRecipes.Count; j < count2; j++)
							{
								TinkerData tinkerData = TinkerData.TinkerRecipes[j];
								if (tinkerData.DisplayName == value2.TinkerDisplayName && tinkerData.Type == "Mod")
								{
									Cost.Increment(BitType.TierBits[Tier.Constrain(tinkerData.Tier)]);
									num++;
									break;
								}
							}
						}
						int tier = ParentObject.GetTier();
						for (int k = intProperty - num; k < intProperty; k++)
						{
							Cost.Increment(BitType.TierBits[Tier.Constrain(tier + k)]);
						}
					}
				}
				ModifyBitCostEvent.Process(IComponent<GameObject>.ThePlayer, Cost, "Disassemble");
				return Cost.ToBits();
			}
			return "0";
		}
		set
		{
			if (!BitCostMap.ContainsKey(ActiveBlueprint))
			{
				BitCostMap.Add(ActiveBlueprint, BitType.ToRealBits(value, ActiveBlueprint));
			}
			for (int i = 0; i < Bits.Length; i++)
			{
				if (BitType.BitMap[Bits[i]].Level > BuildTier)
				{
					BuildTier = BitType.BitMap[Bits[i]].Level;
				}
			}
		}
	}

	public string ActiveBlueprint
	{
		get
		{
			if (!string.IsNullOrEmpty(SubstituteBlueprint))
			{
				return SubstituteBlueprint;
			}
			return ParentObject.Blueprint;
		}
		set
		{
			if (value == ParentObject.Blueprint)
			{
				SubstituteBlueprint = null;
			}
			else
			{
				SubstituteBlueprint = value;
			}
		}
	}

	public static string GetBitCostFor(string Blueprint)
	{
		if (!BitCostMap.TryGetValue(Blueprint, out var value))
		{
			value = GameObjectFactory.Factory.GetBlueprint(Blueprint)?.GetPartParameter("TinkerItem", "Bits");
			if (string.IsNullOrEmpty(value))
			{
				MetricsManager.LogError("Obtaining bit cost for invalid blueprint:" + Blueprint);
				return "1";
			}
			value = BitType.ToRealBits(value, Blueprint);
			BitCostMap.Add(Blueprint, value);
		}
		return value;
	}

	public override void LoadBlueprint()
	{
		if (CanBuild && string.IsNullOrEmpty(SubstituteBlueprint) && !ParentObject.HasTag("BaseObject"))
		{
			TinkerData tinkerData = new TinkerData();
			tinkerData.Blueprint = ParentObject.Blueprint;
			tinkerData.Cost = Bits;
			tinkerData.Tier = BuildTier;
			tinkerData.Type = "Build";
			tinkerData.Category = ParentObject.GetTag("TinkerCategory", "none");
			tinkerData.Ingredient = Ingredient;
			tinkerData.DisplayName = ParentObject.pRender.DisplayName;
			TinkerData.TinkerRecipes.Add(tinkerData);
		}
	}

	public static void SaveGlobals(SerializationWriter Writer)
	{
		Writer.Write(BitCostMap);
		Writer.Write(TinkerData.TinkerRecipes.Count);
		foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
		{
			tinkerRecipe.SaveData(Writer);
		}
		Writer.Write(TinkerData.KnownRecipes.Count);
		foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
		{
			knownRecipe.SaveData(Writer);
		}
	}

	public static void LoadGlobals(SerializationReader Reader)
	{
		BitCostMap = Reader.ReadDictionary<string, string>();
		int num = Reader.ReadInt32();
		TinkerData.TinkerRecipes = new List<TinkerData>(num);
		for (int i = 0; i < num; i++)
		{
			TinkerData.TinkerRecipes.Add(TinkerData.LoadData(Reader));
		}
		int num2 = Reader.ReadInt32();
		TinkerData.KnownRecipes = new List<TinkerData>(num2);
		for (int j = 0; j < num2; j++)
		{
			TinkerData.KnownRecipes.Add(TinkerData.LoadData(Reader));
		}
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void Awake()
	{
		objectsToEnable.FirstOrDefault(delegate(UnityEngine.GameObject o)
		{
			o.SetActive(value: true);
			return false;
		});
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsAlwaysEvent.ID && ID != GetScanTypeEvent.ID && ID != IdleQueryEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject && (CanBuild || CanDisassemble))
		{
			E.ScanType = Scanning.Scan.Tech;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if ((E.AsIfKnown || (IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.HasSkill("Tinkering_Disassemble"))) && CanDisassemble && 1160 < E.Cutoff && ParentObject != null && !ParentObject.HasPart("Combat") && E.Context != "Tinkering" && E.Understood() && E.Object != null && !E.Object.HasProperName)
		{
			E.AddTag("{{y|<{{|" + BitType.GetString(Bits) + "}}>}}", 60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsAlwaysEvent E)
	{
		if (E.Actor.IsPlayer() && CanBeDisassembled(E.Actor))
		{
			int @default = -1;
			if ((HasTag("DefaultDisassemble") || TinkeringHelpers.ConsiderScrap(ParentObject, E.Actor)) && !E.Object.IsImportant())
			{
				@default = 200;
			}
			E.AddAction("Disassemble", "disassemble", "Disassemble", null, 'm', FireOnActor: false, @default);
			E.AddAction("Disassemble All", "disassemble all", "DisassembleAll", null, 'm', FireOnActor: false, -1, -1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if ((E.Command == "Disassemble" || E.Command == "DisassembleAll") && E.Actor.IsPlayer() && CanBeDisassembled(E.Actor))
		{
			int count = ParentObject.Count;
			bool flag = E.Command == "DisassembleAll";
			bool flag2 = flag && count > 1;
			List<Action<GameObject>> list = null;
			if (ParentObject.InInventory != E.Actor && !E.Actor.InSameOrAdjacentCellTo(ParentObject))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.ShowFail("You need be near " + ParentObject.t() + " in order to disassemble " + ParentObject.them + ".");
				}
				return false;
			}
			if (E.Actor.IsPlayer())
			{
				if (flag && flag2 && AutoAct.ShouldHostilesInterrupt("o"))
				{
					Popup.ShowFail("You cannot use disassemble all with hostiles nearby.");
					return false;
				}
				if (ParentObject.IsInStasis())
				{
					Popup.ShowFail("You cannot seem to affect " + ParentObject.t() + " in any way.");
					return false;
				}
				if (!string.IsNullOrEmpty(ParentObject.Owner) && !ParentObject.HasPropertyOrTag("DontWarnOnDisassemble"))
				{
					if (Popup.ShowYesNoCancel(ParentObject.The + ParentObject.DisplayNameOnly + (flag2 ? "are" : ParentObject.Is) + " not owned by you. Are you sure you want to disassemble " + (flag2 ? "them" : ParentObject.them) + "?") != 0)
					{
						return false;
					}
					if (list == null)
					{
						list = new List<Action<GameObject>>();
					}
					list.Add(ParentObject.pPhysics.BroadcastForHelp);
				}
				if (E.Item.IsImportant())
				{
					if (!E.Item.ConfirmUseImportant(null, "disassemble", null, (!flag2) ? 1 : count))
					{
						return false;
					}
				}
				else if (ConfirmBeforeDisassembling(ParentObject) && Popup.ShowYesNoCancel("Are you sure you want to disassemble " + (flag2 ? ("all the " + (ParentObject.GetGender().Plural ? ParentObject.ShortDisplayName : Grammar.Pluralize(ParentObject.ShortDisplayName))) : ParentObject.t()) + "?") != 0)
				{
					return false;
				}
				GameObject inInventory = ParentObject.InInventory;
				if (inInventory != null && inInventory != E.Actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnDisassemble"))
				{
					if (Popup.ShowYesNoCancel(inInventory.The + inInventory.DisplayNameOnly + inInventory.Is + " not owned by you. Are you sure you want to disassemble " + (flag2 ? "items" : ParentObject.an()) + " inside " + inInventory.them + "?") != 0)
					{
						return false;
					}
					if (list == null)
					{
						list = new List<Action<GameObject>>();
					}
					list.Add(inInventory.pPhysics.BroadcastForHelp);
				}
			}
			Disassembly disassembly = new Disassembly(ParentObject, (!flag2) ? 1 : count, E.Auto, list);
			if (flag2 || E.Auto)
			{
				AutoAct.Action = disassembly;
				E.RequestInterfaceExit();
				E.Actor.ForfeitTurn(EnergyNeutral: true);
			}
			else
			{
				disassembly.EnergyCostPer = 0;
				disassembly.Continue();
				if (disassembly.CanComplete())
				{
					disassembly.Complete();
				}
				else
				{
					disassembly.Interrupt();
				}
				disassembly.End();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.CurrentCell != null && E.Actor.Owns(ParentObject) && Tinkering_Repair.IsRepairableBy(ParentObject, E.Actor))
		{
			GameObject who = E.Actor;
			who.pBrain.PushGoal(new DelegateGoal(delegate(GoalHandler h)
			{
				if (who.DistanceTo(ParentObject) <= 1)
				{
					InventoryActionEvent.Check(who, who, ParentObject, "Repair");
				}
				h.FailToParent();
			}));
			who.pBrain.PushGoal(new MoveTo(ParentObject, careful: false, overridesCombat: false, 1));
			return false;
		}
		return base.HandleEvent(E);
	}

	public bool CanBeDisassembled(GameObject who = null)
	{
		if (!CanDisassemble)
		{
			return false;
		}
		if (ParentObject.HasPart("Combat"))
		{
			return false;
		}
		if (who != null && !who.HasSkill("Tinkering_Disassemble"))
		{
			return false;
		}
		if (ParentObject.HasRegisteredEvent("CanBeDisassembled") && !ParentObject.FireEvent("CanBeDisassembled"))
		{
			return false;
		}
		return true;
	}

	public static bool ConfirmBeforeDisassembling(GameObject obj)
	{
		if (obj.Equipped != null)
		{
			return true;
		}
		if (obj.Implantee != null)
		{
			return true;
		}
		if (obj.GetIntProperty("Renamed") == 1)
		{
			return true;
		}
		if (!obj.FireEvent("ConfirmBeforeDisassembling"))
		{
			return true;
		}
		return false;
	}
}
