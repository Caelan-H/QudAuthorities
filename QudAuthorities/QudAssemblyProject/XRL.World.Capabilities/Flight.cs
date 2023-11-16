using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

public static class Flight
{
	[NonSerialized]
	private static int BestFlyingLevel;

	public static bool IsFlying(GameObject obj)
	{
		return obj?.IsFlying ?? false;
	}

	private static void CollectBestFlyingLevel(Effect RFX)
	{
		if (RFX is Flying flying && flying.Level > BestFlyingLevel)
		{
			BestFlyingLevel = flying.Level;
		}
	}

	public static int GetBestFlyingLevel(GameObject obj)
	{
		BestFlyingLevel = 0;
		obj.ForeachEffect("Flying", CollectBestFlyingLevel);
		return BestFlyingLevel;
	}

	public static void SuspendFlight(GameObject obj)
	{
		obj.ModIntProperty("SuspendFlight", 1);
	}

	public static void DesuspendFlight(GameObject obj)
	{
		obj.ModIntProperty("SuspendFlight", -1, RemoveIfZero: true);
	}

	public static bool EnvironmentAllowsFlight(Zone Z)
	{
		if (Z != null)
		{
			if (!Z.IsWorldMap())
			{
				return Z.GetZoneZ() <= 10;
			}
			return true;
		}
		return false;
	}

	public static bool EnvironmentAllowsFlight(Cell C)
	{
		if (C != null)
		{
			if (!C.HasObjectWithTag("FlyingWhitelistArea"))
			{
				return EnvironmentAllowsFlight(C.ParentZone);
			}
			return true;
		}
		return false;
	}

	public static bool EnvironmentAllowsFlight(GameObject GO)
	{
		if (GO.pPhysics != null)
		{
			return EnvironmentAllowsFlight(GO.pPhysics.CurrentCell);
		}
		return false;
	}

	private static IFlightSource FlightSource(GameObject Source)
	{
		return Source?.GetFirstFlightSourcePart();
	}

	private static IFlightSource FlightSource(GameObject Source, Predicate<IFlightSource> Filter)
	{
		return Source?.GetFirstFlightSourcePart(Filter);
	}

	private static bool HasActivatedAbilityID(IFlightSource FS)
	{
		return FS.FlightActivatedAbilityID != Guid.Empty;
	}

	public static bool IsAbilityUsable(IFlightSource FS, GameObject Object)
	{
		if (FS == null)
		{
			return false;
		}
		return Object.IsActivatedAbilityUsable(FS.FlightActivatedAbilityID);
	}

	public static bool IsAbilityUsable(GameObject Source, GameObject Object)
	{
		return IsAbilityUsable(FlightSource(Source, HasActivatedAbilityID), Object);
	}

	public static bool IsAbilityAIUsable(IFlightSource FS, GameObject Object)
	{
		if (FS == null)
		{
			return false;
		}
		return Object.IsActivatedAbilityAIUsable(FS.FlightActivatedAbilityID);
	}

	public static bool IsAbilityAIUsable(GameObject Source, GameObject Object)
	{
		return IsAbilityAIUsable(FlightSource(Source, HasActivatedAbilityID), Object);
	}

	public static bool AbilitySetup(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		string text = "Fly";
		if (FS.FlightSourceDescription != null)
		{
			text = text + " (" + FS.FlightSourceDescription + ")";
		}
		FS.FlightActivatedAbilityID = Object.AddActivatedAbility(text, FS.FlightEvent, FS.FlightActivatedAbilityClass, null, "\u009d", null, Toggleable: true, FS.FlightFlying, ActiveToggle: true);
		return true;
	}

	public static bool AbilityTeardown(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		IFlightSource flightSource = FS ?? FlightSource(Source, HasActivatedAbilityID);
		if (flightSource != null)
		{
			Guid ID = flightSource.FlightActivatedAbilityID;
			Object.RemoveActivatedAbility(ref ID);
			flightSource.FlightActivatedAbilityID = ID;
		}
		return true;
	}

