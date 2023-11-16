using System;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Spinnerets : BaseMutation
{
	public const string SAVE_BONUS_VS = "Move";

	public new Guid ActivatedAbilityID;

	public int SpinTimer;

	public bool Phase;

	[FieldSaveVersion(249)]
	public bool Active;

	public Spinnerets()
	{
		DisplayName = "Spinnerets";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != LeftCellEvent.ID)
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			if (ParentObject.OnWorldMap())
			{
				SpinTimer = -1;
			}
			else
			{
				SpinTimer--;
			}
			if (SpinTimer < 0)
			{
				ToggleMyActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				GameObject gameObject;
				if (!Phase && ParentObject.GetPhase() != 2)
				{
					gameObject = GameObject.create("Web");
					if (gameObject.GetPart("Sticky") is Sticky sticky)
					{
						sticky.SaveTarget = 14 + 2 * base.Level;
						sticky.MaxWeight = 120 + 80 * base.Level;
					}
				}
				else
				{
					gameObject = GameObject.create("PhaseWeb");
					gameObject.ApplyEffect(new Phased());
					if (gameObject.GetPart("PhaseSticky") is PhaseSticky phaseSticky)
					{
						phaseSticky.SaveTarget = 18 + 2 * base.Level;
						phaseSticky.MaxWeight = 520 + 80 * base.Level;
					}
				}
				E.Cell.AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (SavingThrows.Applicable("Move", E))
		{
			E.Roll += GetMoveSaveModifier();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetMovementMutationList");
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "ApplyStuck");
		Object.RegisterPartEvent(this, "CommandSpinWeb");
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You can spin sticky silk webs.";
	}

	public int GetMoveSaveModifier()
	{
		return 5 + base.Level;
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Compound("While spinning, you leave webs in your wake as you move.", '\n');
		if (Level != base.Level)
		{
			stringBuilder.Compound("{{rules|Increased web strength}}", '\n');
		}
		stringBuilder.Compound("Duration: {{rules|", '\n').Append(5 + Level).Append("}} move actions");
		SavingThrows.AppendSaveBonusDescription(stringBuilder, GetMoveSaveModifier(), "Move", HighlightNumber: true);
		stringBuilder.Compound("Cooldown: 80 rounds", '\n');
		stringBuilder.Compound("You don't get stuck in other creatures' webs.", '\n');
		stringBuilder.Compound("+300 reputation with {{w|arachnids}}", '\n');
		return stringBuilder.ToString();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck")
		{
			return false;
		}
		if (E.ID == "AIGetMovementMutationList" && Active)
		{
			if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSpinWeb");
			}
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandSpinWeb");
			}
		}
		else if (E.ID == "CommandSpinWeb")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
				return false;
			}
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				CooldownMyActivatedAbility(ActivatedAbilityID, 80);
				SpinTimer = 5 + base.Level;
			}
		}
		else if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spin Webs", "CommandSpinWeb", "Physical Mutation", null, "#", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: true);
		if (Phase)
		{
			GO.ApplyEffect(new Phased(9999));
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
