using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Domination : BaseMutation
{
	public const string METEMPSYCHOSIS_COUNT_KEY = "MetempsychosisCount";

	public const string METEMPSYCHOSIS_FLAG = "MetempsychosisFlag";

	public const string METEMPSYCHOSIS_ORIGINAL_PLAYER_BODY_FLAG = "MetempsychosisOriginalPlayerBodyFlag";

	public bool RealityDistortionBased;

	public string pinnedZone;

	public new Guid ActivatedAbilityID;

	public GameObject Target;

	public Domination()
	{
		DisplayName = "Domination";
		Type = "Mental";
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		Target = null;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeDeathRemovalEvent.ID && ID != EffectAppliedEvent.ID)
		{
			return ID == IsConversationallyResponsiveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		PerformMetempsychosis();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (E.Effect.IsOfType(33554432))
		{
			InterruptDomination();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsConversationallyResponsiveEvent E)
	{
		if (E.Speaker == ParentObject && GameObject.validate(ref Target))
		{
			if (E.Mental && !E.Physical)
			{
				E.Message = ParentObject.Poss("mind") + " seems to be elsewhere.";
			}
			else
			{
				E.Message = ParentObject.T() + ParentObject.Is + " utterly unresponsive.";
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		return "You garrote an adjacent creature's mind and control its actions while your own body lies dormant.";
	}

	public void Pin()
	{
		Unpin();
		pinnedZone = ParentObject.pPhysics.CurrentCell.ParentZone.ZoneID;
		if (!The.ZoneManager.PinnedZones.CleanContains(pinnedZone))
		{
			The.ZoneManager.PinnedZones.Add(pinnedZone);
		}
	}

	public void Unpin()
	{
		if (pinnedZone != null)
		{
			The.ZoneManager.PinnedZones.Remove(pinnedZone);
			pinnedZone = null;
		}
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat(string.Concat("Mental attack versus creature with a mind\n" + "Success roll: {{rules|mutation rank}} or Ego mod (whichever is higher) + character level + 1d8 VS. Defender MA + character level\n", "Range: 1\n"), "Duration: {{rules|", GetDuration(Level).ToString(), "}} rounds\n"), "Cooldown: 75 rounds");
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeAITakingAction");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "ChainInterruptDomination");
		Object.RegisterPartEvent(this, "CommandDominateCreature");
		Object.RegisterPartEvent(this, "DominationBroken");
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (GameObject.validate(ref Target) && RealityDistortionBased && (!ParentObject.FireEvent("CheckRealityDistortionUsability") || !Target.LocalEvent("CheckRealityDistortionAccessibility")))
			{
				BreakDomination();
			}
		}
		else if (E.ID == "TookDamage")
		{
			InterruptDomination();
		}
		else if (E.ID == "DominationBroken")
		{
			BreakDomination();
		}
		else if (E.ID == "ChainInterruptDomination")
		{
			if (GameObject.validate(ref Target) && !Target.FireEvent("InterruptDomination"))
			{
				return false;
			}
		}
		else if (E.ID == "BeforeAITakingAction")
		{
			if (GameObject.validate(ref Target))
			{
				return false;
			}
		}
		else if (E.ID == "CommandDominateCreature")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return true;
			}
			Cell cell = PickDirection(ForAttack: true);
			if (cell != null)
			{
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						goto IL_0320;
					}
				}
				foreach (GameObject item in cell.GetObjectsWithPart("Combat"))
				{
					if (item.HasCopyRelationship(ParentObject))
					{
						if (ParentObject.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You can't dominate " + ParentObject.itself + "!", 'R');
						}
						break;
					}
					if (GetDominationEffect(item) != null)
					{
						if (ParentObject.IsPlayer())
						{
							Popup.ShowFail("You can't dominate someone you are already dominating.");
						}
						break;
					}
					if (item.HasEffect("Dominated"))
					{
						if (ParentObject.IsPlayer())
						{
							Popup.ShowFail("You can't do that.");
						}
						break;
					}
					if (!item.CheckInfluence(By: ParentObject, Type: base.Name))
					{
						break;
					}
					if (item.HasStat("Level"))
					{
						int attackModifier = ParentObject.Stat("Level") + Math.Max(ParentObject.StatMod("Ego"), base.Level);
						PerformMentalAttack(Dominate, ParentObject, item, null, "Domination", "1d8", 1, GetDuration(base.Level), int.MinValue, attackModifier, item.Stat("Level"));
						CooldownMyActivatedAbility(ActivatedAbilityID, 75);
						UseEnergy(1000, "Mental Mutation");
						break;
					}
				}
			}
		}
		goto IL_0320;
		IL_0320:
		return base.FireEvent(E);
	}

	public bool Dominate(MentalAttackEvent E)
	{
		GameObject defender = E.Defender;
		if (defender.FireEvent("CanApplyDomination") && CanApplyEffectEvent.Check(defender, "Domination"))
		{
			if (E.Penetrations > 0)
			{
				if (defender.ApplyEffect(new Dominated(ParentObject, E.Magnitude)))
				{
					defender.SmallTeleportSwirl(null, "&M");
					DidXToY("take", "control of", defender, null, "!");
					Pin();
					Target = defender;
					The.Game.Player.Body = defender;
					IComponent<GameObject>.ThePlayer.Target = null;
					return true;
				}
				IComponent<GameObject>.AddPlayerMessage("Something prevents you from dominating " + defender.t() + ".", 'R');
			}
			else
			{
				IComponent<GameObject>.XDidY(defender, "resist", "domination", "!", null, null, ParentObject);
				defender.pBrain?.GetAngryAt(ParentObject);
			}
		}
		else if (defender.HasPart("Brain"))
		{
			IComponent<GameObject>.AddPlayerMessage(defender.T() + defender.GetVerb("do") + " not have a consciousness you can make psychic contact with.", 'R');
		}
		else
		{
			IComponent<GameObject>.AddPlayerMessage("There seems to be no mind in " + defender.t() + " to dominate.", 'R');
		}
		return false;
	}

	private bool IsOurDominationEffect(Dominated FX)
	{
		return FX.Dominator == ParentObject;
	}

	public Dominated GetDominationEffect(GameObject obj = null)
	{
		return (obj ?? Target)?.GetEffect<Dominated>(IsOurDominationEffect);
	}

	public int GetDuration(int Level)
	{
		return 100 + 100 * Level;
	}

	public bool BreakDomination()
	{
		if (!GameObject.validate(ref Target))
		{
			return false;
		}
		Dominated dominationEffect = GetDominationEffect();
		if (dominationEffect != null && !dominationEffect.BeingRemovedBySource)
		{
			dominationEffect.BeingRemovedBySource = true;
			Target.RemoveEffect(dominationEffect);
		}
		if (Target.OnWorldMap())
		{
			Target.PullDown();
		}
		Target.UpdateVisibleStatusColor();
		Target = null;
		XRLCore.Core.Game.Player.Body = ParentObject;
		IComponent<GameObject>.ThePlayer.Target = null;
		if (ParentObject.IsPlayer())
		{
			Popup.Show("{{r|Your domination is broken!}}");
			CheckMetempsychosis(ParentObject);
		}
		ParentObject.SmallTeleportSwirl(null, "&M");
		ParentObject.pBrain.Goals.Clear();
		CooldownMyActivatedAbility(ActivatedAbilityID, 75);
		ParentObject.UpdateVisibleStatusColor();
		Sidebar.UpdateState();
		Unpin();
		return true;
	}

	public static void Metempsychosis(GameObject who, bool FromOriginalPlayerBody = false)
	{
		if (who.IsPlayer())
		{
			int intGameState = The.Game.GetIntGameState("MetempsychosisCount");
			if (FromOriginalPlayerBody && intGameState <= 0)
			{
				Popup.Show("{{r|Your mind is stranded here.}}");
			}
			else if (Scanning.GetScanTypeFor(who) == Scanning.Scan.Bio)
			{
				Popup.Show("{{r|Your mind is stranded here.}}");
			}
			else
			{
				Popup.Show("{{r|Your mind is stranded here.}}");
			}
			The.Game.SetIntGameState("MetempsychosisCount", intGameState + 1);
			who.pBrain.Factions = "";
			who.pBrain.FactionMembership.Clear();
			who.pBrain.FactionFeelings.Clear();
			who.RemovePart("GivesRep");
		}
		else
		{
			who.SetIntProperty("MetempsychosisFlag", 1);
			if (FromOriginalPlayerBody)
			{
				who.SetIntProperty("MetempsychosisOriginalPlayerBodyFlag", 1);
			}
		}
	}

	public static void CheckMetempsychosis(GameObject who)
	{
		if (who.GetIntProperty("MetempsychosisFlag") > 0)
		{
			who.RemoveIntProperty("MetempsychosisFlag");
			bool fromOriginalPlayerBody = false;
			if (who.GetIntProperty("MetempsychosisOriginalPlayerBodyFlag") > 0)
			{
				who.RemoveIntProperty("MetempsychosisOriginalPlayerBodyFlag");
				fromOriginalPlayerBody = true;
			}
			Metempsychosis(who, fromOriginalPlayerBody);
		}
	}

	public bool PerformMetempsychosis()
	{
		if (!GameObject.validate(ref Target))
		{
			return false;
		}
		Dominated dominationEffect = GetDominationEffect();
		if (dominationEffect != null && !dominationEffect.BeingRemovedBySource)
		{
			dominationEffect.BeingRemovedBySource = true;
			dominationEffect.Metempsychosis = true;
			Target.RemoveEffect(dominationEffect);
			Metempsychosis(Target, dominationEffect.FromOriginalPlayerBody);
		}
		Target = null;
		Unpin();
		return true;
	}

	public void InterruptDomination()
	{
		if (GameObject.validate(ref Target))
		{
			Target.FireEvent("InterruptDomination");
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dominate Creature", "CommandDominateCreature", "Mental Mutation", GetDescription(), "\u0003", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
