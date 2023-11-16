using System;

namespace XRL.World.Parts;

[Serializable]
public class ActivatedAbilityEntry : IEquatable<ActivatedAbilityEntry>
{
	public Guid ID;

	public string DisplayName;

	public string Command;

	public string Class;

	public string Description;

	public string Icon;

	public string DisabledMessage;

	public bool Toggleable;

	public bool ToggleState;

	public bool ActiveToggle;

	public bool Enabled = true;

	public bool IsAttack;

	public bool IsRealityDistortionBased;

	public bool AIDisable;

	public bool AlwaysAllowToggleOff = true;

	public bool Visible;

	public bool AffectedByWillpower = true;

	public bool TickPerTurn;

	[NonSerialized]
	public GameObject ParentObject;

	[NonSerialized]
	private string _TrackingPropertyName;

	public int _Cooldown = -1;

	public string TrackingPropertyName
	{
		get
		{
			if (_TrackingPropertyName == null)
			{
				_TrackingPropertyName = "ActivatedAbilityCooldown" + GetBaseHashCode();
			}
			return _TrackingPropertyName;
		}
	}

	public int Cooldown
	{
		get
		{
			if (!AlwaysAllowToggleOff || !ToggleState || !Toggleable)
			{
				return _Cooldown;
			}
			return 0;
		}
		set
		{
			if (Toggleable && !AlwaysAllowToggleOff)
			{
				AlwaysAllowToggleOff = true;
			}
			if (The.Core.cool && ParentObject != null && ParentObject.IsPlayer())
			{
				SetCooldown(0);
				return;
			}
			if (value == Cooldown + 1 || value == Cooldown - 1 || value <= 0)
			{
				SetCooldown(value);
				return;
			}
			int percentageReduction = 0;
			if (ParentObject != null && AffectedByWillpower && ParentObject.HasStat("Willpower"))
			{
				percentageReduction = (ParentObject.Stat("Willpower") - 16) * 5;
			}
			SetCooldown(GetCooldownEvent.GetFor(ParentObject, this, value, percentageReduction));
		}
	}

	public int CooldownTurns => (int)Math.Ceiling((double)Cooldown / 10.0);

	public string CooldownDescription => CooldownTurns.Things("round", "rounds");

	public ActivatedAbilityEntry()
	{
	}

	public ActivatedAbilityEntry(ActivatedAbilityEntry Source)
		: this()
	{
		ID = Source.ID;
		DisplayName = Source.DisplayName;
		Command = Source.Command;
		Class = Source.Class;
		Description = Source.Description;
		Icon = Source.Icon;
		DisabledMessage = Source.DisabledMessage;
		Toggleable = Source.Toggleable;
		ToggleState = Source.ToggleState;
		ActiveToggle = Source.ActiveToggle;
		Enabled = Source.Enabled;
		IsAttack = Source.IsAttack;
		IsRealityDistortionBased = Source.IsRealityDistortionBased;
		AIDisable = Source.AIDisable;
		AlwaysAllowToggleOff = Source.AlwaysAllowToggleOff;
		Visible = Source.Visible;
		AffectedByWillpower = Source.AffectedByWillpower;
		TickPerTurn = Source.TickPerTurn;
		Cooldown = Source.Cooldown;
	}

	public bool Equals(ActivatedAbilityEntry Entry)
	{
		if (Entry != null && ID == Entry.ID && DisplayName == Entry.DisplayName && Command == Entry.Command && Class == Entry.Class && Description == Entry.Description && Icon == Entry.Icon && DisabledMessage == Entry.DisabledMessage && Toggleable == Entry.Toggleable && ToggleState == Entry.ToggleState && ActiveToggle == Entry.ActiveToggle && IsAttack == Entry.IsAttack && IsRealityDistortionBased == Entry.IsRealityDistortionBased && AlwaysAllowToggleOff == Entry.AlwaysAllowToggleOff && Visible == Entry.Visible && AffectedByWillpower == Entry.AffectedByWillpower && TickPerTurn == Entry.TickPerTurn)
		{
			return Cooldown == Entry.Cooldown;
		}
		return false;
	}

	public override bool Equals(object Object)
	{
		return Equals(Object as ActivatedAbilityEntry);
	}

	public override int GetHashCode()
	{
		return new
		{
			ID, DisplayName, Command, Class, Description, Icon, DisabledMessage, Toggleable, ActiveToggle, IsAttack,
			IsRealityDistortionBased, AlwaysAllowToggleOff, Visible, AffectedByWillpower, TickPerTurn
		}.GetHashCode();
	}

	public int GetBaseHashCode()
	{
		return new { Command, Class, Toggleable, ActiveToggle, IsAttack, IsRealityDistortionBased, AlwaysAllowToggleOff, Visible, AffectedByWillpower, TickPerTurn }.GetHashCode();
	}

	public void SetCooldown(int n)
	{
		ParentObject?.SetIntProperty(TrackingPropertyName, n);
		_Cooldown = n;
	}

	public void TickDown()
	{
		_Cooldown--;
		ParentObject?.SetIntProperty(TrackingPropertyName, _Cooldown);
	}

	public override string ToString()
	{
		string text = DisplayName + " (" + Command + ") " + Class + " [" + ID.ToString() + "]";
		if (!Enabled)
		{
			text += " {{K|[disabled]}}";
		}
		return text;
	}
}
