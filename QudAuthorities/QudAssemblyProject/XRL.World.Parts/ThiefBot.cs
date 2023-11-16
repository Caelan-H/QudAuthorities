using System;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class ThiefBot : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIBeginKill");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIBeginKill")
		{
			if (ParentObject.HasPart("Inventory") && ParentObject.Inventory.Objects.Count > 0)
			{
				Debug.Log("I should drop off my inventory");
				ParentObject.pBrain.PushGoal(new DropOffStolenGoods());
			}
			else
			{
				GameObject target = ParentObject.Target;
				if (target != null && ParentObject.DistanceTo(target) <= 1 && (ParentObject.IsFlying || !target.IsFlying))
				{
					if (ParentObject.pBrain != null)
					{
						ParentObject.pBrain.Think("I'm going to steal something!");
					}
					ParentObject.UseEnergy(1000);
					if (!ParentObject.PhaseMatches(target))
					{
						if (target.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName)) + " pincers pass through you harmlessly.", 'G');
						}
					}
					else if (target.MakeSave("Agility", 10, ParentObject, "Agility", "Disarm"))
					{
						int num = Stat.Random(1, 100);
						GameObject gameObject = null;
						if (num <= 25)
						{
							BodyPart randomElement = (from p in target.Body.GetEquippedParts()
								where p.Equipped.FireEvent("CanBeUnequipped")
								select p).GetRandomElement();
							if (randomElement != null)
							{
								gameObject = randomElement.Equipped;
								gameObject.UnequipAndRemove();
								if (gameObject.Equipped != null)
								{
									gameObject = null;
								}
							}
						}
						if (num > 25 || gameObject == null)
						{
							gameObject = target.Inventory.Objects.GetRandomElement();
							if (gameObject != null)
							{
								target.Inventory.RemoveObject(gameObject);
								if (gameObject.InInventory != null)
								{
									gameObject = null;
								}
							}
						}
						if (gameObject != null)
						{
							if (target.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("snag") + " " + (gameObject.HasProperName ? "" : "your ") + gameObject.ShortDisplayName + "!", 'R');
							}
							ParentObject.Inventory.AddObject(gameObject);
							target.DustPuff();
						}
					}
					else if (target.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You avoid " + Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " pincers.", 'G');
					}
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
