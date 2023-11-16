using System;
using UnityEngine;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Stinger : BaseDefaultEquipmentMutation
{
	[Obsolete("save compat")]
	public string Placeholder1;

	[Obsolete("save compat")]
	public string Placeholder2;

	public string VenomType = "Poison";

	public GameObject StingerObject;

	public Guid StingActivatedAbilityID = Guid.Empty;

	public string ManagerID => ParentObject.id + "::Stinger";

	public string LateralityID => ParentObject.id + "::Stinger::Change";

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Stinger obj = base.DeepCopy(Parent, MapInv) as Stinger;
		obj.StingerObject = null;
		return obj;
	}

	public Stinger()
	{
		DisplayName = "Stinger";
	}

	public Stinger(string VenomType)
		: this()
	{
		this.VenomType = VenomType;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "AttackerAfterDamage");
		Object.RegisterPartEvent(this, "CommandSting");
		Object.RegisterPartEvent(this, "ChargedTarget");
		Object.RegisterPartEvent(this, "LungedTarget");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			if (gameObjectParameter != null && !gameObjectParameter.IsWall() && E.GetIntParameter("Distance") <= 1 && IsMyActivatedAbilityAIUsable(StingActivatedAbilityID) && ParentObject.CanMoveExtremities("Sting"))
			{
				E.AddAICommand("CommandSting");
			}
		}
		else if (E.ID == "ChargedTarget" || E.ID == "LungedTarget")
		{
			BodyPart bodyPart = StingerObject?.EquippedOn();
			if (bodyPart != null && !bodyPart.Primary)
			{
				Strike(StingerObject, E.GetGameObjectParameter("Defender"));
			}
		}
		else if (E.ID == "CommandSting")
		{
			if (StingerObject == null || !StingerObject.IsEquippedProperly())
			{
				return ParentObject.ShowFailure("You don't have a stinger.");
			}
			if (!ParentObject.CanMoveExtremities("Sting", ShowMessage: true))
			{
				return false;
			}
			Cell cell = PickDirection();
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(ParentObject);
			if (combatTarget == null || combatTarget == ParentObject)
			{
				if (ParentObject.IsPlayer())
				{
					if (cell.HasObjectWithPart("Combat"))
					{
						Popup.ShowFail("There is no one there you can sting.");
					}
					else
					{
						Popup.ShowFail("There is no one there to sting.");
					}
				}
				return false;
			}
			CooldownMyActivatedAbility(StingActivatedAbilityID, 25);
			UseEnergy(1000, "Physical Mutation Stinger");
			Strike(StingerObject, combatTarget, Auto: true);
		}
		else if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			string stringParameter = E.GetStringParameter("Properties", "");
			if (gameObjectParameter3 == null || gameObjectParameter3 != StingerObject)
			{
				return true;
			}
			int chance = 20;
			if (stringParameter.Contains("Stinging") || stringParameter.Contains("Charging") || stringParameter.Contains("Lunging"))
			{
				chance = 100;
			}
			if (chance.in100())
			{
				ApplyPoison(gameObjectParameter2);
			}
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		if (VenomType == "Paralyze")
		{
			return "You bear a tail with a stinger that delivers paralyzing venom to your enemies.";
		}
		if (VenomType == "Confuse")
		{
			return "You bear a tail with a stinger that delivers confusing venom to your enemies.";
		}
		if (VenomType == "Poison")
		{
			return "You bear a tail with a stinger that delivers poisonous venom to your enemies.";
		}
		return "<stinger>";
	}

	public void Strike(GameObject Stinger, GameObject Defender, bool Auto = false)
	{
		Event @event = Event.New("MeleeAttackWithWeapon");
		@event.SetParameter("Attacker", ParentObject);
		@event.SetParameter("Defender", Defender);
		@event.SetParameter("Weapon", Stinger);
		@event.SetParameter("Properties", Auto ? "Autohit,Autopen,Stinging" : "Stinging");
		ParentObject.FireEvent(@event);
	}

	public void ApplyPoison(GameObject Defender)
	{
		GetData(base.Level, out var _, out var _, out var Duration, out var Increment);
		if (VenomType == "Paralyze" && Defender.HasStat("Toughness") && Defender.FireEvent("CanApplyPoison") && CanApplyEffectEvent.Check(Defender, "Poison"))
		{
			Effect effect = new Paralyzed(Duration.RollCached(), -1);
			if (!Defender.MakeSave("Toughness", GetSave(base.Level), null, null, "Stinger Injected Paralysis Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, StingerObject) && Defender.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Defender, "Poison", effect, ParentObject))
			{
				Defender.ApplyEffect(effect, ParentObject);
			}
			else if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of your venom", null, null, Defender);
			}
			else if (Defender.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of " + ParentObject.poss("venom"), "!", null, Defender);
			}
		}
		if (VenomType == "Confuse" && Defender.HasStat("Toughness") && Defender.FireEvent("CanApplyPoison") && CanApplyEffectEvent.Check(Defender, "Poison"))
		{
			Effect effect2 = new Confused(Duration.RollCached(), base.Level, base.Level + 2);
			if (!Defender.MakeSave("Toughness", GetSave(base.Level), null, null, "Stinger Injected Confusion Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, StingerObject) && Defender.FireEvent("ApplyPoison") && ApplyEffectEvent.Check(Defender, "Poison", effect2, ParentObject) && Defender.FireEvent("ApplyAttackConfusion"))
			{
				if (Defender.ApplyEffect(effect2, ParentObject))
				{
					IComponent<GameObject>.XDidY(Defender, "become", "confused", null, null, null, Defender);
				}
			}
			else if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of your venom", null, null, Defender);
			}
			else if (Defender.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of " + ParentObject.poss("venom"), "!", null, Defender);
			}
		}
		if (VenomType == "Poison" && Defender.HasStat("Toughness") && Defender.FireEvent("CanApplyPoison") && CanApplyEffectEvent.Check(Defender, "Poison"))
		{
			Effect e = new StingerPoisoned(Duration.RollCached(), Increment, base.Level, ParentObject);
			if (!Defender.MakeSave("Toughness", GetSave(base.Level), null, null, "Stinger Injected Damaging Poison", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, StingerObject))
			{
				if (!Defender.ApplyEffect(e, ParentObject))
				{
				}
			}
			else if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of your venom", null, null, Defender);
			}
			else if (Defender.IsPlayer())
			{
				IComponent<GameObject>.XDidY(Defender, "resist", "the effects of " + ParentObject.poss("venom"), "!", null, Defender);
			}
		}
		Defender.Splatter("&g.");
	}

	public int GetSave(int Level)
	{
		return 14 + Level * 2;
	}

	public void GetData(int Level, out int Penetration, out string BaseDamage, out string Duration, out string Increment)
	{
		Duration = "";
		Increment = "";
		if (Level < 4)
		{
			BaseDamage = "1d6";
		}
		else if (Level < 7)
		{
			BaseDamage = "1d8";
		}
		else if (Level < 10)
		{
			BaseDamage = "1d10";
		}
		else if (Level < 13)
		{
			BaseDamage = "1d12";
		}
		else if (Level < 16)
		{
			BaseDamage = "2d6";
		}
		else if (Level < 19)
		{
			BaseDamage = "2d6+1";
		}
		else
		{
			BaseDamage = "2d8";
		}
		if (Level < 2)
		{
			Penetration = 3;
		}
		else
		{
			Penetration = Math.Min(9, (Level - 2) / 3 + 4);
		}
		if (VenomType == "Paralyze")
		{
			if (Level < 3)
			{
				Duration = "1d3+1";
			}
			else
			{
				Duration = "1d3+" + Math.Min(7, Level / 3 + 1);
			}
		}
		if (VenomType == "Confuse")
		{
			if (Level < 3)
			{
				Duration = "2d3+2";
			}
			else
			{
				Duration = "2d3+" + Math.Min(14, Level * 2 / 3 + 2);
			}
		}
		if (VenomType == "Poison")
		{
			Duration = "8-12";
			Increment = Level + "d2";
		}
	}

	public override string GetLevelText(int Level)
	{
		string BaseDamage = "";
		GetData(Level, out var Penetration, out BaseDamage, out var Duration, out var Increment);
		string text = "";
		text += "20% chance on melee attack to sting your opponent (";
		text = text + "{{c|\u001a}}{{rules|" + (Penetration + 4) + "}} {{r|\u0003}}{{rules|" + BaseDamage + "}})\n";
		text += "Stinger is a long blade and can only penetrate once.\n";
		text += "Always sting on charge or lunge.\n";
		text += "Stinger applies venom on damage (only 20% chance if Stinger is your primary weapon).\n";
		text += "May use Sting activated ability to strike with your stinger and automatically hit and penetrate.\n";
		text += "Sting cooldown: 25\n";
		if (VenomType == "Paralyze")
		{
			text = text + "Venom paralyzes opponents for {{rules|" + Duration + "}} rounds\n";
		}
		if (VenomType == "Confuse")
		{
			text = text + "Venom confuses opponents for {{rules|" + Duration + "}} rounds\n";
		}
		if (VenomType == "Poison")
		{
			text = text + "Venom poisons opponents for {{rules|" + Duration + "}} rounds (damage increment {{rules|" + Increment + "}})\n";
		}
		return text + "+200 reputation with {{w|arachnids}}";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		if (VenomType == "Paralyze")
		{
			DisplayName = "Stinger (Paralyzing Venom)";
		}
		if (VenomType == "Confuse")
		{
			DisplayName = "Stinger (Confusing Venom)";
		}
		if (VenomType == "Poison")
		{
			DisplayName = "Stinger (Poisoning Venom)";
		}
		return base.ChangeLevel(NewLevel);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		BodyPart partByManager = body.GetPartByManager(ManagerID);
		if (partByManager != null)
		{
			AddStingerTo(partByManager);
		}
	}

	public BodyPart AddTail(GameObject GO, bool DoUpdate = true)
	{
		BodyPart bodyPart = GO?.Body?.GetBody();
		if (bodyPart == null)
		{
			return null;
		}
		BodyPart firstAttachedPart = bodyPart.GetFirstAttachedPart("Tail", 0, GO.Body, EvenIfDismembered: true);
		if (firstAttachedPart != null)
		{
			firstAttachedPart.ChangeLaterality(2);
			BodyPart bodyPart2 = firstAttachedPart;
			if (bodyPart2.Manager == null)
			{
				bodyPart2.Manager = LateralityID;
			}
			int? category = bodyPart.Category;
			string managerID = ManagerID;
			return bodyPart.AddPartAt(firstAttachedPart, "Tail", 1, null, null, null, null, managerID, category, null, null, null, null, null, null, null, null, null, null, null, DoUpdate);
		}
		return bodyPart.AddPartAt("Tail", 0, null, null, null, null, Category: bodyPart.Category, Manager: ManagerID, RequiresLaterality: null, Mobility: null, Appendage: null, Integral: null, Mortal: null, Abstract: null, Extrinsic: null, Plural: null, Mass: null, Contact: null, IgnorePosition: null, InsertAfter: "Feet", OrInsertBefore: new string[3] { "Roots", "Thrown Weapon", "Floating Nearby" }, DoUpdate: DoUpdate);
	}

	public void AddStingerTo(BodyPart part)
	{
		if (part.Equipped != null)
		{
			part.ForceUnequip(Silent: true);
		}
		if (StingerObject == null)
		{
			StingerObject = GameObject.create("Stinger");
		}
		if (StingerObject != null)
		{
			GetData(base.Level, out var Penetration, out var BaseDamage, out var _, out var _);
			MeleeWeapon part2 = StingerObject.GetPart<MeleeWeapon>();
			if (part2 != null)
			{
				part2.BaseDamage = BaseDamage;
				part2.PenBonus = Penetration;
				part2.Slot = part.Type;
			}
			if (!part.Equip(StingerObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
			{
				CleanUpMutationEquipment(ParentObject, ref StingerObject);
			}
		}
		else
		{
			Debug.LogError("Could not create Stinger");
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (ParentObject.Body != null)
		{
			BodyPart bodyPart = AddTail(GO);
			if (bodyPart != null)
			{
				AddStingerTo(bodyPart);
				StingActivatedAbilityID = AddMyActivatedAbility("Sting", "CommandSting", "Physical Mutation", null, "\u009f");
			}
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref StingActivatedAbilityID);
		CleanUpMutationEquipment(GO, ref StingerObject);
		GO.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		foreach (BodyPart item in GO.GetBodyPartsByManager(LateralityID))
		{
			if (item.Laterality == 2 && item.IsLateralityConsistent())
			{
				item.ChangeLaterality(0);
			}
		}
		return base.Unmutate(GO);
	}
}
