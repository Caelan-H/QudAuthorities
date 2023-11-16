using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Beguiling : BaseMutation
{
	public const string PROPERTY_ID = "BeguilingTargetID";

	public bool RealityDistortionBased;

	public new Guid ActivatedAbilityID;

	public Beguiling()
	{
		DisplayName = "Beguiling";
		Type = "Mental";
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("jewels", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanCompanionRestoreParty");
		Object.RegisterPartEvent(this, "CommandBeguileCreature");
		Object.RegisterPartEvent(this, "GetMaxBeguiled");
		base.Register(Object);
	}

	public static int GetMaxTargets(GameObject Beguiler)
	{
		int num = Beguiler.GetIntProperty("MaxBeguiledBonus");
		if (Beguiler.HasRegisteredEvent("GetMaxBeguiled"))
		{
			Event @event = Event.New("GetMaxBeguiled", "Amount", 0);
			Beguiler.FireEvent(@event);
			num += @event.GetIntParameter("Amount");
		}
		return num;
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Beguiled beguiled)
		{
			return beguiled.Beguiler == ParentObject;
		}
		return false;
	}

	public override string GetDescription()
	{
		return "You beguile a nearby creature into serving you loyally.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Mental attack versus a creature with a mind\n" + "Success roll: {{rules|mutation rank}} or Ego mod (whichever is higher) + character level + 1d8 VS. Defender MA + character level\n", "Range: 1\n"), "Beguiled creature: +{{rules|", (Level * 5).ToString(), "}} bonus hit points\n"), "Cooldown: 50 rounds");
	}

	public static bool Cast(GameObject who, Beguiling mutation = null, Event ev = null, int genericLevel = 1)
	{
		bool independent = false;
		if (mutation == null)
		{
			mutation = new Beguiling();
			mutation.Level = genericLevel;
			mutation.ParentObject = who;
			independent = true;
		}
		Cell cell = mutation.PickDirection("[Select a direction to beguile a creature]");
		if (cell != null)
		{
			if (mutation.RealityDistortionBased)
			{
				Event e = Event.New("InitiateRealityDistortionTransit", "Object", who, "Mutation", mutation, "Cell", cell);
				if (!who.FireEvent(e, ev) || !cell.FireEvent(e, ev))
				{
					RealityStabilized.ShowGenericInterdictMessage(who);
					return false;
				}
			}
			foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
			{
				if (item == null || item == who || !item.IsValid() || !item.HasStat("Level") || !item.HasStat("Hitpoints"))
				{
					continue;
				}
				if (item.HasCopyRelationship(who) || item.IsOriginalPlayerBody())
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You can't beguile " + who.itself + "!", 'R');
					}
					return false;
				}
				if (!item.FireEvent("CanApplyBeguile") || !CanApplyEffectEvent.Check(item, "Beguile"))
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage(item.Does("seem") + " utterly impervious to your charms.");
					}
					return false;
				}
				if (!item.CheckInfluence(mutation.Name, who))
				{
					return false;
				}
				if (item.HasEffect("Beguiled", mutation.OurEffect))
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You have already beguiled " + item.t() + ".", 'R');
					}
					return false;
				}
				if (item.IsLedBy(who) && who.IsPlayer() && Popup.ShowYesNo(item.Does("are", int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " already your follower. Do you want to beguile " + item.them + " anyway?") != 0)
				{
					return false;
				}
				mutation?.UseEnergy(1000, "Mental Mutation");
				mutation?.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, 50);
				if (Options.SifrahRecruitment)
				{
					int rating = who.Stat("Level") + who.StatMod("Ego");
					int num = item.Stat("Level");
					if (item.HasEffect("Proselytized"))
					{
						num++;
					}
					if (item.HasEffect("Rebuked"))
					{
						num++;
					}
					if (item.HasEffect("Beguiled"))
					{
						Beguiled beguiled = item.GetEffect("Beguiled") as Beguiled;
						num += beguiled.LevelApplied;
					}
					BeguilingSifrah beguilingSifrah = new BeguilingSifrah(item, mutation.Level, independent, rating, num);
					beguilingSifrah.Play(item);
					return beguilingSifrah.Success;
				}
				if (item.HasEffect("Beguiled") && (item.GetEffect("Beguiled") as Beguiled).LevelApplied + "1d8".RollCached() > mutation.Level + "1d8".RollCached())
				{
					if (who.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You fail to outshine the current object of " + item.poss("affection") + ".", 'R');
					}
					return false;
				}
				int attackModifier = who.Stat("Level") + Math.Max(who.StatMod("Ego"), mutation.Level);
				return Mental.PerformAttack(mutation.Beguile, who, item, null, "Beguiling", "1d8", 1, int.MinValue, int.MinValue, attackModifier, item.Stat("Level"));
			}
			if (who.IsPlayer())
			{
				Popup.Show("There are no valid targets in that square.");
			}
		}
		return false;
	}

	private bool Beguile(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		bool independent = ActivatedAbilityID == Guid.Empty;
		if (E.Penetrations <= 0 || !defender.FireEvent("CanApplyBeguile") || !CanApplyEffectEvent.Check(defender, "Beguile") || !defender.ApplyEffect(new Beguiled(E.Attacker, base.Level, independent)))
		{
			IComponent<GameObject>.AddPlayerMessage("Your coquetry infuriates " + defender.the + defender.DisplayNameOnly + ".", 'r');
			defender.GetAngryAt(E.Attacker, -5);
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandBeguileCreature")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
			}
			else
			{
				Cast(ParentObject, this, E);
			}
		}
		else if (E.ID == "CanCompanionRestorePartyLeader")
		{
			if (ParentObject.SupportsFollower(E.GetGameObjectParameter("Companion")))
			{
				return false;
			}
		}
		else if (E.ID == "GetMaxBeguiled")
		{
			E.ModParameter("Amount", 1);
		}
		return base.FireEvent(E);
	}

	public IEnumerable<GameObject> YieldTargets()
	{
		if (ParentObject.pBrain == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, int> partyMember in ParentObject.pBrain.PartyMembers)
		{
			if (partyMember.Value.HasBit(2))
			{
				GameObject gameObject = GameObject.findById(partyMember.Key);
				if (gameObject != null)
				{
					yield return gameObject;
				}
			}
		}
	}

	public static void SyncTarget(GameObject Beguiler, GameObject Target = null, bool Independent = false)
	{
		if (Beguiler.pBrain == null)
		{
			return;
		}
		int num = GetMaxTargets(Beguiler);
		if (Target == null)
		{
			num++;
		}
		Dictionary<string, int> partyMembers = Beguiler.pBrain.PartyMembers;
		string[] array = (from x in partyMembers
			where x.Value.HasBit(2)
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
			partyMembers[Target.id] = 2;
			if (Independent)
			{
				partyMembers[Target.id] |= 8388608;
			}
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		foreach (GameObject item in YieldTargets())
		{
			(item.GetEffect("Beguiled") as Beguiled)?.SyncToMutation();
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Beguile Creature", "CommandBeguileCreature", "Mental Mutation", "Beguile", "\u0003", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		SyncTarget(GO);
		return base.Unmutate(GO);
	}
}
