using System;
using System.Collections.Generic;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
[HasGameBasedStaticCache]
public class Examiner : IPart
{
	public const string OWNER_ANGER_SUPPRESS = "DontWarnOnExamine";

	public const int EPISTEMIC_STATUS_UNINITIALIZED = -1;

	public const int EPISTEMIC_STATUS_UNKNOWN = 0;

	public const int EPISTEMIC_STATUS_PARTIAL = 1;

	public const int EPISTEMIC_STATUS_KNOWN = 2;

	public int Complexity;

	public int Difficulty;

	public string AlternateDisplayName = "weird artifact";

	public string UnknownDisplayName = "weird artifact";

	public string AlternateDescription = "A puzzling artifact.";

	public string UnknownDescription = "A puzzling artifact.";

	public string AlternateTile = "items/sw_gadget.bmp";

	public string UnknownTile = "items/sw_gadget.bmp";

	public string AlternateGender = "neuter";

	public string UnknownGender = "neuter";

	public string AlternateColorString = "&c";

	public string UnknownColorString = "&c";

	public string AlternateTileColor = "&c";

	public string UnknownTileColor = "&c";

	public string AlternateDetailColor = "C";

	public string UnknownDetailColor = "C";

	public string AlternateRenderString = "*";

	public string UnknownRenderString = "*";

	public int EpistemicStatus = -1;

	[NonSerialized]
	[GameBasedStaticCache(CreateInstance = false)]
	private static Dictionary<string, string> _MedTable;

	[NonSerialized]
	[GameBasedStaticCache(CreateInstance = false)]
	public static Dictionary<string, int> UnderstandingTable;

	[NonSerialized]
	public static readonly string[] MedNames = new string[10] { "milky,&Y", "smokey,&K", "rosey,&R", "turquoise,&C", "cobalt,&b", "mossy,&g", "gold-flecked,&W", "muddy,&w", "violet,&m", "platinum,&y" };

	[NonSerialized]
	[GameBasedStaticCache(CreateInstance = false)]
	private static List<string> _MedNamesRemaining;

	public int Understanding
	{
		get
		{
			if (UnderstandingTable == null)
			{
				Loading.LoadTask("Loading medication names", Reset);
			}
			string tinkeringBlueprint = ParentObject.GetTinkeringBlueprint();
			if (!UnderstandingTable.ContainsKey(tinkeringBlueprint))
			{
				return 0;
			}
			return UnderstandingTable[tinkeringBlueprint];
		}
		set
		{
			if (UnderstandingTable == null)
			{
				Loading.LoadTask("Loading medication names", Reset);
			}
			string tinkeringBlueprint = ParentObject.GetTinkeringBlueprint();
			if (UnderstandingTable.ContainsKey(tinkeringBlueprint))
			{
				if (UnderstandingTable[tinkeringBlueprint] < value)
				{
					UnderstandingTable[tinkeringBlueprint] = value;
				}
			}
			else
			{
				UnderstandingTable.Add(tinkeringBlueprint, value);
			}
		}
	}

	public static void LoadGlobals(SerializationReader Reader)
	{
		_MedNamesRemaining = Reader.ReadStringList();
		_MedTable = Reader.ReadDictionary<string, string>();
		UnderstandingTable = Reader.ReadDictionary<string, int>();
	}

	public static void SaveGlobals(SerializationWriter Writer)
	{
		Writer.Write(_MedNamesRemaining);
		Writer.Write(_MedTable);
		Writer.Write(UnderstandingTable);
	}

	public static void Reset()
	{
		UnderstandingTable = new Dictionary<string, int>();
		_MedTable = new Dictionary<string, string>();
		_MedNamesRemaining = new List<string>(MedNames);
		List<IMedNamesExtension> instancesWithAttribute = ModManager.GetInstancesWithAttribute<IMedNamesExtension>(typeof(MedNamesExtension));
		instancesWithAttribute.Sort((IMedNamesExtension a, IMedNamesExtension b) => a.Priority().CompareTo(b.Priority()));
		for (int i = 0; i < instancesWithAttribute.Count; i++)
		{
			instancesWithAttribute[i].OnInitializeMedNames(_MedNamesRemaining);
		}
	}

