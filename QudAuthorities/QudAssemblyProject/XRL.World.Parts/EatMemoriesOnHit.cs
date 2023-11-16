using System;
using Qud.API;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class EatMemoriesOnHit : IPart
{
	public int Chance = 100;

	public string MemoriesLost = "1d2";

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "WeaponHit");
		Object.RegisterPartEvent(this, "WeaponAfterAttack");
		base.Register(Object);
	}

	public static bool CheckEatMemories(GameObject attacker, GameObject defender, GameObject weapon, int chance)
	{
		if (defender != null && defender.IsPlayer())
		{
			if (GetSpecialEffectChanceEvent.GetFor(attacker, weapon, "Part EatMemoriesOnHit Activation", chance, defender).in100() && Stat.rollMentalAttackPenetrations(attacker, defender) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void EatMemories(GameObject attacker, GameObject defender, GameObject weapon, string memoriesLost)
	{
		GameManager.Instance.Fuzzing = true;
		defender.CurrentZone.GetExploredCells().ForEach(delegate(Cell c)
		{
			c.SetExplored(State: false);
		});
		defender.ParticlePulse("&K?");
		int i = 0;
		for (int num = Stat.Roll(memoriesLost); i < num; i++)
		{
			JournalAPI.GetRandomRevealedNote((IBaseJournalEntry c) => c.Forgettable())?.Forget();
		}
		IComponent<GameObject>.AddPlayerMessage("You forget something.");
		GenericDeepNotifyEvent.Send(defender, "MemoriesEaten", defender, weapon);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject obj = E.GetGameObjectParameter("Defender");
			if (GameObject.validate(ref obj) && CheckEatMemories(ParentObject.equippedOrSelf(), obj, ParentObject, Chance))
			{
				E.SetParameter("DidSpecialEffect", 1);
				string text = E.GetStringParameter("Properties", "") ?? "";
				if (!text.Contains("AteMemories"))
				{
					E.SetParameter("Properties", (text == "") ? "AteMemories" : (text + ",AteMemories"));
				}
			}
		}
		else if (E.ID == "WeaponAfterAttack")
		{
			GameObject obj2 = E.GetGameObjectParameter("Attacker");
			GameObject obj3 = E.GetGameObjectParameter("Defender");
			GameObject obj4 = E.GetGameObjectParameter("Weapon");
			if (GameObject.validate(ref obj2) && GameObject.validate(ref obj3) && GameObject.validate(ref obj4) && (E.GetStringParameter("Properties", "") ?? "").Contains("AteMemories"))
			{
				EatMemories(obj2, obj3, obj4, MemoriesLost);
			}
		}
		return base.FireEvent(E);
	}
}
