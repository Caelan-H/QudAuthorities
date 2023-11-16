using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Liquids;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class SoupSludge : IPart
{
	public static readonly string[] Prefixes = new string[20]
	{
		"mono", "di", "tri", "tetra", "penta", "hexa", "hepta", "octa", "ennea", "deca",
		"hendeca", "dodeca", "triskaideca", "tetrakaideca", "pentakaideca", "hexakaideca", "heptakaideca", "octakaideca", "enneakaideca", "icosa"
	};

	public List<string> ComponentLiquids = new List<string>();

	public string LiquidID;

	[FieldSaveVersion(223)]
	public int MaxAdjectives = int.MaxValue;

	public const int CLR_DIV = 240;

	[NonSerialized]
	private byte Hero;

	[NonSerialized]
	private string Detail;

	[NonSerialized]
	private List<int> Components = new List<int>();

	[NonSerialized]
	private long DisplayTurn = -1L;

	public string ManagerID => ParentObject.id + "::SoupSludge";

	public SoupSludge()
	{
	}

	public SoupSludge(string LiquidID)
	{
		this.LiquidID = LiquidID;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Initialize()
	{
		EvolveSludge();
	}

	public override void Remove()
	{
		ParentObject.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		base.Remove();
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		SoupSludge obj = base.DeepCopy(Parent) as SoupSludge;
		obj.ComponentLiquids = new List<string>(ComponentLiquids);
		return obj;
	}

	public static string GetPrefix(int Count)
	{
		if (Count <= 0)
		{
			return "";
		}
		if (Count <= Prefixes.Length)
		{
			return Prefixes[Count - 1];
		}
		return "poly";
	}

	public void CatalyzeLiquid(string ID)
	{
		if (!ComponentLiquids.Contains(ID))
		{
			Mutations mutations = ParentObject.RequirePart<Mutations>();
			LiquidSpitter liquidSpitter = mutations.GetMutation("LiquidSpitter") as LiquidSpitter;
			if (liquidSpitter == null)
			{
				liquidSpitter = new LiquidSpitter(ID);
				mutations.AddMutation(liquidSpitter, 1);
			}
			liquidSpitter.AddLiquid(ID);
			ComponentLiquids.Add(ID);
			EquipPseudopod(ID);
			ParentObject.GetStat("Speed").BaseValue += 10;
			ParentObject.GetStat("Hitpoints").BaseValue += 20;
			ParentObject.GetStat("Level").BaseValue += 3;
		}
	}

	public void EvolveSludge()
	{
		if (LiquidID != null)
		{
			string shortDisplayName = ParentObject.ShortDisplayName;
			CatalyzeLiquid(LiquidID);
			CatalyzeName();
			CatalyzeMessage(LiquidID, shortDisplayName);
			LiquidID = null;
		}
	}

	public void CatalyzeName()
	{
		if (ComponentLiquids.Count != 0)
		{
			string text = GetPrefix(ComponentLiquids.Count) + "sludge";
			if (ParentObject.HasProperName)
			{
				string oldValue = GetPrefix(ComponentLiquids.Count - 1) + "sludge";
				ParentObject.DisplayName = ParentObject.DisplayNameOnlyDirect.Replace(oldValue, text);
			}
			else
			{
				ParentObject.DisplayName = text;
			}
		}
	}

	public void CatalyzeMessage(string ID, string OldName)
	{
		if (ComponentLiquids.Count >= 5)
		{
			JournalAPI.AddAccomplishment("You witnessed the rare formation of " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".", "=name= was blessed to witness the rare formation of " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".", "general", JournalAccomplishment.MuralCategory.HasInspiringExperience, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		if (ParentObject.HasProperName)
		{
			EmitMessage("The " + LiquidVolume.getLiquid(ID).GetName() + " catalyzes " + ParentObject.the + OldName + " into " + Grammar.A(GetPrefix(ComponentLiquids.Count) + "sludge") + ".");
		}
		else
		{
			EmitMessage("The " + LiquidVolume.getLiquid(ID).GetName() + " catalyzes " + ParentObject.the + OldName + " into " + ParentObject.a + ParentObject.DisplayName + ".");
		}
	}

	public bool HasEmptyHands()
	{
		foreach (BodyPart item in ParentObject.Body.GetPart("Hand"))
		{
			if (item.Equipped == null)
			{
				return true;
			}
		}
		return false;
	}

	public void AddHandsIfFull()
	{
		if (!HasEmptyHands())
		{
			BodyPart body = ParentObject.Body.GetBody();
			BodyPart insertAfter = body.AddPartAt("Pseudopod", 2, null, null, null, null, ManagerID, null, null, null, null, null, null, null, null, null, null, null, null, "Hand", "Missile Weapon");
			body.AddPartAt(insertAfter, "Pseudopod", 1, null, null, null, null, ManagerID);
		}
	}

	public void EquipPseudopod(string ID)
	{
		AddHandsIfFull();
		switch (ID)
		{
		case "water":
			ParentObject.TakeObject("Watery Pseudopod", Silent: false, 0);
			break;
		case "salt":
			ParentObject.TakeObject("Salty Pseudopod", Silent: false, 0);
			break;
		case "asphalt":
			ParentObject.TakeObject("Tarry Pseudopod", Silent: false, 0);
			break;
		case "lava":
			ParentObject.TakeObject("Magmatic Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new LavaSludge());
			ParentObject.GetStat("HeatResistance").BaseValue += 100;
			ParentObject.pPhysics.Temperature = 1000;
			break;
		case "slime":
			ParentObject.TakeObject("Slimy Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "oil":
			ParentObject.TakeObject("Oily Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "blood":
			ParentObject.TakeObject("Bloody Pseudopod", Silent: false, 0);
			break;
		case "acid":
			ParentObject.TakeObject("Acidic Pseudopod", Silent: false, 0);
			ParentObject.GetStat("AcidResistance").BaseValue += 100;
			break;
		case "honey":
			ParentObject.TakeObject("Honeyed Pseudopod", Silent: false, 0);
			break;
		case "wine":
			ParentObject.TakeObject("Lush Pseudopod", Silent: false, 0);
			break;
		case "sludge":
			ParentObject.TakeObject("Sludgy Pseudopod2", Silent: false, 0);
			break;
		case "goo":
			ParentObject.TakeObject("Gooey Pseudopod2", Silent: false, 0);
			break;
		case "putrid":
			ParentObject.TakeObject("Putrid Pseudopod", Silent: false, 0);
			break;
		case "gel":
			ParentObject.TakeObject("Unctuous Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "ooze":
			ParentObject.TakeObject("Oozing Pseudopod2", Silent: false, 0);
			break;
		case "cider":
			ParentObject.TakeObject("Spiced Pseudopod", Silent: false, 0);
			break;
		case "convalessence":
			ParentObject.TakeObject("Luminous Pseudopod2", Silent: false, 0);
			ParentObject.GetStat("ColdResistance").BaseValue += 100;
			ParentObject.RequirePart<LightSource>().Radius = 6;
			break;
		case "neutronflux":
			ParentObject.TakeObject("Neutronic Pseudopod", Silent: false, 0);
			break;
		case "cloning":
			ParentObject.TakeObject("Homogenized Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new CloneOnHit());
			break;
		case "wax":
			ParentObject.TakeObject("Waxen Pseudopod", Silent: false, 0);
			break;
		case "ink":
			ParentObject.TakeObject("Inky Pseudopod", Silent: false, 0);
			ParentObject.AddPart(new DisarmOnHit(100));
			break;
		case "sap":
			ParentObject.TakeObject("Sugary Pseudopod", Silent: false, 0);
			break;
		case "brainbrine":
			ParentObject.TakeObject("Nervous Pseudopod", Silent: false, 0);
			break;
		case "algae":
			ParentObject.TakeObject("Algal Pseudopod", Silent: false, 0);
			break;
		case "sunslag":
			ParentObject.TakeObject("Radiant Pseudopod", Silent: false, 0);
			ParentObject.GetStat("Speed").BaseValue += 20;
			ParentObject.RequirePart<LightSource>().Radius = 6;
			break;
		}
		ParentObject.pBrain.PerformEquip();
	}

	public override bool Render(RenderEvent E)
	{
		if (ComponentLiquids.Count > 0)
		{
			long elapsedMilliseconds = XRLCore.FrameTimer.ElapsedMilliseconds;
			if (Hero == 1)
			{
				if (elapsedMilliseconds % 480 > 240)
				{
					return true;
				}
				if (Detail == null)
				{
					Detail = ColorUtility.FindLastForeground(E.ColorString)?.ToString() ?? "M";
				}
				int num = (int)(elapsedMilliseconds % (480 * ComponentLiquids.Count));
				E.ColorString = E.ColorString + "&" + LiquidVolume.getLiquid(ComponentLiquids[num / 480]).GetColor();
				E.DetailColor = Detail;
			}
			else if (Hero == 0)
			{
				if (ParentObject.HasIntProperty("Hero") || ParentObject.HasPart("GivesRep"))
				{
					Hero = 1;
				}
				else
				{
					Hero = 2;
				}
			}
			else
			{
				int num2 = (int)(elapsedMilliseconds % (240 * ComponentLiquids.Count));
				E.ColorString = E.ColorString + "&" + LiquidVolume.getLiquid(ComponentLiquids[num2 / 240]).GetColor();
			}
		}
		return true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != GetDisplayNameEvent.ID && ID != EnteredCellEvent.ID && ID != BeginTakeActionEvent.ID)
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Object.HasProperName)
		{
			return true;
		}
		if (ComponentLiquids.Count > MaxAdjectives)
		{
			long num = The.Game?.Turns ?? 0;
			if (num != DisplayTurn)
			{
				DisplayTurn = num;
				Components.Clear();
				Random random = new Random(DisplayTurn.GetHashCode());
				int num2 = Math.Min(MaxAdjectives, ComponentLiquids.Count);
				while (Components.Count < num2)
				{
					int item = random.Next(0, ComponentLiquids.Count);
					if (!Components.Contains(item))
					{
						Components.Add(item);
					}
				}
			}
			foreach (int component in Components)
			{
				BaseLiquid liquid = LiquidVolume.getLiquid(ComponentLiquids[component]);
				E.AddAdjective(liquid.GetAdjective(null));
			}
		}
		else
		{
			foreach (string componentLiquid in ComponentLiquids)
			{
				E.AddAdjective(LiquidVolume.getLiquid(componentLiquid).GetAdjective(null));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (LiquidID != null)
		{
			return true;
		}
		GameObject openLiquidVolume = E.Cell.GetOpenLiquidVolume();
		if (openLiquidVolume == null)
		{
			return true;
		}
		LiquidVolume liquidVolume = openLiquidVolume.LiquidVolume;
		if (!ReactWith(liquidVolume.GetPrimaryLiquidID(), liquidVolume))
		{
			ReactWith(liquidVolume.GetSecondaryLiquidID(), liquidVolume);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (E.Killer != null && E.Killer.IsPlayer())
		{
			switch (ComponentLiquids.Count)
			{
			case 3:
				AchievementManager.SetAchievement("ACH_KILL_TRISLUDGE");
				break;
			case 5:
				AchievementManager.SetAchievement("ACH_KILL_PENTASLUDGE");
				break;
			case 10:
				AchievementManager.SetAchievement("ACH_KILL_DECASLUDGE");
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (LiquidID != null)
		{
			EvolveSludge();
		}
		return base.HandleEvent(E);
	}

	public bool ReactWith(string ID, LiquidVolume LV)
	{
		if (ID == null || ComponentLiquids.Contains(ID))
		{
			return false;
		}
		if (ID == "cloning" || ID == "proteangunk")
		{
			return false;
		}
		DidX("start", "reacting with the " + LiquidVolume.getLiquid(ID).GetName(LV));
		LV.UseDrams(500);
		LiquidID = ID;
		return true;
	}
}