	public override bool SameAs(IPart p)
	{
		Examiner examiner = p as Examiner;
		if (examiner.Complexity != Complexity)
		{
			return false;
		}
		if (examiner.Difficulty != Difficulty)
		{
			return false;
		}
		if (examiner.AlternateDisplayName != AlternateDisplayName)
		{
			return false;
		}
		if (examiner.UnknownDisplayName != UnknownDisplayName)
		{
			return false;
		}
		if (examiner.AlternateDescription != AlternateDescription)
		{
			return false;
		}
		if (examiner.UnknownDescription != UnknownDescription)
		{
			return false;
		}
		if (examiner.AlternateTile != AlternateTile)
		{
			return false;
		}
		if (examiner.UnknownTile != UnknownTile)
		{
			return false;
		}
		if (examiner.AlternateGender != AlternateGender)
		{
			return false;
		}
		if (examiner.UnknownGender != UnknownGender)
		{
			return false;
		}
		if (examiner.AlternateColorString != AlternateColorString)
		{
			return false;
		}
		if (examiner.UnknownColorString != UnknownColorString)
		{
			return false;
		}
		if (examiner.AlternateTileColor != AlternateTileColor)
		{
			return false;
		}
		if (examiner.UnknownTileColor != UnknownTileColor)
		{
			return false;
		}
		if (examiner.AlternateDetailColor != AlternateDetailColor)
		{
			return false;
		}
		if (examiner.UnknownDetailColor != UnknownDetailColor)
		{
			return false;
		}
		if (examiner.AlternateRenderString != AlternateRenderString)
		{
			return false;
		}
		if (examiner.UnknownRenderString != UnknownRenderString)
		{
			return false;
		}
		if (examiner.EpistemicStatus != EpistemicStatus)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public void CopyFrom(Examiner source)
	{
		Complexity = source.Complexity;
		Difficulty = source.Difficulty;
		AlternateDisplayName = source.AlternateDisplayName;
		UnknownDisplayName = source.UnknownDisplayName;
		AlternateDescription = source.AlternateDescription;
		UnknownDescription = source.UnknownDescription;
		AlternateTile = source.AlternateTile;
		UnknownTile = source.UnknownTile;
		AlternateGender = source.AlternateGender;
		UnknownGender = source.UnknownGender;
		AlternateColorString = source.AlternateColorString;
		UnknownColorString = source.UnknownColorString;
		AlternateTileColor = source.AlternateTileColor;
		UnknownTileColor = source.UnknownTileColor;
		AlternateDetailColor = source.AlternateDetailColor;
		UnknownDetailColor = source.UnknownDetailColor;
		AlternateRenderString = source.AlternateRenderString;
		UnknownRenderString = source.UnknownRenderString;
		EpistemicStatus = source.EpistemicStatus;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != AfterObjectCreatedEvent.ID && ID != AnimateEvent.ID && ID != BeforeObjectCreatedEvent.ID && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEarlyEvent.ID && ID != EnteredCellEvent.ID && ID != GetDebugInternalsEvent.ID && ID != GetDisplayNameEvent.ID && ID != GetGenderEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetInventoryCategoryEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Complexity", Complexity);
		E.AddEntry(this, "Difficulty", Difficulty);
		E.AddEntry(this, "AlternateDisplayName", AlternateDisplayName);
		E.AddEntry(this, "UnknownDisplayName", UnknownDisplayName);
		E.AddEntry(this, "AlternateDescription", AlternateDescription);
		E.AddEntry(this, "UnknownDescription", UnknownDescription);
		E.AddEntry(this, "AlternateTile", AlternateTile);
		E.AddEntry(this, "UnknownTile", UnknownTile);
		E.AddEntry(this, "AlternateGender", AlternateGender);
		E.AddEntry(this, "UnknownGender", UnknownGender);
		E.AddEntry(this, "AlternateColorString", AlternateColorString);
		E.AddEntry(this, "UnknownColorString", UnknownColorString);
		E.AddEntry(this, "AlternateTileColor", AlternateTileColor);
		E.AddEntry(this, "UnknownTileColor", UnknownTileColor);
		E.AddEntry(this, "AlternateDetailColor", AlternateDetailColor);
		E.AddEntry(this, "UnknownDetailColor", UnknownDetailColor);
		E.AddEntry(this, "AlternateRenderString", AlternateRenderString);
		E.AddEntry(this, "UnknownRenderString", UnknownRenderString);
		E.AddEntry(this, "EpistemicStatus", EpistemicStatus);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.AsIfKnown && CheckEpistemicStatus() != 2)
		{
			if (E.DB.PrimaryBase != null)
			{
				E.DB.Remove(E.DB.PrimaryBase);
			}
			E.AddBase((GetEpistemicStatus() == 0) ? UnknownDisplayName : AlternateDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		if (E.Object == ParentObject && GetEpistemicStatus() != 2)
		{
			SetEpistemicStatus(2);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		if (GetEpistemicStatus() != 2)
		{
			GameObject gameObject = ParentObject.Equipped ?? ParentObject.InInventory;
			if (gameObject != null && gameObject.IsPlayer())
			{
				E.AdjustValue((GetEpistemicStatus() == 1) ? 0.2 : 0.1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetGenderEvent E)
	{
		if (!E.AsIfKnown)
		{
			switch (GetEpistemicStatus())
			{
			case 0:
				if (!string.IsNullOrEmpty(UnknownGender))
				{
					E.Name = UnknownGender;
				}
				break;
			case 1:
				if (!string.IsNullOrEmpty(AlternateGender))
				{
					E.Name = AlternateGender;
				}
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!ParentObject.Understood(this))
		{
			E.AddAction("Examine", "examine", "Examine", null, 'x');
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Examine")
		{
			if (IsBroken())
			{
				Popup.ShowFail("Whatever " + ParentObject.it + ParentObject.Is + ", " + ParentObject.itis + " broken...");
				return false;
			}
			if (!E.Actor.CanMoveExtremities("Examine", ShowMessage: true))
			{
				return false;
			}
			bool Interrupt = false;
			int num = E.Actor.Stat("Intelligence") - Difficulty;
			int totalConfusion = E.Actor.GetTotalConfusion();
			if (totalConfusion > 0)
			{
				num -= totalConfusion;
			}
			int intProperty = E.Actor.GetIntProperty("InspectorEquipped");
			intProperty = GetTinkeringBonusEvent.GetFor(E.Actor, ParentObject, "Inspect", num, intProperty, ref Interrupt);
			if (Interrupt)
			{
				return false;
			}
			num += intProperty;
			if (E.Actor.IsPlayer())
			{
				if (!string.IsNullOrEmpty(ParentObject.Owner) && !ParentObject.HasPropertyOrTag("DontWarnOnExamine") && Popup.ShowYesNoCancel(ParentObject.Does("are") + " not owned by you, and examining " + ParentObject.them + " risks damaging " + ParentObject.them + ". Are you sure you want to do so?") != 0)
				{
					return false;
				}
				GameObject inInventory = ParentObject.InInventory;
				if (inInventory != null && inInventory != E.Actor && !string.IsNullOrEmpty(inInventory.Owner) && inInventory.Owner != ParentObject.Owner && !inInventory.HasPropertyOrTag("DontWarnOnExamine") && Popup.ShowYesNoCancel(inInventory.Does("are") + " not owned by you, and examining " + ParentObject.a + ParentObject.DisplayNameOnly + " inside " + inInventory.them + " risks causing damage. Are you sure you want to do so?") != 0)
				{
					return false;
				}
			}
			if (Options.SifrahExamine && E.Actor.IsPlayer())
			{
				if (totalConfusion > 0)
				{
					Popup.ShowFail("You're too confused to do that.");
				}
				else
				{
					ExamineSifrah examineSifrah = new ExamineSifrah(ParentObject, Complexity, Difficulty, Understanding, num);
					examineSifrah.Play(ParentObject);
					if (examineSifrah.InterfaceExitRequested)
					{
						E.RequestInterfaceExit();
					}
				}
			}
			else
			{
				int num2 = Stat.RollResult(num);
				if (num2 >= 10 || num2 > Complexity)
				{
					num2 = Complexity;
				}
				if (num2 <= 0 && !ParentObject.HasPropertyOrTag("CantBreakOnExamine"))
				{
					ResultCriticalFailure(E.Actor);
					E.RequestInterfaceExit();
					if (E.Actor.IsPlayer())
					{
						AutoAct.Interrupt();
					}
				}
				else if (totalConfusion > 0)
				{
					ResultFakeConfusionFailure(E.Actor);
					E.RequestInterfaceExit();
					if (E.Actor.IsPlayer())
					{
						AutoAct.Interrupt();
					}
				}
				else if (num2 > 0 && num2 > Understanding)
				{
					ResultPartialSuccess(E.Actor, num2);
				}
				else
				{
					ResultFailure(E.Actor);
				}
			}
			E.Actor.UseEnergy(1000, "Examine");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryCategoryEvent E)
	{
		if (!ParentObject.Understood(this))
		{
			E.Category = "Artifacts";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer() && !ParentObject.Understood(this))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEarlyEvent E)
	{
		if (E.Actor.IsPlayer() && CheckEpistemicStatus() != 2)
		{
			ParentObject.Twiddle();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		if (AlternateDisplayName == "*med" && !ParentObject.HasTag("BaseObject"))
		{
			if (_MedNamesRemaining == null)
			{
				Loading.LoadTask("Loading medication names", Reset);
			}
			if (!_MedTable.TryGetValue(ParentObject.Blueprint, out var value))
			{
				if (_MedNamesRemaining.Count == 0)
				{
					MetricsManager.LogError("No medication names remaining, reusing defaults.");
					value = MedNames.GetRandomElementCosmetic();
					_MedTable[ParentObject.Blueprint] = value;
				}
				else
				{
					int index = Stat.RandomCosmetic(0, _MedNamesRemaining.Count - 1);
					value = _MedNamesRemaining[index];
					_MedTable[ParentObject.Blueprint] = value;
					_MedNamesRemaining.RemoveAt(index);
				}
			}
			string[] array = value.Split(',');
			ParentObject.pRender.ColorString = array[1];
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("small {{").Append(array[1]).Append("|")
				.Append(array[0])
				.Append("}} tube");
			UnknownDisplayName = (AlternateDisplayName = stringBuilder.ToString());
			UnknownColorString = (AlternateColorString = array[1]);
			UnknownTileColor = (AlternateTileColor = array[1]);
			UnknownDetailColor = (AlternateDetailColor = "m");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckEpistemicStatus();
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		switch (GetEpistemicStatus())
		{
		case 1:
			RenderPartial(E);
			break;
		case 0:
			RenderUnknown(E);
			break;
		}
		return base.Render(E);
	}

	public override bool OverlayRender(RenderEvent E)
	{
		switch (GetEpistemicStatus())
		{
		case 1:
			RenderPartial(E, Color: false);
			break;
		case 0:
			RenderUnknown(E, Color: false);
			break;
		}
		return true;
	}

	public void RenderPartial(RenderEvent E, bool Color = true)
	{
		string text = AlternateTile ?? UnknownTile;
		if (!string.IsNullOrEmpty(text))
		{
			E.Tile = text;
		}
		string text2 = AlternateRenderString ?? UnknownRenderString;
		if (!string.IsNullOrEmpty(text2))
		{
			E.RenderString = text2;
		}
		if (!Color)
		{
			return;
		}
		string text3 = AlternateTileColor ?? UnknownTileColor;
		if (!string.IsNullOrEmpty(text3) && Options.UseTiles)
		{
			E.ColorString = text3;
		}
		else
		{
			string text4 = AlternateColorString ?? UnknownColorString;
			if (!string.IsNullOrEmpty(text4))
			{
				E.ColorString = text4;
			}
		}
		string text5 = AlternateDetailColor ?? UnknownDetailColor;
		if (!string.IsNullOrEmpty(text5))
		{
			E.DetailColor = text5;
		}
	}

	public void RenderUnknown(RenderEvent E, bool Color = true)
	{
		if (!string.IsNullOrEmpty(UnknownTile))
		{
			E.Tile = UnknownTile;
		}
		if (!string.IsNullOrEmpty(UnknownRenderString))
		{
			E.RenderString = UnknownRenderString;
		}
		if (Color)
		{
			if (!string.IsNullOrEmpty(UnknownTileColor) && Options.UseTiles)
			{
				E.ColorString = UnknownTileColor;
			}
			else if (!string.IsNullOrEmpty(UnknownColorString))
			{
				E.ColorString = UnknownColorString;
			}
			if (!string.IsNullOrEmpty(UnknownDetailColor))
			{
				E.DetailColor = UnknownDetailColor;
			}
		}
	}

	public int GetAppropriateEpistemicStatus()
	{
		int understanding = Understanding;
		if (understanding >= Complexity)
		{
			return 2;
		}
		int scanEpistemicStatus = Scanning.GetScanEpistemicStatus(IComponent<GameObject>.ThePlayer, ParentObject);
		if (scanEpistemicStatus == 2 || scanEpistemicStatus == 1)
		{
			return scanEpistemicStatus;
		}
		if (understanding > 0)
		{
			return 1;
		}
		if (ParentObject.HasProperty("PartiallyUnderstood"))
		{
			return 1;
		}
		return 0;
	}

	public new int GetEpistemicStatus()
	{
		if (EpistemicStatus == -1)
		{
			SetEpistemicStatus(GetAppropriateEpistemicStatus());
		}
		return EpistemicStatus;
	}

	public void SetEpistemicStatus(int status)
	{
		EpistemicStatus = status;
	}

	public int CheckEpistemicStatus()
	{
		if (EpistemicStatus != 2)
		{
			SetEpistemicStatus(GetAppropriateEpistemicStatus());
		}
		return EpistemicStatus;
	}

	public bool MakeUnderstood()
	{
		if (Understanding >= Complexity)
		{
			return false;
		}
		ParentObject.Seen();
		Understanding = Complexity;
		CheckEpistemicStatus();
		return true;
	}

	public bool MakePartiallyUnderstood()
	{
		if (Understanding >= Complexity)
		{
			return false;
		}
		ParentObject.Seen();
		Understanding = Complexity - 1;
		CheckEpistemicStatus();
		return true;
	}

	public void ResultSuccess(GameObject who)
	{
		Understanding = Complexity;
		if (who.IsPlayer())
		{
			Popup.Show("You now understand " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false) + ".");
		}
		who.FireEvent("ExaminedCompleteSuccess");
		ParentObject.FireEvent("ExamineSuccess");
		CheckEpistemicStatus();
	}

	public void ResultExceptionalSuccess(GameObject who)
	{
		string text = ParentObject.t();
		bool flag = false;
		if (20.in100())
		{
			if (RelicGenerator.ApplyBasicBestowal(ParentObject))
			{
				flag = true;
			}
			else if (ModificationFactory.ApplyModifications(ParentObject, ParentObject.GetBlueprint(), -999, 1, "Examination") > 0)
			{
				flag = true;
			}
		}
		else if (ModificationFactory.ApplyModifications(ParentObject, ParentObject.GetBlueprint(), -999, 1, "Examination") > 0)
		{
			flag = true;
		}
		else if (50.in100() && RelicGenerator.ApplyBasicBestowal(ParentObject))
		{
			flag = true;
		}
		if (flag && who.IsPlayer())
		{
			Popup.Show("You discover something about " + text + " that was hidden!");
		}
		ResultSuccess(ParentObject);
	}

	public void ResultPartialSuccess(GameObject who, int newLevel = -1)
	{
		if (newLevel == -1)
		{
			newLevel = Math.Min(Stat.Random(1, Complexity - 1), Complexity - 1);
		}
		int num = Convert.ToInt32(ParentObject.GetBlueprint().GetPartParameter("Examiner", "Complexity", "1"));
		bool flag = false;
		if (newLevel < Complexity && newLevel >= num && Understanding < num)
		{
			GameObject gameObject = GameObject.createSample(ParentObject.GetTinkeringBlueprint());
			if (gameObject.GetPart("Examiner") is Examiner examiner && examiner.Complexity > 0 && !gameObject.DisplayNameOnlyDirect.StartsWith("[") && !gameObject.HasTag("BaseObject"))
			{
				if (newLevel > Understanding)
				{
					Understanding = newLevel;
				}
				flag = true;
				if (who.IsPlayer())
				{
					Popup.Show("You make some progress understanding " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false) + ". You think " + ParentObject.itis + " probably a variety of " + gameObject.BaseDisplayName + ", and you believe you would be able to recognize an ordinary " + (gameObject.IsPlural ? "set" : "one") + " of those now.");
				}
			}
		}
		if (!flag)
		{
			if (newLevel > Understanding)
			{
				Understanding = newLevel;
			}
			if (who.IsPlayer())
			{
				Popup.Show("You make some progress understanding " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false) + ".");
			}
		}
		if (Understanding >= Complexity)
		{
			who.FireEvent("ExaminedCompleteSuccess");
			ParentObject.FireEvent("ExamineSuccess");
			CheckEpistemicStatus();
		}
		else
		{
			who.FireEvent("ExaminedPartialSuccess");
			ParentObject.FireEvent("ExaminePartialSuccess");
		}
	}

	public void ResultFailure(GameObject who)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You are puzzled by " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutEpithet: false, Short: false) + ".");
		}
		who.FireEvent("ExaminedFailure");
		ParentObject.FireEvent("ExamineFailure");
	}

	public void ResultFakeConfusionFailure(GameObject who)
	{
		if (who.IsPlayer())
		{
			Popup.Show("You think you broke " + ParentObject.them + "...");
		}
		who.FireEvent("ExaminedConfusionFailure");
		ParentObject.FireEvent("ExamineConfusionFailure");
	}

	public void ResultCriticalFailure(GameObject who)
	{
		if (!ExamineCriticalFailureEvent.Check(who, ParentObject))
		{
			return;
		}
		string message = "You think you broke " + ParentObject.them + "...";
		if (ParentObject.ApplyEffect(new Broken(FromDamage: false, FromExamine: true)) && ParentObject.IsBroken())
		{
			if (who.IsPlayer())
			{
				Popup.Show(message);
			}
			if (ParentObject.IsValid())
			{
				ParentObject.PotentiallyAngerOwner(who, "DontWarnOnExamine");
			}
		}
		else
		{
			ResultFailure(ParentObject);
		}
	}

	[WishCommand(null, null)]
	public static void IDAllHere()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakeUnderstood();
		}
	}

	[WishCommand(null, null)]
	public static void IDAll()
	{
		foreach (GameObject item in GetContentsEvent.GetFor(IComponent<GameObject>.ThePlayer.CurrentZone))
		{
			item.MakeUnderstood();
		}
		foreach (GameObjectBlueprint value in GameObjectFactory.Factory.Blueprints.Values)
		{
			if (value.HasPart("Examiner") && value.GetPartParameter("TinkerItem", "SubstituteBlueprint") == null)
			{
				UnderstandingTable[value.Name] = Convert.ToInt32(value.GetPartParameter("Examiner", "Complexity", "0")) + 10;
			}
		}
	}
}