	public static int GetMoveFallChance(GameObject Object, IFlightSource FS)
	{
		int num = ((FS == null) ? int.MaxValue : (FS.FlightBaseFallChance - FS.FlightLevel));
		foreach (Effect effect in Object.GetEffects("Flying"))
		{
			if (effect is Flying flying && flying.Source != null)
			{
				IFlightSource flightSource = FlightSource(flying.Source);
				int num2 = flightSource.FlightBaseFallChance - flightSource.FlightLevel;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (num != int.MaxValue)
		{
			return num;
		}
		return 0;
	}

	public static int GetMoveFallChance(GameObject Object, GameObject Source = null)
	{
		return GetMoveFallChance(Object, (Source == null) ? null : FlightSource(Source));
	}

	public static int GetSwoopFallChance(GameObject Object, IFlightSource FS)
	{
		int num = ((FS == null) ? int.MaxValue : (FS.FlightBaseFallChance * 4 - FS.FlightLevel));
		foreach (Effect effect in Object.GetEffects("Flying"))
		{
			if (effect is Flying flying && flying.Source != null)
			{
				IFlightSource flightSource = FlightSource(flying.Source);
				int num2 = flightSource.FlightBaseFallChance * 4 - flightSource.FlightLevel;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (num != int.MaxValue)
		{
			return num;
		}
		return 0;
	}

	public static int GetSwoopFallChance(GameObject Object, GameObject Source = null)
	{
		return GetSwoopFallChance(Object, (Source == null) ? null : FlightSource(Source));
	}

	public static bool MaintainFlight(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
		}
		if (FS == null)
		{
			return FailFlying(Source, Object);
		}
		if (FS.FlightFlying)
		{
			if (FS.FlightRequiresOngoingEffort)
			{
				if (!Object.CanMoveExtremities("Fly"))
				{
					return FailFlying(Source, Object);
				}
				PowerSwitch part = Object.GetPart<PowerSwitch>();
				if (part != null && !part.Active)
				{
					return FailFlying(Source, Object);
				}
			}
			if (!Object.OnWorldMap())
			{
				int num = FS.FlightBaseFallChance - FS.FlightLevel;
				if (num > 0 && Stat.Random(1, 200) <= num)
				{
					return FailFlying(Source, Object, FS);
				}
			}
		}
		return true;
	}

	public static bool CheckFlight(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return StopFlying(Source, Object);
			}
		}
		if (FS.FlightFlying && !Object.OnWorldMap() && !EnvironmentAllowsFlight(Object) && !Object.CurrentCell.HasObjectWithPart("StairsDown"))
		{
			return StopFlying(Source, Object, FS);
		}
		return true;
	}

	public static Flying GetFlyingEffectFromSource(GameObject Source, GameObject Object)
	{
		if (GameObject.validate(ref Source) && GameObject.validate(ref Object))
		{
			foreach (Effect effect in Object.GetEffects("Flying"))
			{
				if (effect is Flying flying && flying.Source == Source)
				{
					return flying;
				}
			}
		}
		return null;
	}

	public static bool StartFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (FS.FlightFlying)
		{
			return false;
		}
		if (!EnvironmentAllowsFlight(Object))
		{
			if (Object.IsPlayer())
			{
				Popup.Show("You can't fly underground!");
			}
			return false;
		}
		if (Object.HasEffect("Grounded"))
		{
			if (Object.IsPlayer())
			{
				Popup.Show("You're grounded!");
			}
			return false;
		}
		if (!Object.CanChangeMovementMode("Flying", ShowMessage: true))
		{
			return false;
		}
		if (Object.GetEffectCount("Flying") == 0)
		{
			if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You begin flying!");
			}
			else if (Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(Object.Does("begin") + " flying.");
			}
			Object.AddActivatedAbility("Swoop", "CommandSwoopAttack", "Maneuvers", "You swoop down to attack a target on the ground, using one turn to attack and one to return to the air. Your chance of crashing is " + GetSwoopFallChance(Object, FS) + "%, and is doubled if your attack is shield blocked.", "û");
			Object.MovementModeChanged("Flying");
		}
		else if (Object.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("You begin using an additional flight capability.");
		}
		FS.FlightFlying = true;
		Object.ApplyEffect(new Flying(FS.FlightLevel, Source));
		Object.RemoveEffect("Prone");
		Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
		Object.FireEvent("FlightStarted");
		ObjectStartedFlyingEvent.SendFor(Object);
		return true;
	}

	private static void NoLongerFlying(GameObject Object)
	{
		ActivatedAbilities activatedAbilities = Object.ActivatedAbilities;
		if (activatedAbilities == null || !activatedAbilities.AbilityLists.ContainsKey("Maneuvers"))
		{
			return;
		}
		List<ActivatedAbilityEntry> list = new List<ActivatedAbilityEntry>(1);
		foreach (ActivatedAbilityEntry item in activatedAbilities.AbilityLists["Maneuvers"])
		{
			if (item.DisplayName == "Swoop" && item.Command == "CommandSwoopAttack")
			{
				list.Add(item);
			}
		}
		foreach (ActivatedAbilityEntry item2 in list)
		{
			activatedAbilities.RemoveAbility(item2.ID);
		}
	}

	public static bool StopFlying(GameObject Source, GameObject Object, IFlightSource FS = null, bool Silent = false, bool FromFail = false)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (!FS.FlightFlying)
		{
			return false;
		}
		if (Object == null)
		{
			FS.FlightFlying = false;
			return false;
		}
		int effectCount = Object.GetEffectCount("Flying");
		Object.GetCurrentCell();
		if (!Silent)
		{
			if (effectCount <= 1)
			{
				if (Object.IsPlayer())
				{
					MessageQueue.AddPlayerMessage("You return to the ground.");
				}
				else if (Object.IsVisible())
				{
					MessageQueue.AddPlayerMessage(Object.Does("return") + " to the ground.");
				}
			}
			else if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You cease using one of your flight capabilities.");
			}
		}
		FS.FlightFlying = false;
		if (!Object.RemoveEffect("Flying", (Effect FX) => (FX as Flying).Source == Source))
		{
			Object.RemoveEffect("Flying");
		}
		Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
		Object.FireEvent("FlightStoppedFromOneSource");
		if (effectCount <= 1)
		{
			Object.FireEvent("FlightStopped");
			if (!FromFail)
			{
				NoLongerFlying(Object);
				Object.MovementModeChanged("NotFlying");
			}
			ObjectStoppedFlyingEvent.SendFor(Object);
		}
		Object.Gravitate();
		return true;
	}

	public static bool FailFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return false;
			}
		}
		if (!FS.FlightFlying)
		{
			return false;
		}
		if (Object == null)
		{
			FS.FlightFlying = false;
			return false;
		}
		Object.StopMoving();
		int effectCount = Object.GetEffectCount("Flying");
		if (effectCount <= 1)
		{
			if (Object.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("You fall to the ground!", 'R');
			}
			else if (Object.IsVisible())
			{
				MessageQueue.AddPlayerMessage(Object.Does("fall") + " to the ground.");
			}
		}
		else if (Object.IsPlayer())
		{
			MessageQueue.AddPlayerMessage("One of your flight capabilities fails.", 'R');
		}
		bool result = StopFlying(Source, Object, FS, Silent: true, FromFail: true);
		Object.CooldownActivatedAbility(FS.FlightActivatedAbilityID, 5, null, Involuntary: true);
		Object.FireEvent("FlightFailedFromOneSource");
		if (effectCount <= 1)
		{
			Object.FireEvent("FlightFailed");
			NoLongerFlying(Object);
			Object.MovementModeChanged("NotFlying", Involuntary: true);
			ObjectStoppedFlyingEvent.SendFor(Object);
			Object.ApplyEffect(new Prone());
		}
		return result;
	}

	public static void Fall(GameObject Object)
	{
		foreach (Effect effect in Object.GetEffects("Flying"))
		{
			Flying flying = effect as Flying;
			if (flying.Source != null)
			{
				FailFlying(flying.Source, Object);
			}
		}
	}

	public static void SyncFlying(GameObject Source, GameObject Object, IFlightSource FS = null)
	{
		if (Source == null || Object == null)
		{
			return;
		}
		if (FS == null)
		{
			FS = FlightSource(Source);
			if (FS == null)
			{
				return;
			}
		}
		bool flag = false;
		if (FS.FlightFlying)
		{
			flag = true;
			bool flag2 = false;
			foreach (Effect effect in Object.GetEffects("Flying"))
			{
				if ((effect as Flying)?.Source == Source)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				FS.FlightFlying = false;
				if (Object.IsActivatedAbilityToggledOn(FS.FlightActivatedAbilityID))
				{
					Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
				}
			}
		}
		else
		{
			foreach (Effect effect2 in Object.GetEffects("Flying"))
			{
				flag = true;
				Flying flying = effect2 as Flying;
				if (flying?.Source == Source)
				{
					Object.RemoveEffect(flying);
				}
			}
		}
		int effectCount = Object.GetEffectCount("Flying");
		if (flag && effectCount <= 0)
		{
			NoLongerFlying(Object);
		}
	}
}
