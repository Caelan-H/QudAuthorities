using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_Proselytize : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	[Obsolete("save compat")]
	public string TargetID;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	[Obsolete("save compat")]
	public override void Attach()
	{
		if (TargetID != null && ParentObject.pBrain != null)
		{
			ParentObject.pBrain.PartyMembers[TargetID] = 1;
			TargetID = null;
		}
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanCompanionRestorePartyLeader");
		Object.RegisterPartEvent(this, "CommandProselytize");
		Object.RegisterPartEvent(this, "GetMaxProselytized");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandProselytize")
		{
			AttemptProselytization();
		}
		else if (E.ID == "CanCompanionRestorePartyLeader")
		{
			if (ParentObject.SupportsFollower(E.GetGameObjectParameter("Companion")))
			{
				return false;
			}
		}
		else if (E.ID == "GetMaxProselytized")
		{
			E.ModParameter("Amount", 1);
		}
		return base.FireEvent(E);
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Proselytized proselytized)
		{
			return proselytized.Proselytizer == ParentObject;
		}
		return false;
	}

	public static void SyncTarget(GameObject Proselytizer, GameObject Target = null, bool Independent = false)
	{
		if (Proselytizer.pBrain == null)
		{
			return;
		}
		int num = GetMaxTargets(Proselytizer);
		if (Target == null)
		{
			num++;
		}
		Dictionary<string, int> partyMembers = Proselytizer.pBrain.PartyMembers;
		string[] array = (from x in partyMembers
			where x.Value.HasBit(1)
			orderby x.Value.HasBit(8388608) descending
			select x.Key).ToArray();
		int num2 = 0;
		for (int num3 = array.Length; num3 >= num; num3--)
		{
			partyMembers.Remove(array[num2]);
			num2++;
		}
		if (Target != null)
		{
			partyMembers[Target.id] = 1;
			if (Independent)
			{
				partyMembers[Target.id] |= 8388608;
			}
		}
	}

	public static int GetMaxTargets(GameObject Proselytizer)
	{
		int num = Proselytizer.GetIntProperty("MaxProselytizedBonus");
		if (Proselytizer.HasRegisteredEvent("GetMaxProselytized"))
		{
			Event @event = Event.New("GetMaxProselytized", "Amount", 0);
			Proselytizer.FireEvent(@event);
			num += @event.GetIntParameter("Amount");
		}
		return num;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Proselytize", "CommandProselytize", "Skill", null, "\u0003");
		if (GO.IsPlayer())
		{
			SocialSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		SyncTarget(GO);
		return base.RemoveSkill(GO);
	}

	public bool AttemptProselytization()
	{
		bool flag = ParentObject.IsMissingTongue();
		if (flag && !ParentObject.HasPart("Telepathy"))
		{
			if (ParentObject.IsPlayer())
			{
				Popup.ShowBlock("You cannot proselytize without a tongue.");
			}
			return false;
		}
		if (!ParentObject.CheckFrozen(Telepathic: true))
		{
			return false;
		}
		Cell cell = PickDirection();
		if (cell == null)
		{
			return false;
		}
		foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
		{
			if (item == ParentObject || !item.HasStat("Level"))
			{
				continue;
			}
			if (item.HasCopyRelationship(ParentObject) || item.IsOriginalPlayerBody())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't proselytize " + ParentObject.itself + "!");
				}
				return false;
			}
			if (flag && !ParentObject.CanMakeTelepathicContactWith(item))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Without a tongue, you cannot proselytize " + item.t() + ".");
				}
				return false;
			}
			if (!ParentObject.CheckFrozen(Telepathic: true, Telekinetic: false, Silent: true, item))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("Frozen solid, you cannot proselytize " + item.t() + ".");
				}
				return false;
			}
			if (item.HasEffect("Proselytized", OurEffect))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You have already proselytized " + item.t() + ".");
				}
				return false;
			}
			if (!ConversationScript.IsPhysicalConversationPossible(ParentObject, item, ShowPopup: true, AllowCombat: true, AllowFrozen: true) && !ConversationScript.IsMentalConversationPossible(ParentObject, item, ShowPopup: true, AllowCombat: true))
			{
				return false;
			}
			if (item.PartyLeader == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo(item.Does("are") + " already your follower. Do you want to proselytize " + item.them + " anyway?") != 0)
			{
				return false;
			}
			if (!item.CheckInfluence(By: ParentObject, Type: base.Name))
			{
				return false;
			}
			int num = Math.Max(item.Stat("Level") - ParentObject.Stat("Level"), 0);
			if (item.HasEffect("Proselytized"))
			{
				num++;
			}
			if (item.HasEffect("Rebuked"))
			{
				num++;
			}
			if (item.GetEffect("Beguiled") is Beguiled beguiled)
			{
				num += beguiled.LevelApplied;
			}
			int num2 = ParentObject.StatMod("Ego");
			if (Options.SifrahRecruitment)
			{
				new ProselytizationSifrah(item, num2, num).Play(item);
			}
			else
			{
				PerformMentalAttack(Proselytize, ParentObject, item, null, "Proselytize", "1d8-6", 2, int.MinValue, int.MinValue, num2, num);
			}
			ParentObject.UseEnergy(1000, "Skill Proselytize");
			CooldownMyActivatedAbility(ActivatedAbilityID, 25);
		}
		return true;
	}

	public static bool Proselytize(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		if (E.Penetrations <= 0 || !defender.ApplyEffect(new Proselytized(E.Attacker)))
		{
			Popup.ShowFail(defender.T() + defender.Is + " unconvinced by your pleas.");
			return false;
		}
		return true;
	}
}
