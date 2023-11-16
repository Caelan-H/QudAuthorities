using System;
using System.Collections.Generic;
using XRL.Messages;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Backswing : BaseSkill
{
	[NonSerialized]
	private List<GameObject> WeaponsUsed = new List<GameObject>(1);

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AttackerAfterAttack");
		Object.RegisterPartEvent(this, "AttackerMeleeMiss");
		Object.RegisterPartEvent(this, "EndSegment");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndSegment")
		{
			if (WeaponsUsed == null)
			{
				WeaponsUsed = new List<GameObject>(1);
			}
			WeaponsUsed.Clear();
			return true;
		}
		if (E.ID == "AttackerAfterAttack" || E.ID == "AttackerMeleeMiss")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject obj = E.GetGameObjectParameter("Weapon");
			GameObject obj2 = E.GetGameObjectParameter("Defender");
			if (GameObject.validate(ref obj) && GameObject.validate(ref obj2) && obj2.HasHitpoints() && !obj2.IsInGraveyard())
			{
				if (WeaponsUsed == null)
				{
					WeaponsUsed = new List<GameObject>(1);
				}
				if (!WeaponsUsed.CleanContains(obj) && obj.IsEquippedOrDefaultOfPrimary(gameObjectParameter))
				{
					WeaponsUsed.Add(obj);
					if (obj.GetPart("MeleeWeapon") is MeleeWeapon meleeWeapon && meleeWeapon.Skill.Contains("Cudgel") && GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, obj, "Skill Backswing", 25, obj2).in100())
					{
						if (ParentObject.IsPlayer())
						{
							MessageQueue.AddPlayerMessage("You backswing with " + ParentObject.its_(obj) + ".", 'g');
						}
						if (obj2.IsPlayer())
						{
							MessageQueue.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.ShortDisplayName + gameObjectParameter.GetVerb("backswing") + " with " + gameObjectParameter.its_(obj) + ".", 'r');
						}
						Event @event = Event.New("MeleeAttackWithWeapon");
						@event.AddParameter("Attacker", gameObjectParameter);
						@event.AddParameter("Defender", obj2);
						@event.AddParameter("Weapon", obj);
						gameObjectParameter.FireEvent(@event);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
