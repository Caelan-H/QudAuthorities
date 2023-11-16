using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class SphynxSalt_Tonic : ITonicEffect
{
	public bool WasPlayer;

	public int HitpointsAtSave;

	public int TemperatureAtSave;

	private long ActivatedSegment;

	public SphynxSalt_Tonic()
	{
	}

	public SphynxSalt_Tonic(int _Duration)
		: this()
	{
		base.Duration = _Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "{{sphynx|sphynx}} {{Y|salt}} tonic";
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "Immune to confusing attacks.\nCan peer into the near future.";
		}
		return "Immune to confusing attacks.\nActivated mental mutations cool down twice as quickly.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("The clouds part in your mind and a ray of clarity strikes through.");
		}
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		int num = 0;
		while (Object.HasEffect("Confused") && ++num < 20)
		{
			Object.RemoveEffect("Confused");
		}
		if (Object.GetEffect("SphynxSalt_Tonic") is SphynxSalt_Tonic sphynxSalt_Tonic)
		{
			sphynxSalt_Tonic.Duration += base.Duration;
			return false;
		}
		if (Object.IsPlayer())
		{
			WasPlayer = true;
			if (Object.IsTrueKin())
			{
				Precognition.Save();
			}
		}
		else
		{
			WasPlayer = false;
			if (SensePsychic.SensePsychicFromPlayer(Object) != null)
			{
				IComponent<GameObject>.AddPlayerMessage("You sense a subtle psychic disturbance.");
			}
		}
		HitpointsAtSave = Object.hitpoints;
		TemperatureAtSave = Object.pPhysics.Temperature;
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.IsTrueKin() && WasPlayer)
			{
				if (Popup.ShowYesNo("Your {{sphynx|sphynx}} {{Y|salt}} is about to run out. Would you like to return to the start of your vision?") == DialogResult.Yes)
				{
					AutoAct.Interrupt();
					Precognition.Load(Object);
				}
			}
			else
			{
				Popup.Show("Your mind clouds over once again.");
			}
		}
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Duration > 0)
		{
			ActivatedAbilities activatedAbilities = base.Object.ActivatedAbilities;
			if (activatedAbilities?.AbilityByGuid != null)
			{
				foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
				{
					if (value.Class.Contains("Mental"))
					{
						if (value.Cooldown > 10)
						{
							value.SetCooldown(value.Cooldown - 10);
						}
						else if (value.Cooldown > 0)
						{
							value.SetCooldown(0);
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyAttackConfusion");
		Object.RegisterEffectEvent(this, "Overdose");
		Object.RegisterEffectEvent(this, "BeforeDie");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "ApplyAttackConfusion");
		Object.UnregisterEffectEvent(this, "Overdose");
		Object.UnregisterEffectEvent(this, "BeforeDie");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyAttackConfusion")
		{
			return false;
		}
		if (E.ID == "BeforeDie" && base.Duration > 0 && base.Object.IsTrueKin())
		{
			return Precognition.OnBeforeDie(base.Object, Guid.Empty, ref _Duration, ref HitpointsAtSave, ref TemperatureAtSave, ref ActivatedSegment, WasPlayer, RealityDistortionBased: true);
		}
		if (E.ID == "Overdose")
		{
			if (base.Duration > 0)
			{
				base.Duration = 0;
				ApplyOverdose(base.Object);
			}
		}
		else if (E.ID == "GameRestored")
		{
			GenericDeepNotifyEvent.Send(base.Object, "PrecognitionGameRestored");
		}
		return base.FireEvent(E);
	}

	public override void ApplyAllergy(GameObject Object)
	{
		ApplyOverdose(Object);
	}

	public static void ApplyOverdose(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			if (Object.GetLongProperty("Overdosing") == 1)
			{
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You cannot see to see -- your mind cracks as a bell struck by a hammer.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You cannot see to see -- your mind cracks as a bell struck by a hammer.");
			}
		}
		if (!Object.HasPart("ActivatedAbilities") || !(Object.GetPart("ActivatedAbilities") is ActivatedAbilities activatedAbilities) || activatedAbilities.AbilityByGuid == null)
		{
			return;
		}
		foreach (ActivatedAbilityEntry value in activatedAbilities.AbilityByGuid.Values)
		{
			if (value.Class.Contains("Mental") && value.Cooldown <= 2000 && value.Cooldown >= -1)
			{
				value.SetCooldown(2021);
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 30 && num < 40)
		{
			E.Tile = null;
			E.RenderString = "ì";
			E.ColorString = "&C";
		}
		return true;
	}
}
