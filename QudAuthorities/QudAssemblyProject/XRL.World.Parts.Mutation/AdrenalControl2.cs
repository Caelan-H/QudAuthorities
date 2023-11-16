using System;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class AdrenalControl2 : BaseMutation
{
	public int CurrentBonus;

	public Guid ReleaseAdrenalineAbilityID = Guid.Empty;

	public int Duration;

	public int CurrentMLBonus;

	public AdrenalControl2()
	{
		DisplayName = "Adrenal Control";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandReleaseAdrenaline");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You regulate your body's release of adrenaline.";
	}

	public int GetQuicknessBonus(int Level)
	{
		return 9 + Level;
	}

	public int GetMutationBonus(int Level)
	{
		return Level / 3 + 1;
	}

	public int GetQuicknessDuration(int Level)
	{
		return 20;
	}

	public int GetCooldown(int Level)
	{
		return 200;
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "You can increase your body's adrenaline flow for " + GetQuicknessDuration(Level) + " rounds.\n";
		text = text + "While it's flowing, you gain +{{C|" + GetQuicknessBonus(Level) + "}} quickness and other physical mutations gain +{{C|" + GetMutationBonus(Level) + "}} rank.\n";
		return text + "Cooldown: " + GetCooldown(Level) + " rounds";
	}

	public void SyncBonus()
	{
		int num = GetQuicknessBonus(base.Level);
		if (Duration <= 0)
		{
			num = 0;
		}
		if (ParentObject.HasStat("Speed") && num != CurrentBonus)
		{
			ParentObject.GetStat("Speed").Bonus -= CurrentBonus;
			CurrentBonus = num;
			ParentObject.GetStat("Speed").Bonus += CurrentBonus;
		}
		int num2 = GetMutationBonus(base.Level);
		if (Duration <= 0)
		{
			num2 = 0;
		}
		if (num2 != CurrentMLBonus)
		{
			ParentObject.ModIntProperty("AdrenalLevelModifier", -CurrentMLBonus);
			CurrentMLBonus = num2;
			ParentObject.ModIntProperty("AdrenalLevelModifier", CurrentMLBonus);
			ParentObject.FireEvent("SyncMutationLevels");
		}
		if (num2 == 0 || num == 0)
		{
			ParentObject.RemoveEffect(typeof(AdrenalControl2Boosted));
			return;
		}
		AdrenalControl2Boosted adrenalControl2Boosted = ParentObject.GetEffect<AdrenalControl2Boosted>();
		if (adrenalControl2Boosted == null)
		{
			adrenalControl2Boosted = new AdrenalControl2Boosted(this);
			ParentObject.ApplyEffect(adrenalControl2Boosted);
		}
		adrenalControl2Boosted.AppliedMutationBonus = num2;
		adrenalControl2Boosted.AppliedSpeedBonus = num;
		adrenalControl2Boosted.Duration = Duration;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = 60 - Duration / 2;
			int num2 = XRLCore.CurrentFrame % num;
			if (num2 > 21 && num2 < 31)
			{
				E.Tile = null;
				E.RenderString = "\u0003";
				E.ColorString = "&r";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Duration > 0)
			{
				Duration--;
				if (Duration == 0 && ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your adrenaline subsides.");
				}
			}
			SyncBonus();
		}
		else if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") <= 4 && IsMyActivatedAbilityAIUsable(ReleaseAdrenalineAbilityID))
			{
				E.AddAICommand("CommandReleaseAdrenaline");
			}
		}
		else if (E.ID == "CommandReleaseAdrenaline")
		{
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your adrenaline starts to flow.");
			}
			Duration = GetQuicknessDuration(base.Level);
			CooldownMyActivatedAbility(ReleaseAdrenalineAbilityID, GetCooldown(base.Level));
			SyncBonus();
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		SyncBonus();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ReleaseAdrenalineAbilityID = AddMyActivatedAbility("Release Adrenaline", "CommandReleaseAdrenaline", "Physical Mutation", null, "å", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		Duration = 0;
		SyncBonus();
		RemoveMyActivatedAbility(ref ReleaseAdrenalineAbilityID);
		return base.Unmutate(GO);
	}
}
