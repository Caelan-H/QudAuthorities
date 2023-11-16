using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ActivatedAbilities : IPart
{
	public const string DEFAULT_ICON = "\a";

	[NonSerialized]
	public Dictionary<Guid, ActivatedAbilityEntry> AbilityByGuid;

	[NonSerialized]
	public Dictionary<string, List<ActivatedAbilityEntry>> AbilityLists;

	[NonSerialized]
	public bool Silent;

	public static int MinimumValueForCooldown(int Cooldown)
	{
		return Math.Max((int)Math.Round((double)Cooldown * 0.2, MidpointRounding.AwayFromZero), Math.Min(60, Cooldown));
	}

	public int GetAbilityCount()
	{
		if (AbilityByGuid != null)
		{
			return AbilityByGuid.Count;
		}
		return 0;
	}

	public ActivatedAbilityEntry GetAbilityByCommand(string command)
	{
		if (AbilityByGuid != null)
		{
			foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
			{
				if (item.Value.Command == command)
				{
					return item.Value;
				}
			}
		}
		return null;
	}

	public ActivatedAbilityEntry GetAbility(Guid ID)
	{
		if (AbilityByGuid != null && AbilityByGuid.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public override void SaveData(SerializationWriter Writer)
	{
		base.SaveData(Writer);
		if (AbilityByGuid == null)
		{
			Writer.Write(0);
			return;
		}
		Writer.Write(AbilityByGuid.Count);
		foreach (ActivatedAbilityEntry value in AbilityByGuid.Values)
		{
			Writer.WriteObject(value);
		}
	}

	public override void LoadData(SerializationReader Reader)
	{
		base.LoadData(Reader);
		int num = Reader.ReadInt32();
		AbilityByGuid = null;
		AbilityLists = null;
		if (num >= 0)
		{
			AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>();
			AbilityLists = new Dictionary<string, List<ActivatedAbilityEntry>>();
		}
		for (int i = 0; i < num; i++)
		{
			ActivatedAbilityEntry activatedAbilityEntry = (ActivatedAbilityEntry)Reader.ReadObject();
			activatedAbilityEntry.ParentObject = ParentObject;
			AbilityByGuid.Add(activatedAbilityEntry.ID, activatedAbilityEntry);
			if (!AbilityLists.TryGetValue(activatedAbilityEntry.Class, out var value))
			{
				value = new List<ActivatedAbilityEntry>();
				AbilityLists.Add(activatedAbilityEntry.Class, value);
			}
			value.Add(activatedAbilityEntry);
		}
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		ActivatedAbilities activatedAbilities = new ActivatedAbilities();
		activatedAbilities._ParentObject = Parent;
		if (AbilityByGuid != null)
		{
			activatedAbilities.AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>();
			{
				foreach (Guid key in AbilityByGuid.Keys)
				{
					ActivatedAbilityEntry activatedAbilityEntry = new ActivatedAbilityEntry(AbilityByGuid[key]);
					activatedAbilityEntry.ParentObject = Parent;
					activatedAbilities.AbilityByGuid.Add(key, activatedAbilityEntry);
					if (activatedAbilities.AbilityLists == null)
					{
						activatedAbilities.AbilityLists = new Dictionary<string, List<ActivatedAbilityEntry>>();
					}
					if (!activatedAbilities.AbilityLists.TryGetValue(activatedAbilityEntry.Class, out var value))
					{
						value = new List<ActivatedAbilityEntry>();
						activatedAbilities.AbilityLists.Add(activatedAbilityEntry.Class, value);
					}
					value.Add(activatedAbilityEntry);
				}
				return activatedAbilities;
			}
		}
		return activatedAbilities;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeBeginTakeActionEvent.ID)
		{
			return ID == EndSegmentEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (AbilityByGuid != null)
		{
			foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
			{
				ActivatedAbilityEntry value = item.Value;
				if (value._Cooldown > 0 && value.TickPerTurn)
				{
					for (int i = 0; i < 10; i++)
					{
						value.TickDown();
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndSegmentEvent E)
	{
		if (AbilityByGuid != null)
		{
			foreach (KeyValuePair<Guid, ActivatedAbilityEntry> item in AbilityByGuid)
			{
				ActivatedAbilityEntry value = item.Value;
				if (value._Cooldown > 0 && !value.TickPerTurn)
				{
					value.TickDown();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public Guid AddAbility(string Name, string Command, string Class, string Description = null, string Icon = "\a", string DisabledMessage = null, bool Toggleable = false, bool DefaultToggleState = false, bool ActiveToggle = false, bool IsAttack = false, bool IsRealityDistortionBased = false, bool Silent = false, bool AIDisable = false, bool AlwaysAllowToggleOff = true, bool AffectedByWillpower = true, bool TickPerTurn = false, bool Distinct = false, int Cooldown = -1)
	{
		if (Distinct)
		{
			ActivatedAbilityEntry abilityByCommand = GetAbilityByCommand(Command);
			if (abilityByCommand != null)
			{
				return abilityByCommand.ID;
			}
		}
		Guid guid = Guid.NewGuid();
		if (AbilityByGuid == null)
		{
			AbilityByGuid = new Dictionary<Guid, ActivatedAbilityEntry>();
		}
		if (AbilityLists == null)
		{
			AbilityLists = new Dictionary<string, List<ActivatedAbilityEntry>>();
		}
		ActivatedAbilityEntry activatedAbilityEntry = new ActivatedAbilityEntry();
		activatedAbilityEntry.ParentObject = ParentObject;
		activatedAbilityEntry.ID = guid;
		activatedAbilityEntry.DisplayName = Name;
		activatedAbilityEntry.Command = Command;
		activatedAbilityEntry.Class = Class;
		activatedAbilityEntry.Description = Description;
		activatedAbilityEntry.Icon = Icon;
		activatedAbilityEntry.DisabledMessage = DisabledMessage;
		activatedAbilityEntry.Toggleable = Toggleable;
		activatedAbilityEntry.ToggleState = DefaultToggleState;
		activatedAbilityEntry.ActiveToggle = ActiveToggle;
		activatedAbilityEntry.IsAttack = IsAttack;
		activatedAbilityEntry.IsRealityDistortionBased = IsRealityDistortionBased;
		activatedAbilityEntry.AIDisable = AIDisable;
		activatedAbilityEntry.AlwaysAllowToggleOff = AlwaysAllowToggleOff;
		activatedAbilityEntry.AffectedByWillpower = AffectedByWillpower;
		activatedAbilityEntry.TickPerTurn = TickPerTurn;
		string trackingPropertyName = activatedAbilityEntry.TrackingPropertyName;
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.HasIntProperty(trackingPropertyName))
		{
			activatedAbilityEntry.SetCooldown(ParentObject.GetIntProperty(trackingPropertyName));
		}
		else if (Cooldown > 0)
		{
			activatedAbilityEntry.Cooldown = Cooldown;
		}
		AbilityByGuid.Add(guid, activatedAbilityEntry);
		if (!AbilityLists.TryGetValue(Class, out var value))
		{
			value = new List<ActivatedAbilityEntry>();
			AbilityLists.Add(Class, value);
		}
		value.Add(activatedAbilityEntry);
		if (!Silent && !this.Silent && ParentObject.IsPlayer())
		{
			AbilityManager.UpdateFavorites();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("You have gained the activated ability {{Y|").Append(Name).Append("}}.");
			if (ParentObject.GetIntProperty("HasAccessedAbilities") < 3 && LegacyKeyMapping.GetKeyFromCommand("CmdAbilities") == 65)
			{
				stringBuilder.Append("\n(press {{W|a}} to use activated abilities)");
			}
			Popup.Show(stringBuilder.ToString());
		}
		return guid;
	}

	public bool RemoveAbility(Guid ID)
	{
		if (AbilityByGuid.TryGetValue(ID, out var value))
		{
			AbilityByGuid.Remove(ID);
			if (value != null && AbilityLists.TryGetValue(value.Class, out var value2))
			{
				value2.Remove(value);
				if (value2.Count == 0)
				{
					AbilityLists.Remove(value.Class);
				}
			}
			return true;
		}
		return false;
	}

	public List<ActivatedAbilityEntry> GetAbilityListByClass(string Class)
	{
		if (AbilityLists != null && AbilityLists.TryGetValue(Class, out var value))
		{
			return value;
		}
		return null;
	}
}
