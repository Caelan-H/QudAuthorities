using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MassMind : BaseMutation
{
	public new Guid ActivatedAbilityID = Guid.Empty;

	public int nDuration;

	public MassMind()
	{
		DisplayName = "Mass Mind";
		Type = "Mental";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandMassMind");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override string GetDescription()
	{
		return "You tap into the aggregate mind and steal power from other espers.";
	}

	public int GetCooldown(int Level)
	{
		return Math.Max(100, 550 - 50 * Level);
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text += "Refreshes all mental mutations\n";
		text = text + "Cooldown: {{rules|" + GetCooldown(Level) + "}} rounds\n";
		text += "Cooldown is not affected by Willpower.\n";
		text += "Each use attracts slightly more attention from psychic interlopers.\n";
		text = ((Level != base.Level) ? (text + "{{rules|Decreased chance for another esper to steal your powers}}\n") : (text + "{{rules|Small chance each round for another esper to steal your powers}}\n"));
		return text + "-200 reputation with {{w|the Seekers of the Sightless Way}}";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (nDuration > 0)
			{
				nDuration--;
			}
			else if ((double)Stat.Random(1, 10000) < (0.13 - 0.005 * (double)ParentObject.StatMod("Willpower") - 0.0065 * (double)base.Level) * 100.0 && ParentObject.IsPlayer() && ParentObject.CurrentCell != null && !ParentObject.OnWorldMap())
			{
				List<ActivatedAbilityEntry> abilityListByClass = base.MyActivatedAbilities.GetAbilityListByClass("Mental Mutation");
				if (abilityListByClass != null && abilityListByClass.Count > 0)
				{
					if (ParentObject.HasEffect("FungalVisionary"))
					{
						IComponent<GameObject>.AddPlayerMessage("You feel a small ripple in space and time.");
					}
					else
					{
						nDuration = "8d10".RollCached();
						foreach (ActivatedAbilityEntry item in abilityListByClass)
						{
							if (item.Enabled)
							{
								item.Cooldown += nDuration * 10;
							}
						}
						IComponent<GameObject>.AddPlayerMessage("{{R|Someone reaches through the aggregate mind and exhausts your power!}}");
					}
				}
			}
		}
		else if (E.ID == "CommandMassMind")
		{
			if (ParentObject.HasEffect("FungalVisionary"))
			{
				Popup.Show("Too far! The aggregate mind is stretched to gossamers, and even the closest mind is too far away.");
			}
			else
			{
				MessageQueue.AddPlayerMessage("{{G|You innervate your mind at someone's expense.}}");
				List<ActivatedAbilityEntry> abilityListByClass2 = base.MyActivatedAbilities.GetAbilityListByClass("Mental Mutation");
				if (abilityListByClass2 != null)
				{
					foreach (ActivatedAbilityEntry item2 in abilityListByClass2)
					{
						if (item2.Cooldown > 0 && item2.ID != ActivatedAbilityID)
						{
							item2.Cooldown = 0;
						}
					}
				}
				int cooldown = GetCooldown(base.Level);
				CooldownMyActivatedAbility(ActivatedAbilityID, cooldown);
				ParentObject.ModIntProperty("GlimmerModifier", 1);
				ParentObject.SyncMutationLevelAndGlimmer();
				UseEnergy(1000, "Mental Mutation Mass Mind");
				ParentObject.FireEvent("AfterMassMind");
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Tap the Mass Mind", "CommandMassMind", "Mental Mutation", null, "!", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: false);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
