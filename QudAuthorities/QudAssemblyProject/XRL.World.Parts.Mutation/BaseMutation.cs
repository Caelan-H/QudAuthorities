using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BaseMutation : IPart
{
	public struct LevelCalculation
	{
		public int bonus;

		public bool temporary;

		public string reason;

		public char sigil;
	}

	public string Variant;

	public string DisplayName = "";

	public int BaseLevel = 1;

	public int LastLevel;

	[FieldSaveVersion(254)]
	public Guid ActivatedAbilityID;

	[NonSerialized]
	private List<LevelCalculation> _levelCalcWorkspace = new List<LevelCalculation>();

	[NonSerialized]
	public int MaxLevel = -1;

	[NonSerialized]
	private string _Type;

	public string Type = "Physical";

	[NonSerialized]
	private MutationEntry _Entry;

	private string EnergyUseType;

	public int CapOverride = -1;

	public int Level
	{
		get
		{
			return CalcLevel();
		}
		set
		{
			BaseLevel = value;
			if (ParentObject != null)
			{
				ParentObject.FireEvent("SyncMutationLevels");
			}
		}
	}

	public new StatShifter StatShifter
	{
		get
		{
			StatShifter statShifter = base.StatShifter;
			if (string.IsNullOrEmpty(statShifter.DefaultDisplayName))
			{
				statShifter.DefaultDisplayName = DisplayName;
			}
			return statShifter;
		}
	}

	public int CalcLevel()
	{
		return CalcLevel(storeWork: false);
	}

	public List<LevelCalculation> GetLevelCalculations()
	{
		CalcLevel(storeWork: true);
		return _levelCalcWorkspace;
	}

	public int CalcLevel(bool storeWork = false)
	{
		if (storeWork)
		{
			_levelCalcWorkspace.Clear();
		}
		if (!CanLevel() || ParentObject == null)
		{
			return BaseLevel;
		}
		string @for = GetMutationTermEvent.GetFor(ParentObject, this);
		int num = BaseLevel;
		if (storeWork)
		{
			if (num <= 0)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* You do not possess this " + @for + " inherently, and so you cannot advance its rank."
				});
			}
			else
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* This " + Grammar.MakePossessive(@for) + " base rank is " + num + "."
				});
			}
		}
		MutationEntry mutationEntry = GetMutationEntry();
		Statistic value = null;
		Dictionary<string, Statistic> statistics = ParentObject.Statistics;
		if (statistics != null && statistics.TryGetValue(mutationEntry?.GetStat() ?? "", out value))
		{
			num += value.Modifier;
			if (value.Modifier > 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '+',
					temporary = false,
					reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + value.Modifier + " due to your high " + value.Name + "."
				});
			}
			if (value.Modifier < 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '-',
					temporary = false,
					reason = "- This " + Grammar.MakePossessive(@for) + " rank is decreased by " + -value.Modifier + " due to your low " + value.Name + "."
				});
			}
		}
		int intProperty = ParentObject.GetIntProperty(mutationEntry?.Class);
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old ClassName based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...):" + DebugName);
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetProperty());
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old MutationEntry.Property based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...): " + DebugName);
		}
		intProperty = ParentObject.GetIntProperty("AllMutationLevelModifier");
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + Grammar.Pluralize(@for) + "' ranks are increased by " + intProperty + "."
			});
		}
		if (intProperty < 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + Grammar.Pluralize(@for) + "' ranks are decreased by " + -intProperty + "."
			});
		}
		intProperty = ParentObject.GetIntProperty((mutationEntry?.Category?.Name ?? "Unknown") + "MutationLevelModifier");
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(@for)) + " ranks are increased by " + intProperty + "."
			});
		}
		if (intProperty < 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(@for)) + " ranks are decreased by " + -intProperty + "."
			});
		}
		if (GetType() != typeof(AdrenalControl2) && IsPhysical())
		{
			intProperty = ParentObject.GetIntProperty("AdrenalLevelModifier");
			num += intProperty;
			if (intProperty > 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					temporary = true,
					bonus = intProperty,
					sigil = '+',
					reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + intProperty + " due to your high adrenaline."
				});
			}
		}
		intProperty = GetRapidLevelAmount();
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + intProperty + " due to being rapidly advanced " + intProperty / 3 + " time" + ((intProperty > 3) ? "s" : "") + "."
			});
		}
		foreach (Mutations.MutationModifierTracker item in ParentObject.RequirePart<Mutations>().MutationMods.Where((Mutations.MutationModifierTracker m) => m.mutationName == base.Name))
		{
			num += item.bonus;
			if (storeWork)
			{
				switch (item.sourceType)
				{
				case Mutations.MutationModifierTracker.SourceType.Cooking:
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						temporary = true,
						bonus = item.bonus,
						sigil = '+',
						reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + item.bonus + " due to a metabolizing effect."
					});
					break;
				case Mutations.MutationModifierTracker.SourceType.Tonic:
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						temporary = true,
						bonus = item.bonus,
						sigil = '+',
						reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + item.bonus + " due to a tonic effect."
					});
					break;
				case Mutations.MutationModifierTracker.SourceType.Equipment:
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						temporary = true,
						bonus = item.bonus,
						sigil = '+',
						reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + item.bonus + " due to your equipped item, " + item.sourceName + "."
					});
					break;
				default:
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						temporary = true,
						bonus = item.bonus,
						sigil = ((item.bonus > 0) ? '+' : '-'),
						reason = "+ This " + Grammar.MakePossessive(@for) + " rank is increased by " + Math.Abs(item.bonus) + " due to your " + item.sourceName + "."
					});
					break;
				}
			}
		}
		if (num < 1)
		{
			if (storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = 1 - num,
					temporary = false,
					sigil = '+',
					reason = "+ " + ColorUtility.CapitalizeExceptFormatting(@for) + " ranks cannot be reduced below 1."
				});
			}
			num = 1;
		}
		int mutationCap = GetMutationCap();
		if (mutationCap < num)
		{
			if (storeWork)
			{
				bool flag = false;
				int num2 = num - mutationCap;
				int num3 = _levelCalcWorkspace.Where((LevelCalculation c) => c.temporary).Aggregate(0, (int a, LevelCalculation b) => a + b.bonus);
				if (num3 > 0)
				{
					int num4 = Math.Min(num2, num3);
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num4,
						temporary = true,
						sigil = '-',
						reason = "- This " + Grammar.MakePossessive(@for) + " rank is capped at " + mutationCap + " due to your level."
					});
					num2 -= num4;
					flag = true;
				}
				if (num2 > 0)
				{
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num2,
						temporary = false,
						sigil = ((!flag) ? '-' : '\0'),
						reason = (flag ? null : ("- This " + Grammar.MakePossessive(@for) + " rank is capped at " + mutationCap + " due to your level."))
					});
				}
			}
			num = mutationCap;
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetCategoryForceProperty());
		intProperty += ParentObject.GetIntProperty(mutationEntry?.GetForceProperty());
		return num + intProperty;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetPsychicGlimmerEvent.ID || !(Type == "Mental")))
		{
			if (ID == IsSensableAsPsychicEvent.ID)
			{
				return Type == "Mental";
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		if (Type == "Mental" && E.Subject == ParentObject && !IsDefect())
		{
			E.Level += Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		if (Type == "Mental")
		{
			E.Sensable = true;
		}
		return base.HandleEvent(E);
	}

	public void SyncLevel()
	{
		Mutations.SyncMutation(this, syncGlimmer: true);
	}

	public virtual bool CompatibleWith(GameObject go)
	{
		List<MutationEntry> mutationEntry = MutationFactory.GetMutationEntry(this);
		if (mutationEntry == null)
		{
			return true;
		}
		foreach (MutationEntry item in mutationEntry)
		{
			string[] exclusions = item.GetExclusions();
			foreach (string name in exclusions)
			{
				if (MutationFactory.HasMutation(name))
				{
					string @class = MutationFactory.GetMutationEntryByName(name).Class;
					if (go.HasPart(@class))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public int GetTemporaryLevels()
	{
		return (from v in GetLevelCalculations()
			where v.temporary
			select v).Aggregate(0, (int memo, LevelCalculation value) => memo + value.bonus);
	}

	public bool IsPhysical()
	{
		return GetMutationEntry()?.IsPhysical() ?? false;
	}

	public bool IsMental()
	{
		return GetMutationEntry()?.IsMental() ?? false;
	}

	public bool IsDefect()
	{
		return GetMutationEntry()?.IsDefect() ?? false;
	}

	public virtual int GetMaxLevel()
	{
		if (MaxLevel == -1)
		{
			MaxLevel = 10;
			string name = GetType().Name;
			foreach (MutationEntry item in MutationFactory.AllMutationEntries())
			{
				if (item.Class.EqualsNoCase(name) && item.MaxLevel > -1)
				{
					MaxLevel = item.MaxLevel;
					break;
				}
			}
		}
		return MaxLevel;
	}

	public bool CanIncreaseLevel()
	{
		if (CanLevel() && BaseLevel < GetMaxLevel())
		{
			return Level < GetMutationCap();
		}
		return false;
	}

	public string GetMutationType()
	{
		if (_Type != null)
		{
			return _Type;
		}
		_Type = Type;
		string name = GetType().Name;
		foreach (MutationEntry item in MutationFactory.AllMutationEntries())
		{
			if (item.Type != null && item.Class.EqualsNoCase(name))
			{
				_Type = item.Type;
				break;
			}
		}
		return _Type;
	}

	public bool isCategory(string category)
	{
		return GetMutationEntry()?.Category?.Name == category;
	}

	public MutationEntry GetMutationEntry()
	{
		if (_Entry != null)
		{
			return _Entry;
		}
		string name = GetType().Name;
		foreach (MutationEntry item in MutationFactory.AllMutationEntries())
		{
			if (item.Class.EqualsNoCase(name))
			{
				_Entry = item;
				return _Entry;
			}
		}
		return _Entry;
	}

	public string GetMutationClass()
	{
		return GetMutationEntry()?.Class;
	}

	public string GetBearerDescription()
	{
		MutationEntry mutationEntry = GetMutationEntry();
		if (mutationEntry == null)
		{
			return "";
		}
		return mutationEntry.BearerDescription;
	}

	public void UseEnergy(int Amount)
	{
		if (EnergyUseType == null)
		{
			EnergyUseType = Type + " Mutation";
		}
		ParentObject.UseEnergy(Amount, EnergyUseType);
	}

	public void UseEnergy(int Amount, string Type)
	{
		ParentObject.UseEnergy(Amount, Type);
	}

	public static int GetMutationCapForLevel(int level)
	{
		return level / 2 + 1;
	}

	public virtual int GetMutationCap()
	{
		return Math.Max(CapOverride, ParentObject.HasStat("Level") ? GetMutationCapForLevel(ParentObject.Stat("Level")) : (-1));
	}

	public string GetStat()
	{
		MutationEntry mutationEntry = GetMutationEntry();
		if (mutationEntry == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(mutationEntry.Stat) && mutationEntry.Category != null && !string.IsNullOrEmpty(mutationEntry.Category.Stat))
		{
			mutationEntry.Stat = mutationEntry.Category.Stat;
		}
		return mutationEntry.Stat;
	}

	public virtual bool ShouldShowLevel()
	{
		return CanLevel();
	}

	public virtual bool CanLevel()
	{
		return true;
	}

	public virtual bool AffectsBodyParts()
	{
		return false;
	}

	public virtual bool GeneratesEquipment()
	{
		return false;
	}

	public virtual string GetCreateCharacterDisplayName()
	{
		return DisplayName;
	}

	public virtual string GetDescription()
	{
		return "<description>";
	}

	public virtual string GetLevelText(int Level)
	{
		return "<Does level " + Level + " stuff>";
	}

	public virtual bool IsObvious()
	{
		return false;
	}

	public virtual bool Mutate(GameObject GO, int Level = 1)
	{
		ChangeLevel(Level);
		return true;
	}

	public virtual void AfterMutate()
	{
	}

	public virtual bool Unmutate(GameObject GO)
	{
		LastLevel = 0;
		return true;
	}

	public virtual void AfterUnmutate(GameObject GO)
	{
	}

	public int GetRapidLevelAmount()
	{
		return ParentObject.GetIntProperty("RapidLevel_" + GetMutationClass());
	}

	public virtual void RapidLevel(int amount)
	{
		ParentObject.ModIntProperty("RapidLevel_" + GetMutationClass(), amount);
		ParentObject.SyncMutationLevelAndGlimmer();
	}

	public virtual bool ChangeLevel(int NewLevel)
	{
		LastLevel = NewLevel;
		return true;
	}

	protected void CleanUpMutationEquipment(GameObject who, ref GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		BodyPart bodyPart = obj.EquippedOn();
		if (bodyPart != null && who != null)
		{
			who.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart).SetSilent(Silent: true));
		}
		else if (who != null)
		{
			Body body = who.Body;
			if (body != null)
			{
				bodyPart = body.FindDefaultOrEquippedItem(obj);
				if (bodyPart != null && bodyPart.DefaultBehavior == obj)
				{
					bodyPart.DefaultBehavior = null;
				}
			}
		}
		obj.Destroy();
		obj = null;
	}

	public virtual List<string> GetVariants()
	{
		return null;
	}

	public virtual void SetVariant(int n)
	{
		List<string> variants = GetVariants();
		if (variants != null && variants.Count > n)
		{
			Variant = variants[n];
		}
	}

	protected void CleanUpMutationEquipment(GameObject who, GameObject obj)
	{
		CleanUpMutationEquipment(who, ref obj);
	}
}
