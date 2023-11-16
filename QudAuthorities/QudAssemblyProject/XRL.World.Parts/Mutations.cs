using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.Wish;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class Mutations : IPart
{
	[Serializable]
	public class MutationModifierTracker
	{
		[Serializable]
		public enum SourceType
		{
			Unknown,
			StatMod,
			Equipment,
			Cooking,
			Tonic
		}

		public Guid id;

		public int bonus;

		public string mutationName;

		public SourceType sourceType;

		public string sourceName;

		public MutationModifierTracker()
		{
		}

		public MutationModifierTracker(MutationModifierTracker Source, bool CopyID = false)
		{
			id = (CopyID ? Source.id : Guid.NewGuid());
			bonus = Source.bonus;
			mutationName = Source.mutationName;
			sourceType = Source.sourceType;
			sourceName = Source.sourceName;
		}
	}

	[NonSerialized]
	public List<BaseMutation> MutationList = new List<BaseMutation>();

	[NonSerialized]
	public List<MutationModifierTracker> MutationMods = new List<MutationModifierTracker>();

	private List<string> FinalizeMutations;

	private int SyncAttempts;

	private bool RestartSync;

	[NonSerialized]
	private static List<MutationEntry> _pool = new List<MutationEntry>();

	public List<BaseMutation> ActiveMutationList => MutationList.Where((BaseMutation m) => m.Level > 0).ToList();

	public override void FinalizeCopyEarly(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyEarly(Source, CopyEffects, CopyID, MapInv);
		if (!(Source.GetPart("Mutations") is Mutations mutations))
		{
			return;
		}
		foreach (BaseMutation mutation in mutations.MutationList)
		{
			if (ParentObject.GetPart(mutation.Name) is BaseMutation item)
			{
				MutationList.Add(item);
			}
		}
		foreach (MutationModifierTracker mutationMod in mutations.MutationMods)
		{
			if (ParentObject.HasPart(mutationMod.mutationName))
			{
				MutationMods.Add(new MutationModifierTracker(mutationMod, CopyID || CopyEffects));
			}
		}
	}

	public override void FinalizeCopyLate(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopyLate(Source, CopyEffects, CopyID, MapInv);
		if (CopyID)
		{
			return;
		}
		if (CopyEffects)
		{
			foreach (MutationModifierTracker item in new List<MutationModifierTracker>(MutationMods))
			{
				if (item.sourceType != MutationModifierTracker.SourceType.StatMod && item.sourceType != MutationModifierTracker.SourceType.Equipment)
				{
					RemoveMutationMod(item.id);
				}
			}
			return;
		}
		foreach (MutationModifierTracker item2 in new List<MutationModifierTracker>(MutationMods))
		{
			RemoveMutationMod(item2.id);
		}
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(MutationList.Count);
		foreach (BaseMutation mutation in MutationList)
		{
			try
			{
				Writer.Write(mutation.Name);
			}
			catch (Exception ex)
			{
				XRLCore.LogError("Exception serializing mutation " + mutation.Name + " : " + ex.ToString());
			}
		}
		Writer.Write(MutationMods);
		base.SaveData(Writer);
	}

	public override void LoadData(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		MutationList.Clear();
		if (num > 0)
		{
			FinalizeMutations = new List<string>();
		}
		for (int i = 0; i < num; i++)
		{
			FinalizeMutations.Add(Reader.ReadString());
		}
		if (Reader.FileVersion >= 141)
		{
			MutationMods = Reader.ReadList<MutationModifierTracker>();
		}
		base.LoadData(Reader);
	}

	public override void FinalizeLoad()
	{
		if (FinalizeMutations == null)
		{
			return;
		}
		if (MutationList == null)
		{
			MutationList = new List<BaseMutation>();
		}
		foreach (string finalizeMutation in FinalizeMutations)
		{
			BaseMutation baseMutation = (BaseMutation)ParentObject.GetPart(finalizeMutation);
			if (baseMutation != null)
			{
				MutationList.Add(baseMutation);
			}
		}
		FinalizeMutations = null;
		base.FinalizeLoad();
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public BaseMutation GetMutation(string MutationName)
	{
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == MutationName)
				{
					return MutationList[i];
				}
			}
		}
		return null;
	}

	public bool HasMutation(string MutationName)
	{
		if (MutationName == "Chimera")
		{
			return ParentObject.IsChimera();
		}
		if (MutationName == "Esper")
		{
			return ParentObject.IsEsper();
		}
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == MutationName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMutation(BaseMutation CheckedMutation)
	{
		if (CheckedMutation == null)
		{
			return false;
		}
		if (MutationList != null)
		{
			int i = 0;
			for (int count = MutationList.Count; i < count; i++)
			{
				if (MutationList[i].Name == CheckedMutation.Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static BaseMutation CreateMutationInstance(string className)
	{
		return ModManager.CreateInstance<BaseMutation>("XRL.World.Parts.Mutation." + className);
	}

	public static Type GetMutationType(string className)
	{
		return ModManager.ResolveType("XRL.World.Parts.Mutation." + className);
	}

	public Guid AddMutationMod(string mutationName, int bonus, MutationModifierTracker.SourceType sourceType = MutationModifierTracker.SourceType.Unknown, string sourceName = null)
	{
		MutationModifierTracker mutationModifierTracker = new MutationModifierTracker
		{
			mutationName = mutationName,
			sourceType = sourceType,
			sourceName = sourceName,
			bonus = bonus
		};
		mutationModifierTracker.id = Guid.NewGuid();
		MutationMods.Add(mutationModifierTracker);
		if (!ParentObject.HasPart(GetMutationType(mutationName)))
		{
			BaseMutation baseMutation = CreateMutationInstance(mutationName);
			if (!baseMutation.CompatibleWith(ParentObject))
			{
				MutationMods.Remove(mutationModifierTracker);
				return Guid.Empty;
			}
			baseMutation.ParentObject = ParentObject;
			if (baseMutation.CanLevel())
			{
				baseMutation.BaseLevel = 0;
			}
			ParentObject.AddPart(baseMutation);
			baseMutation.AfterMutate();
		}
		ParentObject.SyncMutationLevelAndGlimmer();
		return mutationModifierTracker.id;
	}

	public int GetLevelAdjustmentsForMutation(string className)
	{
		return MutationMods.Where((MutationModifierTracker m) => m.mutationName == className).Aggregate(0, (int a, MutationModifierTracker b) => a + b.bonus);
	}

	public void RemoveMutationMod(Guid id)
	{
		MutationModifierTracker tracker = MutationMods.Find((MutationModifierTracker m) => m.id == id);
		if (tracker != null)
		{
			MutationMods.Remove(tracker);
			if (!MutationList.Exists((BaseMutation m) => m.Name == tracker.mutationName) && !MutationMods.Exists((MutationModifierTracker m) => m.mutationName == tracker.mutationName))
			{
				BaseMutation baseMutation = ParentObject.GetPart(tracker.mutationName) as BaseMutation;
				if (baseMutation.Unmutate(ParentObject))
				{
					ParentObject.RemovePart(baseMutation);
					baseMutation.AfterUnmutate(ParentObject);
				}
			}
		}
		ParentObject.SyncMutationLevelAndGlimmer();
	}

	public int AddMutation(string NewMutationClass, int Level)
	{
		Type type = ModManager.ResolveType("XRL.World.Parts.Mutation." + NewMutationClass);
		return AddMutation((BaseMutation)Activator.CreateInstance(type), Level);
	}

	public int AddMutation(MutationEntry NewMutation, int Level)
	{
		return AddMutation(NewMutation.CreateInstance(), Level);
	}

	public int AddMutation(BaseMutation NewMutation, int Level)
	{
		if (NewMutation == null)
		{
			return -1;
		}
		if (MutationList == null)
		{
			MutationList = new List<BaseMutation>();
		}
		if (NewMutation.Name != null)
		{
			if (HasMutation(NewMutation.Name))
			{
				if (NewMutation.GetMutationEntry().Ranked)
				{
					BaseMutation mutation = GetMutation(NewMutation.Name);
					(mutation as IRankedMutation).AdjustRank(1);
					return MutationList.IndexOf(mutation);
				}
			}
			else
			{
				if (ParentObject.HasPart(NewMutation.Name))
				{
					(ParentObject.GetPart(NewMutation.Name) as BaseMutation)?.Unmutate(ParentObject);
					ParentObject.RemovePart(NewMutation.Name);
					NewMutation.AfterUnmutate(ParentObject);
				}
				NewMutation.ParentObject = ParentObject;
				if (NewMutation.Mutate(ParentObject, Level))
				{
					if (ParentObject.HasRegisteredEvent("BeforeMutationAdded"))
					{
						ParentObject.FireEvent(Event.New("BeforeMutationAdded", "Object", ParentObject, "Mutation", NewMutation.GetMutationClass()));
					}
					NewMutation.BaseLevel = Level;
					ParentObject.AddPart(NewMutation);
					MutationList.Add(NewMutation);
					try
					{
						NewMutation.AfterMutate();
						if (ParentObject.HasRegisteredEvent("MutationAdded"))
						{
							ParentObject.FireEvent(Event.New("MutationAdded", "Object", ParentObject, "Mutation", NewMutation.GetMutationClass()));
						}
					}
					catch (Exception message)
					{
						MetricsManager.LogError(message);
					}
					ParentObject.SyncMutationLevelAndGlimmer();
					if (ParentObject != null && ParentObject.IsPlayer() && MutationList.Count >= 10)
					{
						AchievementManager.SetAchievement("ACH_HAVE_10_MUTATIONS");
					}
					return MutationList.Count - 1;
				}
			}
		}
		return -1;
	}

	public BodyPart CheckAddChimericBodyPart(bool Silent = false)
	{
		if (ParentObject != null && ParentObject.CurrentCell != null && ParentObject.IsChimera() && GlobalConfig.GetIntSetting("ChimericBodyPartChance").in100())
		{
			return AddChimericBodyPart(Silent);
		}
		return null;
	}

	public BodyPart AddChimericBodyPart(bool Silent = false)
	{
		BodyPartType randomBodyPartType = Anatomies.GetRandomBodyPartType(IncludeVariants: true, true, false, RequireLiveCategory: true, null, null, UseChimeraWeight: true);
		if (randomBodyPartType == null)
		{
			MetricsManager.LogWarning("could not generate a random body part type");
			return null;
		}
		bool flag = GlobalConfig.GetIntSetting("ChimericBodyPartStandardChance").in100();
		BodyPart chimericBodyPartAttachmentPoint = GetChimericBodyPartAttachmentPoint(ParentObject, randomBodyPartType, flag);
		if (chimericBodyPartAttachmentPoint == null)
		{
			MetricsManager.LogWarning("could not find " + (flag ? "standard" : "random") + " attachment point for " + randomBodyPartType.FinalType);
			return null;
		}
		BodyPart bodyPart = new BodyPart(randomBodyPartType, chimericBodyPartAttachmentPoint.ParentBody);
		List<BodyPartType> list = Anatomies.FindUsualChildBodyPartTypes(randomBodyPartType);
		if (list != null)
		{
			foreach (BodyPartType item in list)
			{
				bodyPart.AddPart(new BodyPart(item, bodyPart.ParentBody, null, null, null, null, null, "Chimera"));
			}
		}
		if (!Silent)
		{
			string text = (bodyPart.Mass ? ("Some " + bodyPart.Name) : ((!bodyPart.Plural) ? Grammar.A(bodyPart.Name, capitalize: true) : ("A set of " + bodyPart.Name)));
			EmitMessage(text + " grows out of " + (ParentObject.IsPlayer() ? "your" : Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName)) + " " + chimericBodyPartAttachmentPoint.GetOrdinalName() + "!", FromDialog: true);
		}
		if (bodyPart.Laterality == 0)
		{
			if (!string.IsNullOrEmpty(randomBodyPartType.UsuallyOn) && randomBodyPartType.UsuallyOn != chimericBodyPartAttachmentPoint.Type)
			{
				BodyPartType bodyPartType = chimericBodyPartAttachmentPoint.VariantTypeModel();
				bodyPart.ModifyNameAndDescriptionRecursively(bodyPartType.Name.Replace(" ", "-"), bodyPartType.Description.Replace(" ", "-"));
			}
			if (chimericBodyPartAttachmentPoint.Laterality != 0)
			{
				bodyPart.ChangeLaterality(chimericBodyPartAttachmentPoint.Laterality | bodyPart.Laterality, Recursive: true);
			}
		}
		chimericBodyPartAttachmentPoint.AddPart(bodyPart, bodyPart.Type, new string[2] { "Thrown Weapon", "Floating Nearby" });
		GlobalConfig.GetIntSetting("ChimericBodyPartMirrorChance").in100();
		return null;
	}

	public static BodyPart GetChimericBodyPartAttachmentPoint(GameObject who, BodyPartType newType, bool Standard = true)
	{
		if (who == null)
		{
			return null;
		}
		Body body = who.Body;
		if (body == null)
		{
			return null;
		}
		if (Standard)
		{
			if (string.IsNullOrEmpty(newType.UsuallyOn))
			{
				return body.GetBody();
			}
		}
		else if (GlobalConfig.GetIntSetting("ChimericBodyPartRandomFromBodyChance").in100())
		{
			return body.GetBody();
		}
		List<BodyPart> list = new List<BodyPart>();
		List<BodyPart> list2 = new List<BodyPart>();
		foreach (BodyPart part in body.GetParts())
		{
			if (part.Abstract || !part.Contact || part.Extrinsic || !string.IsNullOrEmpty(part.DependsOn) || !string.IsNullOrEmpty(part.RequiresType) || !BodyPartCategory.IsLiveCategory(part.Category))
			{
				continue;
			}
			if (Standard)
			{
				if (newType.UsuallyOn == part.Type)
				{
					list2.Add(part);
					if (string.IsNullOrEmpty(newType.UsuallyOnVariant) || newType.UsuallyOnVariant == part.VariantType)
					{
						list.Add(part);
					}
				}
			}
			else
			{
				list.Add(part);
			}
		}
		return list.GetRandomElement() ?? list2.GetRandomElement() ?? body.GetBody();
	}

	[WishCommand("chimericpart", null)]
	public static bool HandleChimericPartWish()
	{
		IComponent<GameObject>.ThePlayer.RequirePart<Mutations>().AddChimericBodyPart();
		return true;
	}

	public void RemoveMutation(BaseMutation Mutation)
	{
		if (Mutation.Unmutate(ParentObject))
		{
			ParentObject.RemovePart(Mutation);
			MutationList.Remove(Mutation);
			Mutation.AfterUnmutate(ParentObject);
		}
	}

	public void LevelMutation(BaseMutation Mutation, int Level)
	{
		Mutation.BaseLevel = Level;
		Mutation.ChangeLevel(Mutation.Level);
		ParentObject.SyncMutationLevelAndGlimmer();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (MutationList != null)
		{
			foreach (BaseMutation mutation in MutationList)
			{
				stringBuilder.Append(mutation.DisplayName + " (" + mutation.Level + ")\n");
			}
		}
		return stringBuilder.ToString();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == StatChangeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (MutationList != null && MutationFactory.StatsUsedByMutations.Contains(E.Name))
		{
			bool flag = false;
			foreach (BaseMutation mutation in MutationList)
			{
				if (!(mutation.GetStat() == E.Name))
				{
					continue;
				}
				int level = mutation.Level;
				if (level != mutation.LastLevel)
				{
					if (level > 0 && mutation.LastLevel <= 0)
					{
						mutation.Mutate(mutation.ParentObject, level);
					}
					else if (level <= 0 && mutation.LastLevel > 0)
					{
						mutation.Unmutate(mutation.ParentObject);
					}
					else
					{
						mutation.ChangeLevel(mutation.Level);
					}
					flag = true;
				}
			}
			if (flag)
			{
				ParentObject.SyncMutationLevelAndGlimmer();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "SyncMutationLevels");
		Object.RegisterPartEvent(this, "AfterLevelGainedEarly");
		base.Register(Object);
	}

	public static bool SyncMutation(BaseMutation mutation, bool syncGlimmer = false)
	{
		if (mutation?.ParentObject == null)
		{
			return false;
		}
		int level = mutation.Level;
		if (level != mutation.LastLevel)
		{
			if (mutation.LastLevel < 15 && level >= 15 && mutation.ParentObject != null && mutation.ParentObject.IsPlayer() && !mutation.ParentObject.HasEffect("Dominated"))
			{
				AchievementManager.SetAchievement("ACH_GET_MUTATION_LEVEL_15");
			}
			if (level > 0 && mutation.LastLevel <= 0)
			{
				mutation.Mutate(mutation.ParentObject, level);
			}
			else if (level <= 0 && mutation.LastLevel > 0)
			{
				mutation.Unmutate(mutation.ParentObject);
			}
			else
			{
				mutation.ChangeLevel(mutation.Level);
			}
			if (syncGlimmer)
			{
				GlimmerChangeEvent.Send(mutation.ParentObject);
			}
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SyncMutationLevels" || E.ID == "AfterLevelGainedEarly")
		{
			if (MutationList == null)
			{
				return true;
			}
			if (SyncAttempts > 0)
			{
				if (SyncAttempts > 100)
				{
					MetricsManager.LogError("Too many mutation sync attempts", new StackTrace());
				}
				else
				{
					RestartSync = true;
				}
				return true;
			}
			try
			{
				bool flag = false;
				while (true)
				{
					IL_0061:
					SyncAttempts++;
					RestartSync = false;
					foreach (BaseMutation mutation in MutationList)
					{
						if (SyncMutation(mutation))
						{
							flag = true;
						}
						if (RestartSync)
						{
							goto IL_0061;
						}
					}
					List<string> list = null;
					foreach (MutationModifierTracker tracker in MutationMods)
					{
						if (!MutationList.Exists((BaseMutation m) => m.Name == tracker.mutationName))
						{
							if (list == null)
							{
								list = new List<string>();
							}
							else if (list.Contains(tracker.mutationName))
							{
								continue;
							}
							list.Add(tracker.mutationName);
						}
					}
					if (list == null)
					{
						break;
					}
					foreach (string item in list)
					{
						if (SyncMutation(ParentObject.GetPart(item) as BaseMutation))
						{
							flag = true;
						}
						if (RestartSync)
						{
							goto IL_0061;
						}
					}
					break;
				}
				if (flag)
				{
					GlimmerChangeEvent.Send(ParentObject);
				}
			}
			finally
			{
				SyncAttempts = 0;
				RestartSync = false;
			}
		}
		return base.FireEvent(E);
	}

	private static bool IncludedInMutatePool(GameObject who, List<BaseMutation> CurrentMutations, MutationEntry entry, bool allowMultipleDefects = false)
	{
		if (who.Property.TryGetValue("MutationLevel", out var value) && !string.IsNullOrEmpty(value) && !MutationFactory.GetMutationEntryByName(value).OkWith(entry, CheckOther: true, allowMultipleDefects))
		{
			return false;
		}
		if (CurrentMutations != null)
		{
			foreach (BaseMutation CurrentMutation in CurrentMutations)
			{
				if (CurrentMutation.GetMutationEntry() == entry && (entry == null || !entry.Ranked))
				{
					return false;
				}
				if (!entry.OkWith(CurrentMutation.GetMutationEntry(), CheckOther: true, allowMultipleDefects))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static List<MutationEntry> GetMutatePool(GameObject who, List<BaseMutation> CurrentMutations = null, Predicate<MutationEntry> filter = null, bool allowMultipleDefects = false)
	{
		_pool.Clear();
		if (CurrentMutations == null && who.GetPart("Mutations") is Mutations mutations)
		{
			CurrentMutations = mutations.MutationList;
		}
		foreach (MutationCategory category in MutationFactory.GetCategories())
		{
			if (!category.IncludeInMutatePool && filter == null)
			{
				continue;
			}
			foreach (MutationEntry entry in category.Entries)
			{
				if (IncludedInMutatePool(who, CurrentMutations, entry, allowMultipleDefects) && (filter == null || filter(entry)))
				{
					_pool.Add(entry);
				}
			}
		}
		return _pool;
	}

	public List<MutationEntry> GetMutatePool(Predicate<MutationEntry> filter = null, bool allowMultipleDefects = false)
	{
		return GetMutatePool(ParentObject, MutationList, filter, allowMultipleDefects);
	}
}
