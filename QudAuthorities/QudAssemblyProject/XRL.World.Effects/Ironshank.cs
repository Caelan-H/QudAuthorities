using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Ironshank : Effect
{
	public int Count;

	public int Penalty;

	public int AVBonus;

	public bool DrankCure;

	public Ironshank()
	{
		base.DisplayName = "ironshank";
	}

	public override string GetDetails()
	{
		return "Leg bones are fusing at the joints.\n{{C|-" + Penalty + "}} Move Speed\n{{C|+" + AVBonus + "}} AV\nWill continue to lose Move Speed and gain AV until Move Speed penalty reaches -80.";
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public void SetPenalty(int Amount)
	{
		if (Amount < 0)
		{
			Amount = 0;
		}
		base.StatShifter.SetStatShift("MoveSpeed", Amount);
		AVBonus = Amount / 15;
		base.StatShifter.SetStatShift("AV", AVBonus);
		Penalty = Amount;
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Ironshank") || Object.HasEffect("IronshankOnset"))
		{
			return false;
		}
		if (!IsInfectable(Object))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyDisease"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Disease", this))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyIronshank"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "Ironshank", this))
		{
			return false;
		}
		base.Duration = 1;
		SetPenalty(Penalty + Stat.Random(6, 10));
		if (Object.IsPlayer())
		{
			AchievementManager.SetAchievement("ACH_GET_IRONSHANK");
			Popup.Show("You have contracted Ironshank! You feel the cartilage stretch as your leg bones grind together at the joints.");
			JournalAPI.AddAccomplishment("You contracted ironshank.", "Woe to the scroundrels and dastards who conspired to have =name= contract Stiff Leg!", "general", JournalAccomplishment.MuralCategory.BodyExperienceBad, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.StatShifter.RemoveStatShifts();
		base.Remove(Object);
	}

	public override string GetDescription()
	{
		return "{{ironshank|ironshank}}";
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "DrinkingFrom");
		Object.RegisterEffectEvent(this, "Eating");
		Object.RegisterEffectEvent(this, "EndTurn");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "DrinkingFrom");
		Object.UnregisterEffectEvent(this, "Eating");
		Object.UnregisterEffectEvent(this, "EndTurn");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			if (base.Duration > 0 && !IsInfectable(base.Object))
			{
				base.Duration = 0;
			}
			if (base.Duration > 0)
			{
				Count++;
				if (Count >= 1200 && DrankCure)
				{
					Count = 0;
					DrankCure = false;
					if (base.Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("The pain in your joints subsides.");
					}
					SetPenalty(Penalty - Stat.Random(6, 10));
					if (Penalty <= 0)
					{
						base.Duration = 0;
						if (base.Object.IsPlayer())
						{
							AchievementManager.SetAchievement("ACH_CURE_IRONSHANK");
							Popup.Show("You are cured of ironshank.");
						}
					}
				}
				else if (Count >= 4800)
				{
					Count = 0;
					if (Penalty < 80)
					{
						int num = Stat.Random(6, 10);
						if (Penalty + num >= 80)
						{
							SetPenalty(80);
							if (base.Object.IsPlayer())
							{
								Popup.Show("Your legs bones are nearly fused at the joints.");
							}
						}
						else
						{
							if (base.Object.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("You feel the cartilage stretch as your leg bones grind together at the joints.", 'R');
							}
							SetPenalty(Penalty + num);
						}
					}
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = E.GetGameObjectParameter("Container").LiquidVolume;
			if (liquidVolume.ContainsLiquid(The.Game.GetStringGameState("IronshankCure")) && liquidVolume.ContainsLiquid("gel"))
			{
				DrankCure = true;
			}
		}
		return base.FireEvent(E);
	}

	public static bool IsInfectable(GameObject who)
	{
		return who.HasBodyPart(IsInfectableLimb);
	}

	public static bool IsInfectableLimb(BodyPart Part)
	{
		if (Part.Type != "Feet")
		{
			return false;
		}
		if (!Part.IsCategoryLive())
		{
			return false;
		}
		if (Part.Mobility <= 0)
		{
			return false;
		}
		return true;
	}
}
