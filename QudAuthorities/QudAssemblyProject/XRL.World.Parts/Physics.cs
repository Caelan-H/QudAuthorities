using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Language;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Physics : IPart
{
	[NonSerialized]
	private int AmbientCache = -1;

	public string Category = "Unknown";

	public int _Weight;

	public int _Temperature;

	public int FlameTemperature;

	public int VaporTemperature;

	public int FreezeTemperature;

	public int BrittleTemperature;

	public string Owner;

	public float SpecificHeat = 1f;

	public bool Solid;

	public bool Takeable;

	public bool IsReal;

	public Cell _CurrentCell;

	public GameObject _InInventory;

	public GameObject _Equipped;

	public GameObject LastDamagedBy;

	public GameObject LastWeaponDamagedBy;

	public GameObject LastProjectileDamagedBy;

	public GameObject InflamedBy;

	public string LastDamagedByType = "";

	public string LastDeathReason = "";

	public string LastThirdPersonDeathReason = "";

	public bool LastDamageAccidental;

	public bool WasFrozen;

	[NonSerialized]
	public string ConfusedName;

	[NonSerialized]
	private int lastPushSegment = int.MinValue;

	[NonSerialized]
	private int lastPushCount;

	[NonSerialized]
	private static List<string> PassingBy = new List<string>();

	private static Event eBeforePhysicsRejectObjectEntringCell = new Event("BeforePhysicsRejectObjectEntringCell", "Object", (object)null, "Actual", 1);

	public int AmbientTemperature
	{
		get
		{
			if (AmbientCache != -1)
			{
				return AmbientCache;
			}
			Cell cell = CurrentCell ?? ParentObject.GetCurrentCell();
			if (cell != null && cell.ParentZone != null)
			{
				if (cell.ParentZone.BaseTemperature > 25 && ParentObject.Stat("HeatResistance") > 0)
				{
					AmbientCache = Math.Max(25, cell.ParentZone.BaseTemperature - 4 * ParentObject.Stat("HeatResistance"));
				}
				else if (cell.ParentZone.BaseTemperature < 25 && ParentObject.Stat("ColdResistance") > 0)
				{
					AmbientCache = Math.Min(25, cell.ParentZone.BaseTemperature + ParentObject.Stat("ColdResistance"));
				}
				else
				{
					AmbientCache = cell.ParentZone.BaseTemperature;
				}
			}
			else
			{
				AmbientCache = 25;
			}
			return AmbientCache;
		}
	}

	public int Temperature
	{
		get
		{
			return _Temperature;
		}
		set
		{
			if (SpecificHeat != 0f || Temperature == 25)
			{
				_Temperature = value;
			}
		}
	}

	public int IntrinsicWeight
	{
		get
		{
			return (int)GetIntrinsicWeight();
		}
		set
		{
			_Weight = value;
		}
	}

	public int Weight
	{
		get
		{
			return (int)GetWeight();
		}
		set
		{
			_Weight = value;
		}
	}

	public int IntrinsicWeightEach => (int)GetIntrinsicWeightEach();

	public int WeightEach => (int)GetWeightEach();

	public bool UsesTwoSlots
	{
		get
		{
			return ParentObject.GetIntProperty("UsesTwoSlots") == 1;
		}
		set
		{
			if (value)
			{
				ParentObject.SetIntProperty("UsesTwoSlots", 1);
			}
			else
			{
				ParentObject.RemoveIntProperty("UsesTwoSlots");
			}
		}
	}

	public bool bUsesTwoSlots
	{
		get
		{
			return UsesTwoSlots;
		}
		set
		{
			UsesTwoSlots = value;
		}
	}

	public string UsesSlots
	{
		get
		{
			return ParentObject.GetTagOrStringProperty("UsesSlots");
		}
		set
		{
			ParentObject.SetStringProperty("UsesSlots", value);
		}
	}

	public string VaporObject => ParentObject.GetTag("VaporObject", "");

	public Cell CurrentCell
	{
		get
		{
			return _CurrentCell;
		}
		set
		{
			if (_CurrentCell != null && _CurrentCell != value && _CurrentCell.Objects.Contains(ParentObject))
			{
				try
				{
					_CurrentCell.RemoveObject(ParentObject);
				}
				catch (Exception x)
				{
					MetricsManager.LogException("CurrentCell failsafe remove", x);
				}
			}
			_CurrentCell = value;
			if (value != null)
			{
				InInventory = null;
				Equipped = null;
			}
		}
	}

	public GameObject InInventory
	{
		get
		{
			return _InInventory;
		}
		set
		{
			if (_InInventory != null && _InInventory != value)
			{
				try
				{
					Inventory inventory = _InInventory.Inventory;
					if (inventory != null && inventory.Objects.Contains(ParentObject))
					{
						inventory.RemoveObject(ParentObject);
					}
				}
				catch (Exception x)
				{
					MetricsManager.LogException("InInventory failsafe remove", x);
				}
			}
			_InInventory = value;
			if (value != null)
			{
				Equipped = null;
				CurrentCell = null;
			}
		}
	}

	public GameObject Equipped
	{
		get
		{
			return _Equipped;
		}
		set
		{
			GameObject equipped = _Equipped;
			if (equipped != null)
			{
				if (equipped == value)
				{
					MetricsManager.LogWarning($"Physics.Equipped assigned to identical object: {value}.\n{new StackTrace()}");
					return;
				}
				_Equipped = null;
				BeforeUnequippedEvent.Send(ParentObject, equipped);
				UnequippedEvent.Send(ParentObject, equipped);
			}
			_Equipped = value;
			if (value != null)
			{
				InInventory = null;
				CurrentCell = null;
			}
		}
	}

	public int Conductivity
	{
		get
		{
			return ParentObject.GetIntProperty("Conductivity");
		}
		set
		{
			if (ParentObject != null)
			{
				if (value == 0)
				{
					ParentObject.RemoveIntProperty("Conductivity");
				}
				else
				{
					ParentObject.SetIntProperty("Conductivity", value);
				}
			}
		}
	}

	public Physics()
	{
		PoolReset();
	}

	public override bool IsPoolabe()
	{
		return true;
	}

	public override bool PoolReset()
	{
		bool result = base.PoolReset();
		Owner = null;
		Category = "Unknown";
		_Weight = 0;
		Temperature = 25;
		FlameTemperature = 350;
		VaporTemperature = 10000;
		FreezeTemperature = 0;
		BrittleTemperature = -100;
		Conductivity = 5;
		Solid = false;
		Takeable = true;
		IsReal = false;
		CurrentCell = null;
		AmbientCache = -1;
		SpecificHeat = 1f;
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		LastDamagedByType = "";
		LastDeathReason = "";
		LastThirdPersonDeathReason = "";
		LastDamageAccidental = false;
		ConfusedName = null;
		WasFrozen = false;
		_Equipped = null;
		_InInventory = null;
		_CurrentCell = null;
		return result;
	}

	public double GetIntrinsicWeightEach()
	{
		double num = (double)_Weight + (double)ParentObject.GetBodyWeight();
		double num2 = num;
		if (ParentObject.WantEvent(GetIntrinsicWeightEvent.ID, MinEvent.CascadeLevel))
		{
			GetIntrinsicWeightEvent getIntrinsicWeightEvent = GetIntrinsicWeightEvent.FromPool(ParentObject, num, num2);
			ParentObject.HandleEvent(getIntrinsicWeightEvent);
			num2 = getIntrinsicWeightEvent.Weight;
		}
		if (ParentObject.WantEvent(AdjustWeightEvent.ID, MinEvent.CascadeLevel))
		{
			AdjustWeightEvent adjustWeightEvent = AdjustWeightEvent.FromPool(ParentObject, num, num2);
			ParentObject.HandleEvent(adjustWeightEvent);
			num2 = adjustWeightEvent.Weight;
		}
		return num2;
	}

	public double GetWeightEach()
	{
		double baseWeight = (double)_Weight + (double)ParentObject.GetBodyWeight();
		double num = GetIntrinsicWeightEach();
		if (ParentObject.WantEvent(GetExtrinsicWeightEvent.ID, MinEvent.CascadeLevel))
		{
			GetExtrinsicWeightEvent getExtrinsicWeightEvent = GetExtrinsicWeightEvent.FromPool(ParentObject, baseWeight, num);
			ParentObject.HandleEvent(getExtrinsicWeightEvent);
			num = getExtrinsicWeightEvent.Weight;
		}
		if (ParentObject.WantEvent(AdjustTotalWeightEvent.ID, MinEvent.CascadeLevel))
		{
			AdjustTotalWeightEvent adjustTotalWeightEvent = AdjustTotalWeightEvent.FromPool(ParentObject, baseWeight, num);
			ParentObject.HandleEvent(adjustTotalWeightEvent);
			num = adjustTotalWeightEvent.Weight;
		}
		return num;
	}

	public double GetIntrinsicWeight()
	{
		return GetIntrinsicWeightEach() * (double)ParentObject.Count;
	}

	public double GetWeight()
	{
		return GetWeightEach() * (double)ParentObject.Count;
	}

	public int GetIntrinsicWeightTimes(double Factor)
	{
		return (int)(GetIntrinsicWeight() * Factor);
	}

	public int GetWeightTimes(double Factor)
	{
		return (int)(GetWeight() * Factor);
	}

	private Cell GetBroadcastCell(GameObject Target)
	{
		if (!GameObject.validate(ref Target))
		{
			return null;
		}
		if (Owner == null)
		{
			return null;
		}
		if (ParentObject.pBrain != null)
		{
			return null;
		}
		if (Target.Owns(Owner) && !Target.IsPlayerControlled())
		{
			return null;
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.IsGraveyard())
		{
			return null;
		}
		return cell;
	}

	public void CheckBroadcastForHelp(GameObject Target, bool Accidental = false)
	{
		if (GetBroadcastCell(Target) == null)
		{
			return;
		}
		double howMuch = (Accidental ? 0.3 : 0.5);
		if (!ParentObject.isDamaged(howMuch))
		{
			string sProperty = "HelpBroadcastChecksFor" + Target.id;
			int num = (Accidental ? 5 : 3);
			if (ParentObject.ModIntProperty(sProperty, 1) < num)
			{
				return;
			}
		}
		BroadcastForHelp(Target);
	}

	public void BroadcastForHelp(GameObject Target)
	{
		Cell broadcastCell = GetBroadcastCell(Target);
		if (broadcastCell != null)
		{
			Event @event = Event.New("AIHelpBroadcast");
			@event.SetParameter("Faction", Owner);
			@event.SetParameter("Target", Target);
			@event.SetParameter("Owned", ParentObject);
			List<GameObject> list = broadcastCell.ParentZone.FastFloodVisibility(broadcastCell.X, broadcastCell.Y, 20, "Brain", ParentObject);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].FireEvent(@event);
			}
		}
	}

	public bool IsFrozen()
	{
		return Temperature <= BrittleTemperature;
	}

	public bool IsFreezing()
	{
		return Temperature <= FreezeTemperature;
	}

	public bool IsAflame()
	{
		return Temperature >= FlameTemperature;
	}

	public bool IsVaporizing()
	{
		return Temperature >= VaporTemperature;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		GameObject.validate(ref LastDamagedBy);
		GameObject.validate(ref LastWeaponDamagedBy);
		GameObject.validate(ref LastProjectileDamagedBy);
		GameObject.validate(ref InflamedBy);
		base.SaveData(Writer);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Physics obj = base.DeepCopy(Parent, MapInv) as Physics;
		obj._CurrentCell = null;
		obj._Equipped = null;
		obj._InInventory = null;
		return obj;
	}

	public override bool SameAs(IPart p)
	{
		Physics physics = p as Physics;
		if (physics._Weight != _Weight)
		{
			return false;
		}
		if (physics.Solid != Solid)
		{
			return false;
		}
		if (physics.Category != Category)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public static bool IsMoveable(GameObject obj, bool Involuntary = true)
	{
		if (!obj.IsReal)
		{
			return false;
		}
		if (obj.IsScenery)
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		if (Involuntary && !obj.CanBeInvoluntarilyMoved())
		{
			return false;
		}
		return true;
	}

	public bool Push(string Direction, int Force, int MaxDistance = 9999999, bool IgnoreGravity = false, bool Involuntary = true, GameObject Actor = null)
	{
		if (lastPushSegment != The.Game.Segments)
		{
			lastPushCount = 0;
		}
		else
		{
			if (lastPushCount >= 2)
			{
				return true;
			}
			lastPushCount++;
		}
		if (MaxDistance < 0)
		{
			return false;
		}
		if (CurrentCell == null)
		{
			return false;
		}
		if (CurrentCell.IsGraveyard())
		{
			return false;
		}
		if (Involuntary)
		{
			if (!IsMoveable(ParentObject))
			{
				return false;
			}
			int kineticResistance = ParentObject.GetKineticResistance();
			if (kineticResistance > Force)
			{
				return false;
			}
			if (kineticResistance < 0)
			{
				return false;
			}
		}
		List<string> adjacentDirections = Directions.GetAdjacentDirections(Direction, 2);
		if (50.in100())
		{
			adjacentDirections.Reverse();
		}
		adjacentDirections.Remove(Direction);
		adjacentDirections.Insert(0, Direction);
		foreach (string item in adjacentDirections)
		{
			Cell localCellFromDirection = CurrentCell.GetLocalCellFromDirection(item);
			if (localCellFromDirection == null)
			{
				return false;
			}
			if (localCellFromDirection.IsEmpty())
			{
				if (ParentObject.Move(Direction, Forced: true, System: false, IgnoreGravity))
				{
					return true;
				}
				continue;
			}
			List<GameObject> list = Event.NewGameObjectList();
			list.AddRange(localCellFromDirection.GetSolidObjects());
			foreach (GameObject item2 in localCellFromDirection.GetObjectsWithPartReadonly("Combat"))
			{
				if (!list.Contains(item2))
				{
					list.Add(item2);
				}
			}
			if (list.Count > 0)
			{
				int force = (Force * 9 / 10 - 10) / list.Count;
				int i = 0;
				for (int count = list.Count; i < count; i++)
				{
					list[i].Push(Direction, force, MaxDistance - 1);
				}
			}
			if (ParentObject.Move(Direction, Forced: true, System: false, IgnoreGravity))
			{
				return true;
			}
		}
		return false;
	}

	public int Accelerate(int Force, string Direction = null, Cell Toward = null, Cell AwayFrom = null, string Type = null, GameObject Actor = null, bool Accidental = false, GameObject IntendedTarget = null, string BonusDamage = null, double DamageFactor = 1.0, bool SuspendFalling = true, bool OneShort = false, bool Repeat = false, bool BuiltOnly = true, bool MessageForInanimate = true, bool DelayForDisplay = true)
	{
		if (!IsMoveable(ParentObject))
		{
			return 0;
		}
		return AccelerateInternal(Force, Direction, Toward, AwayFrom, Type, Actor, Accidental, IntendedTarget, BonusDamage, DamageFactor, SuspendFalling, OneShort, Repeat, BuiltOnly, MessageForInanimate, DelayForDisplay);
	}

	protected int AccelerateInternal(int Force, string Direction = null, Cell Toward = null, Cell AwayFrom = null, string Type = null, GameObject Actor = null, bool Accidental = false, GameObject IntendedTarget = null, string BonusDamage = null, double DamageFactor = 1.0, bool SuspendFalling = true, bool OneShort = false, bool Repeat = false, bool BuiltOnly = true, bool MessageForInanimate = true, bool DelayForDisplay = true, bool Subsequent = false)
	{
		int num = 0;
		int myMatterPhase = 0;
		int num2 = 5;
		List<string> list = null;
		List<string> list2 = null;
		List<GameObject> list3 = null;
		if (num > 10000 || (num >= 100 && num > Force * 2))
		{
			MetricsManager.LogError("infinite loop on " + ParentObject.DebugName);
			return num;
		}
		bool flag = Subsequent && (MessageForInanimate || ParentObject.IsCreature);
		while (true)
		{
			if (CurrentCell == null || CurrentCell.IsGraveyard())
			{
				return num;
			}
			int num3 = ParentObject.GetKineticResistance();
			if (num3 > Force)
			{
				return num;
			}
			if (num3 < 0)
			{
				return num;
			}
			if (num3 == 0)
			{
				num3 = 1;
			}
			Force -= num3;
			int springiness = ParentObject.GetSpringiness();
			int num4 = num3 + springiness;
			string text = Direction;
			if (string.IsNullOrEmpty(text))
			{
				if (list != null && list.Count == 0 && list2 != null)
				{
					list = new List<string>(list2);
				}
				if (Toward != null && !Repeat && (CurrentCell == Toward || (OneShort && ParentObject.DistanceTo(Toward) <= 1)))
				{
					return num;
				}
				if (list != null && list.Count > 0)
				{
					text = list[0];
					list.RemoveAt(0);
				}
				else if (Toward != null)
				{
					list = null;
					if (CurrentCell.ParentZone == Toward.ParentZone)
					{
						List<Tuple<Cell, char>> lineTo = ParentObject.GetLineTo(Toward);
						if (lineTo != null && lineTo.Count > 1)
						{
							list = new List<string>(lineTo.Count - 1);
							int i = 0;
							for (int num5 = lineTo.Count - 1; i < num5; i++)
							{
								string directionFromCell = lineTo[i].Item1.GetDirectionFromCell(lineTo[i + 1].Item1);
								if (!string.IsNullOrEmpty(directionFromCell) && directionFromCell != "." && directionFromCell != "?")
								{
									list.Add(directionFromCell);
									continue;
								}
								list = null;
								break;
							}
							if (list.Count <= 0)
							{
								list = null;
							}
						}
					}
					if (list != null)
					{
						num2 = 5 - list.Count / 5;
						if (Repeat)
						{
							list2 = new List<string>(list);
						}
						text = list[0];
						if (AwayFrom == null)
						{
							list.RemoveAt(0);
						}
						else
						{
							list = null;
						}
					}
					else
					{
						text = CurrentCell.GetDirectionFromCell(Toward);
					}
					if (AwayFrom != null)
					{
						text = Directions.CombineDirections(text, Directions.GetOppositeDirection(CurrentCell.GetDirectionFromCell(AwayFrom)), (int)CurrentCell.RealDistanceTo(Toward), (int)CurrentCell.RealDistanceTo(AwayFrom));
					}
				}
				else if (AwayFrom != null)
				{
					text = ((AwayFrom != CurrentCell) ? Directions.GetOppositeDirection(CurrentCell.GetDirectionFromCell(AwayFrom)) : Directions.GetRandomDirection());
				}
			}
			if (string.IsNullOrEmpty(text) || text == "." || text == "?")
			{
				return num;
			}
			Cell cellFromDirection = CurrentCell.GetCellFromDirection(text, BuiltOnly);
			if (cellFromDirection == null || cellFromDirection == CurrentCell)
			{
				return num;
			}
			if (cellFromDirection.IsEmpty())
			{
				if (!ParentObject.Move(text, Forced: true, System: false, SuspendFalling, NoStack: true, null, NearestAvailable: false, null, Type))
				{
					ParentObject.CheckStack();
					return num;
				}
				if (flag)
				{
					DidX("are", "knocked " + Directions.GetDirectionDescription(text));
					flag = false;
				}
				if (Type == "Telekinetic")
				{
					ParentObject.TelekinesisBlip();
				}
				if (Direction != null)
				{
					Direction = text;
				}
				num++;
				if (!Subsequent)
				{
					The.Core.RenderBase(UpdateSidebar: false);
					if (num2 > 0 && DelayForDisplay)
					{
						Thread.Sleep(num2);
					}
				}
				continue;
			}
			if (flag)
			{
				DidX("are", "knocked " + Directions.GetDirectionDescription(text));
				flag = false;
			}
			List<GameObject> list4 = Event.NewGameObjectList();
			int j = 0;
			for (int count = cellFromDirection.Objects.Count; j < count; j++)
			{
				GameObject gameObject = cellFromDirection.Objects[j];
				if (CollidesWith(gameObject, ref myMatterPhase))
				{
					list4.Add(gameObject);
				}
			}
			if (list4.Count > 0)
			{
				Direction = Directions.GetOppositeDirection(text);
				int num6 = num4;
				foreach (GameObject item in list4)
				{
					num6 += item.GetKineticAbsorption();
				}
				int num7 = Force;
				Force = num7 * num4 / num6;
				if (list3 == null)
				{
					list3 = Event.NewGameObjectList();
				}
				string text2 = (ParentObject.IsPlayer() ? "you" : ParentObject.an());
				foreach (GameObject item2 in list4)
				{
					int kineticResistance = item2.GetKineticResistance();
					int springiness2 = item2.GetSpringiness();
					if (MessageForInanimate || ParentObject.IsCreature || item2.IsCreature)
					{
						DidXToY("collide", "with", item2);
					}
					if (!list3.Contains(item2))
					{
						list3.Add(item2);
						int increments = (int)((double)(num7 * (num3 + kineticResistance - springiness - springiness2)) * DamageFactor / (double)num6) / 20;
						CalculateIncrementalDamageRange(increments, out var Low, out var High);
						bool flag2 = item2.ConsiderSolid() || item2.HasPart("Combat");
						int num8;
						if (flag2)
						{
							num8 = 1;
						}
						else
						{
							GetCollidedWithPenetration(item2, Force, out var Bonus, out var MaxBonus);
							num8 = Stat.RollDamagePenetrations(Stats.GetCombatAV(ParentObject), Bonus, MaxBonus);
						}
						int num9 = 0;
						for (int k = 0; k < num8; k++)
						{
							num9 += Stat.Random(Low, High);
							if (!string.IsNullOrEmpty(BonusDamage))
							{
								num9 += BonusDamage.RollCached();
							}
						}
						bool flag3 = Solid || ParentObject.HasPart("Combat");
						int num10;
						if (flag3)
						{
							num10 = 1;
						}
						else
						{
							GetCollidedWithPenetration(ParentObject, Force, out var Bonus2, out var MaxBonus2);
							num10 = Stat.RollDamagePenetrations(Stats.GetCombatAV(item2), Bonus2, MaxBonus2);
						}
						int num11 = 0;
						for (int l = 0; l < num10; l++)
						{
							num11 += Stat.Random(Low, High);
							if (!string.IsNullOrEmpty(BonusDamage))
							{
								num11 += BonusDamage.RollCached();
							}
						}
						if (num9 > 0)
						{
							string text3 = "from colliding with " + (item2.IsPlayer() ? "you" : item2.an()) + ".";
							if (!flag2)
							{
								string resultColor = Stat.GetResultColor(num8);
								text3 = "{{" + resultColor + "|(x" + num8 + ")}} " + text3;
							}
							GameObject parentObject = ParentObject;
							int amount = num9;
							bool accidental = Accidental && ParentObject != IntendedTarget;
							GameObject attacker = Actor;
							GameObject source = item2;
							parentObject.TakeDamage(amount, text3, "Crushing Collision", null, null, null, attacker, source, null, accidental, Environmental: false, Indirect: false, ShowUninvolved: false, MessageForInanimate);
						}
						if (num11 > 0)
						{
							string text4 = "from " + text2 + " colliding with " + item2.them + ".";
							if (!flag3)
							{
								string resultColor2 = Stat.GetResultColor(num10);
								text4 = "{{" + resultColor2 + "|(x" + num10 + ")}} " + text4;
							}
							int amount2 = num11;
							bool accidental = Accidental && ParentObject != IntendedTarget;
							GameObject source = Actor;
							GameObject attacker = ParentObject;
							item2.TakeDamage(amount2, text4, "Crushing Collision", null, null, null, source, attacker, null, accidental, Environmental: false, Indirect: false, ShowUninvolved: false, MessageForInanimate);
						}
					}
					if (GameObject.validate(item2) && !item2.IsInGraveyard() && item2.GetMatterPhase() <= 1)
					{
						item2.pPhysics.AccelerateInternal(num7 * kineticResistance / num6, text, null, null, Type, Actor, Accidental: true, null, BonusDamage, DamageFactor, SuspendFalling, OneShort: false, Repeat: false, BuiltOnly, MessageForInanimate, DelayForDisplay, Subsequent: true);
					}
					if (!GameObject.validate(ParentObject) || ParentObject.IsInGraveyard())
					{
						return num;
					}
				}
			}
			if (!ParentObject.Move(text, Forced: true, System: false, SuspendFalling, NoStack: true, null, NearestAvailable: false, null, Type))
			{
				break;
			}
			if (Type == "Telekinetic")
			{
				ParentObject.TelekinesisBlip();
			}
			if (Direction != null)
			{
				Direction = text;
			}
			num++;
			if (!Subsequent)
			{
				The.Core.RenderBase(UpdateSidebar: false);
				if (num2 > 0 && DelayForDisplay)
				{
					Thread.Sleep(num2);
				}
			}
		}
		if (SuspendFalling)
		{
			ParentObject.Gravitate();
		}
		ParentObject.CheckStack();
		return num;
	}

	private void CalculateIncrementalDamageRange(int Increments, out int Low, out int High)
	{
		Low = 0;
		High = 0;
		if (Increments <= 0)
		{
			return;
		}
		Low = 1;
		High = 2;
		for (int i = 1; i < Increments; i++)
		{
			if (i % 3 == 0)
			{
				Low++;
			}
			else
			{
				High += 2;
			}
		}
	}

	public static void LegacyApplyExplosion(Cell C, List<Cell> UsedCells, List<GameObject> Hit, int Force, bool Local = true, bool Show = true, GameObject Owner = null, string BonusDamage = null, int Phase = 1, float DamageModifier = 1f)
	{
		if (C == null)
		{
			return;
		}
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (Show)
		{
			TextConsole.LoadScrapBuffers();
			The.Core.RenderMapToBuffer(scrapBuffer);
		}
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		CleanQueue<int> cleanQueue2 = new CleanQueue<int>();
		CleanQueue<string> cleanQueue3 = new CleanQueue<string>();
		cleanQueue.Enqueue(C);
		cleanQueue2.Enqueue(Force);
		cleanQueue3.Enqueue(".");
		UsedCells.Add(C);
		while (cleanQueue.Count > 0)
		{
			Event.PinCurrentPool();
			Cell cell = cleanQueue.Dequeue();
			int num = cleanQueue2.Dequeue();
			string text = cleanQueue3.Dequeue();
			for (int i = 0; i < UsedCells.Count; i++)
			{
				Cell cell2 = UsedCells[i];
				if (cell2 == null)
				{
					return;
				}
				if (cell2.ParentZone == The.ZoneManager.ActiveZone)
				{
					scrapBuffer.Goto(cell2.X, cell2.Y);
					scrapBuffer.Write("&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
				}
			}
			if (Show && C.ParentZone != null && C.ParentZone.IsActive())
			{
				textConsole.DrawBuffer(scrapBuffer);
				if (Force < 100000)
				{
					Thread.Sleep(5);
				}
			}
			List<Cell> list = ((!Local) ? cell.GetAdjacentCells() : cell.GetLocalAdjacentCells(1));
			for (int j = 0; j < UsedCells.Count; j++)
			{
				Cell item = UsedCells[j];
				if (list.CleanContains(item))
				{
					list.Remove(item);
				}
			}
			int num2 = 0;
			Damage damage = null;
			Event @event = null;
			foreach (GameObject item2 in cell.GetObjectsWithPartReadonly("Physics"))
			{
				if (Hit.Contains(item2))
				{
					continue;
				}
				Hit.Add(item2);
				if (!item2.PhaseMatches(Phase))
				{
					continue;
				}
				num2 += item2.GetKineticResistance();
				if (damage == null || !string.IsNullOrEmpty(BonusDamage))
				{
					damage = new Damage((int)(DamageModifier * (float)num / 250f));
					if (!string.IsNullOrEmpty(BonusDamage))
					{
						damage.Amount += BonusDamage.RollCached();
					}
					damage.AddAttribute("Explosion");
					if (cell != C)
					{
						damage.AddAttribute("Accidental");
					}
				}
				if (@event == null || !string.IsNullOrEmpty(BonusDamage))
				{
					@event = Event.New("TakeDamage");
					@event.SetParameter("Damage", damage);
					@event.SetParameter("Owner", Owner);
					@event.SetParameter("Attacker", Owner);
					@event.SetParameter("Message", "from %t explosion!");
				}
				item2.FireEvent(@event);
			}
			Random random = new Random();
			for (int k = 0; k < list.Count; k++)
			{
				int index = random.Next(0, list.Count);
				Cell value = list[k];
				list[k] = list[index];
				list[index] = value;
			}
			Damage damage2 = null;
			Event event2 = null;
			while (true)
			{
				IL_02fd:
				for (int l = 0; l < list.Count; l++)
				{
					Cell cell3 = list[l];
					if (Local && (cell3.X == 0 || cell3.X == 79 || cell3.Y == 0 || cell3.Y == 24))
					{
						continue;
					}
					foreach (GameObject item3 in cell3.GetObjectsWithPartReadonly("Physics"))
					{
						if (!Hit.Contains(item3))
						{
							Hit.Add(item3);
							if (item3.PhaseMatches(Phase))
							{
								if (damage2 == null || !string.IsNullOrEmpty(BonusDamage))
								{
									damage2 = new Damage(num / 250);
									if (!string.IsNullOrEmpty(BonusDamage))
									{
										damage2.Amount += BonusDamage.RollCached();
									}
									damage2.AddAttribute("Explosion");
									damage2.AddAttribute("Accidental");
								}
								if (event2 == null || !string.IsNullOrEmpty(BonusDamage))
								{
									event2 = Event.New("TakeDamage");
									event2.SetParameter("Damage", damage2);
									event2.SetParameter("Owner", Owner);
									event2.SetParameter("Attacker", Owner);
									event2.SetParameter("Message", "from %t explosion!");
								}
								item3.FireEvent(event2);
							}
						}
						if (item3.PhaseMatches(Phase))
						{
							int kineticResistance = item3.GetKineticResistance();
							if (kineticResistance > num)
							{
								list.Remove(cell3);
								goto IL_02fd;
							}
							if (kineticResistance > 0)
							{
								item3.Move((text == ".") ? Directions.GetRandomDirection() : text, Forced: true);
							}
						}
					}
					if (cell3.IsSolid())
					{
						list.Remove(cell3);
						goto IL_02fd;
					}
				}
				break;
			}
			if (list.Count > 0)
			{
				int num3 = (num - num2) / list.Count;
				if (num3 > 100)
				{
					foreach (Cell item4 in list)
					{
						if (item4 != null && !UsedCells.Contains(item4))
						{
							UsedCells.Add(item4);
							cleanQueue.Enqueue(item4);
							cleanQueue2.Enqueue(num3);
							cleanQueue3.Enqueue(cell.GetDirectionFromCell(item4));
						}
					}
				}
			}
			Event.ResetToPin();
		}
	}

	private void GetCollidingPenetration(GameObject obj, int Force, out int Bonus, out int MaxBonus)
	{
		Bonus = 0;
		MaxBonus = 0;
		ThrownWeapon thrownWeapon = obj.GetPart("ThrownWeapon") as ThrownWeapon;
		MeleeWeapon meleeWeapon = obj.GetPart("MeleeWeapon") as MeleeWeapon;
		if (thrownWeapon != null)
		{
			Bonus = thrownWeapon.Penetration;
			MaxBonus = Bonus * 2;
		}
		else if (meleeWeapon != null)
		{
			Bonus = (4 + meleeWeapon.PenBonus) / 2;
			MaxBonus = Bonus + meleeWeapon.MaxStrengthBonus;
		}
		else
		{
			Bonus = 2;
			MaxBonus = 4;
		}
		Bonus += Stat.GetScoreModifier(Force / 15);
		MaxBonus += obj.Weight / 50;
		if (Bonus > MaxBonus)
		{
			Bonus = MaxBonus;
		}
		if (Bonus < 0)
		{
			Bonus = 0;
		}
	}

	private void GetCollidedWithPenetration(GameObject obj, int Force, out int Bonus, out int MaxBonus)
	{
		Bonus = 0;
		MaxBonus = 0;
		if (obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
		{
			Bonus = (4 + meleeWeapon.PenBonus) / 2;
			MaxBonus = Bonus + meleeWeapon.MaxStrengthBonus;
		}
		else
		{
			Bonus = 2;
			MaxBonus = 4;
		}
		Bonus += Stat.GetScoreModifier(Force / 15);
		MaxBonus += obj.Weight / 50;
		if (Bonus > MaxBonus)
		{
			Bonus = MaxBonus;
		}
		if (Bonus < 0)
		{
			Bonus = 0;
		}
	}

	private bool CollidesWith(GameObject obj, ref int myMatterPhase)
	{
		if (!IsReal)
		{
			return false;
		}
		if (obj == ParentObject)
		{
			return false;
		}
		if (!GameObject.validate(ref obj) || !GameObject.validate(ParentObject))
		{
			return false;
		}
		if (obj.IsScenery)
		{
			return false;
		}
		if (obj.IsInGraveyard() || ParentObject.IsInGraveyard())
		{
			return false;
		}
		if (obj.GetMatterPhase() >= 3)
		{
			return false;
		}
		if (myMatterPhase == 0)
		{
			myMatterPhase = ParentObject.GetMatterPhase();
		}
		if (myMatterPhase >= 3)
		{
			return false;
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return false;
		}
		bool flag = false;
		if (obj.ConsiderSolidFor(ParentObject) || ParentObject.ConsiderSolidFor(obj))
		{
			flag = true;
		}
		else if (obj.HasPart("Combat"))
		{
			if (ParentObject.HasPart("Combat"))
			{
				flag = true;
			}
			else if (Stat.Random(1, 20) > Stats.GetCombatDV(obj))
			{
				flag = true;
			}
		}
		else if (obj.IsReal && Math.Min(20 + ParentObject.Weight + obj.Weight, 80).in100() && Stat.Random(1, 20) > Stats.GetCombatDV(obj))
		{
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		if (!obj.PhaseMatches(ParentObject))
		{
			return false;
		}
		if (!obj.FlightMatches(ParentObject))
		{
			return 50.in100();
		}
		return true;
	}

	public bool CollidesWith(GameObject obj)
	{
		int myMatterPhase = 0;
		return CollidesWith(obj, ref myMatterPhase);
	}

	public static void ApplyExplosion(Cell C, int Force, List<Cell> UsedCells = null, List<GameObject> Hit = null, bool Local = true, bool Show = true, GameObject Owner = null, string BonusDamage = null, int Phase = 1, bool Neutron = false, bool Indirect = false, float DamageModifier = 1f, GameObject WhatExploded = null)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		if (Show)
		{
			TextConsole.LoadScrapBuffers();
			The.Core.RenderMapToBuffer(scrapBuffer);
		}
		if (UsedCells == null)
		{
			UsedCells = Event.NewCellList();
		}
		if (Hit == null)
		{
			Hit = Event.NewGameObjectList();
		}
		CleanQueue<Cell> cleanQueue = new CleanQueue<Cell>();
		CleanQueue<int> cleanQueue2 = new CleanQueue<int>();
		CleanQueue<string> cleanQueue3 = new CleanQueue<string>();
		cleanQueue.Enqueue(C);
		cleanQueue2.Enqueue(Force);
		cleanQueue3.Enqueue(".");
		UsedCells.Add(C);
		int num = 20 - Force / 1000;
		while (cleanQueue.Count > 0)
		{
			Cell cell = cleanQueue.Dequeue();
			if (cell == null)
			{
				continue;
			}
			int num2 = cleanQueue2.Dequeue();
			string text = cleanQueue3.Dequeue();
			List<Cell> list = ((!Local) ? cell.GetAdjacentCells() : cell.GetLocalAdjacentCells(1));
			for (int i = 0; i < UsedCells.Count; i++)
			{
				list.Remove(UsedCells[i]);
			}
			int num3 = 0;
			foreach (GameObject item in cell.GetObjectsWithPartReadonly("Physics"))
			{
				if (!Hit.Contains(item))
				{
					Hit.Add(item);
					int num4 = ExplosionDamage(item, Owner, C, cell, num2, Phase, BonusDamage, DamageModifier, Neutron, Indirect, WhatExploded);
					num3 += num4;
				}
			}
			Random random = new Random();
			for (int j = 0; j < list.Count; j++)
			{
				int index = random.Next(0, list.Count);
				Cell value = list[j];
				list[j] = list[index];
				list[index] = value;
			}
			while (true)
			{
				IL_019f:
				for (int num5 = UsedCells.Count - 1; num5 >= 0; num5--)
				{
					Cell cell2 = UsedCells[num5];
					if (cell2 == null)
					{
						return;
					}
					if (cell2.ParentZone.IsActive())
					{
						scrapBuffer.WriteAt(cell2, "&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
					}
				}
				for (int num6 = list.Count - 1; num6 >= 0; num6--)
				{
					Cell cell3 = list[num6];
					if (cell3 == null)
					{
						return;
					}
					if (cell3.ParentZone.IsActive())
					{
						scrapBuffer.WriteAt(cell3, "&" + XRL.World.Capabilities.Phase.getRandomExplosionColor(Phase) + "*");
					}
				}
				if (Show && C.ParentZone != null && C.ParentZone.IsActive())
				{
					textConsole.DrawBuffer(scrapBuffer);
					if (num > 0)
					{
						Thread.Sleep(num);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					Cell cell4 = list[k];
					if (Local && (cell4.X == 0 || cell4.X == 79 || cell4.Y == 0 || cell4.Y == 24))
					{
						continue;
					}
					foreach (GameObject item2 in cell4.GetObjectsWithPartReadonly("Physics"))
					{
						if (Hit.Contains(item2))
						{
							continue;
						}
						Hit.Add(item2);
						int num7 = ExplosionDamage(item2, Owner, C, cell, num2, Phase, BonusDamage, DamageModifier, Neutron, Indirect, WhatExploded);
						if (num7 > 0)
						{
							num3 += num7;
							if (num7 > num2)
							{
								list.Remove(cell4);
								goto IL_019f;
							}
							if (IsMoveable(item2))
							{
								item2.Move((text == ".") ? Directions.GetRandomDirection() : text, Forced: true, System: false, IgnoreGravity: false, NoStack: false, null, NearestAvailable: false, null, "Explosion");
							}
						}
					}
					if (cell4.IsSolid())
					{
						list.Remove(cell4);
						goto IL_019f;
					}
				}
				break;
			}
			if (list.Count <= 0)
			{
				continue;
			}
			int num8 = (num2 - num3) / list.Count;
			if (num8 <= 100)
			{
				continue;
			}
			foreach (Cell item3 in list)
			{
				if (item3 != null && !UsedCells.Contains(item3))
				{
					UsedCells.Add(item3);
					cleanQueue.Enqueue(item3);
					cleanQueue2.Enqueue(num8);
					cleanQueue3.Enqueue(cell.GetDirectionFromCell(item3));
				}
			}
		}
	}

	private static int ExplosionDamage(GameObject GO, GameObject Owner, Cell C, Cell CurrentC, int CurrentForce, int Phase, string BonusDamage, float DamageModifier, bool Neutron, bool Indirect, GameObject WhatExploded, bool TrackResistance = true)
	{
		int result = 0;
		if (GO.PhaseMatches(Phase))
		{
			result = GO.GetKineticResistance();
			int num = (int)(DamageModifier * (float)CurrentForce / 250f);
			bool flag = CurrentC != C;
			if (!string.IsNullOrEmpty(BonusDamage))
			{
				num += BonusDamage.RollCached();
			}
			string message;
			string deathReason;
			string thirdPersonDeathReason;
			if (Neutron)
			{
				message = "from being crushed under the weight of a thousand suns.";
				deathReason = "You were crushed under the weight of a thousand suns.";
				thirdPersonDeathReason = GO.It + GO.GetVerb("were") + " @@crushed under the weight of a thousand suns.";
			}
			else
			{
				message = "from %t explosion!";
				StringBuilder stringBuilder = Event.NewStringBuilder();
				StringBuilder stringBuilder2 = Event.NewStringBuilder();
				stringBuilder.Append("You");
				stringBuilder2.Append(GO.It);
				if (WhatExploded == GO)
				{
					stringBuilder.Append(" exploded");
					stringBuilder2.Append(" @@exploded");
				}
				else
				{
					stringBuilder.Append(" died in ");
					stringBuilder2.Append(" @@died in ");
					if (WhatExploded != null)
					{
						stringBuilder.Append("the explosion of ").Append(WhatExploded.an());
						stringBuilder2.Append("the explosion of ").Append(WhatExploded.an());
					}
					else
					{
						stringBuilder.Append("an explosion");
						stringBuilder2.Append("an explosion");
					}
				}
				if (Owner != null && Owner != WhatExploded)
				{
					if (Owner == GO)
					{
						if (WhatExploded != null)
						{
							stringBuilder.Append(", which you caused");
							stringBuilder2.Append(", which ").Append(GO.it).Append(" caused");
						}
						else
						{
							stringBuilder.Append(" you caused");
							stringBuilder2.Append(' ').Append(GO.it).Append(" caused");
						}
					}
					else
					{
						stringBuilder.Append(" caused by ").Append(Owner.an());
						stringBuilder2.Append(" caused by ").Append(Owner.an());
					}
				}
				stringBuilder.Append('.');
				stringBuilder2.Append('.');
				deathReason = stringBuilder.ToString();
				thirdPersonDeathReason = stringBuilder2.ToString();
			}
			int amount = num;
			string attributes = (Neutron ? "Neutron Explosion" : "Explosion");
			bool accidental = flag;
			bool indirect = Indirect;
			GO.TakeDamage(amount, message, attributes, deathReason, thirdPersonDeathReason, Owner, null, WhatExploded, null, accidental, Environmental: false, indirect, ShowUninvolved: false, ShowForInanimate: false, SilentIfNoDamage: false, Phase);
		}
		return result;
	}

	public void ApplyDischarge(Cell C, Cell TargetCell, int Voltage, string Damage, List<Cell> UsedCells = null, GameObject Owner = null, int Phase = 0, bool Accidental = false)
	{
		ApplyDischarge(C, TargetCell, Voltage, Damage.RollCached(), UsedCells, Owner, Phase, Accidental);
	}

	public void ApplyDischarge(Cell C, Cell TargetCell, int Voltage, int Damage, List<Cell> UsedCells = null, GameObject Owner = null, int Phase = 0, bool Accidental = false)
	{
		if (C == null || C.ParentZone == null || C.IsGraveyard() || TargetCell == null || TargetCell.ParentZone == null || TargetCell.IsGraveyard())
		{
			return;
		}
		if (Phase == 0 && Owner != null)
		{
			Phase = Owner.GetPhase();
		}
		if (UsedCells == null)
		{
			UsedCells = Event.NewCellList();
		}
		if (Options.DrawArcs)
		{
			TargetCell.GetFirstObject()?.ParticleBlip("&WX", 300);
		}
		List<Point> list = Zone.Line(C.X, C.Y, TargetCell.X, TargetCell.Y);
		bool flag = false;
		int num = 1;
		if (C == TargetCell)
		{
			num = 0;
		}
		for (int i = num; i < list.Count; i++)
		{
			Cell cell = TargetCell.ParentZone.GetCell(list[i].X, list[i].Y);
			if (cell.IsVisible())
			{
				cell.GetFirstObject()?.ParticleBlip("&" + XRL.World.Capabilities.Phase.getRandomElectricArcColor(Phase) + (char)Stat.RandomCosmetic(191, 198), 30);
			}
			if (C.X == cell.X && C.Y == cell.Y && C != TargetCell)
			{
				continue;
			}
			foreach (GameObject item in cell.GetObjectsWithPartReadonly("Combat"))
			{
				if (item.pPhysics.Conductivity <= 0 || !item.PhaseMatches(Phase))
				{
					continue;
				}
				if (item.TakeDamage(Damage, "from %t electrical discharge!", "Electric Shock", null, null, Accidental: Accidental, Owner: Owner, Attacker: Owner ?? ParentObject))
				{
					for (int j = 0; j < 3; j++)
					{
						item.ParticleText("&" + XRL.World.Capabilities.Phase.getRandomElectricArcColor(Phase) + "ú");
					}
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				foreach (GameObject item2 in cell.GetObjectsWithPartReadonly("Physics"))
				{
					if (item2.pPhysics.Conductivity <= 0 || !item2.PhaseMatches(Phase))
					{
						continue;
					}
					if (item2.TakeDamage(Damage, "from %t electrical discharge!", "Electric Shock", null, null, Accidental: Accidental, Owner: Owner, Attacker: Owner ?? ParentObject))
					{
						for (int k = 0; k < 3; k++)
						{
							item2.ParticleText("&" + XRL.World.Capabilities.Phase.getRandomElectricArcColor(Phase) + "ú");
						}
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			if (UsedCells == null)
			{
				UsedCells = new List<Cell>();
			}
			if (!UsedCells.Contains(C))
			{
				UsedCells.Add(C);
			}
			if (!UsedCells.Contains(cell))
			{
				UsedCells.Add(cell);
			}
			if (Voltage <= 1)
			{
				break;
			}
			List<Cell> adjacentCells = cell.GetAdjacentCells();
			foreach (Cell UsedCell in UsedCells)
			{
				adjacentCells.Remove(UsedCell);
			}
			int num2 = Damage * 4 / 5;
			if (num2 <= 0 || adjacentCells.Count <= 0)
			{
				break;
			}
			Cell cell2 = null;
			int num3 = -1;
			foreach (Cell item3 in adjacentCells)
			{
				foreach (GameObject item4 in item3.GetObjectsWithPartReadonly("Physics"))
				{
					if (item4.PhaseMatches(Phase) && item4.pPhysics.Conductivity > num3)
					{
						num3 = item4.pPhysics.Conductivity;
						cell2 = item3;
					}
				}
			}
			if (cell2 != null)
			{
				ApplyDischarge(TargetCell, cell2, Voltage - 1, num2, UsedCells, Owner, Phase, Accidental: true);
			}
			break;
		}
	}

	public override bool RenderTile(ConsoleChar E)
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (IsFrozen())
		{
			E.ColorString = "&c^C";
			return false;
		}
		if (IsFreezing() && FreezeTemperature != -9999)
		{
			if (ParentObject.HasPart("Brain"))
			{
				_ = ParentObject.pRender;
				int num = XRLCore.CurrentFrame % 60;
				if (num > 5 && num < 15)
				{
					E.RenderString = "ø";
					E.ColorString = "&C";
					return false;
				}
			}
		}
		else
		{
			if (IsAflame() && ParentObject.HasEffect("CoatedInPlasma"))
			{
				switch (Stat.RandomCosmetic(1, 4))
				{
				case 1:
					E.ColorString = "&G^g";
					break;
				case 2:
					E.ColorString = "&g^Y";
					break;
				case 3:
					E.ColorString = "&G^k";
					break;
				case 4:
					E.ColorString = "&Y^k";
					break;
				}
				if (Stat.RandomCosmetic(1, 20) == 1)
				{
					ParentObject.Smoke();
				}
				return false;
			}
			if (IsAflame())
			{
				switch (Stat.RandomCosmetic(1, 4))
				{
				case 1:
					E.ColorString = "&R^W";
					break;
				case 2:
					E.ColorString = "&r^W";
					break;
				case 3:
					E.ColorString = "&R^k";
					break;
				case 4:
					E.ColorString = "&Y^k";
					break;
				}
				if (Stat.RandomCosmetic(1, 20) == 1)
				{
					ParentObject.Smoke();
				}
				return false;
			}
		}
		return true;
	}

	private void DoSearching(Cell C, ref Event eSearched)
	{
		if (C.HasObjectWithRegisteredEvent("Searched"))
		{
			if (eSearched == null)
			{
				eSearched = Event.New("Searched", "Searcher", ParentObject);
			}
			C.FireEvent(eSearched);
		}
	}

	public void Search()
	{
		Event eSearched = null;
		DoSearching(CurrentCell, ref eSearched);
		List<Cell> localAdjacentCells = CurrentCell.GetLocalAdjacentCells();
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			DoSearching(localAdjacentCells[i], ref eSearched);
		}
	}

	public bool EnterCell(Cell C)
	{
		CurrentCell = C;
		InInventory = null;
		Equipped = null;
		EnvironmentalUpdateEvent.Send(ParentObject);
		if (IsPlayer())
		{
			Search();
			if (!IComponent<GameObject>.TerseMessages)
			{
				PassingBy.Clear();
				int i = 0;
				for (int count = CurrentCell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = CurrentCell.Objects[i];
					if (gameObject == ParentObject || SkipInPassingBy(gameObject, CurrentCell))
					{
						continue;
					}
					if (gameObject.HasProperty("SeenMessage"))
					{
						string stringProperty = gameObject.GetStringProperty("SeenMessage");
						if (!string.IsNullOrEmpty(stringProperty))
						{
							PassingBy.Add(stringProperty);
						}
					}
					else
					{
						PassingBy.Add(gameObject.an());
					}
				}
				if (PassingBy.Count > 0)
				{
					IComponent<GameObject>.AddPlayerMessage("You pass by " + Grammar.MakeAndList(PassingBy) + ".");
				}
			}
		}
		return true;
	}

	private bool SkipInPassingBy(GameObject obj, Cell C)
	{
		if (obj.pRender == null)
		{
			return true;
		}
		if (!obj.pRender.Visible)
		{
			return true;
		}
		if (obj.HasTagOrProperty("NoPassByMessage"))
		{
			return true;
		}
		if (obj.IsWadingDepthLiquid() && !C.HasBridge() && obj.PhaseAndFlightMatches(ParentObject))
		{
			return true;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanBeTradedEvent.ID && (ID != BeforeRenderEvent.ID || !IsAflame()) && ID != DroppedEvent.ID && ID != EndTurnEvent.ID && ID != EquippedEvent.ID && ID != EnvironmentalUpdateEvent.ID && ID != GeneralAmnestyEvent.ID && ID != GetContextEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetInventoryCategoryEvent.ID && ID != GetNavigationWeightEvent.ID && ID != HasFlammableEquipmentEvent.ID && ID != HasFlammableEquipmentOrInventoryEvent.ID && ID != InventoryActionEvent.ID && ID != LeftCellEvent.ID && (ID != ObjectEnteringCellEvent.ID || !Solid) && (ID != OkayToDamageEvent.ID || string.IsNullOrEmpty(Owner)) && ID != QueryEquippableListEvent.ID && ID != RemoveFromContextEvent.ID && ID != ReplaceInContextEvent.ID && ID != StatChangeEvent.ID && ID != SuspendingEvent.ID && ID != TakenEvent.ID && ID != TryRemoveFromContextEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(OkayToDamageEvent E)
	{
		if (!string.IsNullOrEmpty(Owner))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		if (IsAflame())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnvironmentalUpdateEvent E)
	{
		AmbientCache = -1;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasFlammableEquipmentEvent E)
	{
		if (E.Object != ParentObject && FlameTemperature <= E.Temperature)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(HasFlammableEquipmentOrInventoryEvent E)
	{
		if (E.Object != ParentObject && FlameTemperature <= E.Temperature)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (_CurrentCell != null)
		{
			E.CellContext = _CurrentCell;
			E.Relation = 1;
			E.RelationManager = this;
			return false;
		}
		if (GameObject.validate(ref _InInventory))
		{
			E.ObjectContext = _InInventory;
			E.Relation = 2;
			E.RelationManager = this;
			return false;
		}
		if (GameObject.validate(ref _Equipped))
		{
			E.ObjectContext = _Equipped;
			E.Relation = 3;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		if (GameObject.validate(ref _InInventory))
		{
			try
			{
				Inventory inventory = _InInventory.Inventory;
				if (inventory != null)
				{
					inventory.RemoveObject(ParentObject);
					inventory.ParentObject.ReceiveObject(E.Replacement);
				}
			}
			catch
			{
			}
		}
		if (GameObject.validate(ref _Equipped))
		{
			try
			{
				ParentObject.SplitFromStack();
				GameObject equipped = _Equipped;
				BodyPart bodyPart = equipped.FindEquippedObject(ParentObject);
				if (bodyPart != null)
				{
					bodyPart.ForceUnequip(Silent: true);
					equipped.FireEvent(Event.New("CommandEquipObject", "Object", E.Replacement, "BodyPart", bodyPart));
				}
			}
			catch
			{
			}
		}
		if (_CurrentCell != null)
		{
			try
			{
				Cell cell = _CurrentCell;
				_CurrentCell.RemoveObject(ParentObject);
				cell.AddObject(E.Replacement);
			}
			catch
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		if (GameObject.validate(ref _InInventory))
		{
			try
			{
				_InInventory.Inventory?.RemoveObject(ParentObject, E);
				_InInventory = null;
			}
			catch
			{
			}
		}
		if (GameObject.validate(ref _Equipped))
		{
			try
			{
				BodyPart bodyPart = _Equipped.FindEquippedObject(ParentObject);
				if (bodyPart != null)
				{
					bodyPart.ForceUnequip(Silent: true);
					if (bodyPart.Equipped == ParentObject)
					{
						bodyPart.Unequip();
					}
				}
				Equipped = null;
			}
			catch
			{
				_Equipped = null;
			}
		}
		if (_CurrentCell != null)
		{
			try
			{
				_CurrentCell.RemoveObject(ParentObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, E);
				_CurrentCell = null;
			}
			catch
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		if (GameObject.validate(ref _InInventory))
		{
			Inventory inventory = _InInventory.Inventory;
			if (inventory == null || !inventory.FireEvent(Event.New("CommandRemoveObject", "Object", ParentObject).SetSilent(Silent: true), E))
			{
				return false;
			}
			if (_InInventory != null)
			{
				return false;
			}
		}
		if (GameObject.validate(ref _Equipped))
		{
			_Equipped.FindEquippedObject(ParentObject)?.TryUnequip(Silent: true);
			if (_Equipped != null)
			{
				return false;
			}
		}
		if (_CurrentCell != null)
		{
			if (!_CurrentCell.RemoveObject(ParentObject, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, null, null, E))
			{
				return false;
			}
			if (_CurrentCell != null)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		bool flag = true;
		if (Solid && !E.IgnoresWalls && !E.WallWalker && (!E.Flying || !ParentObject.HasTag("Flyover")) && !ParentObject.HasPart("Combat"))
		{
			flag = false;
			if (E.Smart)
			{
				if (ParentObject.GetPart("Forcefield") is Forcefield forcefield)
				{
					E.Uncacheable = true;
					if (forcefield.CanPass(E.Actor))
					{
						flag = true;
					}
				}
				if (!flag && ParentObject.GetPart("Door") is Door door)
				{
					E.Uncacheable = true;
					if (door.CanPathThrough(E.Actor))
					{
						flag = true;
					}
				}
			}
			if (!flag && E.Burrower && ParentObject.IsDiggable())
			{
				if (OkayToDamageEvent.Check(ParentObject, E.Actor, out var WasWanted))
				{
					flag = true;
					E.MinWeight(20);
				}
				if (WasWanted)
				{
					E.Uncacheable = true;
				}
			}
		}
		if (!flag)
		{
			E.MinWeight(100);
			return false;
		}
		if (Temperature > 200)
		{
			E.Uncacheable = true;
			int num = Temperature / 20;
			if (E.Smart && E.Actor != null)
			{
				int num2 = E.Actor.Stat("HeatResistance");
				if (num2 != 0)
				{
					num = num * (100 - num2) / 100;
				}
			}
			E.MinWeight(num, 98);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		CurrentCell = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (Solid)
		{
			if (E.Object.HasTagOrProperty("IgnoresWalls"))
			{
				return true;
			}
			Brain pBrain = E.Object.pBrain;
			if (pBrain != null && pBrain.WallWalker && !E.Object.IsFlying)
			{
				return true;
			}
			if (E.Object.IsPlayer() && The.Core.IDKFA)
			{
				return true;
			}
			if (ParentObject.HasPropertyOrTag("Flyover") && E.Object.IsFlying)
			{
				return true;
			}
			if (E.Object.PhaseMatches(ParentObject))
			{
				eBeforePhysicsRejectObjectEntringCell.SetParameter("Object", E.Object);
				if (ParentObject.FireEvent(eBeforePhysicsRejectObjectEntringCell) && Solid && E.Object.FireEvent(Event.New("ObjectEnteringCellBlockedBySolid", "Object", ParentObject)) && Solid)
				{
					if (E.Object.IsPlayer())
					{
						if (E.Forced)
						{
							string text = "OUCH! You collide with " + ParentObject.an() + ".";
							if (The.Game.Player.Messages.LastLine != text)
							{
								IComponent<GameObject>.AddPlayerMessage(text);
							}
						}
						else if (E.Object.OnWorldMap())
						{
							IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("are") + " too difficult to traverse via the world map. You'll have to find your way on the surface.");
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("The way is blocked by " + ParentObject.an() + ".");
						}
					}
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (IsAflame())
		{
			AddLight(3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (!E.List.Contains(ParentObject) && E.SlotType == "Thrown Weapon")
		{
			E.List.Add(ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.HasPart("Combat"))
		{
			if (Equipped != null)
			{
				if (!HasPropertyOrTag("NoRemoveOptionInInventory"))
				{
					E.AddAction("Remove", "remove", "Unequip", null, 'r', FireOnActor: true, 10);
				}
			}
			else if (Takeable)
			{
				bool flag = true;
				if (InInventory == null || !InInventory.IsPlayer())
				{
					if (ParentObject.IsTakeable())
					{
						E.AddAction("Get", "get", "CommandTakeObject", null, 'g', FireOnActor: true, 30, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
					}
					else
					{
						flag = false;
					}
				}
				else if (!ParentObject.HasTagOrProperty("CannotDrop") && !IComponent<GameObject>.ThePlayer.OnWorldMap())
				{
					E.AddAction("Drop", "drop", "CommandDropObject", null, 'd', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
				}
				if (flag && !ParentObject.HasTagOrProperty("CannotEquip"))
				{
					E.AddAction("AutoEquip", "equip (auto)", "CommandAutoEquipObject", null, 'e', FireOnActor: true, 10, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
					E.AddAction("DoEquip", "equip (manual)", "CommandEquipObject", null, 'e', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: false, WorksTelepathically: false, AsMinEvent: false);
				}
			}
		}
		if (IsAflame())
		{
			E.AddAction("Firefight", "fight fire", "FightFire", null, 'f', FireOnActor: false, 40);
		}
		if (CurrentCell != null && IsReal && ParentObject.HasStat("Hitpoints"))
		{
			E.AddAction("Attack", "attack", "Attack", null, 'k', FireOnActor: false, -10);
		}
		if ((Equipped == IComponent<GameObject>.ThePlayer || InInventory == IComponent<GameObject>.ThePlayer) && IComponent<GameObject>.ThePlayer.GetConfusion() <= 0)
		{
			if (TechModding.CanMod(ParentObject))
			{
				E.AddAction("Mod", "mod with tinkering", "Mod", "tinkering", 't');
			}
			E.AddAction("Add Notes", "add notes", "AddNotes", null, 'n');
			if (ParentObject.IsImportant())
			{
				E.AddAction("Mark Unimportant", "mark unimportant", "MarkUnimportant", null, 'i', FireOnActor: false, -1);
			}
			else
			{
				E.AddAction("Mark Important", "mark important", "MarkImportant", null, 'i', FireOnActor: false, -1);
			}
		}
		if (ParentObject.HasPart("Notes") && IComponent<GameObject>.ThePlayer.GetConfusion() <= 0)
		{
			E.AddAction("Remove Notes", "remove notes", "RemoveNotes", null, 'n', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		if (CheckAnythingToCleanEvent.Check(ParentObject) && CheckAnythingToCleanWithEvent.Check(IComponent<GameObject>.ThePlayer, ParentObject))
		{
			E.AddAction("Clean", "clean", "CleanItem", null, 'a', FireOnActor: false, -1);
		}
		if (Options.DebugInternals)
		{
			E.AddAction("Show Internals", "show internals", "ShowInternals", null, 'W', FireOnActor: false, -1, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Unequip")
		{
			BodyPart bodyPart = E.Actor.Body?.FindEquippedItem(E.Item);
			if (bodyPart != null)
			{
				E.Actor.FireEvent(Event.New("CommandUnequipObject", "BodyPart", bodyPart));
			}
		}
		else if (E.Command == "CleanItem")
		{
			if (!E.Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
			{
				return false;
			}
			string cleaningLiquidGeneralization = LiquidVolume.GetCleaningLiquidGeneralization();
			List<GameObject> @for = GetCleaningItemsEvent.GetFor(E.Actor, ParentObject);
			if (@for != null && @for.Count > 0)
			{
				GameObject gameObject = PickItem.ShowPicker(@for, null, PickItem.PickItemDialogStyle.SelectItemDialog, Title: "[ {{W|Choose where to use a dram of " + cleaningLiquidGeneralization + " from}} ]", Actor: E.Actor, Container: null, Cell: null, PreserveOrder: false, Regenerate: null, ShowContext: true);
				if (gameObject != null)
				{
					if (!string.IsNullOrEmpty(gameObject.Owner) && Popup.ShowYesNoCancel(gameObject.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " not owned by you. Are you sure you want to pour from " + gameObject.them + "?") != 0)
					{
						return false;
					}
					if (CleanItemsEvent.PerformFor(E.Actor, ParentObject, gameObject, out var Objects, out var Types) && Objects != null && Objects.Count > 0)
					{
						LiquidVolume.CleaningMessage(E.Actor, Objects, Types, gameObject, null, useDram: true);
						E.Actor.UseEnergy(1000, "Cleaning");
						E.RequestInterfaceExit();
						if (!string.IsNullOrEmpty(gameObject.Owner))
						{
							gameObject.pPhysics?.BroadcastForHelp(E.Actor);
						}
					}
				}
			}
			else
			{
				Popup.ShowFail("You don't have any " + cleaningLiquidGeneralization + " to clean " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " with.");
			}
		}
		else if (E.Command == "FightFire")
		{
			if (Firefighting.AttemptFirefighting(E.Actor, E.Item, 1000, Automatic: false, Dialog: true))
			{
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "Attack")
		{
			bool flag = true;
			if (E.Actor.IsPlayer() && !ParentObject.IsHostileTowards(E.Actor) && ParentObject != E.Actor.Target && Popup.ShowYesNo("Do you really want to attack " + ((ParentObject == E.Actor) ? E.Actor.itself : ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)) + "?") != 0)
			{
				flag = false;
			}
			if (flag)
			{
				E.Actor.FireEvent(Event.New("CommandAttackObject", "Defender", ParentObject));
				E.RequestInterfaceExit();
			}
		}
		else if (E.Command == "Mod")
		{
			ScreenBuffer screenBuffer = ScreenBuffer.create(80, 25);
			screenBuffer.Copy(TextConsole.CurrentBuffer);
			new TinkeringScreen().Show(IComponent<GameObject>.ThePlayer, E.Item, E);
			Popup._TextConsole.DrawBuffer(screenBuffer);
		}
		else if (E.Command == "MarkImportant")
		{
			E.Item.SetImportant(flag: true, force: true, player: true);
		}
		else if (E.Command == "MarkUnimportant")
		{
			E.Item.SetImportant(flag: false, force: true, player: true);
		}
		else if (E.Command == "AddNotes")
		{
			string text = Popup.AskString("Enter notes for " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".", "", 30);
			if (!string.IsNullOrEmpty(text))
			{
				Notes notes = E.Item.RequirePart<Notes>();
				if (string.IsNullOrEmpty(notes.Text))
				{
					notes.Text = text;
				}
				else
				{
					notes.Text = notes.Text + "\n" + text;
				}
				Popup.Show("Notes added.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
		}
		else if (E.Command == "RemoveNotes")
		{
			Notes part = E.Item.GetPart<Notes>();
			if (part != null)
			{
				ParentObject.RemovePart(part);
				Popup.Show("Notes removed.", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			}
			else
			{
				Popup.ShowFail("No notes found.");
			}
		}
		else if (E.Command == "ShowInternals")
		{
			Popup.Show(GetDebugInternalsEvent.GetFor(ParentObject), CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Equipped = E.Actor;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (_Equipped == E.Actor)
		{
			_Equipped = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (E.Actor != null && E.Actor.IsPlayer())
		{
			InInventory = E.Actor;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		InInventory = null;
		return base.HandleEvent(E);
	}

	private bool ShouldUpdateTemperature()
	{
		if (ParentObject.IsPlayer())
		{
			return true;
		}
		if (CurrentCell != null && CurrentCell.ParentZone != null && CurrentCell.ParentZone.IsActive())
		{
			return true;
		}
		if (Equipped != null && Equipped.IsPlayerControlled())
		{
			return true;
		}
		if (InInventory != null && InInventory.IsPlayerControlled())
		{
			return true;
		}
		if (CurrentCell == null && Equipped == null && InInventory == null)
		{
			GameObject objectContext = ParentObject.GetObjectContext();
			if (objectContext != null)
			{
				if (objectContext.IsPlayer())
				{
					return true;
				}
				GameObject equipped = objectContext.Equipped;
				if (equipped != null && equipped.IsPlayer())
				{
					return true;
				}
				GameObject inInventory = objectContext.InInventory;
				if (inInventory != null && inInventory.IsPlayer())
				{
					return true;
				}
				if ((equipped != null || inInventory != null) && (IsAflame() || IsFrozen()))
				{
					return true;
				}
			}
		}
		else if (IsAflame() || IsFrozen())
		{
			return true;
		}
		return false;
	}

	private void UpdateTemperature()
	{
		if (ParentObject.pRender != null && ParentObject.pRender.RenderLayer != 0 && Temperature != AmbientTemperature && CanTemperatureReturnToAmbientEvent.Check(ParentObject))
		{
			int intProperty = ParentObject.GetIntProperty("ThermalInsulation", 5);
			if (Temperature < AmbientTemperature - intProperty || Temperature > AmbientTemperature + intProperty)
			{
				int num = Temperature - AmbientTemperature;
				if (Temperature < AmbientTemperature)
				{
					num += intProperty;
					int num2 = Math.Max(5, (int)((double)(AmbientTemperature - Temperature) * 0.02));
					if (Temperature >= 25)
					{
						num2 = num2 * (100 - ParentObject.Stat("HeatResistance")) / 100;
					}
					if (num2 > 0)
					{
						Temperature += num2;
					}
				}
				else if (Temperature > AmbientTemperature)
				{
					num -= intProperty;
					int num3 = Math.Max(5, (int)((double)(Temperature - AmbientTemperature) * 0.02));
					if (Temperature <= 25)
					{
						num3 = num3 * (100 - ParentObject.Stat("ColdResistance")) / 100;
					}
					if (num3 > 0)
					{
						Temperature -= num3;
					}
				}
				if (CurrentCell != null && !CurrentCell.ParentZone.IsWorldMap())
				{
					int amount = AmbientTemperature + num;
					int phase = ParentObject.GetPhase();
					foreach (Cell localAdjacentCell in CurrentCell.GetLocalAdjacentCells())
					{
						localAdjacentCell.TemperatureChange(amount, InflamedBy, Radiant: true, MinAmbient: false, MaxAmbient: false, phase);
					}
				}
			}
		}
		if (IsVaporizing())
		{
			GameObject.validate(ref InflamedBy);
			Cell cell = CurrentCell;
			if (VaporizedEvent.Check(ParentObject, InflamedBy))
			{
				LastDamagedByType = "Vaporized";
				LastDamageAccidental = true;
				ParentObject.Die(InflamedBy, null, "You were vaporized.", ParentObject.It + ParentObject.GetVerb("were") + " @@vaporized.", Accidental: true);
				if (cell != null && !string.IsNullOrEmpty(VaporObject))
				{
					GameObject gameObject = GameObject.create(VaporObject, 0, 0, "Vaporization");
					if (InflamedBy != null && gameObject.GetPart("Gas") is Gas gas)
					{
						gas.Creator = InflamedBy;
					}
					cell.AddObject(gameObject);
				}
			}
		}
		else if (IsAflame())
		{
			if (ParentObject.FireEvent("Burn") && IsAflame())
			{
				if (ParentObject.IsPlayer() && !ParentObject.HasEffect("Burning"))
				{
					ParentObject.ApplyEffect(new Burning(1));
				}
				ParentObject.TakeDamage(Burning.GetBurningAmount(ParentObject).RollCached(), "from the fire%S!", "Fire", null, null, Environmental: AmbientTemperature >= FlameTemperature || RadiatesHeatEvent.Check(CurrentCell) || RadiatesHeatAdjacentEvent.Check(CurrentCell), Owner: InflamedBy, Attacker: null, Source: null, Perspective: null, Accidental: true, Indirect: true);
				if (ParentObject.HasTag("HotBurn") && int.TryParse(ParentObject.GetTag("HotBurn"), out var result))
				{
					Temperature += result;
				}
			}
		}
		else if (ParentObject.IsPlayer())
		{
			ParentObject.RemoveEffect("Burning");
		}
		if (IsFrozen())
		{
			if (!WasFrozen)
			{
				WasFrozen = true;
				PlayWorldSound("freeze");
				FrozeEvent.Send(ParentObject);
				if (ParentObject.IsPlayer() && !ParentObject.HasEffect("Frozen"))
				{
					ParentObject.ApplyEffect(new Frozen(1));
				}
			}
		}
		else if (WasFrozen)
		{
			WasFrozen = false;
			if (ParentObject.IsPlayer())
			{
				ParentObject.RemoveEffect("Frozen");
			}
			ThawedEvent.Send(ParentObject);
		}
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		GameObject.validate(ref LastDamagedBy);
		GameObject.validate(ref LastWeaponDamagedBy);
		GameObject.validate(ref LastProjectileDamagedBy);
		GameObject.validate(ref InflamedBy);
		if (ShouldUpdateTemperature())
		{
			UpdateTemperature();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.NoConfusion && IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.GetConfusion() > 0)
		{
			if (ConfusedName == null)
			{
				if (IComponent<GameObject>.ThePlayer.GetFuriousConfusion() > 0)
				{
					ConfusedName = "{{R|" + NameMaker.MakeName(null, null, null, null, null, null, null, null, null, "FuriousConfusion") + "}}";
				}
				else
				{
					ConfusedName = NameMaker.MakeName(null, null, null, null, null, null, null, null, null, "Confusion");
				}
			}
			E.DB.Clear();
			E.AddBase(ConfusedName);
			return false;
		}
		if (IsFrozen())
		{
			E.AddAdjective("{{freezing|frozen}}", -40);
		}
		if (IsAflame())
		{
			if (ParentObject.HasEffect("CoatedInPlasma"))
			{
				E.AddAdjective("{{auroral|auroral}}", -40);
			}
			else
			{
				E.AddAdjective("{{fiery|flaming}}", -40);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		if (ParentObject != null)
		{
			E.AddEntry("GameObject", "Blueprint", ParentObject.Blueprint);
			E.AddEntry("GameObject", "KineticAbsorption", ParentObject.GetKineticAbsorption());
			E.AddEntry("GameObject", "KineticResistance", ParentObject.GetKineticResistance());
			E.AddEntry("GameObject", "MaximumLiquidExposure", ParentObject.GetMaximumLiquidExposure());
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (ParentObject.Property != null && ParentObject.Property.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list = new List<string>(ParentObject.Property.Keys);
				list.Sort();
				foreach (string item in list)
				{
					stringBuilder.Append(item).Append(": ").Append(ParentObject.Property[item] ?? "NULL")
						.Append('\n');
				}
				E.AddEntry("GameObject", "Property", stringBuilder.ToString());
			}
			if (ParentObject.IntProperty != null && ParentObject.IntProperty.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list2 = new List<string>(ParentObject.IntProperty.Keys);
				list2.Sort();
				foreach (string item2 in list2)
				{
					stringBuilder.Append(item2).Append(": ").Append(ParentObject.IntProperty[item2])
						.Append('\n');
				}
				E.AddEntry("GameObject", "IntProperty", stringBuilder.ToString());
			}
			if (ParentObject.Statistics != null && ParentObject.Statistics.Count > 0)
			{
				stringBuilder.Clear();
				List<string> list3 = new List<string>(ParentObject.Statistics.Keys);
				list3.Sort();
				foreach (string item3 in list3)
				{
					stringBuilder.Append(item3).Append(": ").Append(ParentObject.Stat(item3) + " / " + ParentObject.BaseStat(item3))
						.Append('\n');
				}
				E.AddEntry("GameObject", "Statistics", stringBuilder.ToString());
			}
			E.AddEntry("Scanning", "ScanType", Scanning.GetScanTypeFor(ParentObject).ToString());
		}
		E.AddEntry(this, "Temperature", Temperature);
		E.AddEntry(this, "FlameTemperature", FlameTemperature);
		E.AddEntry(this, "VaporTemperature", VaporTemperature);
		E.AddEntry(this, "FreezeTemperature", FreezeTemperature);
		if (CurrentCell != null)
		{
			E.AddEntry(this, "Position", CurrentCell.X + ", " + CurrentCell.Y);
		}
		if (!string.IsNullOrEmpty(Owner))
		{
			E.AddEntry(this, "Owner", Owner);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryCategoryEvent E)
	{
		if (IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.GetConfusion() > 0)
		{
			E.Category = "???";
			return false;
		}
		if (string.IsNullOrEmpty(E.Category))
		{
			E.Category = Category;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Hitpoints")
		{
			if (E.NewValue < E.OldValue && AutoAct.IsInterruptable() && ParentObject.IsPlayerControlledAndPerceptible())
			{
				if (ParentObject.IsPlayer())
				{
					AutoAct.Interrupt();
				}
				else if (!ParentObject.IsTrifling)
				{
					AutoAct.Interrupt("you " + ParentObject.GetPerceptionVerb() + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + " being injured" + (ParentObject.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(ParentObject))), null, ParentObject);
				}
			}
			if (!ParentObject.WillCheckHP())
			{
				CheckHP(E.NewValue, E.OldValue, E.Stat.BaseValue);
			}
		}
		else if (E.Name == "HeatResistance" || E.Name == "ColdResistance")
		{
			AmbientCache = -1;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		LastDamagedBy = null;
		LastWeaponDamagedBy = null;
		LastProjectileDamagedBy = null;
		InflamedBy = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		AmbientCache = -1;
		if (IsReal && CurrentCell != null && CurrentCell.ParentZone != null)
		{
			int i = 0;
			for (int count = CurrentCell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = CurrentCell.Objects[i];
				if (gameObject == ParentObject || !ParentObject.ConsiderUnableToOccupySameCell(gameObject))
				{
					continue;
				}
				if (ParentObject.IsWall() && gameObject.IsWall() && !ParentObject.HasPart("Combat") && !gameObject.HasPart("Combat"))
				{
					if (ParentObject.GetTier() > gameObject.GetTier())
					{
						gameObject.Obliterate();
					}
					else
					{
						ParentObject.Obliterate();
					}
					break;
				}
				if (!CheckSpawnMergeEvent.Check(ParentObject, gameObject))
				{
					i = 0;
					count = CurrentCell.Objects.Count;
					break;
				}
				foreach (Cell item in CurrentCell.YieldAdjacentCells(9, LocalOnly: true))
				{
					if (item.IsPassable(ParentObject) && !item.HasObjectWithTagOrProperty("SpawnBlocker"))
					{
						ParentObject.SystemMoveTo(item, null, forced: true);
						break;
					}
				}
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public bool ProcessTakeDamage(Event E)
	{
		if (ParentObject.IsPlayer() && The.Core.IDKFA)
		{
			return false;
		}
		Damage damage = E.GetParameter("Damage") as Damage;
		GameObject gameObject = E.GetGameObjectParameter("Source") ?? E.GetGameObjectParameter("Attacker") ?? E.GetGameObjectParameter("Owner");
		GameObject gameObject2 = E.GetGameObjectParameter("Owner") ?? E.GetGameObjectParameter("Attacker") ?? ((gameObject != null && gameObject.IsCreature) ? gameObject : null);
		GameObject gameObjectParameter = E.GetGameObjectParameter("Weapon");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Projectile");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Source");
		bool flag = E.HasFlag("Indirect");
		if (E.HasParameter("Phase"))
		{
			if (!ParentObject.PhaseMatches(E.GetIntParameter("Phase")))
			{
				return false;
			}
		}
		else if (gameObject != null && !ParentObject.PhaseMatches(gameObject))
		{
			return false;
		}
		LastWeaponDamagedBy = gameObjectParameter;
		LastProjectileDamagedBy = gameObjectParameter2;
		LastDamagedBy = gameObject2;
		if (GameObject.validate(ref LastDamagedBy))
		{
			LastDamageAccidental = damage.HasAttribute("Accidental") && !LastDamagedBy.IsHostileTowards(ParentObject);
			if (LastDamagedBy.IsPlayer() && LastDamagedBy != ParentObject)
			{
				if (!flag && Sidebar.CurrentTarget == null && ParentObject.HasPart("Combat") && IComponent<GameObject>.Visible(ParentObject))
				{
					Sidebar.CurrentTarget = ParentObject;
				}
				if (The.Core.IDKFA)
				{
					damage.Amount = 999;
				}
			}
		}
		else
		{
			LastDamageAccidental = false;
		}
		if (!damage.HasAttribute("IgnoreResist"))
		{
			int num = ParentObject.Stat("AcidResistance");
			if (num != 0 && damage.Amount > 0 && damage.IsAcidDamage())
			{
				if (num > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num) / 100f));
					if (num < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num / -100f));
				}
			}
			int num2 = ParentObject.Stat("HeatResistance");
			if (num2 != 0 && damage.Amount > 0 && damage.IsHeatDamage())
			{
				if (num2 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num2) / 100f));
					if (num2 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num2 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num2 / -100f));
				}
			}
			int num3 = ParentObject.Stat("ColdResistance");
			if (num3 != 0 && damage.Amount > 0 && damage.IsColdDamage())
			{
				if (num3 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num3) / 100f));
					if (num3 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num3 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num3 / -100f));
				}
			}
			int num4 = ParentObject.Stat("ElectricResistance");
			if (num4 != 0 && damage.Amount > 0 && damage.IsElectricDamage())
			{
				if (num4 > 0)
				{
					damage.Amount = (int)((float)damage.Amount * ((float)(100 - num4) / 100f));
					if (num4 < 100 && damage.Amount < 1)
					{
						damage.Amount = 1;
					}
				}
				else if (num4 < 0)
				{
					damage.Amount += (int)((float)damage.Amount * ((float)num4 / -100f));
				}
			}
		}
		string stringParameter = E.GetStringParameter("Message", "");
		if (!BeforeApplyDamageEvent.Check(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		if (!AttackerDealingDamageEvent.Check(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		if (!LateBeforeApplyDamageEvent.Check(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E))
		{
			return false;
		}
		string value = "killed";
		string value2 = "by";
		string text = null;
		bool flag2 = false;
		bool flag3 = LastDamageAccidental;
		bool flag4 = false;
		bool flag5 = false;
		if (damage.Amount > 0)
		{
			if (damage.IsHeatDamage())
			{
				if (damage.HasAttribute("NoBurn"))
				{
					LastDamagedByType = "Heat";
					value = "cooked";
				}
				else
				{
					LastDamagedByType = "Fire";
					value = "immolated";
				}
			}
			else if (damage.HasAttribute("Vaporized"))
			{
				LastDamagedByType = "Vaporized";
				value = "vaporized";
				flag3 = false;
			}
			else if (damage.IsColdDamage())
			{
				LastDamagedByType = "Cold";
				value = "frozen to death";
			}
			else if (damage.IsElectricDamage())
			{
				LastDamagedByType = "Electric";
				value = "electrocuted";
			}
			else if (damage.IsAcidDamage())
			{
				LastDamagedByType = "Acid";
				value = "dissolved";
			}
			else if (damage.IsDisintegrationDamage())
			{
				LastDamagedByType = "Disintegration";
				value = "disintegrated";
			}
			else if (damage.HasAttribute("Laser"))
			{
				LastDamagedByType = "Light";
				value = "lased to death";
			}
			else if (damage.HasAttribute("Light"))
			{
				LastDamagedByType = "Light";
				value = "illuminated to death";
			}
			else if (damage.HasAttribute("Poison"))
			{
				LastDamagedByType = "Poison";
				value = "died of poison";
				flag2 = true;
				value2 = "from";
			}
			else if (damage.HasAttribute("Bleeding"))
			{
				LastDamagedByType = "Bleeding";
				value = "bled to death";
				flag2 = true;
				value2 = "because of";
			}
			else if (damage.HasAttribute("Asphyxiation"))
			{
				LastDamagedByType = "Asphyxiation";
				value = "died of asphyxiation";
				flag2 = true;
				value2 = "from";
			}
			else if (damage.HasAttribute("Metabolic"))
			{
				LastDamagedByType = "Metabolic";
				text = "metabolism";
				value = "collapsed";
				value2 = "from";
			}
			else if (damage.HasAttribute("Drain"))
			{
				LastDamagedByType = "Drain";
				text = "vital essence was";
				value = "drained to extinction";
				value2 = "by";
			}
			else if (damage.HasAttribute("Psionic"))
			{
				LastDamagedByType = "Psionic";
				value = "psychically extinguished";
			}
			else if (damage.HasAttribute("Mental"))
			{
				LastDamagedByType = "Mental";
				value = "mentally obliterated";
			}
			else if (damage.HasAttribute("Thorns"))
			{
				LastDamagedByType = "Thorns";
				value = "pricked to death";
			}
			else if (damage.HasAttribute("Collision"))
			{
				LastDamagedByType = "Collision";
				value2 = "by colliding with";
			}
			else if (damage.HasAttribute("Bite"))
			{
				LastDamagedByType = "Bite";
				value = "bitten to death";
			}
			else
			{
				LastDamagedByType = ((damage.Attributes.Count > 0) ? damage.Attributes[0] : "Physical");
				flag5 = true;
			}
			if (damage.HasAttribute("Illusion"))
			{
				flag4 = true;
			}
			if (damage.HasAttribute("Neutron") && ParentObject.IsPlayer())
			{
				AchievementManager.SetAchievement("ACH_CRUSHED_UNDER_SUNS");
			}
		}
		if (E.HasParameter("DeathReason"))
		{
			LastDeathReason = E.GetStringParameter("DeathReason");
			if (E.HasParameter("ThirdPersonDeathReason"))
			{
				LastThirdPersonDeathReason = E.GetStringParameter("ThirdPersonDeathReason");
			}
			else
			{
				LastThirdPersonDeathReason = GameText.RoughConvertSecondPersonToThirdPerson(LastDeathReason, ParentObject);
			}
		}
		else
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			StringBuilder stringBuilder2 = Event.NewStringBuilder();
			if (flag4)
			{
				stringBuilder.Append("You convinced yourself ").Append((text != null) ? "your" : "you").Append(' ');
				stringBuilder2.Append(ParentObject.It).Append(" @@convinced ").Append(ParentObject.itself)
					.Append(' ')
					.Append((text != null) ? ParentObject.its : ParentObject.it)
					.Append(' ');
			}
			else
			{
				stringBuilder.Append((text != null) ? "Your " : "You ");
				stringBuilder2.Append((text != null) ? ParentObject.Its : ParentObject.It).Append(' ');
			}
			if (text != null)
			{
				stringBuilder.Append(text).Append(' ');
				stringBuilder2.Append(text).Append(' ');
			}
			else if (!flag2)
			{
				stringBuilder.Append("were ");
				stringBuilder2.Append(ParentObject.GetVerb("were", PrependSpace: false, PronounAntecedent: true)).Append(' ');
			}
			if (flag3 && text == null)
			{
				stringBuilder.Append("accidentally ");
				stringBuilder2.Append("accidentally ");
			}
			stringBuilder.Append(value);
			if (!flag4)
			{
				stringBuilder2.Append("@@");
			}
			stringBuilder2.Append(value);
			if (gameObject != null && gameObject != ParentObject)
			{
				if (!string.IsNullOrEmpty(value2))
				{
					stringBuilder.Append(' ').Append(value2);
					stringBuilder2.Append(' ').Append(value2);
				}
				stringBuilder.Append(' ').Append(gameObject.an());
				stringBuilder2.Append(' ').Append(gameObject.an());
			}
			if (flag5)
			{
				GameObject gameObject3 = gameObjectParameter2 ?? gameObjectParameter ?? gameObjectParameter3;
				if (gameObject3 != null)
				{
					stringBuilder2.Append("## with ").Append(gameObject3.an()).Append("##");
				}
			}
			stringBuilder.Append('.');
			stringBuilder2.Append('.');
			LastDeathReason = stringBuilder.ToString();
			LastThirdPersonDeathReason = stringBuilder2.ToString();
		}
		if (IsPlayer())
		{
			The.Game.DeathReason = LastDeathReason;
		}
		Statistic stat = ParentObject.GetStat("Hitpoints");
		if (stat == null)
		{
			return false;
		}
		ParentObject.SplitFromStack();
		if (!string.IsNullOrEmpty(stringParameter) && (damage.Amount > 0 || !E.HasFlag("SilentIfNoDamage")))
		{
			bool flag6 = false;
			if (ParentObject.IsPlayer())
			{
				flag6 = true;
			}
			else if (gameObject2 != null && gameObject2.IsPlayer() && (IComponent<GameObject>.Visible(ParentObject) || gameObject2.isAdjacentTo(ParentObject)))
			{
				flag6 = true;
			}
			else if (ParentObject.HasPart("Combat") || E.HasFlag("ShowForInanimate"))
			{
				GameObject gameObjectParameter4 = E.GetGameObjectParameter("Perspective");
				if (gameObjectParameter4 != null && IComponent<GameObject>.Visible(gameObjectParameter4))
				{
					flag6 = true;
				}
				else if ((gameObject2 == null || E.HasFlag("ShowUninvolved")) && Visible())
				{
					flag6 = true;
				}
			}
			string stringParameter2 = E.GetStringParameter("ShowDamageType", "damage");
			if (flag6)
			{
				stringParameter = stringParameter.Replace("%d", damage.Amount.ToString());
				stringParameter = stringParameter.Replace("%e", stringParameter2);
				GameObject gameObject4 = gameObject2;
				if (flag && gameObjectParameter3 != null)
				{
					gameObject4 = gameObjectParameter3;
				}
				if (gameObject4 != null)
				{
					if (gameObject4.IsPlayer())
					{
						stringParameter = stringParameter.Replace("%o", "your");
						stringParameter = stringParameter.Replace("%O", "you");
						stringParameter = stringParameter.Replace("%S", " you started");
						stringParameter = stringParameter.Replace("%t", "your");
						stringParameter = stringParameter.Replace("%T", "you");
					}
					else if (gameObject4 == ParentObject)
					{
						if (stringParameter.Contains("%o"))
						{
							stringParameter = stringParameter.Replace("%o", gameObject4.its);
						}
						if (stringParameter.Contains("%O"))
						{
							stringParameter = stringParameter.Replace("%O", gameObject4.itself);
						}
						if (stringParameter.Contains("%S"))
						{
							stringParameter = stringParameter.Replace("%S", " started by " + gameObject4.itself);
						}
						if (stringParameter.Contains("%t"))
						{
							stringParameter = stringParameter.Replace("%t", gameObject4.its);
						}
						if (stringParameter.Contains("%T"))
						{
							stringParameter = stringParameter.Replace("%T", gameObject4.It);
						}
					}
					else
					{
						if (stringParameter.Contains("%o"))
						{
							stringParameter = stringParameter.Replace("%o", Grammar.MakePossessive(gameObject4.ShortDisplayName));
						}
						if (stringParameter.Contains("%O"))
						{
							stringParameter = stringParameter.Replace("%O", gameObject4.t());
						}
						if (stringParameter.Contains("%S"))
						{
							stringParameter = stringParameter.Replace("%S", " started by " + gameObject4.t());
						}
						if (stringParameter.Contains("%t"))
						{
							stringParameter = stringParameter.Replace("%t", Grammar.MakePossessive(gameObject4.t()));
						}
						if (stringParameter.Contains("%T"))
						{
							stringParameter = stringParameter.Replace("%T", gameObject4.T());
						}
					}
				}
				else
				{
					stringParameter = stringParameter.Replace("%o", "the");
					stringParameter = stringParameter.Replace("%O ", "");
					stringParameter = stringParameter.Replace("%S", "");
					stringParameter = stringParameter.Replace("%t", "the");
					stringParameter = stringParameter.Replace("%T ", "");
				}
				if (E.HasParameter("NoDamageMessage"))
				{
					IComponent<GameObject>.AddPlayerMessage(stringParameter);
				}
				else if (ParentObject.IsPlayer())
				{
					StringBuilder stringBuilder3 = Event.NewStringBuilder();
					stringBuilder3.Append("{{r|You take ").Append(damage.Amount).Append(' ')
						.Append(stringParameter2)
						.Append(' ')
						.Append(stringParameter)
						.Append("}}");
					IComponent<GameObject>.AddPlayerMessage(stringBuilder3.ToString());
				}
				else if (ParentObject.HasPart("Combat") || E.HasFlag("ShowForInanimate"))
				{
					StringBuilder stringBuilder4 = Event.NewStringBuilder();
					stringBuilder4.Append(ParentObject.T()).Append(" ").Append(ParentObject.GetVerb("take", PrependSpace: false))
						.Append(' ')
						.Append(damage.Amount)
						.Append(' ')
						.Append(stringParameter2)
						.Append(' ')
						.Append(stringParameter);
					IComponent<GameObject>.AddPlayerMessage(stringBuilder4.ToString());
				}
			}
		}
		AttackerDealtDamageEvent.Send(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E);
		ParentObject.FireEvent(Event.New("AIMessage", "Object", ParentObject, "Message", "Attacked", "By", gameObject2));
		if (damage.Amount > 0)
		{
			if (ParentObject.IsPlayer())
			{
				if (AutoAct.IsInterruptable())
				{
					AutoAct.Interrupt();
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(Owner))
				{
					CheckBroadcastForHelp(gameObject2, LastDamageAccidental);
				}
				if (AutoAct.IsInterruptable() && ParentObject.IsPlayerLedAndPerceptible() && !ParentObject.IsTrifling)
				{
					AutoAct.Interrupt("you " + ParentObject.GetPerceptionVerb() + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + " being injured" + (ParentObject.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(ParentObject))), null, ParentObject);
				}
			}
			PlayWorldSound(ParentObject.GetTagOrStringProperty("TakeDamageSound"));
			if (base.juiceEnabled && ParentObject.IsValid() && (ParentObject.IsPlayer() || (gameObject2 != null && gameObject2.IsPlayer() && gameObject2.isAdjacentTo(ParentObject)) || Visible()))
			{
				float scale = 1f;
				if (stat.BaseValue > 0)
				{
					float num5 = (float)damage.Amount / (float)stat.BaseValue;
					if ((double)num5 >= 0.5)
					{
						scale = 1.8f;
					}
					else if ((double)num5 >= 0.4)
					{
						scale = 1.6f;
					}
					else if ((double)num5 >= 0.3)
					{
						scale = 1.4f;
					}
					else if ((double)num5 >= 0.2)
					{
						scale = 1.2f;
					}
					else if ((double)num5 >= 0.1)
					{
						scale = 1.1f;
					}
				}
				CombatJuice.floatingText(ParentObject, "-" + damage.Amount, ColorUtility.ColorMap['R'], 1.5f, 24f, scale);
			}
			BeforeTookDamageEvent.Send(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E);
			stat.Penalty += damage.Amount;
			if (ParentObject.IsValid() && !ParentObject.IsInGraveyard())
			{
				TookDamageEvent.Send(damage, ParentObject, gameObject2, gameObjectParameter3, gameObjectParameter, gameObjectParameter2, flag, E);
				if (ParentObject.IsValid() && !ParentObject.IsInGraveyard())
				{
					if (damage.HasAttribute("Environmental"))
					{
						TookEnvironmentalDamageEvent.Send(damage, ParentObject, gameObject2, gameObjectParameter3, flag, E);
					}
					if (ParentObject.IsValid() && !ParentObject.IsPlayer() && !ParentObject.IsInGraveyard())
					{
						ParentObject.UpdateVisibleStatusColor();
					}
				}
			}
		}
		ParentObject.CheckStack();
		return true;
	}

	public bool ProcessTargetedMove(Cell TargetCell, string Type, string PreEvent, string PostEvent, int? EnergyCost = null, bool Forced = false, bool System = false, bool IgnoreCombat = false, bool IgnoreGravity = false, bool NoStack = false, string LeaveVerb = null, string ArriveVerb = null)
	{
		if (CurrentCell == TargetCell)
		{
			return true;
		}
		Zone zone = CurrentCell?.ParentZone;
		int amount = EnergyCost ?? 1000;
		if ((ParentObject.FireEvent(Event.New(PreEvent, "Cell", TargetCell, Type, 1)) || System) && ParentObject.ProcessBeginMove(out var _, TargetCell, Forced, System, IgnoreGravity, NoStack, null, Type))
		{
			if (ParentObject.HasPart("Combat") && !IgnoreCombat)
			{
				foreach (GameObject item in TargetCell.GetObjectsWithPartReadonly("Combat"))
				{
					if (item.IsHostileTowards(ParentObject))
					{
						if (!ParentObject.IsPlayer())
						{
							continue;
						}
						if (!(The.Core.PlayerWalking == ""))
						{
							The.Core.RenderBase();
							continue;
						}
						ParentObject.FireEvent(Event.New("CommandAttackCell", "Cell", TargetCell));
					}
					else if (ParentObject.IsPlayer() && Popup.ShowYesNo("Do you really want to attack " + item.DisplayName + "?") == DialogResult.Yes)
					{
						ParentObject.FireEvent(Event.New("CommandAttackCell", "Cell", TargetCell));
					}
					goto IL_02fd;
				}
			}
			Cell cell = CurrentCell;
			if (ParentObject.ProcessObjectLeavingCell(cell, Forced, System, IgnoreGravity, NoStack, null, Type) && ParentObject.ProcessEnteringCell(TargetCell, Forced, System, IgnoreGravity, NoStack, null, Type) && ParentObject.ProcessObjectEnteringCell(TargetCell, Forced, System, IgnoreGravity, NoStack, null, Type) && ParentObject.ProcessLeaveCell(cell, Forced, System, IgnoreGravity, NoStack, null, Type))
			{
				if (!ParentObject.IsPlayer() && !string.IsNullOrEmpty(LeaveVerb))
				{
					DidX(LeaveVerb, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
				}
				cell?.RemoveObject(ParentObject, Forced, System, IgnoreGravity, NoStack, Repaint: true, null, Type);
				TargetCell.AddObject(ParentObject, Forced, System, IgnoreGravity, NoStack, Repaint: true, null, Type);
				if (!ParentObject.IsPlayer() && !string.IsNullOrEmpty(ArriveVerb))
				{
					DidX(ArriveVerb, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
				}
				ParentObject.UseEnergy(amount, "Movement");
				if (IsPlayer() && zone != TargetCell.ParentZone)
				{
					The.ZoneManager.SetActiveZone(TargetCell.ParentZone.ZoneID);
					The.ZoneManager.ProcessGoToPartyLeader();
				}
				ParentObject.FireEvent(Event.New(PostEvent, "FromCell", cell, Type, 1));
				return true;
			}
		}
		goto IL_02fd;
		IL_02fd:
		ParentObject.FireEvent("MoveFailed");
		return false;
	}

	public void TeardownForDestroy(bool Silent = false)
	{
		Body body = Equipped?.Body;
		if (body != null)
		{
			int num = 0;
			BodyPart value;
			while ((value = body.FindEquippedItem(ParentObject)) != null)
			{
				if (++num >= 100)
				{
					MetricsManager.LogError("infinite looping trying to unequip " + ParentObject.DebugName);
					break;
				}
				Event @event = Event.New("CommandForceUnequipObject");
				@event.SetParameter("BodyPart", value);
				@event.SetFlag("NoTake", State: true);
				@event.SetSilent(Silent: true);
				Equipped.FireEvent(@event);
			}
		}
		InInventory?.Inventory.RemoveObject(ParentObject);
		if (CurrentCell != null)
		{
			if (CurrentCell.ParentZone != null && ParentObject.WantEvent(CheckExistenceSupportEvent.ID, CheckExistenceSupportEvent.CascadeLevel))
			{
				CurrentCell.ParentZone.WantSynchronizeExistence();
			}
			CurrentCell.RemoveObject(ParentObject);
		}
		The.Game?.Graveyard?.AddObject(ParentObject);
		The.ActionManager?.RemoveActiveObject(ParentObject);
	}

	public bool ProcessTemperatureChange(int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, int Phase = 0, int? Min = null, int? Max = null)
	{
		if (SpecificHeat == 0f)
		{
			return false;
		}
		if (Phase == 0 && Actor != null)
		{
			Phase = Actor.GetPhase();
		}
		if (!ParentObject.PhaseMatches(Phase))
		{
			return false;
		}
		if (ParentObject.HasRegisteredEvent("BeforeTemperatureChange"))
		{
			Event @event = Event.New("BeforeTemperatureChange");
			@event.SetParameter("Amount", Amount);
			@event.SetFlag("Radiant", Radiant);
			if (!ParentObject.FireEvent(@event))
			{
				return false;
			}
			Amount = @event.GetIntParameter("Amount");
		}
		if (Actor != null && Actor.HasRegisteredEvent("AttackerBeforeTemperatureChange"))
		{
			Event event2 = Event.New("AttackerBeforeTemperatureChange");
			event2.SetParameter("Amount", Amount);
			event2.SetFlag("Radiant", Radiant);
			if (!Actor.FireEvent(event2))
			{
				return false;
			}
			Amount = event2.GetIntParameter("Amount");
		}
		int temperature = Temperature;
		bool num = IsAflame();
		if (Radiant)
		{
			if (Amount > 0)
			{
				int num2 = ParentObject.Stat("HeatResistance");
				if (num2 != 0)
				{
					Amount = (int)((float)Amount * ((float)(100 - num2) / 100f));
				}
			}
			else if (Amount < 0)
			{
				int num3 = ParentObject.Stat("ColdResistance");
				if (num3 != 0)
				{
					Amount = (int)((float)Amount * ((float)(100 - num3) / 100f));
				}
			}
			if (Temperature > Amount)
			{
				if (!IsAflame())
				{
					Temperature += (int)((float)(Amount - Temperature) * (0.035f / SpecificHeat));
				}
			}
			else
			{
				Temperature += (int)((float)(Amount - Temperature) * (0.035f / SpecificHeat));
			}
		}
		else
		{
			Amount = (int)((float)Amount / SpecificHeat);
			if (ParentObject.GetPart("Mutations") is Mutations mutations && mutations.HasMutation("FattyHump"))
			{
				Amount /= 2;
			}
			if (Amount > 0 && Temperature + Amount > 50)
			{
				int num4 = ParentObject.Stat("HeatResistance");
				if (num4 != 0)
				{
					Amount = (int)((float)Amount * ((float)(100 - num4) / 100f));
				}
			}
			else if (Amount < 0 && Temperature + Amount < 25)
			{
				int num5 = ParentObject.Stat("ColdResistance");
				if (num5 != 0)
				{
					Amount = (int)((float)Amount * ((float)(100 - num5) / 100f));
				}
			}
			Temperature += Amount;
		}
		if (!num && IsAflame())
		{
			InflamedBy = Actor;
			if (AutoAct.IsActive())
			{
				if (ParentObject.IsPlayer())
				{
					AutoAct.Interrupt("you caught fire");
				}
				else if (ParentObject.IsPlayerLedAndPerceptible() && !ParentObject.IsTrifling)
				{
					AutoAct.Interrupt("you " + ParentObject.GetPerceptionVerb() + " " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + " catch fire" + (ParentObject.IsVisible() ? "" : (" " + The.Player.DescribeDirectionToward(ParentObject))), null, ParentObject);
				}
			}
		}
		if (MinAmbient && Temperature < AmbientTemperature)
		{
			Temperature = AmbientTemperature;
		}
		if (MaxAmbient && Temperature > AmbientTemperature)
		{
			Temperature = AmbientTemperature;
		}
		if (Min.HasValue && Temperature < Min)
		{
			Temperature = Min.Value;
		}
		if (Max.HasValue && Temperature > Max)
		{
			Temperature = Max.Value;
		}
		if (Temperature != temperature)
		{
			EnvironmentalUpdateEvent.Send(ParentObject);
		}
		return true;
	}

	public bool CheckHP(int? CurrentHP = null, int? PreviousHP = null, int? MaxHP = null, bool Preregistered = false)
	{
		if (Preregistered)
		{
			ParentObject.WillCheckHP(false);
		}
		if ((CurrentHP ?? ParentObject.hitpoints) <= 0)
		{
			if (GameObject.validate(ref LastDamagedBy))
			{
				if (string.IsNullOrEmpty(LastDeathReason))
				{
					if (LastDamagedBy == ParentObject)
					{
						LastDeathReason = "You " + (LastDamageAccidental ? "accidentally " : "") + "killed " + ParentObject.itself + ".";
					}
					else
					{
						LastDeathReason = "You were " + (LastDamageAccidental ? "accidentally " : "") + "killed by " + LastDamagedBy.an() + ".";
					}
				}
				if (string.IsNullOrEmpty(LastThirdPersonDeathReason))
				{
					if (LastDamagedBy == ParentObject)
					{
						LastThirdPersonDeathReason = ParentObject.It + " @@" + (LastDamageAccidental ? "accidentally " : "") + "killed " + ParentObject.itself + ".";
					}
					else
					{
						LastThirdPersonDeathReason = ParentObject.It + " were @@" + (LastDamageAccidental ? "accidentally " : "") + "killed by " + LastDamagedBy.an() + ".";
					}
				}
				if (ParentObject.IsPlayer())
				{
					if (LastDamagedBy.HasProperty("EvilTwin") && LastDamagedBy.HasProperty("PlayerCopy"))
					{
						AchievementManager.SetAchievement("ACH_KILLED_BY_TWIN");
					}
					if (LastDamagedBy.Blueprint == "Chute Crab")
					{
						AchievementManager.SetAchievement("ACH_KILLED_BY_CHUTE_CRAB");
					}
				}
			}
			if (ParentObject.IsPlayer() && string.IsNullOrEmpty(The.Game.DeathReason) && !string.IsNullOrEmpty(LastDeathReason))
			{
				The.Game.DeathReason = LastDeathReason;
			}
			ParentObject.Die(LastDamagedBy, null, LastDeathReason, LastThirdPersonDeathReason, LastDamageAccidental, LastWeaponDamagedBy, LastProjectileDamagedBy);
			return true;
		}
		if ((!PreviousHP.HasValue || CurrentHP < PreviousHP) && CurrentHP <= (MaxHP ?? ParentObject.baseHitpoints) / 4 && !ParentObject.IsCreature && ParentObject.HasTagOrProperty("Breakable"))
		{
			ParentObject.ForceApplyEffect(new Broken(FromDamage: true));
		}
		return false;
	}
}
