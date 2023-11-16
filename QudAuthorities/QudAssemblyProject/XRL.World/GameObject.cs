using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using HistoryKit;
using Qud.API;
using UnityEngine;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.Messages;
using XRL.Names;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Encounters;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.Skills;
using XRL.World.Tinkering;

namespace XRL.World;

[Serializable]
public class GameObject
{
	public class DisplayNameSort : Comparer<GameObject>
	{
		public override int Compare(GameObject a, GameObject b)
		{
			return ConsoleLib.Console.ColorUtility.CompareExceptFormattingAndCase(a.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: true), b.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: true));
		}
	}

	private struct PullDownChoice
	{
		public string location;

		public int X;

		public int Y;
	}

	[NonSerialized]
	private static Event eHealing = new Event("Healing", "Amount", 0);

	[NonSerialized]
	private static Event eCommandTakeObject = new Event("CommandTakeObject", "Object", (object)null, "Context", (string)null);

	[NonSerialized]
	private static Event eCommandTakeObjectSilent = new Event("CommandTakeObject", "Object", null, "Context", null, "IsSilent", 1);

	[NonSerialized]
	private static Event eCommandTakeObjectWithEnergyCost = new Event("CommandTakeObject", "Object", null, "Context", null, "EnergyCost", 0);

	[NonSerialized]
	private static Event eCommandTakeObjectSilentWithEnergyCost = new Event("CommandTakeObject", "Object", null, "Context", null, "IsSilent", 1, "EnergyCost", 0);

	[NonSerialized]
	private static Event eIsMobile = new ImmutableEvent("IsMobile");

	[NonSerialized]
	private static Event eCanHypersensesDetect = new ImmutableEvent("CanHypersensesDetect");

	[NonSerialized]
	public Dictionary<GameObject, GameObject> DeepCopyInventoryObjectMap;

	[NonSerialized]
	public static int StatsLoaded = 0;

	[NonSerialized]
	public static int PartsLoaded = 0;

	[NonSerialized]
	public static int EffectsLoaded = 0;

	[NonSerialized]
	public static int PartEventsLoaded = 0;

	[NonSerialized]
	public static int EffectEventsLoaded = 0;

	[NonSerialized]
	public static Dictionary<string, int> LoadedEvents = new Dictionary<string, int>();

	[NonSerialized]
	public static Dictionary<GameObject, List<ExternalEventBind>> ExternalLoadBindings = null;

	public string Blueprint = "Object";

	[NonSerialized]
	public Render _pRender;

	[NonSerialized]
	public XRL.World.Parts.Physics _pPhysics;

	[NonSerialized]
	public Brain _pBrain;

	public Dictionary<string, int> _IntProperty;

	public Dictionary<string, string> _Property;

	[NonSerialized]
	public Dictionary<string, Statistic> Statistics = new Dictionary<string, Statistic>();

	[NonSerialized]
	public Statistic _Energy;

	public List<Effect> _Effects;

	[NonSerialized]
	private Dictionary<string, int> TakeObjectsFromTableGeneration = new Dictionary<string, int>();

	[NonSerialized]
	private bool TakeObjectsFromTableGenerationInUse;

	[NonSerialized]
	private RenderEvent _contextRender;

	[NonSerialized]
	public string _CachedStrippedName;

	/// <summary>
	///              The object blueprint tags that control gender and pronoun set setup are:
	///
	///              Gender: this can be used to specify exactly one gender name from Genders.xml that
	///              will be assigned to the object.  If both Gender and RandomGender are specified,
	///              Gender controls.
	///
	///              RandomGender: this can be used to specify a comma-separated list (no spaces around
	///              the commas) of gender specifiers, one of which will be randomly selected.  The
	///              specifiers may be gender names or abstract specifications from the following list.
	///              ("Personal" means UseBareIndicative is false, which essentially means the gender
	///              is treated as being for a person rather than a thing.  "Singular" means Plural is
	///              false, that is, the gender addresses a singular subject.  "Generic" means the
	///              gender is considered generic to the world rather than specific to an individual or
	///              group.)
	///
	///                - generate: if EnableGeneration is true in Genders.xml, procedurally generate
	///                  a singular personal gender that will be registered with the system as non-generic;
	///                  otherwise select a random personal singular gender
	///                - generatemaybeplural: as generate, but with a 10% chance of being plural
	///                - generatemaybenonpersonal: as generate, but with a 10% chance of being non-personal
	///                - generatemaybepluralmaybenonpersonal: as generate, but with a 10% chance
	///                  of being plural and a 10% chance of being non-personal
	///                - any: randomly select from any gender in the system
	///                - anyplural: randomly select a plural gender
	///                - anysingular: randomly select a singular gender
	///                - generic: randomly select a generic gender
	///                - genericplural: randomly select a generic plural gender
	///                - genericsingular: randomly select a generic singular gender
	///                - personal: randomly select a personal gender
	///                - personalplural: randomly select a personal plural gender
	///                - personalsingular: randomly select a personal singular gender
	///                - genericpersonal: randomly select a generic personal gender
	///                - genericpersonalplural: randomly select a generic personal plural gender
	///                - genericpersonalsingular: randomly select a generic personal singular gender
	///                - nonpersonal: randomly select a nonpersonal gender
	///                - nonpersonalplural: randomly select a nonpersonal plural gender
	///                - nonpersonalsingular: randomly select a nonpersonal singular gender
	///                - genericnonpersonal: randomly select a generic nonpersonal gender
	///                - genericnonpersonalplural: randomly select a generic nonpersonal plural gender
	///                - genericnonpersonalsingular: randomly select a generic nonpersonal singular gender
	///
	///              PronounSet: this can be used to specify a pronoun set that will be assigned to the
	///              object.  This can be the full slash-separated set of values that make up a fully
	///              characterized pronoun set name (see PronounSet.CalculateName()) or a limited subset
	///              of these, like "xe/xem/xyr", from which the system will attempt to derive the full
	///              set of pronouns as best it can.  If both PronounSet and RandomPronounSet are specified,
	///              PronounSet controls.
	///
	///              RandomPronounSet: similar to RandomGender, but operating on pronoun sets.  The same set
	///              of abstract specifications are available, referring to pronoun sets rather than genders
	///              (and the generation control used is the one in PronounSets.xml).
	///
	///              RandomPronounSetChance: if specified, RandomPronounSet will only take effect this
	///              percentage of the time.  Otherwise, it always takes effect.
	///              </summary>
	public string GenderName;

	public string PronounSetName;

	public bool PronounSetKnown;

	public Dictionary<string, List<Effect>> RegisteredEffectEvents;

	public Dictionary<string, List<IPart>> RegisteredPartEvents;

	public List<IPart> PartsList = new List<IPart>(8);

	[NonSerialized]
	public byte _isCombatObject = byte.MaxValue;

	private static List<Effect> targetEffects = new List<Effect>();

	[NonSerialized]
	private static Event eVisibleStatusColor = new Event("VisibleStatusColor", "Color", null);

	[NonSerialized]
	private static List<Effect> SameAsEffectsUsed = new List<Effect>(16);

	[NonSerialized]
	private static Event eCheckRealityDistortionUsabilityThreshold100 = new ImmutableEvent("CheckRealityDistortionUsability", "Threshold", 100);

	[NonSerialized]
	private Event eCanChangeMovementMode = new Event("CanChangeMovementMode", "To", null, "ShowMessage", 0, "Involuntary", 0);

	[NonSerialized]
	private Event eCanChangeBodyPosition = new Event("CanChangeBodyPosition", "To", null, "ShowMessage", 0, "Involuntary", 0);

	[NonSerialized]
	private Event eCanMoveExtremities = new Event("CanMoveExtremities", "ShowMessage", 0, "Involuntary", 0);

	[NonSerialized]
	private static List<IPart> TurnTickParts = new List<IPart>(8);

	[NonSerialized]
	private static bool TurnTickPartsInUse = false;

	private byte WantTurnTickCache;

	[NonSerialized]
	private static Type[] handleEventMethodParameterList = new Type[1];

	[NonSerialized]
	private static object[] handleEventMethodArgumentList1 = new object[1];

	[NonSerialized]
	private static bool handleEventMethodArgumentList1InUse = false;

	[NonSerialized]
	private static object[] handleEventMethodArgumentList2 = new object[1];

	[NonSerialized]
	private static bool handleEventMethodArgumentList2InUse = false;

	[NonSerialized]
	private static object[] handleEventMethodArgumentList3 = new object[1];

	[NonSerialized]
	private static bool handleEventMethodArgumentList3InUse = false;

	[NonSerialized]
	private static Dictionary<Type, Dictionary<Type, MethodInfo>> handleEventLookup = new Dictionary<Type, Dictionary<Type, MethodInfo>>(512);

	[NonSerialized]
	private bool Dying;

	public bool IsReal => pPhysics?.IsReal ?? false;

	public bool Takeable => pPhysics?.Takeable ?? false;

	public bool IsScenery => (pRender?.RenderLayer ?? 0) <= 0;

	public bool Slimewalking
	{
		get
		{
			if (IntProperty.TryGetValue("Slimewalking", out var value) && value != 0)
			{
				return value > 0;
			}
			return HasTag("Slimewalking");
		}
		set
		{
			if (value)
			{
				SetIntProperty("Slimewalking", 1);
			}
			else
			{
				SetIntProperty("Slimewalking", -1);
			}
		}
	}

	public bool Polypwalking
	{
		get
		{
			if (IntProperty.TryGetValue("Polypwalking", out var value) && value != 0)
			{
				return value > 0;
			}
			return HasTag("Polypwalking");
		}
		set
		{
			if (value)
			{
				SetIntProperty("Polypwalking", 1);
			}
			else
			{
				SetIntProperty("Polypwalking", -1);
			}
		}
	}

	public bool Strutwalking
	{
		get
		{
			if (IntProperty.TryGetValue("Strutwalking", out var value) && value != 0)
			{
				return value > 0;
			}
			return HasTag("Strutwalking");
		}
		set
		{
			if (value)
			{
				SetIntProperty("Strutwalking", 1);
			}
			else
			{
				SetIntProperty("Strutwalking", -1);
			}
		}
	}

	public bool Reefer
	{
		get
		{
			if (IntProperty.TryGetValue("Reefer", out var value) && value != 0)
			{
				return value > 0;
			}
			return HasTag("Reefer");
		}
		set
		{
			if (value)
			{
				SetIntProperty("Reefer", 1);
			}
			else
			{
				SetIntProperty("Reefer", -1);
			}
		}
	}

	public bool IsCurrency
	{
		get
		{
			if (IntProperty.TryGetValue("Currency", out var value) && value > 0)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (value)
			{
				ModIntProperty("Currency", 1, RemoveIfZero: true);
			}
			else
			{
				ModIntProperty("Currency", -1, RemoveIfZero: true);
			}
		}
	}

	public GenotypeEntry genotypeEntry
	{
		get
		{
			string genotype = GetGenotype();
			if (genotype != null && GenotypeFactory.GenotypesByName.TryGetValue(genotype, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public SubtypeEntry subtypeEntry
	{
		get
		{
			string stringProperty;
			if ((stringProperty = GetStringProperty("Subtype")) != null && SubtypeFactory.SubtypesByName.TryGetValue(stringProperty, out var value))
			{
				return value;
			}
			return null;
		}
	}

	public GameObject ThePlayer => XRLCore.Core?.Game?.Player?.Body;

	public string id
	{
		get
		{
			string text = GetStringProperty("id");
			if (text == null)
			{
				if (XRLCore.Core != null && XRLCore.Core.Game != null)
				{
					int intGameState = XRLCore.Core.Game.GetIntGameState("nextId");
					XRLCore.Core.Game.SetIntGameState("nextId", intGameState + 1);
					text = intGameState.ToString();
					SetStringProperty("id", text);
				}
				else
				{
					text = "[pre-game]";
					SetStringProperty("id", text);
				}
			}
			return text;
		}
		set
		{
			SetStringProperty("id", value);
		}
	}

	public bool hasid => HasStringProperty("id");

	public GameObject Target
	{
		get
		{
			if (IsPlayer())
			{
				return Sidebar.CurrentTarget;
			}
			if (pBrain == null)
			{
				return null;
			}
			return pBrain.Target;
		}
		set
		{
			if (IsPlayer())
			{
				Sidebar.CurrentTarget = value;
			}
			else if (pBrain != null)
			{
				pBrain.Target = value;
			}
		}
	}

	public GameObject Equipped => pPhysics?.Equipped;

	public GameObject InInventory => pPhysics?.InInventory;

	public GameObject Implantee => (GetPart("CyberneticsBaseItem") as CyberneticsBaseItem)?.ImplantedOn;

	public Cell CurrentCell
	{
		get
		{
			return pPhysics?.CurrentCell;
		}
		set
		{
			if (pPhysics != null)
			{
				pPhysics.CurrentCell = value;
			}
		}
	}

	public Zone CurrentZone => CurrentCell?.ParentZone;

	public GameObject PartyLeader
	{
		get
		{
			return pBrain?.PartyLeader;
		}
		set
		{
			if (pBrain != null)
			{
				pBrain.PartyLeader = value;
			}
		}
	}

	public string Factions => pBrain?.Factions;

	public Armor Armor => GetPart("Armor") as Armor;

	public Render pRender => _pRender;

	public XRL.World.Parts.Physics pPhysics => _pPhysics;

	public Brain pBrain => _pBrain;

	[field: NonSerialized]
	public Body Body { get; private set; }

	[field: NonSerialized]
	public LiquidVolume LiquidVolume { get; private set; }

	[field: NonSerialized]
	public Inventory Inventory { get; private set; }

	public Dictionary<string, int> IntProperty
	{
		get
		{
			if (_IntProperty == null)
			{
				_IntProperty = new Dictionary<string, int>();
			}
			return _IntProperty;
		}
		set
		{
			_IntProperty = value;
		}
	}

	public Dictionary<string, string> Property
	{
		get
		{
			if (_Property == null)
			{
				_Property = new Dictionary<string, string>();
			}
			return _Property;
		}
		set
		{
			_Property = value;
		}
	}

	public Statistic Energy
	{
		get
		{
			if (_Energy == null && Statistics != null)
			{
				Statistics.TryGetValue("Energy", out _Energy);
			}
			return _Energy;
		}
	}

	public int Speed
	{
		get
		{
			if (Statistics.TryGetValue("Speed", out var value))
			{
				return value.Value;
			}
			return 0;
		}
	}

	public List<Effect> Effects
	{
		get
		{
			if (_Effects == null)
			{
				_Effects = new List<Effect>();
			}
			return _Effects;
		}
	}

	public double ValueEach
	{
		get
		{
			double num = ((GetPart("Commerce") is Commerce commerce) ? commerce.Value : 0.01);
			if (WantEvent(GetIntrinsicValueEvent.ID, MinEvent.CascadeLevel))
			{
				GetIntrinsicValueEvent getIntrinsicValueEvent = GetIntrinsicValueEvent.FromPool(this, num);
				HandleEvent(getIntrinsicValueEvent);
				num = getIntrinsicValueEvent.Value;
			}
			if (WantEvent(AdjustValueEvent.ID, MinEvent.CascadeLevel))
			{
				AdjustValueEvent adjustValueEvent = AdjustValueEvent.FromPool(this, num);
				HandleEvent(adjustValueEvent);
				num = adjustValueEvent.Value;
			}
			if (WantEvent(GetExtrinsicValueEvent.ID, MinEvent.CascadeLevel))
			{
				GetExtrinsicValueEvent getExtrinsicValueEvent = GetExtrinsicValueEvent.FromPool(this, num);
				HandleEvent(getExtrinsicValueEvent);
				num = getExtrinsicValueEvent.Value;
			}
			return num;
		}
	}

	public double Value => ValueEach * (double)Count;

	public int Weight => pPhysics?.Weight ?? 0;

	public int WeightEach => pPhysics?.WeightEach ?? 0;

	public int IntrinsicWeight => pPhysics?.IntrinsicWeight ?? 0;

	public int Count
	{
		get
		{
			if (GetPart("Stacker") is Stacker stacker)
			{
				return stacker.Number;
			}
			return 1;
		}
		set
		{
			if (GetPart("Stacker") is Stacker stacker)
			{
				stacker.StackCount = value;
			}
		}
	}

	public int Level => GetStat("Level")?.Value ?? 1;

	public string Owner => pPhysics?.Owner;

	public string UsesSlots
	{
		get
		{
			return pPhysics?.UsesSlots;
		}
		set
		{
			if (pPhysics != null)
			{
				pPhysics.UsesSlots = value;
			}
		}
	}

	public string DebugName
	{
		get
		{
			string text = Blueprint + "(" + DisplayNameOnly + ")";
			if (IsPlayer())
			{
				text = "Player:" + text;
			}
			return text;
		}
	}

	private string DisplayNameBase => pRender?.DisplayName ?? Blueprint;

	public string DisplayName
	{
		get
		{
			return GetDisplayNameEvent.GetFor(this, DisplayNameBase);
		}
		set
		{
			if (pRender != null)
			{
				pRender.DisplayName = value;
			}
		}
	}

	/// <summary>
	///             The full object display name with colors removed.
	///             </summary>
	public string DisplayNameStripped
	{
		get
		{
			_CachedStrippedName = DisplayName.Strip();
			return _CachedStrippedName;
		}
	}

	public string DisplayNameOnly => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	/// <summary>
	///             The object display name without tags and with colors removed.
	///             </summary>
	public string DisplayNameOnlyStripped => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	public string DisplayNameOnlyDirect
	{
		get
		{
			if (pRender == null)
			{
				return "<unknown>";
			}
			return pRender.DisplayName;
		}
	}

	public string DisplayNameOnlyDirectAndStripped => DisplayNameOnlyDirect.Strip();

	/// <summary>
	///             The object display name without tags, suppressing any modification
	///             by the player's confusion state.
	///             </summary>
	public string DisplayNameOnlyUnconfused => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	public string DisplayNameSingle => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true);

	/// <summary>
	///             The object's display name without tags and with any information about
	///             multiple stacked items suppressed.
	///             </summary>
	public string DisplayNameOnlySingle => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	public string ShortDisplayName => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	/// <summary>
	///             The object's display name without tags and with any information about
	///             multiple stacked items suppressed.
	///             </summary>
	public string ShortDisplayNameSingle => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	public string ShortDisplayNameStripped => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	/// <summary>
	///             The object's display name without tags, with any information about
	///             multiple stacked items suppressed, and with colors removed.
	///             </summary>
	public string ShortDisplayNameSingleStripped => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);

	public string ShortDisplayNameWithoutEpithet => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: true);

	/// <summary>
	///             The object's display name without tags, with any name portion
	///             following a comma removed, and with colors removed.
	///             </summary>
	public string ShortDisplayNameWithoutEpithetStripped => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: true, Short: true);

	public string BaseDisplayName => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: true, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true);

	/// <summary>
	///             The object's display name, as if fully known, without adjectives,
	///             clauses, or tags; only alterations to the display name that are
	///             considered core to the object's identity are included.
	///             </summary>
	public string BaseKnownDisplayName => GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: true, NoColor: true, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true);

	public string BaseDisplayNameStripped => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: true, NoColor: true, Stripped: true, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: false, BaseOnly: true);

	/// <summary>
	///             The main color of the object's display name.
	///             </summary>
	public string DisplayNameColor => GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: true, Visible: true, WithoutEpithet: false, Short: true);

	public string Its
	{
		get
		{
			if (IsPlayer())
			{
				return "Your";
			}
			return GetPronounProvider().CapitalizedPossessiveAdjective;
		}
	}

	public string its
	{
		get
		{
			if (IsPlayer())
			{
				return "your";
			}
			return GetPronounProvider().PossessiveAdjective;
		}
	}

	public string It
	{
		get
		{
			if (IsPlayer())
			{
				return "You";
			}
			return GetPronounProvider().CapitalizedSubjective;
		}
	}

	public string it
	{
		get
		{
			if (IsPlayer())
			{
				return "you";
			}
			return GetPronounProvider().Subjective;
		}
	}

	public string Itself
	{
		get
		{
			if (IsPlayer())
			{
				if (!IsPlural)
				{
					return "Yourself";
				}
				return "Yourselves";
			}
			return GetPronounProvider().CapitalizedReflexive;
		}
	}

	public string itself
	{
		get
		{
			if (IsPlayer())
			{
				if (!IsPlural)
				{
					return "yourself";
				}
				return "yourselves";
			}
			return GetPronounProvider().Reflexive;
		}
	}

	public string Them
	{
		get
		{
			if (IsPlayer())
			{
				return "You";
			}
			return GetPronounProvider().CapitalizedObjective;
		}
	}

	public string them
	{
		get
		{
			if (IsPlayer())
			{
				return "you";
			}
			return GetPronounProvider().Objective;
		}
	}

	public string theirs
	{
		get
		{
			if (IsPlayer())
			{
				return "yours";
			}
			return GetPronounProvider().SubstantivePossessive;
		}
	}

	public string Theirs
	{
		get
		{
			if (IsPlayer())
			{
				return "Yours";
			}
			return GetPronounProvider().CapitalizedSubstantivePossessive;
		}
	}

	public string indicativeProximal
	{
		get
		{
			if (IsPlayer())
			{
				return "you";
			}
			return GetPronounProvider().IndicativeProximal;
		}
	}

	public string IndicativeProximal
	{
		get
		{
			if (IsPlayer())
			{
				return "you";
			}
			return GetPronounProvider().CapitalizedIndicativeProximal;
		}
	}

	public string indicativeDistal
	{
		get
		{
			if (IsPlayer())
			{
				return "you";
			}
			return GetPronounProvider().IndicativeDistal;
		}
	}

	public string IndicativeDistal
	{
		get
		{
			if (IsPlayer())
			{
				return "You";
			}
			return GetPronounProvider().CapitalizedIndicativeDistal;
		}
	}

	public bool UseBareIndicative => GetPronounProvider().UseBareIndicative;

	public string YouAre
	{
		get
		{
			if (IsPlayer())
			{
				return "You are";
			}
			IPronounProvider pronounProvider = GetPronounProvider();
			if (!pronounProvider.Plural && !pronounProvider.PseudoPlural)
			{
				return pronounProvider.CapitalizedSubjective + " is";
			}
			return pronounProvider.CapitalizedSubjective + " are";
		}
	}

	public string Itis
	{
		get
		{
			if (IsPlayer())
			{
				return "You're";
			}
			IPronounProvider pronounProvider = GetPronounProvider();
			if (!pronounProvider.Plural && !pronounProvider.PseudoPlural)
			{
				return pronounProvider.CapitalizedSubjective + "'s";
			}
			return pronounProvider.CapitalizedSubjective + "'re";
		}
	}

	public string itis
	{
		get
		{
			if (IsPlayer())
			{
				return "you're";
			}
			IPronounProvider pronounProvider = GetPronounProvider();
			if (!pronounProvider.Plural && !pronounProvider.PseudoPlural)
			{
				return pronounProvider.Subjective + "'s";
			}
			return pronounProvider.Subjective + "'re";
		}
	}

	public string Ithas
	{
		get
		{
			if (IsPlayer())
			{
				return "You've";
			}
			IPronounProvider pronounProvider = GetPronounProvider();
			if (!pronounProvider.Plural && !pronounProvider.PseudoPlural)
			{
				return pronounProvider.CapitalizedSubjective + "'s";
			}
			return pronounProvider.CapitalizedSubjective + "'ve";
		}
	}

	public string ithas
	{
		get
		{
			if (IsPlayer())
			{
				return "you've";
			}
			IPronounProvider pronounProvider = GetPronounProvider();
			if (!pronounProvider.Plural && !pronounProvider.PseudoPlural)
			{
				return pronounProvider.Subjective + "'s";
			}
			return pronounProvider.Subjective + "'ve";
		}
	}

	public string personTerm => GetPronounProvider().PersonTerm;

	public string PersonTerm => GetPronounProvider().CapitalizedPersonTerm;

	public string immaturePersonTerm => GetPronounProvider().ImmaturePersonTerm;

	public string ImmaturePersonTerm => GetPronounProvider().CapitalizedImmaturePersonTerm;

	public string formalAddressTerm => GetPronounProvider().FormalAddressTerm;

	public string FormalAddressTerm => GetPronounProvider().CapitalizedFormalAddressTerm;

	public string offspringTerm => GetPronounProvider().OffspringTerm;

	public string OffspringTerm => GetPronounProvider().CapitalizedOffspringTerm;

	public string siblingTerm => GetPronounProvider().SiblingTerm;

	public string SiblingTerm => GetPronounProvider().CapitalizedSiblingTerm;

	public string parentTerm => GetPronounProvider().ParentTerm;

	public string ParentTerm => GetPronounProvider().CapitalizedParentTerm;

	public string A => IndefiniteArticle(capital: true);

	public string a => IndefiniteArticle();

	public string AForBase => IndefiniteArticle(capital: true, null, forBase: true);

	public string aForBase => IndefiniteArticle(capital: false, null, forBase: true);

	public string The => DefiniteArticle(capital: true);

	public string the => DefiniteArticle();

	public string Is => GetVerb("are");

	public string Has => GetVerb("have");

	public bool HasProperName
	{
		get
		{
			if (!Understood())
			{
				return false;
			}
			int intProperty = GetIntProperty("ProperNoun");
			if (intProperty > 0)
			{
				return true;
			}
			if (intProperty < 0)
			{
				return false;
			}
			return GetxTag("Grammar", "Proper") == "true";
		}
		set
		{
			SetIntProperty("ProperNoun", value ? 1 : (-1));
		}
	}

	public bool IsPlural => GetPronounProvider().Plural;

	public bool IsPseudoPlural => GetPronounProvider().PseudoPlural;

	public bool IsPluralIfKnown => GetPronounProvider(AsIfKnown: true).Plural;

	public bool IsPseudoPluralIfKnown => GetPronounProvider(AsIfKnown: true).PseudoPlural;

	public string OriginalColorString
	{
		get
		{
			return GetStringProperty("OriginalColorString");
		}
		set
		{
			SetStringProperty("OriginalColorString", value);
		}
	}

	public string OriginalDetailColor
	{
		get
		{
			return GetStringProperty("OriginalDetailColor");
		}
		set
		{
			SetStringProperty("OriginalDetailColor", value);
		}
	}

	public string OriginalTileColor
	{
		get
		{
			return GetStringProperty("OriginalTileColor");
		}
		set
		{
			SetStringProperty("OriginalTileColor", value);
		}
	}

	public int baseHitpoints
	{
		get
		{
			if (Statistics != null && Statistics.TryGetValue("Hitpoints", out var value))
			{
				return value.BaseValue;
			}
			return 0;
		}
	}

	public int hitpoints
	{
		get
		{
			if (Statistics != null && Statistics.TryGetValue("Hitpoints", out var value))
			{
				return value.Value;
			}
			return 0;
		}
		set
		{
			if (Statistics != null && Statistics.TryGetValue("Hitpoints", out var value2))
			{
				int num = value - value2.Value;
				if (num != 0)
				{
					value2.Penalty -= num;
				}
			}
		}
	}

	public bool IsImplant => HasPart("CyberneticsBaseItem");

	public bool juiceEnabled => Options.UseOverlayCombatEffects;

	public ActivatedAbilities ActivatedAbilities => GetPart("ActivatedAbilities") as ActivatedAbilities;

	public bool IsDying => Dying;

	public bool UsesTwoSlots
	{
		get
		{
			return pPhysics?.UsesTwoSlots ?? false;
		}
		set
		{
			if (pPhysics != null)
			{
				pPhysics.UsesTwoSlots = value;
			}
		}
	}

	public bool IsCreature => HasPropertyOrTag("Creature");

	public bool IsTrifling
	{
		get
		{
			return HasIntProperty("trifling");
		}
		set
		{
			if (value)
			{
				SetIntProperty("trifling", 1);
			}
			else
			{
				RemoveIntProperty("trifling");
			}
		}
	}

	public bool Respires => RespiresEvent.Check(this);

	public bool IsHidden
	{
		get
		{
			if (!(GetPart("Hidden") is Hidden hidden))
			{
				return false;
			}
			return !hidden.Found;
		}
	}

	public bool IsAlive
	{
		get
		{
			if (!IsCreature && !HasTagOrProperty("LivePlant") && !HasTagOrProperty("LiveFungus") && !HasTagOrProperty("LiveAnimal"))
			{
				return false;
			}
			if (GetIntProperty("Inorganic") > 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsBridge => HasTagOrProperty("Bridge");

	public bool IsTemporary => XRL.World.Parts.Temporary.IsTemporary(this);

	public bool IsBurrower
	{
		get
		{
			string stringProperty = GetStringProperty("Burrowing");
			if (!string.IsNullOrEmpty(stringProperty))
			{
				return stringProperty.EqualsNoCase("true");
			}
			if (!HasPart("Digging"))
			{
				return HasTag("Burrowing");
			}
			return true;
		}
	}

	public bool IsFlying
	{
		get
		{
			if (HasEffect("Flying"))
			{
				return GetIntProperty("SuspendFlight") <= 0;
			}
			return false;
		}
	}

	public bool IsThrownWeapon
	{
		get
		{
			if (HasPart("ThrownWeapon"))
			{
				return true;
			}
			if (HasPart("GeomagneticDisc"))
			{
				return true;
			}
			return false;
		}
	}

	public bool OwnedByPlayer
	{
		get
		{
			if (GetIntProperty("StoredByPlayer") <= 0)
			{
				return GetIntProperty("FromStoredByPlayer") > 0;
			}
			return true;
		}
	}

	public GameObjectReference takeReference()
	{
		return new GameObjectReference(this);
	}

	public GameObject Split(int n)
	{
		SplitStack(n);
		return this;
	}

	public GameObject SplitFromStack()
	{
		SplitStack(1);
		return this;
	}

	public GameObject RemoveOne()
	{
		if (GetPart("Stacker") is Stacker stacker)
		{
			return stacker.RemoveOne();
		}
		return this;
	}

	public static GameObject create(string blueprint)
	{
		return GameObjectFactory.Factory.CreateObject(blueprint);
	}

	public static GameObject create(string blueprint, int BonusModChance = 0, int SetModNumber = 0, string Context = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		return GameObjectFactory.Factory.CreateObject(blueprint, BonusModChance, SetModNumber, beforeObjectCreated, afterObjectCreated, Context);
	}

	public static GameObject createSample(string blueprint)
	{
		return GameObjectFactory.Factory.CreateSampleObject(blueprint);
	}

	public static GameObject createUnmodified(string blueprint)
	{
		return GameObjectFactory.Factory.CreateObject(blueprint, -9999);
	}

	public static GameObject createUnmodified(string blueprint, string Context = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		return GameObjectFactory.Factory.CreateObject(blueprint, -9999, 0, beforeObjectCreated, afterObjectCreated, Context);
	}

	public static bool validate(ref GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.IsInvalid())
		{
			obj = null;
			return false;
		}
		return true;
	}

	public static bool validate(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.IsInvalid())
		{
			return false;
		}
		return true;
	}

	public bool InACell()
	{
		return pPhysics?.CurrentCell?.ParentZone != null;
	}

	public bool OnWorldMap()
	{
		return CurrentCell?.OnWorldMap() ?? false;
	}

	public bool InZone(Zone Z)
	{
		if (Z == null)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		return currentCell.ParentZone == Z;
	}

	public bool InZone(string Z)
	{
		if (Z == null)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (currentCell.ParentZone == null)
		{
			return false;
		}
		return currentCell.ParentZone.ZoneID == Z;
	}

	public bool InSameZone(Cell C)
	{
		return InZone(C?.ParentZone);
	}

	public bool InSameZone(GameObject GO)
	{
		return InZone(GO?.CurrentZone);
	}

	public void MeleeAttackWithWeapon(GameObject Target, GameObject Weapon, bool autohit = false, bool autopen = false)
	{
		Event @event = Event.New("MeleeAttackWithWeapon");
		@event.AddParameter("Attacker", this);
		@event.AddParameter("Defender", Target);
		@event.AddParameter("Weapon", Weapon);
		if (autohit)
		{
			@event.SetParameter("Properties", @event.GetStringParameter("Properties", "") + "Autohit");
		}
		if (autopen)
		{
			@event.SetParameter("Properties", @event.GetStringParameter("Properties", "") + "Autopen");
		}
		FireEvent(@event);
	}

	public bool CellTeleport(Cell C, Event FromEvent = null, GameObject Device = null, GameObject DeviceOperator = null, IPart Mutation = null, string SuccessMessage = null, int? EnergyCost = 0, bool Forced = false, bool VisualEffects = true, bool ReducedVisualEffects = false, bool SkipRealityDistortion = false, string LeaveVerb = "disappear", string ArriveVerb = "appear")
	{
		if (C == null)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		Zone parentZone = currentCell.ParentZone;
		_ = XRLCore.Core.Game.ZoneManager;
		Zone parentZone2 = C.ParentZone;
		if (parentZone2 == null)
		{
			return false;
		}
		bool flag = SkipRealityDistortion;
		if (!flag)
		{
			Event @event = Event.New("InitiateRealityDistortionTransit");
			@event.SetParameter("Object", this);
			@event.SetParameter("Cell", C);
			if (Device != null)
			{
				@event.SetParameter("Device", Device);
			}
			if (DeviceOperator != null)
			{
				@event.SetParameter("Operator", DeviceOperator);
			}
			if (Mutation != null)
			{
				@event.SetParameter("Mutation", Mutation);
			}
			flag = FireEvent(@event, FromEvent) && C.FireEvent(@event, FromEvent);
		}
		if (flag)
		{
			if (!string.IsNullOrEmpty(SuccessMessage) && IsPlayer())
			{
				Popup.Show(SuccessMessage);
			}
			if (TeleportTo(C, EnergyCost, ignoreCombat: true, ignoreGravity: false, Forced, LeaveVerb, ArriveVerb))
			{
				if (VisualEffects && !IsPlayer() && ThePlayer != null && ThePlayer.InZone(parentZone))
				{
					if (ReducedVisualEffects)
					{
						SmallTeleportSwirl(currentCell);
					}
					else
					{
						TeleportSwirl(currentCell);
					}
				}
				if (VisualEffects && (IsPlayer() || (ThePlayer != null && ThePlayer.InZone(parentZone2))))
				{
					if (parentZone2.ZoneWorld == parentZone.ZoneWorld || !IsPlayer())
					{
						if (ReducedVisualEffects)
						{
							SmallTeleportSwirl();
						}
						else
						{
							TeleportSwirl();
						}
					}
					else
					{
						GameManager.Instance.Spacefolding = true;
						pPhysics?.PlayWorldSound("teleport_world", 1f);
					}
				}
			}
		}
		return flag;
	}

	public bool ZoneTeleport(string Zone, int X = -1, int Y = -1, IEvent FromEvent = null, GameObject Device = null, GameObject DeviceOperator = null, IPart Mutation = null, string SuccessMessage = "You are transported!", bool VisualEffects = true)
	{
		Cell currentCell = CurrentCell;
		ZoneManager zoneManager = XRLCore.Core.Game.ZoneManager;
		Zone parentZone = currentCell.ParentZone;
		Zone zone = zoneManager.GetZone(Zone);
		Cell cell = zone.GetCell(X, Y);
		Event e = Event.New("CheckRealityDistortionAccessibility");
		if (X == -1 || Y == -1 || cell == null || !cell.FireEvent(e))
		{
			try
			{
				List<Cell> emptyReachableCells = zone.GetEmptyReachableCells(e);
				cell = ((emptyReachableCells.Count <= 0) ? zone.GetCell(40, 20) : emptyReachableCells.GetRandomElement());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				cell = zone.GetCell(40, 20);
			}
		}
		Event @event = Event.New("InitiateRealityDistortionTransit");
		@event.SetParameter("Object", this);
		@event.SetParameter("Cell", cell);
		if (Device != null)
		{
			@event.SetParameter("Device", Device);
		}
		if (DeviceOperator != null)
		{
			@event.SetParameter("Operator", DeviceOperator);
		}
		if (Mutation != null)
		{
			@event.SetParameter("Mutation", Mutation);
		}
		bool flag = FireEvent(@event, FromEvent) && cell.FireEvent(@event, FromEvent);
		if (flag)
		{
			if (!string.IsNullOrEmpty(SuccessMessage) && IsPlayer())
			{
				Popup.Show(SuccessMessage);
			}
			if (TeleportTo(cell, 0))
			{
				if (VisualEffects && !IsPlayer() && ThePlayer != null && ThePlayer.InZone(parentZone))
				{
					TeleportSwirl(currentCell);
				}
				if (VisualEffects && (IsPlayer() || (ThePlayer != null && ThePlayer.InZone(zone))))
				{
					if (zone.ZoneWorld == parentZone.ZoneWorld || !IsPlayer())
					{
						TeleportSwirl();
					}
					else
					{
						GameManager.Instance.Spacefolding = true;
						pPhysics?.PlayWorldSound("teleport_world", 1f);
					}
				}
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	public void StopMoving()
	{
		if (IsPlayer())
		{
			AutoAct.Interrupt();
		}
		if (pBrain != null)
		{
			pBrain.StopMoving();
		}
	}

	public Cell GetRandomTeleportTargetCell(int MaxDistance = 0)
	{
		Cell CC = CurrentCell;
		if (CC == null)
		{
			return null;
		}
		Zone parentZone = CC.ParentZone;
		if (parentZone == null)
		{
			return null;
		}
		Event e = Event.New("CheckRealityDistortionAccessibility");
		List<Cell> list = (IsPlayer() ? parentZone.GetEmptyReachableCells(e) : parentZone.GetEmptyCells(e));
		if (MaxDistance > 0)
		{
			list = list.Where((Cell EC) => EC.PathDistanceTo(CC) <= MaxDistance).ToList();
		}
		if (list.Contains(CC))
		{
			list.Remove(CC);
		}
		return list.GetRandomElement();
	}

	public bool RandomTeleport(bool Swirl = false, IPart Mutation = null, GameObject Device = null, GameObject DeviceOperator = null, Event E = null, int EnergyCost = 0, int MaxDistance = 0, bool InterruptMovement = true, Cell TargetCell = null, bool Forced = false, bool IgnoreCombat = true)
	{
		if (OnWorldMap())
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (TargetCell == null)
		{
			TargetCell = GetRandomTeleportTargetCell(MaxDistance);
			if (TargetCell == null)
			{
				return false;
			}
		}
		Event @event = Event.New("InitiateRealityDistortionTransit");
		@event.SetParameter("Object", this);
		@event.SetParameter("Cell", TargetCell);
		if (Mutation != null)
		{
			@event.SetParameter("Mutation", Mutation);
			if (Mutation.ParentObject != null && Mutation.ParentObject != this && !Mutation.ParentObject.FireEvent(@event, E))
			{
				return false;
			}
		}
		if (Device != null)
		{
			@event.SetParameter("Device", Device);
			if (DeviceOperator != null)
			{
				@event.SetParameter("Operator", DeviceOperator);
			}
			if (Device != this && !Device.FireEvent(@event, E))
			{
				return false;
			}
			if (DeviceOperator != this && DeviceOperator != Device && !DeviceOperator.FireEvent(@event, E))
			{
				return false;
			}
		}
		if (!FireEvent(@event, E) || !TargetCell.FireEvent(@event, E))
		{
			return false;
		}
		ParticleBlip("&C\u000f");
		Cell targetCell = TargetCell;
		int? energyCost = EnergyCost;
		bool forced = Forced;
		if (!TeleportTo(targetCell, energyCost, IgnoreCombat, ignoreGravity: false, forced))
		{
			return false;
		}
		if (Swirl && currentCell != null && currentCell.ParentZone.IsActive())
		{
			if (currentCell.PathDistanceTo(TargetCell) > 5)
			{
				TeleportSwirl();
			}
			else
			{
				SmallTeleportSwirl();
			}
		}
		ParticleBlip("&C\u000f");
		if (InterruptMovement)
		{
			StopMoving();
		}
		if (IsPlayer())
		{
			if (currentCell.X > 42 && Sidebar.State == "right")
			{
				Sidebar.SetSidebarState("left");
			}
			if (currentCell.X < 38 && Sidebar.State == "left")
			{
				Sidebar.SetSidebarState("right");
			}
		}
		return true;
	}

	public void TeleportSwirl(Cell C = null, string color = "&C", string sound = "teleport_long", char c = 'ù')
	{
		if (C == null)
		{
			C = CurrentCell;
		}
		if (C != null && C.ParentZone.IsActive())
		{
			pPhysics?.PlayWorldSound(sound, 1f);
			for (int i = 0; i < 30; i++)
			{
				XRLCore.ParticleManager.AddRadial(color + c, C.X, C.Y, XRL.Rules.Stat.Random(0, 7), XRL.Rules.Stat.Random(5, 10), 0.01f * (float)XRL.Rules.Stat.Random(4, 6), -0.05f * (float)XRL.Rules.Stat.Random(3, 7));
			}
		}
	}

	public void SmallTeleportSwirl(Cell C = null, string color = "&C", string sound = "teleport_short")
	{
		if (C == null)
		{
			C = CurrentCell;
		}
		if (C != null && C.ParentZone.IsActive())
		{
			pPhysics?.PlayWorldSound(sound, 1f);
			for (int i = 0; i < 10; i++)
			{
				XRLCore.ParticleManager.AddRadial(color + "ù", C.X, C.Y, XRL.Rules.Stat.Random(0, 5), XRL.Rules.Stat.Random(4, 8), 0.01f * (float)XRL.Rules.Stat.Random(3, 5), -0.05f * (float)XRL.Rules.Stat.Random(2, 6));
			}
		}
	}

	public void SpatialDistortionBlip(Cell C = null, string color = "&C")
	{
		if (C == null)
		{
			C = CurrentCell;
		}
		if (C != null && C.ParentZone.IsActive())
		{
			XRLCore.ParticleManager.AddRadial(color + "ù", C.X, C.Y, XRL.Rules.Stat.Random(0, 5), XRL.Rules.Stat.Random(4, 8), 0.01f * (float)XRL.Rules.Stat.Random(3, 5), -0.05f * (float)XRL.Rules.Stat.Random(2, 6));
		}
	}

	public void TechTeleportSwirlOut(Cell C = null, string color = "&B", string sound = null)
	{
		if (C == null)
		{
			C = CurrentCell;
		}
		if (C != null && C.ParentZone.IsActive())
		{
			pPhysics?.PlayWorldSound(sound, 1f);
			for (int i = 0; i < 15; i++)
			{
				XRLCore.ParticleManager.AddRadial(color + "ù", C.X, C.Y, XRL.Rules.Stat.Random(0, 7), XRL.Rules.Stat.Random(0, 2), 0.01f * (float)XRL.Rules.Stat.Random(4, 6), 0.05f * (float)XRL.Rules.Stat.Random(3, 7), 30);
			}
		}
	}

	public void TechTeleportSwirlIn(Cell C = null, string color = "&C", string sound = "teleport_short")
	{
		if (C == null)
		{
			C = CurrentCell;
		}
		if (C != null && C.ParentZone.IsActive())
		{
			pPhysics?.PlayWorldSound(sound, 1f);
			for (int i = 0; i < 15; i++)
			{
				XRLCore.ParticleManager.AddRadial(color + "ù", C.X, C.Y, XRL.Rules.Stat.Random(0, 7), XRL.Rules.Stat.Random(5, 10), 0.01f * (float)XRL.Rules.Stat.Random(4, 6), -0.05f * (float)XRL.Rules.Stat.Random(3, 7), 30);
			}
		}
	}

	public bool DirectMoveTo(GlobalLocation targetLocation, int energyCost = 0, bool forced = false, bool ignoreCombat = false, bool ignoreGravity = false)
	{
		if (targetLocation != null)
		{
			Zone zone = ZoneManager.instance.GetZone(targetLocation.ZoneID);
			return DirectMoveTo(zone.GetCell(targetLocation.CellX, targetLocation.CellY));
		}
		return false;
	}

	public bool DirectMoveTo(Cell targetCell, int energyCost = 0, bool forced = false, bool ignoreCombat = false, bool ignoreGravity = false)
	{
		if (pPhysics == null)
		{
			return false;
		}
		return pPhysics.ProcessTargetedMove(targetCell, "DirectMove", "BeforeDirectMove", "AfterDirectMove", energyCost, forced, System: false, ignoreCombat, ignoreGravity);
	}

	public bool SystemLongDistanceMoveTo(Cell targetCell, int? energyCost = null, bool forced = false, bool ignoreCombat = true)
	{
		if (pPhysics == null)
		{
			return false;
		}
		return pPhysics.ProcessTargetedMove(targetCell, "SystemLongDistanceMove", "BeforeSystemLongDistanceMove", "AfterSystemLongDistanceMove", energyCost, forced, System: true, ignoreCombat);
	}

	public bool SystemMoveTo(Cell targetCell, int? energyCost = null, bool forced = false, bool ignoreCombat = true, bool ignoreGravity = false, bool noStack = false)
	{
		if (pPhysics == null)
		{
			return false;
		}
		return pPhysics.ProcessTargetedMove(targetCell, "SystemMove", "BeforeSystemMove", "AfterSystemMove", energyCost, forced, System: true, ignoreCombat, ignoreGravity, noStack);
	}

	public bool TeleportTo(Cell targetCell, int? energyCost = 0, bool ignoreCombat = true, bool ignoreGravity = false, bool forced = false, string leaveVerb = "disappear", string arriveVerb = "appear")
	{
		if (pPhysics == null)
		{
			return false;
		}
		return pPhysics.ProcessTargetedMove(targetCell, "Teleporting", "BeforeTeleport", "AfterTeleport", energyCost, forced, System: true, ignoreCombat, ignoreGravity, NoStack: false, leaveVerb, arriveVerb);
	}

	public void PerformMeleeAttack(GameObject target)
	{
		Event @event = Event.New("PerformMeleeAttack");
		@event.SetParameter("Attacker", this);
		@event.SetParameter("TargetCell", CurrentCell);
		@event.SetParameter("Defender", target);
		@event.SetParameter("Properties", null);
		FireEvent(@event);
	}

	public Cell FastGetCurrentCell()
	{
		if (pPhysics == null)
		{
			return null;
		}
		if (pPhysics.CurrentCell != null)
		{
			return pPhysics.CurrentCell;
		}
		if (pPhysics.Equipped != null)
		{
			return pPhysics.Equipped.FastGetCurrentCell();
		}
		if (pPhysics.InInventory != null)
		{
			return pPhysics.InInventory.FastGetCurrentCell();
		}
		return null;
	}

	public void SyncMutationLevelAndGlimmer()
	{
		FireEvent("SyncMutationLevels");
		GlimmerChangeEvent.Send(this);
	}

	public Cell GetCurrentCell()
	{
		Cell currentCell = CurrentCell;
		if (currentCell != null)
		{
			return currentCell;
		}
		GetContextEvent.Get(this, out var ObjectContext, out var CellContext);
		if (CellContext != null)
		{
			return CellContext;
		}
		return ObjectContext?.GetCurrentCell();
	}

	public bool Contains(GameObject obj)
	{
		return ContainsEvent.Check(this, obj);
	}

	public bool ContainsBlueprint(string blueprint)
	{
		return ContainsBlueprintEvent.Check(this, blueprint);
	}

	public bool ContainsAnyBlueprint(List<string> blueprints)
	{
		return ContainsAnyBlueprintEvent.Check(this, blueprints);
	}

	public GameObject FindContainedObjectByBlueprint(string blueprint)
	{
		return ContainsBlueprintEvent.Find(this, blueprint);
	}

	public GameObject FindContainedObjectByAnyBlueprint(List<string> blueprints)
	{
		return ContainsAnyBlueprintEvent.Find(this, blueprints);
	}

	public bool HasTag(string TagName)
	{
		if (TagName == null)
		{
			return false;
		}
		if (GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
		{
			return value.HasTag(TagName);
		}
		return false;
	}

	public bool IsInGraveyard()
	{
		return CurrentCell?.IsGraveyard() ?? false;
	}

	public bool HasHitpoints()
	{
		return Stat("Hitpoints") > 0;
	}

	public bool IsValid()
	{
		return pPhysics != null;
	}

	public bool IsInvalid()
	{
		return !IsValid();
	}

	public bool HasContext()
	{
		return GetContextEvent.HasAny(this);
	}

	public void GetContext(out GameObject ObjectContext, out Cell CellContext, out int Relation, out IPart RelationManager)
	{
		GetContextEvent.Get(this, out ObjectContext, out CellContext, out Relation, out RelationManager);
	}

	public void GetContext(out GameObject ObjectContext, out Cell CellContext, out int Relation)
	{
		GetContextEvent.Get(this, out ObjectContext, out CellContext, out Relation);
	}

	public void GetContext(out GameObject ObjectContext, out Cell CellContext)
	{
		GetContextEvent.Get(this, out ObjectContext, out CellContext);
	}

	public GameObject GetObjectContext()
	{
		GetContext(out var ObjectContext, out var _);
		return ObjectContext;
	}

	public GameObject GetObjectContext(out int Relation)
	{
		GetContext(out var ObjectContext, out var _, out Relation);
		return ObjectContext;
	}

	public GameObject GetObjectContext(out int Relation, out IPart RelationManager)
	{
		GetContext(out var ObjectContext, out var _, out Relation, out RelationManager);
		return ObjectContext;
	}

	public Cell GetCellContext()
	{
		GetContext(out var _, out var CellContext);
		return CellContext;
	}

	public Cell GetCellContext(out int Relation)
	{
		GetContext(out var _, out var CellContext, out Relation);
		return CellContext;
	}

	public Cell GetCellContext(out int Relation, out IPart RelationManager)
	{
		GetContext(out var _, out var CellContext, out Relation, out RelationManager);
		return CellContext;
	}

	public int GetMatterPhase()
	{
		return MatterPhase.getMatterPhase(this);
	}

	public bool IsNowhere()
	{
		Cell currentCell = CurrentCell;
		if (currentCell != null)
		{
			return currentCell.IsGraveyard();
		}
		GetContext(out var ObjectContext, out var CellContext);
		return ObjectContext?.IsNowhere() ?? CellContext?.IsGraveyard() ?? true;
	}

	public bool IsOwned()
	{
		return !string.IsNullOrEmpty(pPhysics?.Owner);
	}

	public void RemoveFromContext(IEvent ParentEvent = null)
	{
		RemoveFromContextEvent.Send(this, ParentEvent);
	}

	public bool TryRemoveFromContext(IEvent ParentEvent = null)
	{
		return TryRemoveFromContextEvent.Check(this, ParentEvent = null);
	}

	public bool idmatch(string testID)
	{
		if (testID == null)
		{
			return false;
		}
		string stringProperty = GetStringProperty("id");
		if (stringProperty != null)
		{
			return testID == stringProperty;
		}
		return false;
	}

	public bool idmatch(GameObject obj)
	{
		string stringProperty = GetStringProperty("id");
		if (stringProperty != null)
		{
			return obj.idmatch(stringProperty);
		}
		return false;
	}

	public void injectId(string id)
	{
		SetStringProperty("id", id);
	}

	public static GameObject findById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		XRLGame xRLGame = null;
		if (XRLCore.Core != null)
		{
			xRLGame = XRLCore.Core.Game;
		}
		if (xRLGame == null)
		{
			return null;
		}
		if (id == xRLGame.lastFindId && validate(ref xRLGame.lastFind) && xRLGame.lastFind.idmatch(xRLGame.lastFindId))
		{
			return xRLGame.lastFind;
		}
		Zone activeZone = xRLGame.ZoneManager.ActiveZone;
		if (activeZone != null)
		{
			GameObject gameObject = activeZone.findObjectById(id);
			if (gameObject != null)
			{
				xRLGame.lastFindId = id;
				xRLGame.lastFind = gameObject;
				return gameObject;
			}
		}
		foreach (KeyValuePair<string, Zone> cachedZone in xRLGame.ZoneManager.CachedZones)
		{
			if (cachedZone.Value != activeZone)
			{
				GameObject gameObject2 = cachedZone.Value.findObjectById(id);
				if (gameObject2 != null)
				{
					xRLGame.lastFindId = id;
					xRLGame.lastFind = gameObject2;
					return gameObject2;
				}
			}
		}
		return null;
	}

	public static GameObject findByBlueprint(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		XRLGame xRLGame = null;
		if (XRLCore.Core != null)
		{
			xRLGame = XRLCore.Core.Game;
		}
		if (xRLGame == null)
		{
			return null;
		}
		Zone activeZone = xRLGame.ZoneManager.ActiveZone;
		if (activeZone != null)
		{
			GameObject gameObject = activeZone.FindFirstObject(name);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		foreach (KeyValuePair<string, Zone> cachedZone in xRLGame.ZoneManager.CachedZones)
		{
			if (cachedZone.Value != activeZone)
			{
				GameObject gameObject2 = cachedZone.Value.FindFirstObject(name);
				if (gameObject2 != null)
				{
					return gameObject2;
				}
			}
		}
		return null;
	}

	public static GameObject find(Predicate<GameObject> filter)
	{
		if (filter == null)
		{
			return null;
		}
		XRLGame xRLGame = null;
		if (XRLCore.Core != null)
		{
			xRLGame = XRLCore.Core.Game;
		}
		if (xRLGame == null)
		{
			return null;
		}
		Zone activeZone = xRLGame.ZoneManager.ActiveZone;
		if (activeZone != null)
		{
			GameObject gameObject = activeZone.FindFirstObject(filter);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		foreach (KeyValuePair<string, Zone> cachedZone in xRLGame.ZoneManager.CachedZones)
		{
			if (cachedZone.Value != activeZone)
			{
				GameObject gameObject2 = cachedZone.Value.FindFirstObject(filter);
				if (gameObject2 != null)
				{
					return gameObject2;
				}
			}
		}
		return null;
	}

	public static List<GameObject> findAll(Predicate<GameObject> filter)
	{
		List<GameObject> list = Event.NewGameObjectList();
		if (filter == null)
		{
			return list;
		}
		XRLGame xRLGame = null;
		if (XRLCore.Core != null)
		{
			xRLGame = XRLCore.Core.Game;
		}
		if (xRLGame == null)
		{
			return list;
		}
		Zone activeZone = xRLGame.ZoneManager.ActiveZone;
		activeZone?.FindObjects(list, filter);
		foreach (KeyValuePair<string, Zone> cachedZone in xRLGame.ZoneManager.CachedZones)
		{
			if (cachedZone.Value != activeZone)
			{
				cachedZone.Value.FindObjects(list, filter);
			}
		}
		return list;
	}

	public int? Con(GameObject who = null, bool IgnoreHideCon = false)
	{
		return DifficultyEvaluation.GetDifficultyRating(this, who);
	}

	public string GetDirectionToward(Location2D L, bool General = false)
	{
		if (L == null)
		{
			return "?";
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return "?";
		}
		if (!General)
		{
			return currentCell.GetDirectionFrom(L);
		}
		return currentCell.GetGeneralDirectionFrom(L);
	}

	public string GetDirectionToward(Cell C, bool General = false)
	{
		if (C == null)
		{
			return "?";
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return "?";
		}
		if (!General)
		{
			return currentCell.GetDirectionFromCell(C);
		}
		return currentCell.GetGeneralDirectionFromCell(C);
	}

	public string GetDirectionToward(GameObject obj, bool General = false)
	{
		return GetDirectionToward(obj.GetCurrentCell(), General);
	}

	public string DescribeDirectionToward(Location2D L, bool General = false, bool Short = false)
	{
		string directionToward = GetDirectionToward(L, General);
		if (Short)
		{
			return Directions.GetDirectionShortDescription(directionToward);
		}
		return Directions.GetDirectionDescription(directionToward);
	}

	public string DescribeDirectionToward(Cell C, bool General = false, bool Short = false)
	{
		string directionToward = GetDirectionToward(C, General);
		if (Short)
		{
			return Directions.GetDirectionShortDescription(directionToward);
		}
		return Directions.GetDirectionDescription(directionToward);
	}

	public string DescribeRelativeDirectionToward(Location2D L, bool General = false)
	{
		return Directions.GetDirectionDescription(this, GetDirectionToward(L, General));
	}

	public string DescribeRelativeDirectionToward(Cell C, bool General = false)
	{
		return Directions.GetDirectionDescription(this, GetDirectionToward(C, General));
	}

	public string DescribeDirectionToward(GameObject obj, bool General = false, bool Short = false)
	{
		return DescribeDirectionToward(obj.GetCurrentCell(), General, Short);
	}

	public string DescribeRelativeDirectionToward(GameObject obj, bool General = false)
	{
		return Directions.GetDirectionDescription(this, GetDirectionToward(obj, General));
	}

	public string DescribeDirectionFrom(Location2D L, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(GetDirectionToward(L, General));
	}

	public string DescribeDirectionFrom(Cell C, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(GetDirectionToward(C, General));
	}

	public string DescribeRelativeDirectionFrom(Location2D L, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(this, GetDirectionToward(L, General));
	}

	public string DescribeRelativeDirectionFrom(Cell C, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(this, GetDirectionToward(C, General));
	}

	public string DescribeDirectionFrom(GameObject obj, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(GetDirectionToward(obj, General));
	}

	public string DescribeRelativeDirectionFrom(GameObject obj, bool General = false)
	{
		return Directions.GetIncomingDirectionDescription(this, GetDirectionToward(obj, General));
	}

	public int Heal(int Amount, bool Message = false, bool FloatText = false, bool RandomMinimum = false)
	{
		int num = 0;
		eHealing.SetParameter("Amount", Amount);
		if (FireEvent(eHealing))
		{
			Statistic stat = GetStat("Hitpoints");
			if (stat != null)
			{
				int num2 = eHealing.GetIntParameter("Amount");
				if (num2 <= 0 && RandomMinimum && XRL.Rules.Stat.Random(1, 1 + Amount) > 1)
				{
					num2 = 1;
				}
				if (num2 > 0)
				{
					int value = stat.Value;
					stat.Penalty -= num2;
					int value2 = stat.Value;
					if (value != value2)
					{
						num = value2 - value;
						if (Message || FloatText)
						{
							if (num > 0)
							{
								char color = ColorCoding.ConsequentialColorChar(this);
								if (Message)
								{
									if (IsPlayer())
									{
										MessageQueue.AddPlayerMessage("You heal for " + num + " hit " + ((num == 1) ? "point" : "points") + ".", color);
									}
									else if (IsVisible())
									{
										MessageQueue.AddPlayerMessage(T() + GetVerb("heal") + " for " + num + " hit " + ((Amount == 1) ? "point" : "points") + ".", color);
									}
								}
								if (FloatText)
								{
									ParticleText("+" + num, color, IgnoreVisibility: false, 1.5f, 24f);
								}
							}
							else
							{
								char color2 = ColorCoding.ConsequentialColorChar(null, this);
								if (Message)
								{
									if (IsPlayer())
									{
										MessageQueue.AddPlayerMessage("You lose " + num + " hit " + ((num == 1) ? "point" : "points") + ".", color2);
									}
									else if (IsVisible())
									{
										MessageQueue.AddPlayerMessage(T() + GetVerb("lose") + num + " hit " + ((num == 1) ? "point" : "points") + ".", color2);
									}
								}
								if (FloatText)
								{
									ParticleText(num.ToString(), color2, IgnoreVisibility: false, 1.5f, 24f);
								}
							}
						}
						UpdateVisibleStatusColor();
					}
				}
			}
		}
		return num;
	}

	public double Health()
	{
		if (!Statistics.TryGetValue("Hitpoints", out var value))
		{
			return 1.0;
		}
		int value2 = value.Value;
		int baseValue = value.BaseValue;
		return (double)value2 / (double)baseValue;
	}

	public bool GoToPartyLeader()
	{
		return pBrain?.GoToPartyLeader() ?? false;
	}

	public string GetPrimaryFaction()
	{
		return pBrain?.GetPrimaryFaction();
	}

	public bool BelongsToFaction(string Faction)
	{
		if (string.IsNullOrEmpty(Faction))
		{
			return false;
		}
		if (pBrain == null)
		{
			return false;
		}
		if (pBrain.GetPrimaryFaction() == Faction)
		{
			return true;
		}
		return false;
	}

	public bool HasGoal(string GoalName)
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.HasGoal(GoalName);
	}

	public bool HasGoal()
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.HasGoal();
	}

	public bool HasGoalOtherThan(string what)
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.HasGoalOtherThan(what);
	}

	public bool IsBusy()
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.IsBusy();
	}

	public GameObject GetHostilityTarget()
	{
		GameObject target = Target;
		if (target == null || !IsHostileTowards(target))
		{
			return null;
		}
		return target;
	}

	public void MakeActive(bool Force = false)
	{
		if (Force || pBrain != null)
		{
			XRLCore.Core?.Game?.ActionManager?.AddActiveObject(this);
		}
	}

	public void MakeInactive()
	{
		XRLCore.Core.Game.ActionManager.RemoveActiveObject(this);
	}

	public void BecomeCompanionOf(GameObject who, bool trifling = false)
	{
		if (pBrain != null)
		{
			pBrain.BecomeCompanionOf(who, trifling);
		}
	}

	public void SetFeeling(GameObject GO, int Feeling)
	{
		if (pBrain != null)
		{
			pBrain.SetFeeling(GO, Feeling);
		}
	}

	public bool IsUnderSky()
	{
		Zone currentZone = CurrentZone;
		if (currentZone == null)
		{
			return false;
		}
		if (currentZone.IsWorldMap())
		{
			return true;
		}
		if (currentZone.Z <= 10 && !currentZone.HasZoneProperty("inside"))
		{
			return true;
		}
		return false;
	}

	public bool IsFrozen()
	{
		return pPhysics?.IsFrozen() ?? false;
	}

	public bool CheckFrozen(bool Telepathic = false, bool Telekinetic = false, bool Silent = false, GameObject Target = null)
	{
		if (IsFrozen() && (!Telepathic || ((Target == null) ? (!HasPart("Telepathy")) : (!CanMakeTelepathicContactWith(Target)))) && (!Telekinetic || ((Target == null) ? (!HasPart("Telekinesis")) : (!CanManipulateTelekinetically(Target)))))
		{
			if (!Silent && IsPlayer())
			{
				Popup.ShowFail("You are frozen solid!");
			}
			return false;
		}
		return true;
	}

	public bool IsFreezing()
	{
		return pPhysics?.IsFreezing() ?? false;
	}

	public bool IsAflame()
	{
		return pPhysics?.IsAflame() ?? false;
	}

	public bool IsVaporizing()
	{
		return pPhysics?.IsVaporizing() ?? false;
	}

	public bool IsMissingTongue()
	{
		if (GetEffect("Glotrot") is Glotrot glotrot)
		{
			return glotrot.Stage >= 3;
		}
		return false;
	}

	/// <summary>
	///             Gets how many drams of space you have available for storing a
	///             specified liquid.
	///             </summary>
	public int GetStorableDrams(string liquidType = "water", GameObject skip = null, List<GameObject> skipList = null, Predicate<GameObject> filter = null, bool safeOnly = true, LiquidVolume liquidVolume = null)
	{
		return GetStorableDramsEvent.GetFor(this, liquidType, skip, skipList, filter, safeOnly, liquidVolume);
	}

	public int GetAutoCollectDrams(string liquidType = "water", GameObject skip = null, List<GameObject> skipList = null)
	{
		return GetAutoCollectDramsEvent.GetFor(this, liquidType, skip, skipList);
	}

	/// <summary>
	///             Gets how many drams of a specified liquid you have usable on hand.
	///             </summary>
	public int GetFreeDrams(string liquidType = "water", GameObject skip = null, List<GameObject> skipList = null, Predicate<GameObject> filter = null, bool impureOkay = false)
	{
		return GetFreeDramsEvent.GetFor(this, liquidType, skip, skipList, filter, impureOkay);
	}

	public bool GiveDrams(int drams, string liquidType = "water", bool auto = false, GameObject skip = null, List<GameObject> skipList = null, Predicate<GameObject> filter = null, List<GameObject> storedIn = null, bool safeOnly = true, LiquidVolume liquidVolume = null)
	{
		return !GiveDramsEvent.Check(this, liquidType, drams, skip, skipList, filter, auto, storedIn, safeOnly, liquidVolume);
	}

	public bool UseDrams(int drams, string liquidType = "water", GameObject skip = null, List<GameObject> skipList = null, Predicate<GameObject> filter = null, List<GameObject> trackContainers = null, bool drinking = false)
	{
		return !UseDramsEvent.Check(this, liquidType, drams, skip, skipList, filter, ImpureOkay: false, trackContainers, drinking);
	}

	public bool UseImpureDrams(int drams, string liquidType = "water", GameObject skip = null, List<GameObject> skipList = null, Predicate<GameObject> filter = null, List<GameObject> trackContainers = null, bool drinking = false)
	{
		return !UseDramsEvent.Check(this, liquidType, drams, skip, skipList, filter, ImpureOkay: true, trackContainers, drinking);
	}

	public bool AllowLiquidCollection(string liquidType = "water", GameObject actor = null)
	{
		return AllowLiquidCollectionEvent.Check(this, actor, liquidType);
	}

	public bool WantsLiquidCollection(string liquidType = "water", GameObject actor = null)
	{
		return WantsLiquidCollectionEvent.Check(this, actor, liquidType);
	}

	public bool UseEnergy(int Amount, string Type, string Context = null, int? MoveSpeed = null)
	{
		int num = 0;
		if (IsFreezing() && !IsFrozen())
		{
			int num2 = pPhysics.FreezeTemperature - pPhysics.Temperature;
			int num3 = pPhysics.FreezeTemperature - pPhysics.BrittleTemperature;
			num -= num2 * 100 / num3;
		}
		int minAmount = 0;
		if (Type != null && Type.Contains("Movement"))
		{
			minAmount = Amount / 20;
			if (!IsFlying && HasStat("MoveSpeed"))
			{
				int num4 = MoveSpeed ?? Stat("MoveSpeed");
				if (num4 != 100)
				{
					num += 100 - (int)(100f / ((float)(100 - num4) / 100f + 1f));
				}
			}
		}
		Amount = GetEnergyCostEvent.GetFor(this, Amount, Type, num, 0, minAmount);
		if (Energy != null)
		{
			Amount = Math.Max(Amount * (900 + XRL.Rules.Stat.Random(0, 200)) / 1000, 0);
			Energy.BaseValue -= Amount;
		}
		return Amount > 0;
	}

	public void UseEnergy(int Amount)
	{
		UseEnergy(Amount, "None");
	}

	public void LoadBlueprint()
	{
		foreach (IPart parts in PartsList)
		{
			parts.LoadBlueprint();
		}
	}

	public string GetWeaponSkill()
	{
		if (!(GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon))
		{
			return "";
		}
		return meleeWeapon.Skill;
	}

	public bool WillTrade()
	{
		if (HasTagOrProperty("NoTrade"))
		{
			return false;
		}
		if (HasTagOrProperty("FugueCopy"))
		{
			return false;
		}
		if (HasTagOrProperty("Nullphase"))
		{
			return false;
		}
		return true;
	}

	public int DistanceTo(Location2D cellLocation)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		return DistanceTo(currentCell.ParentZone.GetCell(cellLocation));
	}

	public int DistanceTo(GameObject Object)
	{
		Cell cell = Object?.CurrentCell;
		if (cell == null || cell.IsGraveyard() || cell.ParentZone == null || cell.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		return cell.PathDistanceTo(currentCell);
	}

	public int DistanceTo(Cell C)
	{
		if (C.IsGraveyard() || C.ParentZone == null || C.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
		{
			return 9999999;
		}
		return C.PathDistanceTo(currentCell);
	}

	public double RealDistanceTo(GameObject Object)
	{
		Cell currentCell = Object.CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone == null || currentCell.ParentZone.IsWorldMap())
		{
			return 9999999.0;
		}
		Cell currentCell2 = CurrentCell;
		if (currentCell2 == null || currentCell2.IsGraveyard() || currentCell2.ParentZone == null || currentCell2.ParentZone.IsWorldMap())
		{
			return 9999999.0;
		}
		return currentCell.RealDistanceTo(currentCell2);
	}

	public List<Tuple<Cell, char>> GetLineTo(Cell OC, bool IncludeSolid = true, bool UseTargetability = false)
	{
		if (OC == null || OC.IsGraveyard())
		{
			return null;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard())
		{
			return null;
		}
		if (OC.ParentZone != currentCell.ParentZone)
		{
			return null;
		}
		return currentCell.ParentZone.GetLine(currentCell.X, currentCell.Y, OC.X, OC.Y, IncludeSolid, UseTargetability ? this : null);
	}

	public List<Tuple<Cell, char>> GetLineTo(GameObject Object, bool bIncludeSolid = true, bool UseTargetability = false)
	{
		return GetLineTo(Object.CurrentCell, bIncludeSolid, UseTargetability);
	}

	public List<Tuple<Cell, char>> GetLineToNLong(GameObject Object, int N, bool bIncludeSolid = true, bool UseTargetability = false)
	{
		Cell currentCell = Object.CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard())
		{
			return null;
		}
		Cell currentCell2 = CurrentCell;
		if (currentCell2 == null || currentCell2.IsGraveyard())
		{
			return null;
		}
		if (currentCell.ParentZone != currentCell2.ParentZone)
		{
			return null;
		}
		return currentCell2.ParentZone.GetLine(currentCell2.X, currentCell2.Y, currentCell.X, currentCell.Y, bIncludeSolid, UseTargetability ? this : null);
	}

	public bool HasLOSTo(int x, int y, bool IncludeSolid = true, bool UseTargetability = false, Predicate<Cell> OverrideBlocking = null)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard())
		{
			return false;
		}
		return currentCell.ParentZone.CalculateLOS(currentCell.X, currentCell.Y, x, y, IncludeSolid, UseTargetability ? this : null, OverrideBlocking);
	}

	public bool HasLOSTo(Cell C, bool IncludeSolid = true, bool UseTargetability = false, Predicate<Cell> OverrideBlocking = null)
	{
		if (C == null || C.IsGraveyard())
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || C.ParentZone != currentCell.ParentZone)
		{
			return false;
		}
		return HasLOSTo(C.X, C.Y, IncludeSolid, UseTargetability, OverrideBlocking);
	}

	public bool HasLOSTo(GameObject Object, bool IncludeSolid = true, bool UseTargetability = false, Predicate<Cell> OverrideBlocking = null)
	{
		if (Object == null)
		{
			return false;
		}
		return HasLOSTo(Object.CurrentCell, IncludeSolid, UseTargetability, OverrideBlocking);
	}

	public bool HasUnobstructedLineTo(Cell C, bool UseTargetability = false)
	{
		if (C == null || C.IsGraveyard())
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.IsGraveyard() || currentCell.ParentZone != C.ParentZone)
		{
			return false;
		}
		return currentCell.ParentZone.HasUnobstructedLineTo(currentCell.X, currentCell.Y, C.X, C.Y, UseTargetability ? this : null);
	}

	public bool HasUnobstructedLineTo(GameObject Object, bool UseTargetability = false)
	{
		return HasUnobstructedLineTo(Object.CurrentCell, UseTargetability);
	}

	public bool TryUnequip(bool Silent = false, bool SemiForced = false)
	{
		return EquippedOn()?.TryUnequip(Silent, SemiForced) ?? false;
	}

	public bool ForceUnequip(bool Silent = false)
	{
		return EquippedOn()?.ForceUnequip(Silent) ?? false;
	}

	public BodyPart EquippedOn()
	{
		return Equipped?.Body?.FindEquippedItem(this);
	}

	public bool IsEquippedOnType(string FindType)
	{
		return (Equipped?.Body?.IsEquippedOnType(this, FindType)).GetValueOrDefault();
	}

	public bool IsEquippedOnCategory(int FindCategory)
	{
		return (Equipped?.Body?.IsEquippedOnCategory(this, FindCategory)).GetValueOrDefault();
	}

	public bool IsEquippedOnPrimary()
	{
		return Equipped?.HasEquippedOnPrimary(this) ?? false;
	}

	public bool HasEquippedOnPrimary(GameObject obj)
	{
		return Body?.IsEquippedOnPrimary(obj) ?? false;
	}

	public void GainSP(int amount, bool message = true)
	{
		if (message && IsPlayer())
		{
			Popup.Show("You gain {{C|" + amount + "}} skill points!");
		}
		Statistics["SP"].BaseValue += amount;
	}

	public void GainEgo(int amount, bool message = true)
	{
		if (message && IsPlayer())
		{
			Popup.Show("Your Ego is increased by {{G|" + amount + "}}!");
		}
		GetStat("Ego").BaseValue += amount;
	}

	public void LoseEgo(int amount, bool message = true)
	{
		if (message && IsPlayer())
		{
			Popup.Show("Your Ego is decreased by {{R|" + amount + "}}!");
		}
		GetStat("Ego").BaseValue -= amount;
	}

	public void GainIntelligence(int amount, bool message = true)
	{
		if (message && IsPlayer())
		{
			Popup.Show("Your Intelligence is increased by {{G|" + amount + "}}!");
		}
		GetStat("Intelligence").BaseValue += amount;
	}

	public void GainWillpower(int amount, bool message = true)
	{
		if (message && IsPlayer())
		{
			Popup.Show("Your Willpower is increased by {{G|" + amount + "}}!");
		}
		GetStat("Willpower").BaseValue += amount;
	}

	public GameObject GetShield(Predicate<GameObject> Filter = null, GameObject Attacker = null)
	{
		return Body?.GetShield(Filter, Attacker);
	}

	public GameObject GetWeaponOfType(string Type, bool PreferPrimary = false)
	{
		return Body?.GetWeaponOfType(Type, NeedPrimary: false, PreferPrimary);
	}

	public GameObject GetPrimaryWeaponOfType(string Type)
	{
		return Body?.GetPrimaryWeaponOfType(Type);
	}

	public GameObject GetPrimaryWeaponOfType(string Type, bool AcceptFirstHandForNonHandPrimary)
	{
		return Body?.GetPrimaryWeaponOfType(Type, AcceptFirstHandForNonHandPrimary);
	}

	public bool HasWeaponOfType(string Type, bool NeedPrimary = false)
	{
		return Body?.HasWeaponOfType(Type, NeedPrimary) ?? false;
	}

	public bool HasPrimaryWeaponOfType(string Type)
	{
		return Body?.HasPrimaryWeaponOfType(Type) ?? false;
	}

	public void ClearShieldBlocks()
	{
		Body?.ClearShieldBlocks();
	}

	public bool IsImplantedInCategory(int FindCategory)
	{
		return (Equipped?.Body?.IsImplantedInCategory(this, FindCategory)).GetValueOrDefault();
	}

	public GameObject ReplaceWith(GameObject NewObject)
	{
		SplitFromStack();
		ReplaceInContextEvent.Send(this, NewObject);
		Obliterate();
		return NewObject;
	}

	public GameObject ReplaceWith(string NewObject)
	{
		return ReplaceWith(create(NewObject));
	}

	public bool Obliterate(string Reason = null, bool Silent = false, string ThirdPersonReason = null)
	{
		return Destroy(Reason, Silent, Obliterate: true, ThirdPersonReason);
	}

	public bool Destroy(string Reason = null, bool Silent = false, bool Obliterate = false, string ThirdPersonReason = null)
	{
		if (IsInGraveyard())
		{
			return true;
		}
		if (!BeforeDestroyObjectEvent.Check(this, Obliterate, Silent, Reason, ThirdPersonReason) && !Obliterate)
		{
			return false;
		}
		if (IsPlayer())
		{
			if (HasEffect("Dominated"))
			{
				MetricsManager.LogInfo("Player dominating something when it was destroyed");
				XRLCore.Core.RenderBase();
				AchievementManager.SetAchievement("ACH_WINKED_OUT");
				if (CheckpointingSystem.ShowDeathMessage("Your mind winks out of existence."))
				{
					return true;
				}
				if (Options.AllowReallydie && Popup.ShowYesNo("DEBUG: Do you really want to die?", AllowEscape: true, DialogResult.No) != 0)
				{
					RemoveEffect("Dominated");
				}
				else
				{
					XRLCore.Core.Game.DeathReason = "Your mind winked out of existence.";
				}
			}
			else
			{
				MetricsManager.LogInfo("PlayerDestroyed (probably alright but just in case)", Environment.StackTrace);
				XRLCore.Core.RenderBase();
				if (CheckpointingSystem.ShowDeathMessage("You die! (good job)"))
				{
					return true;
				}
				if (Options.AllowReallydie && Popup.ShowYesNo("DEBUG: Do you really want to die?", AllowEscape: true, DialogResult.No) != 0)
				{
					RestorePristineHealth();
					return false;
				}
				XRLCore.Core.Game.DeathReason = Reason ?? ("You were " + (Obliterate ? "obliterated" : "destroyed"));
			}
			if (IsPlayer())
			{
				XRLCore.Core.Game.Running = false;
				return true;
			}
		}
		else if (pBrain != null && ThePlayer != null && pBrain.PartyLeader == ThePlayer && !IsTrifling)
		{
			string propertyOrTag = GetPropertyOrTag("CustomDeathVerb", "died");
			string value = null;
			if (!HasTagOrProperty("CustomDeathVerb"))
			{
				if (!string.IsNullOrEmpty(ThirdPersonReason))
				{
					value = ThirdPersonReason.Replace("@@", "");
					value = Regex.Replace(value, "##.*?##", "");
					string oldValue = "by " + ThePlayer.a + ThePlayer.ShortDisplayName;
					value = value.Replace(oldValue, "by you");
				}
				else
				{
					value = GameText.RoughConvertSecondPersonToThirdPerson(Reason, this);
				}
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("Your companion, ").Append(HasProperName ? BaseDisplayName : Grammar.A(BaseDisplayName)).Append(", ")
				.Append(propertyOrTag)
				.Append('.');
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(' ').Append(value);
			}
			if (HasTagOrProperty("NoFollowerDeathPopup"))
			{
				MessageQueue.AddPlayerMessage(stringBuilder.ToString());
			}
			else
			{
				Popup.Show(stringBuilder.ToString());
			}
		}
		if (Energy != null)
		{
			Energy.BaseValue = 0;
		}
		OnDestroyObjectEvent.Send(this, Obliterate, Silent, Reason, ThirdPersonReason);
		if (this == Sidebar.CurrentTarget)
		{
			Sidebar.CurrentTarget = null;
		}
		pPhysics?.TeardownForDestroy(Silent);
		return true;
	}

	public GameObject DeepCopy(bool CopyEffects = false, bool CopyID = false, Func<GameObject, GameObject> MapInv = null)
	{
		GameObject gameObject = new GameObject();
		try
		{
			FireEvent(CopyEffects ? "BeforeDeepCopyWithEffects" : "BeforeDeepCopyWithoutEffects");
			gameObject.DeepCopyInventoryObjectMap = new Dictionary<GameObject, GameObject>();
			gameObject.Blueprint = Blueprint;
			gameObject.GenderName = GenderName;
			gameObject.PronounSetName = PronounSetName;
			gameObject.PronounSetKnown = PronounSetKnown;
			foreach (string key in Property.Keys)
			{
				if (!(key == "id") || CopyID)
				{
					gameObject.Property.Add(key, Property[key]);
				}
			}
			foreach (string key2 in IntProperty.Keys)
			{
				gameObject.IntProperty.Add(key2, IntProperty[key2]);
			}
			if (CopyEffects)
			{
				foreach (Effect effect in Effects)
				{
					gameObject.Effects.Add(effect.DeepCopy(gameObject, MapInv));
				}
			}
			else
			{
				foreach (Effect effect2 in Effects)
				{
					if (effect2.allowCopyOnNoEffectDeepCopy())
					{
						gameObject.Effects.Add(effect2.DeepCopy(gameObject, MapInv));
					}
				}
			}
			foreach (string key3 in Statistics.Keys)
			{
				Statistic statistic = new Statistic(Statistics[key3]);
				statistic.Owner = gameObject;
				while (statistic.Shifts != null && statistic.Shifts.Count > 0)
				{
					statistic.RemoveShift(statistic.Shifts[0].ID);
				}
				gameObject.Statistics.Add(key3, statistic);
			}
			for (int i = 0; i < PartsList.Count; i++)
			{
				gameObject.AddPartInternals(PartsList[i].DeepCopy(gameObject, MapInv), DoRegistration: false, Initial: false, Creation: true);
			}
			if (RegisteredPartEvents != null)
			{
				gameObject.RegisteredPartEvents = new Dictionary<string, List<IPart>>();
				foreach (string key4 in RegisteredPartEvents.Keys)
				{
					gameObject.RegisteredPartEvents.Add(key4, new List<IPart>());
					foreach (IPart item in RegisteredPartEvents[key4])
					{
						if (item.ParentObject == this)
						{
							gameObject.RegisteredPartEvents[key4].Add(gameObject.GetPart(item.Name));
						}
						else if (item.ParentObject != null && gameObject.DeepCopyInventoryObjectMap.ContainsKey(item.ParentObject))
						{
							gameObject.RegisteredPartEvents[key4].Add(gameObject.DeepCopyInventoryObjectMap[item.ParentObject].GetPart(item.Name));
						}
					}
				}
			}
			gameObject.FinalizeCopy(this, CopyEffects, CopyID);
			gameObject.DeepCopyInventoryObjectMap = null;
			return gameObject;
		}
		finally
		{
			FireEvent(CopyEffects ? "AfterDeepCopyWithEffects" : "AfterDeepCopyWithoutEffects");
		}
	}

	public void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv = null)
	{
		List<IPart> list = new List<IPart>(PartsList.Count + (8 - PartsList.Count % 8));
		list.Clear();
		list.AddRange(PartsList);
		foreach (IPart item in list)
		{
			if (PartsList.Contains(item))
			{
				item.FinalizeCopyEarly(Source, CopyEffects, CopyID, MapInv);
			}
		}
		list.Clear();
		list.AddRange(PartsList);
		foreach (IPart item2 in list)
		{
			if (PartsList.Contains(item2))
			{
				item2.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
			}
		}
		list.Clear();
		list.AddRange(PartsList);
		foreach (IPart item3 in list)
		{
			if (PartsList.Contains(item3))
			{
				item3.FinalizeCopyLate(Source, CopyEffects, CopyID, MapInv);
			}
		}
	}

	public void WasUnstackedFrom(GameObject obj)
	{
		if (Effects == null)
		{
			return;
		}
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			Effect effect = Effects[i];
			effect.WasUnstackedFrom(obj);
			if (count != Effects.Count)
			{
				count = Effects.Count;
				if (i < count && Effects[i] != effect)
				{
					i--;
				}
			}
		}
	}

	public bool StripContents(bool KeepNatural = false, bool Silent = false)
	{
		return HandleEvent(StripContentsEvent.FromPool(this, KeepNatural, Silent));
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(Blueprint);
		Writer.Write(GenderName);
		Writer.Write(PronounSetName);
		Writer.Write(PronounSetKnown);
		if (Property.Count > 0)
		{
			Writer.Write(Property.Count);
			Writer.Write(Property);
		}
		else
		{
			Writer.Write(0);
		}
		if (IntProperty.Count > 0)
		{
			Writer.Write(IntProperty.Count);
			Writer.Write(IntProperty);
		}
		else
		{
			Writer.Write(0);
		}
		if (_Effects == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(_Effects.Count);
			foreach (Effect effect in Effects)
			{
				effect.Save(Writer);
			}
		}
		Writer.Write(Statistics.Count);
		foreach (string key in Statistics.Keys)
		{
			Writer.Write(key);
			Statistics[key].Save(Writer);
		}
		if (PartsList == null)
		{
			Writer.Write(0);
		}
		else
		{
			Writer.Write(PartsList.Count);
			for (int i = 0; i < PartsList.Count; i++)
			{
				try
				{
					PartsList[i].Save(Writer);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Concat("Exception serializing part " + PartsList[i].Name + " : " + ex.ToString(), " : "), ex);
				}
			}
		}
		if (RegisteredEffectEvents == null)
		{
			Writer.Write(-1);
		}
		else
		{
			Writer.Write(RegisteredEffectEvents.Keys.Count);
			foreach (string key2 in RegisteredEffectEvents.Keys)
			{
				Writer.Write(key2);
				Writer.Write(RegisteredEffectEvents[key2].Count);
				foreach (Effect item in RegisteredEffectEvents[key2])
				{
					Writer.Write(item.ID);
				}
			}
		}
		if (RegisteredPartEvents == null)
		{
			Writer.Write(0);
			return;
		}
		int num = RegisteredPartEvents.Keys.Count;
		foreach (string key3 in RegisteredPartEvents.Keys)
		{
			int num2 = RegisteredPartEvents[key3].Count;
			for (int j = 0; j < RegisteredPartEvents[key3].Count; j++)
			{
				if (RegisteredPartEvents[key3][j].AllowStaticRegistration())
				{
					num2--;
				}
			}
			if (num2 <= 0)
			{
				num--;
			}
		}
		Writer.Write(num);
		foreach (string key4 in RegisteredPartEvents.Keys)
		{
			int num3 = RegisteredPartEvents[key4].Count;
			for (int k = 0; k < RegisteredPartEvents[key4].Count; k++)
			{
				if (RegisteredPartEvents[key4][k].AllowStaticRegistration())
				{
					num3--;
				}
			}
			if (num3 <= 0)
			{
				continue;
			}
			Writer.Write(key4);
			Writer.Write(num3);
			foreach (IPart item2 in RegisteredPartEvents[key4])
			{
				if (item2.AllowStaticRegistration())
				{
					continue;
				}
				if (item2.ParentObject == this)
				{
					Writer.Write(item2.Name);
				}
				else if (item2.ParentObject == null)
				{
					if (!(item2 is QuestManager questManager))
					{
						XRLCore.LogError("Bad external binding, tell support@freeholdentertainment.com this: " + item2.Name + ",NULL," + key4);
						continue;
					}
					Writer.Write("QUESTMANAGER");
					Writer.Write(questManager.MyQuestID);
				}
				else
				{
					Writer.Write("EXTERNAL");
					Writer.WriteGameObject(item2.ParentObject);
					Writer.Write(item2.Name);
				}
			}
		}
	}

	public void Load(SerializationReader Reader)
	{
		Blueprint = Reader.ReadString();
		GenderName = Reader.ReadString();
		PronounSetName = Reader.ReadString();
		PronounSetKnown = Reader.ReadBoolean();
		if (Reader.ReadInt32() > 0)
		{
			Property = Reader.ReadDictionary<string, string>();
		}
		if (Reader.ReadInt32() > 0)
		{
			_IntProperty = Reader.ReadDictionary<string, int>();
		}
		RegisteredEffectEvents = null;
		int num = Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Effect item = Effect.Load(Reader);
			Effects.Add(item);
			EffectsLoaded++;
		}
		int num2 = Reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			string key = Reader.ReadString();
			Statistics.Add(key, Statistic.Load(Reader, this));
			StatsLoaded++;
		}
		int num3 = Reader.ReadInt32();
		for (int k = 0; k < num3; k++)
		{
			IPart part = IPart.Load(Reader);
			if (part == null)
			{
				MetricsManager.LogError("Deserialized a null part on blueprint: " + Blueprint);
			}
			else
			{
				AddPartInternals(part, DoRegistration: false, Initial: false, Creation: true);
			}
			PartsLoaded++;
		}
		int num4 = Reader.ReadInt32();
		if (num4 != -1)
		{
			for (int l = 0; l < num4; l++)
			{
				EffectEventsLoaded++;
				string @event = Reader.ReadString();
				int num5 = Reader.ReadInt32();
				for (int m = 0; m < num5; m++)
				{
					Guid guid = Reader.ReadGuid();
					for (int n = 0; n < Effects.Count; n++)
					{
						Effect effect = Effects[n];
						if (effect.ID == guid)
						{
							RegisterEffectEvent(effect, @event);
							break;
						}
					}
				}
			}
		}
		RegisteredPartEvents = new Dictionary<string, List<IPart>>();
		int num6 = Reader.ReadInt32();
		for (int num7 = 0; num7 < num6; num7++)
		{
			PartEventsLoaded++;
			string text = Reader.ReadString();
			RegisteredPartEvents.Add(text, new List<IPart>());
			int num8 = Reader.ReadInt32();
			if (LoadedEvents == null)
			{
				LoadedEvents = new Dictionary<string, int>();
			}
			if (!LoadedEvents.ContainsKey(text))
			{
				LoadedEvents.Add(text, 0);
			}
			LoadedEvents[text]++;
			for (int num9 = 0; num9 < num8; num9++)
			{
				string text2 = Reader.ReadString();
				if (text2 == "QUESTMANAGER")
				{
					if (!ExternalLoadBindings.ContainsKey(this))
					{
						ExternalLoadBindings.Add(this, new List<ExternalEventBind>());
					}
					ExternalLoadBindings[this].Add(new ExternalEventBind(text, null, Reader.ReadString()));
				}
				else if (text2 == "EXTERNAL")
				{
					GameObject gO = Reader.ReadGameObject("external");
					string part2 = Reader.ReadString();
					if (!ExternalLoadBindings.ContainsKey(this))
					{
						ExternalLoadBindings.Add(this, new List<ExternalEventBind>());
					}
					ExternalLoadBindings[this].Add(new ExternalEventBind(text, gO, part2));
				}
				else if (HasPart(text2))
				{
					RegisteredPartEvents[text].Add(GetPart(text2));
				}
				else
				{
					XRLCore.LogError("Bad part binding IM or e-mail to support@freeholdentertainment.com: " + text2);
				}
			}
		}
		if (PartsList == null)
		{
			return;
		}
		int num10 = 0;
		for (int count = PartsList.Count; num10 < count; num10++)
		{
			if (PartsList[num10].AllowStaticRegistration())
			{
				PartsList[num10].Register(this);
			}
		}
		int num11 = 0;
		for (int count2 = PartsList.Count; num11 < count2; num11++)
		{
			PartsList[num11].ObjectLoaded();
		}
	}

	public virtual void FinalizeLoad()
	{
		if (ExternalLoadBindings.ContainsKey(this))
		{
			List<ExternalEventBind> list = ExternalLoadBindings[this];
			for (int i = 0; i < list.Count; i++)
			{
				ExternalEventBind externalEventBind = list[i];
				if (!RegisteredPartEvents.ContainsKey(externalEventBind.Event))
				{
					RegisteredPartEvents.Add(externalEventBind.Event, new List<IPart>());
				}
				if (externalEventBind.GO != null && externalEventBind.GO.HasPart(externalEventBind.Part))
				{
					RegisteredPartEvents[externalEventBind.Event].Add(externalEventBind.GO.GetPart(externalEventBind.Part));
				}
				else if (externalEventBind.GO != null && !externalEventBind.GO.HasPart(externalEventBind.Part))
				{
					string text = "";
					text = ((externalEventBind.GO != null) ? ("Bad binding, tell support@freeholdentertainment.com this: " + externalEventBind.Part + "," + externalEventBind.GO.DisplayName + "," + externalEventBind.Event) : ("Bad binding, tell support@freeholdentertainment.com this: " + externalEventBind.Part + ",NULL," + externalEventBind.Event));
					XRLCore.LogError(text);
				}
				else
				{
					RegisteredPartEvents[externalEventBind.Event].Add(XRLCore.Core.Game.Quests[externalEventBind.Part]._Manager);
				}
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			PartsList[j].FinalizeLoad();
		}
	}

	public GameObjectBlueprint GetBlueprint()
	{
		if (GameObjectFactory.Factory.Blueprints.TryGetValue(Blueprint, out var value))
		{
			return value;
		}
		MetricsManager.LogError("GameObject::GetBlueprint()", "Unknown Blueprint=" + Blueprint);
		return GameObjectFactory.Factory.Blueprints["Object"];
	}

	public void _clearCaches()
	{
		Body = null;
		LiquidVolume = null;
		Inventory = null;
		_pBrain = null;
	}

	public void SetPartyLeader(GameObject leader, bool takeOnAttitudesOfLeader = true, bool trifling = false, bool copyTargetWithAttitudes = false)
	{
		if (pBrain == null)
		{
			return;
		}
		pBrain.PartyLeader = leader;
		if (leader != null)
		{
			if (leader.IsPlayer() && !HasProperty("WasPlayerFollower") && !trifling)
			{
				SetLongProperty("WasPlayerFollower", XRLCore.CurrentTurn);
				leader.FireEvent(Event.New("GainedNewFollower", "Follower", this));
			}
			if (takeOnAttitudesOfLeader)
			{
				TakeOnAttitudesOf(leader, CopyLeader: false, copyTargetWithAttitudes);
			}
		}
		if (trifling)
		{
			IsTrifling = true;
		}
	}

	public bool SupportsFollower(GameObject Object)
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.PartyMembers.ContainsKey(Object.id);
	}

	public bool SupportsFollower(GameObject Object, int Mask)
	{
		if (pBrain == null)
		{
			return false;
		}
		if (!pBrain.PartyMembers.TryGetValue(Object.id, out var value))
		{
			return false;
		}
		return value.HasBit(Mask);
	}

	public Brain.FactionAllegiance GetFactionAllegiance(string Faction)
	{
		if (pBrain == null)
		{
			return Brain.FactionAllegiance.none;
		}
		return pBrain.GetFactionAllegiance(Faction);
	}

	public bool IsMemberOfFaction(string Faction)
	{
		return GetFactionAllegiance(Faction) != Brain.FactionAllegiance.none;
	}

	public bool TakeOnAttitudesOf(GameObject who, bool CopyLeader = false, bool CopyTarget = false)
	{
		if (pBrain == null)
		{
			return false;
		}
		return pBrain.TakeOnAttitudesOf(who, CopyLeader, CopyTarget);
	}

	public bool Owns(string Owner)
	{
		if (string.IsNullOrEmpty(Owner))
		{
			return false;
		}
		return GetFactionAllegiance(Owner) == Brain.FactionAllegiance.member;
	}

	public bool Owns(GameObject obj)
	{
		return Owns(obj.Owner);
	}

	public bool IsNatural()
	{
		if (!HasPropertyOrTag("Natural"))
		{
			return HasPropertyOrTag("NaturalGear");
		}
		return true;
	}

	private string GetDirectionFromCellXY(int X, int Y, bool showCenter = false)
	{
		switch (Y)
		{
		case 0:
			switch (X)
			{
			case 0:
				return "NW";
			case 1:
				return "N";
			case 2:
				return "NE";
			}
			break;
		case 2:
			switch (X)
			{
			case 0:
				return "SW";
			case 1:
				return "S";
			case 2:
				return "SE";
			}
			break;
		default:
			switch (X)
			{
			case 0:
				return "W";
			case 2:
				return "E";
			}
			break;
		}
		if (!showCenter)
		{
			return null;
		}
		return "C";
	}

	public void PullDown(bool AllowAlternate = false)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null || !currentCell.OnWorldMap() || !IsPlayer())
		{
			return;
		}
		XRLGame game = XRLCore.Core.Game;
		string zoneWorld = game.ZoneManager.ActiveZone.GetZoneWorld();
		string stringGameState = XRLCore.Core.Game.GetStringGameState("LastLocationOnSurface");
		int x = currentCell.X;
		int y = currentCell.Y;
		int z = 10;
		string text = null;
		JournalMapNote journalMapNote = null;
		int num = 1;
		int num2 = 1;
		foreach (CellBlueprint cellBlueprint in game.ZoneManager.GetCellBlueprints(zoneWorld + "." + x + "." + y))
		{
			if (!string.IsNullOrEmpty(cellBlueprint.LandingZone))
			{
				try
				{
					string[] array = cellBlueprint.LandingZone.Split(',');
					num = Convert.ToInt32(array[0]);
					num2 = Convert.ToInt32(array[1]);
				}
				catch
				{
					continue;
				}
				break;
			}
		}
		if (AllowAlternate)
		{
			List<string> list = new List<string>();
			List<char> list2 = new List<char>();
			List<PullDownChoice> list3 = new List<PullDownChoice>();
			char c = 'a';
			if (!string.IsNullOrEmpty(stringGameState))
			{
				string text2 = "Current location";
				Cell cell = Cell.FromAddress(stringGameState);
				if (cell != null && ZoneID.Parse(cell.ParentZone?.ZoneID, out var _, out var _, out var ZoneX, out var ZoneY))
				{
					string directionFromCellXY = GetDirectionFromCellXY(ZoneX, ZoneY, showCenter: true);
					if (!string.IsNullOrEmpty(directionFromCellXY))
					{
						text2 = text2 + " (" + directionFromCellXY + ")";
					}
				}
				list.Add(text2);
				list2.Add((c <= 'z') ? c++ : ' ');
				list3.Add(new PullDownChoice
				{
					location = stringGameState,
					X = num,
					Y = num2
				});
			}
			list.Add((num == 1 && num2 == 1) ? "Center" : "Arrival location");
			list2.Add(c++);
			list3.Add(new PullDownChoice
			{
				X = num,
				Y = num2
			});
			GameObject firstObjectWithPart = currentCell.GetFirstObjectWithPart("TerrainNotes");
			if (firstObjectWithPart != null && firstObjectWithPart.GetPart("TerrainNotes") is TerrainNotes terrainNotes && terrainNotes.notes != null)
			{
				foreach (JournalMapNote note in terrainNotes.notes)
				{
					if (note.cz != 10)
					{
						continue;
					}
					bool flag = false;
					int i = 0;
					for (int count = list3.Count; i < count; i++)
					{
						PullDownChoice pullDownChoice = list3[i];
						if (pullDownChoice.location == null && pullDownChoice.X == note.cx && pullDownChoice.Y == note.cy)
						{
							List<string> list4 = list;
							int index = i;
							list4[index] = list4[index] + ", " + note.GetShortText();
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list.Add(note.GetShortText());
						list2.Add(c++);
						list3.Add(new PullDownChoice
						{
							X = note.cx,
							Y = note.cy
						});
					}
				}
			}
			if (list3.Count > 1)
			{
				int j = 0;
				for (int count2 = list3.Count; j < count2; j++)
				{
					PullDownChoice pullDownChoice2 = list3[j];
					string directionFromCellXY2 = GetDirectionFromCellXY(pullDownChoice2.X, pullDownChoice2.Y);
					if (!string.IsNullOrEmpty(directionFromCellXY2))
					{
						List<string> list4 = list;
						int index = j;
						list4[index] = list4[index] + " (" + directionFromCellXY2 + ")";
					}
				}
				int num3 = Popup.ShowOptionList("Select a destination", list.ToArray(), list2.ToArray(), 1, null, 60, RespectOptionNewlines: false, AllowEscape: true);
				if (num3 < 0)
				{
					return;
				}
				text = list3[num3].location;
				num = list3[num3].X;
				num2 = list3[num3].Y;
			}
			else if (list3.Count == 1)
			{
				text = list3[0].location;
				num = list3[0].X;
				num2 = list3[0].Y;
			}
		}
		if (text != null)
		{
			Cell cell2 = Cell.FromAddress(stringGameState);
			if (cell2 != null)
			{
				DirectMoveTo(cell2, 0, forced: false, ignoreCombat: true);
				return;
			}
		}
		if (journalMapNote != null)
		{
			num = journalMapNote.cx;
			num2 = journalMapNote.cy;
		}
		Zone zone = XRL.The.ZoneManager.GetZone(zoneWorld, x, y, num, num2, z);
		Cell pullDownLocation = zone.GetPullDownLocation(this);
		if (pullDownLocation == null)
		{
			MetricsManager.LogError("failed to get pulldown location from " + zone.ZoneID);
		}
		else if (string.IsNullOrEmpty(stringGameState))
		{
			SystemMoveTo(pullDownLocation, 0);
		}
		else
		{
			SystemLongDistanceMoveTo(pullDownLocation, 0);
		}
		zone.CheckWeather();
	}

	public void FinalizeStats()
	{
		foreach (Statistic value in Statistics.Values)
		{
			if (!(value.sValue != ""))
			{
				continue;
			}
			if (value.sValue == "*XP")
			{
				float num = Statistics["Level"].Value;
				num /= 2f;
				string text = "Minion";
				if (Property.ContainsKey("Role"))
				{
					text = Property["Role"];
				}
				switch (text)
				{
				case "Minion":
					value.BaseValue = (int)(num * 20f);
					break;
				case "Leader":
					value.BaseValue = (int)(num * 100f);
					break;
				case "Hero":
					value.BaseValue = (int)(num * 200f);
					break;
				default:
					value.BaseValue = (int)(num * 50f);
					break;
				}
				continue;
			}
			string text2 = "NPC";
			if (Property.ContainsKey("Role"))
			{
				text2 = Property["Role"];
			}
			if (text2 == "Minion" && (value.Name == "Strength" || value.Name == "Agility" || value.Name == "Toughness" || value.Name == "Willpower" || value.Name == "Intelligence" || value.Name == "Ego"))
			{
				value.Boost--;
			}
			int num2 = 0;
			int num3 = 0;
			if (Statistics.ContainsKey("Level"))
			{
				num3 = Statistics["Level"].Value / 5 + 1;
			}
			if (value.sValue.Contains(","))
			{
				string[] array = value.sValue.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Contains("(t"))
					{
						if (array[i].Contains("(t)"))
						{
							array[i] = array[i].Replace("(t)", num3.ToString());
						}
						if (array[i].Contains("(t-1)"))
						{
							array[i] = array[i].Replace("(t-1)", (num3 - 1).ToString());
						}
						if (array[i].Contains("(t+1)"))
						{
							array[i] = array[i].Replace("(t+1)", (num3 + 1).ToString());
						}
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					num2 += array[j].RollCached();
				}
			}
			else
			{
				string text3 = value.sValue;
				if (text3.Contains("(t"))
				{
					if (text3.Contains("(t)"))
					{
						text3 = text3.Replace("(t)", num3.ToString());
					}
					if (text3.Contains("(t-1)"))
					{
						text3 = text3.Replace("(t-1)", (num3 - 1).ToString());
					}
					if (text3.Contains("(t+1)"))
					{
						text3 = text3.Replace("(t+1)", (num3 + 1).ToString());
					}
				}
				num2 += text3.RollCached();
			}
			value.BaseValue = num2;
			if (value.Boost > 0)
			{
				value.BaseValue += (int)Math.Ceiling((float)num2 * 0.25f * (float)value.Boost);
			}
			else
			{
				value.BaseValue += (int)Math.Ceiling((float)num2 * 0.2f * (float)value.Boost);
			}
		}
		if (Statistics.ContainsKey("XP") && Statistics.ContainsKey("Level"))
		{
			Statistics["XP"].BaseValue = Leveler.GetXPForLevel(Statistics["Level"].Value);
		}
	}

	public long GetLongProperty(string name)
	{
		if (Property.TryGetValue(name, out var value))
		{
			return Convert.ToInt64(value);
		}
		return 0L;
	}

	public string GetStringProperty(string Name, string Default = null)
	{
		if (Property.TryGetValue(Name, out var value))
		{
			return value;
		}
		return Default;
	}

	public int GetIntProperty(string sProperty, int Default = 0)
	{
		if (sProperty == null)
		{
			return Default;
		}
		if (IntProperty.TryGetValue(sProperty, out var value))
		{
			return value;
		}
		return Default;
	}

	public bool TryGetIntProperty(string Name, out int Result)
	{
		return IntProperty.TryGetValue(Name, out Result);
	}

	public void SetStringProperty(string sProperty, string Value)
	{
		Property[sProperty] = Value;
	}

	public void RemoveStringProperty(string sProperty)
	{
		if (sProperty != null)
		{
			Property.Remove(sProperty);
		}
	}

	public void DeleteStringProperty(string sProperty)
	{
		RemoveStringProperty(sProperty);
	}

	public bool TryGetStringProperty(string Name, out string Result)
	{
		return Property.TryGetValue(Name, out Result);
	}

	public void SetLongProperty(string sProperty, long Value)
	{
		if (!Property.ContainsKey(sProperty))
		{
			Property.Add(sProperty, Value.ToString());
		}
		else
		{
			Property[sProperty] = Value.ToString();
		}
	}

	public bool canPathTo(Cell TC, bool Global = false)
	{
		if (TC == null || TC.ParentZone == null)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.ParentZone == null)
		{
			return false;
		}
		return new FindPath(currentCell.ParentZone.ZoneID, currentCell.X, currentCell.Y, TC.ParentZone.ZoneID, TC.X, TC.Y, Global, PathUnlimited: false, this).bFound;
	}

	public GameObject SetIntProperty(string sProperty, int Value, bool RemoveIfZero = false)
	{
		if (!IntProperty.ContainsKey(sProperty))
		{
			if (!RemoveIfZero || Value != 0)
			{
				IntProperty.Add(sProperty, Value);
			}
		}
		else if (!RemoveIfZero || Value != 0)
		{
			IntProperty[sProperty] = Value;
		}
		else
		{
			IntProperty.Remove(sProperty);
		}
		return this;
	}

	public int ModIntProperty(string sProperty, int Value, bool RemoveIfZero = false)
	{
		if (!IntProperty.ContainsKey(sProperty))
		{
			if (!RemoveIfZero || Value != 0)
			{
				IntProperty.Add(sProperty, Value);
			}
			return Value;
		}
		int num = IntProperty[sProperty] + Value;
		if (!RemoveIfZero || num != 0)
		{
			IntProperty[sProperty] = num;
		}
		else
		{
			IntProperty.Remove(sProperty);
		}
		return num;
	}

	public void RemoveIntProperty(string name)
	{
		if (name != null)
		{
			IntProperty.Remove(name);
		}
	}

	public void RemoveProperty(string name)
	{
		RemoveStringProperty(name);
		RemoveIntProperty(name);
	}

	public int GetStatValue(string stat, int defaultValue = 0)
	{
		if (Statistics == null || !Statistics.ContainsKey(stat))
		{
			return defaultValue;
		}
		return Statistics[stat].Value;
	}

	public bool CanGainMP()
	{
		return Statistics.ContainsKey("MP");
	}

	public bool GainMP(int amount)
	{
		if (Statistics.TryGetValue("MP", out var value))
		{
			value.BaseValue += amount;
			FireEvent(Event.New("GainedMP", "Amount", amount));
			return true;
		}
		return false;
	}

	public bool UseMP(int amount, string context = "default")
	{
		if (Statistics.TryGetValue("MP", out var value))
		{
			value.Penalty += amount;
			FireEvent(Event.New("UsedMP", "Amount", amount, "Context", context));
			return true;
		}
		return false;
	}

	public bool HasStat(string Name)
	{
		if (Statistics != null)
		{
			return Statistics.ContainsKey(Name);
		}
		return false;
	}

	public Statistic GetStat(string Name)
	{
		if (string.IsNullOrEmpty(Name))
		{
			return null;
		}
		if (Statistics != null && Statistics.TryGetValue(Name, out var value))
		{
			return value;
		}
		return null;
	}

	public int Stat(string Name, int Default = 0)
	{
		if (string.IsNullOrEmpty(Name) || Statistics == null)
		{
			return Default;
		}
		if (Statistics.TryGetValue(Name, out var value))
		{
			return value.Value;
		}
		return Default;
	}

	public int BaseStat(string Name, int Default = 0)
	{
		if (string.IsNullOrEmpty(Name) || Statistics == null)
		{
			return Default;
		}
		if (Statistics.TryGetValue(Name, out var value))
		{
			return value.BaseValue;
		}
		return Default;
	}

	public int StatMod(string Name, int Default = 0)
	{
		if (string.IsNullOrEmpty(Name) || Statistics == null)
		{
			return Default;
		}
		if (Name.IndexOf(',') != -1)
		{
			int num = int.MinValue;
			bool flag = false;
			foreach (string item in Name.CachedCommaExpansion())
			{
				if (Statistics.TryGetValue(item, out var value))
				{
					flag = true;
					int modifier = value.Modifier;
					if (modifier > num)
					{
						num = modifier;
					}
				}
			}
			if (!flag)
			{
				return Default;
			}
			return num;
		}
		if (Statistics.TryGetValue(Name, out var value2))
		{
			return value2.Modifier;
		}
		return Default;
	}

	public void BoostStat(string Name, int Amount)
	{
		if (!string.IsNullOrEmpty(Name) && Statistics != null && Statistics.TryGetValue(Name, out var value))
		{
			value.BoostStat(Amount);
		}
	}

	public void BoostStat(string Name, double Amount)
	{
		if (!string.IsNullOrEmpty(Name) && Statistics != null && Statistics.TryGetValue(Name, out var value))
		{
			value.BoostStat(Amount);
		}
	}

	public void MultiplyStat(string Name, int Factor)
	{
		if (!string.IsNullOrEmpty(Name) && Statistics != null && Statistics.TryGetValue(Name, out var value))
		{
			value.BaseValue *= Factor;
		}
	}

	[Obsolete("You may want to switch to using the new StatShifter API")]
	public bool ApplyStatShift(string Name, int Amount)
	{
		if (string.IsNullOrEmpty(Name) || Statistics == null)
		{
			return false;
		}
		if (!Statistics.TryGetValue(Name, out var value))
		{
			return false;
		}
		if (Amount > 0)
		{
			value.Bonus += Amount;
		}
		else if (Amount < 0)
		{
			value.Penalty += -Amount;
		}
		return true;
	}

	[Obsolete("You may want to switch to using the new StatShifter API")]
	public bool UnapplyStatShift(string Name, int Amount)
	{
		if (string.IsNullOrEmpty(Name))
		{
			return false;
		}
		if (!Statistics.TryGetValue(Name, out var value))
		{
			return false;
		}
		if (Amount > 0)
		{
			value.Bonus -= Amount;
		}
		else if (Amount < 0)
		{
			value.Penalty -= -Amount;
		}
		return true;
	}

	public void ShowActiveEffects()
	{
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (Effect effect in Effects)
		{
			if (effect.GetDescription() != null)
			{
				if (num != 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine("{{Y|");
				stringBuilder.AppendLine(effect.GetDescription());
				stringBuilder.Append("}}  ");
				stringBuilder.AppendLine(Campfire.ProcessEffectDescription(effect.GetDetails(), this).Replace("\n", "\n  "));
				num++;
			}
		}
		if (num <= 0)
		{
			if (IsPlayer())
			{
				BookUI.ShowBook("No active effects.", "&WActive Effects&Y - " + DisplayName);
			}
			else
			{
				BookUI.ShowBook("No active effects.", "&WActive Effects&Y - " + DisplayName);
			}
		}
		else
		{
			BookUI.ShowBook(stringBuilder.ToString(), "&WActive Effects&Y - " + DisplayName);
		}
		scrapBuffer.Draw();
	}

	public List<GameObject> GetEquippedObjects()
	{
		return Body?.GetEquippedObjects();
	}

	public List<GameObject> GetEquippedObjectsReadonly()
	{
		return Body?.GetEquippedObjectsReadonly();
	}

	public void GetEquippedObjects(List<GameObject> result)
	{
		Body?.GetEquippedObjects(result);
	}

	public List<GameObject> GetInstalledCybernetics()
	{
		return Body?.GetInstalledCybernetics();
	}

	public List<GameObject> GetInstalledCyberneticsReadonly()
	{
		return Body?.GetInstalledCyberneticsReadonly();
	}

	public void GetInstalledCybernetics(List<GameObject> result)
	{
		Body?.GetInstalledCybernetics(result);
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCybernetics()
	{
		return Body?.GetEquippedObjectsAndInstalledCybernetics();
	}

	public List<GameObject> GetEquippedObjectsAndInstalledCyberneticsReadonly()
	{
		return Body?.GetEquippedObjectsAndInstalledCyberneticsReadonly();
	}

	public void GetEquippedObjectsAndInstalledCybernetics(List<GameObject> result)
	{
		Body?.GetEquippedObjectsAndInstalledCybernetics(result);
	}

	public List<GameObject> GetWholeInventory()
	{
		List<GameObject> list = new List<GameObject>();
		if (HasPart("Inventory"))
		{
			list.AddRange(GetPart<Inventory>().GetObjects());
		}
		if (HasPart("Body"))
		{
			list.AddRange(Body.GetEquippedObjects());
		}
		return list;
	}

	public List<GameObject> GetInventory()
	{
		Inventory inventory = Inventory;
		if (inventory == null)
		{
			return new List<GameObject>(0);
		}
		return inventory.GetObjects();
	}

	public void GetInventory(List<GameObject> result)
	{
		Inventory?.GetObjects(result);
	}

	public void GetInventoryDirect(List<GameObject> result)
	{
		Inventory?.GetObjectsDirect(result);
	}

	public List<GameObject> GetInventory(Predicate<GameObject> pFilter)
	{
		Inventory inventory = Inventory;
		if (inventory == null)
		{
			return new List<GameObject>(0);
		}
		return inventory.GetObjects(pFilter);
	}

	public List<GameObject> GetInventoryDirect(Predicate<GameObject> pFilter)
	{
		Inventory part = GetPart<Inventory>();
		if (part == null)
		{
			return new List<GameObject>(0);
		}
		return part.GetObjectsDirect(pFilter);
	}

	public List<GameObject> GetInventoryAndEquipment()
	{
		List<GameObject> list = (HasPart("Inventory") ? GetPart<Inventory>().GetObjects() : null);
		List<GameObject> list2 = (HasPart("Body") ? Body.GetEquippedObjects() : null);
		int num = 0;
		if (list != null)
		{
			num += list.Count;
		}
		if (list2 != null)
		{
			num += list2.Count;
		}
		List<GameObject> list3 = new List<GameObject>(num);
		if (list != null)
		{
			list3.AddRange(list);
		}
		if (list2 != null)
		{
			list3.AddRange(list2);
		}
		return list3;
	}

	public void GetInventoryAndEquipment(List<GameObject> result)
	{
		Body?.GetEquippedObjects(result);
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			result.AddRange(inventory.Objects);
		}
	}

	public List<GameObject> GetInventoryDirectAndEquipment()
	{
		List<GameObject> list = (HasPart("Inventory") ? GetPart<Inventory>().GetObjectsDirect() : null);
		List<GameObject> list2 = (HasPart("Body") ? Body.GetEquippedObjects() : null);
		int num = 0;
		if (list != null)
		{
			num += list.Count;
		}
		if (list2 != null)
		{
			num += list2.Count;
		}
		List<GameObject> list3 = new List<GameObject>(num);
		if (list != null)
		{
			list3.AddRange(list);
		}
		if (list2 != null)
		{
			list3.AddRange(list2);
		}
		return list3;
	}

	public List<GameObject> GetInventoryAndEquipment(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = (HasPart("Inventory") ? GetPart<Inventory>().GetObjects(pFilter) : null);
		List<GameObject> list2 = (HasPart("Body") ? Body.GetEquippedObjects(pFilter) : null);
		int num = 0;
		if (list != null)
		{
			num += list.Count;
		}
		if (list2 != null)
		{
			num += list2.Count;
		}
		List<GameObject> list3 = new List<GameObject>(num);
		if (list != null)
		{
			list3.AddRange(list);
		}
		if (list2 != null)
		{
			list3.AddRange(list2);
		}
		return list3;
	}

	public List<GameObject> GetInventoryDirectAndEquipment(Predicate<GameObject> pFilter)
	{
		List<GameObject> list = (HasPart("Inventory") ? GetPart<Inventory>().GetObjectsDirect(pFilter) : null);
		List<GameObject> list2 = (HasPart("Body") ? Body.GetEquippedObjects(pFilter) : null);
		int num = 0;
		if (list != null)
		{
			num += list.Count;
		}
		if (list2 != null)
		{
			num += list2.Count;
		}
		List<GameObject> list3 = new List<GameObject>(num);
		if (list != null)
		{
			list3.AddRange(list);
		}
		if (list2 != null)
		{
			list3.AddRange(list2);
		}
		return list3;
	}

	public void ForeachEquippedObject(Action<GameObject> aProc)
	{
		Body?.ForeachEquippedObject(aProc);
	}

	public void SafeForeachEquippedObject(Action<GameObject> aProc)
	{
		Body?.SafeForeachEquippedObject(aProc);
	}

	public void ForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		Body?.ForeachInstalledCybernetics(aProc);
	}

	public void SafeForeachInstalledCybernetics(Action<GameObject> aProc)
	{
		Body?.SafeForeachInstalledCybernetics(aProc);
	}

	public void ForeachEquipmentAndCybernetics(Action<GameObject> aProc)
	{
		Body body = Body;
		if (body != null)
		{
			body.ForeachEquippedObject(aProc);
			body.ForeachInstalledCybernetics(aProc);
		}
	}

	public void SafeForeachEquipmentAndCybernetics(Action<GameObject> aProc)
	{
		Body body = Body;
		if (body != null)
		{
			body.SafeForeachEquippedObject(aProc);
			body.SafeForeachInstalledCybernetics(aProc);
		}
	}

	public void ForeachInventoryAndEquipment(Action<GameObject> aProc)
	{
		GetPart<Inventory>()?.ForeachObject(aProc);
		Body?.ForeachEquippedObject(aProc);
	}

	public void SafeForeachInventoryAndEquipment(Action<GameObject> aProc)
	{
		GetPart<Inventory>()?.SafeForeachObject(aProc);
		Body?.SafeForeachEquippedObject(aProc);
	}

	public void ForeachInventoryEquipmentAndCybernetics(Action<GameObject> aProc)
	{
		GetPart<Inventory>()?.ForeachObject(aProc);
		Body body = Body;
		if (body != null)
		{
			body.ForeachEquippedObject(aProc);
			body.ForeachInstalledCybernetics(aProc);
		}
	}

	public void SafeForeachInventoryEquipmentAndCybernetics(Action<GameObject> aProc)
	{
		GetPart<Inventory>()?.SafeForeachObject(aProc);
		Body body = Body;
		if (body != null)
		{
			body.SafeForeachEquippedObject(aProc);
			body.SafeForeachInstalledCybernetics(aProc);
		}
	}

	public void EquipObject(GameObject Object, BodyPart Part, bool Silent = false, int? EnergyCost = null)
	{
		Event @event = Event.New("CommandEquipObject", "Object", Object, "BodyPart", Part);
		if (EnergyCost.HasValue)
		{
			@event.SetParameter("EnergyCost", EnergyCost.Value);
		}
		if (Silent)
		{
			@event.SetSilent(Silent);
		}
		FireEvent(@event);
	}

	public void EquipObject(GameObject Object, string Slot, bool Silent = false, int? EnergyCost = null)
	{
		Body body = Body;
		if (body == null)
		{
			Debug.LogError("Object with blueprint " + Blueprint + " had no body to equip on");
			return;
		}
		BodyPart firstPart = body.GetFirstPart(Slot);
		if (body == null)
		{
			Debug.LogError("Object with blueprint " + Blueprint + " had no " + Slot + " body part to equip on");
		}
		else
		{
			EquipObject(Object, firstPart, Silent);
		}
	}

	public void ForceEquipObject(GameObject Object, BodyPart Part, bool Silent = false, int? EnergyCost = null)
	{
		Event @event = Event.New("CommandForceEquipObject", "Object", Object, "BodyPart", Part);
		if (EnergyCost.HasValue)
		{
			@event.SetParameter("EnergyCost", EnergyCost.Value);
		}
		if (Silent)
		{
			@event.SetSilent(Silent);
		}
		FireEvent(@event);
	}

	public void ForceEquipObject(GameObject Object, string Slot, bool Silent = false, int? EnergyCost = null)
	{
		Body body = Body;
		if (body == null)
		{
			Debug.LogError("Object with blueprint " + Blueprint + " had no body to equip on");
			return;
		}
		BodyPart firstPart = body.GetFirstPart(Slot);
		if (body == null)
		{
			Debug.LogError("Object with blueprint " + Blueprint + " had no " + Slot + " body part to equip on");
		}
		else
		{
			ForceEquipObject(Object, firstPart, Silent, EnergyCost);
		}
	}

	public bool IsNonStackableFromParts()
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (!PartsList[i].SameAs(PartsList[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsStackable()
	{
		if (HasIntProperty("NeverStack"))
		{
			return false;
		}
		if (IsNonStackableFromParts())
		{
			return false;
		}
		return true;
	}

	public bool PartsPreventGeneratingStacked()
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (!PartsList[i].CanGenerateStacked())
			{
				return true;
			}
		}
		return false;
	}

	public bool CanGenerateStacked()
	{
		if (HasIntProperty("NeverStack"))
		{
			return false;
		}
		if (HasTag("AlwaysStack"))
		{
			return true;
		}
		if (PartsPreventGeneratingStacked())
		{
			return false;
		}
		string tag = GetTag("Mods");
		if (!string.IsNullOrEmpty(tag) && tag != "None")
		{
			return false;
		}
		return true;
	}

	public void TakePopulation(string population)
	{
		foreach (PopulationResult item in PopulationManager.Generate(population))
		{
			for (int i = 0; i < item.Number; i++)
			{
				TakeObject(item.Blueprint, Silent: false, 0);
			}
		}
	}

	public int TakeObjectsFromEncounterTable(string Table, int Number, bool Silent = false, int? EnergyCost = 0, int BonusModChance = 0, int SetModNumber = 0, string Context = null, List<GameObject> Tracking = null)
	{
		Dictionary<string, int> dictionary;
		if (TakeObjectsFromTableGenerationInUse)
		{
			dictionary = new Dictionary<string, int>();
		}
		else
		{
			TakeObjectsFromTableGenerationInUse = true;
			dictionary = TakeObjectsFromTableGeneration;
			dictionary.Clear();
		}
		for (int i = 0; i < Number; i++)
		{
			string text = EncounterFactory.Factory.RollOneStringFromTable(Table, Context);
			if (!string.IsNullOrEmpty(text))
			{
				if (dictionary.ContainsKey(text))
				{
					dictionary[text]++;
				}
				else
				{
					dictionary.Add(text, 1);
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			int num2 = num;
			string key = item.Key;
			int value = item.Value;
			int bonusModChance = BonusModChance;
			num = num2 + TakeObject(key, value, Silent, EnergyCost, Context, bonusModChance, SetModNumber, Tracking);
		}
		if (dictionary == TakeObjectsFromTableGeneration)
		{
			dictionary.Clear();
			TakeObjectsFromTableGenerationInUse = false;
		}
		return num;
	}

	public int TakeObjectsFromPopulation(string Table, int Number, Dictionary<string, string> Variables = null, bool Silent = false, int? EnergyCost = 0, int BonusModChance = 0, int SetModNumber = 0, string Context = null, List<GameObject> Tracking = null)
	{
		Dictionary<string, int> dictionary;
		if (TakeObjectsFromTableGenerationInUse)
		{
			dictionary = new Dictionary<string, int>();
		}
		else
		{
			TakeObjectsFromTableGenerationInUse = true;
			dictionary = TakeObjectsFromTableGeneration;
			dictionary.Clear();
		}
		for (int i = 0; i < Number; i++)
		{
			string blueprint = PopulationManager.RollOneFrom(Table, Variables).Blueprint;
			if (!string.IsNullOrEmpty(blueprint))
			{
				if (dictionary.ContainsKey(blueprint))
				{
					dictionary[blueprint]++;
				}
				else
				{
					dictionary.Add(blueprint, 1);
				}
			}
		}
		int num = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			int num2 = num;
			string key = item.Key;
			int value = item.Value;
			int bonusModChance = BonusModChance;
			num = num2 + TakeObject(key, value, Silent, EnergyCost, Context, bonusModChance, SetModNumber, Tracking);
		}
		if (dictionary == TakeObjectsFromTableGeneration)
		{
			dictionary.Clear();
			TakeObjectsFromTableGenerationInUse = false;
		}
		return num;
	}

	public bool TakeObjectFromPopulation(string Table, Dictionary<string, string> Variables = null, bool Silent = false, int? EnergyCost = 0, int BonusModChance = 0, int SetModNumber = 0, string Context = null, List<GameObject> Tracking = null)
	{
		string blueprint = PopulationManager.RollOneFrom(Table, Variables).Blueprint;
		return TakeObject(blueprint, Silent, EnergyCost, BonusModChance, SetModNumber, Context, Tracking);
	}

	public int TakeObject(List<GameObject> GOList, bool Silent = false, int? EnergyCost = 0, string Context = null, List<GameObject> Tracking = null)
	{
		int num = 0;
		foreach (GameObject GO in GOList)
		{
			if (TakeObject(GO, Silent, EnergyCost, Context, Tracking))
			{
				num++;
			}
		}
		return num;
	}

	public int TakeObject(string Blueprint, int Number, bool Silent = false, int? EnergyCost = 0, string Context = null, int BonusModChance = 0, int SetModNumber = 0, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		if (Number <= 0)
		{
			return 0;
		}
		GameObject gameObject = create(Blueprint, BonusModChance, SetModNumber, Context, beforeObjectCreated, afterObjectCreated);
		if (Number > 1 && gameObject.CanGenerateStacked() && gameObject.GetPart("Stacker") is Stacker stacker && stacker.StackCount == 1)
		{
			stacker.StackCount = Number;
			if (!TakeObject(gameObject, Silent, EnergyCost, Context, Tracking))
			{
				return 0;
			}
			return Number;
		}
		int num = 0;
		for (int i = 0; i < Number; i++)
		{
			if (TakeObject(Blueprint, Silent, EnergyCost, BonusModChance, SetModNumber, Context, Tracking, beforeObjectCreated, afterObjectCreated))
			{
				num++;
			}
		}
		return num;
	}

	public bool TakeObject(string Blueprint, bool Silent = false, int? EnergyCost = 0, int BonusModChance = 0, int SetModNumber = 0, string Context = null, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		return TakeObject(create(Blueprint, BonusModChance, SetModNumber, Context, beforeObjectCreated, afterObjectCreated), Silent, EnergyCost, Context, Tracking);
	}

	public bool TakeObject(string Blueprint, out GameObject obj, bool Silent = false, int? EnergyCost = 0, int BonusModChance = 0, int SetModNumber = 0, string Context = null, List<GameObject> Tracking = null, Action<GameObject> beforeObjectCreated = null, Action<GameObject> afterObjectCreated = null)
	{
		obj = create(Blueprint, BonusModChance, SetModNumber, Context, beforeObjectCreated, afterObjectCreated);
		return TakeObject(obj, Silent, EnergyCost, Context, Tracking);
	}

	public bool TakeObject(GameObject GO, bool Silent = false, int? EnergyCost = 0, string Context = null, List<GameObject> Tracking = null)
	{
		Event.PinCurrentPool();
		Event @event;
		if (!EnergyCost.HasValue)
		{
			@event = (Silent ? eCommandTakeObjectSilent : eCommandTakeObject);
		}
		else
		{
			@event = (Silent ? eCommandTakeObjectSilentWithEnergyCost : eCommandTakeObjectWithEnergyCost);
			@event.SetParameter("EnergyCost", EnergyCost.Value);
		}
		@event.SetParameter("Object", GO);
		@event.SetParameter("Context", Context);
		bool num = FireEvent(@event);
		Event.ResetToPin();
		if (num)
		{
			Tracking?.Add(GO);
		}
		return num;
	}

	public bool ReceiveObject(GameObject GO)
	{
		return TakeObject(GO, Silent: true, 0);
	}

	public bool ReceiveObject(string Blueprint)
	{
		return TakeObject(Blueprint, Silent: true, 0);
	}

	public int ReceiveObject(string Blueprint, int Number)
	{
		return TakeObject(Blueprint, Number, Silent: true, 0);
	}

	public bool ForceApplyEffect(Effect E, GameObject Owner = null)
	{
		if (!ForceApplyEffectEvent.Check(this, E.ClassName, E))
		{
			return ApplyEffect(E);
		}
		ApplyEffectEvent.Check(this, E.ClassName, E, Owner);
		if (!E.CanApplyToStack() && !HasTag("AlwaysStack"))
		{
			SplitStack(1, null, NoRemove: true);
		}
		E.Object = this;
		if (E.Apply(this))
		{
			E.Register(this);
			Effects.Add(E);
			EffectForceAppliedEvent.Send(this, E.ClassName, E);
			EffectAppliedEvent.Send(this, E.ClassName, E);
			CheckStack();
			return true;
		}
		CheckStack();
		return false;
	}

	public bool RenderTile(ConsoleChar Char)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Count; i++)
			{
				if (!_Effects[i].RenderTile(Char))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			if (!PartsList[j].RenderTile(Char))
			{
				return false;
			}
		}
		return true;
	}

	public RenderEvent RenderForUI()
	{
		if (pRender == null)
		{
			return null;
		}
		if (_contextRender == null)
		{
			_contextRender = new RenderEvent();
		}
		_contextRender.RenderString = pRender.RenderString;
		if (!string.IsNullOrEmpty(pRender.TileColor) && Options.UseTiles)
		{
			_contextRender.ColorString = pRender.TileColor;
		}
		else
		{
			_contextRender.ColorString = pRender.ColorString;
		}
		_contextRender.DetailColor = pRender.DetailColor;
		_contextRender.BackgroundString = pRender.GetBackgroundColor();
		_contextRender.HighestLayer = pRender.RenderLayer;
		_contextRender.Tile = pRender.Tile;
		_contextRender.WantsToPaint = false;
		Render(_contextRender);
		FinalRender(_contextRender, bAlt: false);
		return _contextRender;
	}

	public void Paint(ScreenBuffer buf)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Count; i++)
			{
				_Effects[i].OnPaint(buf);
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			PartsList[j].OnPaint(buf);
		}
	}

	public bool Render(RenderEvent E)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Count; i++)
			{
				if (!_Effects[i].Render(E))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			if (!PartsList[j].Render(E))
			{
				return false;
			}
		}
		return true;
	}

	public bool OverlayRender(RenderEvent E)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Count; i++)
			{
				if (!_Effects[i].OverlayRender(E))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			if (!PartsList[j].OverlayRender(E))
			{
				return false;
			}
		}
		return true;
	}

	public bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (_Effects != null)
		{
			for (int i = 0; i < _Effects.Count; i++)
			{
				if (!_Effects[i].FinalRender(E, bAlt))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			if (!PartsList[j].FinalRender(E, bAlt))
			{
				return false;
			}
		}
		return true;
	}

	public bool TakeDamage(ref int Amount, string Attributes = null, string DeathReason = null, string ThirdPersonDeathReason = null, GameObject Owner = null, GameObject Attacker = null, GameObject Source = null, GameObject Perspective = null, string Message = "from %t attack.", bool Accidental = false, bool Environmental = false, bool Indirect = false, bool ShowUninvolved = false, bool ShowForInanimate = false, bool SilentIfNoDamage = false, int Phase = 5, string ShowDamageType = null)
	{
		if (Phase == 0 && Attacker != null)
		{
			Phase = Attacker.GetPhase();
		}
		Damage damage = new Damage(Amount);
		damage.AddAttributes(Attributes);
		if (Accidental)
		{
			damage.AddAttribute("Accidental");
		}
		if (Environmental)
		{
			damage.AddAttribute("Environmental");
		}
		Event @event = Event.New("TakeDamage");
		@event.SetParameter("Damage", damage);
		@event.SetParameter("Owner", Owner ?? Attacker);
		@event.SetParameter("Attacker", Attacker);
		@event.SetParameter("Source", Source);
		@event.SetParameter("Perspective", Perspective);
		@event.SetParameter("Phase", Phase);
		if (!string.IsNullOrEmpty(Message))
		{
			@event.SetParameter("Message", Message);
		}
		if (!string.IsNullOrEmpty(DeathReason))
		{
			@event.SetParameter("DeathReason", DeathReason);
		}
		if (!string.IsNullOrEmpty(ThirdPersonDeathReason))
		{
			@event.SetParameter("ThirdPersonDeathReason", ThirdPersonDeathReason);
		}
		if (ShowDamageType != null)
		{
			@event.SetParameter("ShowDamageType", ShowDamageType);
		}
		if (Indirect)
		{
			@event.SetFlag("Indirect", State: true);
		}
		if (ShowForInanimate)
		{
			@event.SetFlag("ShowForInanimate", State: true);
		}
		if (ShowUninvolved)
		{
			@event.SetFlag("ShowUninvolved", State: true);
		}
		if (SilentIfNoDamage)
		{
			@event.SetFlag("SilentIfNoDamage", State: true);
		}
		bool result = FireEvent(@event);
		Amount = damage.Amount;
		return result;
	}

	public bool TakeDamage(int Amount, string Message, string Attributes = null, string DeathReason = null, string ThirdPersonDeathReason = null, GameObject Owner = null, GameObject Attacker = null, GameObject Source = null, GameObject Perspective = null, bool Accidental = false, bool Environmental = false, bool Indirect = false, bool ShowUninvolved = false, bool ShowForInanimate = false, bool SilentIfNoDamage = false, int Phase = 5, string ShowDamageType = null)
	{
		return TakeDamage(ref Amount, Attributes, DeathReason, ThirdPersonDeathReason, Owner, Attacker, Source, Perspective, Message, Accidental, Environmental, Indirect, ShowUninvolved, ShowForInanimate, SilentIfNoDamage, Phase, ShowDamageType);
	}

	public bool TakeDamage(int Amount, StringBuilder Message, string Attributes = null, string DeathReason = null, string ThirdPersonDeathReason = null, GameObject Owner = null, GameObject Attacker = null, GameObject Source = null, GameObject Perspective = null, bool Accidental = false, bool Environmental = false, bool Indirect = false, bool ShowUninvolved = false, bool ShowForInanimate = false, bool SilentIfNoDamage = false, int Phase = 5, string ShowDamageType = null)
	{
		if (Message != null)
		{
			return TakeDamage(ref Amount, Attributes, DeathReason, ThirdPersonDeathReason, Owner, Attacker, Source, Perspective, Message.ToString(), Accidental, Environmental, Indirect, ShowUninvolved, ShowForInanimate, SilentIfNoDamage, Phase, ShowDamageType);
		}
		return TakeDamage(ref Amount, Attributes, DeathReason, ThirdPersonDeathReason, Owner, Attacker, Source, null, "from %t attack.", Accidental, Environmental, Indirect, ShowUninvolved, ShowForInanimate, SilentIfNoDamage, Phase, ShowDamageType);
	}

	public bool TakeDamage(int DamageAmount, GameObject FromAttacker, string ShowMessage)
	{
		return TakeDamage(DamageAmount, ShowMessage, null, null, null, null, FromAttacker);
	}

	public bool ApplyEffect(Effect E, GameObject Owner = null)
	{
		if (HasTag("NoEffects") && GetIntProperty("ForceEffects") == 0)
		{
			return false;
		}
		if (ApplyEffectEvent.Check(this, E.ClassName, E, Owner))
		{
			if (!E.CanApplyToStack() && !HasTag("AlwaysStack"))
			{
				SplitStack(1, null, NoRemove: true);
			}
			E.Object = this;
			if (E.Apply(this))
			{
				E.Register(this);
				Effects.Add(E);
				EffectAppliedEvent.Send(this, E.ClassName, E);
				CheckStack();
				return true;
			}
			CheckStack();
		}
		return false;
	}

	public bool CheckStack()
	{
		return GetPart<Stacker>()?.Check() ?? false;
	}

	public bool RemoveEffect(Effect E, bool NeedStackCheck)
	{
		if (FireEvent(Event.New("RemoveEffect", "Effect", E)))
		{
			E.Remove(this);
			E.Unregister(this);
			Effects.Remove(E);
			E.Object = null;
			EffectRemovedEvent.Send(this, E.ClassName, E);
			if (NeedStackCheck)
			{
				CheckStack();
			}
			return true;
		}
		return false;
	}

	public bool RemoveEffect(Effect E)
	{
		return RemoveEffect(E, NeedStackCheck: true);
	}

	public bool RemoveEffect(string Name, bool WarnIfNotFound = false)
	{
		foreach (Effect effect in Effects)
		{
			if ((effect.ClassName.EqualsNoCase(Name) || effect.DisplayName.EqualsNoCase(Name)) && RemoveEffect(effect))
			{
				return true;
			}
		}
		foreach (Effect effect2 in Effects)
		{
			if (effect2.DisplayName.Contains(Name, CompareOptions.IgnoreCase) && RemoveEffect(effect2))
			{
				return true;
			}
		}
		if (WarnIfNotFound)
		{
			Debug.LogWarning("Found no effect " + Name + " for remove");
		}
		return false;
	}

	public bool RemoveEffect(Type EffectType)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.GetType() == EffectType && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffect(Predicate<Effect> filter)
	{
		foreach (Effect effect in Effects)
		{
			if (filter(effect) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffect(string Name, Predicate<Effect> filter)
	{
		foreach (Effect effect in Effects)
		{
			if ((effect.ClassName.EqualsNoCase(Name) || effect.DisplayName.EqualsNoCase(Name)) && filter(effect) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffect(Type EffectType, Predicate<Effect> filter)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.GetType() == EffectType && filter(effect) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffectByClass(string Name)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.ClassName.EqualsNoCase(Name) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffectByClass(string Name, Predicate<Effect> filter)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.ClassName.EqualsNoCase(Name) && filter(effect) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffectByExactClass(string Name)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.ClassName == Name && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveEffectByExactClass(string Name, Predicate<Effect> filter)
	{
		foreach (Effect effect in Effects)
		{
			if (effect.ClassName == Name && filter(effect) && RemoveEffect(effect))
			{
				return true;
			}
		}
		return false;
	}

	public int RemoveAllEffects(string Name)
	{
		int num = 0;
		while (RemoveEffect(Name) && ++num < 10000)
		{
			num++;
		}
		return num;
	}

	public GameObject FindObjectInInventory(string Blueprint)
	{
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			foreach (GameObject item in inventory.GetObjectsDirect())
			{
				if (item.Blueprint == Blueprint)
				{
					return item;
				}
			}
		}
		Body body = Body;
		if (body != null)
		{
			foreach (BodyPart item2 in body.LoopParts())
			{
				if (item2.Equipped != null && item2.Equipped.Blueprint == Blueprint)
				{
					return item2.Equipped;
				}
			}
		}
		return null;
	}

	public bool HasObjectInInventory(Func<GameObject, bool> test, int n = 1)
	{
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			foreach (GameObject item in inventory.GetObjectsDirect())
			{
				if (test(item))
				{
					n = ((!(item.GetPart("Stacker") is Stacker stacker)) ? (n - 1) : (n - stacker.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		Body body = Body;
		if (body != null)
		{
			foreach (BodyPart item2 in body.LoopParts())
			{
				if (item2.Equipped != null && test(item2.Equipped))
				{
					n = ((!(item2.Equipped.GetPart("Stacker") is Stacker stacker2)) ? (n - 1) : (n - stacker2.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectInInventory(string Blueprint, int n = 1)
	{
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			foreach (GameObject item in inventory.GetObjectsDirect())
			{
				if (item.Blueprint == Blueprint)
				{
					n = ((!(item.GetPart("Stacker") is Stacker stacker)) ? (n - 1) : (n - stacker.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		Body body = Body;
		if (body != null)
		{
			foreach (BodyPart item2 in body.LoopParts())
			{
				if (item2.Equipped != null && item2.Equipped.Blueprint == Blueprint)
				{
					n = ((!(item2.Equipped.GetPart("Stacker") is Stacker stacker2)) ? (n - 1) : (n - stacker2.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectWithPartInDirection(string part, string direction)
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null)
		{
			return false;
		}
		return currentCell.GetCellFromDirection(direction)?.HasObjectWithPart(part) ?? false;
	}

	public bool HasObjectEquippedOrDefault(string Blueprint, int n = 1)
	{
		Body body = Body;
		if (body != null)
		{
			foreach (BodyPart item in body.LoopParts())
			{
				if (item.Equipped != null && item.Equipped.Blueprint == Blueprint && item.Equipped.IsEquippedProperly())
				{
					n = ((!(item.Equipped.GetPart("Stacker") is Stacker stacker)) ? (n - 1) : (n - stacker.Number));
				}
				if (item.DefaultBehavior != null && item.DefaultBehavior.Blueprint == Blueprint)
				{
					n = ((!(item.DefaultBehavior.GetPart("Stacker") is Stacker stacker2)) ? (n - 1) : (n - stacker2.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObjectEquipped(string Blueprint, int n = 1)
	{
		Body body = Body;
		if (body != null)
		{
			foreach (BodyPart item in body.LoopParts())
			{
				if (item.Equipped != null && item.Equipped.Blueprint == Blueprint && item.Equipped.IsEquippedProperly())
				{
					n = ((!(item.Equipped.GetPart("Stacker") is Stacker stacker)) ? (n - 1) : (n - stacker.Number));
				}
				if (n <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void UseObject(Predicate<GameObject> test)
	{
		GameObject gameObject = Inventory?.FindObject(test);
		if (gameObject != null)
		{
			gameObject.Destroy();
		}
		else
		{
			(Body?.FindObject(test))?.Destroy();
		}
	}

	public void UseObject(string Blueprint)
	{
		GameObject gameObject = Inventory?.FindObjectByBlueprint(Blueprint);
		if (gameObject != null)
		{
			gameObject.Destroy();
		}
		else
		{
			(Body?.FindObjectByBlueprint(Blueprint))?.Destroy();
		}
	}

	public double GetWeight()
	{
		return pPhysics?.GetWeight() ?? 0.0;
	}

	public int GetWeightTimes(double Factor)
	{
		return pPhysics?.GetWeightTimes(Factor) ?? 0;
	}

	public double GetIntrinsicWeight()
	{
		return pPhysics?.GetIntrinsicWeight() ?? 0.0;
	}

	public int GetIntrinsicWeightTimes(double Factor)
	{
		return pPhysics?.GetIntrinsicWeightTimes(Factor) ?? 0;
	}

	public int GetCarriedWeight()
	{
		int num = 0;
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			num += inventory.GetWeight();
		}
		Body body = Body;
		if (body != null)
		{
			num += body.GetWeight();
		}
		return num;
	}

	public int GetMaxCarriedWeight()
	{
		return GetMaxCarriedWeightEvent.GetFor(this, Stat("Strength") * 15);
	}

	public string GetCachedDisplayNameStripped()
	{
		if (_CachedStrippedName == null)
		{
			return DisplayNameStripped;
		}
		return _CachedStrippedName;
	}

	public void ResetNameCache()
	{
		_CachedStrippedName = null;
	}

	public string GetDisplayName(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool ColorOnly = false, bool Visible = true, bool WithoutEpithet = false, bool Short = false, bool BaseOnly = false, bool WithIndefiniteArticle = false, bool WithDefiniteArticle = false, string DefaultDefiniteArticle = null, bool IndicateHidden = false, bool Capitalize = false)
	{
		if (Short)
		{
			Cutoff = 1040;
		}
		string text = GetDisplayNameEvent.GetFor(this, Base ?? DisplayNameBase, Cutoff, Context, AsIfKnown, Single, NoConfusion, NoColor || Stripped, ColorOnly, Visible, BaseOnly);
		if (WithoutEpithet && text.Contains(","))
		{
			text = ConsoleLib.Console.ColorUtility.ClipToFirstExceptFormatting(text, ',');
		}
		if (Stripped)
		{
			text = text.Strip();
		}
		if (IndicateHidden && IsHidden)
		{
			text = "hidden " + text;
			if (WithDefiniteArticle)
			{
				WithDefiniteArticle = false;
				WithIndefiniteArticle = true;
			}
		}
		if (WithIndefiniteArticle)
		{
			text = IndefiniteArticle(Capitalize, text, BaseOnly) + text;
			Capitalize = false;
		}
		if (WithDefiniteArticle)
		{
			text = DefiniteArticle(Capitalize, text, BaseOnly, DefaultDefiniteArticle) + text;
			Capitalize = false;
		}
		if (Capitalize)
		{
			text = ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(text);
		}
		return text;
	}

	public string an(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool IndicateHidden = false)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden);
	}

	public string An(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool IndicateHidden = false)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle: true, WithDefiniteArticle: false, null, IndicateHidden, Capitalize: true);
	}

	public string t(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, string DefaultDefiniteArticle = null, bool IndicateHidden = false)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle: false, WithDefiniteArticle: true, DefaultDefiniteArticle, IndicateHidden);
	}

	public string T(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, string DefaultDefiniteArticle = null, bool IndicateHidden = false)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle: false, WithDefiniteArticle: true, DefaultDefiniteArticle, IndicateHidden, Capitalize: true);
	}

	public string one(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool WithIndefiniteArticle = false, string DefaultDefiniteArticle = null, bool IndicateHidden = true)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle, !WithIndefiniteArticle, DefaultDefiniteArticle, IndicateHidden);
	}

	public string One(int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool WithIndefiniteArticle = false, string DefaultDefiniteArticle = null, bool IndicateHidden = true)
	{
		return GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle, !WithIndefiniteArticle, DefaultDefiniteArticle, IndicateHidden, Capitalize: true);
	}

	public string does(string Verb, int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool WithIndefiniteArticle = false, string DefaultDefiniteArticle = null, bool IndicateHidden = false, bool Pronoun = false)
	{
		string displayName;
		if (!Pronoun)
		{
			bool withDefiniteArticle = !WithIndefiniteArticle;
			displayName = GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle, withDefiniteArticle, DefaultDefiniteArticle, IndicateHidden);
		}
		else
		{
			displayName = it;
		}
		string text = displayName;
		string verb = GetVerb(Verb, PrependSpace: true, Pronoun);
		if (text.Contains(","))
		{
			return text + "," + verb;
		}
		return text + verb;
	}

	public string Does(string Verb, int Cutoff = int.MaxValue, string Base = null, string Context = null, bool AsIfKnown = false, bool Single = false, bool NoConfusion = false, bool NoColor = false, bool Stripped = false, bool WithoutEpithet = false, bool Short = true, bool BaseOnly = false, bool WithIndefiniteArticle = false, string DefaultDefiniteArticle = null, bool IndicateHidden = false, bool Pronoun = false)
	{
		string displayName;
		if (!Pronoun)
		{
			bool withDefiniteArticle = !WithIndefiniteArticle;
			displayName = GetDisplayName(Cutoff, Base, Context, AsIfKnown, Single, NoConfusion, NoColor, Stripped, ColorOnly: false, Visible: true, WithoutEpithet, Short, BaseOnly, WithIndefiniteArticle, withDefiniteArticle, DefaultDefiniteArticle, IndicateHidden, Capitalize: true);
		}
		else
		{
			displayName = It;
		}
		string text = displayName;
		string verb = GetVerb(Verb, PrependSpace: true, Pronoun);
		if (text.Contains(","))
		{
			return text + "," + verb;
		}
		return text + verb;
	}

	public Gender GetGender(bool AsIfKnown = false)
	{
		if (GenderName == null)
		{
			if (!IsOriginalPlayerBody())
			{
				GenderName = GetPropertyOrTag("Gender");
				if (string.IsNullOrEmpty(GenderName))
				{
					string propertyOrTag = GetPropertyOrTag("RandomGender");
					if (!string.IsNullOrEmpty(propertyOrTag))
					{
						GenderName = propertyOrTag.Split(',').GetRandomElement();
					}
					GenderName = Gender.CheckSpecial(GenderName);
				}
			}
			if (GenderName == null)
			{
				string text = ((pBrain == null) ? "neuter" : "nonspecific");
				GenderName = (Gender.Exists(text) ? text : Gender.GetAllGenericPersonalSingular()[0].Name);
			}
		}
		return Gender.Get(GetGenderEvent.GetFor(this, GenderName, AsIfKnown));
	}

	public void SetGender(Gender Spec)
	{
		GenderName = Spec.Name;
	}

	public void SetGender(string Name)
	{
		SetGender(Gender.Get(Gender.CheckSpecial(Name)));
	}

	public PronounSet GetPronounSet()
	{
		if (PronounSetName == "")
		{
			return null;
		}
		if (PronounSetName == null)
		{
			PronounSetName = GetPropertyOrTag("PronounSet");
			if (string.IsNullOrEmpty(PronounSetName))
			{
				string propertyOrTag = GetPropertyOrTag("RandomPronounSet");
				if (!string.IsNullOrEmpty(propertyOrTag))
				{
					string propertyOrTag2 = GetPropertyOrTag("RandomPronounSetChance");
					if (string.IsNullOrEmpty(propertyOrTag2) || Convert.ToInt32(propertyOrTag2).in100())
					{
						PronounSetName = propertyOrTag.Split(',').GetRandomElement();
					}
					PronounSetName = PronounSet.CheckSpecial(PronounSetName);
				}
				if (PronounSetName == null)
				{
					PronounSetName = "";
					return null;
				}
			}
		}
		return PronounSet.Get(PronounSetName);
	}

	public void SetPronounSet(PronounSet Spec)
	{
		PronounSetName = Spec?.Name;
		PronounSetKnown = false;
	}

	public void SetPronounSet(string Name)
	{
		SetPronounSet(PronounSet.Get(PronounSet.CheckSpecial(Name)));
	}

	public void ClearPronounSet()
	{
		PronounSetName = null;
		PronounSetKnown = false;
	}

	public bool IsPronounSetKnown()
	{
		if (PronounSetKnown)
		{
			return true;
		}
		if (!PronounSet.EnableConversationalExchange)
		{
			return true;
		}
		if (IsPlayer())
		{
			return true;
		}
		if (WasPlayer())
		{
			return true;
		}
		if (HasCopyRelationship(ThePlayer))
		{
			return true;
		}
		if (XRLCore.Core == null || XRLCore.Core.Game == null || XRLCore.Core.Game.Player.Body == null)
		{
			return true;
		}
		if (XRLCore.Core.Game.Player.Body.HasSkill("Customs_Tactful"))
		{
			return true;
		}
		return false;
	}

	public PronounSet GetPronounSetIfKnown()
	{
		if (!IsPronounSetKnown())
		{
			return null;
		}
		return GetPronounSet();
	}

	public IPronounProvider GetPronounProvider(bool AsIfKnown = false)
	{
		IPronounProvider pronounSetIfKnown = GetPronounSetIfKnown();
		return pronounSetIfKnown ?? GetGender(AsIfKnown);
	}

	public string Its_(GameObject obj)
	{
		if (obj.HasProperName)
		{
			return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(obj.ShortDisplayName);
		}
		return Its + " " + obj.ShortDisplayName;
	}

	public void Its_(GameObject obj, StringBuilder AppendTo)
	{
		if (obj.HasProperName)
		{
			AppendTo.Append(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(obj.ShortDisplayName));
		}
		else
		{
			AppendTo.Append(Its).Append(' ').Append(obj.ShortDisplayName);
		}
	}

	public string Poss(GameObject obj, bool Definite = true)
	{
		if (obj.HasProperName)
		{
			return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(obj.ShortDisplayName);
		}
		if (IsPlayer())
		{
			return "Your " + obj.ShortDisplayName;
		}
		if (HasProperName)
		{
			return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ShortDisplayName)) + " " + obj.ShortDisplayName;
		}
		return Grammar.MakePossessive((Definite ? "The " : A) + ShortDisplayName) + " " + obj.ShortDisplayName;
	}

	public void Poss(GameObject obj, StringBuilder AppendTo, bool Definite = true)
	{
		if (obj.HasProperName)
		{
			AppendTo.Append(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(obj.ShortDisplayName));
		}
		else if (IsPlayer())
		{
			AppendTo.Append("Your ").Append(obj.ShortDisplayName);
		}
		else if (HasProperName)
		{
			AppendTo.Append(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ShortDisplayName))).Append(' ').Append(obj.ShortDisplayName);
		}
		else
		{
			AppendTo.Append(Definite ? "The " : A).Append(Grammar.MakePossessive(ShortDisplayName)).Append(' ')
				.Append(obj.ShortDisplayName);
		}
	}

	public string Poss(string text, bool Definite = true)
	{
		if (IsPlayer())
		{
			return "Your " + text;
		}
		if (HasProperName)
		{
			return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ShortDisplayName)) + " " + text;
		}
		return Grammar.MakePossessive((Definite ? "The " : A) + ShortDisplayName) + " " + text;
	}

	public string its_(GameObject obj)
	{
		if (obj.HasProperName)
		{
			return obj.ShortDisplayName;
		}
		return its + " " + obj.ShortDisplayName;
	}

	public void its_(GameObject obj, StringBuilder AppendTo)
	{
		if (obj.HasProperName)
		{
			AppendTo.Append(obj.ShortDisplayName);
		}
		else
		{
			AppendTo.Append(its).Append(' ').Append(obj.ShortDisplayName);
		}
	}

	public string poss(GameObject obj, bool Definite = true)
	{
		if (obj.HasProperName)
		{
			return obj.ShortDisplayName;
		}
		if (IsPlayer())
		{
			return "your " + obj.ShortDisplayName;
		}
		if (HasProperName)
		{
			return Grammar.MakePossessive(ShortDisplayName) + " " + obj.ShortDisplayName;
		}
		return Grammar.MakePossessive((Definite ? "the " : a) + " " + ShortDisplayName) + " " + obj.ShortDisplayName;
	}

	public void poss(GameObject obj, StringBuilder AppendTo, bool Definite = true)
	{
		if (obj.HasProperName)
		{
			AppendTo.Append(obj.ShortDisplayName);
		}
		else if (IsPlayer())
		{
			AppendTo.Append("your ").Append(obj.ShortDisplayName);
		}
		else if (HasProperName)
		{
			AppendTo.Append(Grammar.MakePossessive(ShortDisplayName)).Append(' ').Append(obj.ShortDisplayName);
		}
		else
		{
			AppendTo.Append(Definite ? "the " : a).Append(Grammar.MakePossessive(ShortDisplayName)).Append(' ')
				.Append(obj.ShortDisplayName);
		}
	}

	public string poss(string text, bool Definite = true)
	{
		if (IsPlayer())
		{
			return "your " + text;
		}
		if (HasProperName)
		{
			return Grammar.MakePossessive(ShortDisplayName) + " " + text;
		}
		return Grammar.MakePossessive((Definite ? "the " : a) + ShortDisplayName) + " " + text;
	}

	public string IndefiniteArticle(bool capital = false, string word = null, bool forBase = false)
	{
		string propertyOrTag = GetPropertyOrTag("IndefiniteArticle");
		if (!string.IsNullOrEmpty(propertyOrTag) && Understood())
		{
			return (capital ? Grammar.InitCap(propertyOrTag) : propertyOrTag) + " ";
		}
		if (HasProperName)
		{
			return "";
		}
		string text = GetxTag("Grammar", "iArticle");
		if (HasPropertyOrTag("OverrideIArticle"))
		{
			text = GetPropertyOrTag("OverrideIArticle");
		}
		if (!string.IsNullOrEmpty(text) && Understood())
		{
			return (capital ? Grammar.InitCap(text) : text) + " ";
		}
		if (IsPlural)
		{
			if (!capital)
			{
				return "some ";
			}
			return "Some ";
		}
		if (word == null)
		{
			word = (forBase ? BaseDisplayName : ShortDisplayName);
		}
		if (!Grammar.IndefiniteArticleShouldBeAn(word))
		{
			if (!capital)
			{
				return "a ";
			}
			return "A ";
		}
		if (!capital)
		{
			return "an ";
		}
		return "An ";
	}

	public void IndefiniteArticle(StringBuilder SB, bool capital = false, string word = null, bool forBase = false)
	{
		string propertyOrTag = GetPropertyOrTag("IndefiniteArticle");
		if (!string.IsNullOrEmpty(propertyOrTag) && Understood())
		{
			SB.Append(capital ? Grammar.InitCap(propertyOrTag) : propertyOrTag).Append(' ');
		}
		else
		{
			if (HasProperName)
			{
				return;
			}
			string text = GetxTag("Grammar", "iArticle");
			if (HasPropertyOrTag("OverrideIArticle"))
			{
				text = GetPropertyOrTag("OverrideIArticle");
			}
			if (!string.IsNullOrEmpty(text) && Understood())
			{
				SB.Append(capital ? Grammar.InitCap(text) : text).Append(' ');
				return;
			}
			if (IsPlural)
			{
				SB.Append(capital ? "Some " : "some ");
				return;
			}
			if (word == null)
			{
				word = (forBase ? BaseDisplayName : ShortDisplayName);
			}
			SB.Append((!Grammar.IndefiniteArticleShouldBeAn(word)) ? (capital ? "A " : "a ") : (capital ? "An " : "an "));
		}
	}

	public string DefiniteArticle(bool capital = false, string word = null, bool forBase = false, string useAsDefault = null)
	{
		string propertyOrTag = GetPropertyOrTag("DefiniteArticle");
		if (!string.IsNullOrEmpty(propertyOrTag) && Understood())
		{
			return (capital ? Grammar.InitCap(propertyOrTag) : propertyOrTag) + " ";
		}
		if (HasProperName)
		{
			return "";
		}
		string text = GetxTag("Grammar", "dArticle");
		if (!string.IsNullOrEmpty(text) && Understood())
		{
			return (capital ? Grammar.InitCap(text) : text) + " ";
		}
		if (useAsDefault != null)
		{
			if (useAsDefault == "")
			{
				return "";
			}
			return useAsDefault + " ";
		}
		if (!capital)
		{
			return "the ";
		}
		return "The ";
	}

	public void DefiniteArticle(StringBuilder SB, bool capital = false, string word = null, bool forBase = false, string useAsDefault = null)
	{
		string propertyOrTag = GetPropertyOrTag("DefiniteArticle");
		if (!string.IsNullOrEmpty(propertyOrTag) && Understood())
		{
			SB.Append(capital ? Grammar.InitCap(propertyOrTag) : propertyOrTag).Append(' ');
		}
		else
		{
			if (HasProperName)
			{
				return;
			}
			string text = GetxTag("Grammar", "dArticle");
			if (!string.IsNullOrEmpty(text) && Understood())
			{
				SB.Append(capital ? Grammar.InitCap(text) : text).Append(' ');
			}
			else if (useAsDefault != null)
			{
				if (useAsDefault != "")
				{
					SB.Append(useAsDefault).Append(' ');
				}
			}
			else
			{
				SB.Append(capital ? "The " : "the ");
			}
		}
	}

	public string getSingularSemantic(string name, string defaultResult)
	{
		return Semantics.GetSingularSemantic(name, this, defaultResult);
	}

	public string getPluralSemantic(string name, string defaultResult)
	{
		return Semantics.GetPluralSemantic(name, this, defaultResult);
	}

	public bool Twiddle(Action After, bool Distant)
	{
		bool Done = false;
		EquipmentAPI.TwiddleObject(this, ref Done, After, Distant);
		return Done;
	}

	public bool Twiddle(Action After)
	{
		bool distant = ThePlayer.DistanceTo(this) > 1 || ThePlayer.IsFrozen();
		return Twiddle(After, distant);
	}

	public bool Twiddle()
	{
		return Twiddle(null);
	}

	public bool TelekineticTwiddle(Action After = null)
	{
		bool Done = false;
		EquipmentAPI.TwiddleObject(this, ref Done, After, Distant: true, TelekineticOnly: true);
		return Done;
	}

	public string GetVerb(string Verb, bool PrependSpace = true, bool PronounAntecedent = false)
	{
		if (IsPlural || IsPlayer())
		{
			if (!PrependSpace)
			{
				return Verb;
			}
			return " " + Verb;
		}
		if (PronounAntecedent && IsPseudoPlural)
		{
			if (!PrependSpace)
			{
				return Verb;
			}
			return " " + Verb;
		}
		return Grammar.ThirdPerson(Verb, PrependSpace);
	}

	public bool IsCombatObject(bool NoBrainOnly = false)
	{
		if (_isCombatObject == byte.MaxValue)
		{
			if (HasTagOrProperty("NoCombat"))
			{
				_isCombatObject = 0;
			}
			else if (HasPart("Combat"))
			{
				_isCombatObject = 2;
			}
			else if (HasPart("Brain"))
			{
				_isCombatObject = 1;
			}
			else
			{
				_isCombatObject = 0;
			}
		}
		return _isCombatObject > (NoBrainOnly ? 1 : 0);
	}

	public bool isFurniture()
	{
		return HasTag("Furniture");
	}

	public bool HasRegisteredEvent(string Event)
	{
		if (Event == "EndTurn" && WantsEndTurnEvent())
		{
			return true;
		}
		switch (Event)
		{
		case "TakeDamage":
			return true;
		case "BeforeDeathRemoval":
			return true;
		case "CommandAutoEquipObject":
			return true;
		case "Regenera":
			return true;
		default:
			if (RegisteredPartEvents != null && RegisteredPartEvents.ContainsKey(Event))
			{
				return true;
			}
			if (RegisteredEffectEvents != null && RegisteredEffectEvents.ContainsKey(Event))
			{
				return true;
			}
			return false;
		}
	}

	public bool HasRegisteredEventDirect(string Event)
	{
		if (RegisteredPartEvents != null && RegisteredPartEvents.ContainsKey(Event))
		{
			return true;
		}
		if (RegisteredEffectEvents != null && RegisteredEffectEvents.ContainsKey(Event))
		{
			return true;
		}
		return false;
	}

	public bool HasRegisteredEventFrom(string Event, IPart P)
	{
		if (RegisteredPartEvents != null && RegisteredPartEvents.ContainsKey(Event) && RegisteredPartEvents[Event].Contains(P))
		{
			return true;
		}
		return false;
	}

	public bool HasRegisteredEventFrom(string Event, Effect E)
	{
		if (RegisteredEffectEvents != null && RegisteredEffectEvents.ContainsKey(Event) && RegisteredEffectEvents[Event].Contains(E))
		{
			return true;
		}
		return false;
	}

	public void RegisterEffectEvent(Effect Ef, string Event)
	{
		if (RegisteredEffectEvents == null)
		{
			RegisteredEffectEvents = new Dictionary<string, List<Effect>>();
		}
		if (!RegisteredEffectEvents.ContainsKey(Event))
		{
			RegisteredEffectEvents.Add(Event, new List<Effect>());
		}
		RegisteredEffectEvents[Event].Add(Ef);
	}

	public void UnregisterEffectEvent(Effect Ef, string Event)
	{
		if (RegisteredEffectEvents != null && RegisteredEffectEvents.TryGetValue(Event, out var value))
		{
			for (int num = value.IndexOf(Ef); num >= 0; num = value.IndexOf(Ef))
			{
				value.RemoveAt(num);
			}
			if (value.Count == 0)
			{
				RegisteredEffectEvents.Remove(Event);
			}
		}
	}

	public void RegisterPartEvent(IPart Ef, string Event)
	{
		if (RegisteredPartEvents == null)
		{
			RegisteredPartEvents = new Dictionary<string, List<IPart>>();
		}
		if (!RegisteredPartEvents.TryGetValue(Event, out var value))
		{
			value = new List<IPart>(2);
			RegisteredPartEvents.Add(Event, value);
		}
		if (!value.Contains(Ef))
		{
			value.Add(Ef);
		}
	}

	public void UnregisterPartEvent(IPart Part, string Event)
	{
		if (RegisteredPartEvents != null && RegisteredPartEvents.TryGetValue(Event, out var value))
		{
			for (int num = value.IndexOf(Part); num >= 0; num = value.IndexOf(Part))
			{
				value.RemoveAt(num);
			}
			if (value.Count == 0)
			{
				RegisteredPartEvents.Remove(Event);
			}
		}
	}

	public bool HasOtherRegisteredEvent(string Event, IPart Part)
	{
		if (Event == "EndTurn" && WantsEndTurnEvent(Part))
		{
			return true;
		}
		if (Event == "TakeDamage" && Part != pPhysics)
		{
			return true;
		}
		if (Event == "BeforeDeathRemoval" && !(Part is Body))
		{
			return true;
		}
		if (Event == "CommandAutoEquipObject")
		{
			return true;
		}
		if (Event == "Regenera")
		{
			return true;
		}
		if (RegisteredPartEvents != null && RegisteredPartEvents.TryGetValue(Event, out var value))
		{
			foreach (IPart item in value)
			{
				if (item != Part)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasOtherRegisteredEvent(string Event, Effect fx)
	{
		if (Event == "EndTurn" && WantsEndTurnEvent())
		{
			return true;
		}
		switch (Event)
		{
		case "TakeDamage":
			return true;
		case "BeforeDeathRemoval":
			return true;
		case "CommandAutoEquipObject":
			return true;
		case "Regenera":
			return true;
		default:
		{
			if (RegisteredEffectEvents != null && RegisteredEffectEvents.TryGetValue(Event, out var value))
			{
				foreach (Effect item in value)
				{
					if (item != fx)
					{
						return true;
					}
				}
			}
			return false;
		}
		}
	}

	public bool FireEvent(string ID)
	{
		if (ID.IndexOf(',') != -1)
		{
			bool result = true;
			string[] array = ID.Split(',');
			foreach (string text in array)
			{
				if (HasRegisteredEvent(text) && !FireEvent(Event.New(text)))
				{
					result = false;
				}
			}
			return result;
		}
		if (!HasRegisteredEvent(ID))
		{
			return true;
		}
		return FireEvent(Event.New(ID));
	}

	public bool FireEvent(string ID, IEvent ParentEvent)
	{
		if (!HasRegisteredEvent(ID))
		{
			return true;
		}
		return FireEvent(Event.New(ID), ParentEvent);
	}

	public bool FireEvent(string ID, Event ParentEvent)
	{
		if (!HasRegisteredEvent(ID))
		{
			return true;
		}
		return FireEvent(Event.New(ID), ParentEvent);
	}

	public bool FireEvent(Event E, IEvent ParentEvent)
	{
		bool result = FireEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEvent(Event E, Event ParentEvent)
	{
		bool result = FireEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	public bool FireEventDirect(Event E)
	{
		for (int i = 0; i < Effects.Count; i++)
		{
			if (!Effects[i].FireEvent(E))
			{
				return false;
			}
		}
		for (int j = 0; j < PartsList.Count; j++)
		{
			if (!PartsList[j].FireEvent(E))
			{
				return false;
			}
		}
		return true;
	}

	public bool BroadcastEvent(Event E)
	{
		if (PartsList != null)
		{
			int i = 0;
			for (int num = PartsList.Count; i < num; i++)
			{
				if (!PartsList[i].FireEvent(E))
				{
					return false;
				}
				if (PartsList.Count < num)
				{
					int num2 = num - PartsList.Count;
					i -= num2;
					num -= num2;
				}
			}
		}
		return true;
	}

	public bool LocalEvent(Event E)
	{
		if (!FireEvent(E))
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell != null)
		{
			int i = 0;
			for (int count = currentCell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = currentCell.Objects[i];
				if (gameObject != this && !gameObject.FireEvent(E))
				{
					return false;
				}
				if (currentCell.Objects.Count != count)
				{
					count = currentCell.Objects.Count;
					if (i < count && currentCell.Objects[i] != gameObject)
					{
						i--;
					}
				}
			}
		}
		return true;
	}

	public bool LocalEvent(string Name)
	{
		return LocalEvent(Event.New(Name));
	}

	public bool ExtendedLocalEvent(Event E)
	{
		if (!FireEvent(E))
		{
			return false;
		}
		if (CurrentCell != null)
		{
			foreach (GameObject @object in CurrentCell.Objects)
			{
				if (@object != this && !@object.FireEvent(E))
				{
					return false;
				}
			}
			foreach (Cell localAdjacentCell in CurrentCell.GetLocalAdjacentCells())
			{
				foreach (GameObject object2 in localAdjacentCell.Objects)
				{
					if (!object2.FireEvent(E))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public bool ExtendedLocalEvent(string Name)
	{
		return ExtendedLocalEvent(Event.New(Name));
	}

	public bool FireEventOnBodyparts(Event E)
	{
		return Body?.FireEventOnBodyparts(E) ?? true;
	}

	public bool WantsEndTurnEvent()
	{
		Body body = Body;
		if (body != null && body.WantsEndTurnEvent())
		{
			return true;
		}
		Inventory inventory = Inventory;
		if (inventory != null && inventory.WantsEndTurnEvent())
		{
			return true;
		}
		return false;
	}

	public bool WantsEndTurnEvent(IPart Except)
	{
		Body body = Body;
		if (body != null && body != Except && body.WantsEndTurnEvent())
		{
			return true;
		}
		Inventory inventory = Inventory;
		if (inventory != null && inventory != Except && inventory.WantsEndTurnEvent())
		{
			return true;
		}
		return false;
	}

	public bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Body body = Body;
			if (body != null && !body.FireEvent(E))
			{
				return false;
			}
			Inventory inventory = Inventory;
			if (inventory != null && !inventory.FireEvent(E))
			{
				return false;
			}
			CleanEffects();
		}
		else if (E.ID == "BeforeDeathRemoval")
		{
			Body body2 = Body;
			if (body2 != null && !body2.FireEvent(E))
			{
				return false;
			}
		}
		else if (E.ID == "TakeDamage")
		{
			if (pPhysics != null && !pPhysics.ProcessTakeDamage(E))
			{
				return false;
			}
		}
		else if (E.ID == "CommandAutoEquipObject")
		{
			if (!AutoEquip(E.GetGameObjectParameter("Object")))
			{
				return false;
			}
		}
		else if (E.ID == "Regenera")
		{
			int mask = 9244;
			int num = 100663296;
			if (E.GetIntParameter("Level") < 5)
			{
				num |= 0x1000000;
			}
			targetEffects.Clear();
			int i = 0;
			for (int count = Effects.Count; i < count; i++)
			{
				Effect effect = Effects[i];
				if (effect.IsOfType(mask) && effect.IsOfTypes(num))
				{
					targetEffects.Add(effect);
				}
			}
			Effect randomElement = targetEffects.GetRandomElement();
			if (randomElement != null)
			{
				if (IsPlayer())
				{
					GameObject gameObjectParameter = E.GetGameObjectParameter("Source");
					string text = null;
					if (gameObjectParameter != null)
					{
						text = gameObjectParameter.T() + gameObjectParameter.GetVerb("cure") + " you of";
					}
					else
					{
						text = E.GetStringParameter("SourceDescription");
						if (text != null)
						{
							ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(text);
						}
					}
					if (text == null)
					{
						text = "Your regenerative metabolism cures you of";
					}
					string text2 = text;
					text2 = ((!(randomElement.DisplayName == randomElement.ClassName)) ? (text2 + " " + randomElement.DisplayName + ".") : (text2 + " a malady."));
					MessageQueue.AddPlayerMessage(text2);
				}
				RemoveEffect(randomElement);
			}
			targetEffects.Clear();
		}
		if (RegisteredEffectEvents != null && RegisteredEffectEvents.TryGetValue(E.ID, out var value))
		{
			for (int j = 0; j < value.Count; j++)
			{
				if (!value[j].FireEvent(E))
				{
					return false;
				}
			}
		}
		if (RegisteredPartEvents != null && RegisteredPartEvents.TryGetValue(E.ID, out var value2))
		{
			for (int k = 0; k < value2.Count; k++)
			{
				int count2 = value2.Count;
				if (!value2[k].FireEvent(E))
				{
					return false;
				}
				if (value2.Count < count2)
				{
					k--;
				}
			}
		}
		return true;
	}

	public void CompanionDirectionEnergyCost(GameObject GO, int EnergyCost, string Action)
	{
		if (CanMakeTelepathicContactWith(GO))
		{
			EnergyCost /= 10;
			Action = "Mental Direct Companion " + Action;
		}
		else
		{
			Action = "Direct Companion " + Action;
		}
		UseEnergy(EnergyCost, Action);
	}

	public int GetTier()
	{
		string tag = GetTag("Tier");
		if (!string.IsNullOrEmpty(tag))
		{
			try
			{
				return Convert.ToInt32(tag);
			}
			catch
			{
			}
		}
		return (Stat("Level") - 1) / 5 + 1;
	}

	public int GetTechTier()
	{
		string tag = GetTag("TechTier");
		if (!string.IsNullOrEmpty(tag))
		{
			try
			{
				return Convert.ToInt32(tag);
			}
			catch
			{
			}
		}
		return GetTier();
	}

	public bool CleanEffects()
	{
		bool flag = false;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			Effect effect = Effects[i];
			if (effect.Duration <= 0)
			{
				effect.Expired();
				RemoveEffect(effect, NeedStackCheck: false);
				i = -1;
				count = Effects.Count;
				flag = true;
			}
		}
		if (flag)
		{
			CheckStack();
		}
		return flag;
	}

	public bool AnyEffects()
	{
		return Effects.Count > 0;
	}

	public int GetPhase()
	{
		return Phase.getPhase(this);
	}

	public bool PhaseMatches(int VsPhase)
	{
		return Phase.phaseMatches(this, VsPhase);
	}

	public bool PhaseMatches(GameObject GO)
	{
		return Phase.phaseMatches(this, GO);
	}

	public bool FlightMatches(GameObject GO)
	{
		if (GO == null)
		{
			return true;
		}
		return GO.IsFlying == IsFlying;
	}

	public bool PhaseAndFlightMatches(GameObject GO)
	{
		if (GO == null)
		{
			return true;
		}
		if (PhaseMatches(GO))
		{
			return FlightMatches(GO);
		}
		return false;
	}

	public bool FlightCanReach(GameObject GO)
	{
		if (GO == null)
		{
			return true;
		}
		if (!GO.IsFlying)
		{
			return true;
		}
		if (IsFlying)
		{
			return true;
		}
		return false;
	}

	public bool HasEffect(string EffectType)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].Duration > 0 && (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(Type EffectType)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].GetType() == EffectType && Effects[i].Duration > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(string EffectType1, string EffectType2)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].Duration > 0 && (EffectType1 == Effects[i].ClassName || EffectType1 == Effects[i].DisplayName || EffectType2 == Effects[i].ClassName || EffectType2 == Effects[i].DisplayName))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (filter(Effects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(string EffectType, Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName) && filter(Effects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(Type EffectType, Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].GetType() == EffectType && filter(Effects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect<T>() where T : Effect
	{
		string text = ModManager.ResolveTypeName(typeof(T));
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].ClassName == text)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect<T>(Predicate<T> Filter) where T : Effect
	{
		string text = ModManager.ResolveTypeName(typeof(T));
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].ClassName == text && Effects[i] is T obj && Filter(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(string EffectType1, string EffectType2, Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType1 == Effects[i].ClassName || EffectType1 == Effects[i].DisplayName || EffectType2 == Effects[i].ClassName || EffectType2 == Effects[i].DisplayName) && filter(Effects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffectByClass(string EffectClass)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectClass == Effects[i].ClassName)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffectByClass(string EffectClass, Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectClass == Effects[i].ClassName && filter(Effects[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffectOtherThan(string EffectType, Effect Skip)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i] != Skip && (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffectOtherThan(Type EffectType, Effect Skip)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i] != Skip && Effects[i].GetType() == EffectType)
			{
				return true;
			}
		}
		return false;
	}

	public List<Effect> GetEffects(string EffectType)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName)
			{
				num++;
			}
		}
		List<Effect> list = new List<Effect>(num);
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (EffectType == Effects[j].ClassName || EffectType == Effects[j].DisplayName)
			{
				list.Add(Effects[j]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(Type EffectType)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType == Effects[i].GetType())
			{
				num++;
			}
		}
		List<Effect> list = new List<Effect>(num);
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (EffectType == Effects[j].GetType())
			{
				list.Add(Effects[j]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(string EffectType1, string EffectType2)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType1 == Effects[i].ClassName || EffectType1 == Effects[i].DisplayName || EffectType2 == Effects[i].ClassName || EffectType2 == Effects[i].DisplayName)
			{
				num++;
			}
		}
		List<Effect> list = new List<Effect>(num);
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (EffectType1 == Effects[j].ClassName || EffectType1 == Effects[j].DisplayName || EffectType2 == Effects[j].ClassName || EffectType2 == Effects[j].DisplayName)
			{
				list.Add(Effects[j]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(string[] EffectTypes)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName))
			{
				num++;
			}
		}
		List<Effect> list = new List<Effect>(num);
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (EffectTypes.Contains(Effects[j].ClassName) || EffectTypes.Contains(Effects[j].DisplayName))
			{
				list.Add(Effects[j]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(List<string> EffectTypes)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName))
			{
				num++;
			}
		}
		List<Effect> list = new List<Effect>(num);
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (EffectTypes.Contains(Effects[j].ClassName) || EffectTypes.Contains(Effects[j].DisplayName))
			{
				list.Add(Effects[j]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(Predicate<Effect> filter)
	{
		List<Effect> list = new List<Effect>();
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (filter(Effects[i]))
			{
				list.Add(Effects[i]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(string EffectType, Predicate<Effect> filter)
	{
		List<Effect> list = new List<Effect>();
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName) && filter(Effects[i]))
			{
				list.Add(Effects[i]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(string EffectType1, string EffectType2, Predicate<Effect> filter)
	{
		List<Effect> list = new List<Effect>();
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType1 == Effects[i].ClassName || EffectType1 == Effects[i].DisplayName || EffectType2 == Effects[i].ClassName || EffectType2 == Effects[i].DisplayName) && filter(Effects[i]))
			{
				list.Add(Effects[i]);
			}
		}
		return list;
	}

	public List<Effect> GetEffects(string[] EffectTypes, Predicate<Effect> filter)
	{
		List<Effect> list = new List<Effect>();
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName)) && filter(Effects[i]))
			{
				list.Add(Effects[i]);
			}
		}
		return list;
	}

	public int GetEffectCount(string EffectType)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName)
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(string[] EffectTypes)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(List<string> EffectTypes)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(Type EffectType)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].GetType() == EffectType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(Type EffectType, Predicate<Effect> filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].GetType() == EffectType && filter(Effects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(Predicate<Effect> filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (filter(Effects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(string EffectType, Predicate<Effect> filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName) && filter(Effects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCount(string[] EffectTypes, Predicate<Effect> filter)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectTypes.Contains(Effects[i].ClassName) || EffectTypes.Contains(Effects[i].DisplayName)) && filter(Effects[i]))
			{
				num++;
			}
		}
		return num;
	}

	public int GetEffectCountExcept(string EffectType, Effect skip)
	{
		int num = 0;
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i] != skip && (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName))
			{
				num++;
			}
		}
		return num;
	}

	public Effect GetEffect(string EffectType)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName)
			{
				return Effects[i];
			}
		}
		return null;
	}

	public T GetEffect<T>() where T : Effect
	{
		string text = ModManager.ResolveTypeName(typeof(T));
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].ClassName == text)
			{
				return Effects[i] as T;
			}
		}
		return null;
	}

	public T GetEffect<T>(Predicate<T> Filter) where T : Effect
	{
		string text = ModManager.ResolveTypeName(typeof(T));
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (Effects[i].ClassName == text && Effects[i] is T val && Filter(val))
			{
				return val;
			}
		}
		return null;
	}

	public Effect GetEffectByClassName(string ClassName)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (ClassName == Effects[i].ClassName)
			{
				return Effects[i];
			}
		}
		return null;
	}

	public Effect GetEffect(Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (filter(Effects[i]))
			{
				return Effects[i];
			}
		}
		return null;
	}

	public Effect GetEffect(string EffectType, Predicate<Effect> filter)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if ((EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName) && filter(Effects[i]))
			{
				return Effects[i];
			}
		}
		return null;
	}

	public void ForeachEffect(Action<Effect> aProc)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			aProc(Effects[i]);
		}
	}

	public void ForeachEffect(string EffectType, Action<Effect> aProc)
	{
		int i = 0;
		for (int count = Effects.Count; i < count; i++)
		{
			if (EffectType == Effects[i].ClassName || EffectType == Effects[i].DisplayName)
			{
				aProc(Effects[i]);
			}
		}
	}

	public bool IsAutogetLiquid()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume == null || !liquidVolume.IsOpenVolume() || liquidVolume.IsMixed())
		{
			return false;
		}
		string primaryLiquidID = liquidVolume.GetPrimaryLiquidID();
		if (primaryLiquidID != null)
		{
			return ThePlayer.GetAutoCollectDrams(primaryLiquidID) > 0;
		}
		return false;
	}

	public bool IsWaterPuddle()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume != null && liquidVolume.MaxVolume < 0)
		{
			return liquidVolume.IsWater();
		}
		return false;
	}

	public bool IsFreshWaterPuddle()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume != null && liquidVolume.MaxVolume < 0)
		{
			return liquidVolume.IsFreshWater();
		}
		return false;
	}

	public bool ContainsFreshWater()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume != null && liquidVolume.MaxVolume >= 0)
		{
			return liquidVolume.IsFreshWater();
		}
		return false;
	}

	public bool IsTakeable()
	{
		if (Takeable)
		{
			return FireEvent("CanBeTaken");
		}
		return false;
	}

	public int GetHPPercent()
	{
		int result = 100;
		int num = baseHitpoints;
		if (num != 0)
		{
			result = hitpoints * 100 / num;
		}
		return result;
	}

	public string GetHPColor()
	{
		int hPPercent = GetHPPercent();
		if (hPPercent < 15)
		{
			return "&r";
		}
		if (hPPercent < 33)
		{
			return "&R";
		}
		if (hPPercent < 66)
		{
			return "&W";
		}
		if (hPPercent < 100)
		{
			return "&G";
		}
		return "&Y";
	}

	public string GetVisibleStatusColor(out string DetailColor, string HPColor = null)
	{
		eVisibleStatusColor.SetParameter("Color", HPColor ?? GetHPColor());
		eVisibleStatusColor.SetParameter("DetailColor", null);
		FireEvent(eVisibleStatusColor);
		DetailColor = eVisibleStatusColor.GetStringParameter("DetailColor");
		return eVisibleStatusColor.GetStringParameter("Color");
	}

	public string GetVisibleStatusColor(string HPColor = null)
	{
		eVisibleStatusColor.SetParameter("Color", HPColor ?? GetHPColor());
		FireEvent(eVisibleStatusColor);
		return eVisibleStatusColor.GetStringParameter("Color");
	}

	public void UpdateVisibleStatusColor(string HPColor = null)
	{
		if (HasTagOrProperty("NoPlayerColor"))
		{
			return;
		}
		if (IsPlayerControlled() && Options.AlwaysHPColor)
		{
			if (pRender == null)
			{
				return;
			}
			if (HPColor == null)
			{
				HPColor = GetHPColor();
			}
			if (OriginalColorString == null)
			{
				OriginalColorString = pRender.ColorString;
			}
			if (OriginalDetailColor == null)
			{
				OriginalDetailColor = pRender.DetailColor;
			}
			string DetailColor;
			string visibleStatusColor = GetVisibleStatusColor(out DetailColor, HPColor);
			pRender.ColorString = HPColor;
			if (!string.IsNullOrEmpty(DetailColor))
			{
				pRender.DetailColor = DetailColor;
			}
			if (!string.IsNullOrEmpty(pRender.TileColor))
			{
				if (OriginalTileColor == null)
				{
					OriginalTileColor = pRender.TileColor;
				}
				pRender.TileColor = visibleStatusColor;
			}
		}
		else if (IsPlayerControlled() && Options.HPColor)
		{
			if (pRender == null)
			{
				return;
			}
			if (HPColor == null)
			{
				HPColor = GetHPColor();
			}
			if (OriginalColorString == null)
			{
				OriginalColorString = pRender.ColorString;
			}
			if (OriginalDetailColor == null)
			{
				OriginalDetailColor = pRender.DetailColor;
			}
			string DetailColor2;
			string visibleStatusColor2 = GetVisibleStatusColor(out DetailColor2, HPColor);
			pRender.ColorString = visibleStatusColor2;
			if (!string.IsNullOrEmpty(DetailColor2))
			{
				pRender.DetailColor = DetailColor2;
			}
			if (!string.IsNullOrEmpty(pRender.TileColor))
			{
				if (OriginalTileColor == null)
				{
					OriginalTileColor = pRender.TileColor;
				}
				pRender.TileColor = visibleStatusColor2;
			}
		}
		else if (OriginalColorString != null && pRender != null)
		{
			pRender.ColorString = OriginalColorString;
			OriginalColorString = null;
			pRender.DetailColor = OriginalDetailColor;
			OriginalDetailColor = null;
			if (OriginalTileColor != null)
			{
				pRender.TileColor = OriginalTileColor;
				OriginalTileColor = null;
			}
		}
	}

	public bool ShouldAutoexploreAsChest()
	{
		if (!HasTag("AutoexploreChest"))
		{
			return false;
		}
		if (HasIntProperty("Autoexplored"))
		{
			return false;
		}
		if (Owner != null)
		{
			return false;
		}
		Inventory inventory = Inventory;
		if (inventory == null)
		{
			return false;
		}
		if (inventory.Objects.Count == 0)
		{
			return false;
		}
		if (HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return true;
	}

	public bool ShouldAutoexploreAsShelf()
	{
		if (!HasTag("AutoexploreShelf"))
		{
			return false;
		}
		if (HasIntProperty("Autoexplored"))
		{
			return false;
		}
		if (Owner != null)
		{
			return false;
		}
		Inventory inventory = Inventory;
		if (inventory == null)
		{
			return false;
		}
		if (inventory.Objects.Count == 0)
		{
			return false;
		}
		if (HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return true;
	}

	public bool ShouldAutoget()
	{
		if (pPhysics == null)
		{
			return false;
		}
		if (pPhysics.Owner != null)
		{
			return false;
		}
		if (!pPhysics.IsReal)
		{
			return false;
		}
		if (HasTagOrProperty("NoAutoget"))
		{
			return false;
		}
		if (HasIntProperty("DroppedByPlayer"))
		{
			return false;
		}
		if (IsTemporary)
		{
			return false;
		}
		if (HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		if (IsAutogetLiquid())
		{
			return true;
		}
		if (!IsTakeable())
		{
			return false;
		}
		if (Options.AutogetSpecialItems && IsSpecialItem())
		{
			return true;
		}
		if (GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0 && Options.AutogetArtifacts)
		{
			return true;
		}
		if (Options.AutogetPrimitiveAmmo && HasPart("AmmoArrow"))
		{
			return true;
		}
		if (Options.AutogetAmmo && (HasPart("AmmoSlug") || HasPart("EnergyCell") || HasPart("LiquidFueledEnergyCell") || HasPart("AmmoShotgunShell")))
		{
			return true;
		}
		switch (GetInventoryCategory())
		{
		case "Trade Goods":
			if (Options.AutogetTradeGoods)
			{
				return true;
			}
			break;
		case "Food":
			if (Options.AutogetFood)
			{
				return true;
			}
			break;
		case "Books":
			if (Options.AutogetBooks)
			{
				return true;
			}
			break;
		}
		bool flag = false;
		double num = 0.0;
		if (Options.AutogetFreshWater && ContainsFreshWater())
		{
			if (!flag)
			{
				num = GetWeight();
				flag = true;
			}
			if (num <= 1.0)
			{
				return true;
			}
		}
		if (Options.AutogetZeroWeight)
		{
			if (!flag)
			{
				num = GetWeight();
				flag = true;
			}
			if (num <= 0.0)
			{
				return true;
			}
		}
		if (Options.AutogetNuggets && HasTagOrProperty("Nugget"))
		{
			return true;
		}
		if (Options.AutogetScrap && TinkeringHelpers.ConsiderScrap(this, ThePlayer))
		{
			return true;
		}
		return false;
	}

	public bool ShouldTakeAll()
	{
		if (!IsTakeable())
		{
			return false;
		}
		if (GetInventoryCategory() == "Corpse" && !Options.TakeallCorpses)
		{
			return false;
		}
		return true;
	}

	public bool HasPropertyOrTag(string Name)
	{
		if (!HasProperty(Name))
		{
			return HasTag(Name);
		}
		return true;
	}

	public bool HasIntProperty(string Name)
	{
		if (IntProperty != null && Name != null)
		{
			return IntProperty.ContainsKey(Name);
		}
		return false;
	}

	public bool HasProperty(string Name)
	{
		if (Name == null)
		{
			return false;
		}
		if (Property != null && Property.ContainsKey(Name))
		{
			return true;
		}
		if (IntProperty != null && IntProperty.ContainsKey(Name))
		{
			return true;
		}
		return false;
	}

	public bool HasStringProperty(string Name)
	{
		if (Property != null && Name != null)
		{
			return Property.ContainsKey(Name);
		}
		return false;
	}

	public bool IsInvisibile()
	{
		if (HasPart("Invisibility"))
		{
			return true;
		}
		if (HasPart("MentalInvisibility"))
		{
			return true;
		}
		return false;
	}

	public bool IsEsper()
	{
		if (HasPart("Esper"))
		{
			return true;
		}
		if (Property.TryGetValue("MutationLevel", out var value) && value != null && value.Contains("Esper"))
		{
			return true;
		}
		return false;
	}

	public bool IsImportant()
	{
		int intProperty = GetIntProperty("Important");
		if (intProperty >= 1)
		{
			return true;
		}
		if (intProperty <= -1)
		{
			return false;
		}
		if (HasTagOrProperty("Important"))
		{
			return true;
		}
		if (HasTagOrProperty("QuestItem"))
		{
			return true;
		}
		if (HasPart("StairsUp"))
		{
			return true;
		}
		if (HasPart("StairsDown"))
		{
			return true;
		}
		if (HasTagOrProperty("Storied"))
		{
			return true;
		}
		return false;
	}

	public bool IsMarkedImportantByPlayer()
	{
		return GetIntProperty("Important") == 2;
	}

	public void SetImportant(bool flag, bool force = false, bool player = false)
	{
		if (flag)
		{
			if (force || GetIntProperty("Important") >= 0)
			{
				SetIntProperty("Important", (!player) ? 1 : 2);
			}
		}
		else if (force)
		{
			SetIntProperty("Important", player ? (-2) : (-1));
		}
		else
		{
			RemoveIntProperty("Important");
		}
	}

	/// <summary>
	///             Display a formatted confirmation popup if this object is important and the provided actor is the player.
	///             </summary><returns><c>true</c> if this object is not important or the player confirmed its usage; otherwise, <c>false</c>.</returns>
	public bool ConfirmUseImportant(GameObject Actor = null, string Verb = "use", string Extra = null, int Amount = -1)
	{
		if (!IsImportant())
		{
			return true;
		}
		if (Actor != null && !Actor.IsPlayer())
		{
			return false;
		}
		Extra = ((!string.IsNullOrEmpty(Extra)) ? (" " + Extra) : "");
		if (Amount == -1)
		{
			Amount = Count;
		}
		if (Amount > 1 && !IsPlural)
		{
			if (Popup.ShowYesNo(The + Grammar.Pluralize(ShortDisplayNameSingle) + " are important. Are you sure you want to " + Verb + " them" + Extra + "?") != 0)
			{
				return false;
			}
		}
		else if (Popup.ShowYesNo(T(int.MaxValue, null, null, AsIfKnown: false, Single: true) + Is + " important. Are you sure you want to " + Verb + " " + them + Extra + "?") != 0)
		{
			return false;
		}
		return true;
	}

	public bool IsChimera()
	{
		if (HasPart("Chimera"))
		{
			return true;
		}
		if (Property.TryGetValue("MutationLevel", out var value) && value != null && value.Contains("Chimera"))
		{
			return true;
		}
		return false;
	}

	public bool IsWall()
	{
		return HasTagOrProperty("Wall");
	}

	public bool IsDoor()
	{
		return HasPart("Door");
	}

	public bool IsDiggable()
	{
		return HasTagOrProperty("Diggable");
	}

	public bool HasPart(string Name)
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].Name == Name)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPart(Type type)
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyInstalledCybernetics()
	{
		return Body?.AnyInstalledCybernetics() ?? false;
	}

	public bool AnyInstalledCybernetics(Predicate<GameObject> Filter)
	{
		return Body?.AnyInstalledCybernetics(Filter) ?? false;
	}

	public bool UnequipAndRemove()
	{
		bool flag = true;
		if (Equipped != null)
		{
			flag = EquipmentAPI.UnequipObject(this);
		}
		if (flag)
		{
			InInventory?.Inventory?.RemoveObject(this);
		}
		return flag;
	}

	public bool ForceUnequipAndRemove(bool Silent = false)
	{
		bool flag = true;
		if (Equipped != null)
		{
			flag = EquipmentAPI.ForceUnequipObject(this, Silent);
		}
		if (flag)
		{
			InInventory?.Inventory?.RemoveObject(this);
		}
		return flag;
	}

	public void ForceUnequipRemoveAndRemoveContents(bool Silent = false, bool ExcludeNatural = true)
	{
		Cell currentCell = GetCurrentCell();
		GameObject gameObject = null;
		if (Equipped != null)
		{
			gameObject = Equipped;
			EquipmentAPI.ForceUnequipObject(this, Silent);
		}
		if (InInventory != null)
		{
			gameObject = InInventory;
			InInventory.Inventory.RemoveObject(this);
		}
		if (gameObject != null)
		{
			foreach (GameObject content in GetContents())
			{
				if (!ExcludeNatural || !content.HasTag("NaturalGear"))
				{
					content.RemoveFromContext();
					gameObject.TakeObject(content, Silent, 0);
				}
			}
			return;
		}
		if (currentCell == null)
		{
			return;
		}
		foreach (GameObject content2 in GetContents())
		{
			if (!ExcludeNatural || !content2.HasTag("NaturalGear"))
			{
				content2.RemoveFromContext();
				currentCell.AddObject(content2);
			}
		}
	}

	public void RemoveContents(bool Silent = false, bool ExcludeNatural = true)
	{
		GetContext(out var ObjectContext, out var CellContext);
		if (ObjectContext == null && CellContext == null)
		{
			return;
		}
		foreach (GameObject content in GetContents())
		{
			if (!ExcludeNatural || !content.HasTag("NaturalGear"))
			{
				content.RemoveFromContext();
				if (ObjectContext != null)
				{
					ObjectContext.TakeObject(content, Silent, 0);
				}
				else
				{
					CellContext.AddObject(content);
				}
			}
		}
	}

	public bool Unimplant(bool MoveToInventory = false, List<BodyPart> Parts = null)
	{
		GameObject implantee = Implantee;
		if (implantee == null)
		{
			return false;
		}
		Body body = implantee.Body;
		if (body == null)
		{
			return false;
		}
		int num = 0;
		BodyPart bodyPart;
		while ((bodyPart = body.FindCybernetics(this)) != null)
		{
			Parts?.Add(bodyPart);
			if (++num >= 100)
			{
				Debug.LogError("infinite looping trying to unimplant " + DebugName);
				break;
			}
			bodyPart.Unimplant(MoveToInventory);
		}
		body?.RegenerateDefaultEquipment();
		return true;
	}

	public List<GameObject> GetContents()
	{
		return GetContentsEvent.GetFor(this);
	}

	public GameObject HasItemWithBlueprint(string bp)
	{
		GameObject result = null;
		Inventory part = GetPart<Inventory>();
		Body body = Body;
		if (part != null)
		{
			foreach (GameObject @object in part.Objects)
			{
				if (@object.Blueprint == bp)
				{
					return @object;
				}
			}
		}
		if (body != null)
		{
			foreach (GameObject equippedObject in body.GetEquippedObjects())
			{
				if (!equippedObject.IsNatural() && equippedObject.pPhysics.IsReal && equippedObject.Blueprint == bp)
				{
					return equippedObject;
				}
			}
			return result;
		}
		return result;
	}

	public GameObject GetMostValuableItem()
	{
		double num = double.MinValue;
		GameObject result = null;
		Inventory part = GetPart<Inventory>();
		Body body = Body;
		if (part != null)
		{
			foreach (GameObject @object in part.Objects)
			{
				if (!@object.IsNatural() && @object.pPhysics.IsReal)
				{
					double value = @object.Value;
					if (@object.HasPart("Commerce"))
					{
						value = (@object.GetPart("Commerce") as Commerce).Value;
					}
					if (value > num)
					{
						num = @object.Value;
						result = @object;
					}
				}
			}
		}
		if (body != null)
		{
			foreach (GameObject equippedObject in body.GetEquippedObjects())
			{
				if (!equippedObject.IsNatural() && equippedObject.pPhysics.IsReal && equippedObject.Value > num)
				{
					num = equippedObject.Value;
					result = equippedObject;
				}
			}
			return result;
		}
		return result;
	}

	public void AddSkill(string Class)
	{
		if (GetPart("Skills") is XRL.World.Parts.Skills skills)
		{
			Type type = ModManager.ResolveType("XRL.World.Parts.Skill." + Class);
			skills.AddSkill(Activator.CreateInstance(type) as BaseSkill);
		}
	}

	public void RemoveSkill(string Class)
	{
		if (GetPart("Skills") is XRL.World.Parts.Skills skills)
		{
			skills.RemoveSkill(GetPart(Class) as BaseSkill);
			RemovePart(Class);
		}
	}

	public bool HasSkill(string Name)
	{
		return HasPart(Name);
	}

	public bool HasTagOrStringProperty(string Name, string Default = null)
	{
		if (!HasStringProperty(Name))
		{
			return HasTag(Name);
		}
		return true;
	}

	public string GetTagOrStringProperty(string Name, string Default = null)
	{
		return GetStringProperty(Name) ?? GetTag(Name) ?? Default;
	}

	public string GetTagOrStringProperty_RandomSplit(string Name, string Default = null, char Delimiter = ',')
	{
		return GetTagOrStringProperty(Name, Default).Split(Delimiter).GetRandomElement();
	}

	public string GetTag(string Tag, string Default = null)
	{
		return GetBlueprint().GetTag(Tag, Default);
	}

	public bool HasTagOrProperty(string Name)
	{
		if (!HasTag(Name))
		{
			return HasProperty(Name);
		}
		return true;
	}

	public string GetPropertyOrTag(string Name, string Default = null)
	{
		if (Name == null)
		{
			return Default;
		}
		if (HasStringProperty(Name))
		{
			return GetStringProperty(Name, Default);
		}
		if (HasIntProperty(Name))
		{
			return GetIntProperty(Name).ToString();
		}
		return GetBlueprint().GetTag(Name, Default);
	}

	public string GetxTag(string Tag, string Value, string Default = null)
	{
		return GetBlueprint().GetxTag(Tag, Value, Default);
	}

	public string GetxTag_CommaDelimited(string Tag, string Value, string Default = null, System.Random R = null)
	{
		return GetBlueprint().GetxTag_CommaDelimited(Tag, Value, Default, R);
	}

	public List<string> GetMutationNames()
	{
		if (!(GetPart("Mutations") is Mutations mutations) || mutations.MutationList == null || mutations.MutationList.Count <= 0)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (BaseMutation mutation in mutations.MutationList)
		{
			list.Add(mutation.DisplayName);
		}
		return list;
	}

	public List<BaseMutation> GetPhysicalMutations()
	{
		return GetMutationsOfCategory("Physical");
	}

	public List<BaseMutation> GetMentalMutations()
	{
		return GetMutationsOfCategory("Mental");
	}

	public List<BaseMutation> GetMutationsOfCategory(string category)
	{
		if (!(GetPart("Mutations") is Mutations mutations))
		{
			return new List<BaseMutation>();
		}
		List<BaseMutation> list = new List<BaseMutation>();
		list.AddRange(mutations.MutationList.Where((BaseMutation m) => m.isCategory(category)));
		return list;
	}

	public T GetPart<T>() where T : IPart
	{
		string text = ModManager.ResolveTypeName(typeof(T));
		switch (text)
		{
		case "Render":
			return _pRender as T;
		case "Physics":
			return _pPhysics as T;
		case "Brain":
			return _pBrain as T;
		default:
		{
			int i = 0;
			for (int count = PartsList.Count; i < count; i++)
			{
				if (PartsList[i].Name == text)
				{
					return PartsList[i] as T;
				}
			}
			return null;
		}
		}
	}

	public IPart GetPart(string Name)
	{
		switch (Name)
		{
		case "Render":
			return _pRender;
		case "Physics":
			return _pPhysics;
		case "Brain":
			return _pBrain;
		default:
		{
			int i = 0;
			for (int count = PartsList.Count; i < count; i++)
			{
				if (PartsList[i].Name == Name)
				{
					return PartsList[i];
				}
			}
			return null;
		}
		}
	}

	public IPart GetPartExcept(string Name, IPart skip)
	{
		if (skip == null)
		{
			return GetPart(Name);
		}
		if (Name == "Render" && _pRender != skip)
		{
			return _pRender;
		}
		if (Name == "Physics" && _pRender != skip)
		{
			return _pPhysics;
		}
		if (Name == "Brain" && _pRender != skip)
		{
			return _pBrain;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].Name == Name && PartsList[i] != skip)
			{
				return PartsList[i];
			}
		}
		return null;
	}

	protected void AddPartInternals(IPart P, bool DoRegistration = true, bool Initial = false, bool Creation = false)
	{
		FlushWantTurnTickCache();
		if (P != null)
		{
			if (P.Name == "Physics")
			{
				_pPhysics = P as XRL.World.Parts.Physics;
			}
			else if (P.Name == "Brain")
			{
				_pBrain = P as Brain;
			}
			else if (P.Name == "Render")
			{
				_pRender = P as Render;
			}
			else if (P.Name == "Body")
			{
				Body = P as Body;
			}
			else if (P.Name == "LiquidVolume")
			{
				LiquidVolume = P as LiquidVolume;
			}
			else if (P.Name == "Inventory")
			{
				Inventory = P as Inventory;
			}
			PartsList.Add(P);
			if (DoRegistration)
			{
				P.ParentObject = this;
				P.Register(this);
			}
			if (Initial)
			{
				P.Initialize();
			}
			P.Attach();
			if (!Creation)
			{
				P.AddedAfterCreation();
			}
			_isCombatObject = byte.MaxValue;
		}
	}

	public IPart AddPart(IPart P, bool DoRegistration = true, bool Creation = false)
	{
		if (!(P is IModification modification))
		{
			AddPartInternals(P, DoRegistration, Initial: true, Creation);
		}
		else
		{
			if (modification.Tier == 0)
			{
				modification.ApplyTier(GetTier());
			}
			ApplyModification(modification, DoRegistration, null, Creation);
		}
		return P;
	}

	public IModification AddPart(IModification P, bool DoRegistration = true, bool Creation = false)
	{
		AddPartInternals(P, DoRegistration, Initial: true, Creation);
		return P;
	}

	public T AddPart<T>(T P, bool DoRegistration = true, bool Creation = false) where T : IPart
	{
		return AddPart((IPart)P, DoRegistration, Creation) as T;
	}

	public T AddPart<T>(bool DoRegistration = true, bool Creation = false) where T : IPart, new()
	{
		return AddPart((IPart)new T(), DoRegistration, Creation) as T;
	}

	public T RequirePart<T>(bool Creation = false) where T : IPart, new()
	{
		T part = GetPart<T>();
		if (part != null)
		{
			return part;
		}
		return AddPart<T>(DoRegistration: true, Creation);
	}

	public bool ApplyModification(IModification ModPart, bool DoRegistration = true, GameObject by = null, bool Creation = false)
	{
		return TechModding.ApplyModification(this, ModPart, DoRegistration, by, Creation);
	}

	public bool ApplyModification(string ModPartName, bool DoRegistration = true, GameObject by = null, bool Creation = false)
	{
		return TechModding.ApplyModification(this, ModPartName, DoRegistration, by, Creation);
	}

	public bool ApplyModification(string ModPartName, int Tier, bool DoRegistration = true, GameObject by = null, bool Creation = false)
	{
		return TechModding.ApplyModification(this, ModPartName, Tier, DoRegistration, by, Creation);
	}

	public bool HasPartDescendedFrom<T>() where T : IPart
	{
		if (_pRender != null && _pRender is T)
		{
			return true;
		}
		if (_pPhysics != null && _pPhysics is T)
		{
			return true;
		}
		if (_pBrain != null && _pBrain is T)
		{
			return true;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	public T GetPartDescendedFrom<T>() where T : IPart
	{
		if (_pRender != null && _pRender is T)
		{
			return _pRender as T;
		}
		if (_pPhysics != null && _pPhysics is T)
		{
			return _pPhysics as T;
		}
		if (_pBrain != null && _pBrain is T)
		{
			return _pBrain as T;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T)
			{
				return PartsList[i] as T;
			}
		}
		return null;
	}

	public T GetPartDescendedFrom<T>(Predicate<T> pFilter) where T : IPart
	{
		if (_pRender != null && _pRender is T && pFilter(_pRender as T))
		{
			return _pRender as T;
		}
		if (_pPhysics != null && _pPhysics is T && pFilter(_pPhysics as T))
		{
			return _pPhysics as T;
		}
		if (_pBrain != null && _pBrain is T && pFilter(_pBrain as T))
		{
			return _pBrain as T;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T && pFilter(PartsList[i] as T))
			{
				return PartsList[i] as T;
			}
		}
		return null;
	}

	public List<T> GetPartsDescendedFrom<T>() where T : IPart
	{
		List<T> list = new List<T>();
		if (_pRender != null && _pRender is T)
		{
			list.Add(_pRender as T);
		}
		if (_pPhysics != null && _pPhysics is T)
		{
			list.Add(_pPhysics as T);
		}
		if (_pBrain != null && _pBrain is T)
		{
			list.Add(_pBrain as T);
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T)
			{
				list.Add(PartsList[i] as T);
			}
		}
		return list;
	}

	public List<T> GetPartsDescendedFrom<T>(Predicate<T> pFilter) where T : IPart
	{
		List<T> list = new List<T>();
		if (_pRender != null && _pRender is T && pFilter(_pRender as T))
		{
			list.Add(_pRender as T);
		}
		if (_pPhysics != null && _pPhysics is T && pFilter(_pPhysics as T))
		{
			list.Add(_pPhysics as T);
		}
		if (_pBrain != null && _pBrain is T && pFilter(_pBrain as T))
		{
			list.Add(_pBrain as T);
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T && pFilter(PartsList[i] as T))
			{
				list.Add(PartsList[i] as T);
			}
		}
		return list;
	}

	public T GetFirstPartDescendedFrom<T>() where T : IPart
	{
		if (_pRender != null && _pRender is T)
		{
			return _pRender as T;
		}
		if (_pPhysics != null && _pPhysics is T)
		{
			return _pPhysics as T;
		}
		if (_pBrain != null && _pBrain is T)
		{
			return _pBrain as T;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T)
			{
				return PartsList[i] as T;
			}
		}
		return null;
	}

	public T GetFirstPartDescendedFrom<T>(Predicate<T> pFilter) where T : IPart
	{
		if (_pRender != null && _pRender is T && pFilter(_pRender as T))
		{
			return _pRender as T;
		}
		if (_pPhysics != null && _pPhysics is T && pFilter(_pPhysics as T))
		{
			return _pPhysics as T;
		}
		if (_pBrain != null && _pBrain is T && pFilter(_pBrain as T))
		{
			return _pBrain as T;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is T && pFilter(PartsList[i] as T))
			{
				return PartsList[i] as T;
			}
		}
		return null;
	}

	public IFlightSource GetFirstFlightSourcePart()
	{
		if (_pRender != null && _pRender is IFlightSource)
		{
			return _pRender as IFlightSource;
		}
		if (_pPhysics != null && _pPhysics is IFlightSource)
		{
			return _pPhysics as IFlightSource;
		}
		if (_pBrain != null && _pBrain is IFlightSource)
		{
			return _pBrain as IFlightSource;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is IFlightSource)
			{
				return PartsList[i] as IFlightSource;
			}
		}
		return null;
	}

	public IFlightSource GetFirstFlightSourcePart(Predicate<IFlightSource> pFilter)
	{
		if (_pRender != null && _pRender is IFlightSource && pFilter(_pRender as IFlightSource))
		{
			return _pRender as IFlightSource;
		}
		if (_pPhysics != null && _pPhysics is IFlightSource && pFilter(_pPhysics as IFlightSource))
		{
			return _pPhysics as IFlightSource;
		}
		if (_pBrain != null && _pBrain is IFlightSource && pFilter(_pBrain as IFlightSource))
		{
			return _pBrain as IFlightSource;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i] is IFlightSource && pFilter(PartsList[i] as IFlightSource))
			{
				return PartsList[i] as IFlightSource;
			}
		}
		return null;
	}

	public IEnumerable<IPart> LoopParts()
	{
		foreach (IPart parts in PartsList)
		{
			yield return parts;
		}
	}

	public void ForeachPart(Action<IPart> aProc)
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			aProc(PartsList[i]);
		}
	}

	public bool ForeachPart(Predicate<IPart> pProc)
	{
		foreach (IPart parts in PartsList)
		{
			if (!pProc(parts))
			{
				return false;
			}
		}
		return true;
	}

	public void ForeachPartDescendedFrom<T>(Action<T> aProc) where T : IPart
	{
		foreach (IPart parts in PartsList)
		{
			if (parts is T)
			{
				aProc(parts as T);
			}
		}
	}

	public bool ForeachPartDescendedFrom<T>(Predicate<T> pProc) where T : IPart
	{
		foreach (IPart parts in PartsList)
		{
			if (parts is T && !pProc(parts as T))
			{
				return false;
			}
		}
		return true;
	}

	public void RemovePart(Type P)
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].GetType() == P)
			{
				RemovePart(PartsList[i]);
				break;
			}
		}
	}

	public void RemovePart(string P)
	{
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].Name == P)
			{
				RemovePart(PartsList[i]);
				break;
			}
		}
	}

	public void RemovePart<T>() where T : IPart
	{
		IPart part = GetPart<T>();
		if (part != null)
		{
			RemovePart(part);
		}
	}

	public void RemovePart(IPart P)
	{
		FlushWantTurnTickCache();
		if (P == null || PartsList == null || !PartsList.Remove(P))
		{
			return;
		}
		if (RegisteredPartEvents != null)
		{
			foreach (KeyValuePair<string, List<IPart>> registeredPartEvent in RegisteredPartEvents)
			{
				while (registeredPartEvent.Value.Contains(P))
				{
					registeredPartEvent.Value.Remove(P);
				}
			}
			CleanRegisteredPartEvents();
		}
		P.Remove();
	}

	public void CleanRegisteredPartEvents()
	{
		if (RegisteredPartEvents == null)
		{
			return;
		}
		string text = null;
		while (true)
		{
			text = null;
			foreach (KeyValuePair<string, List<IPart>> registeredPartEvent in RegisteredPartEvents)
			{
				if (registeredPartEvent.Value.Count <= 0)
				{
					text = registeredPartEvent.Key;
					break;
				}
			}
			if (text != null)
			{
				RegisteredPartEvents.Remove(text);
				text = null;
				continue;
			}
			break;
		}
	}

	public bool isDamaged()
	{
		if (Statistics == null)
		{
			return false;
		}
		Statistic value = null;
		if (!Statistics.TryGetValue("Hitpoints", out value))
		{
			return false;
		}
		return value.Value < value.BaseValue;
	}

	public bool isDamaged(double howMuch = 1.0, bool inclusive = false)
	{
		if (Statistics == null)
		{
			return false;
		}
		Statistic value = null;
		if (!Statistics.TryGetValue("Hitpoints", out value))
		{
			return false;
		}
		if (inclusive)
		{
			return (double)value.Value <= (double)value.BaseValue * howMuch;
		}
		return (double)value.Value < (double)value.BaseValue * howMuch;
	}

	public bool isDamaged(int percentageHowMuch, bool inclusive = false)
	{
		if (Statistics == null)
		{
			return false;
		}
		Statistic value = null;
		if (!Statistics.TryGetValue("Hitpoints", out value))
		{
			return false;
		}
		if (inclusive)
		{
			return value.Value <= value.BaseValue * percentageHowMuch / 100;
		}
		return value.Value < value.BaseValue * percentageHowMuch / 100;
	}

	public int GetPercentDamaged()
	{
		Statistic value = null;
		if (!Statistics.TryGetValue("Hitpoints", out value))
		{
			return 0;
		}
		return 100 - value.Value * 100 / value.BaseValue;
	}

	public GameObject equippedOrSelf()
	{
		return Equipped ?? this;
	}

	public bool IsVisible()
	{
		if (IsPlayer())
		{
			return true;
		}
		if (HasPropertyOrTag("Non"))
		{
			return false;
		}
		if (pPhysics == null)
		{
			return false;
		}
		if (pRender == null || !pRender.Visible)
		{
			return false;
		}
		if (HasPart("FungalVision") && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		if (HasPart("TerrainTravel"))
		{
			return true;
		}
		Cell currentCell = CurrentCell;
		if (currentCell != null)
		{
			if (OnWorldMap())
			{
				return true;
			}
			if (ThePlayer == null || currentCell.DistanceTo(ThePlayer) > ThePlayer.GetVisibilityRadius())
			{
				return false;
			}
			if (!ConsiderSolidInRenderingContext() && (int)currentCell.GetLight() < 228)
			{
				int i = 0;
				for (int count = currentCell.Objects.Count; i < count; i++)
				{
					GameObject gameObject = currentCell.Objects[i];
					if (gameObject != this && gameObject.ConsiderSolidInRenderingContext())
					{
						return false;
					}
				}
			}
			if (currentCell.IsVisible())
			{
				return true;
			}
		}
		if (InInventory != null && InInventory.IsPlayer())
		{
			return true;
		}
		if (Equipped != null && Equipped.IsVisible())
		{
			return true;
		}
		return false;
	}

	public string GetGenotype()
	{
		return GetPropertyOrTag("Genotype");
	}

	public string GetSubtype()
	{
		return GetPropertyOrTag("Subtype");
	}

	public bool IsTrueKin()
	{
		return IsTrueKinEvent.Check(this);
	}

	public bool IsMutant()
	{
		return IsMutantEvent.Check(this);
	}

	public int GetEpistemicStatus(Examiner pExaminer)
	{
		return pExaminer?.GetEpistemicStatus() ?? 2;
	}

	public bool SetEpistemicStatus(int epistemicStatus)
	{
		if (!(GetPart("Examiner") is Examiner examiner))
		{
			return false;
		}
		examiner.SetEpistemicStatus(epistemicStatus);
		return true;
	}

	public int GetEpistemicStatus()
	{
		return GetEpistemicStatus(GetPart("Examiner") as Examiner);
	}

	public bool MakeUnderstood()
	{
		if (!(GetPart("Examiner") is Examiner examiner))
		{
			return false;
		}
		return examiner.MakeUnderstood();
	}

	public bool Understood(Examiner pExaminer)
	{
		return GetEpistemicStatus(pExaminer) == 2;
	}

	public bool Understood()
	{
		return GetEpistemicStatus() == 2;
	}

	public bool MakePartiallyUnderstood()
	{
		if (!(GetPart("Examiner") is Examiner examiner))
		{
			return false;
		}
		return examiner.MakePartiallyUnderstood();
	}

	public bool PartiallyUnderstood(Examiner pExaminer)
	{
		return GetEpistemicStatus(pExaminer) == 1;
	}

	public bool PartiallyUnderstood()
	{
		return GetEpistemicStatus() == 1;
	}

	public int QueryCharge(bool LiveOnly = false, long GridMask = 0L, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		bool liveOnly = LiveOnly;
		return QueryChargeEvent.Retrieve(this, this, 1, GridMask, Forced: false, liveOnly, IncludeTransient, IncludeBiological);
	}

	public bool TestCharge(int Charge, bool LiveOnly = false, long GridMask = 0L, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Charge <= 0)
		{
			return true;
		}
		bool liveOnly = LiveOnly;
		return TestChargeEvent.Check(this, this, Charge, 1, GridMask, Forced: false, liveOnly, IncludeTransient, IncludeBiological);
	}

	public bool UseCharge(int Charge, bool LiveOnly = false, long GridMask = 0L, bool IncludeTransient = true, bool IncludeBiological = true, int? PowerLoadLevel = null)
	{
		if (Charge <= 0)
		{
			return true;
		}
		int powerLoadLevel = PowerLoadLevel ?? GetPowerLoadLevelEvent.GetFor(this);
		bool liveOnly;
		if (Equipped != null)
		{
			liveOnly = LiveOnly;
			UsingChargeEvent.Send(this, this, Charge, 1, GridMask, Forced: false, liveOnly, IncludeTransient, IncludeBiological, powerLoadLevel);
		}
		liveOnly = LiveOnly;
		return UseChargeEvent.Check(this, this, Charge, 1, GridMask, Forced: false, liveOnly, IncludeTransient, IncludeBiological, powerLoadLevel);
	}

	public int QueryChargeStorage(bool IncludeTransient = true, bool IncludeBiological = true, GameObject Source = null)
	{
		return QueryChargeStorageEvent.Retrieve(this, Source ?? this, IncludeTransient, 0L, Forced: false, LiveOnly: false, IncludeBiological);
	}

	public int QueryChargeStorage(out int Transient, out bool UnlimitedTransient, GameObject Source = null, bool IncludeBiological = true)
	{
		QueryChargeStorageEvent.Retrieve(out var E, this, Source ?? this, IncludeTransient: true, 0L, Forced: false, LiveOnly: false, IncludeBiological);
		if (E != null)
		{
			Transient = E.Transient;
			UnlimitedTransient = E.UnlimitedTransient;
			return E.Amount;
		}
		Transient = 0;
		UnlimitedTransient = false;
		return 0;
	}

	public int ChargeAvailable(int Charge, long GridMask = 0L, int MultipleCharge = 1, bool Forced = false, GameObject Source = null)
	{
		if (Charge <= 0)
		{
			return 0;
		}
		Charge *= MultipleCharge;
		ChargeAvailableEvent.Send(out var E, this, Source ?? this, Charge, MultipleCharge, GridMask, Forced);
		if (E == null)
		{
			return FinishChargeAvailableEvent.Send(this, Source ?? this, Charge, MultipleCharge, GridMask, Forced);
		}
		return FinishChargeAvailableEvent.Send(this, E);
	}

	public int QueryChargeProduction()
	{
		return QueryChargeProductionEvent.Retrieve(this, this, 0L);
	}

	public override string ToString()
	{
		if (IsPlayer())
		{
			return "The Player";
		}
		return Blueprint;
	}

	public void StripOffGear()
	{
		Body?.GetBody()?.UnequipPartAndChildren();
		Inventory inventory = Inventory;
		if (inventory == null)
		{
			return;
		}
		foreach (GameObject @object in inventory.GetObjects())
		{
			if (!@object.IsNatural())
			{
				inventory.RemoveObject(@object);
			}
		}
	}

	public void RandomlySpendPoints(int maxAPtospend = int.MaxValue, int maxSPtospend = int.MaxValue, int maxMPtospend = int.MaxValue, StringBuilder result = null)
	{
		int num = Math.Max(0, Stat("AP") - maxAPtospend);
		while (Stat("AP") > num)
		{
			int num2 = Stat("AP");
			Statistics["AP"].Penalty++;
			int num3 = XRL.Rules.Stat.Random(1, 6);
			if (num3 == 1)
			{
				Statistics["Strength"].BaseValue++;
			}
			if (num3 == 2)
			{
				Statistics["Intelligence"].BaseValue++;
			}
			if (num3 == 3)
			{
				Statistics["Willpower"].BaseValue++;
			}
			if (num3 == 4)
			{
				Statistics["Agility"].BaseValue++;
			}
			if (num3 == 5)
			{
				Statistics["Toughness"].BaseValue++;
			}
			if (num3 == 6)
			{
				Statistics["Ego"].BaseValue++;
			}
			if (Stat("AP") == num2)
			{
				break;
			}
		}
		int num4 = Math.Max(0, Stat("SP") - maxSPtospend);
		while (Stat("SP") > num4)
		{
			int num5 = Stat("SP");
			List<PowerEntry> list = new List<PowerEntry>(64);
			List<int> list2 = new List<int>();
			foreach (SkillEntry value in SkillFactory.Factory.SkillList.Values)
			{
				if (value.Initiatory == true && !HasSkill(value.Class))
				{
					continue;
				}
				foreach (PowerEntry value2 in value.Powers.Values)
				{
					int cost = value2.Cost;
					if (cost == 0)
					{
						cost = value.Cost;
					}
					if (cost <= GetStatValue("SP") && value2.MeetsRequirements(this))
					{
						list.Add(value2);
						list2.Add(cost);
					}
				}
			}
			if (list.Count <= 0)
			{
				break;
			}
			int index = XRL.Rules.Stat.Random(0, list2.Count - 1);
			Statistics["SP"].Penalty += list2[index];
			object obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Skill." + list[index].Class));
			GetPart<XRL.World.Parts.Skills>().AddSkill(obj as BaseSkill);
			if (Stat("SP") == num5)
			{
				break;
			}
		}
		int num6 = Math.Max(0, Stat("MP") - maxMPtospend);
		while (Stat("MP") > num6)
		{
			int num7 = Stat("MP");
			Mutations part = GetPart<Mutations>();
			if (part == null)
			{
				break;
			}
			if (Stat("MP") - num6 >= 4)
			{
				if (MutationsAPI.RandomlyMutate(this, null, null, allowMultipleDefects: false, result) == null)
				{
					break;
				}
				Statistics["MP"].Penalty += 4;
			}
			else
			{
				if (part.MutationList.Count == 0)
				{
					break;
				}
				BaseMutation randomElement = part.MutationList.Where((BaseMutation m) => m.CanIncreaseLevel()).GetRandomElement();
				if (randomElement != null)
				{
					UseMP(1);
					part.LevelMutation(randomElement, randomElement.BaseLevel + 1);
					if (result != null)
					{
						result.Append(" ");
						result.Append(Poss("base rank in " + randomElement.DisplayName + " increases to {{C|" + randomElement.BaseLevel + "}}!"));
					}
				}
			}
			if (Stat("MP") == num7)
			{
				break;
			}
		}
	}

	public int GetPsychicGlimmer(List<GameObject> domChain = null)
	{
		if (HasEffect("Dominated") && GetEffect("Dominated") is Dominated dominated && dominated.Dominator != null)
		{
			if (domChain == null)
			{
				domChain = new List<GameObject>(1) { this };
			}
			else
			{
				if (domChain.Contains(this))
				{
					goto IL_005a;
				}
				domChain.Add(this);
			}
			return dominated.Dominator.GetPsychicGlimmer(domChain);
		}
		goto IL_005a;
		IL_005a:
		return GetPsychicGlimmerEvent.GetFor(this, GetIntProperty("GlimmerModifier"));
	}

	public bool InActiveZone()
	{
		return CurrentZone == XRLCore.Core.Game.ZoneManager.ActiveZone;
	}

	public bool IsPotentiallyMobile()
	{
		bool immobile = true;
		bool waterbound = false;
		bool wallwalker = false;
		pBrain?.checkMobility(out immobile, out waterbound, out wallwalker);
		return !immobile;
	}

	public bool IsMobile()
	{
		if (!IsPotentiallyMobile())
		{
			return false;
		}
		if (IsFrozen())
		{
			return false;
		}
		return FireEvent(eIsMobile);
	}

	public bool CanHypersensesDetect()
	{
		return FireEvent(eCanHypersensesDetect);
	}

	public bool IsCarryingObject(string Blueprint)
	{
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			for (int i = 0; i < inventory.Objects.Count; i++)
			{
				if (inventory.Objects[i].Blueprint == Blueprint)
				{
					return true;
				}
			}
		}
		Body body = Body;
		if (body != null && body.HasEquippedItem(Blueprint))
		{
			return true;
		}
		return false;
	}

	public bool HasEquippedItem(string Blueprint)
	{
		return Body?.HasEquippedItem(Blueprint) ?? false;
	}

	public bool MovingIntoWouldCreateContainmentLoop(GameObject obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj.InInventory == null)
		{
			return false;
		}
		return MovingIntoWouldCreateContainmentLoop(obj.InInventory);
	}

	public bool IsPlayer()
	{
		if (XRLCore.Core.Game == null)
		{
			return false;
		}
		return this == XRLCore.Core.Game.Player.Body;
	}

	public bool IsOriginalPlayerBody()
	{
		return HasStringProperty("OriginalPlayerBody");
	}

	private string RetrieveEquipProfile(out GameObject User, out bool IsCyber)
	{
		string text = null;
		IsCyber = false;
		User = null;
		if (GetPart("CyberneticsBaseItem") is CyberneticsBaseItem cyberneticsBaseItem)
		{
			IsCyber = true;
			text = cyberneticsBaseItem.Slots;
			User = cyberneticsBaseItem.ImplantedOn;
			if (User == null)
			{
				return null;
			}
			return text;
		}
		User = Equipped;
		if (User == null)
		{
			return null;
		}
		text = UsesSlots;
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (GetPart("Armor") is Armor armor)
		{
			text = armor.WornOn;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (GetPart("Shield") is XRL.World.Parts.Shield shield)
		{
			text = shield.WornOn;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (GetPart("MissileWeapon") is MissileWeapon missileWeapon)
		{
			text = missileWeapon.SlotType;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon)
		{
			return meleeWeapon.Slot;
		}
		return null;
	}

	public GameObject EquippedProperlyBy()
	{
		bool IsCyber = false;
		GameObject User = null;
		string text = RetrieveEquipProfile(out User, out IsCyber);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		Body body = User?.Body;
		if (body == null)
		{
			return null;
		}
		if (IsCyber)
		{
			if (!body.CheckSlotCyberneticsMatch(this, text))
			{
				return null;
			}
		}
		else if (!body.CheckSlotEquippedMatch(this, text))
		{
			return null;
		}
		return User;
	}

	public bool IsEquippedProperly(BodyPart ProspectivelyCheckAgainstPart = null)
	{
		bool IsCyber = false;
		GameObject User = null;
		string text = RetrieveEquipProfile(out User, out IsCyber);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (ProspectivelyCheckAgainstPart == null)
		{
			if (IsCyber)
			{
				return User.Body?.CheckSlotCyberneticsMatch(this, text) ?? false;
			}
			return User.Body?.CheckSlotEquippedMatch(this, text) ?? false;
		}
		if (text.IndexOf(',') != -1)
		{
			List<string> list = text.CachedCommaExpansion();
			if (!list.Contains("*") && !list.Contains(ProspectivelyCheckAgainstPart.Type))
			{
				return false;
			}
		}
		else if (text != "*" && ProspectivelyCheckAgainstPart.Type != text)
		{
			return false;
		}
		return true;
	}

	public bool IsEquippedProperly(string OnType)
	{
		bool IsCyber = false;
		GameObject User = null;
		string text = RetrieveEquipProfile(out User, out IsCyber);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.IndexOf(',') != -1)
		{
			List<string> list = text.CachedCommaExpansion();
			if (!list.Contains("*") && !list.Contains(OnType))
			{
				return false;
			}
		}
		else if (text != "*" && OnType != text)
		{
			return false;
		}
		return true;
	}

	public bool IsWorn(BodyPart ProspectivelyCheckAgainstPart = null)
	{
		string text = null;
		if (!(GetPart("Armor") is Armor armor))
		{
			if (!(GetPart("Shield") is XRL.World.Parts.Shield shield))
			{
				return false;
			}
			text = UsesSlots;
			if (string.IsNullOrEmpty(text))
			{
				text = shield.WornOn;
			}
		}
		else
		{
			text = UsesSlots;
			if (string.IsNullOrEmpty(text))
			{
				text = armor.WornOn;
			}
		}
		if (ProspectivelyCheckAgainstPart == null)
		{
			return (Equipped?.Body?.CheckSlotEquippedMatch(this, text)).GetValueOrDefault();
		}
		if (text.IndexOf(',') != -1)
		{
			List<string> list = text.CachedCommaExpansion();
			if (!list.Contains("*") && !list.Contains(ProspectivelyCheckAgainstPart.Type))
			{
				return false;
			}
		}
		else if (text != "*" && ProspectivelyCheckAgainstPart.Type != text)
		{
			return false;
		}
		return true;
	}

	public bool IsEquippedOnLimbType(string Type)
	{
		if (Equipped == null)
		{
			return false;
		}
		return Equipped.Body?.IsItemEquippedOnLimbType(this, Type) ?? false;
	}

	public bool IsHeld()
	{
		if (!IsEquippedOnLimbType("Hand"))
		{
			return IsEquippedOnLimbType("Missile Weapon");
		}
		return true;
	}

	public bool IsEquippedOrDefaultOfPrimary(GameObject holder)
	{
		if (holder == null)
		{
			return false;
		}
		Body body = holder.Body;
		if (body == null)
		{
			return false;
		}
		return body.FindDefaultOrEquippedItem(this)?.Primary ?? false;
	}

	public bool IsEquippedInMainHand()
	{
		return EquippedOn()?.Primary ?? false;
	}

	public bool IsEquippedAsThrownWeapon()
	{
		return IsEquippedOnLimbType("Thrown Weapon");
	}

	public bool SameAs(GameObject GO)
	{
		if (Blueprint != GO.Blueprint)
		{
			return false;
		}
		if (PartsList.Count != GO.PartsList.Count)
		{
			return false;
		}
		if (((RegisteredPartEvents != null) ? RegisteredPartEvents.Count : 0) != ((GO.RegisteredPartEvents != null) ? GO.RegisteredPartEvents.Count : 0))
		{
			return false;
		}
		if (((_Effects != null) ? _Effects.Count : 0) != ((GO._Effects != null) ? GO._Effects.Count : 0))
		{
			return false;
		}
		if (((RegisteredEffectEvents != null) ? RegisteredEffectEvents.Count : 0) != ((GO.RegisteredEffectEvents != null) ? GO.RegisteredEffectEvents.Count : 0))
		{
			return false;
		}
		if (Statistics.Count != GO.Statistics.Count)
		{
			return false;
		}
		foreach (Statistic value2 in Statistics.Values)
		{
			if (!GO.Statistics.TryGetValue(value2.Name, out var value))
			{
				return false;
			}
			if (!value.SameAs(value2))
			{
				return false;
			}
		}
		for (int i = 0; i < PartsList.Count; i++)
		{
			IPart part = GO.GetPart(PartsList[i].Name);
			if (part == null)
			{
				return false;
			}
			if (!part.SameAs(PartsList[i]))
			{
				return false;
			}
		}
		if (_Effects != null && _Effects.Count > 0)
		{
			SameAsEffectsUsed.Clear();
			foreach (Effect effect in GO._Effects)
			{
				string name = effect.GetType().Name;
				bool flag = false;
				foreach (Effect effect2 in _Effects)
				{
					if (effect2.GetType().Name == name && effect2.SameAs(effect) && !SameAsEffectsUsed.Contains(effect2))
					{
						flag = true;
						SameAsEffectsUsed.Add(effect2);
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (SameAsEffectsUsed.Count != _Effects.Count)
			{
				return false;
			}
		}
		if (GetIntProperty("Important") != GO.GetIntProperty("Important"))
		{
			return false;
		}
		return true;
	}

	public int GetBodyPartCountEquippedOn(GameObject obj)
	{
		if (Body == null)
		{
			return 0;
		}
		return Body.GetPartCountEquippedOn(obj);
	}

	public int RemoveBodyPartsByManager(string Manager, bool EvenIfDismembered = false)
	{
		return Body?.RemovePartsByManager(Manager, EvenIfDismembered) ?? 0;
	}

	public List<BodyPart> GetBodyPartsByManager(string Manager, bool EvenIfDismembered = false)
	{
		List<BodyPart> list = new List<BodyPart>();
		Body?.GetPartsByManager(Manager, list, EvenIfDismembered);
		return list;
	}

	public BodyPart GetBodyPartByManager(string Manager, bool EvenIfDismembered = false)
	{
		return Body?.GetPartByManager(Manager, EvenIfDismembered);
	}

	public BodyPart GetBodyPartByManager(string Manager, string Type, bool EvenIfDismembered = false)
	{
		return Body?.GetPartByManager(Manager, Type, EvenIfDismembered);
	}

	public BodyPart GetBodyPartByID(int ID, bool EvenIfDismembered = false)
	{
		return Body?.GetPartByID(ID, EvenIfDismembered);
	}

	public BodyPart FindEquippedObject(GameObject GO)
	{
		return Body?.FindEquippedItem(GO);
	}

	public BodyPart FindEquippedObject(string Blueprint)
	{
		return Body?.FindEquippedItem(Blueprint);
	}

	public bool HasEquippedObject(string Blueprint)
	{
		Body body = Body;
		if (body == null)
		{
			return false;
		}
		return body.FindEquippedItem(Blueprint) != null;
	}

	public BodyPart FindCybernetics(GameObject GO)
	{
		return Body?.FindCybernetics(GO);
	}

	public bool IsADefaultBehavior(GameObject obj)
	{
		return Body?.IsADefaultBehavior(obj) ?? false;
	}

	public void Smoke(int StartAngle, int EndAngle)
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone.IsActive())
		{
			ParticleFX.Smoke(currentCell.X, currentCell.Y, StartAngle, EndAngle);
		}
	}

	public void Smoke()
	{
		Smoke(85, 185);
	}

	public GameObject GetPrimaryWeapon()
	{
		Body body = Body;
		if (body == null)
		{
			return null;
		}
		return body.GetMainWeapon(NeedPrimary: true, FailDownFromPrimary: true) ?? create("DefaultFist");
	}

	public Brain.CreatureOpinion GetOpinion(GameObject obj)
	{
		if (_pBrain == null)
		{
			return Brain.CreatureOpinion.neutral;
		}
		return pBrain.GetOpinion(obj);
	}

	public bool IsHostileTowards(GameObject obj)
	{
		if (_pBrain != null)
		{
			return _pBrain.IsHostileTowards(obj);
		}
		return false;
	}

	public bool IsAlliedTowards(GameObject obj)
	{
		if (_pBrain != null)
		{
			return _pBrain.IsAlliedTowards(obj);
		}
		return false;
	}

	public bool IsNeutralTowards(GameObject obj)
	{
		if (_pBrain != null)
		{
			return _pBrain.IsNeutralTowards(obj);
		}
		return false;
	}

	public bool IsRegardedWithHostilityBy(GameObject obj)
	{
		return obj.IsHostileTowards(this);
	}

	public bool IsRegardedAsAnAllyBy(GameObject obj)
	{
		return obj.IsAlliedTowards(this);
	}

	public bool IsRegardedNeutrallyBy(GameObject obj)
	{
		return obj.IsNeutralTowards(this);
	}

	public bool IsNonAggressive()
	{
		if (_pBrain != null)
		{
			return _pBrain.IsNonAggressive();
		}
		return true;
	}

	public bool IsRelevantHostile(GameObject GO)
	{
		int num = int.MinValue;
		int num2 = 9999999;
		if (IsPlayer())
		{
			if (XRLCore.Core.IDKFA)
			{
				return false;
			}
			num = Options.AutoexploreIgnoreEasyEnemies;
			num2 = Options.AutoexploreIgnoreDistantEnemies;
		}
		if (GO == this || GO.HasTag("ExcludeFromHostiles"))
		{
			return false;
		}
		int? num3 = GO.Con(this);
		if (!num3.HasValue || num3 < num)
		{
			return false;
		}
		if (GO.IsHostileTowards(this) && !GO.IsNonAggressive())
		{
			int hostileWalkRadius = GO.GetHostileWalkRadius(this);
			if (hostileWalkRadius > 0)
			{
				int num4 = DistanceTo(GO);
				if (num4 <= hostileWalkRadius && num4 <= num2 && num4 <= GetVisibilityRadius())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsVisibleHostile(GameObject GO)
	{
		if (IsPlayer() && XRLCore.Core.IDKFA)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (XRL.The.ZoneManager.ActiveZone != null)
		{
			ThePlayer.HandleEvent(XRLCore.eBeforeRender);
			XRL.The.ZoneManager.ActiveZone.AddVisibility(currentCell.X, currentCell.Y, GetVisibilityRadius());
		}
		int ignoreEasierThan = int.MinValue;
		int ignoreFartherThan = 9999999;
		if (IsPlayer())
		{
			ignoreEasierThan = Options.AutoexploreIgnoreEasyEnemies;
			ignoreFartherThan = Options.AutoexploreIgnoreDistantEnemies;
		}
		return IsVisibleHostileInternal(GO, ignoreEasierThan, ignoreFartherThan);
	}

	private bool IsVisibleHostileInternal(GameObject GO, int IgnoreEasierThan = int.MinValue, int IgnoreFartherThan = int.MaxValue)
	{
		if (GO == this || GO.HasTag("ExcludeFromHostiles"))
		{
			return false;
		}
		if (!GO.pRender.Visible)
		{
			return false;
		}
		if (GO.GetPart("Hidden") is Hidden hidden && !hidden.Found)
		{
			return false;
		}
		int? num = GO.Con(this);
		if (!num.HasValue || num < IgnoreEasierThan)
		{
			return false;
		}
		if (GO.IsHostileTowards(this) && !GO.IsNonAggressive())
		{
			int hostileWalkRadius = GO.GetHostileWalkRadius(this);
			if (hostileWalkRadius > 0)
			{
				int num2 = DistanceTo(GO);
				if (num2 <= hostileWalkRadius && num2 <= IgnoreFartherThan && num2 < GetVisibilityRadius())
				{
					Cell currentCell = GO.CurrentCell;
					if (currentCell.IsVisible() && currentCell.IsLit())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsVisibleHostileInternal(GameObject GO)
	{
		return IsVisibleHostileInternal(GO, int.MinValue, int.MaxValue);
	}

	public string GenerateSpotMessage(GameObject who, string Description = null, OngoingAction Action = null, string verb = "see", bool CheckingPrior = false, string setting = null, bool treatAsVisible = true)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("You ").Append(verb).Append(' ')
			.Append(who.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, ColorOnly: false, treatAsVisible, WithoutEpithet: false, Short: true, BaseOnly: false, WithIndefiniteArticle: true));
		Cell currentCell = CurrentCell;
		if (currentCell != null)
		{
			string generalDirectionFromCell = currentCell.GetGeneralDirectionFromCell(who.CurrentCell);
			if (generalDirectionFromCell != ".")
			{
				stringBuilder.Append(" to the ").Append(Directions.GetExpandedDirection(generalDirectionFromCell));
			}
		}
		stringBuilder.Append(CheckingPrior ? ", so you refrain from " : " and stop ").Append(Description ?? ((setting != AutoAct.Setting) ? AutoAct.GetDescription(setting, Action) : AutoAct.GetDescription())).Append('.');
		return stringBuilder.ToString();
	}

	public bool ArePerceptibleHostilesNearby(bool logSpot = false, bool popSpot = false, string Description = null, OngoingAction Action = null, string setting = null, int IgnoreEasierThan = int.MinValue, int IgnoreFartherThan = 40, bool IgnorePlayerTarget = false, bool CheckingPrior = false)
	{
		if (XRLCore.Core.IDKFA)
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (currentCell.ParentZone == null)
		{
			return false;
		}
		if (CheckingPrior && XRL.The.ZoneManager.ActiveZone != null)
		{
			ThePlayer.HandleEvent(XRLCore.eBeforeRender);
			XRL.The.ZoneManager.ActiveZone.AddVisibility(currentCell.X, currentCell.Y, GetVisibilityRadius());
		}
		GameObject gameObject = currentCell.ParentZone.FastSquareVisibilityFirst(currentCell.X, currentCell.Y, Math.Min(IgnoreFartherThan, 80), "Brain", IsVisibleHostileInternal, this, VisibleToPlayerOnly: true, IncludeWalls: true, IgnorePlayerTarget ? Sidebar.CurrentTarget : null, IgnoreFartherThan, IgnoreEasierThan);
		string verb = "see";
		bool treatAsVisible = true;
		if (gameObject == null && ExtraHostilePerceptionEvent.Check(this, out var Hostile, out var PerceiveVerb, out var TreatAsVisible))
		{
			gameObject = Hostile;
			verb = PerceiveVerb;
			treatAsVisible = TreatAsVisible;
		}
		if (gameObject != null)
		{
			if (logSpot || popSpot)
			{
				GameObject who = gameObject;
				bool checkingPrior = CheckingPrior;
				string message = GenerateSpotMessage(who, Description, Action, verb, checkingPrior, setting, treatAsVisible);
				if (popSpot)
				{
					Popup.Show(message, CopyScrap: true, Capitalize: true, DimBackground: true, logSpot);
				}
				else if (logSpot)
				{
					MessageQueue.AddPlayerMessage(message);
				}
				gameObject.Indicate();
			}
			return true;
		}
		return false;
	}

	public bool isAdjacentTo(GameObject go)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (currentCell.HasObject(go))
		{
			return true;
		}
		foreach (Cell localAdjacentCell in currentCell.GetLocalAdjacentCells())
		{
			if (localAdjacentCell != null && localAdjacentCell.HasObject(go))
			{
				return true;
			}
		}
		return false;
	}

	public bool AreHostilesAdjacent(bool RequireCombat = true)
	{
		if (RequireCombat)
		{
			return CurrentCell.AnyAdjacentCell((Cell C) => C.HasObjectWithPart("Combat", (GameObject GO) => GO != this && GO.IsHostileTowards(this)));
		}
		return CurrentCell.AnyAdjacentCell((Cell C) => C.HasObjectWithPart("Brain", (GameObject GO) => GO != this && GO.IsHostileTowards(this)));
	}

	public bool AreViableHostilesAdjacent()
	{
		return CurrentCell.AnyAdjacentCell((Cell C) => C.HasObjectWithPart("Combat", (GameObject GO) => GO != this && GO.IsHostileTowards(this) && GO.CanMoveExtremities()));
	}

	public void EquipFromPopulationTable(string Table, int ZoneTier = 1, Action<GameObject> Process = null, string Context = null, bool Silent = true)
	{
		Inventory part = GetPart<Inventory>();
		if (part == null)
		{
			return;
		}
		foreach (PopulationResult item in PopulationManager.Generate(Table, new Dictionary<string, string>
		{
			{
				"ownertier",
				GetTier().ToString()
			},
			{
				"ownertechtier",
				GetTechTier().ToString()
			},
			{
				"zonetier",
				ZoneTier.ToString()
			},
			{
				"zonetier+1",
				(ZoneTier + 1).ToString()
			}
		}))
		{
			try
			{
				for (int i = 0; i < item.Number; i++)
				{
					int bonusModChance = 0;
					if (item.Hint != null && item.Hint.Contains("SetBonusModChance:"))
					{
						bonusModChance = item.Hint.Split(':')[1].RollCached();
					}
					GameObject gameObject = ((!item.Blueprint.StartsWith("*relic:")) ? create(item.Blueprint, bonusModChance, 0, Context) : RelicGenerator.GenerateRelic(XRLCore.Core.Game.sultanHistory.entities.GetRandomElement().GetCurrentSnapshot(), ZoneTier, item.Blueprint.Split(':')[1]));
					Process?.Invoke(gameObject);
					Event @event = (Silent ? eCommandTakeObjectSilentWithEnergyCost : eCommandTakeObjectWithEnergyCost);
					@event.SetParameter("Object", gameObject);
					@event.SetParameter("Context", Context);
					@event.SetParameter("EnergyCost", 0);
					part.FireEvent(@event);
				}
			}
			catch (Exception)
			{
				Debug.LogError("Exception creating item from population table: " + item.Blueprint);
			}
		}
	}

	public void MutateFromPopulationTable(string Table, int ZoneTier = 1)
	{
		Mutations mutations = RequirePart<Mutations>();
		foreach (PopulationResult item in PopulationManager.Generate(Table, new Dictionary<string, string>
		{
			{
				"ownertier",
				GetTier().ToString()
			},
			{
				"ownertechtier",
				GetTechTier().ToString()
			},
			{
				"zonetier",
				ZoneTier.ToString()
			},
			{
				"zonetier+1",
				(ZoneTier + 1).ToString()
			}
		}))
		{
			if (item.Number <= 0)
			{
				continue;
			}
			Type type = ModManager.ResolveType("XRL.World.Parts.Mutation." + item.Blueprint);
			if (type == null)
			{
				MetricsManager.LogError("Unknown mutation " + item.Blueprint);
				continue;
			}
			if (!(Activator.CreateInstance(type) is BaseMutation baseMutation))
			{
				MetricsManager.LogError("Mutation " + item.Blueprint + " is not a BaseMutation");
				continue;
			}
			mutations.AddMutation(baseMutation, item.Number);
			if (baseMutation.CapOverride == -1)
			{
				baseMutation.CapOverride = baseMutation.Level;
			}
		}
	}

	private bool CheckHostile(GameObject GO)
	{
		if (GO == this)
		{
			return false;
		}
		if (GO.pBrain == null)
		{
			return false;
		}
		if (GO.HasTag("ExcludeFromHostiles"))
		{
			return false;
		}
		if (GO.IsHostileTowards(this) && !GO.IsNonAggressive())
		{
			int hostileWalkRadius = GO.GetHostileWalkRadius(this);
			if (hostileWalkRadius > 0 && DistanceTo(GO) <= hostileWalkRadius)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreHostilesNearby()
	{
		if (XRLCore.Core.IDKFA)
		{
			return false;
		}
		if (OnWorldMap())
		{
			return false;
		}
		Cell currentCell = CurrentCell;
		return CurrentZone?.GlobalFloodAny(currentCell.X, currentCell.Y, 7, "Combat", CheckHostile, this, ForFluid: true, CheckInWalls: true) ?? false;
	}

	public void Splash(string Particle)
	{
		CurrentCell?.Splash(Particle);
	}

	public void LiquidSplash(string Color)
	{
		CurrentCell?.LiquidSplash(Color);
	}

	public void LiquidSplash(List<string> Colors)
	{
		CurrentCell?.LiquidSplash(Colors);
	}

	public void LiquidSplash(BaseLiquid Liquid)
	{
		CurrentCell?.LiquidSplash(Liquid);
	}

	public void Splatter(string Particle)
	{
		if (CurrentZone != null && CurrentZone.IsActive())
		{
			for (int i = 0; i < 5; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = (float)XRL.Rules.Stat.RandomCosmetic(0, 359) / 58f;
				num = (float)Math.Sin(num3) / 2f;
				num2 = (float)Math.Cos(num3) / 2f;
				XRLCore.ParticleManager.Add(Particle, pPhysics.CurrentCell.X, pPhysics.CurrentCell.Y, num, num2, 5, 0f, 0f);
			}
		}
	}

	public void ShatterSplatter()
	{
		if (CurrentZone == null || !CurrentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			string text = ".";
			if (i == 0)
			{
				text = "&b.";
			}
			if (i == 0)
			{
				text = "&b,";
			}
			if (i == 0)
			{
				text = "&k'";
			}
			if (i == 0)
			{
				text = "&b.";
			}
			if (i == 0)
			{
				text = "&W.";
			}
			XRLCore.ParticleManager.Add(text, CurrentCell.X, CurrentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void FlingBlood()
	{
		if (Options.DisableBloodsplatter || pPhysics == null || GetIntProperty("Bleeds") <= 0)
		{
			return;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return;
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		localAdjacentCells.Add(currentCell);
		string propertyOrTag = GetPropertyOrTag("BleedLiquid", "blood-1000");
		foreach (Cell item in localAdjacentCells)
		{
			bool flag = false;
			if (50.in100())
			{
				foreach (GameObject item2 in item.GetObjectsWithPartReadonly("Render"))
				{
					LiquidVolume liquidVolume = item2.LiquidVolume;
					if (liquidVolume != null && liquidVolume.MaxVolume == -1)
					{
						liquidVolume.MixWith(new LiquidVolume(propertyOrTag, XRL.Rules.Stat.Random(1, 3)));
						flag = true;
					}
					else
					{
						item2.MakeBloody(propertyOrTag, XRL.Rules.Stat.Random(1, 3));
					}
				}
			}
			if (!flag && 5.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = propertyOrTag;
				item.AddObject(gameObject);
			}
		}
	}

	public void Bloodsplatter()
	{
		Bloodsplatter(bSelfsplatter: true);
	}

	public void BloodsplatterBurst(bool bSelfsplatter, float Angle, int ConeWidth)
	{
		if (Options.DisableBloodsplatter || GetIntProperty("Bleeds") <= 0)
		{
			return;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.ParentZone != XRLCore.Core.Game.ZoneManager.ActiveZone || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (HasPart("Robot"))
		{
			Sparksplatter();
		}
		else if (HasTag("Ooze"))
		{
			Slimesplatter(bSelfsplatter: true);
		}
		else
		{
			for (int i = 0; i < 10; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = ((float)XRL.Rules.Stat.Random(-ConeWidth / 2, ConeWidth / 2) + Angle) / 58f;
				num = (float)Math.Sin(num3) / 2f;
				num2 = (float)Math.Cos(num3) / 2f;
				if (XRL.Rules.Stat.Random(1, 2) == 1)
				{
					XRLCore.ParticleManager.Add("&r.", currentCell.X, currentCell.Y, num, num2, 7, 0f, 0f);
				}
				else
				{
					XRLCore.ParticleManager.Add("&R.", currentCell.X, currentCell.Y, num, num2, 7, 0f, 0f);
				}
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		string propertyOrTag = GetPropertyOrTag("BleedLiquid", "blood-1000");
		foreach (Cell item in localAdjacentCells)
		{
			bool flag = false;
			if (50.in100())
			{
				foreach (GameObject item2 in item.GetObjectsWithPartReadonly("Render"))
				{
					LiquidVolume liquidVolume = item2.LiquidVolume;
					if (liquidVolume != null && liquidVolume.MaxVolume == -1)
					{
						liquidVolume.MixWith(new LiquidVolume(propertyOrTag, XRL.Rules.Stat.Random(1, 3)));
						flag = true;
					}
					else
					{
						item2.MakeBloody(propertyOrTag, XRL.Rules.Stat.Random(1, 3));
					}
				}
			}
			if (!flag && 10.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = propertyOrTag;
				item.AddObject(gameObject);
			}
		}
	}

	public void SetActive()
	{
		XRLCore.Core.Game.ActionManager.AddActiveObject(this);
	}

	public bool MakeBloody(string Liquid = "blood", int Amount = 1, int Duration = 1)
	{
		return ForceApplyEffect(new LiquidCovered(Liquid, Amount, Duration));
	}

	public bool MakeBloodstained(string Liquid = "blood", int Amount = 1, int Duration = 9999)
	{
		return ForceApplyEffect(new LiquidStained(Liquid, Amount, Duration));
	}

	public void BloodsplatterCone(bool bSelfsplatter, float Angle, int ConeWidth)
	{
		if (Options.DisableBloodsplatter || GetIntProperty("Bleeds") <= 0)
		{
			return;
		}
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.ParentZone != XRLCore.Core.Game.ZoneManager.ActiveZone || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (HasPart("Robot"))
		{
			Sparksplatter();
		}
		else if (HasTag("Ooze"))
		{
			Slimesplatter(bSelfsplatter: true);
		}
		else
		{
			for (int i = 0; i < 10; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = ((float)XRL.Rules.Stat.Random(-ConeWidth / 2, ConeWidth / 2) + Angle) / 58f;
				num = (float)Math.Sin(num3) / 2f;
				num2 = (float)Math.Cos(num3) / 2f;
				if (XRL.Rules.Stat.Random(1, 2) == 1)
				{
					XRLCore.ParticleManager.Add("&r.", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
				}
				else
				{
					XRLCore.ParticleManager.Add("&R.", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
				}
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		string propertyOrTag = GetPropertyOrTag("BleedLiquid", "blood-1000");
		foreach (Cell item in localAdjacentCells)
		{
			bool flag = false;
			if (50.in100())
			{
				foreach (GameObject item2 in item.GetObjectsWithPartReadonly("Render"))
				{
					LiquidVolume liquidVolume = item2.LiquidVolume;
					if (liquidVolume != null && liquidVolume.MaxVolume == -1)
					{
						liquidVolume.MixWith(new LiquidVolume(propertyOrTag, XRL.Rules.Stat.Random(1, 3)));
						flag = true;
					}
					else
					{
						item2.MakeBloody(propertyOrTag, XRL.Rules.Stat.Random(1, 3));
					}
				}
			}
			if (!flag && 10.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = propertyOrTag;
				item.AddObject(gameObject);
			}
		}
	}

	public void HolographicBloodsplatter(bool bSelfsplatter = true)
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (currentCell.ParentZone.IsActive())
		{
			if (HasPart("Robot"))
			{
				Sparksplatter();
			}
			else if (HasTag("Ooze"))
			{
				Slimesplatter(bSelfsplatter: true);
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					float num = 0f;
					float num2 = 0f;
					float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
					num = (float)Math.Sin(num3) / 2f;
					num2 = (float)Math.Cos(num3) / 2f;
					XRLCore.ParticleManager.Add("&r.", currentCell.X, currentCell.Y, num, num2, 5, 0f, 0f);
				}
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		foreach (Cell item in localAdjacentCells)
		{
			if (10.in100())
			{
				item.ForeachObjectWithPart("Render", (Action<GameObject>)SplashHolographicBlood);
			}
			if (10.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = GetPropertyOrTag("BleedLiquid", "blood-1000");
				gameObject.AddPart(new XRL.World.Parts.Temporary(25));
				item.AddObject(gameObject);
			}
		}
	}

	private void SplashHolographicBlood(GameObject GO)
	{
		LiquidVolume liquidVolume = GO.LiquidVolume;
		if (liquidVolume == null || liquidVolume.MaxVolume != -1)
		{
			GO.MakeBloody("blood", XRL.Rules.Stat.Random(1, 3));
		}
	}

	public void BigBloodsplatter(bool bSelfsplatter = true)
	{
		if (GetIntProperty("Bleeds") <= 0)
		{
			return;
		}
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (currentCell.ParentZone.IsActive())
		{
			if (HasPart("Robot"))
			{
				Sparksplatter();
			}
			else if (HasTag("Ooze"))
			{
				Slimesplatter(bSelfsplatter: true);
			}
			else
			{
				for (int i = 0; i < 15; i++)
				{
					float num = 0f;
					float num2 = 0f;
					float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
					num = (float)Math.Sin(num3) / 2f;
					num2 = (float)Math.Cos(num3) / 2f;
					XRLCore.ParticleManager.Add("&r.", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
				}
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(3);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		string propertyOrTag = GetPropertyOrTag("BleedLiquid", "blood-1000");
		foreach (Cell item in localAdjacentCells)
		{
			bool flag = false;
			if (50.in100())
			{
				foreach (GameObject item2 in item.GetObjectsWithPartReadonly("Render"))
				{
					LiquidVolume liquidVolume = item2.LiquidVolume;
					if (liquidVolume != null && liquidVolume.MaxVolume == -1)
					{
						liquidVolume.MixWith(new LiquidVolume(propertyOrTag, XRL.Rules.Stat.Random(1, 3)));
						flag = true;
					}
					else
					{
						item2.MakeBloody(propertyOrTag, XRL.Rules.Stat.Random(1, 3));
					}
				}
			}
			if (!flag && 10.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = propertyOrTag;
				item.AddObject(gameObject);
			}
		}
	}

	public void Bloodsplatter(bool bSelfsplatter)
	{
		if (GetIntProperty("Bleeds") <= 0)
		{
			return;
		}
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (currentCell.ParentZone.IsActive())
		{
			if (HasPart("Robot"))
			{
				Sparksplatter();
			}
			else if (HasTag("Ooze"))
			{
				Slimesplatter(bSelfsplatter: true);
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					float num = 0f;
					float num2 = 0f;
					float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
					num = (float)Math.Sin(num3) / 2f;
					num2 = (float)Math.Cos(num3) / 2f;
					XRLCore.ParticleManager.Add("&r.", currentCell.X, currentCell.Y, num, num2, 5, 0f, 0f);
				}
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		string propertyOrTag = GetPropertyOrTag("BleedLiquid", "blood-1000");
		foreach (Cell item in localAdjacentCells)
		{
			bool flag = false;
			if (50.in100())
			{
				foreach (GameObject item2 in item.GetObjectsWithPartReadonly("Render"))
				{
					LiquidVolume liquidVolume = item2.LiquidVolume;
					if (liquidVolume != null && liquidVolume.MaxVolume == -1)
					{
						liquidVolume.MixWith(new LiquidVolume(propertyOrTag, XRL.Rules.Stat.Random(1, 3)));
						flag = true;
					}
					else
					{
						item2.MakeBloody(propertyOrTag, XRL.Rules.Stat.Random(1, 3));
					}
				}
			}
			if (!flag && 10.in100())
			{
				GameObject gameObject = create("BloodSplash");
				gameObject.LiquidVolume.InitialLiquid = propertyOrTag;
				item.AddObject(gameObject);
			}
		}
	}

	public void DilationSplat()
	{
		GetCurrentCell()?.DilationSplat();
	}

	public void ImplosionSplat(int Radius = 12)
	{
		GetCurrentCell()?.ImplosionSplat(Radius);
	}

	public void TelekinesisBlip()
	{
		GetCurrentCell()?.TelekinesisBlip();
	}

	public void Acidsplatter()
	{
		if (OnWorldMap() || CurrentZone == null || !CurrentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			int num4 = XRL.Rules.Stat.Random(1, 2);
			string text = "";
			if (num4 == 0)
			{
				text += "&G";
			}
			if (num4 == 1)
			{
				text += "&g";
			}
			text += ".";
			XRLCore.ParticleManager.Add(text, CurrentCell.X, CurrentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void Firesplatter()
	{
		if (OnWorldMap() || CurrentZone == null || !CurrentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			int num4 = XRL.Rules.Stat.Random(1, 2);
			string text = "";
			if (num4 == 0)
			{
				text += "&R";
			}
			if (num4 == 1)
			{
				text += "&W";
			}
			text += ".";
			XRLCore.ParticleManager.Add(text, CurrentCell.X, CurrentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void Icesplatter()
	{
		if (OnWorldMap() || CurrentZone == null || !CurrentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			int num4 = XRL.Rules.Stat.Random(1, 2);
			string text = "";
			if (num4 == 0)
			{
				text += "&C";
			}
			if (num4 == 1)
			{
				text += "&Y";
			}
			text += ".";
			XRLCore.ParticleManager.Add(text, CurrentCell.X, CurrentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void Sparksplatter()
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap() || !currentCell.ParentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			int num4 = XRL.Rules.Stat.Random(1, 2);
			string text = "";
			if (num4 == 0)
			{
				text += "&W";
			}
			if (num4 == 1)
			{
				text += "&Y";
			}
			text += (char)XRL.Rules.Stat.RandomCosmetic(191, 198);
			XRLCore.ParticleManager.Add(text, currentCell.X, currentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void Rainbowsplatter()
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap() || !currentCell.ParentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 2f;
			num2 = (float)Math.Cos(num3) / 2f;
			int num4 = XRL.Rules.Stat.Random(1, 2);
			string text = "";
			if (num4 == 0)
			{
				text = text + "&" + Crayons.GetRandomColor();
			}
			if (num4 == 1)
			{
				text = text + "&" + Crayons.GetRandomColor().ToLower();
			}
			text += (char)XRL.Rules.Stat.RandomCosmetic(191, 198);
			XRLCore.ParticleManager.Add(text, currentCell.X, currentCell.Y, num, num2, 5, 0f, 0f);
		}
	}

	public void Slimesplatter(bool bSelfsplatter, string particle = "&g.")
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (currentCell.ParentZone.IsActive())
		{
			for (int i = 0; i < 5; i++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = (float)XRL.Rules.Stat.Random(0, 359) / 58f;
				num = (float)Math.Sin(num3) / 2f;
				num2 = (float)Math.Cos(num3) / 2f;
				XRLCore.ParticleManager.Add(particle, currentCell.X, currentCell.Y, num, num2, 5, 0f, 0f);
			}
		}
		List<Cell> localAdjacentCells = currentCell.GetLocalAdjacentCells(1);
		if (bSelfsplatter)
		{
			localAdjacentCells.Add(currentCell);
		}
		foreach (Cell item in localAdjacentCells)
		{
			if (!50.in100())
			{
				continue;
			}
			item.ForeachObjectWithPart("Render", delegate(GameObject GO)
			{
				if (GO.HasPart("LiquidVolume") && GO.LiquidVolume.MaxVolume == -1)
				{
					LiquidVolume liquidVolume = GO.LiquidVolume;
					LiquidVolume liquidVolume2 = GameObjectFactory.Factory.CreateObject("Water").LiquidVolume;
					liquidVolume2.InitialLiquid = "slime-1000";
					liquidVolume2.Volume = 2;
					liquidVolume.MixWith(liquidVolume2);
				}
			});
		}
	}

	public void DotPuff(string Color)
	{
		if (CurrentZone == null || !CurrentZone.IsActive())
		{
			return;
		}
		for (int i = 0; i < 15; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 4f;
			num2 = (float)Math.Cos(num3) / 4f;
			if (XRL.Rules.Stat.RandomCosmetic(1, 4) <= 3)
			{
				XRLCore.ParticleManager.Add(Color + ".", CurrentCell.X, CurrentCell.Y, num, num2, 15, 0f, 0f);
			}
			else
			{
				XRLCore.ParticleManager.Add(Color + "ù", CurrentCell.X, CurrentCell.Y, num, num2, 15, 0f, 0f);
			}
		}
	}

	public void PistonPuff(string color = "&y", int intensity = 15)
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone != XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			return;
		}
		for (int i = 0; i < intensity; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 4f;
			num2 = (float)Math.Cos(num3) / 4f;
			if (XRL.Rules.Stat.RandomCosmetic(1, 4) <= 2)
			{
				XRLCore.ParticleManager.Add(color + ".", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
			}
			else
			{
				XRLCore.ParticleManager.Add(color + "±", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
			}
		}
	}

	public void DustPuff(string color = "&y", int intensity = 15)
	{
		if (!IsVisible())
		{
			return;
		}
		Cell currentCell = GetCurrentCell();
		if (currentCell == null || currentCell.ParentZone != XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			return;
		}
		for (int i = 0; i < intensity; i++)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)XRL.Rules.Stat.RandomCosmetic(0, 359) / 58f;
			num = (float)Math.Sin(num3) / 4f;
			num2 = (float)Math.Cos(num3) / 4f;
			if (XRL.Rules.Stat.RandomCosmetic(1, 4) <= 3)
			{
				XRLCore.ParticleManager.Add(color + ".", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
			}
			else
			{
				XRLCore.ParticleManager.Add(color + "±", currentCell.X, currentCell.Y, num, num2, 15, 0f, 0f);
			}
		}
	}

	public void PsychicPulse()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				ParticleText("&B" + (char)(219 + XRL.Rules.Stat.RandomCosmetic(0, 4)), 4.9f, 5);
			}
			for (int k = 0; k < 5; k++)
			{
				ParticleText("&b" + (char)(219 + XRL.Rules.Stat.RandomCosmetic(0, 4)), 4.9f, 5);
			}
			for (int l = 0; l < 5; l++)
			{
				ParticleText("&W" + (char)(219 + XRL.Rules.Stat.RandomCosmetic(0, 4)), 4.9f, 5);
			}
		}
	}

	public void Soundwave()
	{
		Cell currentCell = GetCurrentCell();
		if (currentCell != null && currentCell.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone)
		{
			for (int i = 0; i < 3; i++)
			{
				XRLCore.ParticleManager.AddRadial("&R!", currentCell.X, currentCell.Y, XRL.Rules.Stat.RandomCosmetic(0, 7), XRL.Rules.Stat.RandomCosmetic(1, 1), 0.015f * (float)XRL.Rules.Stat.RandomCosmetic(8, 12), 0.3f + 0.05f * (float)XRL.Rules.Stat.RandomCosmetic(1, 3), 40);
				XRLCore.ParticleManager.AddRadial("&r!", currentCell.X, currentCell.Y, XRL.Rules.Stat.RandomCosmetic(0, 7), XRL.Rules.Stat.RandomCosmetic(1, 1), 0.015f * (float)XRL.Rules.Stat.RandomCosmetic(8, 12), 0.3f + 0.05f * (float)XRL.Rules.Stat.RandomCosmetic(1, 3), 40);
				XRLCore.ParticleManager.AddRadial("&R\r", currentCell.X, currentCell.Y, XRL.Rules.Stat.RandomCosmetic(0, 7), XRL.Rules.Stat.RandomCosmetic(1, 1), 0.015f * (float)XRL.Rules.Stat.RandomCosmetic(8, 12), 0.3f + 0.05f * (float)XRL.Rules.Stat.RandomCosmetic(1, 3), 40);
				XRLCore.ParticleManager.AddRadial("&r\u000e", currentCell.X, currentCell.Y, XRL.Rules.Stat.RandomCosmetic(0, 7), XRL.Rules.Stat.RandomCosmetic(1, 1), 0.015f * (float)XRL.Rules.Stat.RandomCosmetic(8, 12), 0.3f + 0.05f * (float)XRL.Rules.Stat.RandomCosmetic(1, 3), 40);
			}
		}
	}

	public bool IsInActiveZone()
	{
		return CurrentZone?.IsActive() ?? false;
	}

	public void ParticleSpray(string Text1, string Text2, string Text3, string Text4, int amount)
	{
		if (!IsInActiveZone())
		{
			return;
		}
		Cell currentCell = CurrentCell;
		for (int i = 0; i < 16; i++)
		{
			string text = Text1;
			if (XRL.Rules.Stat.RandomCosmetic(0, 3) == 0)
			{
				text = Text2;
			}
			if (XRL.Rules.Stat.RandomCosmetic(0, 3) == 0)
			{
				text = Text3;
			}
			if (XRL.Rules.Stat.RandomCosmetic(0, 3) == 0)
			{
				text = Text4;
			}
			float yDel = -0.5f + (float)XRL.Rules.Stat.RandomCosmetic(1, 4) * -0.2f;
			float xDel = -1f + (float)XRL.Rules.Stat.RandomCosmetic(1, 30) * 0.06f;
			XRLCore.ParticleManager.Add(text, currentCell.X, currentCell.Y, xDel, yDel, 60, 0f, 0.05f);
		}
	}

	public void Heartspray(string Color1 = "&M", string Color2 = "&R", string Color3 = "&r", string Color4 = "&Y", char c = '\u0003')
	{
		ParticleSpray(Color1 + c, Color4 + ".", Color2 + c, Color3 + c, 16);
	}

	public void ParticlePulse(string particle)
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				ParticleText(particle, 4.9f, 5);
			}
		}
	}

	public void TileParticleBlip(string Tile, string ColorString, string DetailColor, int Duration, bool IgnoreVisibility = false, bool HFlip = false, bool VFlip = false)
	{
		if (IsInActiveZone() && (IgnoreVisibility || IsVisible()))
		{
			XRLCore.ParticleManager.AddTile(Tile, ColorString, DetailColor, CurrentCell.X, CurrentCell.Y, 0f, 0f, Duration, 0f, 0f, HFlip, VFlip);
		}
	}

	public void ParticleBlip(string Text, int Duration, bool IgnoreVisibility = false)
	{
		if (IsInActiveZone() && (IgnoreVisibility || IsVisible()))
		{
			XRLCore.ParticleManager.Add(Text, CurrentCell.X, CurrentCell.Y, 0f, 0f, Duration, 0f, 0f);
		}
	}

	public void ParticleBlip(string Text, bool IgnoreVisibility = false)
	{
		if (IsInActiveZone() && (IgnoreVisibility || IsVisible()))
		{
			XRLCore.ParticleManager.Add(Text, CurrentCell.X, CurrentCell.Y, 0f, 0f, 10, 0f, 0f);
		}
	}

	public void ParticleText(string Text, float Velocity, int Life)
	{
		CurrentCell?.ParticleText(Text, Velocity, Life);
	}

	public void ParticleText(string Text, bool IgnoreVisibility = false)
	{
		if (IgnoreVisibility || IsVisible())
		{
			CurrentCell?.ParticleText(Text, IgnoreVisibility);
		}
	}

	public void ParticleText(string Text, float xVel, float yVel, char color = ' ', bool IgnoreVisibility = false)
	{
		if (IgnoreVisibility || IsVisible())
		{
			CurrentCell?.ParticleText(Text, xVel, yVel, color, IgnoreVisibility);
		}
	}

	public void ParticleText(string Text, char color, bool IgnoreVisibility = false, float juiceDuration = 1.5f, float floatLength = -8f)
	{
		if (IgnoreVisibility || IsVisible())
		{
			CurrentCell?.ParticleText(Text, color, IgnoreVisibility: true, juiceDuration, floatLength, this);
		}
	}

	public string GetUnitName()
	{
		string text = pRender.DisplayName;
		if (HasPart("Book") || HasPart("MarkovBook") || HasPart("Cookbook"))
		{
			return "copy of " + text;
		}
		if (HasPart("CyberneticsBaseItem"))
		{
			text += " implant";
		}
		string text2 = GetxTag("Grammar", "groupTerm");
		if (!string.IsNullOrEmpty(text2))
		{
			return text2 + " of " + text;
		}
		if (IsPluralIfKnown)
		{
			return "set of " + text;
		}
		return text;
	}

	public string GetPluralName()
	{
		string text = pRender.DisplayName;
		if (HasPart("Book") || HasPart("MarkovBook") || HasPart("Cookbook"))
		{
			return "copies of " + text;
		}
		if (HasPart("CyberneticsBaseItem"))
		{
			text += " implant";
		}
		string text2 = GetxTag("Grammar", "groupTerm");
		if (!string.IsNullOrEmpty(text2))
		{
			return Grammar.Pluralize(text2) + " of " + text;
		}
		if (IsPluralIfKnown)
		{
			return "sets of " + text;
		}
		return Grammar.Pluralize(text);
	}

	public string GetDemandName(int DemandCount)
	{
		string text = pRender.DisplayName.Strip();
		StringBuilder stringBuilder = new StringBuilder();
		if (HasPart("Book") || HasPart("MarkovBook") || HasPart("Cookbook"))
		{
			if (DemandCount > 1)
			{
				stringBuilder.Append(Grammar.Cardinal(DemandCount));
				stringBuilder.Append(" copies of ");
				stringBuilder.Append(text);
			}
			else
			{
				stringBuilder.Append("a copy of ");
				stringBuilder.Append(Grammar.Pluralize(text));
			}
		}
		else
		{
			if (HasPart("CyberneticsBaseItem"))
			{
				text += " implant";
			}
			string text2 = GetxTag("Grammar", "groupTerm");
			if (!string.IsNullOrEmpty(text2))
			{
				if (DemandCount > 1)
				{
					stringBuilder.Append(Grammar.Cardinal(DemandCount)).Append(' ').Append(Grammar.Pluralize(text2));
				}
				else
				{
					stringBuilder.Append(Grammar.A(text2));
				}
				stringBuilder.Append(" of ").Append(text);
			}
			else if (IsPluralIfKnown)
			{
				if (DemandCount > 1)
				{
					stringBuilder.Append(Grammar.Cardinal(DemandCount));
					stringBuilder.Append(" sets of ");
				}
				else
				{
					stringBuilder.Append("a set of ");
				}
				stringBuilder.Append(text);
			}
			else if (DemandCount > 1)
			{
				stringBuilder.Append(Grammar.Cardinal(DemandCount));
				stringBuilder.Append(" ");
				stringBuilder.Append(Grammar.Pluralize(text));
			}
			else
			{
				stringBuilder.Append(a);
				stringBuilder.Append(text);
			}
		}
		return stringBuilder.ToString();
	}

	public GameObject GetNearestVisibleObject(bool Hostile = false, string SearchPart = "Physics", int Radius = 80, bool IncludeSolid = true, bool IgnoreLOS = false, Predicate<GameObject> ExtraVisibility = null)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null || currentCell.ParentZone == null)
		{
			return null;
		}
		GameObject result = null;
		int num = 9999999;
		List<GameObject> list = ((Radius >= currentCell.ParentZone.Width) ? currentCell.ParentZone.GetObjectsWithPartReadonly(SearchPart) : currentCell.ParentZone.FastFloodVisibility(currentCell.X, currentCell.Y, Radius, SearchPart, this, ExtraVisibility));
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject == this || !gameObject.IsVisible() || (!IgnoreLOS && !HasLOSTo(gameObject, IncludeSolid, UseTargetability: true)))
			{
				continue;
			}
			Cell currentCell2 = gameObject.CurrentCell;
			if (currentCell2 == null)
			{
				continue;
			}
			int num2 = currentCell2.PathDistanceTo(currentCell);
			if (num2 >= num)
			{
				continue;
			}
			if (Hostile)
			{
				if (gameObject.IsHostileTowards(this))
				{
					result = gameObject;
					num = num2;
				}
			}
			else
			{
				result = gameObject;
				num = num2;
			}
		}
		return result;
	}

	public List<GameObject> GetVisibleCombatObjects()
	{
		return CurrentZone?.FastFloodVisibility(CurrentCell.X, CurrentCell.Y, 999, "Combat", this);
	}

	public bool InSameCellAs(GameObject GO)
	{
		Cell currentCell = CurrentCell;
		Cell currentCell2 = GO.CurrentCell;
		if (currentCell == currentCell2)
		{
			return true;
		}
		if (currentCell != null && currentCell2 != null)
		{
			return false;
		}
		if (currentCell == null)
		{
			currentCell = GetCurrentCell();
		}
		else if (currentCell2 == null)
		{
			currentCell2 = GO.GetCurrentCell();
		}
		return currentCell == currentCell2;
	}

	public bool InAdjacentCellTo(GameObject GO)
	{
		Cell currentCell = CurrentCell;
		Cell currentCell2 = GO.CurrentCell;
		if (currentCell != null && currentCell2 != null && currentCell.IsAdjacentTo(currentCell2))
		{
			return true;
		}
		if (currentCell != null && currentCell2 != null)
		{
			return false;
		}
		if (currentCell == null)
		{
			currentCell = GetCurrentCell();
			if (currentCell == null)
			{
				return false;
			}
		}
		else if (currentCell2 == null)
		{
			currentCell2 = GO.GetCurrentCell();
			if (currentCell2 == null)
			{
				return false;
			}
		}
		return currentCell.IsAdjacentTo(currentCell2);
	}

	public bool InSameOrAdjacentCellTo(GameObject GO)
	{
		Cell currentCell = CurrentCell;
		Cell currentCell2 = GO.CurrentCell;
		if (currentCell == currentCell2)
		{
			return true;
		}
		if (currentCell != null && currentCell2 != null && currentCell.IsAdjacentTo(currentCell2))
		{
			return true;
		}
		if (currentCell != null && currentCell2 != null)
		{
			return false;
		}
		if (currentCell == null)
		{
			currentCell = GetCurrentCell();
			if (currentCell == null)
			{
				return false;
			}
		}
		else if (currentCell2 == null)
		{
			currentCell2 = GO.GetCurrentCell();
			if (currentCell2 == null)
			{
				return false;
			}
		}
		if (currentCell != currentCell2)
		{
			return currentCell.IsAdjacentTo(currentCell2);
		}
		return true;
	}

	public bool IsEngagedInMelee()
	{
		if (pBrain == null)
		{
			return false;
		}
		GameObject target = pBrain.Target;
		if (target != null)
		{
			return DistanceTo(target) <= 1;
		}
		return false;
	}

	public bool IsRealityDistortionUsable()
	{
		return FireEvent(eCheckRealityDistortionUsabilityThreshold100);
	}

	public bool IsSelfControlledPlayer()
	{
		if (!IsPlayer())
		{
			return false;
		}
		if (HasStringProperty("Skittishing"))
		{
			return false;
		}
		return true;
	}

	public bool IsPlayerLed()
	{
		if (pBrain != null)
		{
			return pBrain.IsPlayerLed();
		}
		return false;
	}

	public bool IsPlayerControlled()
	{
		if (!IsPlayer())
		{
			return IsPlayerLed();
		}
		return true;
	}

	public bool IsPlayerLedAndPerceptible()
	{
		if (IsPlayerLed())
		{
			if (!IsVisible())
			{
				return IsAudible(ThePlayer);
			}
			return true;
		}
		return false;
	}

	public bool IsPlayerControlledAndPerceptible()
	{
		if (!IsPlayer())
		{
			return IsPlayerLedAndPerceptible();
		}
		return true;
	}

	public string GetPerceptionVerb()
	{
		if (!IsVisible())
		{
			return "hear";
		}
		return "see";
	}

	public bool IsOverburdened()
	{
		return Inventory?.IsOverburdened() ?? false;
	}

	public bool WouldBeOverburdened(int Weight)
	{
		return Inventory?.WouldBeOverburdened(Weight) ?? false;
	}

	public bool WouldBeOverburdened(GameObject GO)
	{
		return Inventory?.WouldBeOverburdened(GO) ?? false;
	}

	public bool MakeSave(out int SuccessMargin, out int FailureMargin, string Stat, int Difficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false, GameObject Source = null)
	{
		return XRL.Rules.Stat.MakeSave(out SuccessMargin, out FailureMargin, this, Stat, Difficulty, Attacker, AttackerStat, Vs, IgnoreNaturals, IgnoreNatural1, IgnoreNatural20, IgnoreGodmode, Source);
	}

	public bool MakeSave(string Stat, int Difficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false, GameObject Source = null)
	{
		int SuccessMargin;
		int FailureMargin;
		return MakeSave(out SuccessMargin, out FailureMargin, Stat, Difficulty, Attacker, AttackerStat, Vs, IgnoreNaturals, IgnoreNatural1, IgnoreNatural20, IgnoreGodmode, Source);
	}

	public int SaveChance(string Stat, int Difficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false, GameObject Source = null)
	{
		return XRL.Rules.Stat.SaveChance(this, Stat, Difficulty, Attacker, AttackerStat, Vs, IgnoreNaturals, IgnoreNatural1, IgnoreNatural20, IgnoreGodmode, Source);
	}

	public bool IsCopyOf(GameObject who)
	{
		if (who.IsPlayer() && HasStringProperty("PlayerCopy"))
		{
			return true;
		}
		string stringProperty = GetStringProperty("FugueCopy");
		if (stringProperty != null && who.idmatch(stringProperty))
		{
			return true;
		}
		return false;
	}

	public bool HasCopyRelationship(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (who == this)
		{
			return true;
		}
		if (who.IsPlayer() && HasStringProperty("PlayerCopy"))
		{
			return true;
		}
		if (IsPlayer() && who.HasStringProperty("PlayerCopy"))
		{
			return true;
		}
		string stringProperty = GetStringProperty("FugueCopy");
		if (stringProperty != null && who.idmatch(stringProperty))
		{
			return true;
		}
		string stringProperty2 = who.GetStringProperty("FugueCopy");
		if (stringProperty2 != null)
		{
			if (stringProperty2 == stringProperty)
			{
				return true;
			}
			if (idmatch(stringProperty2))
			{
				return true;
			}
		}
		return false;
	}

	public int GetMark()
	{
		string tag = GetTag("Mark");
		if (tag != null)
		{
			return Convert.ToInt32(tag);
		}
		return 0;
	}

	public bool LeftBehindByPlayer()
	{
		GameObject gameObject = ThePlayer;
		Dominated dominated;
		do
		{
			dominated = gameObject?.GetEffect("Dominated") as Dominated;
			if (dominated != null)
			{
				if (dominated.Dominator == this)
				{
					return true;
				}
				gameObject = dominated.Dominator;
			}
		}
		while (dominated != null && gameObject != null && gameObject != ThePlayer);
		return false;
	}

	public bool WasPlayer()
	{
		if (IsOriginalPlayerBody())
		{
			return true;
		}
		if (LeftBehindByPlayer())
		{
			return true;
		}
		return false;
	}

	public bool CanChangeMovementMode(string To = null, bool ShowMessage = false, bool Involuntary = false, bool AllowTelekinetic = false)
	{
		bool silent = !ShowMessage;
		if (!CheckFrozen(Telepathic: false, AllowTelekinetic, silent))
		{
			return false;
		}
		if (AllowTelekinetic && CanManipulateTelekinetically(this))
		{
			return true;
		}
		eCanChangeMovementMode.SetParameter("To", To);
		eCanChangeMovementMode.SetFlag("ShowMessage", ShowMessage);
		eCanChangeMovementMode.SetFlag("Involuntary", Involuntary);
		return FireEvent(eCanChangeMovementMode);
	}

	public bool CanChangeBodyPosition(string To = null, bool ShowMessage = false, bool Involuntary = false, bool AllowTelekinetic = false)
	{
		bool silent = !ShowMessage;
		if (!CheckFrozen(Telepathic: false, AllowTelekinetic, silent))
		{
			return false;
		}
		if (AllowTelekinetic && CanManipulateTelekinetically(this))
		{
			return true;
		}
		eCanChangeBodyPosition.SetParameter("To", To);
		eCanChangeBodyPosition.SetFlag("ShowMessage", ShowMessage);
		eCanChangeBodyPosition.SetFlag("Involuntary", Involuntary);
		return FireEvent(eCanChangeBodyPosition);
	}

	public bool CanMoveExtremities(string To = null, bool ShowMessage = false, bool Involuntary = false, bool AllowTelekinetic = false)
	{
		bool silent = !ShowMessage;
		if (!CheckFrozen(Telepathic: false, AllowTelekinetic, silent))
		{
			return false;
		}
		if (AllowTelekinetic && CanManipulateTelekinetically(this))
		{
			return true;
		}
		eCanMoveExtremities.SetParameter("To", To);
		eCanMoveExtremities.SetFlag("ShowMessage", ShowMessage);
		eCanMoveExtremities.SetFlag("Involuntary", Involuntary);
		return FireEvent(eCanMoveExtremities);
	}

	public void MovementModeChanged(string To = null, bool Involuntary = false)
	{
		MovementModeChangedEvent.Send(this, To, Involuntary);
	}

	public void BodyPositionChanged(string To = null, bool Involuntary = false)
	{
		BodyPositionChangedEvent.Send(this, To, Involuntary);
	}

	public void ExtremitiesMoved(string To = null, bool Involuntary = false)
	{
		ExtremitiesMovedEvent.Send(this, To, Involuntary);
	}

	public bool WasThrown(GameObject By, GameObject At = null)
	{
		if (!BeforeAfterThrownEvent.Check(By, this, At))
		{
			return false;
		}
		if (!AfterThrownEvent.Check(By, this, At))
		{
			return false;
		}
		if (!AfterAfterThrownEvent.Check(By, this, At))
		{
			return false;
		}
		return true;
	}

	public bool IsOpenLiquidVolume()
	{
		return LiquidVolume?.IsOpenVolume() ?? false;
	}

	public bool IsDangerousOpenLiquidVolume()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume == null)
		{
			return false;
		}
		if (liquidVolume.IsOpenVolume())
		{
			return liquidVolume.ConsiderLiquidDangerousToContact();
		}
		return false;
	}

	public bool IsSwimmingDepthLiquid()
	{
		return LiquidVolume?.IsSwimmingDepth() ?? false;
	}

	public bool IsWadingDepthLiquid()
	{
		return LiquidVolume?.IsWadingDepth() ?? false;
	}

	public bool IsSwimmableFor(GameObject who)
	{
		return LiquidVolume?.IsSwimmableFor(who) ?? false;
	}

	public bool IsHealingPool()
	{
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume == null)
		{
			return false;
		}
		if (liquidVolume.MaxVolume != -1)
		{
			return false;
		}
		return liquidVolume.ContainsSignificantLiquid("convalessence");
	}

	public bool HasReadyMissileWeapon()
	{
		return Body?.HasReadyMissileWeapon() ?? false;
	}

	public bool HasMissileWeapon()
	{
		return Body?.HasMissileWeapon() ?? false;
	}

	public List<GameObject> GetMissileWeapons()
	{
		return Body?.GetMissileWeapons();
	}

	public BodyPart GetFirstBodyPart(string Type)
	{
		return Body?.GetFirstPart(Type);
	}

	public BodyPart GetFirstBodyPart(string Type, int Laterality)
	{
		return Body?.GetFirstPart(Type, Laterality);
	}

	public BodyPart GetFirstBodyPart(Predicate<BodyPart> Filter)
	{
		return Body?.GetFirstPart(Filter);
	}

	public BodyPart GetFirstBodyPart(string Type, Predicate<BodyPart> Filter)
	{
		return Body?.GetFirstPart(Type, Filter);
	}

	public BodyPart GetFirstBodyPart(string Type, int Laterality, Predicate<BodyPart> Filter)
	{
		return Body?.GetFirstPart(Type, Laterality, Filter);
	}

	public BodyPart GetFirstBodyPart(string Type, bool EvenIfDismembered)
	{
		return Body?.GetFirstPart(Type, EvenIfDismembered);
	}

	public BodyPart GetFirstBodyPart(string Type, int Laterality, bool EvenIfDismembered)
	{
		return Body?.GetFirstPart(Type, Laterality, EvenIfDismembered);
	}

	public BodyPart GetFirstBodyPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.GetFirstPart(Filter, EvenIfDismembered);
	}

	public BodyPart GetFirstBodyPart(string Type, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.GetFirstPart(Type, Filter, EvenIfDismembered);
	}

	public BodyPart GetFirstBodyPart(string Type, int Laterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.GetFirstPart(Type, Laterality, Filter, EvenIfDismembered);
	}

	public bool HasBodyPart(string Type)
	{
		return Body?.HasPart(Type) ?? false;
	}

	public bool HasBodyPart(string Type, int Laterality)
	{
		return Body?.HasPart(Type, Laterality) ?? false;
	}

	public bool HasBodyPart(Predicate<BodyPart> Filter)
	{
		return Body?.HasPart(Filter) ?? false;
	}

	public bool HasBodyPart(string Type, Predicate<BodyPart> Filter)
	{
		return Body?.HasPart(Type, Filter) ?? false;
	}

	public bool HasBodyPart(string Type, bool EvenIfDismembered)
	{
		return Body?.HasPart(Type, EvenIfDismembered) ?? false;
	}

	public bool HasBodyPart(string Type, int Laterality, bool EvenIfDismembered)
	{
		return Body?.HasPart(Type, Laterality, EvenIfDismembered) ?? false;
	}

	public bool HasBodyPart(Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.HasPart(Filter, EvenIfDismembered) ?? false;
	}

	public bool HasBodyPart(string Type, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.HasPart(Type, Filter, EvenIfDismembered) ?? false;
	}

	public bool HasBodyPart(string Type, int Laterality, Predicate<BodyPart> Filter, bool EvenIfDismembered)
	{
		return Body?.HasPart(Type, Laterality, Filter, EvenIfDismembered) ?? false;
	}

	public bool CheckInfluence(string Type = null, GameObject By = null, bool Silent = false)
	{
		if (By == null)
		{
			By = ThePlayer;
			if (By == null)
			{
				return false;
			}
		}
		Event @event = Event.New("CanBeInfluenced", "Type", Type, "By", By, "Message", null);
		if (FireEvent(@event))
		{
			return true;
		}
		if (By.IsPlayer())
		{
			string stringParameter = @event.GetStringParameter("Message");
			if (string.IsNullOrEmpty(stringParameter))
			{
				Popup.ShowFail("Nothing happens.");
			}
			else
			{
				Popup.Show(GameText.VariableReplace(stringParameter, this));
			}
		}
		return false;
	}

	public bool IsAudible(GameObject By, int Volume = 20)
	{
		if (By == null)
		{
			return false;
		}
		return CurrentCell?.IsAudible(By, Volume) ?? false;
	}

	public bool IsSmellable(GameObject By)
	{
		if (By == null)
		{
			return false;
		}
		return CurrentCell?.IsSmellable(By, GetIntProperty("SmellIntensity", 5)) ?? false;
	}

	public Cell GetDropCell()
	{
		Cell cell = CurrentCell ?? GetCurrentCell();
		if (cell != null && cell.IsSolidOtherThan(this))
		{
			foreach (Cell adjacentCell in cell.GetAdjacentCells())
			{
				if (!adjacentCell.IsSolid())
				{
					return adjacentCell;
				}
			}
			return cell;
		}
		return cell;
	}

	public bool CanBeTargetedByPlayer()
	{
		if (IsPlayerControlled())
		{
			return false;
		}
		if (!Statistics.ContainsKey("Hitpoints"))
		{
			return false;
		}
		if (CurrentCell == null)
		{
			return false;
		}
		return true;
	}

	public int AwardXP(int Amount, int Tier = -1, int Minimum = 0, int Maximum = int.MaxValue, GameObject Kill = null, GameObject InfluencedBy = null, GameObject PassedUpFrom = null, GameObject PassedDownFrom = null, string Deed = null)
	{
		return AwardXPEvent.Send(this, Amount, Tier, Minimum, Maximum, Kill, InfluencedBy, PassedUpFrom, PassedDownFrom, Deed);
	}

	public int AwardXPTo(GameObject who, bool ForKill = true, string Deed = null, bool MockAward = false)
	{
		int result = 0;
		if (!HasTagOrProperty("NoXP") && Statistics.TryGetValue("XPValue", out var value))
		{
			int intProperty = GetIntProperty("*XPValue", value.Value);
			if (intProperty > 0)
			{
				int tier = -1;
				if (Statistics.TryGetValue("Level", out var value2))
				{
					tier = value2.Value / 5;
				}
				result = (MockAward ? intProperty : who.AwardXP(intProperty, tier, 0, int.MaxValue, ForKill ? this : null, ForKill ? null : this, null, null, Deed));
			}
			Statistics.Remove("XPValue");
		}
		return result;
	}

	public void StopFighting(bool Involuntary = false)
	{
		if (pBrain != null)
		{
			pBrain.StopFighting(Involuntary);
		}
	}

	public void StopFighting(GameObject who, bool Involuntary = false)
	{
		if (pBrain != null)
		{
			pBrain.StopFighting(who, Involuntary);
		}
	}

	public bool GetAngryAt(GameObject who, int Amount = -50)
	{
		return pBrain?.GetAngryAt(who, Amount) ?? false;
	}

	public bool LikeBetter(GameObject who, int Amount = 50)
	{
		return pBrain?.LikeBetter(who, Amount) ?? false;
	}

	public string GetWaterRitualLiquid(GameObject Actor = null)
	{
		return GetWaterRitualLiquidEvent.GetFor(Actor ?? ThePlayer, this);
	}

	public string GetWaterRitualLiquidName(GameObject Actor = null)
	{
		return LiquidVolume.getLiquid(GetWaterRitualLiquid(Actor)).GetName();
	}

	public int ResistMentalIntrusion(string Type, int Magnitude, GameObject Attacker)
	{
		Event @event = Event.New("ResistMentalIntrusion");
		@event.SetParameter("Type", Type);
		@event.SetParameter("Magnitude", Magnitude);
		@event.SetParameter("Attacker", Attacker);
		@event.SetParameter("Defender", this);
		FireEvent(@event);
		return @event.GetIntParameter("Magnitude");
	}

	public void FlushContextWeightCaches()
	{
		(InInventory ?? Equipped)?.FlushWeightCaches();
	}

	public void FlushWeightCaches()
	{
		Inventory?.FlushWeightCache();
		Body?.FlushWeightCache();
	}

	public void FlushWantTurnTickCache()
	{
		WantTurnTickCache = 0;
	}

	public bool WantTurnTick()
	{
		if (((uint)WantTurnTickCache & (true ? 1u : 0u)) != 0)
		{
			return true;
		}
		if ((WantTurnTickCache & 2u) != 0)
		{
			return false;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].WantTurnTick())
			{
				WantTurnTickCache |= 1;
				return true;
			}
		}
		WantTurnTickCache |= 2;
		return false;
	}

	public void TurnTick(long TurnNumber)
	{
		if (TurnTickPartsInUse)
		{
			IPart part = null;
			IPart part2 = null;
			IPart part3 = null;
			List<IPart> list = null;
			int i = 0;
			for (int count = PartsList.Count; i < count; i++)
			{
				IPart part4 = PartsList[i];
				if (part4.WantTurnTick())
				{
					if (list != null)
					{
						list.Add(part4);
						continue;
					}
					if (part == null)
					{
						part = part4;
						continue;
					}
					if (part2 == null)
					{
						part2 = part4;
						continue;
					}
					if (part3 == null)
					{
						part3 = part4;
						continue;
					}
					list = new List<IPart>(4) { part, part2, part3, part4 };
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].TurnTick(TurnNumber);
				}
			}
			else if (part != null)
			{
				part.TurnTick(TurnNumber);
				if (part2 != null)
				{
					part2.TurnTick(TurnNumber);
					part3?.TurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickPartsInUse = true;
		try
		{
			TurnTickParts.Clear();
			int k = 0;
			for (int count3 = PartsList.Count; k < count3; k++)
			{
				IPart part5 = PartsList[k];
				if (part5.WantTurnTick())
				{
					TurnTickParts.Add(part5);
				}
			}
			int l = 0;
			for (int count4 = TurnTickParts.Count; l < count4; l++)
			{
				TurnTickParts[l].TurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickPartsInUse = false;
		}
	}

	public bool WantTenTurnTick()
	{
		if ((WantTurnTickCache & 4u) != 0)
		{
			return true;
		}
		if ((WantTurnTickCache & 8u) != 0)
		{
			return false;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].WantTenTurnTick())
			{
				WantTurnTickCache |= 4;
				return true;
			}
		}
		WantTurnTickCache |= 8;
		return false;
	}

	public void TenTurnTick(long TurnNumber)
	{
		if (TurnTickPartsInUse)
		{
			IPart part = null;
			IPart part2 = null;
			IPart part3 = null;
			List<IPart> list = null;
			int i = 0;
			for (int count = PartsList.Count; i < count; i++)
			{
				IPart part4 = PartsList[i];
				if (part4.WantTenTurnTick())
				{
					if (list != null)
					{
						list.Add(part4);
						continue;
					}
					if (part == null)
					{
						part = part4;
						continue;
					}
					if (part2 == null)
					{
						part2 = part4;
						continue;
					}
					if (part3 == null)
					{
						part3 = part4;
						continue;
					}
					list = new List<IPart>(4) { part, part2, part3, part4 };
				}
			}
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					list[j].TenTurnTick(TurnNumber);
				}
			}
			else if (part != null)
			{
				part.TenTurnTick(TurnNumber);
				if (part2 != null)
				{
					part2.TenTurnTick(TurnNumber);
					part3?.TenTurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickPartsInUse = true;
		try
		{
			TurnTickParts.Clear();
			int k = 0;
			for (int count3 = PartsList.Count; k < count3; k++)
			{
				IPart part5 = PartsList[k];
				if (part5.WantTenTurnTick())
				{
					TurnTickParts.Add(part5);
				}
			}
			int l = 0;
			for (int count4 = TurnTickParts.Count; l < count4; l++)
			{
				TurnTickParts[l].TenTurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickPartsInUse = false;
		}
	}

	public bool WantHundredTurnTick()
	{
		if ((WantTurnTickCache & 0x10u) != 0)
		{
			return true;
		}
		if ((WantTurnTickCache & 0x20u) != 0)
		{
			return false;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].WantHundredTurnTick())
			{
				WantTurnTickCache |= 16;
				return true;
			}
		}
		WantTurnTickCache |= 32;
		return false;
	}

	public void HundredTurnTick(long TurnNumber)
	{
		if (TurnTickPartsInUse)
		{
			IPart part = null;
			IPart part2 = null;
			IPart part3 = null;
			List<IPart> list = null;
			int i = 0;
			for (int count = PartsList.Count; i < count; i++)
			{
				IPart part4 = PartsList[i];
				if (part4.WantHundredTurnTick())
				{
					if (list != null)
					{
						list.Add(part4);
						continue;
					}
					if (part == null)
					{
						part = part4;
						continue;
					}
					if (part2 == null)
					{
						part2 = part4;
						continue;
					}
					if (part3 == null)
					{
						part3 = part4;
						continue;
					}
					list = new List<IPart>(4) { part, part2, part3, part4 };
				}
			}
			if (list != null)
			{
				foreach (IPart item in list)
				{
					item.HundredTurnTick(TurnNumber);
				}
				return;
			}
			if (part != null)
			{
				part.HundredTurnTick(TurnNumber);
				if (part2 != null)
				{
					part2.HundredTurnTick(TurnNumber);
					part3?.HundredTurnTick(TurnNumber);
				}
			}
			return;
		}
		TurnTickPartsInUse = true;
		try
		{
			TurnTickParts.Clear();
			int j = 0;
			for (int count2 = PartsList.Count; j < count2; j++)
			{
				IPart part5 = PartsList[j];
				if (part5.WantHundredTurnTick())
				{
					TurnTickParts.Add(part5);
				}
			}
			int k = 0;
			for (int count3 = TurnTickParts.Count; k < count3; k++)
			{
				TurnTickParts[k].HundredTurnTick(TurnNumber);
			}
		}
		finally
		{
			TurnTickPartsInUse = false;
		}
	}

	public GameObject AddAsActiveObject()
	{
		XRLCore.Core.Game.ActionManager.AddActiveObject(this);
		return this;
	}

	public ActivatedAbilityEntry GetActivatedAbility(Guid ID)
	{
		ActivatedAbilities activatedAbilities = ActivatedAbilities;
		if (activatedAbilities == null)
		{
			return null;
		}
		if (activatedAbilities.AbilityByGuid == null)
		{
			return null;
		}
		if (activatedAbilities.AbilityByGuid.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public bool RemoveActivatedAbility(ref Guid ID)
	{
		bool result = false;
		if (ID != Guid.Empty)
		{
			ActivatedAbilities activatedAbilities = ActivatedAbilities;
			if (activatedAbilities != null)
			{
				result = activatedAbilities.RemoveAbility(ID);
			}
			ID = Guid.Empty;
		}
		return result;
	}

	public bool EnableActivatedAbility(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.Enabled = true;
			return true;
		}
		return false;
	}

	public bool DisableActivatedAbility(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.Enabled = false;
			return true;
		}
		return false;
	}

	public bool ToggleActivatedAbility(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.ToggleState = !activatedAbility.ToggleState;
			return true;
		}
		return false;
	}

	public bool IsActivatedAbilityToggledOn(Guid ID)
	{
		return GetActivatedAbility(ID)?.ToggleState ?? false;
	}

	public bool IsActivatedAbilityCoolingDown(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility == null)
		{
			return false;
		}
		return activatedAbility.Cooldown > 0;
	}

	public int GetActivatedAbilityCooldown(Guid ID)
	{
		return GetActivatedAbility(ID)?.Cooldown ?? 0;
	}

	public int GetActivatedAbilityCooldownTurns(Guid ID)
	{
		return GetActivatedAbility(ID)?.CooldownTurns ?? 0;
	}

	public string GetActivatedAbilityCooldownDescription(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility == null)
		{
			return "";
		}
		return activatedAbility.CooldownDescription;
	}

	public bool CooldownActivatedAbility(Guid ID, int Turns, string tags = null, bool Involuntary = false)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		Event @event = Event.New("BeforeCooldownActivatedAbility");
		@event.SetParameter("AbilityEntry", activatedAbility);
		@event.SetParameter("Turns", Turns);
		@event.SetParameter("Tags", tags);
		@event.SetFlag("Involuntary", Involuntary);
		if (!FireEvent(@event))
		{
			return true;
		}
		Turns = @event.GetIntParameter("Turns");
		if (Turns <= 0)
		{
			return true;
		}
		if (activatedAbility != null)
		{
			activatedAbility.Cooldown = (Turns + 1) * 10;
			return true;
		}
		return false;
	}

	public bool TakeActivatedAbilityOffCooldown(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.Cooldown = 0;
			return true;
		}
		return false;
	}

	public bool IsActivatedAbilityUsable(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility == null)
		{
			return false;
		}
		if (!activatedAbility.Enabled)
		{
			return false;
		}
		if (activatedAbility.Toggleable && !activatedAbility.ToggleState && !activatedAbility.ActiveToggle)
		{
			return false;
		}
		if (activatedAbility.Cooldown > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsActivatedAbilityAIUsable(Guid ID)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility == null)
		{
			return false;
		}
		if (activatedAbility.AIDisable)
		{
			return false;
		}
		if (!activatedAbility.Enabled)
		{
			return false;
		}
		if (activatedAbility.Toggleable && !activatedAbility.ToggleState && !activatedAbility.ActiveToggle)
		{
			return false;
		}
		if (activatedAbility.Cooldown > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsActivatedAbilityAIDisabled(Guid ID)
	{
		return GetActivatedAbility(ID)?.AIDisable ?? false;
	}

	public bool IsActivatedAbilityVoluntarilyUsable(Guid ID)
	{
		if (!IsPlayer())
		{
			return IsActivatedAbilityAIUsable(ID);
		}
		return IsActivatedAbilityUsable(ID);
	}

	public bool SetActivatedAbilityDisplayName(Guid ID, string DisplayName)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.DisplayName = DisplayName;
			return true;
		}
		return false;
	}

	public bool SetActivatedAbilityDisabledMessage(Guid ID, string DisabledMessage)
	{
		ActivatedAbilityEntry activatedAbility = GetActivatedAbility(ID);
		if (activatedAbility != null)
		{
			activatedAbility.DisabledMessage = DisabledMessage;
			return true;
		}
		return false;
	}

	public Guid AddActivatedAbility(string Name, string Command, string Class, string Description = null, string Icon = "\a", string DisabledMessage = null, bool Toggleable = false, bool DefaultToggleState = false, bool ActiveToggle = false, bool IsAttack = false, bool IsRealityDistortionBased = false, bool Silent = false, bool AIDisable = false, bool AlwaysAllowToggleOff = true, bool AffectedByWillpower = true, bool TickPerTurn = false, bool Distinct = false, int Cooldown = -1)
	{
		return ActivatedAbilities?.AddAbility(Name, Command, Class, Description, Icon, DisabledMessage, Toggleable, DefaultToggleState, ActiveToggle, IsAttack, IsRealityDistortionBased, Silent, AIDisable, AlwaysAllowToggleOff, AffectedByWillpower, TickPerTurn, Distinct, Cooldown) ?? Guid.Empty;
	}

	public int GetHighestActivatedAbilityCooldown()
	{
		int num = -1;
		ActivatedAbilities activatedAbilities = ActivatedAbilities;
		if (activatedAbilities != null && activatedAbilities.AbilityByGuid != null)
		{
			foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
			{
				int cooldown = value.Cooldown;
				if (cooldown > num)
				{
					num = cooldown;
				}
			}
			return num;
		}
		return num;
	}

	public int GetHighestActivatedAbilityCooldownTurns()
	{
		int num = -1;
		ActivatedAbilities activatedAbilities = ActivatedAbilities;
		if (activatedAbilities != null && activatedAbilities.AbilityByGuid != null)
		{
			foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
			{
				int cooldownTurns = value.CooldownTurns;
				if (cooldownTurns > num)
				{
					num = cooldownTurns;
				}
			}
			return num;
		}
		return num;
	}

	public void Seen()
	{
		XRLCore.Core.Game.BlueprintSeen(Blueprint);
	}

	public bool BlueprintSeen()
	{
		return XRLCore.Core.Game.HasBlueprintBeenSeen(Blueprint);
	}

	public string GetSpecies()
	{
		return GetPropertyOrTag("Species") ?? Blueprint;
	}

	public string GetCulture()
	{
		return GetPropertyOrTag("Culture") ?? GetSpecies();
	}

	public bool WantEvent(int ID, int cascade)
	{
		if (ID == OwnerGetInventoryActionsEvent.ID && IsPlayer())
		{
			return true;
		}
		if (ID == EnteredCellEvent.ID && IsPlayer())
		{
			return true;
		}
		if (ID == InventoryActionEvent.ID)
		{
			return true;
		}
		if (ID == ContainsAnyBlueprintEvent.ID)
		{
			return true;
		}
		if (ID == ContainsEvent.ID)
		{
			return true;
		}
		if (ID == ContainsBlueprintEvent.ID)
		{
			return true;
		}
		if (ID == FindObjectByIdEvent.ID)
		{
			return true;
		}
		if (ID == MakeTemporaryEvent.ID)
		{
			return true;
		}
		if (ID == EndTurnEvent.ID && IsPlayer())
		{
			return true;
		}
		if (ID == GlimmerChangeEvent.ID && IsPlayer())
		{
			return true;
		}
		if (ID == GetPointsOfInterestEvent.ID && CurrentCell != null && IsMarkedImportantByPlayer())
		{
			return true;
		}
		if (ID == IsRepairableEvent.ID)
		{
			return true;
		}
		if (ID == RepairedEvent.ID)
		{
			return true;
		}
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			if (PartsList[i].WantEvent(ID, cascade))
			{
				return true;
			}
		}
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			if (Effects[j].WantEvent(ID, cascade))
			{
				return true;
			}
		}
		return false;
	}

	public static MethodInfo getHandleEventMethod(Type handlingType, Type handledType)
	{
		if (!handleEventLookup.TryGetValue(handlingType, out var value))
		{
			value = new Dictionary<Type, MethodInfo>();
			handleEventLookup.Add(handlingType, value);
		}
		MethodInfo value2 = null;
		if (!value.TryGetValue(handledType, out value2))
		{
			handleEventMethodParameterList[0] = handledType;
			value2 = handlingType.GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.Public, null, handleEventMethodParameterList, null);
			if (value2 != null && value2.GetParameters()[0].ParameterType.Name == "MinEvent")
			{
				value2 = null;
			}
			value.Add(handledType, value2);
		}
		return value2;
	}

	public static bool callHandleEventMethod(object obj, MethodInfo method, MinEvent E)
	{
		if (method == null)
		{
			return true;
		}
		bool flag = false;
		object[] array;
		if (!handleEventMethodArgumentList1InUse)
		{
			array = handleEventMethodArgumentList1;
			handleEventMethodArgumentList1InUse = true;
		}
		else if (!handleEventMethodArgumentList2InUse)
		{
			array = handleEventMethodArgumentList2;
			handleEventMethodArgumentList2InUse = true;
		}
		else if (!handleEventMethodArgumentList3InUse)
		{
			array = handleEventMethodArgumentList3;
			handleEventMethodArgumentList3InUse = true;
		}
		else
		{
			array = new object[1];
			flag = true;
		}
		array[0] = E;
		try
		{
			return (bool)method.Invoke(obj, array);
		}
		catch (ArgumentException ex)
		{
			Debug.LogError("failed to call event handler " + obj.GetType().FullName + "::" + method.Name + "(" + method.GetParameters()[0].ParameterType.Name + ") with parameter " + E.GetType().FullName);
			throw ex;
		}
		finally
		{
			array[0] = null;
			if (!flag)
			{
				if (array == handleEventMethodArgumentList1)
				{
					handleEventMethodArgumentList1InUse = false;
				}
				else if (array == handleEventMethodArgumentList2)
				{
					handleEventMethodArgumentList2InUse = false;
				}
				else if (array == handleEventMethodArgumentList3)
				{
					handleEventMethodArgumentList3InUse = false;
				}
			}
		}
	}

	public bool HandleEvent<T>(T E) where T : MinEvent
	{
		int iD = E.ID;
		if (iD == OwnerGetInventoryActionsEvent.ID)
		{
			if (!HandleOwnerGetInventoryActionsEvent(E as OwnerGetInventoryActionsEvent))
			{
				return false;
			}
		}
		else if (iD == EnteredCellEvent.ID)
		{
			if (!HandleEnteredCellEvent(E as EnteredCellEvent))
			{
				return false;
			}
		}
		else if (iD == InventoryActionEvent.ID)
		{
			if (!HandleInventoryActionEvent(E as InventoryActionEvent))
			{
				return false;
			}
		}
		else if (iD == ContainsAnyBlueprintEvent.ID)
		{
			if (E is ContainsAnyBlueprintEvent containsAnyBlueprintEvent && containsAnyBlueprintEvent.Blueprints.Contains(Blueprint) && containsAnyBlueprintEvent.Container != this)
			{
				containsAnyBlueprintEvent.Object = this;
				return false;
			}
		}
		else if (iD == ContainsEvent.ID)
		{
			if (E is ContainsEvent containsEvent && containsEvent.Object == this && containsEvent.Container != this)
			{
				return false;
			}
		}
		else if (iD == ContainsBlueprintEvent.ID)
		{
			if (E is ContainsBlueprintEvent containsBlueprintEvent && containsBlueprintEvent.Blueprint == Blueprint && containsBlueprintEvent.Container != this)
			{
				containsBlueprintEvent.Object = this;
				return false;
			}
		}
		else if (iD == FindObjectByIdEvent.ID)
		{
			if (E is FindObjectByIdEvent findObjectByIdEvent && idmatch(findObjectByIdEvent.FindID))
			{
				findObjectByIdEvent.Object = this;
				return false;
			}
		}
		else if (iD == MakeTemporaryEvent.ID)
		{
			if (!HandleMakeTemporaryEvent(E as MakeTemporaryEvent))
			{
				return false;
			}
		}
		else if (iD == GlimmerChangeEvent.ID)
		{
			if (IsPlayer())
			{
				PsychicGlimmer.Update(this);
			}
		}
		else if (iD == GetPointsOfInterestEvent.ID)
		{
			if (CurrentCell != null && IsMarkedImportantByPlayer())
			{
				(E as GetPointsOfInterestEvent).Add(this, BaseDisplayName, null, null, null, CurrentCell.location);
			}
		}
		else if (iD == IsRepairableEvent.ID)
		{
			if ((!HasTag("Creature") || GetIntProperty("Inorganic") > 0) && isDamaged())
			{
				return false;
			}
		}
		else if (iD == RepairedEvent.ID && (!HasTag("Creature") || GetIntProperty("Inorganic") > 0))
		{
			Heal(500, Message: false, FloatText: true);
		}
		bool result = HandleEventInner(E, iD);
		if (iD == EndTurnEvent.ID)
		{
			if (IsPlayer())
			{
				Zone currentZone = CurrentZone;
				if (currentZone != null)
				{
					currentZone.LastPlayerPresence = XRLCore.CurrentTurn;
				}
			}
			CleanEffects();
		}
		return result;
	}

	public bool HandleEvent<T>(T E, IEvent ParentEvent) where T : MinEvent
	{
		bool result = HandleEvent(E);
		ParentEvent?.ProcessChildEvent(E);
		return result;
	}

	private bool HandleEventInner(MinEvent E, int ID)
	{
		int cascadeLevel = E.GetCascadeLevel();
		bool flag = E.WantInvokeDispatch();
		Type handledType = (flag ? E.GetType() : null);
		int i = 0;
		for (int count = PartsList.Count; i < count; i++)
		{
			IPart part = PartsList[i];
			if (!part.WantEvent(ID, cascadeLevel))
			{
				continue;
			}
			if (!flag && !E.handlePartDispatch(part))
			{
				return false;
			}
			if (flag && !callHandleEventMethod(part, getHandleEventMethod(part.GetType(), handledType), E))
			{
				return false;
			}
			if (!part.HandleEvent(E))
			{
				return false;
			}
			if (count != PartsList.Count)
			{
				count = PartsList.Count;
				if (i < count && PartsList[i] != part)
				{
					i--;
				}
			}
		}
		int j = 0;
		for (int count2 = Effects.Count; j < count2; j++)
		{
			Effect effect = Effects[j];
			if (!effect.WantEvent(ID, cascadeLevel))
			{
				continue;
			}
			if (!flag && !E.handleEffectDispatch(effect))
			{
				return false;
			}
			if (flag && !callHandleEventMethod(effect, getHandleEventMethod(effect.GetType(), handledType), E))
			{
				return false;
			}
			if (count2 != Effects.Count)
			{
				count2 = Effects.Count;
				if (j < count2 && Effects[j] != effect)
				{
					j--;
				}
			}
		}
		return true;
	}

	private bool HandleMakeTemporaryEvent(MakeTemporaryEvent E)
	{
		XRL.World.Parts.Temporary temporary = GetPart("Temporary") as XRL.World.Parts.Temporary;
		int num = E.Duration;
		GameObject gameObject = E.DependsOn;
		if (E.RootObject != this)
		{
			num = -1;
			gameObject = E.RootObject;
		}
		if (temporary == null)
		{
			AddPart(new XRL.World.Parts.Temporary(num, E.TurnInto));
		}
		else
		{
			if (num != -1 && (temporary.Duration == -1 || temporary.Duration < num))
			{
				temporary.Duration = E.Duration;
			}
			if (!string.IsNullOrEmpty(E.TurnInto) && string.IsNullOrEmpty(temporary.TurnInto))
			{
				temporary.TurnInto = E.TurnInto;
			}
		}
		if (gameObject != null)
		{
			RequirePart<ExistenceSupport>().SupportedBy = gameObject;
		}
		return true;
	}

	private bool HandleOwnerGetInventoryActionsEvent(OwnerGetInventoryActionsEvent E)
	{
		if (E == null)
		{
			MetricsManager.LogError("called with null event");
			return true;
		}
		if (E.Actor == this && IsPlayer() && E.Object != null)
		{
			if (E.Object.IsPlayerLed() && !E.Object.IsPlayer())
			{
				E.AddAction("Attack Target", "direct to attack target", "CompanionAttackTarget", "attack", 'a', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
				if (E.Object.pBrain != null)
				{
					if (E.Object.pBrain.Passive)
					{
						E.AddAction("Aggressive Engagement", "direct to engage aggressively", "CompanionToggleEngagement", "engage", 'e', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
					}
					else
					{
						E.AddAction("Passive Engagement", "direct to engage defensively only", "CompanionToggleEngagement", "engage", 'e', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
					}
					if (E.Object.IsPotentiallyMobile() && E.Object.pBrain.Staying)
					{
						E.AddAction("Come", "direct to come along", "CompanionCome", "come", 'c', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
					}
				}
				if (E.Object.WillTrade())
				{
					E.AddAction("Give Items", "give items", "CompanionGiveItems", null, 'g', FireOnActor: true);
				}
				if (E.Object.IsPotentiallyMobile())
				{
					E.AddAction("Move To", "direct to move", "CompanionMoveTo", null, 'm', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
				}
				if (!E.Object.HasProperName || E.Object.GetIntProperty("Renamed") == 1)
				{
					E.AddAction("Rename", "rename", "CompanionRename", null, 'r', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
				}
				if (E.Object.IsPotentiallyMobile() && E.Object.pBrain != null && !E.Object.pBrain.Staying)
				{
					E.AddAction("Stay", "direct to stay there", "CompanionStay", null, 's', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
				}
				ActivatedAbilities activatedAbilities = E.Object.ActivatedAbilities;
				if (activatedAbilities != null && activatedAbilities.GetAbilityCount() > 0)
				{
					E.AddAction("Change Ability Use", "direct ability use", "CompanionAbilityUse", null, 'u', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
				}
			}
			if (E.Object == Sidebar.CurrentTarget)
			{
				E.AddAction("Untarget", "untarget", "Untarget", null, 't', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
			else if (E.Object.CanBeTargetedByPlayer())
			{
				E.AddAction("Target", "target", "Target", null, 't', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
			}
			E.AddAction("Show Effects", "show effects", "ShowEffects", null, 'w', FireOnActor: true, 0, 0, Override: false, WorksAtDistance: true);
		}
		return true;
	}

	private bool HandleEnteredCellEvent(EnteredCellEvent E)
	{
		if (IsPlayer() && (!AutoAct.IsActive() || AutoAct.IsMovement()) && !AutoAct.ShouldHostilesPreventAutoget())
		{
			Sidebar.ClearAutogotItems();
			AutoAct.ResumeSetting = AutoAct.Setting;
			AutoAct.Setting = "g";
		}
		return true;
	}

	private bool HandleInventoryActionEvent(InventoryActionEvent E)
	{
		E.Item.ModIntProperty("InventoryActions", 1);
		E.Item.ModIntProperty("InventoryActions" + E.Command, 1);
		E.Item.SetStringProperty("LastInventoryActionCommand", E.Command);
		E.Item.SetLongProperty("LastInventoryActionTurn", XRLCore.CurrentTurn);
		if (E.Command == "Target")
		{
			if (E.Item.CanBeTargetedByPlayer())
			{
				Sidebar.CurrentTarget = E.Item;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "Untarget")
		{
			if (Sidebar.CurrentTarget == E.Item)
			{
				Sidebar.CurrentTarget = null;
			}
			E.RequestInterfaceExit();
		}
		else if (E.Command == "ShowEffects")
		{
			E.Item.ShowActiveEffects();
		}
		else if (E.Command.StartsWith("Companion"))
		{
			if (E.Command == "CompanionRename")
			{
				if (E.Item.IsPlayerLed() && CanBeNamedEvent.Check(E.Actor, E.Item))
				{
					if (!E.Item.HasProperName || E.Item.GetIntProperty("Renamed") == 1)
					{
						if (GameManager.Instance.OverlayUIEnabled && Options.OverlayTooltips)
						{
							GameManager.Instance.uiQueue.queueTask(delegate
							{
								GameManager.Instance.lookerTooltip.ForceHideTooltip();
							});
						}
						switch (Popup.ShowOptionList("Rename your companion", new string[3]
						{
							"Enter a name for " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".",
							"Choose a random name from your own culture.",
							"Choose a random name from " + Grammar.MakePossessive(E.Item.t()) + " culture."
						}, new char[3] { 'a', 'b', 'c' }, 1, null, 60, RespectOptionNewlines: false, AllowEscape: true))
						{
						case 0:
						{
							string text3 = Popup.AskString("Enter a new name for " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: true, BaseOnly: true) + ".", "", 128);
							if (!string.IsNullOrEmpty(text3))
							{
								Popup.Show("You start calling " + E.Item.t() + " by the name '" + text3 + "'.");
								JournalAPI.AddAccomplishment("You started calling " + E.Item.a + E.Item.ShortDisplayName + " by the name '" + text3 + "'.", HistoricStringExpander.ExpandString("<spice.instancesOf.justice.!random>! =name= brought enlightenment to a simple " + E.Item.ShortDisplayName + " and bestowed onto " + E.Item.it + " the name " + text3 + "."), "general", JournalAccomplishment.MuralCategory.Treats, JournalAccomplishment.MuralWeight.Medium, null, -1L);
								E.Item.pRender.DisplayName = text3;
								E.Item.HasProperName = true;
								E.Item.SetIntProperty("Renamed", 1);
							}
							break;
						}
						case 1:
						{
							string text2 = NameMaker.MakeName(this);
							Popup.Show("You start calling " + E.Item.t() + " by the name '" + text2 + "'.");
							JournalAPI.AddAccomplishment("You started calling " + E.Item.a + E.Item.ShortDisplayName + " by the name '" + text2 + "'.", HistoricStringExpander.ExpandString("<spice.instancesOf.justice.!random>! =name= brought enlightenment to a simple " + E.Item.ShortDisplayName + " and bestowed onto " + E.Item.it + " the name " + text2 + "."), "general", JournalAccomplishment.MuralCategory.Treats, JournalAccomplishment.MuralWeight.Medium, null, -1L);
							E.Item.pRender.DisplayName = text2;
							E.Item.HasProperName = true;
							E.Item.SetIntProperty("Renamed", 1);
							break;
						}
						case 2:
						{
							string text = NameMaker.MakeName(E.Item);
							Popup.Show("You start calling " + E.Item.t() + " by the name '" + text + "'.");
							JournalAPI.AddAccomplishment("You started calling " + E.Item.a + E.Item.ShortDisplayName + " by the name '" + text + "'.", HistoricStringExpander.ExpandString("<spice.instancesOf.justice.!random>! =name= brought enlightenment to a simple " + E.Item.ShortDisplayName + " and bestowed onto " + E.Item.it + " the name " + text + "."), "general", JournalAccomplishment.MuralCategory.Treats, JournalAccomplishment.MuralWeight.Medium, null, -1L);
							E.Item.pRender.DisplayName = text;
							E.Item.HasProperName = true;
							E.Item.SetIntProperty("Renamed", 1);
							break;
						}
						}
						UseEnergy(1000, "Companion Rename");
						E.RequestInterfaceExit();
					}
					else
					{
						Popup.ShowFail(E.Item.T() + E.Item.GetVerb("don't") + " want a new name.");
					}
				}
			}
			else if (E.Command == "CompanionGiveItems")
			{
				if (E.Item.IsPlayerLed() && DistanceTo(E.Item) <= 1 && !HasProperty("FugueCopy"))
				{
					TradeUI.ShowTradeScreen(E.Item, 0f);
					UseEnergy(1000, "Companion Trade");
					E.RequestInterfaceExit();
				}
			}
			else if (E.Command == "CompanionAttackTarget")
			{
				if (CheckCompanionDirection(E.Item))
				{
					if (GameManager.Instance.OverlayUIEnabled && Options.OverlayTooltips)
					{
						GameManager.Instance.uiQueue.queueTask(delegate
						{
							GameManager.Instance.lookerTooltip.ForceHideTooltip();
						});
					}
					Cell cell = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, 0, 999, CurrentCell.X, CurrentCell.Y, Locked: true, AllowVis.OnlyVisible);
					if (cell != null)
					{
						GameObject combatTarget = cell.GetCombatTarget(E.Item, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5);
						if (combatTarget != null)
						{
							E.Item.pBrain.Goals.Clear();
							E.Item.pBrain.Target = combatTarget;
							CompanionDirectionEnergyCost(E.Item, 100, "Attack Target");
							E.RequestInterfaceExit();
						}
					}
				}
			}
			else if (E.Command == "CompanionMoveTo")
			{
				if (E.Item.IsPotentiallyMobile() && CheckCompanionDirection(E.Item))
				{
					if (GameManager.Instance.OverlayUIEnabled && Options.OverlayTooltips)
					{
						GameManager.Instance.uiQueue.queueTask(delegate
						{
							GameManager.Instance.lookerTooltip.ForceHideTooltip();
						});
					}
					Cell cell2 = PickTarget.ShowPicker(PickTarget.PickStyle.Burst, 0, 999, E.Item.CurrentCell.X, E.Item.CurrentCell.Y, Locked: true, AllowVis.OnlyExplored);
					if (cell2 != null && cell2 != E.Item.CurrentCell)
					{
						if (E.Item.pBrain.Staying)
						{
							E.Item.pBrain.Stay(cell2);
						}
						E.Item.pBrain.Goals.Clear();
						E.Item.pBrain.PushGoal(new MoveTo(cell2, careful: false, overridesCombat: true, 0, wandering: false, global: false, juggernaut: false, 100));
						CompanionDirectionEnergyCost(E.Item, 100, "Move");
						E.RequestInterfaceExit();
					}
				}
			}
			else if (E.Command == "CompanionStay")
			{
				if (E.Item.IsPotentiallyMobile() && !E.Item.pBrain.Staying && CheckCompanionDirection(E.Item))
				{
					E.Item.pBrain.Goals.Clear();
					E.Item.pBrain.Stay(E.Item.CurrentCell);
					CompanionDirectionEnergyCost(E.Item, 100, "Stay");
					E.RequestInterfaceExit();
				}
			}
			else if (E.Command == "CompanionCome")
			{
				if (E.Item.IsPotentiallyMobile() && E.Item.pBrain.Staying && CheckCompanionDirection(E.Item))
				{
					E.Item.pBrain.Stay(null);
					CompanionDirectionEnergyCost(E.Item, 100, "Come");
					E.RequestInterfaceExit();
				}
			}
			else if (E.Command == "CompanionToggleEngagement")
			{
				if (CheckCompanionDirection(E.Item))
				{
					E.Item.pBrain.Passive = !E.Item.pBrain.Passive;
					CompanionDirectionEnergyCost(E.Item, 100, "Engage");
					E.RequestInterfaceExit();
				}
			}
			else if (E.Command == "CompanionAbilityUse")
			{
				ActivatedAbilities activatedAbilities = E.Item.ActivatedAbilities;
				if (activatedAbilities != null && activatedAbilities.GetAbilityCount() > 0 && CheckCompanionDirection(E.Item))
				{
					ChangeCompanionAbilityUse(E.Item, activatedAbilities);
					CompanionDirectionEnergyCost(E.Item, 100, "Ability Use");
					E.RequestInterfaceExit();
				}
			}
		}
		return true;
	}

	private void ChangeCompanionAbilityUse(GameObject who, ActivatedAbilities AA)
	{
		List<ActivatedAbilityEntry> list = new List<ActivatedAbilityEntry>(AA.AbilityByGuid.Values);
		list.Sort((ActivatedAbilityEntry a, ActivatedAbilityEntry b) => a.DisplayName.CompareTo(b.DisplayName));
		List<string> list2 = new List<string>(list.Count);
		List<char> list3 = new List<char>(list.Count);
		char c = 'a';
		foreach (ActivatedAbilityEntry item in list)
		{
			string displayName = item.DisplayName;
			displayName = ((item.Toggleable && !item.ActiveToggle && !string.IsNullOrEmpty(item.Command)) ? ((!item.ToggleState) ? (displayName + " {{y|[toggled off]}}") : (displayName + " {{g|[toggled on]}}")) : ((!item.AIDisable) ? (displayName + " {{Y|[allowed]}}") : (displayName + " {{K|[forbidden]}}")));
			list2.Add(displayName);
			list3.Add((c <= 'z') ? c++ : ' ');
		}
		int num = Popup.ShowOptionList("", list2.ToArray(), list3.ToArray(), 0, "Choose one of " + Grammar.MakePossessive(who.t()) + " abilities to forbid or allow its use.", 60, RespectOptionNewlines: false, AllowEscape: true);
		if (num < 0)
		{
			return;
		}
		ActivatedAbilityEntry activatedAbilityEntry = list[num];
		if (activatedAbilityEntry.Toggleable && !activatedAbilityEntry.ActiveToggle && !string.IsNullOrEmpty(activatedAbilityEntry.Command))
		{
			bool toggleState = activatedAbilityEntry.ToggleState;
			who.FireEvent(Event.New(activatedAbilityEntry.Command));
			if (toggleState != activatedAbilityEntry.ToggleState)
			{
				Popup.Show(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(who.T())) + " " + activatedAbilityEntry.DisplayName + " ability will now be toggled " + (activatedAbilityEntry.ToggleState ? "on" : "off") + ".");
			}
			else
			{
				Popup.Show(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(who.T())) + " " + activatedAbilityEntry.DisplayName + " ability cannot be toggled at this time.");
			}
		}
		else
		{
			activatedAbilityEntry.AIDisable = !activatedAbilityEntry.AIDisable;
			Popup.Show(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(who.T())) + " " + activatedAbilityEntry.DisplayName + " ability will now be " + (activatedAbilityEntry.AIDisable ? "forbidden" : "allowed") + ".");
		}
	}

	public bool CheckCompanionDirection(GameObject who)
	{
		if (!who.IsPlayerLed())
		{
			return false;
		}
		if (!IsAudible(who) && !CanMakeTelepathicContactWith(who))
		{
			Popup.ShowFail(who.T() + who.GetVerb("can't") + " hear you!");
			return false;
		}
		return true;
	}

	public string GetInventoryCategory()
	{
		GetInventoryCategoryEvent getInventoryCategoryEvent = GetInventoryCategoryEvent.FromPool(this);
		HandleEvent(getInventoryCategoryEvent);
		return getInventoryCategoryEvent.Category;
	}

	public bool Die(GameObject Killer = null, string KillerText = null, string Reason = null, string ThirdPersonReason = null, bool Accidental = false, GameObject Weapon = null, GameObject Projectile = null, bool Force = false, string Message = null)
	{
		if (Dying)
		{
			return true;
		}
		bool result = true;
		try
		{
			Dying = true;
			if (BeforeDieEvent.Check(this, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason))
			{
				StopMoving();
				string propertyOrTag = GetPropertyOrTag("DeathSounds");
				if (!string.IsNullOrEmpty(propertyOrTag))
				{
					pPhysics?.PlayWorldSound(propertyOrTag.CachedCommaExpansion().GetRandomElement(), 0.5f, 0f, combat: true);
				}
				if (IsPlayer())
				{
					if (XRLCore.Core.Game.Running)
					{
						KilledPlayerEvent.Send(this, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental);
						XRLCore.Core.RenderBase();
						if (CheckpointingSystem.ShowDeathMessage("You died.\n\n" + (Reason ?? XRLCore.Core.Game.DeathReason)))
						{
							return true;
						}
						if (!Force && (!Options.AllowReallydie || Popup.ShowYesNo("DEBUG: Do you really want to die?", AllowEscape: true, DialogResult.No) == DialogResult.Yes))
						{
							if (Reason != null)
							{
								XRLCore.Core.Game.DeathReason = Reason;
							}
							XRLCore.Core.Game.Running = false;
							string text = XRLCore.Core.Game.DeathReason;
							if (!string.IsNullOrEmpty(text))
							{
								text = text[0].ToString().ToLower() + text.Substring(1);
							}
							JournalAPI.AddAccomplishment("On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", " + text.Replace("!", "."), null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Nil, null, -1L);
							MetricsManager.LogEvent("Death:Reason:" + text.Replace(':', '_'));
							MetricsManager.LogEvent("Death:Turns:" + XRLCore.CurrentTurn);
							MetricsManager.LogEvent("Death:Walltime:" + XRLCore.Core.Game._walltime);
							if (HasStat("Level"))
							{
								MetricsManager.LogEvent("Death:Level:" + Statistics["Level"].BaseValue);
							}
							AchievementManager.SetAchievement("ACH_DIE");
						}
						else
						{
							Statistics["Hitpoints"].Penalty = 0;
							if (GetPart("Stomach") is Stomach stomach)
							{
								stomach.Water = 50000;
							}
							result = false;
						}
					}
				}
				else
				{
					if (Message != null)
					{
						if (Message != "")
						{
							EmitMessage(Message);
						}
					}
					else if (!HasTagOrProperty("NoDeathVerb"))
					{
						if (HasTagOrProperty("CustomDeathVerb"))
						{
							pBrain.DidX(GetTagOrStringProperty("CustomDeathVerb", "die"), null, "!", null, null, this);
						}
						else if (pBrain != null && !HasTagOrProperty("DeathMessageAsInanimate"))
						{
							pBrain.DidX("die", null, "!", null, null, this);
						}
						else
						{
							pPhysics?.DidX("are", "destroyed", "!", null, null, this);
						}
					}
					if (Killer != null)
					{
						KilledEvent.Send(this, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental);
						AwardXPTo(Killer);
						if (Killer.IsPlayer())
						{
							MetricsManager.LogEvent("PlayerKill:" + Blueprint);
						}
					}
					WeaponUsageTracking.TrackKill(Killer, this, Weapon, Projectile, Accidental);
					EarlyBeforeDeathRemovalEvent.Send(this, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
					BeforeDeathRemovalEvent.Send(this, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
					OnDeathRemovalEvent.Send(this, Killer, Weapon, Projectile, Accidental, KillerText, Reason, ThirdPersonReason);
					Destroy(Reason, Silent: false, Obliterate: false, ThirdPersonReason);
					MetricsManager.LogEvent("Kill:" + Blueprint);
				}
			}
			else
			{
				result = false;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("GameObject::Die", x);
		}
		Dying = false;
		return result;
	}

	public int GetComplexity()
	{
		return GetPart<Examiner>()?.Complexity ?? GetTechTier();
	}

	public int GetExamineDifficulty()
	{
		return GetPart<Examiner>()?.Difficulty ?? 0;
	}

	public bool Explode(int Force, GameObject Owner = null, string BonusDamage = null, float DamageModifier = 1f, bool Neutron = false, bool SuppressDestroy = false, bool Indirect = false, int Phase = 0, List<GameObject> Hit = null)
	{
		SplitFromStack();
		if (Hit == null)
		{
			Hit = Event.NewGameObjectList();
			Hit.Add(this);
		}
		else if (!Hit.Contains(this))
		{
			Hit.Add(this);
		}
		XRL.World.Parts.Physics.ApplyExplosion(GetCurrentCell(), Force, null, Hit, Local: false, Show: true, Owner, BonusDamage, DamageModifier: DamageModifier, Phase: (Phase == 0) ? GetPhase() : Phase, Neutron: Neutron, Indirect: Indirect, WhatExploded: this);
		if (!SuppressDestroy)
		{
			if (IsPlayer())
			{
				if (Neutron)
				{
					AchievementManager.SetAchievement("ACH_CRUSHED_UNDER_SUNS");
					Die(null, "laws of physics", "You were crushed under the weight of a thousand suns.", It + GetVerb("were") + " @@crushed under the weight of a thousand suns.");
				}
				else
				{
					Die(null, "laws of physics", "You exploded.", It + " @@exploded.");
				}
			}
			else
			{
				Destroy(null, Silent: true);
			}
		}
		return true;
	}

	public void Discharge(Cell TargetCell, int Voltage, string Damage, GameObject Owner = null, Cell StartCell = null, int Phase = 0, bool Accidental = false)
	{
		pPhysics?.ApplyDischarge(StartCell ?? GetCurrentCell(), TargetCell, Voltage, Damage, null, Owner, Phase, Accidental);
	}

	public void Discharge(Cell TargetCell, int Voltage, int Damage, GameObject Owner = null, Cell StartCell = null, int Phase = 0, bool Accidental = false)
	{
		pPhysics?.ApplyDischarge(StartCell ?? GetCurrentCell(), TargetCell, Voltage, Damage, null, Owner, Phase, Accidental);
	}

	public int GetBaseThrowRange(GameObject Thrown = null, GameObject ApparentTarget = null, Cell TargetCell = null, int Distance = 0)
	{
		GetThrowProfileEvent.Process(out var Range, out var _, out var _, out var _, this, Thrown, ApparentTarget, TargetCell, Distance);
		return Range;
	}

	public bool PerformThrow(GameObject obj, Cell TargetCell, MissilePath MPath = null, int Phase = 0)
	{
		BodyPart bodyPart = obj.EquippedOn();
		if (bodyPart == null)
		{
			return false;
		}
		if (!FireEvent("BeginAttack"))
		{
			return false;
		}
		int num = DistanceTo(TargetCell) + 1;
		GameObject combatTarget = TargetCell.GetCombatTarget(this, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, AllowInanimate: true, InanimateSolidOnly: true);
		if (combatTarget == this && combatTarget.IsPlayer() && Popup.ShowYesNoCancel("Are you sure you want to target " + itself + "?") != 0)
		{
			return false;
		}
		if (Phase == 0)
		{
			Phase = XRL.World.Capabilities.Phase.getWeaponPhase(this, GetActivationPhaseEvent.GetFor(obj));
		}
		Event @event = Event.New("BeforeThrown");
		@event.SetParameter("TargetCell", TargetCell);
		@event.SetParameter("Thrower", this);
		@event.SetParameter("ApparentTarget", combatTarget);
		@event.SetParameter("Phase", Phase);
		if (!obj.FireEvent(@event))
		{
			return false;
		}
		bodyPart.Unequip();
		Inventory inventory = Inventory;
		if (inventory != null)
		{
			string propertyOrTag = GetPropertyOrTag("NoEquip");
			List<string> list = (string.IsNullOrEmpty(propertyOrTag) ? null : new List<string>(propertyOrTag.CachedCommaExpansion()));
			GameObject gameObject = null;
			List<GameObject> objectsDirect = inventory.GetObjectsDirect();
			foreach (GameObject item in objectsDirect)
			{
				if (item.SameAs(obj) && (IsPlayer() || !item.HasPropertyOrTag("NoAIEquip")) && (list == null || !list.Contains(item.Blueprint)) && !item.IsBroken() && !item.IsRusted())
				{
					gameObject = item;
					break;
				}
			}
			if (gameObject == null)
			{
				foreach (GameObject item2 in objectsDirect)
				{
					if (item2.Blueprint == obj.Blueprint && (IsPlayer() || !item2.HasPropertyOrTag("NoAIEquip")) && (list == null || !list.Contains(item2.Blueprint)) && !item2.IsBroken() && !item2.IsRusted())
					{
						gameObject = item2;
						break;
					}
				}
				if (gameObject == null && !IsPlayer() && obj.HasTag("Grenade"))
				{
					foreach (GameObject item3 in objectsDirect)
					{
						if (item3.HasTag("Grenade") && (IsPlayer() || !item3.HasPropertyOrTag("NoAIEquip")) && (list == null || !list.Contains(item3.Blueprint)) && !item3.IsBroken() && !item3.IsRusted())
						{
							gameObject = item3;
							break;
						}
					}
				}
			}
			if (gameObject != null)
			{
				FireEvent(Event.New("CommandEquipObject", "Object", gameObject, "BodyPart", bodyPart));
			}
		}
		MissileWeapon.SetupProjectile(obj, this);
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (MPath == null)
		{
			currentCell.ParentZone.CalculateMissileMap(this);
			MPath = MissileWeapon.CalculateMissilePath(currentCell.ParentZone, currentCell.X, currentCell.Y, TargetCell.X, TargetCell.Y);
			if (MPath == null)
			{
				return false;
			}
		}
		int num2 = XRL.Rules.Stat.RollPenetratingSuccesses("1d" + Stat("Agility"), 3);
		int num3 = 0;
		int num4 = currentCell.X - TargetCell.X;
		int num5 = currentCell.Y - TargetCell.Y;
		int num6 = (int)Math.Sqrt(num4 * num4 + num5 * num5);
		GetThrowProfileEvent.Process(out var Range, out var Strength, out var AimVariance, out var Telekinetic, this, obj, combatTarget, TargetCell, num6);
		Range += XRL.Rules.Stat.Random(1, 6);
		if (num <= Range && (HasIntProperty("CloseThrowRangeAccuracyBonus") || HasIntProperty("CloseThrowRangeAccuracySkillBonus")))
		{
			float num7 = (100f - Math.Max((float)GetIntProperty("CloseThrowRangeAccuracyBonus"), (float)GetIntProperty("CloseThrowRangeAccuracySkillBonus"))) / 100f;
			AimVariance = (int)((float)AimVariance * num7);
		}
		else
		{
			num += "1d3-2".RollCached();
			if (num < 2 && Range >= 2)
			{
				num = 2;
			}
		}
		List<Point> list2 = MissileWeapon.CalculateBulletTrajectory(MPath, obj, obj, this, currentCell.ParentZone, AimVariance.ToString());
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer2();
		Zone parentZone = currentCell.ParentZone;
		Cell cell = null;
		List<GameObject> objectsThatWantEvent = TargetCell.ParentZone.GetObjectsThatWantEvent(ProjectileMovingEvent.ID, ProjectileMovingEvent.CascadeLevel);
		ProjectileMovingEvent projectileMovingEvent = null;
		if (objectsThatWantEvent.Count > 0)
		{
			GameObject projectile = obj;
			ScreenBuffer screenBuffer = scrapBuffer;
			projectileMovingEvent = ProjectileMovingEvent.FromPool(this, null, projectile, null, null, TargetCell, list2, -1, screenBuffer, Throw: true);
		}
		if (Telekinetic)
		{
			TelekinesisBlip();
		}
		for (int i = 0; i < list2.Count; i++)
		{
			int num8 = currentCell.X - list2[i].X;
			int num9 = currentCell.Y - list2[i].Y;
			int num10 = (int)Math.Sqrt(num8 * num8 + num9 * num9);
			if (num10 >= Range || num10 >= num)
			{
				break;
			}
			cell = parentZone.GetCell(list2[i].X, list2[i].Y);
			if (Telekinetic)
			{
				cell.TelekinesisBlip();
			}
			GameObject combatTarget2 = cell.GetCombatTarget(this, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, Phase, null, null, AllowInanimate: true, InanimateSolidOnly: true);
			bool flag = false;
			if (projectileMovingEvent != null)
			{
				projectileMovingEvent.Defender = combatTarget2;
				projectileMovingEvent.Cell = cell;
				projectileMovingEvent.PathIndex = i;
				foreach (GameObject item4 in objectsThatWantEvent)
				{
					if (!item4.HandleEvent(projectileMovingEvent))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag || (list2.Count > 2 && i != list2.Count - 1 && parentZone.GetCell(list2[i + 1].X, list2[i + 1].Y).IsSolidFor(obj, this)))
			{
				break;
			}
			if (num2 > 0 && cell == TargetCell && combatTarget2 != null && combatTarget2 != this && combatTarget2.FireEvent("DefenderMissileHit"))
			{
				obj?.pPhysics?.DidXToY("hit", combatTarget2, null, "!", null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, this, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, UsePopup: false, combatTarget2);
				bool flag2 = combatTarget2 != combatTarget;
				bool isCreature = combatTarget2.IsCreature;
				string blueprint = combatTarget2.Blueprint;
				WeaponUsageTracking.TrackThrownWeaponHit(this, obj, isCreature, blueprint, flag2);
				num2 -= num6;
				int num11 = 0;
				int combatAV = Stats.GetCombatAV(combatTarget2);
				int bonus = Math.Max(XRL.Rules.Stat.GetScoreModifier(Strength), 1);
				ThrownWeapon thrownWeapon = obj.GetPart("ThrownWeapon") as ThrownWeapon;
				num11 = XRL.Rules.Stat.RollDamagePenetrations(combatAV, bonus, thrownWeapon?.Penetration ?? 2);
				for (int j = 0; j < num11; j++)
				{
					num3 += thrownWeapon?.Damage.RollCached() ?? XRL.Rules.Stat.Random(1, 2);
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				Damage damage = new Damage(num3);
				if (flag2)
				{
					damage.AddAttribute("Accidental");
				}
				Event event2 = Event.New("WeaponThrowHit");
				event2.SetParameter("Damage", damage);
				event2.SetParameter("Penetrations", num11);
				event2.SetParameter("Owner", this);
				event2.SetParameter("Attacker", this);
				event2.SetParameter("Defender", combatTarget2);
				event2.SetParameter("Weapon", obj);
				event2.SetParameter("Projectile", obj);
				event2.SetParameter("Phase", Phase);
				event2.SetParameter("ApparentTarget", combatTarget);
				if (!obj.FireEvent(event2))
				{
					break;
				}
				num11 = event2.GetIntParameter("Penetrations");
				Event event3 = Event.New("TakeDamage");
				event3.SetParameter("Damage", damage);
				event3.SetParameter("Penetrations", num11);
				event3.SetParameter("Owner", this);
				event3.SetParameter("Attacker", this);
				event3.SetParameter("Defender", combatTarget2);
				event3.SetParameter("Weapon", obj);
				event3.SetParameter("Phase", Phase);
				event3.SetParameter("Projectile", obj);
				if (combatTarget2.FireEvent(event3))
				{
					num11 = event3.GetIntParameter("Penetrations");
					WeaponUsageTracking.TrackThrownWeaponDamage(this, obj, isCreature, blueprint, flag2, damage);
					if (damage.Amount > 0 && !juiceEnabled && Options.ShowMonsterHPHearts)
					{
						combatTarget2.ParticleBlip(combatTarget2.GetHPColor() + "\u0003");
					}
					if (damage.Amount > 0 || !damage.SuppressionMessageDone)
					{
						if (IsPlayer())
						{
							MessageQueue.AddPlayerMessage("You hit " + ((combatTarget2 == this) ? itself : combatTarget2.t()) + " with " + obj.t() + " (x" + num11 + ") for " + damage.Amount + " damage!", XRL.Rules.Stat.GetResultColor(num11));
						}
						else if (combatTarget2.IsPlayer())
						{
							MessageQueue.AddPlayerMessage(T() + GetVerb("hit") + " with " + obj.a + obj.ShortDisplayName + " (x" + num11 + ") for " + damage.Amount + " damage!", ColorCoding.ConsequentialColor(null, combatTarget2));
						}
						else if (combatTarget2.IsVisible())
						{
							MessageQueue.AddPlayerMessage(T() + GetVerb("hit") + " " + ((combatTarget2 == this) ? itself : combatTarget2.t()) + " with " + obj.a + obj.ShortDisplayName + " (x" + num11 + ") for " + damage.Amount + " damage!", ColorCoding.ConsequentialColor(null, combatTarget2));
						}
					}
				}
				Event event4 = new Event("ThrownProjectileHit");
				event4.SetParameter("Damage", damage);
				event4.SetParameter("Penetrations", num11);
				event4.SetParameter("Owner", this);
				event4.SetParameter("Attacker", this);
				event4.SetParameter("Defender", combatTarget2);
				event4.SetParameter("Weapon", obj);
				event4.SetParameter("Projectile", obj);
				event4.SetParameter("Phase", Phase);
				event4.SetParameter("ApparentTarget", combatTarget);
				obj.FireEvent(event4);
				if (IsPlayer())
				{
					Sidebar.CurrentTarget = combatTarget2;
				}
				break;
			}
			XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
			if (parentZone != ThePlayer.CurrentCell.ParentZone || !parentZone.GetCell(list2[i].X, list2[i].Y).IsVisible())
			{
				continue;
			}
			scrapBuffer.Goto(list2[i].X, list2[i].Y);
			if (Options.UseTiles && !string.IsNullOrEmpty(obj.pRender.Tile))
			{
				string text = obj.pRender.TileColor;
				if (string.IsNullOrEmpty(text))
				{
					text = obj.pRender.ColorString;
				}
				string detailColor = obj.pRender.DetailColor;
				XRLCore.ParticleManager.AddTile(obj.pRender.Tile, text, detailColor, list2[i].X, list2[i].Y, 0f, 0f, 2, 0f, 0f, obj.pRender.HFlip, obj.pRender.VFlip);
				XRLCore.ParticleManager.Frame();
				XRLCore.ParticleManager.Render(scrapBuffer);
			}
			else
			{
				scrapBuffer.Write(obj.pRender.ColorString + obj.pRender.RenderString);
			}
			XRLCore._Console.DrawBuffer(scrapBuffer);
			Thread.Sleep(25);
		}
		UseEnergy(1000, "Missile Combat Throw");
		if (validate(ref obj) && !obj.IsInGraveyard())
		{
			(cell ?? TargetCell).AddObject(obj, Forced: false, System: false, IgnoreGravity: false, NoStack: false, Repaint: true, null, "Thrown");
			obj.WasThrown(this, combatTarget);
			MissileWeapon.CleanupProjectile(obj);
		}
		return true;
	}

	public bool LimitToAquatic()
	{
		if (pBrain != null)
		{
			return pBrain.limitToAquatic();
		}
		return false;
	}

	public bool Move(string Direction, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, GameObject Dragging = null, bool NearestAvailable = false, int? EnergyCost = null, string Type = null, int? MoveSpeed = null, bool Peaceful = false)
	{
		bool flag = false;
		int num = 0;
		int num2 = 20;
		int num3 = EnergyCost ?? ((!Forced && Dragging == null) ? 1000 : 0);
		int num4 = num3;
		int value = MoveSpeed ?? Stat("MoveSpeed");
		if (CurrentCell == null)
		{
			return false;
		}
		Zone parentZone = CurrentCell.ParentZone;
		if (parentZone == null)
		{
			return false;
		}
		if (Dragging != null && !CanBeInvoluntarilyMoved())
		{
			return false;
		}
		if (!Forced && HasEffect("Dashing") && !parentZone.IsWorldMap())
		{
			num3 = 0;
			flag = true;
			num2 = 20;
		}
		while (true)
		{
			num++;
			bool immobile = true;
			bool waterbound = false;
			bool wallwalker = false;
			pBrain?.checkMobility(out immobile, out waterbound, out wallwalker);
			if (immobile && !Forced)
			{
				if (IsPlayer())
				{
					Popup.ShowFail("You are not a mobile creature.");
				}
				return false;
			}
			if (!Forced)
			{
				if (!CheckFrozen())
				{
					return false;
				}
				if (HasEffect("Paralyzed"))
				{
					if (IsPlayer())
					{
						Popup.ShowFail("You are paralyzed!");
					}
					return false;
				}
				if (IsOverburdened() && (!IsPlayer() || !XRLCore.Core.IDKFA))
				{
					StopMoving();
					if (IsPlayer())
					{
						Popup.ShowFail("You are carrying too much to move!");
					}
					return false;
				}
			}
			Cell cell = CurrentCell.GetCellFromDirection(Direction, BuiltOnly: false);
			if (cell != null && NearestAvailable)
			{
				List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells(1);
				localAdjacentCells.Add(cell);
				for (int i = 0; i < localAdjacentCells.Count; i++)
				{
					Cell cell2 = localAdjacentCells[i];
					if (cell2.IsEmpty())
					{
						cell = cell2;
						break;
					}
				}
			}
			GameObject gameObject6;
			if (cell != null)
			{
				List<GameObject> list = null;
				bool flag2 = IsPlayer() || HasPart("Combat") || ConsiderSolid();
				if (flag2)
				{
					list = cell.GetObjectsWithPartReadonly("Combat");
				}
				if (wallwalker && !cell.HasWalkableWallFor(this))
				{
					if (IsPlayer())
					{
						Popup.ShowFail("You are wall-dwelling creature and may only move onto walls.");
					}
				}
				else
				{
					if (waterbound)
					{
						if (!Forced && !cell.HasAquaticSupportFor(this))
						{
							if (IsPlayer())
							{
								Popup.ShowFail("You are an aquatic creature and may not move onto land!");
							}
							goto IL_0a95;
						}
					}
					else if (!Forced && IsPlayer() && Type != "Charge" && GetConfusion() <= 0)
					{
						if (AutoAct.IsActive())
						{
							GameObject gameObject = (Options.ConfirmDangerousLiquid ? cell.GetDangerousOpenLiquidVolume() : null);
							if (gameObject != null && gameObject.PhaseAndFlightMatches(this) && Popup.ShowYesNo("Your path would take you into " + gameObject.an() + ". Are you sure you want to do this?", AllowEscape: true, DialogResult.No) != 0)
							{
								AutoAct.Interrupt(null, null, gameObject);
								return false;
							}
						}
						else
						{
							GameObject gameObject2 = (Options.ConfirmSwimming ? cell.GetSwimmingDepthLiquid() : null);
							if (gameObject2 != null && !HasEffect("Swimming") && !cell.HasBridge() && gameObject2.PhaseAndFlightMatches(this))
							{
								if (XRLCore.Core.MoveConfirmDirection != Direction)
								{
									bool flag3 = false;
									if (list != null)
									{
										int j = 0;
										for (int count = list.Count; j < count; j++)
										{
											GameObject gameObject3 = list[j];
											if (gameObject3 != this && gameObject3.IsHostileTowards(this) && PhaseAndFlightMatches(gameObject3))
											{
												flag3 = true;
												break;
											}
										}
									}
									if (!flag3)
									{
										bool flag4 = gameObject2.IsDangerousOpenLiquidVolume();
										MessageQueue.AddPlayerMessage("There" + gameObject2.Is + " " + (flag4 ? ("a dangerous-looking " + gameObject2.ShortDisplayName) : gameObject2.an()) + " that way. Move " + Directions.GetExpandedDirection(Direction) + " again to enter " + gameObject2.them + " and start swimming.", flag4 ? 'R' : 'W');
										XRLCore.Core.MoveConfirmDirection = Direction;
										return false;
									}
								}
							}
							else if ((gameObject2 = (Options.ConfirmDangerousLiquid ? cell.GetDangerousOpenLiquidVolume() : null)) != null && gameObject2.PhaseAndFlightMatches(this) && XRLCore.Core.MoveConfirmDirection != Direction)
							{
								bool flag5 = false;
								if (list != null)
								{
									int k = 0;
									for (int count2 = list.Count; k < count2; k++)
									{
										GameObject gameObject4 = list[k];
										if (gameObject4 != this && gameObject4.IsHostileTowards(this) && PhaseAndFlightMatches(gameObject4))
										{
											flag5 = true;
											break;
										}
									}
								}
								if (!flag5)
								{
									MessageQueue.AddPlayerMessage("Are you sure you want to move into " + gameObject2.t() + "? Move " + Directions.GetExpandedDirection(Direction) + " again to confirm.", 'R');
									XRLCore.Core.MoveConfirmDirection = Direction;
									return false;
								}
							}
						}
					}
					int l = 0;
					for (int count3 = cell.Objects.Count; l < count3; l++)
					{
						if (cell.Objects[l] != null && cell.Objects[l].HasTagOrStringProperty("TerrainMovementEnergyCostMultiplier"))
						{
							try
							{
								float num5 = float.Parse(cell.Objects[l].GetTagOrStringProperty("TerrainMovementEnergyCostMultiplier", "1.0"));
								num3 += (int)((float)num4 * num5);
							}
							catch (Exception)
							{
								MetricsManager.LogError("Object " + cell.Objects[l].Blueprint + " had invalid TerrainMovementEnergyCostMultiplier.");
							}
						}
					}
					if (ProcessBeginMove(out var ReobtainCombatants, cell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging))
					{
						if (ReobtainCombatants && flag2)
						{
							list = cell.GetObjectsWithPartReadonly("Combat");
						}
						if (!Forced && HasPart("Digging"))
						{
							int num6 = 0;
							int count4 = cell.Objects.Count;
							while (num6 < count4)
							{
								GameObject gameObject5 = cell.Objects[num6];
								if (!gameObject5.IsWall() || !gameObject5.ConsiderSolidFor(this) || !OkayToDamageEvent.Check(gameObject5, this))
								{
									num6++;
									continue;
								}
								goto IL_0649;
							}
						}
						if (list != null)
						{
							for (int m = 0; m < list.Count; m++)
							{
								gameObject6 = list[m];
								if (gameObject6 == this || !gameObject6.PhaseAndFlightMatches(this))
								{
									continue;
								}
								bool flag6 = gameObject6.IsHostileTowards(this);
								if (!Forced && flag6)
								{
									goto IL_0707;
								}
								if (flag6)
								{
									if (IsPlayer())
									{
										MessageQueue.AddPlayerMessage("You are stopped short by " + gameObject6.t() + ".");
									}
								}
								else if (!gameObject6.IsPlayer() && CurrentCell != null)
								{
									if (!gameObject6.CanBePositionSwapped(this))
									{
										if (IsPlayer())
										{
											MessageQueue.AddPlayerMessage(gameObject6.T() + gameObject6.GetVerb("cannot") + " be moved.");
										}
									}
									else if (gameObject6.HasEffect("Stuck"))
									{
										if (IsPlayer())
										{
											MessageQueue.AddPlayerMessage(gameObject6.T() + gameObject6.Is + " stuck.");
										}
									}
									else
									{
										if (gameObject6.CurrentCell.RemoveObject(gameObject6))
										{
											CurrentCell.AddObject(gameObject6);
											continue;
										}
										if (IsPlayer())
										{
											MessageQueue.AddPlayerMessage("You can't budge " + gameObject6.t() + ".");
										}
									}
								}
								goto IL_0a95;
							}
						}
						if (ProcessObjectLeavingCell(CurrentCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging) && ProcessEnteringCell(cell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging) && ProcessObjectEnteringCell(cell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging) && ProcessLeaveCell(CurrentCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging))
						{
							Cell currentCell = CurrentCell;
							if (CurrentCell != null)
							{
								CurrentCell.RemoveObject(this, Forced, System, IgnoreGravity, NoStack, Repaint: true, Direction, Type, Dragging);
							}
							cell.AddObject(this, Forced, System, IgnoreGravity, NoStack, Repaint: true, Direction, Type, Dragging);
							if (IsPlayer())
							{
								XRLCore.Core.MoveConfirmDirection = null;
							}
							if (num3 > 0)
							{
								UseEnergy(num3, "Movement", null, value);
							}
							Event e = Event.New("AfterMoved", "FromCell", currentCell);
							ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
							FireEvent(e);
							if (parentZone != cell.ParentZone)
							{
								if (IsPlayer())
								{
									XRL.The.ZoneManager.SetActiveZone(cell.ParentZone.ZoneID);
								}
								XRL.The.ZoneManager.ProcessGoToPartyLeader();
							}
							if (flag)
							{
								num2--;
								Smoke();
								int num7 = XRL.Rules.Stat.Random(0, 10);
								if (num7 == 0)
								{
									ParticleBlip("&R*");
								}
								if (num7 == 1)
								{
									ParticleBlip("&Y*");
								}
								if (num7 == 2)
								{
									ParticleBlip("&r*");
								}
								if (num7 == 3)
								{
									ParticleBlip("&W*");
								}
								if (num7 == 4)
								{
									ParticleBlip("&Rú");
								}
								if (num7 == 5)
								{
									ParticleBlip("&Yú");
								}
								if (num7 == 6)
								{
									ParticleBlip("&rú");
								}
								if (num7 == 7)
								{
									ParticleBlip("&Wú");
								}
								if (num7 == 8)
								{
									ParticleBlip("&Rù");
								}
								if (num7 == 9)
								{
									ParticleBlip("&Yù");
								}
								if (num7 == 10)
								{
									ParticleBlip("&rù");
								}
							}
							if (flag && IsPlayer())
							{
								ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
								TextConsole textConsole = Popup._TextConsole;
								XRLCore.Core.RenderBaseToBuffer(scrapBuffer);
								textConsole.DrawBuffer(scrapBuffer);
								XRLCore.ParticleManager.Frame();
								XRLCore.Core.Game.ZoneManager.Tick(bAllowFreeze: true);
							}
							if (!flag || num2 <= 0)
							{
								break;
							}
							continue;
						}
					}
				}
			}
			else if (IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You cannot go that way.");
			}
			goto IL_0a95;
			IL_0a95:
			FireEvent("MoveFailed");
			if (flag)
			{
				UseEnergy(1000, "Movement");
			}
			return false;
			IL_0649:
			if (!Peaceful)
			{
				if (flag)
				{
					int value2 = Math.Min(num / 3, 20);
					int value3 = Math.Min(num / 3, 20);
					FireEvent(Event.New("CommandAttackCell", "Cell", cell, "PenCapBonus", value2, "HitBonus", value3));
				}
				else
				{
					FireEvent(Event.New("CommandAttackCell", "Cell", cell));
				}
			}
			goto IL_0a95;
			IL_0707:
			if (IsPlayer())
			{
				if (AutoAct.IsActive())
				{
					AutoAct.Interrupt("there" + gameObject6.Is + " " + gameObject6.a + gameObject6.ShortDisplayName + " in your way", null, gameObject6);
				}
				else if (!Peaceful)
				{
					if (flag)
					{
						int value4 = num / 3 * 2;
						int value5 = num / 3 * 2;
						FireEvent(Event.New("CommandAttackCell", "Cell", cell, "PenCapBonus", value4, "HitBonus", value5));
					}
					else
					{
						FireEvent(Event.New("CommandAttackCell", "Cell", cell));
					}
				}
			}
			goto IL_0a95;
		}
		if (flag)
		{
			UseEnergy(1000, "Movement");
		}
		return true;
	}

	public bool Push(string Direction, int Force, int MaxDistance = 9999999, bool IgnoreGravity = false, bool Involuntary = true)
	{
		return pPhysics?.Push(Direction, Force, MaxDistance, IgnoreGravity, Involuntary) ?? false;
	}

	public int Accelerate(int Force, string Direction = null, Cell Toward = null, Cell AwayFrom = null, string Type = null, GameObject Actor = null, bool Accidental = false, GameObject IntendedTarget = null, string BonusDamage = null, double DamageFactor = 1.0, bool SuspendFalling = true, bool OneShort = false, bool Repeat = false, bool BuiltOnly = true, bool MessageForInanimate = true, bool DelayForDisplay = true)
	{
		if (pPhysics == null)
		{
			return 0;
		}
		return pPhysics.Accelerate(Force, Direction, Toward, AwayFrom, Type, Actor, Accidental, IntendedTarget, BonusDamage, DamageFactor, SuspendFalling, OneShort, Repeat, BuiltOnly, MessageForInanimate, DelayForDisplay);
	}

	public bool TemperatureChange(int Amount, GameObject Actor = null, bool Radiant = false, bool MinAmbient = false, bool MaxAmbient = false, int Phase = 0, int? Min = null, int? Max = null)
	{
		return pPhysics?.ProcessTemperatureChange(Amount, Actor, Radiant, MinAmbient, MaxAmbient, Phase, Min, Max) ?? false;
	}

	private void ProcessMoveEvent(Event E, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null)
	{
		if (Forced)
		{
			E.SetFlag("Forced", State: true);
		}
		if (System)
		{
			E.SetFlag("System", State: true);
		}
		if (IgnoreGravity)
		{
			E.SetFlag("IgnoreGravity", State: true);
		}
		if (NoStack)
		{
			E.SetFlag("NoStack", State: true);
		}
		if (Direction != null)
		{
			E.SetParameter("Direction", Direction);
		}
		if (Type != null)
		{
			E.SetParameter("Type", Type);
			E.SetParameter(Type, 1);
		}
		if (Dragging != null)
		{
			E.SetParameter("Dragging", Dragging);
		}
	}

	public bool ProcessBeginMove(out bool ReobtainCombatants, Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		ReobtainCombatants = false;
		if (DestinationCell == null)
		{
			return false;
		}
		if (HasRegisteredEvent("BeginMove"))
		{
			ReobtainCombatants = true;
			Event e = Event.New("BeginMove", "DestinationCell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (HasRegisteredEvent("BeginMoveLate"))
		{
			ReobtainCombatants = true;
			Event e2 = Event.New("BeginMoveLate", "DestinationCell", DestinationCell);
			ProcessMoveEvent(e2, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e2, ParentEvent) && !System)
			{
				return false;
			}
		}
		return true;
	}

	public bool ProcessObjectLeavingCell(Cell CC, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (CC == null)
		{
			return true;
		}
		if (CC.HasObjectWithRegisteredEvent("ObjectLeavingCell"))
		{
			Event e = Event.New("ObjectLeavingCell", "Object", this);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!CC.FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (CC.WantEvent(ObjectLeavingCellEvent.ID, MinEvent.CascadeLevel) && !CC.HandleEvent(ObjectLeavingCellEvent.FromPool(this, CC, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessEnteringCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (HasRegisteredEvent("EnteringCell"))
		{
			Event e = Event.New("EnteringCell", "Cell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(EnteringCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(EnteringCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessObjectEnteringCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (DestinationCell.HasObjectWithRegisteredEvent("ObjectEnteringCell"))
		{
			Event e = Event.New("ObjectEnteringCell", "Object", this);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!DestinationCell.FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (DestinationCell.WantEvent(ObjectEnteringCellEvent.ID, MinEvent.CascadeLevel) && !DestinationCell.HandleEvent(ObjectEnteringCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessLeaveCell(Cell CC, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (CC == null)
		{
			return true;
		}
		if (HasRegisteredEvent("LeaveCell"))
		{
			Event e = Event.New("LeaveCell", "Cell", CC);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(LeaveCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(LeaveCellEvent.FromPool(this, CC, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessLeavingCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (HasRegisteredEvent("LeavingCell"))
		{
			Event e = Event.New("LeavingCell", "Cell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(LeavingCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(LeavingCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessLeftCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (HasRegisteredEvent("LeftCell"))
		{
			Event e = Event.New("LeftCell", "Cell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(LeftCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(LeftCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessEnterCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (HasRegisteredEvent("EnterCell"))
		{
			Event e = Event.New("EnterCell", "Cell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(EnterCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(EnterCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessEnteredCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (HasRegisteredEvent("EnteredCell"))
		{
			Event e = Event.New("EnteredCell", "Cell", DestinationCell);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (WantEvent(EnteredCellEvent.ID, MinEvent.CascadeLevel) && !HandleEvent(EnteredCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging)) && !System)
		{
			return false;
		}
		return true;
	}

	public bool ProcessObjectEnteredCell(Cell DestinationCell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, IEvent ParentEvent = null)
	{
		if (DestinationCell.HasObjectWithRegisteredEvent("ObjectEnteredCell"))
		{
			Event e = Event.New("ObjectEnteredCell", "Object", this);
			ProcessMoveEvent(e, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging);
			if (!DestinationCell.FireEvent(e, ParentEvent) && !System)
			{
				return false;
			}
		}
		if (DestinationCell.WantEvent(ObjectEnteredCellEvent.ID, MinEvent.CascadeLevel) && !DestinationCell.HandleEvent(ObjectEnteredCellEvent.FromPool(this, DestinationCell, Forced, System, IgnoreGravity, NoStack, Direction, Type, Dragging), ParentEvent) && !System)
		{
			return false;
		}
		return true;
	}

	public GameObject SplitStack(int Count, GameObject OwningObject = null, bool NoRemove = false)
	{
		if (GetPart("Stacker") is Stacker stacker)
		{
			return stacker.SplitStack(Count, OwningObject, NoRemove);
		}
		return null;
	}

	private void AutoEquipFail(GameObject obj, bool Silent = false, Cell WasInCell = null, GameObject WasInInventory = null, Event E = null, List<GameObject> WasUnequipped = null)
	{
		if (IsPlayer() && !Silent)
		{
			string text = null;
			if (E != null)
			{
				text = E.GetStringParameter("FailureMessage");
				if (WasUnequipped == null)
				{
					WasUnequipped = E.GetParameter("WasUnequipped") as List<GameObject>;
				}
			}
			if (E == null || !E.HasFlag("OwnershipViolationDeclined"))
			{
				if (text == null)
				{
					text = "";
				}
				else if (text != "")
				{
					text += " ";
				}
				text = text + "You can't auto-equip " + obj.t() + ".";
			}
			string text2 = DescribeUnequip(WasUnequipped);
			if (!string.IsNullOrEmpty(text2))
			{
				if (text == null)
				{
					text = "";
				}
				else if (text != "")
				{
					text += "\n\n";
				}
				text += text2;
			}
			if (!string.IsNullOrEmpty(text))
			{
				Popup.ShowFail(text);
			}
		}
		if (WasInCell != null)
		{
			if (validate(ref obj) && obj.CurrentCell != WasInCell)
			{
				obj.RemoveFromContext();
				WasInCell.AddObject(obj);
			}
		}
		else if (WasInInventory?.Inventory != null && validate(ref obj) && obj.InInventory != WasInInventory)
		{
			obj.RemoveFromContext();
			WasInInventory.Inventory.AddObject(obj);
		}
	}

	private bool AutoEquipSucceed(bool Silent, List<GameObject> WasUnequipped)
	{
		if (!IsPlayer())
		{
			return true;
		}
		if (Silent)
		{
			return true;
		}
		string text = DescribeUnequip(WasUnequipped);
		if (!string.IsNullOrEmpty(text))
		{
			Popup.Show(text);
		}
		return true;
	}

	public string DescribeUnequip(List<GameObject> WasUnequipped)
	{
		if (WasUnequipped == null || WasUnequipped.Count == 0)
		{
			return null;
		}
		WasUnequipped.Sort((GameObject a, GameObject b) => a.HasProperName.CompareTo(b.HasProperName));
		List<string> list = new List<string>(WasUnequipped.Count);
		bool flag = false;
		bool flag2 = false;
		foreach (GameObject item in WasUnequipped)
		{
			if (item.IsValid())
			{
				list.Add(item.DisplayNameOnly);
				if (item.IsPlural)
				{
					flag = true;
				}
				if (!item.HasProperName)
				{
					flag2 = true;
				}
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting((flag2 ? "Your " : "") + Grammar.MakeAndList(list)) + " " + ((flag || list.Count > 1) ? "were" : "was") + " unequipped.";
	}

	public bool AutoEquip(GameObject GO, bool Forced = false, bool ForceHeld = false, bool Silent = false)
	{
		if (!validate(ref GO))
		{
			AutoEquipFail(GO, Silent);
			return false;
		}
		if (!Forced && GO.HasPropertyOrTag("CannotEquip"))
		{
			AutoEquipFail(GO, Silent);
			return false;
		}
		Cell currentCell = GO.CurrentCell;
		GameObject inInventory = GO.InInventory;
		if (inInventory != this && GO.Equipped != this)
		{
			GO.SplitFromStack();
			if (!ReceiveObject(GO))
			{
				GO.CheckStack();
				return false;
			}
		}
		List<GameObject> list = Event.NewGameObjectList();
		string text = (ForceHeld ? "Melee Weapon" : GO.GetInventoryCategory());
		if (!ForceHeld && GO.IsThrownWeapon)
		{
			BodyPart firstPart = Body.GetFirstPart("Thrown Weapon");
			if (firstPart == null)
			{
				AutoEquipFail(GO, Silent, currentCell, inInventory);
			}
			else
			{
				Event @event = Event.New("CommandEquipObject", "Object", GO, "BodyPart", firstPart);
				@event.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					@event.SetSilent(Silent: true);
				}
				@event.SetParameter("AutoEquipTry", 1);
				if (FireEvent(@event))
				{
					return AutoEquipSucceed(Silent, list);
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, @event);
			}
		}
		else
		{
			switch (text)
			{
			case "Shield":
			{
				XRL.World.Parts.Shield part5 = GO.GetPart<XRL.World.Parts.Shield>();
				Event event5 = Event.New("CommandEquipObject", "Object", GO);
				event5.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event5.SetSilent(Silent: true);
				}
				List<BodyPart> part6 = Body.GetPart(part5.WornOn);
				int num4 = 0;
				foreach (BodyPart item in part6)
				{
					if (item.Equipped != null && item.Equipped.HasPart("Shield"))
					{
						event5.SetParameter("BodyPart", item);
						event5.SetParameter("AutoEquipTry", ++num4);
						if (FireEvent(event5))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				if (part6.Count > 0)
				{
					event5.SetParameter("BodyPart", part6[0]);
					event5.SetParameter("AutoEquipTry", ++num4);
					if (FireEvent(event5))
					{
						return AutoEquipSucceed(Silent, list);
					}
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, event5);
				break;
			}
			case "Armor":
			{
				Armor part4 = GO.GetPart<Armor>();
				Event event4 = Event.New("CommandEquipObject", "Object", GO);
				event4.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event4.SetSilent(Silent: true);
				}
				int num3 = 0;
				Body body2 = Body;
				if (body2 != null)
				{
					foreach (BodyPart item2 in (part4.WornOn == "*") ? body2.LoopParts() : body2.LoopPart(part4.WornOn))
					{
						if (item2.Equipped == null)
						{
							event4.SetParameter("BodyPart", item2);
							event4.SetParameter("AutoEquipTry", ++num3);
							if (FireEvent(event4))
							{
								return AutoEquipSucceed(Silent, list);
							}
						}
					}
					BodyPart bodyPart2 = ((part4.WornOn == "*") ? body2.GetFirstPart() : body2.GetFirstPart(part4.WornOn));
					if (bodyPart2 != null)
					{
						event4.SetParameter("BodyPart", bodyPart2);
						event4.SetParameter("AutoEquipTry", ++num3);
						if (FireEvent(event4))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, event4);
				break;
			}
			case "Missile Weapon":
			{
				Event event7 = Event.New("CommandEquipObject", "Object", GO);
				event7.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event7.SetSilent(Silent: true);
				}
				MissileWeapon missileWeapon = GO.GetPart("MissileWeapon") as MissileWeapon;
				Body body3 = Body;
				if (missileWeapon == null)
				{
					MetricsManager.LogError("Item for missile weapon auto-equip had no MissileWeapon part: " + GO.DebugName);
					AutoEquipFail(GO, Silent, currentCell, inInventory, event7);
					break;
				}
				if (body3 == null)
				{
					MetricsManager.LogError("Creature trying to equip missile weapon had no body: " + DebugName);
					AutoEquipFail(GO, Silent, currentCell, inInventory, event7);
					break;
				}
				List<BodyPart> part8 = body3.GetPart(missileWeapon.GetSlotType());
				int num5 = 0;
				foreach (BodyPart item3 in part8)
				{
					event7.SetParameter("BodyPart", item3);
					event7.SetParameter("AutoEquipTry", ++num5);
					if (FireEvent(event7))
					{
						return AutoEquipSucceed(Silent, list);
					}
				}
				if (part8.Count > 0)
				{
					event7.SetParameter("BodyPart", part8[0]);
					event7.SetParameter("AutoEquipTry", ++num5);
					if (FireEvent(event7))
					{
						return AutoEquipSucceed(Silent, list);
					}
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, event7);
				break;
			}
			case "Ammo":
			{
				Event event6 = Event.New("CommandEquipObject", "Object", GO);
				event6.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event6.SetSilent(Silent: true);
				}
				List<GameObject> missileWeapons = GetMissileWeapons();
				if (missileWeapons == null || missileWeapons.Count == 0)
				{
					if (!Silent && IsPlayer())
					{
						Popup.ShowFail("You don't have a missile weapon equipped that uses that ammunition.");
					}
					return false;
				}
				foreach (GameObject item4 in missileWeapons)
				{
					MagazineAmmoLoader part7 = item4.GetPart<MagazineAmmoLoader>();
					if (part7 != null && (GO.HasPart(part7.AmmoPart) || GO.HasTag(part7.AmmoPart)))
					{
						part7.Unload(this);
						part7.Load(this, GO);
						UseEnergy(part7.ReloadEnergy, "Reload");
						return AutoEquipSucceed(Silent, list);
					}
				}
				if (!Silent && IsPlayer())
				{
					Popup.ShowFail("You don't have a missile weapon equipped that uses that ammunition.");
				}
				break;
			}
			case "Melee Weapon":
			{
				MeleeWeapon part2 = GO.GetPart<MeleeWeapon>();
				Event event3 = Event.New("CommandEquipObject", "Object", GO);
				event3.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event3.SetSilent(Silent: true);
				}
				List<BodyPart> part3 = Body.GetPart(part2.Slot);
				if (part3.Count > 0)
				{
					int num2 = 0;
					BodyPart bodyPart = null;
					foreach (BodyPart item5 in part3)
					{
						if (!item5.Primary)
						{
							continue;
						}
						if (bodyPart == null)
						{
							bodyPart = item5;
						}
						if (item5.Equipped == null)
						{
							event3.SetParameter("BodyPart", item5);
							event3.SetParameter("AutoEquipTry", ++num2);
							if (FireEvent(event3))
							{
								return AutoEquipSucceed(Silent, list);
							}
						}
					}
					foreach (BodyPart item6 in part3)
					{
						if (item6.Equipped == null)
						{
							event3.SetParameter("BodyPart", item6);
							event3.SetParameter("AutoEquipTry", ++num2);
							if (FireEvent(event3))
							{
								return AutoEquipSucceed(Silent, list);
							}
						}
					}
					event3.SetParameter("BodyPart", bodyPart ?? part3[0]);
					event3.SetParameter("AutoEquipTry", ++num2);
					if (FireEvent(event3))
					{
						return AutoEquipSucceed(Silent, list);
					}
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, event3);
				break;
			}
			default:
			{
				Event event2 = Event.New("CommandEquipObject", "Object", GO);
				event2.SetParameter("WasUnequipped", list);
				if (Silent)
				{
					event2.SetSilent(Silent: true);
				}
				Body body = Body;
				List<BodyPart> list2 = body.GetPart("Hand");
				if (!ForceHeld && GO.HasPart("Armor"))
				{
					Armor part = GO.GetPart<Armor>();
					if (part.WornOn != "Body")
					{
						list2 = ((!(part.WornOn == "*")) ? body.GetPart(part.WornOn) : body.GetParts());
					}
				}
				int num = 0;
				foreach (BodyPart item7 in list2)
				{
					if (!item7.Primary && item7.Equipped == null)
					{
						event2.SetParameter("BodyPart", item7);
						event2.SetParameter("AutoEquipTry", ++num);
						if (FireEvent(event2))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				foreach (BodyPart item8 in list2)
				{
					if (!item8.Primary && item8.Equipped != null)
					{
						event2.SetParameter("BodyPart", item8);
						event2.SetParameter("AutoEquipTry", ++num);
						if (FireEvent(event2))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				foreach (BodyPart item9 in list2)
				{
					if (item9.Primary && item9.Equipped == null)
					{
						event2.SetParameter("BodyPart", item9);
						event2.SetParameter("AutoEquipTry", ++num);
						if (FireEvent(event2))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				foreach (BodyPart item10 in list2)
				{
					if (item10.Primary && item10.Equipped != null)
					{
						event2.SetParameter("BodyPart", item10);
						event2.SetParameter("AutoEquipTry", ++num);
						if (FireEvent(event2))
						{
							return AutoEquipSucceed(Silent, list);
						}
					}
				}
				AutoEquipFail(GO, Silent, currentCell, inInventory, event2);
				break;
			}
			}
		}
		return false;
	}

	public bool UsesTwoSlotsFor(GameObject who)
	{
		if (!UsesTwoSlots)
		{
			return false;
		}
		if (who.GetIntProperty("HugeHands") > 0 && AllowHugeHandsEvent.Check(who, this))
		{
			return false;
		}
		return true;
	}

	public string GetTile()
	{
		if (pRender == null)
		{
			return null;
		}
		return pRender.Tile;
	}

	public string GetForegroundColor()
	{
		if (pRender == null)
		{
			return null;
		}
		return pRender.GetForegroundColor();
	}

	public void SetForegroundColor(string color)
	{
		if (pRender != null)
		{
			pRender.SetForegroundColor(color);
		}
	}

	public void SetForegroundColor(char color)
	{
		if (pRender != null)
		{
			pRender.SetForegroundColor(color);
		}
	}

	public string GetBackgroundColor()
	{
		if (pRender == null)
		{
			return null;
		}
		return pRender.GetBackgroundColor();
	}

	public void SetBackgroundColor(string color)
	{
		if (pRender != null)
		{
			pRender.SetBackgroundColor(color);
		}
	}

	public string GetDetailColor()
	{
		if (pRender == null)
		{
			return null;
		}
		return pRender.DetailColor;
	}

	public void SetBackgroundColor(char color)
	{
		if (pRender != null)
		{
			pRender.SetBackgroundColor(color);
		}
	}

	public void SetDetailColor(string color)
	{
		if (pRender != null)
		{
			pRender.DetailColor = color;
		}
	}

	public void SetDetailColor(char color)
	{
		if (pRender != null)
		{
			pRender.DetailColor = color.ToString() ?? "";
		}
	}

	public List<Effect> GetTonicEffects()
	{
		List<Effect> list = new List<Effect>();
		if (Effects != null)
		{
			foreach (Effect effect in Effects)
			{
				if (effect.Duration > 0 && effect.IsTonic())
				{
					list.Add(effect);
				}
			}
			return list;
		}
		return list;
	}

	public int GetTonicEffectCount()
	{
		int num = 0;
		if (Effects != null)
		{
			int i = 0;
			for (int count = Effects.Count; i < count; i++)
			{
				if (Effects[i].Duration > 0 && Effects[i].IsTonic())
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetTonicCapacity()
	{
		return GetTonicCapacityEvent.GetFor(this);
	}

	public bool IsBroken()
	{
		return HasEffectByClass("Broken");
	}

	public bool IsRusted()
	{
		return HasEffectByClass("Rusted");
	}

	public bool IsEMPed()
	{
		return HasEffectByClass("ElectromagneticPulsed");
	}

	public bool IsInStasis()
	{
		return HasEffectByClass("Stasis");
	}

	public bool IsLedBy(GameObject GO)
	{
		return pBrain?.IsLedBy(GO) ?? false;
	}

	public bool InSamePartyAs(GameObject GO)
	{
		return pBrain?.InSamePartyAs(GO) ?? false;
	}

	public bool IsTryingToJoinPartyLeader()
	{
		return pBrain?.IsTryingToJoinPartyLeader() ?? false;
	}

	public bool IsTryingToJoinPartyLeaderForZoneUncaching()
	{
		if (!IsTryingToJoinPartyLeader())
		{
			return false;
		}
		return FireEvent("KeepZoneCachedForPlayerJoin");
	}

	public bool IsFluid()
	{
		if (HasPart("Gas"))
		{
			return true;
		}
		LiquidVolume liquidVolume = LiquidVolume;
		if (liquidVolume != null && liquidVolume.MaxVolume == -1)
		{
			return true;
		}
		return false;
	}

	public bool ConsiderSolid()
	{
		if (pPhysics == null || !pPhysics.Solid || !pPhysics.IsReal)
		{
			return false;
		}
		return true;
	}

	public bool ConsiderSolidInRenderingContext()
	{
		if (pBrain != null && pBrain.LivesOnWalls)
		{
			return true;
		}
		if (pRender != null && !pRender.Occluding)
		{
			return false;
		}
		if (ConsiderSolid())
		{
			return true;
		}
		return false;
	}

	public bool ConsiderSolid(bool ForFluid)
	{
		if (!ConsiderSolid())
		{
			return false;
		}
		if (ForFluid)
		{
			if (HasTagOrProperty("Flyover"))
			{
				return false;
			}
			if (GetIntProperty("AllowMissiles") != 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool ConsiderSolid(bool ForFluid, int Phase)
	{
		if (!ConsiderSolid())
		{
			return false;
		}
		if (ForFluid)
		{
			if (HasTagOrProperty("Flyover"))
			{
				return false;
			}
			if (GetIntProperty("AllowMissiles") != 0)
			{
				return false;
			}
		}
		if (!PhaseMatches(Phase))
		{
			return false;
		}
		return true;
	}

	public bool ConsiderSolidFor(GameObject obj)
	{
		if (ConsiderSolid())
		{
			bool immobile = true;
			bool waterbound = false;
			bool wallwalker = false;
			obj?.pBrain?.checkMobility(out immobile, out waterbound, out wallwalker);
			if (wallwalker && IsWalkableWall(obj))
			{
				return false;
			}
			if (obj == null)
			{
				return true;
			}
			if (XRLCore.Core.IDKFA && obj.IsPlayer())
			{
				return false;
			}
			if (obj.IsFluid() || obj.IsFlying)
			{
				if (HasTagOrProperty("Flyover"))
				{
					return false;
				}
				if (GetIntProperty("AllowMissiles") != 0)
				{
					return false;
				}
			}
			if (!PhaseMatches(obj))
			{
				return false;
			}
			if (GetPart("Forcefield") is Forcefield forcefield && !HasPart("Stasisfield") && forcefield.CanPass(obj))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool ConsiderSolidInRenderingContextFor(GameObject obj)
	{
		if (pBrain != null && pBrain.LivesOnWalls)
		{
			return true;
		}
		return ConsiderSolidFor(obj);
	}

	public bool ConsiderUnableToOccupySameCell(GameObject obj)
	{
		if (pPhysics == null || !pPhysics.IsReal)
		{
			return false;
		}
		if (obj.pPhysics == null || !obj.pPhysics.IsReal)
		{
			return false;
		}
		if (HasPart("Combat") && obj.HasPart("Combat"))
		{
			return PhaseAndFlightMatches(obj);
		}
		if (obj.ConsiderSolidFor(this) && (pBrain == null || !pBrain.LivesOnWalls || !obj.IsWall()))
		{
			return true;
		}
		return false;
	}

	public bool ConsiderSolidFor(GameObject Projectile, GameObject Attacker)
	{
		if (Projectile == null)
		{
			return ConsiderSolidFor(Attacker);
		}
		if (pPhysics == null || !pPhysics.Solid)
		{
			return false;
		}
		if (HasTagOrProperty("Flyover"))
		{
			return false;
		}
		if (GetIntProperty("AllowMissiles") != 0)
		{
			return false;
		}
		if (!PhaseMatches(Projectile))
		{
			return false;
		}
		if (GetPart("Forcefield") is Forcefield forcefield && !HasPart("Stasisfield") && forcefield.CanMissilePassFrom(Attacker))
		{
			return false;
		}
		if (Projectile.HasTagOrProperty("Light") && HasTagOrProperty("Transparent"))
		{
			return false;
		}
		return true;
	}

	public int GetBodyPartCount()
	{
		return Body?.GetPartCount() ?? 0;
	}

	public int GetBodyPartCount(string Type)
	{
		return Body?.GetPartCount(Type) ?? 0;
	}

	public int GetBodyPartCount(Predicate<BodyPart> Filter)
	{
		return Body?.GetPartCount(Filter) ?? 0;
	}

	public int GetConcreteBodyPartCount()
	{
		return Body?.GetConcretePartCount() ?? 0;
	}

	public int GetAbstractBodyPartCount()
	{
		return Body?.GetAbstractPartCount() ?? 0;
	}

	public BodyPart GetRandomConcreteBodyPart(string preferType = null)
	{
		Body body = Body;
		if (preferType != null)
		{
			BodyPart randomElement = body.GetPart(preferType).GetRandomElement();
			if (randomElement != null && !randomElement.Abstract)
			{
				return randomElement;
			}
		}
		return body.GetConcreteParts().GetRandomElement();
	}

	public int GetSeededRange(string Channel, int low, int high)
	{
		return GetSeededRandom(Channel).Next(low, high + 1);
	}

	public System.Random GetSeededRandom(string Channel = "")
	{
		return WithSeededRandom((System.Random i) => i, Channel);
	}

	/// <summary>Creates a System.Random from the RandomSeed:Channel intproperty (initialized from worldseed + object id + channel)
	///             calling your function with it, and then writing a new seed to the intproperty based on the next number out of the RNG.
	///             Seeding is sensitive to the number of numbers you pull out of the RNG given to your proc</summary>
	public T WithSeededRandom<T>(Func<System.Random, T> proc, string Channel = "")
	{
		string text = "RandomSeed:" + Channel;
		if (!TryGetIntProperty(text, out var Result))
		{
			Result = Hash.String(XRLCore.Core.Game.GetWorldSeed() + id + text);
		}
		System.Random random = new System.Random(Result);
		T result = proc(random);
		SetIntProperty(text, random.Next());
		return result;
	}

	public void WithSeededRandom(Action<System.Random> proc, string Channel = "")
	{
		string text = "RandomSeed:" + Channel;
		if (!TryGetIntProperty(text, out var Result))
		{
			Result = Hash.String(XRLCore.Core.Game.GetWorldSeed() + id + text);
		}
		System.Random random = new System.Random(Result);
		proc(random);
		SetIntProperty(text, random.Next());
	}

	public void PermuteRandomMutationBuys()
	{
		GetSeededRandom("RandomMutationBuy");
		GetSeededRandom("brainbrine");
	}

	public bool CanMakeTelepathicContactWith(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (!HasPart("Telepathy"))
		{
			return false;
		}
		if (!CanReceiveTelepathyEvent.Check(who, this))
		{
			return false;
		}
		return true;
	}

	public bool CanMakeEmpathicContactWith(GameObject who)
	{
		if (who == null)
		{
			return false;
		}
		if (!HasPart("Empathy"))
		{
			return false;
		}
		if (!CanReceiveEmpathyEvent.Check(who, this))
		{
			return false;
		}
		return true;
	}

	public bool CanManipulateTelekinetically(GameObject what)
	{
		if (what == null)
		{
			return false;
		}
		if (!(GetPart("Telekinesis") is Telekinesis telekinesis))
		{
			return false;
		}
		if (DistanceTo(what) > telekinesis.GetTelekineticRange())
		{
			return false;
		}
		return true;
	}

	public Cell PickDirection()
	{
		return pPhysics?.PickDirection();
	}

	public int GetBodyWeight()
	{
		int num = Stat("Strength");
		if (num <= 0)
		{
			return 0;
		}
		return num * num - num * 15 + 150;
	}

	public int GetKineticResistance()
	{
		return GetKineticResistanceEvent.GetFor(this);
	}

	public int GetSpringiness()
	{
		return GetSpringinessEvent.GetFor(this);
	}

	public int GetKineticAbsorption()
	{
		return Math.Max(GetKineticResistance() + GetSpringiness(), 1);
	}

	public void Gravitate()
	{
		GravitationEvent.Check(this, CurrentCell);
	}

	public int GetVisibilityRadius()
	{
		if (XRLCore.Core.VisAllToggle)
		{
			return 80;
		}
		return AdjustVisibilityRadiusEvent.GetFor(this, GetIntProperty("VisibilityRadius", 80));
	}

	public void PotentiallyAngerOwner(GameObject who, string Suppress = null)
	{
		if (!string.IsNullOrEmpty(Owner) && (string.IsNullOrEmpty(Suppress) || !HasPropertyOrTag(Suppress)))
		{
			pPhysics?.BroadcastForHelp(who);
		}
		GameObject inInventory = InInventory;
		if (!string.IsNullOrEmpty(inInventory?.Owner) && inInventory.Owner != Owner && (string.IsNullOrEmpty(Suppress) || !inInventory.HasPropertyOrTag(Suppress)))
		{
			inInventory.pPhysics?.BroadcastForHelp(who);
		}
	}

	public int GetFuriousConfusion()
	{
		int num = 0;
		foreach (Effect effect in Effects)
		{
			if (effect is FuriouslyConfused furiouslyConfused)
			{
				num += furiouslyConfused.Level;
			}
		}
		return num + GetIntProperty("FuriousConfusionLevel");
	}

	public int GetConfusion()
	{
		int num = 0;
		foreach (Effect effect in Effects)
		{
			if (effect is XRL.World.Effects.Confused confused)
			{
				num += confused.Level;
			}
			else if (effect is FuriouslyConfused furiouslyConfused)
			{
				num += furiouslyConfused.Level;
			}
			else if (effect is HulkHoney_Tonic_Allergy)
			{
				num++;
			}
		}
		return num + GetIntProperty("ConfusionLevel");
	}

	public int GetTotalConfusion()
	{
		return GetConfusion();
	}

	public void RestorePristineHealth(bool UseHeal = false)
	{
		Statistic stat = GetStat("Hitpoints");
		if (stat != null)
		{
			if (UseHeal)
			{
				Heal(stat.BaseValue, Message: true, FloatText: true);
			}
			else
			{
				stat.Penalty = 0;
			}
		}
		Body body = Body;
		if (body != null)
		{
			int num = 0;
			while (body.DismemberedParts != null && body.DismemberedParts.Count > 0 && ++num < 100)
			{
				body.RegenerateLimb();
			}
		}
		if (GetPart("Stomach") is Stomach stomach)
		{
			stomach.Water = 50000;
		}
		if (Effects == null)
		{
			return;
		}
		List<Effect> list = null;
		foreach (Effect effect in Effects)
		{
			if (effect.IsOfTypes(100663296) && !effect.IsOfType(134217728))
			{
				if (list == null)
				{
					list = new List<Effect>();
				}
				list.Add(effect);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (Effect item in list)
		{
			if (Effects.Contains(item))
			{
				RemoveEffect(item, NeedStackCheck: false);
			}
		}
		CheckStack();
	}

	public bool HasInventoryActionWithName(string Name, GameObject Actor = null)
	{
		foreach (InventoryAction inventoryAction in EquipmentAPI.GetInventoryActions(this, Actor ?? ThePlayer))
		{
			if (inventoryAction.Name == Name)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInventoryActionWithCommand(string Command, GameObject Actor = null)
	{
		foreach (InventoryAction inventoryAction in EquipmentAPI.GetInventoryActions(this, Actor ?? ThePlayer))
		{
			if (inventoryAction.Command == Command)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSpecialItem()
	{
		if (HasProperty("RelicName"))
		{
			return true;
		}
		if (HasTagOrProperty("QuestItem"))
		{
			return true;
		}
		if (HasPart("ModExtradimensional"))
		{
			return true;
		}
		if (IsImportant())
		{
			return true;
		}
		return false;
	}

	public void Fail(string msg)
	{
		if (IsPlayer())
		{
			Popup.ShowFail(msg);
		}
	}

	public void ForfeitTurn(bool EnergyNeutral = false)
	{
		if (!EnergyNeutral)
		{
			Statistic energy = Energy;
			if (energy != null)
			{
				energy.BaseValue = 0;
			}
		}
		if (IsPlayer())
		{
			ActionManager.SkipPlayerTurn = true;
		}
	}

	public string GetListDisplayContext(GameObject who)
	{
		if (this == who)
		{
			return "self";
		}
		GameObject inInventory = InInventory;
		if (inInventory != null)
		{
			if (inInventory == who)
			{
				return "inventory";
			}
			if (inInventory.InInventory == who || inInventory.Equipped == who)
			{
				return inInventory.DisplayNameOnlyStripped;
			}
		}
		if (Equipped == who)
		{
			BodyPart bodyPart = who.FindEquippedObject(this);
			if (bodyPart != null)
			{
				return bodyPart.Name;
			}
			return "equipped";
		}
		GetContext(out var ObjectContext, out var CellContext);
		if (ObjectContext != null)
		{
			if (ObjectContext == who)
			{
				return "held";
			}
			return ObjectContext.DisplayNameOnlyStripped;
		}
		if (CellContext != null)
		{
			if (who.DistanceTo(this) <= 1)
			{
				return Directions.GetDirectionDescription(who.CurrentCell.GetDirectionFromCell(CellContext));
			}
			return "elsewhere";
		}
		return null;
	}

	public int GetHeartCount()
	{
		int num = 1 + GetIntProperty("ExtraHearts");
		if (HasPart("HeightenedToughness"))
		{
			num++;
		}
		return num;
	}

	public void SetHasMarkOfDeath(bool flag)
	{
		if (flag)
		{
			SetIntProperty("HasMarkOfDeath", 1, RemoveIfZero: true);
		}
		else
		{
			SetIntProperty("HasMarkOfDeath", 2, RemoveIfZero: true);
		}
	}

	public bool FindMarkOfDeath(IPart skip = null)
	{
		if (!(GetPartExcept("Tattoos", skip) is Tattoos tattoos))
		{
			return false;
		}
		string stringGameState = XRL.The.Game.GetStringGameState("MarkOfDeath");
		foreach (List<string> value in tattoos.Descriptions.Values)
		{
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				if (value[i].Contains(stringGameState))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMarkOfDeath(IPart skip = null)
	{
		switch (GetIntProperty("HasMarkOfDeath"))
		{
		case 1:
			return true;
		case 2:
			return false;
		default:
		{
			bool flag = FindMarkOfDeath();
			SetHasMarkOfDeath(flag);
			return flag;
		}
		}
	}

	public void CheckMarkOfDeath(IPart skip = null)
	{
		SetHasMarkOfDeath(FindMarkOfDeath(skip));
	}

	public void ToggleMarkOfDeath()
	{
		SetHasMarkOfDeath(!HasMarkOfDeath());
	}

	public int GetMaximumLiquidExposure()
	{
		return GetMaximumLiquidExposureEvent.GetFor(this);
	}

	public double GetMaximumLiquidExposureAsDouble()
	{
		return GetMaximumLiquidExposureEvent.GetDoubleFor(this);
	}

	public bool CanBeInvoluntarilyMoved()
	{
		return CanBeInvoluntarilyMovedEvent.Check(this);
	}

	public bool CanBeDismembered(string Attributes = null)
	{
		return CanBeDismemberedEvent.Check(this, Attributes);
	}

	public bool CanBeDismembered(GameObject Weapon)
	{
		return CanBeDismembered(Weapon?.GetPart<MeleeWeapon>()?.Attributes);
	}

	public bool CanHaveNosebleed()
	{
		if (!Effect.CanEffectTypeBeAppliedTo(24, this))
		{
			return false;
		}
		if (!Respires)
		{
			return false;
		}
		Body body = Body;
		if (body == null || !body.HasVariantPart("Face"))
		{
			return false;
		}
		if (!GetPropertyOrTag("BleedLiquid", "blood-1000").Contains("blood"))
		{
			return false;
		}
		return true;
	}

	public bool CanClear(bool Important = false, bool Combat = false)
	{
		if ((Important || !IsImportant()) && !HasPropertyOrTag("NoClear"))
		{
			if (!Combat)
			{
				return !IsCombatObject();
			}
			return true;
		}
		return false;
	}

	public int GetHostileWalkRadius(GameObject who)
	{
		return GetHostileWalkRadiusEvent.GetFor(who, this);
	}

	public bool IsWalkableWall(GameObject By, ref bool Uncacheable)
	{
		if (!IsWall())
		{
			return false;
		}
		if (!PhaseMatches(By))
		{
			return false;
		}
		if (IsCreature)
		{
			Uncacheable = true;
			if (!IsAlliedTowards(By))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsWalkableWall(GameObject By)
	{
		bool Uncacheable = false;
		return IsWalkableWall(By, ref Uncacheable);
	}

	public bool CanBePositionSwapped(GameObject By = null)
	{
		if (!HasPropertyOrTag("Noswap"))
		{
			return true;
		}
		if (IsMobile())
		{
			return true;
		}
		return false;
	}

	public string GetBleedLiquid()
	{
		return GetPropertyOrTag("BleedLiquid", "blood-1000");
	}

	public int GetPowerLoadLevel()
	{
		return GetPowerLoadLevelEvent.GetFor(this);
	}

	public Cell MovingTo()
	{
		return pBrain?.MovingTo();
	}

	public bool IsFleeing()
	{
		return pBrain?.IsFleeing() ?? false;
	}

	public bool ShouldShunt()
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		if (!IsCombatObject(NoBrainOnly: true))
		{
			return false;
		}
		int i = 0;
		for (int count = currentCell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = currentCell.Objects[i];
			if (gameObject != this && gameObject.IsCombatObject(NoBrainOnly: true))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFactionMember(string Faction)
	{
		return pBrain?.IsFactionMember(Faction) ?? false;
	}

	public bool IsSafeContainerForLiquid(string liquid)
	{
		return LiquidVolume.IsGameObjectSafeContainerForLiquid(this, liquid);
	}

	public void Indicate()
	{
		if (juiceEnabled)
		{
			ParticleText("v", 'W', IgnoreVisibility: true);
		}
		else
		{
			ParticleBlip("&WX");
		}
	}

	public void EmitMessage(string Message, GameObject Object = null, string Color = null, bool UsePopup = false)
	{
		if (string.IsNullOrEmpty(Message) || !IsVisible())
		{
			return;
		}
		string text = GameText.VariableReplace(Message, this, null, ExplicitSubjectPlural: false, Object);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (UsePopup)
		{
			if (!string.IsNullOrEmpty(Color))
			{
				text = "{{" + Color + "|" + text + "}}";
			}
			Popup.Show(text);
		}
		else
		{
			MessageQueue.AddPlayerMessage(text, Color);
		}
	}

	public void PlayWorldSound(string clip, float Volume = 0.5f, float PitchVariance = 0f, bool combat = false, float delay = 0f)
	{
		if (!string.IsNullOrEmpty(clip))
		{
			CurrentCell?.PlayWorldSound(clip, Volume, PitchVariance, combat, delay);
		}
	}

	public string GetLiquidColor(string Default = "K")
	{
		return LiquidVolume?.GetPrimaryLiquidColor() ?? Default;
	}

	public string GetTinkeringBlueprint()
	{
		return (GetPart("TinkerItem") as TinkerItem)?.ActiveBlueprint ?? Blueprint;
	}

	public bool AttackDirection(string Direction, bool EnableSwoop = true)
	{
		Cell currentCell = CurrentCell;
		if (currentCell == null)
		{
			return false;
		}
		Cell cellFromDirection = currentCell.GetCellFromDirection(Direction, BuiltOnly: false);
		if (cellFromDirection == null)
		{
			return false;
		}
		if (EnableSwoop && IsFlying && cellFromDirection.GetCombatTarget(this) == null && cellFromDirection.GetCombatTarget(this, IgnoreFlight: true) != null)
		{
			return FireEvent(Event.New("CommandSwoopAttack", "Direction", Direction));
		}
		return FireEvent(Event.New("CommandAttackDirection", "Direction", Direction));
	}

	public bool HasBeenRead(GameObject By = null)
	{
		if (By == null)
		{
			By = XRL.The.Player;
		}
		return HasBeenReadEvent.Check(this, By);
	}

	public bool MakeNonflammable()
	{
		if (!HasTag("Creature") && pPhysics != null)
		{
			pPhysics.FlameTemperature = 99999;
			return true;
		}
		return false;
	}

	public bool MakeImperviousToHeat()
	{
		if (!HasTag("Creature") && pPhysics != null)
		{
			pPhysics.FlameTemperature = 99999;
			pPhysics.VaporTemperature = 99999;
			return true;
		}
		return false;
	}

	public bool CheckHP(int? CurrentHP = null, int? PreviousHP = null, int? MaxHP = null, bool Preregistered = false)
	{
		return pPhysics?.CheckHP(CurrentHP, PreviousHP, MaxHP, Preregistered) ?? false;
	}

	public bool WillCheckHP(bool? Registering = null)
	{
		if (Registering == true)
		{
			return ModIntProperty("WillCheckHP", 1) > 0;
		}
		if (Registering == false)
		{
			return ModIntProperty("WillCheckHP", -1) > 0;
		}
		return GetIntProperty("WillCheckHP") > 0;
	}

	public bool NeedsRecharge()
	{
		foreach (IRechargeable item in GetPartsDescendedFrom<IRechargeable>())
		{
			if (item.CanBeRecharged() && item.GetRechargeAmount() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanApplyEffect(string Name, int Duration = 0, string EventName = null)
	{
		if (!FireEvent(EventName ?? ("CanApply" + Name)))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check(this, Name, Duration))
		{
			return false;
		}
		return true;
	}
}
