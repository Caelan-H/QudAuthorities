using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Slam : BaseSkill
{
	public const int MAX_STUN_DURATION = 4;

	public Guid ActivatedAbilityID = Guid.Empty;

	public Cudgel_Slam()
	{
	}

	public Cudgel_Slam(GameObject ParentObject)
		: this()
	{
		this.ParentObject = ParentObject;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "CommandCudgelSlam");
		base.Register(Object);
	}

	public GameObject GetEquippedCudgel()
	{
		return ParentObject.Body?.GetWeaponOfType("Cudgel", NeedPrimary: false, PreferPrimary: true);
	}

	public bool IsCudgelEquipped()
	{
		return GetEquippedCudgel() != null;
	}

	public static bool Cast(GameObject attacker, Cudgel_Slam skill = null, string slamDir = null, GameObject target = null, bool requireWeapon = true, int presetSlamPower = int.MinValue, string impactDamageIncrement = null)
	{
		if (skill == null)
		{
			skill = new Cudgel_Slam(attacker);
		}
		Cell cell = attacker.GetCurrentCell();
		Cell cell2;
		string text;
		if (target != null)
		{
			cell2 = target.GetCurrentCell();
			text = slamDir ?? cell?.GetDirectionFromCell(cell2) ?? Directions.GetRandomDirection();
		}
		else
		{
			text = slamDir ?? skill.PickDirectionS();
			cell2 = cell?.GetCellFromDirection(text);
			if (cell2 == null)
			{
				return false;
			}
			if (!cell2.HasObjectWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)) && !cell2.HasObjectWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)) && !cell2.HasObjectWithPart("Combat", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)))
			{
				if (attacker.IsPlayer())
				{
					Popup.Show("There's nothing there to slam.");
				}
				return false;
			}
		}
		GameObject gameObject = skill.GetEquippedCudgel() ?? attacker.GetPrimaryWeapon();
		if (gameObject == null && requireWeapon)
		{
			if (attacker.IsPlayer())
			{
				Popup.Show("You have no weapon!");
			}
			return false;
		}
		int parameter = 1;
		Event @event = new Event("GetSlamMultiplier", "Multiplier", parameter, "Weapon", gameObject);
		attacker.FireEvent(@event);
		gameObject?.FireEvent(@event);
		parameter = @event.GetIntParameter("Multiplier");
		int num = attacker.StatMod("Strength") * 5 * parameter;
		if (presetSlamPower != int.MinValue)
		{
			num = presetSlamPower;
		}
		GameObject gameObject2 = target ?? cell2.GetCombatTarget(attacker);
		if (gameObject2 == attacker)
		{
			if (attacker.IsPlayer())
			{
				if (Popup.ShowYesNo("Are you sure you want to slam " + attacker.itself + "?") != 0)
				{
					return true;
				}
			}
			else
			{
				MetricsManager.LogError(attacker.DebugName + " attempted to use Slam on self " + ((target == null) ? "via cell targeting" : "via explicit specification"));
			}
		}
		if (gameObject2 == null)
		{
			if (cell2.HasObjectWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)))
			{
				foreach (GameObject item in cell2.GetObjectsWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)))
				{
					if (Stats.GetCombatAV(item) < num)
					{
						item.DustPuff();
						for (int i = 0; i < 5; i++)
						{
							item.ParticleText(item.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
						}
						item.Destroy();
						if (attacker.IsPlayer())
						{
							Cudgel_Slam cudgel_Slam = skill;
							GameObject colorAsBadFor = item;
							cudgel_Slam.DidXToY("slam", "through", item, null, null, null, attacker, colorAsBadFor);
							IComponent<GameObject>.XDidY(item, "are", "destroyed", null, null, null, item);
						}
						continue;
					}
					if (attacker.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You aren't strong enough to slam through " + item.t() + ".");
					}
					return false;
				}
			}
			if (cell2.HasObjectWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)))
			{
				foreach (GameObject item2 in cell2.GetObjectsWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)))
				{
					if (item2.GetPart("Door") is Door door && door.bOpen)
					{
						if (attacker.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage(item2.T() + item2.GetVerb("are") + " open.");
						}
						continue;
					}
					if (Stats.GetCombatAV(item2) < num)
					{
						item2.DustPuff();
						for (int j = 0; j < 5; j++)
						{
							item2.ParticleText(item2.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
						}
						item2.Destroy();
						if (attacker.IsPlayer())
						{
							Cudgel_Slam cudgel_Slam2 = skill;
							GameObject colorAsBadFor = item2;
							cudgel_Slam2.DidXToY("slam", "through", item2, null, null, null, attacker, colorAsBadFor);
							IComponent<GameObject>.XDidY(item2, "are", "destroyed", null, null, null, item2);
						}
						continue;
					}
					if (attacker.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You aren't strong enough to slam through " + item2.t() + ".");
					}
					return false;
				}
			}
			attacker.UseEnergy(1000, "Skill Cudgel Slam");
			if (!attacker.HasEffect("Cudgel_SmashingUp"))
			{
				skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, 50);
			}
		}
		else
		{
			Cudgel_Slam cudgel_Slam3 = skill;
			GameObject obj2 = gameObject2;
			GameObject colorAsBadFor = gameObject2;
			cudgel_Slam3.DidXToY("attempt", "to slam into", obj2, null, "!", null, attacker, colorAsBadFor);
			Event event2 = Event.New("BeginAttack");
			event2.SetParameter("TargetObject", gameObject2);
			event2.SetParameter("TargetCell", cell2);
			if (attacker.FireEvent(event2) && cell2 != null && attacker.pPhysics != null)
			{
				Event event3 = Event.New("MeleeAttackWithWeapon");
				event3.SetParameter("Attacker", attacker);
				event3.SetParameter("Defender", gameObject2);
				event3.SetParameter("Weapon", gameObject);
				event3.SetParameter("PenBonus", 1);
				event3.SetParameter("PenCapBonus", 1);
				attacker.FireEvent(event3);
				attacker.UseEnergy(1000, "Skill Cudgel Slam");
				if (!attacker.HasEffect("Cudgel_SmashingUp"))
				{
					skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, 50);
				}
				if (event3.HasParameter("DidHit") || !requireWeapon)
				{
					bool flag = false;
					if (gameObject2.IsInvalid())
					{
						skill.LogInEditor("Displacing corpse mode!");
						flag = true;
						if (cell2.Objects.Count == 0)
						{
							cell2.AddObject("Garbage");
						}
						if (cell2.HasObjectWithTag("Corpse"))
						{
							gameObject2 = cell2.GetObjectWithTag("Corpse");
						}
					}
					Dictionary<GameObject, string> dictionary = new Dictionary<GameObject, string>(8);
					int num2 = (3 + attacker.GetIntProperty("SlamDistanceBonus")) * parameter;
					for (int k = 0; k < num2 && skill.Slam(gameObject2, text, num2 - k, num, dictionary); k++)
					{
					}
					string dice = ((impactDamageIncrement != null) ? impactDamageIncrement : gameObject.GetPart<MeleeWeapon>().BaseDamage);
					if (dictionary.Count == 0)
					{
						dictionary.Add(gameObject2, "s");
					}
					foreach (KeyValuePair<GameObject, string> item3 in dictionary)
					{
						GameObject key = item3.Key;
						if (flag && key == gameObject2)
						{
							continue;
						}
						string value = item3.Value;
						_ = value.Length;
						int num3 = 0;
						int num4 = 0;
						for (int l = 0; l < value.Length; l++)
						{
							if (value[l] == 'w')
							{
								num3++;
							}
							else if (value[l] == 's')
							{
								num4++;
							}
						}
						int num5 = 0;
						for (int m = 0; m < num3; m++)
						{
							num5 += dice.RollCached();
						}
						int duration = Math.Min(4, (num4 == 1) ? 1 : (num4 + 1));
						if (num5 > 0)
						{
							string message = null;
							if (num3 == 0)
							{
								message = "from %o slam!";
							}
							else if (num3 == 1)
							{
								message = "from slamming into a wall!";
							}
							else if (num3 == 2)
							{
								message = "from slamming into {{W|two}} walls!";
							}
							else if (num3 >= 3)
							{
								message = "from slamming into {{r|three}} walls!";
							}
							int amount = num5;
							colorAsBadFor = attacker;
							key.TakeDamage(amount, message, "Crushing", null, null, null, colorAsBadFor, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: true);
						}
						key.ApplyEffect(new Stun(duration, 9999));
					}
					if (flag && gameObject2.GetCurrentCell() != null)
					{
						List<GameObject> list = new List<GameObject>(cell2.GetObjectsWithPart("Physics", (GameObject obj) => obj.PhaseAndFlightMatches(attacker)));
						Cell targetCell = gameObject2.GetCurrentCell();
						foreach (GameObject item4 in list)
						{
							if (item4.Weight < 2000)
							{
								item4.DirectMoveTo(targetCell);
							}
						}
					}
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (E.GetIntParameter("Distance") == 1 && IsCudgelEquipped() && !ParentObject.CanMoveExtremities() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand("CommandCudgelSlam");
			}
		}
		else if (E.ID == "CommandCudgelSlam")
		{
			if (!IsCudgelEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a cudgel equipped in order to use slam.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities(null, ShowMessage: true))
			{
				return false;
			}
			if (!Cast(ParentObject, this))
			{
				return false;
			}
		}
		return base.FireEvent(E);
	}

	public bool Slam(GameObject target, string sDirection, int MaxDistance, int slamPower, Dictionary<GameObject, string> effects)
	{
		if (MaxDistance < 0)
		{
			return false;
		}
		if (target.IsInvalid())
		{
			return false;
		}
		Cell cell = target.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		if (cell.HasObjectWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
		{
			if (!effects.ContainsKey(target))
			{
				effects.Add(target, "");
			}
			effects[target] += "w";
			foreach (GameObject item in cell.GetObjectsWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
			{
				if (Stats.GetCombatAV(item) < slamPower)
				{
					item.DustPuff();
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText(item.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					IComponent<GameObject>.XDidY(item, "are", "destroyed", "!");
					item.Destroy();
					continue;
				}
				return false;
			}
		}
		if (cell.HasObjectWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
		{
			if (!effects.ContainsKey(target))
			{
				effects.Add(target, "");
			}
			effects[target] += "w";
			foreach (GameObject item2 in cell.GetObjectsWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
			{
				if (Stats.GetCombatAV(item2) < slamPower)
				{
					item2.DustPuff();
					for (int j = 0; j < 5; j++)
					{
						item2.ParticleText(item2.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					IComponent<GameObject>.XDidY(item2, "are", "destroyed", "!");
					item2.Destroy();
					continue;
				}
				return false;
			}
		}
		cell = target.CurrentCell.GetLocalCellFromDirection(sDirection);
		if (cell == null)
		{
			return false;
		}
		if (cell.HasObjectWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
		{
			if (!effects.ContainsKey(target))
			{
				effects.Add(target, "");
			}
			effects[target] += "w";
			foreach (GameObject item3 in cell.GetObjectsWithIntProperty("Wall", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
			{
				if (Stats.GetCombatAV(item3) < slamPower)
				{
					item3.DustPuff();
					for (int k = 0; k < 5; k++)
					{
						item3.ParticleText(item3.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					IComponent<GameObject>.XDidY(item3, "are", "destroyed", "!");
					item3.Destroy();
					continue;
				}
				return false;
			}
		}
		if (cell.HasObjectWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
		{
			if (!effects.ContainsKey(target))
			{
				effects.Add(target, "");
			}
			effects[target] += "w";
			foreach (GameObject item4 in cell.GetObjectsWithTag("Door", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
			{
				if (Stats.GetCombatAV(item4) < slamPower)
				{
					item4.DustPuff();
					for (int l = 0; l < 5; l++)
					{
						item4.ParticleText(item4.pRender.TileColor + (char)(219 + Stat.Random(0, 4)), 4.9f, 5);
					}
					IComponent<GameObject>.XDidY(item4, "are", "destroyed", "!");
					item4.Destroy();
					continue;
				}
				return false;
			}
		}
		if (cell.IsEmpty() && target.Weight < 2000 && target.CanBeInvoluntarilyMoved())
		{
			if (target.Move(sDirection, Forced: true))
			{
				if (!effects.ContainsKey(target))
				{
					effects.Add(target, "");
				}
				effects[target] += "s";
				if (target.CurrentCell != null)
				{
					foreach (Cell adjacentCell in target.CurrentCell.GetAdjacentCells())
					{
						adjacentCell.FireEvent(Event.New("ObjectEnteredAdjacentCell", "Object", target));
					}
				}
				return true;
			}
		}
		else
		{
			foreach (GameObject item5 in cell.GetObjectsWithPart("Combat", (GameObject obj) => obj.PhaseAndFlightMatches(ParentObject)))
			{
				Slam(item5, sDirection, MaxDistance - 1, slamPower, effects);
			}
		}
		if (cell.IsEmpty() && target.Weight < 2000 && target.CanBeInvoluntarilyMoved())
		{
			if (target.Move(sDirection, Forced: true))
			{
				if (!effects.ContainsKey(target))
				{
					effects.Add(target, "");
				}
				effects[target] += "s";
				if (target.CurrentCell != null)
				{
					foreach (Cell adjacentCell2 in target.CurrentCell.GetAdjacentCells())
					{
						adjacentCell2.FireEvent(Event.New("ObjectEnteredAdjacentCell", "Object", target));
					}
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Slam", "CommandCudgelSlam", "Skill", "You make an attack with a cudgel at an adjacent opponent at +1 penetration. If you hit, you slam your opponent backwards up to 3 spaces, pushing other creatures and breaking through walls if their AVs are less than 5 times your strength modifier. Opponents who get pushed are stunned for 1 round plus an additional round for each space pushed. Opponents who are pushed through or against walls take extra weapon damage for each wall. Colossal opponents don't get pushed but are still stunned for 1 round.", "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
