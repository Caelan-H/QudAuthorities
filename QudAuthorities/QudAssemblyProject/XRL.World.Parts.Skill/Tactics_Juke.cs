using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Juke : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandTacticsJuke");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandTacticsJuke")
		{
			if (ParentObject.OnWorldMap())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You cannot juke on the world map.");
				}
				return true;
			}
			if (!ParentObject.CheckFrozen() || !ParentObject.CanChangeBodyPosition("Juking", ShowMessage: true) || !ParentObject.CanChangeMovementMode("Juking", ShowMessage: true))
			{
				return true;
			}
			Cell cell = PickDirection();
			if (cell == null || cell == ParentObject.CurrentCell)
			{
				return true;
			}
			foreach (GameObject item in cell.LoopObjectsWithPart("Physics"))
			{
				if (item.ConsiderSolidFor(ParentObject))
				{
					Popup.ShowFail("You cannot juke into " + item.the + item.ShortDisplayName + ".");
					return true;
				}
			}
			Cell cell2 = ParentObject.CurrentCell;
			if (cell2 == null)
			{
				return true;
			}
			GameObject obj = null;
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject = cell.Objects[i];
				if (gameObject.HasPart("Combat") && ParentObject.PhaseAndFlightMatches(gameObject) && (!gameObject.HasPart("FungalVision") || FungalVisionary.VisionLevel > 0))
				{
					if (gameObject.GetMatterPhase() >= 3 || IsRootedInPlaceEvent.Check(gameObject))
					{
						Popup.ShowFail("You cannot juke " + gameObject.the + gameObject.ShortDisplayName + " out of your way.");
						return true;
					}
					if (obj != null)
					{
						Popup.ShowFail("You cannot juke both " + gameObject.the + gameObject.ShortDisplayName + " and " + obj.the + obj.ShortDisplayName + " out of your way.");
						return true;
					}
					obj = gameObject;
				}
			}
			cell2.RemoveObject(ParentObject);
			if (obj != null)
			{
				cell.RemoveObject(obj);
				cell2.AddObject(obj);
			}
			cell.AddObject(ParentObject);
			if (GameObject.validate(ref obj) && cell2.Objects.Contains(obj))
			{
				if (ParentObject.HasSkill("ShortBlades_PointedCircle") && obj.IsHostileTowards(ParentObject) && ParentObject.PhaseAndFlightMatches(obj))
				{
					GameObject primaryWeaponOfType = ParentObject.GetPrimaryWeaponOfType("ShortBlades", AcceptFirstHandForNonHandPrimary: true);
					if (primaryWeaponOfType != null)
					{
						Event @event = Event.New("MeleeAttackWithWeapon");
						@event.SetParameter("Attacker", ParentObject);
						@event.SetParameter("Defender", obj);
						@event.SetParameter("Weapon", primaryWeaponOfType);
						@event.SetParameter("Properties", "Juking");
						ParentObject.FireEvent(@event);
					}
				}
				ParentObject?.FireEvent(Event.New("JukedTarget", "Defender", obj));
				obj?.FireEvent(Event.New("WasJuked", "Attacker", ParentObject));
			}
			int turns = (ParentObject.HasPart("Acrobatics_Tumble") ? 20 : 40);
			CooldownMyActivatedAbility(ActivatedAbilityID, turns, null, "Agility");
			ParentObject.FireEvent(Event.New("Juked", "FromCell", cell2, "ToCell", cell));
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Juke", "CommandTacticsJuke", "Skill", "Activated; cooldown 40. You move one square at no action cost. You may swap squares with a hostile opponent.", "\u0012");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
