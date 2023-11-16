using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Persuasion_RebukeRobot : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public string TargetID;

	public static void Neutralize(GameObject Actor, GameObject Object)
	{
		Brain pBrain = Object.pBrain;
		if (pBrain != null)
		{
			pBrain.StopFighting();
			pBrain.Goals.Clear();
			pBrain.Hostile = false;
			pBrain.Target = null;
			pBrain.Wanders = true;
			if (pBrain.GetFeeling(Actor) < 0)
			{
				pBrain.SetFeeling(Actor, 0);
			}
		}
	}

	public void Neutralize(GameObject Object)
	{
		Neutralize(ParentObject, Object);
	}

	public override void FinalizeCopy(GameObject Source, bool CopyEffects, bool CopyID, Func<GameObject, GameObject> MapInv)
	{
		base.FinalizeCopy(Source, CopyEffects, CopyID, MapInv);
		TargetID = null;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CanCompanionRestorePartyLeader");
		Object.RegisterPartEvent(this, "CommandRebukeRobot");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRebukeRobot")
		{
			if (ParentObject.IsMissingTongue())
			{
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You cannot rebuke without a tongue.");
				}
			}
			else if (ParentObject.CheckFrozen())
			{
				Cell cell = PickDirection();
				if (cell != null)
				{
					bool flag = false;
					foreach (GameObject item in cell.GetObjectsWithPart("Brain"))
					{
						if (item == ParentObject || !item.Statistics.ContainsKey("Level") || !item.HasPart("Robot"))
						{
							continue;
						}
						if (!item.CheckInfluence(By: ParentObject, Type: base.Name))
						{
							goto IL_023d;
						}
						flag = true;
						int num = item.Stat("Level") * 4 / 5;
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
						int @for = GetRebukeLevelEvent.GetFor(ParentObject, item);
						@for = ParentObject.StatMod("Ego") + @for * 4 / 5;
						if (Options.SifrahRecruitment)
						{
							new RebukingSifrah(item, @for, num).Play(item);
						}
						else
						{
							PerformMentalAttack(Rebuke, ParentObject, item, null, "Rebuke Robot", null, 2, int.MinValue, int.MinValue, @for, num);
						}
						CooldownMyActivatedAbility(ActivatedAbilityID, 100);
						ParentObject.UseEnergy(1000, "Skill Rebuke Robot");
					}
					if (!flag && ParentObject.IsPlayer())
					{
						Popup.Show("There is nothing there to rebuke.");
					}
				}
			}
		}
		else if (E.ID == "CanCompanionRestorePartyLeader" && E.GetGameObjectParameter("Companion").idmatch(TargetID))
		{
			return false;
		}
		goto IL_023d;
		IL_023d:
		return base.FireEvent(E);
	}

	public bool Rebuke(MentalAttackEvent E)
	{
		int penetrations = E.Penetrations;
		GameObject defender = E.Defender;
		if (penetrations <= 0)
		{
			IComponent<GameObject>.AddPlayerMessage("Your argument does not compute.");
			return false;
		}
		if (penetrations <= 2)
		{
			IComponent<GameObject>.XDidY(defender, "wander", "away disinterestedly");
			Neutralize(defender);
		}
		else
		{
			FinalizeRebuke(E.Attacker, defender);
		}
		return true;
	}

	public bool FinalizeRebuke(GameObject Actor, GameObject Robot)
	{
		Neutralize(Robot);
		GameObject obj = ((TargetID != null) ? GameObject.findById(TargetID) : null);
		if ((GameObject.validate(ref obj) && Popup.ShowYesNoCancel("Would you like to release control of " + obj.t() + " for " + Robot.t() + "?") != 0) || !Robot.ApplyEffect(new Rebuked(Actor)))
		{
			IComponent<GameObject>.XDidY(Robot, "wander", "away disinterestedly");
			return false;
		}
		return true;
	}

	public static bool Rebuke(GameObject Actor, GameObject Robot)
	{
		return (Actor.GetPart("Persuasion_RebukeRobot") as Persuasion_RebukeRobot)?.FinalizeRebuke(Actor, Robot) ?? false;
	}

	private bool OurEffect(Effect FX)
	{
		if (FX is Rebuked rebuked)
		{
			return rebuked.Rebuker == ParentObject;
		}
		return false;
	}

	public void SyncTarget(GameObject NewTarget)
	{
		if (TargetID != null)
		{
			GameObject gameObject = GameObject.findById(TargetID);
			if (gameObject != null)
			{
				gameObject.RemoveEffect("Rebuked", OurEffect);
				Neutralize(gameObject);
				IComponent<GameObject>.XDidY(gameObject, "wander", "away disinterestedly");
			}
		}
		TargetID = NewTarget.id;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rebuke Robot", "CommandRebukeRobot", "Skill", null, "\u0003");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
