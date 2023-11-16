using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.Pathfinding;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// <remarks>
///             overload behavior: the range, number of bounces, and damage roll are
///             increased by the standard power load bonus, i.e. 2 for the standard
///             overload power load of 400, and charge usage is adjusted using power
///             load as a percentage.
///             </remarks>
[Serializable]
public class GeomagneticDisc : IPart
{
	public string Damage = "2d6";

	public string ChargeUse = "400";

	public string Bounces = "5";

	public string Range = "12";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == IsOverloadableEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsOverloadableEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "BeforeThrown");
		Object.RegisterPartEvent(this, "GetThrownWeaponPerformance");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GetThrownWeaponPerformance")
		{
			if (!IsBroken() && !IsRusted() && !IsEMPed())
			{
				int num = MyPowerLoadLevel();
				int effectiveChargeUse = GetEffectiveChargeUse(num);
				if (ParentObject.TestCharge(effectiveChargeUse, LiveOnly: false, 0L))
				{
					string text = Damage;
					int num2 = IComponent<GameObject>.PowerLoadBonus(num);
					if (num2 != 0)
					{
						text = DieRoll.AdjustResult(text, num2);
					}
					E.SetParameter("Vorpal", 1);
					E.SetParameter("Damage", text);
				}
			}
		}
		else if (E.ID == "BeforeThrown")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Thrower");
			if (gameObjectParameter == null)
			{
				return false;
			}
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("ApparentTarget");
			if (gameObjectParameter2 == null)
			{
				return gameObjectParameter.IsPlayer();
			}
			if (IsBroken() || IsRusted() || IsEMPed())
			{
				return true;
			}
			int num3 = MyPowerLoadLevel();
			int effectiveChargeUse2 = GetEffectiveChargeUse(num3);
			if (!ParentObject.UseCharge(effectiveChargeUse2, LiveOnly: false, 0L))
			{
				return true;
			}
			int num4 = IComponent<GameObject>.PowerLoadBonus(num3);
			if (gameObjectParameter.IsPlayer())
			{
				gameObjectParameter.Target = gameObjectParameter2;
			}
			int num5 = E.GetIntParameter("Phase");
			if (num5 == 0)
			{
				num5 = Phase.getWeaponPhase(gameObjectParameter, GetActivationPhaseEvent.GetFor(ParentObject));
			}
			gameObjectParameter.UseEnergy(1000, "Physical Skill Throwing Item");
			Cell cell = gameObjectParameter2.CurrentCell;
			GameObject parentObject = ParentObject;
			Cell targetCell = cell;
			int radius = gameObjectParameter.GetBaseThrowRange(parentObject, gameObjectParameter2, targetCell, gameObjectParameter.DistanceTo(cell)) + Range.RollCached() + num4;
			int num6 = Bounces.RollCached() + num4;
			List<GameObject> list = Event.NewGameObjectList();
			list.Add(gameObjectParameter2);
			for (int i = 0; i < num6 && i < list.Count; i++)
			{
				List<GameObject> list2 = list[i].CurrentCell.FastFloodVisibility("Brain", radius);
				if (list2.Count <= 0)
				{
					continue;
				}
				list2.ShuffleInPlace();
				foreach (GameObject item in list2)
				{
					if (gameObjectParameter.IsHostileTowards(item) && item != gameObjectParameter && item.PhaseMatches(num5))
					{
						list.Add(item);
						break;
					}
				}
			}
			try
			{
				if (gameObjectParameter.CurrentZone.IsActive())
				{
					TextConsole textConsole = Look._TextConsole;
					ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
					TextConsole.ScrapBuffer2.Copy(TextConsole.CurrentBuffer);
					XRLCore.Core.RenderMapToBuffer(scrapBuffer);
					List<GameObject> list3 = Event.NewGameObjectList();
					list3.AddRange(list);
					list3.Insert(0, gameObjectParameter);
					for (int j = 0; j < list3.Count; j++)
					{
						GameObject gameObject = null;
						FindPath findPath = new FindPath(C2: ((j != list3.Count - 1) ? list3[j + 1] : gameObjectParameter).CurrentCell, C1: list3[j].CurrentCell, PathGlobal: false, PathUnlimited: true, Looker: ParentObject);
						for (int k = 0; k < findPath.Steps.Count - 1; k++)
						{
							if (k == findPath.Steps.Count - 1)
							{
								scrapBuffer.Copy(TextConsole.ScrapBuffer2);
								scrapBuffer.Goto(findPath.Steps[k].X, findPath.Steps[k].Y);
								scrapBuffer.Write("&RX");
							}
							else
							{
								scrapBuffer.Copy(TextConsole.ScrapBuffer2);
								scrapBuffer.Goto(findPath.Steps[k].X, findPath.Steps[k].Y);
								scrapBuffer.Write("&M\u000f");
							}
							textConsole.DrawBuffer(scrapBuffer);
							textConsole.WaitFrame();
						}
					}
				}
			}
			catch
			{
			}
			string text2 = ParentObject.the + ParentObject.ShortDisplayName;
			string value = "from " + text2 + "!";
			for (int l = 0; l < list.Count; l++)
			{
				int num7 = Stat.Random(1, 20);
				num7 = ((!gameObjectParameter.IsPlayer()) ? (num7 + Math.Max(gameObjectParameter.StatMod("Agility"), gameObjectParameter.StatMod("Strength"))) : (num7 + gameObjectParameter.StatMod("Agility")));
				int num8 = 0;
				if (list[l].HasStat("DV"))
				{
					num8 = Stats.GetCombatDV(list[l]);
				}
				if (num7 > num8)
				{
					bool defenderIsCreature = list[l].HasTag("Creature");
					string blueprint = list[l].Blueprint;
					WeaponUsageTracking.TrackThrownWeaponHit(gameObjectParameter, ParentObject, defenderIsCreature, blueprint, Accidental: false);
					list[l].ParticleBlip("&C\u0003");
					Damage damage = new Damage(Damage.RollCached() + num4);
					damage.AddAttribute("Concussion");
					damage.AddAttribute("Bludgeoning");
					damage.AddAttribute("Cudgel");
					damage.AddAttribute("Crushing");
					Event @event = Event.New("WeaponPseudoThrowHit");
					@event.SetParameter("Damage", damage);
					@event.SetParameter("Owner", gameObjectParameter);
					@event.SetParameter("Attacker", gameObjectParameter);
					@event.SetParameter("Defender", list[l]);
					@event.SetParameter("Weapon", ParentObject);
					@event.SetParameter("Projectile", ParentObject);
					@event.SetParameter("ApparentTarget", gameObjectParameter2);
					ParentObject.FireEvent(@event);
					Event event2 = Event.New("TakeDamage");
					event2.SetParameter("Damage", damage);
					event2.SetParameter("Owner", gameObjectParameter);
					event2.SetParameter("Attacker", gameObjectParameter);
					event2.SetParameter("Message", value);
					event2.SetParameter("Weapon", ParentObject);
					event2.SetParameter("Projectile", ParentObject);
					list[l].FireEvent(event2);
					WeaponUsageTracking.TrackThrownWeaponDamage(gameObjectParameter, ParentObject, defenderIsCreature, blueprint, Accidental: false, damage);
				}
				else
				{
					list[l].ParticleBlip("&K\t");
					IComponent<GameObject>.XDidY(list[l], "flinch", "out of the way of " + text2, "!", null, list[l]);
				}
			}
			return false;
		}
		return base.FireEvent(E);
	}

	public int GetEffectiveChargeUse()
	{
		int num = ChargeUse.RollMinCached();
		int num2 = MyPowerLoadLevel();
		if (num2 != 100)
		{
			num = num * num2 / 100;
		}
		return num;
	}

	public int GetEffectiveChargeUse(int PowerLoad)
	{
		int num = ChargeUse.RollMinCached();
		if (PowerLoad != 100)
		{
			num = num * PowerLoad / 100;
		}
		return num;
	}
}
