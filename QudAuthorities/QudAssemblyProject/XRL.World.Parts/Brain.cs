using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genkit;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;

namespace XRL.World.Parts;

[Serializable]
public class Brain : IPart
{
	public enum CreatureOpinion
	{
		hostile,
		neutral,
		allied
	}

	public enum FactionAllegiance
	{
		none,
		associated,
		affiliated,
		member
	}

	public class WeaponSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public WeaponSorter(GameObject POV)
		{
			this.POV = POV;
		}

		public WeaponSorter(GameObject POV, bool Reverse)
			: this(POV)
		{
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject o1, GameObject o2)
		{
			return CompareWeapons(o1, o2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	public class MissileWeaponSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public MissileWeaponSorter(GameObject POV)
		{
			this.POV = POV;
		}

		public MissileWeaponSorter(GameObject POV, bool Reverse)
			: this(POV)
		{
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject o1, GameObject o2)
		{
			return CompareMissileWeapons(o1, o2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	public class GearSorter : Comparer<GameObject>
	{
		private GameObject POV;

		private bool Reverse;

		public GearSorter(GameObject POV)
		{
			this.POV = POV;
		}

		public GearSorter(GameObject POV, bool Reverse)
			: this(POV)
		{
			this.Reverse = Reverse;
		}

		public override int Compare(GameObject o1, GameObject o2)
		{
			return CompareGear(o1, o2, POV) * ((!Reverse) ? 1 : (-1));
		}
	}

	public const int DEFAULT_MIN_KILL_RADIUS = 5;

	public const int DEFAULT_MAX_KILL_RADIUS = 15;

	public const int DEFAULT_HOSTILE_WALK_RADIUS = 84;

	public const int DEFAULT_MAX_WANDER_RADIUS = 12;

	public const int PARTY_PROSELYTIZED = 1;

	public const int PARTY_BEGUILED = 2;

	public const int PARTY_BONDED = 4;

	public const int PARTY_INDEPENDENT = 8388608;

	public const int BRAIN_HOSTILE = 1;

	public const int BRAIN_CALM = 2;

	public const int BRAIN_WANDERS = 4;

	public const int BRAIN_WANDERS_RANDOMLY = 8;

	public const int BRAIN_AQUATIC = 16;

	public const int BRAIN_LIVES_ON_WALLS = 32;

	public const int BRAIN_WALL_WALKER = 64;

	public const int BRAIN_MOBILE = 128;

	public const int BRAIN_HIBERNATING = 256;

	public const int BRAIN_POINT_BLANK_RANGE = 512;

	public const int BRAIN_DO_REEQUIP = 1024;

	public const int BRAIN_NEED_TO_RELOAD = 2048;

	public const int BRAIN_STAYING = 4096;

	public const int BRAIN_PASSIVE = 8192;

	public string Factions;

	public int Flags = 1408;

	public int MaxMissileRange = 80;

	public int MinKillRadius = 5;

	public int MaxKillRadius = 15;

	public int HostileWalkRadius = 84;

	public int MaxWanderRadius = 12;

	public GameObject _PartyLeader;

	public GlobalLocation StartingCell;

	[NonSerialized]
	public string LastThought;

	[NonSerialized]
	public Dictionary<string, int> FactionMembership = new Dictionary<string, int>();

	[NonSerialized]
	public Dictionary<string, int> FactionFeelings = new Dictionary<string, int>();

	[NonSerialized]
	public Dictionary<string, int> PartyMembers = new Dictionary<string, int>();

	[NonSerialized]
	public CleanStack<GoalHandler> Goals = new CleanStack<GoalHandler>();

	[NonSerialized]
	public Dictionary<GameObject, ObjectOpinion> ObjectMemory = new Dictionary<GameObject, ObjectOpinion>();

	[NonSerialized]
	public Dictionary<GameObject, int> FriendlyFire;

	[NonSerialized]
	private static Event eAITakingAction = new ImmutableEvent("AITakingAction");

	[NonSerialized]
	private static Event eCommandEquipObject = new Event("CommandEquipObject", "Object", (object)null, "BodyPart", (object)null);

	[NonSerialized]
	private static Event eCommandEquipObjectFree = new Event("CommandEquipObject", "Object", (object)null, "BodyPart", (object)null, "EnergyCost", (object)0);

	[NonSerialized]
	private static Event eBeforeAITakingAction = new ImmutableEvent("BeforeAITakingAction");

	[NonSerialized]
	private static Event eCheckingHostilityTowardsPlayer = new ImmutableEvent("CheckingHostilityTowardsPlayer");

	[NonSerialized]
	private static Event eFactionsAdded = new ImmutableEvent("FactionsAdded");

	[NonSerialized]
	private static Event eTakingAction = new ImmutableEvent("TakingAction");

	[NonSerialized]
	public Physics _pPhysics;

	private static Dictionary<string, int> ProcFactions = new Dictionary<string, int>();

	[NonSerialized]
	public static List<GameObject> objectsToRemove = new List<GameObject>();

	public bool Hostile
	{
		get
		{
			return (Flags & 1) == 1;
		}
		set
		{
			Flags = (value ? (Flags | 1) : (Flags & -2));
		}
	}

	public bool Calm
	{
		get
		{
			return (Flags & 2) == 2;
		}
		set
		{
			Flags = (value ? (Flags | 2) : (Flags & -3));
		}
	}

	public bool Wanders
	{
		get
		{
			return (Flags & 4) == 4;
		}
		set
		{
			Flags = (value ? (Flags | 4) : (Flags & -5));
		}
	}

	public bool WandersRandomly
	{
		get
		{
			return (Flags & 8) == 8;
		}
		set
		{
			Flags = (value ? (Flags | 8) : (Flags & -9));
		}
	}

	public bool Aquatic
	{
		get
		{
			return (Flags & 0x10) == 16;
		}
		set
		{
			Flags = (value ? (Flags | 0x10) : (Flags & -17));
		}
	}

	public bool LivesOnWalls
	{
		get
		{
			return (Flags & 0x20) == 32;
		}
		set
		{
			Flags = (value ? (Flags | 0x20) : (Flags & -33));
		}
	}

	public bool WallWalker
	{
		get
		{
			return (Flags & 0x40) == 64;
		}
		set
		{
			Flags = (value ? (Flags | 0x40) : (Flags & -65));
		}
	}

	public bool Mobile
	{
		get
		{
			return (Flags & 0x80) == 128;
		}
		set
		{
			Flags = (value ? (Flags | 0x80) : (Flags & -129));
		}
	}

	public bool Hibernating
	{
		get
		{
			return (Flags & 0x100) == 256;
		}
		set
		{
			Flags = (value ? (Flags | 0x100) : (Flags & -257));
		}
	}

	public bool PointBlankRange
	{
		get
		{
			return (Flags & 0x200) == 512;
		}
		set
		{
			Flags = (value ? (Flags | 0x200) : (Flags & -513));
		}
	}

	public bool DoReequip
	{
		get
		{
			return (Flags & 0x400) == 1024;
		}
		set
		{
			Flags = (value ? (Flags | 0x400) : (Flags & -1025));
		}
	}

	public bool NeedToReload
	{
		get
		{
			return (Flags & 0x800) == 2048;
		}
		set
		{
			Flags = (value ? (Flags | 0x800) : (Flags & -2049));
		}
	}

	public bool Staying
	{
		get
		{
			return (Flags & 0x1000) == 4096;
		}
		set
		{
			Flags = (value ? (Flags | 0x1000) : (Flags & -4097));
		}
	}

	public bool Passive
	{
		get
		{
			return (Flags & 0x2000) == 8192;
		}
		set
		{
			Flags = (value ? (Flags | 0x2000) : (Flags & -8193));
		}
	}

	public GameObject PartyLeader
	{
		get
		{
			GameObject.validate(ref _PartyLeader);
			return _PartyLeader;
		}
		set
		{
			_PartyLeader = value;
			if (_PartyLeader != null)
			{
				if (ParentObject.pPhysics?.LastDamagedBy == _PartyLeader)
				{
					ParentObject.pPhysics.LastDamagedBy = null;
				}
				if (ParentObject.pPhysics?.InflamedBy == _PartyLeader)
				{
					ParentObject.pPhysics.InflamedBy = null;
				}
			}
		}
	}

	public string AddFactions
	{
		set
		{
			if (string.IsNullOrEmpty(Factions))
			{
				Factions = value;
			}
			else
			{
				Factions = Factions + "," + value;
			}
		}
	}

	public GameObject Target
	{
		get
		{
			if (ParentObject.IsPlayer())
			{
				return Sidebar.CurrentTarget;
			}
			for (int num = Goals.Items.Count - 1; num >= 0; num--)
			{
				if (Goals.Items[num] is Kill kill && kill.Target != null && kill.Target.IsValid())
				{
					return kill.Target;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				if (ParentObject.IsPlayer())
				{
					Sidebar.CurrentTarget = null;
				}
				for (int num = Goals.Items.Count - 1; num >= 0; num--)
				{
					if (Goals.Items[num] is Kill kill)
					{
						if (kill.Target != null)
						{
							ObjectMemory.Remove(kill.Target);
						}
						for (int num2 = Goals.Items.Count - 1; num2 >= num; num2--)
						{
							Goals.Pop();
						}
						num = Goals.Items.Count - 1;
					}
				}
			}
			else if (Target == null && value != ParentObject)
			{
				if (ParentObject.IsPlayer())
				{
					Sidebar.CurrentTarget = value;
				}
				else
				{
					WantToKill(value);
				}
			}
		}
	}

	public Physics pPhysics
	{
		get
		{
			if (_pPhysics == null)
			{
				_pPhysics = ParentObject.pPhysics;
			}
			return _pPhysics;
		}
		set
		{
			_pPhysics = null;
		}
	}

	public bool StepTowards(Cell targetCell, bool bGlobal = false)
	{
		if (targetCell == null)
		{
			return true;
		}
		Think("I'm going to move towards my target.");
		if (targetCell.ParentZone.IsWorldMap())
		{
			Think("Target's on the world map, can't follow!");
			return false;
		}
		FindPath findPath = new FindPath(pPhysics.CurrentCell.ParentZone.ZoneID, pPhysics.CurrentCell.X, pPhysics.CurrentCell.Y, targetCell.ParentZone.ZoneID, targetCell.X, targetCell.Y, bGlobal, PathUnlimited: false, ParentObject);
		if (findPath.bFound)
		{
			if (findPath.Directions.Count > 0)
			{
				PushGoal(new Step(findPath.Directions[0]));
			}
			return true;
		}
		return false;
	}

	public bool HasGoal(string name)
	{
		foreach (GoalHandler item in Goals.Items)
		{
			if (item.GetType().Name == name)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasGoal()
	{
		return Goals.Count switch
		{
			0 => false, 
			1 => Goals.Peek().GetType().Name != "Bored", 
			_ => true, 
		};
	}

	public bool HasGoalOtherThan(string what)
	{
		switch (Goals.Count)
		{
		case 0:
			return false;
		case 1:
			if (Goals.Peek().GetType().Name != what)
			{
				return Goals.Peek().GetType().Name != "Bored";
			}
			return false;
		default:
			foreach (GoalHandler item in Goals.Items)
			{
				if (item.GetType().Name != what && item.GetType().Name != "Bored")
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool TakeOnAttitudesOf(Brain o, bool CopyLeader = false, bool CopyTarget = false)
	{
		if (o == null)
		{
			return false;
		}
		Hostile = o.Hostile;
		Calm = o.Calm;
		Wanders = o.Wanders;
		WandersRandomly = o.WandersRandomly;
		Factions = o.Factions;
		FactionMembership = new Dictionary<string, int>(o.FactionMembership);
		FactionFeelings = new Dictionary<string, int>(o.FactionFeelings);
		ObjectMemory = new Dictionary<GameObject, ObjectOpinion>(o.ObjectMemory.Count);
		foreach (GameObject key in o.ObjectMemory.Keys)
		{
			ObjectMemory.Add(key, new ObjectOpinion(o.ObjectMemory[key]));
		}
		if (o.ParentObject != null && ParentObject != null && o.ParentObject.IsTrifling)
		{
			ParentObject.IsTrifling = true;
		}
		if (CopyLeader)
		{
			_PartyLeader = o._PartyLeader;
		}
		if (CopyTarget)
		{
			Target = o.Target;
		}
		return true;
	}

	public bool TakeOnAttitudesOf(GameObject obj, bool CopyLeader = false, bool CopyTarget = false)
	{
		if (obj == null)
		{
			return false;
		}
		return TakeOnAttitudesOf(obj.pBrain, CopyLeader, CopyTarget);
	}

	public bool isMobile()
	{
		if (Mobile)
		{
			return true;
		}
		if ((ParentObject != null) & ParentObject.IsFlying)
		{
			return true;
		}
		return false;
	}

	public bool limitToAquatic()
	{
		if (!Aquatic)
		{
			return false;
		}
		if (ParentObject != null && ParentObject.IsFlying)
		{
			return false;
		}
		return true;
	}

	public void checkMobility(out bool immobile, out bool waterbound, out bool wallwalker)
	{
		immobile = !Mobile;
		waterbound = Aquatic;
		wallwalker = WallWalker;
		if ((immobile | waterbound | wallwalker) && ParentObject != null && ParentObject.IsFlying)
		{
			immobile = false;
			waterbound = false;
			wallwalker = false;
		}
	}

	public void setFactionFeeling(string faction, int feeling)
	{
		FactionFeelings[faction] = feeling;
	}

	public void setFactionMembership(string faction, int feeling)
	{
		FactionMembership[faction] = feeling;
	}

	public bool CanFight()
	{
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (!Goals.Items[i].CanFight())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsNonAggressive()
	{
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (Goals.Items[i].IsNonAggressive())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBusy()
	{
		int i = 0;
		for (int count = Goals.Count; i < count; i++)
		{
			if (Goals.Items[i].IsBusy())
			{
				return true;
			}
		}
		return false;
	}

	public void WantToKill(GameObject who, string because = null)
	{
		ThinkOfKilling(who, because);
		if (GetFeeling(who) >= 0)
		{
			AdjustFeeling(who, -100);
		}
		if (Goals.Count == 0)
		{
			new Bored().Push(this);
		}
		Goals.Peek().PushChildGoal(new Kill(who));
	}

	public override void LoadData(SerializationReader Reader)
	{
		int num = Reader.ReadInt32();
		ObjectMemory.Clear();
		for (int i = 0; i < num; i++)
		{
			GameObject key = Reader.ReadGameObject("memory");
			if (!ObjectMemory.ContainsKey(key))
			{
				ObjectMemory.Add(key, new ObjectOpinion(Reader.ReadInt32()));
			}
		}
		int num2 = Reader.ReadInt32();
		if (num2 == 0)
		{
			FriendlyFire = null;
		}
		else
		{
			if (FriendlyFire == null)
			{
				FriendlyFire = new Dictionary<GameObject, int>(num2);
			}
			else
			{
				FriendlyFire.Clear();
			}
			for (int j = 0; j < num2; j++)
			{
				GameObject key2 = Reader.ReadGameObject("friendlyfire");
				if (!FriendlyFire.ContainsKey(key2))
				{
					FriendlyFire.Add(key2, Reader.ReadInt32());
				}
			}
		}
		FactionMembership = Reader.ReadDictionary<string, int>();
		FactionFeelings = Reader.ReadDictionary<string, int>();
		if (Reader.FileVersion >= 224)
		{
			PartyMembers = Reader.ReadDictionary<string, int>();
		}
		base.LoadData(Reader);
	}

	public void Mindwipe()
	{
		Goals.Clear();
		PartyLeader = null;
		StartingCell = null;
		Staying = false;
		Passive = false;
		ObjectMemory.Clear();
		FactionFeelings.Clear();
		PartyMembers.Clear();
		if (FriendlyFire != null)
		{
			FriendlyFire.Clear();
		}
		GameObjectBlueprint blueprint = ParentObject.GetBlueprint();
		if (blueprint != null && blueprint.ReinitializePart(this))
		{
			InitFromFactions();
		}
	}

	public void StopFighting(bool Involuntary = false)
	{
		Target = null;
		ClearHostileMemory();
		FlushNavigationCaches();
		if (ParentObject.HasRegisteredEvent("StopFighting"))
		{
			ParentObject.FireEvent(Event.New("StopFighting", "Target", (object)null, "Involuntary", Involuntary ? 1 : 0));
		}
	}

	public void StopFighting(GameObject who, bool Involuntary = false)
	{
		if (ParentObject.IsPlayer() && Sidebar.CurrentTarget == who)
		{
			Sidebar.CurrentTarget = null;
		}
		if (ObjectMemory.TryGetValue(who, out var value) && value.Disposition < 0)
		{
			ObjectMemory.Remove(who);
		}
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num] is Kill kill && kill.Target == who)
			{
				for (int num2 = Goals.Items.Count - 1; num2 >= num; num2--)
				{
					Goals.Pop();
				}
				num = Goals.Items.Count;
			}
		}
		FlushNavigationCaches();
		if (ParentObject.HasRegisteredEvent("StopFighting"))
		{
			ParentObject.FireEvent(Event.New("StopFighting", "Target", who, "Involuntary", Involuntary ? 1 : 0));
		}
	}

	public void ClearHostileMemory()
	{
		if (ObjectMemory.Count <= 0)
		{
			return;
		}
		if (ObjectMemory.Count == 1)
		{
			KeyValuePair<GameObject, ObjectOpinion> keyValuePair = ObjectMemory.First();
			if (keyValuePair.Value.Disposition < 0)
			{
				ObjectMemory.Remove(keyValuePair.Key);
			}
			return;
		}
		List<GameObject> list = new List<GameObject>(ObjectMemory.Count);
		foreach (KeyValuePair<GameObject, ObjectOpinion> item in ObjectMemory)
		{
			if (item.Value.Disposition < 0)
			{
				list.Add(item.Key);
			}
		}
		foreach (GameObject item2 in list)
		{
			ObjectMemory.Remove(item2);
		}
	}

	public bool ShouldRemember(GameObject GO)
	{
		if (!GameObject.validate(ref GO))
		{
			return false;
		}
		if (GO.IsPlayer())
		{
			return true;
		}
		Zone currentZone = GO.CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (!currentZone.IsActive())
		{
			return false;
		}
		if (currentZone != ParentObject.CurrentZone)
		{
			return false;
		}
		return true;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		if (pPhysics == null || pPhysics.CurrentCell == null || pPhysics.CurrentCell.ParentZone == null || !pPhysics.CurrentCell.ParentZone.IsActive())
		{
			ObjectMemory.Clear();
		}
		int num = 0;
		foreach (GameObject key in ObjectMemory.Keys)
		{
			if (key != null && ShouldRemember(key))
			{
				num++;
			}
		}
		Writer.Write(num);
		foreach (GameObject key2 in ObjectMemory.Keys)
		{
			if (key2 != null && ShouldRemember(key2))
			{
				Writer.WriteGameObject(key2);
				Writer.Write(ObjectMemory[key2].Disposition);
			}
		}
		int num2 = 0;
		if (FriendlyFire != null)
		{
			foreach (GameObject key3 in FriendlyFire.Keys)
			{
				if (key3 != null && ShouldRemember(key3))
				{
					num2++;
				}
			}
		}
		Writer.Write(num2);
		if (FriendlyFire != null)
		{
			foreach (GameObject key4 in FriendlyFire.Keys)
			{
				if (key4 != null && ShouldRemember(key4))
				{
					Writer.WriteGameObject(key4);
					Writer.Write(FriendlyFire[key4]);
				}
			}
		}
		Writer.Write(FactionMembership);
		Writer.Write(FactionFeelings);
		Writer.Write(PartyMembers);
		base.SaveData(Writer);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Brain obj = base.DeepCopy(Parent, MapInv) as Brain;
		obj.pPhysics = null;
		obj.FactionMembership = new Dictionary<string, int>(FactionMembership);
		obj.PartyMembers = new Dictionary<string, int>();
		return obj;
	}

	public GoalHandler PushGoal(GoalHandler Goal)
	{
		Goal.Push(this);
		return Goal;
	}

	public void Think(string Hrm)
	{
		LastThought = Hrm;
	}

	public void ThinkOfKilling(GameObject who, string because = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("I'm going to kill").Append(who.a).Append(who.ShortDisplayName);
		if (!string.IsNullOrEmpty(because))
		{
			stringBuilder.Append(" because ").Append(because);
		}
		stringBuilder.Append('!');
		Think(stringBuilder.ToString());
	}

	public void SetFeeling(GameObject GO, int NewFeeling)
	{
		if (GO == null || (GO.HasStringProperty("FugueCopy") && ParentObject.GetIntProperty("CopyAllowFeelingAdjust") <= 0))
		{
			return;
		}
		if (ObjectMemory.TryGetValue(GO, out var value))
		{
			if (value.Disposition != 200)
			{
				value.Disposition = NewFeeling;
			}
		}
		else
		{
			ObjectMemory.Add(GO, new ObjectOpinion(NewFeeling));
		}
		if (NewFeeling > 0 && GO == Target)
		{
			Target = null;
			Goals.Clear();
		}
	}

	public bool HasPersonalFeeling(GameObject GO)
	{
		return ObjectMemory.ContainsKey(GO);
	}

	public int? GetPersonalFeeling(GameObject GO)
	{
		if (ObjectMemory.TryGetValue(GO, out var value))
		{
			return value.Disposition;
		}
		return null;
	}

	public void BecomeCompanionOf(GameObject obj, bool trifling = false)
	{
		List<GameObject> list = Event.NewGameObjectList();
		list.Add(ParentObject);
		list.Add(obj);
		ParentObject.CurrentZone?.FindObjects(list, (GameObject o) => o.InSamePartyAs(ParentObject) || o.InSamePartyAs(obj));
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			for (int j = 0; j < count; j++)
			{
				if (i != j)
				{
					list[i].StopFighting(list[j]);
					list[j].StopFighting(list[i]);
				}
			}
		}
		if (Goals != null)
		{
			Goals.Clear();
		}
		ClearHostileMemory();
		ParentObject.SetPartyLeader(obj, takeOnAttitudesOfLeader: false, trifling);
		Hostile = false;
		Wanders = false;
		WandersRandomly = false;
		ParentObject.FireEvent(Event.New("BecomeCompanion", "Leader", obj));
		ParentObject.UpdateVisibleStatusColor();
	}

	public bool IsLedBy(GameObject GO)
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject == GO)
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public bool IsLedBy(string Blueprint)
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject.Blueprint == Blueprint)
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public bool IsPlayerLed()
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			if (gameObject.IsPlayer() || gameObject.LeftBehindByPlayer())
			{
				return true;
			}
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return false;
	}

	public void AdjustFeeling(GameObject GO, int FeelingDelta)
	{
		if (GO == null || (GO.HasStringProperty("FugueCopy") && ParentObject.GetIntProperty("CopyAllowFeelingAdjust") <= 0))
		{
			return;
		}
		if (ObjectMemory.TryGetValue(GO, out var value))
		{
			if (value.Disposition != 200)
			{
				value.Disposition += FeelingDelta;
			}
		}
		else
		{
			ObjectMemory.Add(GO, new ObjectOpinion(FeelingDelta));
		}
		if (GO == ParentObject || (FeelingDelta < 0 && GO == PartyLeader))
		{
			return;
		}
		if (ObjectMemory[GO].Disposition > 0 && GO == Target)
		{
			Goals.Clear();
		}
		Brain pBrain = GO.pBrain;
		if (pBrain != null && pBrain.PartyLeader != null && pBrain.PartyLeader != PartyLeader && pBrain.PartyLeader != ParentObject)
		{
			if (ObjectMemory.TryGetValue(pBrain.PartyLeader, out var value2))
			{
				if (value2.Disposition != 200)
				{
					value2.Disposition += FeelingDelta;
				}
			}
			else
			{
				value2 = new ObjectOpinion(FeelingDelta);
				ObjectMemory.Add(pBrain.PartyLeader, value2);
			}
			if (value2.Disposition > 0 && pBrain.PartyLeader == Target)
			{
				Goals.Clear();
			}
		}
		FlushNavigationCaches();
	}

	public bool GetAngryAt(GameObject GO, int amount = -50)
	{
		if (ParentObject.IsPlayer() || ParentObject.IsInGraveyard())
		{
			return true;
		}
		if (GO != null && !InSamePartyAs(GO) && (GO.IsCreature || 3.in100()))
		{
			AdjustFeeling(GO, amount);
			if (GetFeeling(GO) < 0)
			{
				BroadcastForHelp(GO);
				if ((Target == null || GO.IsPlayer()) && CanFight())
				{
					WantToKill(GO, "out of anger");
				}
			}
		}
		Hibernating = false;
		return true;
	}

	public bool LikeBetter(GameObject GO, int amount = 50)
	{
		if (ParentObject.IsPlayer() || ParentObject.IsInGraveyard())
		{
			return true;
		}
		if (GO != null)
		{
			AdjustFeeling(GO, amount);
		}
		Hibernating = false;
		return true;
	}

	public GameObject GetFinalLeader()
	{
		GameObject gameObject = PartyLeader;
		while (gameObject != null)
		{
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == null)
			{
				break;
			}
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
		}
		return gameObject;
	}

	public Brain GetFinalLeaderBrain()
	{
		GameObject gameObject = PartyLeader;
		GameObject gameObject2 = gameObject;
		while (gameObject != null)
		{
			GameObject partyLeader = gameObject.PartyLeader;
			if (partyLeader == null)
			{
				break;
			}
			if (partyLeader == ParentObject)
			{
				MetricsManager.LogError("leader cycle in " + ParentObject.DebugName + " + " + gameObject.DebugName);
				break;
			}
			gameObject = partyLeader;
			if (gameObject.pBrain != null)
			{
				gameObject2 = gameObject;
			}
		}
		object obj = gameObject?.pBrain;
		if (obj == null)
		{
			if (gameObject2 == null)
			{
				return null;
			}
			obj = gameObject2.pBrain;
		}
		return (Brain)obj;
	}

	public bool InSamePartyAs(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		GameObject finalLeader = GetFinalLeader();
		if (finalLeader == null)
		{
			return who.IsLedBy(ParentObject);
		}
		if (who == finalLeader)
		{
			return true;
		}
		if (who.IsLedBy(finalLeader))
		{
			return true;
		}
		return false;
	}

	public int GetFeeling(GameObject GO)
	{
		if (GO == null)
		{
			return 0;
		}
		if (ParentObject == null)
		{
			return 0;
		}
		if (ParentObject.HasIntProperty("ForceFeeling"))
		{
			return ParentObject.GetIntProperty("ForceFeeling");
		}
		if (ParentObject.HasEffect("Frenzied") && !GO.HasEffect("Frenzied"))
		{
			return -100;
		}
		if (ParentObject.HasCopyRelationship(GO) && ParentObject.GetIntProperty("CopyAllowFeelingAdjust") <= 0)
		{
			return 100;
		}
		if (PartyLeader != null)
		{
			if (!GO.IsPlayer() && GO == Target)
			{
				return -1;
			}
			Brain finalLeaderBrain = GetFinalLeaderBrain();
			if (finalLeaderBrain != null)
			{
				return finalLeaderBrain.GetFeeling(GO);
			}
		}
		Brain pBrain = GO.pBrain;
		if (pBrain == null)
		{
			return 0;
		}
		if (pBrain.PartyLeader == ParentObject)
		{
			return 100;
		}
		GameObject finalLeader = pBrain.GetFinalLeader();
		if (finalLeader != null)
		{
			return GetFeeling(finalLeader);
		}
		if (ParentObject.HasTag("MarkOfDeathHunter"))
		{
			if (GO.HasMarkOfDeath())
			{
				return -200;
			}
			if (!GO.HasMarkOfDeath())
			{
				return 0;
			}
		}
		if (ParentObject.HasTag("AntiMarkOfDeathHunter"))
		{
			if (GO.HasMarkOfDeath())
			{
				return 0;
			}
			if (!GO.HasMarkOfDeath() && !(GO.Blueprint == ParentObject.Blueprint))
			{
				return -200;
			}
		}
		if (ObjectMemory.TryGetValue(GO, out var value))
		{
			return value.Disposition;
		}
		if (ParentObject.HasTagOrProperty("MarkOfDeathGuardian"))
		{
			if (GO.HasTagOrProperty("MarkOfDeathGuardian"))
			{
				return 50;
			}
			if (GO.HasMarkOfDeath())
			{
				return 0;
			}
			if (!GO.HasMarkOfDeath())
			{
				return -200;
			}
		}
		if (GO.HasTag("Calming"))
		{
			return 50;
		}
		if (ParentObject.HasTag("Calming"))
		{
			return 50;
		}
		if (GO.pBrain != null && GO.pBrain.PartyLeader != null && GO.pBrain.PartyLeader.HasTag("Calming"))
		{
			return 50;
		}
		if (ParentObject.pBrain != null && ParentObject.pBrain.PartyLeader != null && ParentObject.pBrain.PartyLeader.HasTag("Calming"))
		{
			return 50;
		}
		int num = 100;
		if (ParentObject.HasProperty("IsDelegate"))
		{
			return 100;
		}
		using (Dictionary<string, int>.Enumerator enumerator = FactionMembership.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string key = enumerator.Current.Key;
				if (GetFactionAllegiance(enumerator.Current.Value) == FactionAllegiance.member)
				{
					if (pBrain.GetFactionAllegiance(key) == FactionAllegiance.member)
					{
						return 100;
					}
					int feelingFactionToObject = XRL.World.Factions.GetFeelingFactionToObject(key, GO);
					if (feelingFactionToObject < num)
					{
						num = feelingFactionToObject;
					}
				}
			}
		}
		if (!GO.IsPlayer() && pBrain.Target == ParentObject && num >= 0)
		{
			num = -1;
		}
		if (Hostile && num < 50)
		{
			num = Math.Min(num, -50);
		}
		if (Calm && num >= -50 && num < 0)
		{
			num = 0;
		}
		return num;
	}

	public static int GetFeeling(string FactionSpec, GameObject GO)
	{
		if (GO == null)
		{
			return 0;
		}
		Brain brain = GO.pBrain;
		if (brain == null)
		{
			return 0;
		}
		Brain finalLeaderBrain = brain.GetFinalLeaderBrain();
		if (finalLeaderBrain != null)
		{
			brain = finalLeaderBrain;
			GO = brain.ParentObject;
		}
		int num = 100;
		FillFactionMembership(ProcFactions, FactionSpec);
		using Dictionary<string, int>.Enumerator enumerator = ProcFactions.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			if (GetFactionAllegiance(enumerator.Current.Value) == FactionAllegiance.member)
			{
				if (brain.GetFactionAllegiance(key) == FactionAllegiance.member)
				{
					return 100;
				}
				int feelingFactionToObject = XRL.World.Factions.GetFeelingFactionToObject(key, GO);
				if (feelingFactionToObject < num)
				{
					num = feelingFactionToObject;
				}
			}
		}
		return num;
	}

	public static string GetPrimaryFaction(string FactionSpec)
	{
		int num = -999;
		string text = null;
		FillFactionMembership(ProcFactions, FactionSpec);
		foreach (KeyValuePair<string, int> procFaction in ProcFactions)
		{
			if (procFaction.Value > num)
			{
				text = procFaction.Key;
				num = procFaction.Value;
			}
		}
		if (text == null)
		{
			if (!string.IsNullOrEmpty(FactionSpec))
			{
				if (FactionSpec.Contains(","))
				{
					return ExtractFaction(FactionSpec.CachedCommaExpansion()[0]);
				}
				return ExtractFaction(FactionSpec);
			}
			return "Beasts";
		}
		return text;
	}

	public string GetPrimaryFaction()
	{
		int num = int.MinValue;
		string text = null;
		foreach (KeyValuePair<string, int> item in FactionMembership)
		{
			if (item.Value > num)
			{
				text = item.Key;
				num = item.Value;
			}
		}
		if (text != null)
		{
			return text;
		}
		if (!string.IsNullOrEmpty(Factions))
		{
			if (Factions.Contains(","))
			{
				return ExtractFaction(Factions.CachedCommaExpansion()[0]);
			}
			return ExtractFaction(Factions);
		}
		return "Beasts";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public bool IsHostileTowards(GameObject GO)
	{
		return GetOpinion(GO) == CreatureOpinion.hostile;
	}

	public bool IsAlliedTowards(GameObject GO)
	{
		return GetOpinion(GO) == CreatureOpinion.allied;
	}

	public bool IsNeutralTowards(GameObject GO)
	{
		return GetOpinion(GO) == CreatureOpinion.neutral;
	}

	public static CreatureOpinion GetOpinion(string FactionSpec, GameObject GO)
	{
		if (GO == null)
		{
			return CreatureOpinion.neutral;
		}
		int feeling = GetFeeling(FactionSpec, GO);
		if (feeling == 200)
		{
			return CreatureOpinion.neutral;
		}
		if (feeling < 0)
		{
			return CreatureOpinion.hostile;
		}
		if (feeling < 50)
		{
			return CreatureOpinion.neutral;
		}
		return CreatureOpinion.allied;
	}

	public CreatureOpinion GetOpinion(GameObject GO)
	{
		if (GO == null)
		{
			return CreatureOpinion.neutral;
		}
		if (GO == ParentObject)
		{
			return CreatureOpinion.allied;
		}
		if (GO == PartyLeader)
		{
			return CreatureOpinion.allied;
		}
		if (GO.IsPlayer())
		{
			ParentObject.FireEvent(eCheckingHostilityTowardsPlayer);
		}
		if (PartyLeader != null)
		{
			return PartyLeader.GetOpinion(GO);
		}
		int feeling = GetFeeling(GO);
		if (feeling == 200)
		{
			return CreatureOpinion.neutral;
		}
		if (Target == GO)
		{
			return CreatureOpinion.hostile;
		}
		if (feeling < 0)
		{
			return CreatureOpinion.hostile;
		}
		if (feeling < 50)
		{
			return CreatureOpinion.neutral;
		}
		return CreatureOpinion.allied;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != CommandTakeActionEvent.ID && ID != GeneralAmnestyEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != GetTradePerformanceEvent.ID && ID != GetWaterRitualLiquidEvent.ID && ID != ObjectCreatedEvent.ID && ID != SuspendingEvent.ID && ID != TookDamageEvent.ID)
		{
			if (ID == TookEnvironmentalDamageEvent.ID)
			{
				return !IsPlayer();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!The.ActionManager.ActionQueue.Contains(ParentObject) && E.Cell.ParentZone.IsActive())
		{
			ParentObject.MakeActive();
		}
		if (!Wanders && !WandersRandomly && StartingCell == null && isMobile() && !ParentObject.HasPropertyOrTag("NoStay"))
		{
			StartingCell = ParentObject.CurrentCell.GetGlobalLocation();
		}
		if (DoReequip)
		{
			DoReequip = false;
			PerformEquip(IsPlayer: false, Silent: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTradePerformanceEvent E)
	{
		if (E.Actor != null && E.Actor.IsPlayer() && E.Trader != null)
		{
			Faction ifExists = XRL.World.Factions.getIfExists(E.Trader.GetPrimaryFaction());
			if (ifExists != null)
			{
				E.LinearAdjustment += The.Game.PlayerReputation.getTradePerformance(ifExists);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetWaterRitualLiquidEvent E)
	{
		string text = XRL.World.Factions.getIfExists(GetPrimaryFaction())?.getWaterRitualLiquid(E.Target);
		if (!string.IsNullOrEmpty(text) && (!(text == "water") || string.IsNullOrEmpty(E.Liquid)))
		{
			E.Liquid = text;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (pPhysics.CurrentCell == null || pPhysics.CurrentCell.IsGraveyard())
		{
			return true;
		}
		Hibernating = false;
		if (E.Actor == null || E.Actor == ParentObject || InSamePartyAs(E.Actor))
		{
			return true;
		}
		bool flag = E.Damage != null && E.Damage.HasAttribute("Accidental");
		if (flag && !FriendlyFireIncident(E.Actor))
		{
			return true;
		}
		if (!ParentObject.FireEvent(Event.New("CanBeAngeredByDamage", "Attacker", E.Actor, "Damage", E.Damage)))
		{
			return true;
		}
		if (ParentObject.IsPlayer())
		{
			BroadcastForHelp(E.Actor);
			return true;
		}
		int feeling = GetFeeling(E.Actor);
		AdjustFeeling(E.Actor, -10);
		if (PartyLeader != null && !PartyLeader.IsPlayer())
		{
			PartyLeader.pBrain.AdjustFeeling(E.Actor, -10);
		}
		if (GetFeeling(E.Actor) < 0)
		{
			if (feeling >= 0 && E.Actor.IsPlayer())
			{
				if (flag)
				{
					ParentObject.SetIntProperty("PlayerAngeredAccidentally", 1);
				}
				else
				{
					ParentObject.RemoveIntProperty("PlayerAngeredAccidentally");
				}
			}
			BroadcastForHelp(E.Actor);
			if ((Target == null || E.Actor.IsPlayer()) && CanFight())
			{
				WantToKill(E.Actor, "because I was injured");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookEnvironmentalDamageEvent E)
	{
		if (!IsPlayer() && MovingTo() == null)
		{
			PushGoal(new FleeLocation(ParentObject.CurrentCell, 1));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		StopFighting();
		ObjectMemory.Clear();
		FriendlyFire = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		E.AddEntry(this, "PartyLeader", PartyLeader);
		E.AddEntry(this, "Wanders", Wanders);
		E.AddEntry(this, "WandersRandomly", WandersRandomly);
		if (Goals.Count > 0)
		{
			stringBuilder.Clear();
			for (int num = Goals.Count - 1; num > 0; num--)
			{
				int num2 = 1;
				string description = Goals.Items[num].GetDescription();
				while (num > 0 && Goals.Items[num - 1].GetDescription() == description)
				{
					num2++;
					num--;
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('\n');
				}
				stringBuilder.Append(description);
				if (num2 > 1)
				{
					stringBuilder.Append(" x").Append(num2);
				}
			}
			E.AddEntry(this, "Goals", stringBuilder.ToString());
		}
		else
		{
			E.AddEntry(this, "Goals", "none");
		}
		E.AddEntry(this, "MinKillRadius", MinKillRadius);
		E.AddEntry(this, "MaxKillRadius", MaxKillRadius);
		E.AddEntry(this, "Hostile walk radius to player", ParentObject.GetHostileWalkRadius(The.Player));
		E.AddEntry(this, "Last thought", string.IsNullOrEmpty(LastThought) ? "none" : LastThought);
		if (FactionMembership.Count > 0)
		{
			stringBuilder.Clear();
			List<string> list = new List<string>(FactionMembership.Keys);
			list.Sort();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(list[i]).Append('-').Append(FactionMembership[list[i]]);
			}
			E.AddEntry(this, "Faction membership", stringBuilder.ToString());
		}
		else
		{
			E.AddEntry(this, "Faction membership", "none");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Hostile)
		{
			E.Postfix.Append("\nBase demeanor: {{r|aggressive}}");
		}
		if (Calm)
		{
			E.Postfix.Append("\nBase demeanor: {{g|docile}}");
		}
		if (Passive)
		{
			E.Postfix.Append("\nEngagement style: {{g|defensive}}");
		}
		else if (IsPlayerLed())
		{
			E.Postfix.Append("\nEngagement style: {{r|aggressive}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Hibernating && 50.in100())
		{
			Hibernating = false;
		}
		if (!string.IsNullOrEmpty(Factions))
		{
			InitFromFactions();
		}
		ParentObject.FireEvent(eFactionsAdded);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		FriendlyFire = null;
		ObjectMemory.Clear();
		if (isMobile() && PartyLeader != null && PartyLeader.IsPlayer())
		{
			if (!PartyLeader.InSameZone(ParentObject))
			{
				Goals.Clear();
			}
		}
		else
		{
			Goals.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		if (pPhysics == null)
		{
			pPhysics = ParentObject.pPhysics;
		}
		objectsToRemove.Clear();
		foreach (GameObject key in ObjectMemory.Keys)
		{
			Zone currentZone = key.CurrentZone;
			if (currentZone == null || !currentZone.IsActive())
			{
				objectsToRemove.Add(key);
			}
		}
		if (objectsToRemove.Count > 0)
		{
			foreach (GameObject item in objectsToRemove)
			{
				ObjectMemory.Remove(item);
			}
		}
		if (FriendlyFire != null)
		{
			objectsToRemove.Clear();
			foreach (GameObject key2 in FriendlyFire.Keys)
			{
				Zone currentZone2 = key2.CurrentZone;
				if (currentZone2 == null || !currentZone2.IsActive())
				{
					objectsToRemove.Add(key2);
				}
			}
			if (objectsToRemove.Count > 0)
			{
				foreach (GameObject item2 in objectsToRemove)
				{
					FriendlyFire.Remove(item2);
				}
				if (FriendlyFire.Count == 0)
				{
					FriendlyFire = null;
				}
			}
		}
		if (IsPlayerLed())
		{
			ParentObject.ModIntProperty("TurnsAsPlayerMinion", 1);
		}
		if (!ParentObject.FireEvent(eBeforeAITakingAction))
		{
			return true;
		}
		if (ParentObject.IsPlayer())
		{
			if (Options.DisablePlayerbrain)
			{
				return true;
			}
		}
		else if (The.Core.Calm)
		{
			ParentObject.UseEnergy(1000);
			return true;
		}
		if (PartyLeader != null && PartyLeader.HasRegisteredEvent("MinionTakingAction"))
		{
			PartyLeader.FireEvent(Event.New("MinionTakingAction", "Object", ParentObject));
		}
		if (ParentObject.IsInvalid())
		{
			return true;
		}
		if (ParentObject.IsNowhere())
		{
			ParentObject.UseEnergy(1000);
			return true;
		}
		if (!ParentObject.IsPlayer())
		{
			if (GoToPartyLeader())
			{
				return true;
			}
			if (Hibernating)
			{
				ParentObject.UseEnergy(1000);
				return true;
			}
			if (pPhysics.Temperature <= pPhysics.BrittleTemperature)
			{
				Think("I'm frozen!");
				ParentObject.UseEnergy(1000);
				return true;
			}
			ParentObject.FireEvent(eAITakingAction);
			Cell cell = pPhysics.CurrentCell;
			if (ParentObject.HasEffect("Confused"))
			{
				string randomDirection = Directions.GetRandomDirection();
				Cell localCellFromDirection = cell.GetLocalCellFromDirection(randomDirection);
				if (localCellFromDirection != null)
				{
					if (limitToAquatic() && !localCellFromDirection.HasAquaticSupportFor(ParentObject))
					{
						ParentObject.UseEnergy(1000);
						return true;
					}
					if (!localCellFromDirection.IsEmpty())
					{
						ParentObject.UseEnergy(1000);
						return true;
					}
				}
				ParentObject.Move(randomDirection);
				return true;
			}
			if (DoReequip)
			{
				DoReequip = false;
				PerformReequip();
			}
			while (Goals.Count > 0 && Goals.Peek().Finished())
			{
				Goals.Pop();
			}
			if (Target == null && PartyLeader != null && PartyLeader.IsPlayerControlled() && CanAcquireTarget())
			{
				GameObject target = PartyLeader.Target;
				if (target != null && CheckPerceptionOf(target))
				{
					WantToKill(target, "to aid my leader");
				}
			}
			if (Target == null && cell != null)
			{
				bool flag = !IsPlayerLed();
				GameObject gameObject = FindProspectiveTarget(null, flag);
				if (gameObject != null)
				{
					WantToKill(gameObject, "out of hostility");
				}
				else if (flag)
				{
					Think("I looked for the player to target but didn't find them.");
				}
				else
				{
					Think("I looked for a target but didn't find one.");
				}
			}
			if (Goals.Count == 0)
			{
				new Bored().Push(this);
			}
		}
		ParentObject.FireEvent(eTakingAction);
		while (Goals.Count > 0 && Goals.Peek().Finished())
		{
			Goals.Pop();
		}
		if (Goals.Count > 0)
		{
			Goals.Peek().TakeAction();
			if (ParentObject.IsPlayer())
			{
				The.Core.RenderDelay(200);
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
		Object.RegisterPartEvent(this, "AIMessage");
		Object.RegisterPartEvent(this, "AIDistractionBroadcast");
		Object.RegisterPartEvent(this, "AIHelpBroadcast");
		Object.RegisterPartEvent(this, "AIWakeupBroadcast");
		base.Register(Object);
	}

	public void Stay(Cell C)
	{
		if (C == null)
		{
			Staying = false;
			return;
		}
		if (isMobile() && !Wanders && !WandersRandomly)
		{
			StartingCell = C.GetGlobalLocation();
		}
		Staying = true;
	}

	public void FleeTo(Cell c, int duration = 3)
	{
		PushGoal(new FleeLocation(c, duration));
	}

	public void MoveTo(Cell c, bool clearFirst = true)
	{
		MoveTo(c.ParentZone, c.location, clearFirst);
	}

	public void MoveTo(Zone z, Location2D location, bool clearFirst = true)
	{
		if (clearFirst)
		{
			Goals.Clear();
		}
		PushGoal(new MoveTo(z.GetCell(location)));
	}

	public void MoveTo(GameObject go, bool clearFirst = true)
	{
		if (clearFirst)
		{
			Goals.Clear();
		}
		PushGoal(new MoveTo(go));
	}

	public void MoveToGlobal(string zone, int x, int y)
	{
		Goals.Clear();
		PushGoal(new MoveToGlobal(zone, x, y));
	}

	public void StopMoving()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			GoalHandler goalHandler = Goals.Items[num] as IMovementGoal;
			if (goalHandler != null)
			{
				int count = Goals.Items.Count;
				goalHandler.FailToParent();
				if (count != Goals.Items.Count)
				{
					num = Goals.Items.Count;
				}
			}
		}
	}

	public bool IsTryingToJoinPartyLeader()
	{
		GameObject partyLeader = PartyLeader;
		if (partyLeader == null || partyLeader.IsNowhere())
		{
			return false;
		}
		if (Staying)
		{
			return false;
		}
		if (ParentObject.HasEffect("Dominated"))
		{
			return false;
		}
		if (ParentObject.HasTagOrProperty("DoesNotJoinPartyLeader"))
		{
			return false;
		}
		Cell TargetCell = partyLeader.CurrentCell;
		Cell cell = ParentObject.CurrentCell;
		if (TargetCell == null || cell == null || TargetCell.ParentZone == null || cell.ParentZone == null || TargetCell.ParentZone == cell.ParentZone)
		{
			return false;
		}
		if (TargetCell.ParentZone.IsWorldMap())
		{
			return false;
		}
		if (!JoinPartyLeaderPossibleEvent.Check(ParentObject, partyLeader, cell, ref TargetCell, ParentObject.IsPotentiallyMobile()))
		{
			return false;
		}
		return true;
	}

	public bool GoToPartyLeader()
	{
		GameObject partyLeader = PartyLeader;
		if (partyLeader == null || partyLeader.IsNowhere())
		{
			return false;
		}
		if (Staying)
		{
			return false;
		}
		if (ParentObject.HasEffect("Dominated"))
		{
			return false;
		}
		Cell TargetCell = partyLeader.CurrentCell;
		Cell cell = ParentObject.CurrentCell;
		if (TargetCell == null || TargetCell.ParentZone == null || (cell != null && TargetCell.ParentZone == cell.ParentZone))
		{
			return false;
		}
		if (TargetCell.ParentZone.IsWorldMap())
		{
			return false;
		}
		bool flag = ParentObject.HasTagOrProperty("DoesNotJoinPartyLeader");
		bool result = false;
		if (!flag && JoinPartyLeaderPossibleEvent.Check(ParentObject, partyLeader, cell, ref TargetCell, ParentObject.IsMobile()))
		{
			Goals.Clear();
			if (!TargetCell.ParentZone.IsWorldMap())
			{
				List<Cell> list = TargetCell.GetPassableConnectedAdjacentCellsFor(ParentObject, 3).ShuffleInPlace();
				if (TargetCell.IsPassable(ParentObject))
				{
					list.Insert(0, TargetCell);
				}
				Cell cell2 = null;
				int num = int.MaxValue;
				int distanceFromPreviousCell = 0;
				int distanceFromLeader = 0;
				foreach (Cell item in list)
				{
					if (item.GetNavigationWeightFor(ParentObject) < 10 && !item.HasCombatObject() && !item.IsSolidFor(ParentObject))
					{
						int num2 = cell?.PathDistanceTo(item) ?? 0;
						int num3 = item.PathDistanceTo(TargetCell);
						int num4 = num2 + num3;
						if (num4 < num && CanJoinPartyLeaderEvent.Check(ParentObject, partyLeader, cell, item, num2, num3))
						{
							cell2 = item;
							num = num4;
							distanceFromPreviousCell = num2;
							distanceFromLeader = num3;
						}
					}
				}
				if (cell2 != null)
				{
					Think("I'm going to join my leader.");
					ParentObject.SystemLongDistanceMoveTo(cell2);
					ParentObject.UseEnergy(1000, "Move Join Leader");
					JoinedPartyLeaderEvent.Send(ParentObject, partyLeader, cell, cell2, distanceFromPreviousCell, distanceFromLeader);
					result = true;
				}
			}
		}
		if ((flag || (!partyLeader.IsPlayer() && !partyLeader.LeftBehindByPlayer())) && (partyLeader.CurrentCell == null || partyLeader.CurrentZone != ParentObject.CurrentZone))
		{
			PartyLeader = null;
		}
		return result;
	}

	public void BroadcastForHelp(GameObject Target)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && !cell.IsGraveyard())
		{
			Event @event = Event.New("AIHelpBroadcast");
			@event.SetParameter("Victim", ParentObject);
			@event.SetParameter("Target", Target);
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 20, "Brain", ParentObject);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].FireEvent(@event);
			}
		}
	}

	public void Wake()
	{
		if (!Hibernating)
		{
			return;
		}
		Hibernating = false;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && !cell.IsGraveyard() && cell.ParentZone != null)
		{
			Event e = Event.New("AIWakeupBroadcast");
			List<GameObject> list = cell.ParentZone.FastFloodVisibility(cell.X, cell.Y, 5, "Brain", ParentObject);
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i].FireEvent(e);
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		Wake();
		if (limitToAquatic())
		{
			LiquidVolume liquidVolume = ParentObject.CurrentCell?.GetAquaticSupportFor(ParentObject)?.LiquidVolume;
			if (liquidVolume != null)
			{
				liquidVolume.GetPrimaryLiquid()?.RenderBackgroundPrimary(liquidVolume, E);
				liquidVolume.GetSecondaryLiquid()?.RenderBackgroundSecondary(liquidVolume, E);
			}
		}
		return true;
	}

	public static FactionAllegiance GetFactionAllegiance(int? Membership)
	{
		if (Membership.HasValue)
		{
			if (Membership >= 75)
			{
				return FactionAllegiance.member;
			}
			if (Membership >= 50)
			{
				return FactionAllegiance.affiliated;
			}
			if (Membership > 0)
			{
				return FactionAllegiance.associated;
			}
		}
		return FactionAllegiance.none;
	}

	public static FactionAllegiance GetFactionAllegiance(Dictionary<string, int> Membership, string Faction)
	{
		if (Membership.ContainsKey(Faction))
		{
			return GetFactionAllegiance(Membership[Faction]);
		}
		return FactionAllegiance.none;
	}

	public FactionAllegiance GetFactionAllegiance(string Faction)
	{
		return GetFactionAllegiance(FactionMembership, Faction);
	}

	public bool CheckVisibilityOf(GameObject who)
	{
		if (ParentObject.IsPlayer())
		{
			if (who.IsVisible())
			{
				return true;
			}
		}
		else
		{
			if (ParentObject.HasPart("Clairvoyance") && ParentObject.Stat("Intelligence").in100())
			{
				return true;
			}
			if (ParentObject.HasLOSTo(who, IncludeSolid: true, UseTargetability: true))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPerceptionOf(GameObject who)
	{
		if (CheckVisibilityOf(who))
		{
			return true;
		}
		if (who.IsAudible(ParentObject))
		{
			return true;
		}
		if (who.IsSmellable(ParentObject))
		{
			return true;
		}
		return false;
	}

	public bool IsSuitableTarget(GameObject who)
	{
		if (ParentObject.IsPlayer() || IsPlayerLed())
		{
			if (who.HasEffectByClass("Asleep"))
			{
				return false;
			}
			if ((who.IsHostileTowards(IComponent<GameObject>.ThePlayer) || who.IsHostileTowards(ParentObject)) && CheckPerceptionOf(who))
			{
				if (who.DistanceTo(IComponent<GameObject>.ThePlayer) <= who.GetHostileWalkRadius(IComponent<GameObject>.ThePlayer))
				{
					return true;
				}
				if (ParentObject != IComponent<GameObject>.ThePlayer && who.DistanceTo(ParentObject) <= who.GetHostileWalkRadius(ParentObject))
				{
					return true;
				}
				GameObject target = who.Target;
				if (target != null && target != IComponent<GameObject>.ThePlayer && target != ParentObject && target.IsPlayerLed() && who.DistanceTo(target) <= who.GetHostileWalkRadius(target))
				{
					return true;
				}
			}
		}
		else if (IsHostileTowards(who) && CheckPerceptionOf(who))
		{
			return true;
		}
		return false;
	}

	public bool IsSuitablePlayerControlledTarget(GameObject who)
	{
		if (!who.IsPlayerControlled())
		{
			return false;
		}
		if (IsHostileTowards(who) && CheckPerceptionOf(who))
		{
			return true;
		}
		return false;
	}

	public int TargetSort(GameObject who1, GameObject who2)
	{
		int num = PreferTargetEvent.Check(ParentObject, who1, who2);
		if (num != 0)
		{
			return -num;
		}
		int num2 = who1.IsPlayer().CompareTo(who2.IsPlayer());
		if (num2 != 0)
		{
			return -num2;
		}
		int num3 = who1.PhaseMatches(ParentObject).CompareTo(who2.PhaseMatches(ParentObject));
		if (num3 != 0)
		{
			return -num3;
		}
		int num4 = who1.FlightMatches(ParentObject).CompareTo(who2.FlightMatches(ParentObject));
		if (num4 != 0)
		{
			return -num4;
		}
		int num5 = who1.DistanceTo(ParentObject).CompareTo(who2.DistanceTo(ParentObject));
		if (num5 != 0)
		{
			return -num5;
		}
		return 0;
	}

	public static int ExtractFactionMembership(ref string spec)
	{
		int num = spec.LastIndexOf('-');
		if (num == -1)
		{
			MetricsManager.LogError("Invalid faction membership specification: " + spec);
			return 0;
		}
		if (!int.TryParse(spec.Substring(num + 1), out var result) || result <= 0)
		{
			MetricsManager.LogError("Invalid faction membership specification: " + spec);
			return 0;
		}
		spec = spec.Substring(0, num);
		return result;
	}

	public static int ExtractFactionMembership(string spec)
	{
		return ExtractFactionMembership(ref spec);
	}

	public static string ExtractFaction(string spec)
	{
		ExtractFactionMembership(ref spec);
		return spec;
	}

	public static void FillFactionMembership(Dictionary<string, int> Map, string Spec)
	{
		Map.Clear();
		if (string.IsNullOrEmpty(Spec))
		{
			return;
		}
		if (Spec.Contains(","))
		{
			foreach (string item in Spec.CachedCommaExpansion())
			{
				string spec = item;
				int num = ExtractFactionMembership(ref spec);
				if (num > 0)
				{
					if (Map.TryGetValue(spec, out var value))
					{
						if (num > value)
						{
							Map[spec] = num;
						}
					}
					else
					{
						Map.Add(spec, num);
					}
				}
			}
			return;
		}
		int num2 = ExtractFactionMembership(ref Spec);
		if (num2 <= 0)
		{
			return;
		}
		if (Map.TryGetValue(Spec, out var value2))
		{
			if (num2 > value2)
			{
				Map[Spec] = num2;
			}
		}
		else
		{
			Map.Add(Spec, num2);
		}
	}

	public void InitFromFactions()
	{
		FillFactionMembership(FactionMembership, Factions);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIWakeupBroadcast")
		{
			Hibernating = false;
		}
		else if (E.ID == "AIDistractionBroadcast")
		{
			if (ParentObject.IsInvalid())
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				return true;
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("OriginalTarget");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("DistractionTarget");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("DistractionGeneratedBy");
			if (Target == gameObjectParameter2)
			{
				return true;
			}
			if (PartyLeader == gameObjectParameter2)
			{
				return true;
			}
			if ((Target == gameObjectParameter || GetFeeling(gameObjectParameter) < 0) && ParentObject.DistanceTo(gameObjectParameter2) <= MaxKillRadius && !ParentObject.MakeSave("Intelligence", E.GetIntParameter("Difficulty"), null, null, "Hologram Illusion Distraction", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, gameObjectParameter3))
			{
				SetFeeling(gameObjectParameter2, Math.Min(GetFeeling(gameObjectParameter2) - 50, -50));
				Target = null;
				WantToKill(gameObjectParameter2, "out of distraction");
			}
		}
		else if (E.ID == "AIHelpBroadcast")
		{
			if (ParentObject == null)
			{
				return true;
			}
			if (ParentObject.IsInvalid())
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				return true;
			}
			if (ParentObject.HasEffect("Asleep"))
			{
				if (ParentObject.FireEvent("CanWakeUpOnHelpBroadcast"))
				{
					ParentObject.FireEvent("WakeUp");
				}
				return true;
			}
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Target");
			if (gameObjectParameter4 == null)
			{
				return true;
			}
			if (gameObjectParameter4.pBrain != null)
			{
				if (gameObjectParameter4 == PartyLeader)
				{
					return true;
				}
				if (gameObjectParameter4.pBrain.PartyLeader == ParentObject)
				{
					return true;
				}
				if (PartyLeader != null && gameObjectParameter4.pBrain.PartyLeader == PartyLeader)
				{
					return true;
				}
			}
			if (E.HasParameter("Victim"))
			{
				GameObject gameObjectParameter5 = E.GetGameObjectParameter("Victim");
				if (Target == null && ParentObject.DistanceTo(gameObjectParameter4) <= MaxKillRadius && GetFeeling(gameObjectParameter5) >= 50)
				{
					SetFeeling(gameObjectParameter4, GetFeeling(gameObjectParameter4) - ParentObject.GetIntProperty("HelpModifier", 50));
					if (IsHostileTowards(gameObjectParameter4) && CanFight() && (!Passive || !HasGoal()))
					{
						WantToKill(gameObjectParameter4, "to help " + gameObjectParameter5.a + gameObjectParameter5.BaseDisplayName);
						return true;
					}
				}
			}
			if (E.HasParameter("Faction") && Target == null && ParentObject.DistanceTo(gameObjectParameter4) <= MaxKillRadius)
			{
				string stringParameter = E.GetStringParameter("Faction");
				FactionAllegiance factionAllegiance = GetFactionAllegiance(stringParameter);
				if (factionAllegiance == FactionAllegiance.member || factionAllegiance == FactionAllegiance.affiliated)
				{
					GameObject gameObjectParameter6 = E.GetGameObjectParameter("Owned");
					if (gameObjectParameter6 == null || ParentObject.FireEvent(Event.New("CanBeAngeredByPropertyCrime", "Attacker", gameObjectParameter4, "Object", gameObjectParameter6, "Faction", stringParameter)))
					{
						SetFeeling(gameObjectParameter4, GetFeeling(gameObjectParameter4) - 50);
						if (IsHostileTowards(gameObjectParameter4) && CanFight())
						{
							GameObject gameObjectParameter7 = E.GetGameObjectParameter("Owner");
							WantToKill(gameObjectParameter4, (gameObjectParameter7 != null) ? ("to protect " + gameObjectParameter7.a + gameObjectParameter7.BaseDisplayName) : ("for " + Faction.getFormattedName(stringParameter)));
							return true;
						}
					}
				}
			}
		}
		else if (E.ID == "AIMessage")
		{
			if (pPhysics.CurrentCell == null || pPhysics.CurrentCell.IsGraveyard())
			{
				return true;
			}
			if (ParentObject.IsPlayer())
			{
				return true;
			}
			Wake();
			if (E.GetStringParameter("Message") == "Attacked")
			{
				GameObject gameObjectParameter8 = E.GetGameObjectParameter("By");
				if (gameObjectParameter8 != null && gameObjectParameter8 != ParentObject && !InSamePartyAs(gameObjectParameter8) && (gameObjectParameter8.IsCreature || 3.in100()) && ParentObject.FireEvent(Event.New("CanBeAngeredByBeingAttacked", "Attacker", gameObjectParameter8)))
				{
					if (IsPlayerLed())
					{
						if (!gameObjectParameter8.IsPlayerControlled() && Target == null)
						{
							WantToKill(gameObjectParameter8, "because I was attacked");
						}
					}
					else
					{
						AdjustFeeling(gameObjectParameter8, -10);
						if (PartyLeader != null)
						{
							PartyLeader.pBrain.AdjustFeeling(gameObjectParameter8, -10);
						}
						if (GetFeeling(gameObjectParameter8) < 0 && CanFight() && (Target == null || GetFeeling(Target) > GetFeeling(gameObjectParameter8)))
						{
							BroadcastForHelp(gameObjectParameter8);
							WantToKill(gameObjectParameter8, "because I was attacked and am now angry at the attacker");
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public bool CanAcquireTarget()
	{
		if (Passive)
		{
			return false;
		}
		if (!CanFight())
		{
			return false;
		}
		if (!ParentObject.FireEvent("AILookForTarget"))
		{
			return false;
		}
		return true;
	}

	public GameObject FindProspectiveTarget(Cell FromCell = null, bool WantPlayer = false)
	{
		if (!CanAcquireTarget())
		{
			return null;
		}
		if (FromCell == null)
		{
			FromCell = ParentObject.CurrentCell;
			if (FromCell == null)
			{
				return null;
			}
		}
		Predicate<GameObject> predicate = null;
		predicate = ((!WantPlayer) ? new Predicate<GameObject>(IsSuitableTarget) : new Predicate<GameObject>(IsSuitablePlayerControlledTarget));
		List<GameObject> list = FromCell.ParentZone.FastCombatSquareVisibility(FromCell.X, FromCell.Y, Stat.Random(MinKillRadius, MaxKillRadius), ParentObject, predicate, VisibleToPlayerOnly: false, IncludeWalls: true, IncludeLooker: false);
		if (list != null && list.Count > 0)
		{
			if (list.Count > 1)
			{
				list.Sort(TargetSort);
			}
			return list[0];
		}
		return null;
	}

	public bool FriendlyFireIncident(GameObject GO)
	{
		CreatureOpinion opinion = GetOpinion(GO);
		int value = 1;
		if (opinion == CreatureOpinion.allied || opinion == CreatureOpinion.neutral)
		{
			if (!ParentObject.FireEvent(Event.New("CanBeAngeredByFriendlyFire", "Attacker", GO)))
			{
				return false;
			}
			if (ParentObject.Stat("Intelligence") < 7)
			{
				return true;
			}
			if (FriendlyFire == null)
			{
				FriendlyFire = new Dictionary<GameObject, int> { { GO, value } };
			}
			else if (FriendlyFire.TryGetValue(GO, out value))
			{
				value++;
				FriendlyFire[GO] = value;
			}
			else
			{
				FriendlyFire.Add(GO, value);
			}
			switch (opinion)
			{
			case CreatureOpinion.allied:
				if (GO.IsPlayer() || GO.IsPlayerLed())
				{
					if (value < 5 && Stat.Random(0, 200) > value)
					{
						return false;
					}
				}
				else if (value < 5 && Stat.Random(0, 500) > value)
				{
					return false;
				}
				break;
			case CreatureOpinion.neutral:
				if (GO.IsPlayer() || GO.IsPlayerLed())
				{
					if (value < 3 && Stat.Random(0, 100) > value)
					{
						return false;
					}
				}
				else if (value < 5 && Stat.Random(0, 200) > value)
				{
					return false;
				}
				break;
			}
		}
		return true;
	}

	public static double PreciseWeaponScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		string inventoryCategory = obj.GetInventoryCategory();
		if (inventoryCategory == "Ammo")
		{
			return 0.0;
		}
		if (!(obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon))
		{
			return 0.0;
		}
		int num = meleeWeapon.BaseDamage.RollMinCached();
		int num2 = meleeWeapon.BaseDamage.RollMaxCached();
		double num3 = num * 2 + num2;
		meleeWeapon.GetNormalPenetration(who, out var BasePenetration, out var StatMod);
		int num4;
		if (meleeWeapon.CheckAdaptivePenetration(out var Bonus))
		{
			num4 = BasePenetration + Bonus + 1;
			if (who != null)
			{
				num4 = ((!Statistic.IsMental(meleeWeapon.Stat)) ? (num4 + Stats.GetCombatAV(who)) : (num4 + Stats.GetCombatMA(who)));
			}
		}
		else
		{
			num4 = BasePenetration + StatMod - 1;
		}
		double num5 = ((num4 < 1) ? (num3 / 2.0) : (num3 * (double)num4));
		double num6 = 0.0;
		double num7 = 50 + meleeWeapon.HitBonus * 5;
		if (who != null)
		{
			num7 += (double)(who.StatMod("Agility") * 5);
			if (who.HasSkill(meleeWeapon.Skill))
			{
				num7 += (double)((meleeWeapon.Skill == "ShortBlades") ? 5 : 10);
				num7 += 10.0;
			}
			if ((meleeWeapon.Skill == "LongBlades" || meleeWeapon.Skill == "ShortBlades") && who.HasPart("LongBladesCore"))
			{
				if (meleeWeapon.Skill == "LongBlades")
				{
					if (who.HasSkill("LongBladesLunge"))
					{
						num6 += 1.0;
					}
					if (who.HasSkill("LongBladesSwipe"))
					{
						num6 += 1.0;
					}
					if (who.HasSkill("LongBladesDeathblow"))
					{
						num6 += 1.0;
					}
				}
				if (who.HasEffect("LongbladeStance_Aggressive"))
				{
					num7 = ((!who.HasPart("LongBladesImprovedAggressiveStance")) ? (num7 - 10.0) : (num7 - 15.0));
				}
				else if (who.HasEffect("LongbladeStance_Duelist"))
				{
					num7 = ((!who.HasPart("LongBladesImprovedDuelistStance")) ? (num7 + 10.0) : (num7 + 15.0));
				}
			}
		}
		num7 = Math.Min(Math.Max(num7, 5.0), 100.0);
		num6 += num5 * num7 / 50.0;
		if (obj.HasTag("Storied"))
		{
			num6 += 5.0;
		}
		string usesSlots = obj.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots))
		{
			num6 = num6 * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		else if (obj.pPhysics.UsesTwoSlots)
		{
			num6 = num6 * 2.0 / 3.0;
		}
		if (meleeWeapon.Ego != 0)
		{
			num6 += (double)meleeWeapon.Ego;
		}
		if (inventoryCategory == "Melee Weapon" || inventoryCategory == "Natural Weapon")
		{
			num6 += 1.0;
		}
		if (num7 != 50.0)
		{
			num6 += (num7 - 50.0) / 5.0;
		}
		string tag = obj.GetTag("AdjustWeaponScore");
		if (tag != null)
		{
			num6 += Convert.ToDouble(tag);
		}
		if (obj.HasRegisteredEvent("AdjustWeaponScore"))
		{
			int num8 = (int)Math.Round(num6, MidpointRounding.AwayFromZero);
			Event @event = Event.New("AdjustWeaponScore", "Score", num8, "OriginalScore", num6, "User", who);
			obj.FireEvent(@event);
			int intParameter = @event.GetIntParameter("Score");
			if (num8 != intParameter)
			{
				num6 += (double)(intParameter - num8);
			}
		}
		return num6;
	}

	public static int WeaponScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseWeaponScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareWeapons(GameObject Weapon1, GameObject Weapon2, GameObject POV)
	{
		if (Weapon1 == Weapon2)
		{
			return 0;
		}
		if (Weapon1 == null)
		{
			return 1;
		}
		if (Weapon2 == null)
		{
			return -1;
		}
		int num = Weapon1.HasTagOrProperty("AlwaysEquipAsWeapon").CompareTo(Weapon2.HasTagOrProperty("AlwaysEquipAsWeapon"));
		if (num != 0)
		{
			return -num;
		}
		int num2 = Weapon1.HasPart("MissileWeapon").CompareTo(Weapon2.HasPart("MissileWeapon"));
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = Weapon1.IsThrownWeapon.CompareTo(Weapon2.IsThrownWeapon);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = Weapon1.HasPart("Food").CompareTo(Weapon2.HasPart("Food"));
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = Weapon1.HasPart("Armor").CompareTo(Weapon2.HasPart("Armor"));
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = Weapon1.HasEffect("Rusted", "Broken").CompareTo(Weapon2.HasEffect("Rusted", "Broken"));
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = (Weapon1.HasTag("NaturalGear") && !Weapon1.HasTag("UndesireableWeapon")).CompareTo(Weapon2.HasTag("NaturalGear") && !Weapon2.HasTag("UndesireableWeapon"));
		if (num7 != 0)
		{
			return -num7;
		}
		int num8 = (Weapon1.HasTag("MeleeWeapon") || Weapon1.HasTag("NaturalGear")).CompareTo(Weapon2.HasTag("MeleeWeapon") || Weapon2.HasTag("NaturalGear"));
		if (num8 != 0)
		{
			return -num8;
		}
		int num9 = Weapon1.HasTag("UndesireableWeapon").CompareTo(Weapon2.HasTag("UndesireableWeapon"));
		if (num9 != 0)
		{
			return num9;
		}
		double num10 = PreciseWeaponScore(Weapon1, POV);
		double value = PreciseWeaponScore(Weapon2, POV);
		return -num10.CompareTo(value);
	}

	public static bool IsNewWeaponBetter(GameObject NewWeapon, GameObject OldWeapon, GameObject POV)
	{
		return CompareWeapons(NewWeapon, OldWeapon, POV) < 0;
	}

	public bool IsNewWeaponBetter(GameObject NewWeapon, GameObject OldWeapon)
	{
		return IsNewWeaponBetter(NewWeapon, OldWeapon, ParentObject);
	}

	public static double PreciseMissileWeaponScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		if (!(obj.GetPart("MissileWeapon") is MissileWeapon missileWeapon))
		{
			return 0.0;
		}
		GetMissileWeaponPerformanceEvent @for = GetMissileWeaponPerformanceEvent.GetFor(who, obj);
		if (string.IsNullOrEmpty(@for.BaseDamage))
		{
			return 0.0;
		}
		int num = @for.BaseDamage.RollMinCached();
		int num2 = @for.BaseDamage.RollMaxCached();
		double num3 = num * 2 + num2;
		int num4 = @for.BasePenetration - 1;
		double num5 = ((num4 < 1) ? (num3 / 2.0) : (num3 * (double)num4));
		double num6 = 50 - missileWeapon.WeaponAccuracy;
		if (who != null)
		{
			num6 += (double)(who.StatMod(missileWeapon.Modifier) * 3);
			if (missileWeapon.IsSkilled(who))
			{
				num6 += 6.0;
			}
		}
		num6 = Math.Min(Math.Max(num6, 5.0), 100.0);
		double num7 = num5 * (double)missileWeapon.ShotsPerAction * num6 / 50.0;
		if (obj.HasTag("Storied"))
		{
			num7 += 5.0;
		}
		string usesSlots = obj.UsesSlots;
		if (CanFireAllMissileWeaponsEvent.Check(who))
		{
			if (!string.IsNullOrEmpty(usesSlots))
			{
				num7 = num7 * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
			}
			else if (obj.UsesTwoSlotsFor(who))
			{
				num7 = num7 * 2.0 / 3.0;
			}
		}
		else if (!string.IsNullOrEmpty(usesSlots))
		{
			num7 = num7 * 3.0 / (double)(usesSlots.CachedCommaExpansion().Count + 2);
		}
		else if (obj.UsesTwoSlotsFor(who))
		{
			num7 = num7 * 4.0 / 5.0;
		}
		if (num6 != 50.0)
		{
			num7 += (num6 - 50.0) / 5.0;
		}
		string tag = obj.GetTag("AdjustMissileWeaponScore");
		if (tag != null)
		{
			num7 += Convert.ToDouble(tag);
		}
		if (obj.HasRegisteredEvent("AdjustMissileWeaponScore"))
		{
			int num8 = (int)Math.Round(num7, MidpointRounding.AwayFromZero);
			Event @event = Event.New("AdjustMissileWeaponScore", "Score", num8, "OriginalScore", num7, "User", who);
			obj.FireEvent(@event);
			int intParameter = @event.GetIntParameter("Score");
			if (num8 != intParameter)
			{
				num7 += (double)(intParameter - num8);
			}
		}
		return num7;
	}

	public static int MissileWeaponScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseMissileWeaponScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareMissileWeapons(GameObject Weapon1, GameObject Weapon2, GameObject POV)
	{
		if (Weapon1 == Weapon2)
		{
			return 0;
		}
		if (Weapon1 == null)
		{
			return 1;
		}
		if (Weapon2 == null)
		{
			return -1;
		}
		int num = Weapon1.HasTagOrProperty("AlwaysEquipAsMissileWeapon").CompareTo(Weapon2.HasTagOrProperty("AlwaysEquipAsMissileWeapon"));
		if (num != 0)
		{
			return -num;
		}
		return -PreciseMissileWeaponScore(Weapon1, POV).CompareTo(PreciseMissileWeaponScore(Weapon2, POV));
	}

	public static bool IsNewMissileWeaponBetter(GameObject NewWeapon, GameObject OldWeapon, GameObject POV)
	{
		return CompareMissileWeapons(NewWeapon, OldWeapon, POV) < 0;
	}

	public bool IsNewMissileWeaponBetter(GameObject NewWeapon, GameObject OldWeapon)
	{
		return IsNewMissileWeaponBetter(NewWeapon, OldWeapon, ParentObject);
	}

	public static double PreciseArmorScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		if (!(obj.GetPart("Armor") is Armor armor))
		{
			return 0.0;
		}
		double num = armor.Acid + armor.Elec + armor.Cold + armor.Heat + armor.Strength * 5 + armor.Intelligence + armor.Ego * 2 + armor.ToHit * 10 - armor.SpeedPenalty * 2 + armor.CarryBonus / 5;
		double num2 = armor.AV;
		double num3 = armor.DV;
		if (who != null && armor.WornOn != "*")
		{
			Body body = who.Body;
			if (body != null)
			{
				int partCount = body.GetPartCount(armor.WornOn);
				if (partCount > 1)
				{
					num2 /= (double)partCount;
					num3 /= (double)partCount;
				}
			}
		}
		if (armor.Agility != 0)
		{
			num += (double)armor.Agility;
			num3 += (double)armor.Agility * 0.5;
		}
		if (num2 > 0.0)
		{
			num += num2 * num2 * 20.0;
		}
		else if (num2 < 0.0)
		{
			num += num2 * 40.0;
		}
		if (num3 > 0.0)
		{
			num += num3 * num3 * 10.0;
		}
		else if (num3 < 0.0)
		{
			num += num3 * 20.0;
		}
		if (obj.GetPart("MoveCostMultiplier") is MoveCostMultiplier moveCostMultiplier)
		{
			num += (double)(-moveCostMultiplier.Amount * 2);
		}
		if (obj.GetPart("EquipStatBoost") is EquipStatBoost equipStatBoost)
		{
			int num4 = 0;
			foreach (KeyValuePair<string, int> bonus in equipStatBoost.GetBonusList())
			{
				int num5 = (Statistic.IsInverseBenefit(bonus.Key) ? (-bonus.Value) : bonus.Value);
				if (!bonus.Key.Contains("Resist"))
				{
					num5 *= 10;
				}
				num4 += num5;
			}
			if (num4 != 0)
			{
				num = ((equipStatBoost.ChargeUse <= 0) ? (num + (double)(num4 * 2)) : (num + (double)num4));
			}
		}
		if (obj.HasTag("Storied"))
		{
			num += 5.0;
		}
		string usesSlots = obj.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots))
		{
			num = num * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		if (obj.HasPart("Metal"))
		{
			num -= Math.Abs(num / 20.0);
		}
		return num;
	}

	public static int ArmorScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseArmorScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareArmors(GameObject Armor1, GameObject Armor2, GameObject POV)
	{
		if (Armor1 == Armor2)
		{
			return 0;
		}
		if (Armor1 == null)
		{
			return 1;
		}
		if (Armor2 == null)
		{
			return -1;
		}
		int num = Armor1.HasTagOrProperty("AlwaysEquipAsArmor").CompareTo(Armor2.HasTagOrProperty("AlwaysEquipAsArmor"));
		if (num != 0)
		{
			return -num;
		}
		return -PreciseArmorScore(Armor1, POV).CompareTo(PreciseArmorScore(Armor2, POV));
	}

	public static bool IsNewArmorBetter(GameObject NewArmor, GameObject OldArmor, GameObject POV)
	{
		return CompareArmors(NewArmor, OldArmor, POV) < 0;
	}

	public bool IsNewArmorBetter(GameObject NewArmor, GameObject OldArmor)
	{
		return IsNewArmorBetter(NewArmor, OldArmor, ParentObject);
	}

	public static double PreciseShieldScore(GameObject obj, GameObject who = null)
	{
		if (obj == null)
		{
			return 0.0;
		}
		if (!(obj.GetPart("Shield") is Shield shield))
		{
			return 0.0;
		}
		double num = -shield.SpeedPenalty;
		if (shield.AV > 0)
		{
			num += (double)(shield.AV * shield.AV);
		}
		if (shield.DV < 0)
		{
			num += (double)(shield.DV * 2);
		}
		else if (shield.DV > 0)
		{
			num += (double)(shield.DV * shield.DV);
		}
		if (obj.HasTag("Storied"))
		{
			num += 5.0;
		}
		string usesSlots = obj.UsesSlots;
		if (!string.IsNullOrEmpty(usesSlots))
		{
			num = num * 2.0 / (double)(usesSlots.CachedCommaExpansion().Count + 1);
		}
		if (shield.WornOn != "Hand")
		{
			num = num * 5.0 / 4.0;
		}
		if (obj.HasPart("Metal") && num > 0.0)
		{
			num -= 1.0;
		}
		return num;
	}

	public static int ShieldScore(GameObject obj, GameObject who = null)
	{
		return (int)Math.Round(PreciseShieldScore(obj, who), MidpointRounding.AwayFromZero);
	}

	public static int CompareShields(GameObject Shield1, GameObject Shield2, GameObject POV)
	{
		if (Shield1 == Shield2)
		{
			return 0;
		}
		if (Shield1 == null)
		{
			return 1;
		}
		if (Shield2 == null)
		{
			return -1;
		}
		return -PreciseShieldScore(Shield1, POV).CompareTo(PreciseShieldScore(Shield2, POV));
	}

	public static bool IsNewShieldBetter(GameObject NewShield, GameObject OldShield, GameObject POV)
	{
		return CompareShields(NewShield, OldShield, POV) < 0;
	}

	public bool IsNewShieldBetter(GameObject NewShield, GameObject OldShield)
	{
		return IsNewShieldBetter(NewShield, OldShield, ParentObject);
	}

	public static int CompareGear(GameObject obj1, GameObject obj2, GameObject POV)
	{
		if (obj1 == obj2)
		{
			return 0;
		}
		if (obj1 == null)
		{
			return 1;
		}
		if (obj2 == null)
		{
			return -1;
		}
		int num = obj1.HasTag("NaturalGear").CompareTo(obj2.HasTag("NaturalGear"));
		if (num != 0)
		{
			return -num;
		}
		int num2 = obj1.HasEffect("Rusted", "Broken").CompareTo(obj2.HasEffect("Rusted", "Broken"));
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = CompareArmors(obj1, obj2, POV);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = CompareMissileWeapons(obj1, obj2, POV);
		if (num4 != 0)
		{
			return num4;
		}
		int num5 = CompareShields(obj1, obj2, POV);
		if (num5 != 0)
		{
			return num5;
		}
		int num6 = CompareWeapons(obj1, obj2, POV);
		if (num6 != 0)
		{
			return num6;
		}
		int num7 = obj1.HasPart("LightSource").CompareTo(obj2.HasPart("LightSource"));
		if (num7 != 0)
		{
			return -num7;
		}
		if (obj1.HasPart("Commerce") && obj2.HasPart("Commerce"))
		{
			int num8 = obj1.GetPart<Commerce>().Value.CompareTo(obj2.GetPart<Commerce>().Value);
			if (num8 != 0)
			{
				return -num8;
			}
		}
		int num9 = obj1.GetTier().CompareTo(obj2.GetTier());
		if (num9 != 0)
		{
			return -num9;
		}
		if (!obj1.HasIntProperty("SortFudge"))
		{
			obj1.SetIntProperty("SortFudge", Stat.Random(0, 1));
		}
		if (!obj2.HasIntProperty("SortFudge"))
		{
			obj2.SetIntProperty("SortFudge", Stat.Random(0, 1));
		}
		return obj1.GetIntProperty("SortFudge").CompareTo(obj2.GetIntProperty("SortFudge"));
	}

	public static bool IsNewGearBetter(GameObject NewGear, GameObject OldGear, GameObject POV)
	{
		return CompareGear(NewGear, OldGear, POV) < 0;
	}

	public bool IsNewGearBetter(GameObject NewGear, GameObject OldGear)
	{
		return IsNewGearBetter(NewGear, OldGear, ParentObject);
	}

	public void CleanNaturalGear(Inventory pInventory = null)
	{
		if (pInventory == null)
		{
			pInventory = ParentObject.Inventory;
			if (pInventory == null)
			{
				return;
			}
		}
		List<GameObject> list = Event.NewGameObjectList();
		foreach (GameObject item in pInventory.GetObjectsDirect())
		{
			if (item.HasTag("NaturalGear"))
			{
				list.Add(item);
			}
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].Obliterate();
		}
	}

	public void PerformEquip(bool IsPlayer = false, bool Silent = false, bool DoPrimaryChoice = true)
	{
		PerformReequip(IsPlayer, Silent, DoPrimaryChoice, Initial: true);
	}

	public void PerformReequip(bool IsPlayer = false, bool Silent = false, bool DoPrimaryChoice = true, bool Initial = false)
	{
		if (ParentObject.CurrentCell != null && !ParentObject.CanMoveExtremities())
		{
			return;
		}
		bool flag = ParentObject.HasPropertyOrTag("Merchant");
		Body body = ParentObject.Body;
		Inventory inventory = ParentObject.Inventory;
		if (body != null && inventory != null)
		{
			try
			{
				if (IComponent<GameObject>.ThePlayer != null && !ParentObject.HasTag("ExcludeGrenadeHack") && IComponent<GameObject>.ThePlayer.Stat("Level") < 3 && IsHostileTowards(IComponent<GameObject>.ThePlayer))
				{
					foreach (GameObject item in inventory.GetObjectsWithTag("Grenade"))
					{
						item.Destroy();
					}
				}
			}
			catch (Exception ex)
			{
				LogErrorInEditor(ex.ToString());
			}
			List<BodyPart> parts = body.GetParts();
			string propertyOrTag = ParentObject.GetPropertyOrTag("NoEquip");
			List<string> list = (string.IsNullOrEmpty(propertyOrTag) ? null : new List<string>(propertyOrTag.CachedCommaExpansion()));
			List<GameObject> equipmentListForSlot = inventory.GetEquipmentListForSlot("Hand");
			BodyPart bodyPart = null;
			int LightSources = 0;
			foreach (BodyPart item2 in parts)
			{
				if (item2.Equipped != null && item2.Equipped.HasPart("LightSource"))
				{
					LightSources++;
				}
			}
			GameObject Shield = null;
			foreach (BodyPart item3 in parts)
			{
				if (item3.Equipped != null && item3.Equipped.HasPart("Shield"))
				{
					Shield = item3.Equipped;
					break;
				}
			}
			if (equipmentListForSlot != null)
			{
				if (equipmentListForSlot.Count > 1)
				{
					equipmentListForSlot.Sort(new WeaponSorter(ParentObject));
				}
				BodyPart bodyPart2 = null;
				foreach (BodyPart item4 in parts)
				{
					if (!item4.Primary || !(item4.Type == "Hand"))
					{
						continue;
					}
					if (item4.Equipped != null && !item4.Equipped.FireEvent("CanBeUnequipped"))
					{
						break;
					}
					bodyPart2 = item4;
					GameObject gameObject = item4.Equipped ?? item4.DefaultBehavior;
					foreach (GameObject item5 in equipmentListForSlot)
					{
						if ((list == null || !list.Contains(item5.Blueprint)) && (ParentObject.IsPlayer() || !item5.HasPropertyOrTag("NoAIEquip")) && (!flag || !item5.HasProperty("_stock")) && IsNewWeaponBetter(item5, gameObject))
						{
							MeleeWeapon part = item5.GetPart<MeleeWeapon>();
							if (part == null || part.Slot == null || !(part.Slot != item4.Type))
							{
								gameObject = item5;
								break;
							}
						}
					}
					Equip(gameObject, item4, equipmentListForSlot, ref LightSources, ref Shield, Silent, Initial);
					break;
				}
				if (equipmentListForSlot.Count > 0 && ParentObject.HasSkill("Dual_Wield"))
				{
					foreach (BodyPart item6 in parts)
					{
						if (item6 == bodyPart2 || !(item6.Type == "Hand"))
						{
							continue;
						}
						bodyPart = item6;
						if (item6.Equipped != null && !item6.Equipped.FireEvent("CanBeUnequipped"))
						{
							break;
						}
						GameObject gameObject2 = item6.Equipped ?? item6.DefaultBehavior;
						foreach (GameObject item7 in equipmentListForSlot)
						{
							if ((list == null || !list.Contains(item7.Blueprint)) && (ParentObject.IsPlayer() || !item7.HasPropertyOrTag("NoAIEquip")) && (!flag || !item7.HasProperty("_stock")) && IsNewWeaponBetter(item7, gameObject2))
							{
								gameObject2 = item7;
								break;
							}
						}
						Equip(gameObject2, item6, equipmentListForSlot, ref LightSources, ref Shield, Silent, Initial);
						break;
					}
				}
			}
			BodyPart bodyPart3 = null;
			foreach (BodyPart item8 in parts)
			{
				if (item8.Type == "Hand" && !item8.Primary)
				{
					bodyPart3 = item8;
				}
			}
			GearSorter gearSorter = null;
			foreach (BodyPart item9 in parts)
			{
				GameObject gO = null;
				GameObject gameObject3 = item9.Equipped;
				List<GameObject> list2 = null;
				if (item9.Type == "Hand")
				{
					if (item9.Primary || item9 == bodyPart)
					{
						continue;
					}
					if (gameObject3 == null)
					{
						gameObject3 = item9.DefaultBehavior;
					}
					list2 = equipmentListForSlot;
				}
				else
				{
					list2 = inventory.GetEquipmentListForSlot(item9.Type);
				}
				if (list2 == null || list2.Count == 0)
				{
					continue;
				}
				if (list2.Count > 1 && item9.Type != "Hand")
				{
					if (gearSorter == null)
					{
						gearSorter = new GearSorter(ParentObject);
					}
					list2.Sort(gearSorter);
				}
				foreach (GameObject item10 in list2)
				{
					if ((list != null && list.Contains(item10.Blueprint)) || (!ParentObject.IsPlayer() && item10.HasPropertyOrTag("NoAIEquip")) || (flag && item10.HasProperty("_stock")) || item10.HasPart("Food") || (item10.HasPart("Shield") && (!ParentObject.HasSkill("Shield") || (Shield != null && (gameObject3 != Shield || !IsNewShieldBetter(item10, Shield))))))
					{
						continue;
					}
					if (item9.Type == "Hand")
					{
						if (item10.HasPart("LightSource") && ((item9 == bodyPart3 && LightSources <= 0) || WeaponScore(item10, ParentObject) >= 4) && (item9.Equipped == null || IsNewWeaponBetter(item10, gameObject3)))
						{
							gO = item10;
							break;
						}
						if (item10.HasPart("Shield"))
						{
							gO = item10;
							break;
						}
						if (!IsPlayer && IsNewWeaponBetter(item10, gameObject3))
						{
							gO = item10;
							break;
						}
					}
					else if (item9.Type == "Thrown Weapon")
					{
						if (item10.pPhysics != null && item10.pPhysics.Category == "Grenades" && IsNewGearBetter(item10, gameObject3))
						{
							gO = item10;
							break;
						}
						if (item10.IsThrownWeapon && !item10.HasTagOrProperty("NoAIEquip") && (list == null || !list.Contains(item10.Blueprint)) && IsNewGearBetter(item10, gameObject3))
						{
							gO = item10;
							break;
						}
					}
					else if (item10.GetPart("MissileWeapon") is MissileWeapon missileWeapon)
					{
						if ((!missileWeapon.FiresManually || missileWeapon.ValidSlotType(item9.Type)) && IsNewMissileWeaponBetter(item10, gameObject3))
						{
							gO = item10;
							break;
						}
					}
					else if (item9.Type != "Missile Weapon" && IsNewGearBetter(item10, gameObject3))
					{
						gO = item10;
						break;
					}
				}
				Equip(gO, item9, equipmentListForSlot, ref LightSources, ref Shield, Silent, Initial);
			}
		}
		CleanNaturalGear(inventory);
		CommandReloadEvent.Execute(ParentObject);
		try
		{
			if (!DoPrimaryChoice || IsPlayer || ParentObject == null || body == null || ParentObject.IsPlayer())
			{
				return;
			}
			BodyPart bestPart = null;
			GameObject bestWeapon = null;
			body.ForeachPart(delegate(BodyPart p)
			{
				if (p.Equipped != null)
				{
					if (IsNewWeaponBetter(p.Equipped, bestWeapon))
					{
						bestPart = p;
						bestWeapon = p.Equipped;
					}
				}
				else if (p.DefaultBehavior != null && IsNewWeaponBetter(p.DefaultBehavior, bestWeapon))
				{
					bestPart = p;
					bestWeapon = p.DefaultBehavior;
				}
			});
			if (bestPart != null)
			{
				bestPart.SetAsPreferredDefault();
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Select primary limb", x);
		}
	}

	private bool Equip(GameObject GO, BodyPart Part, List<GameObject> Weapons, ref int LightSources, ref GameObject Shield, bool Silent, bool Initial)
	{
		if (GO == null)
		{
			return false;
		}
		GameObject equipped = Part.Equipped;
		if (equipped != null)
		{
			if (equipped == GO)
			{
				return false;
			}
			if (equipped.SameAs(GO))
			{
				return false;
			}
			Part.TryUnequip(Silent);
		}
		if (Part.Equipped != null)
		{
			return false;
		}
		if (Part.DefaultBehavior != null)
		{
			if (Part.DefaultBehavior == GO)
			{
				return false;
			}
			if (Part.DefaultBehavior.SameAs(GO))
			{
				return false;
			}
		}
		Event @event = (Initial ? eCommandEquipObjectFree : eCommandEquipObject);
		@event.SetParameter("Object", GO);
		@event.SetParameter("BodyPart", Part);
		@event.SetSilent(Silent);
		bool num = ParentObject.FireEvent(@event);
		if (num && Part.Equipped != null && Part.Equipped != equipped)
		{
			if (Part.Equipped.HasPart("LightSource"))
			{
				LightSources++;
			}
			if (Part.Equipped.HasPart("Shield"))
			{
				Shield = Part.Equipped;
			}
			if (Weapons != null)
			{
				if (GO == Part.Equipped)
				{
					Weapons.Remove(GO);
				}
				if (equipped != null)
				{
					if (equipped.pPhysics != null && equipped.pPhysics.InInventory == ParentObject)
					{
						Weapons.Add(equipped);
					}
					if (equipped.HasPart("LightSource"))
					{
						LightSources--;
					}
					if (equipped.HasPart("Shield") && equipped == Shield)
					{
						Shield = null;
					}
				}
			}
		}
		return num;
	}

	public Cell MovingTo()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num] is MoveTo moveTo)
			{
				Cell destinationCell = moveTo.GetDestinationCell();
				if (destinationCell != null)
				{
					return destinationCell;
				}
			}
		}
		for (int num2 = Goals.Items.Count - 1; num2 >= 0; num2--)
		{
			if (Goals.Items[num2] is Step step)
			{
				Cell destinationCell2 = step.GetDestinationCell();
				if (destinationCell2 != null)
				{
					return destinationCell2;
				}
			}
		}
		return null;
	}

	public bool IsFleeing()
	{
		for (int num = Goals.Items.Count - 1; num >= 0; num--)
		{
			if (Goals.Items[num].IsFleeing())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFactionMember(string Faction)
	{
		if (FactionMembership.TryGetValue(Faction, out var value) && value > 0)
		{
			return true;
		}
		return false;
	}
}
