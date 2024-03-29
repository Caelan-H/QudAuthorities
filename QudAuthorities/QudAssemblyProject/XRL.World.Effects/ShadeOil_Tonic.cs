using System;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class ShadeOil_Tonic : ITonicEffect
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public ShadeOil_Tonic()
	{
	}

	public ShadeOil_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return base.GetEffectType() | 0x100;
	}

	public override string GetDescription()
	{
		return "{{shade|shade oil}} tonic";
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "+8 DV\nCan phase out of the spatial dimension for 7-9 turns.";
		}
		return "+8 DV\n25% chance per turn to phase out of the spatial dimension for 1-3 turns.";
	}

	public override bool Apply(GameObject Object)
	{
		DidX("begin", "to flicker in and out of corporeality", null, null, Object, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
		ApplyChanges();
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		DidX("assume", "full corporeal form again");
		UnapplyChanges();
		base.Remove(Object);
	}

	private void ApplyChanges(bool Silent = false)
	{
		base.StatShifter.SetStatShift(base.Object, "DV", 8);
		if (base.Object.IsTrueKin())
		{
			ActivatedAbilityID = base.Object.AddActivatedAbility("Phase", "ShadeOilPhase", "Tonics", null, "°", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, Silent);
		}
	}

	private void UnapplyChanges()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
		base.Object.RemoveActivatedAbility(ref ActivatedAbilityID);
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
		if (base.Duration > 0 && !base.Object.IsTrueKin() && !base.Object.OnWorldMap() && !base.Object.HasEffect("Phased") && 25.in100())
		{
			base.Object.ApplyEffect(new Phased(Stat.Random(1, 3)));
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "Overdose");
		Object.RegisterEffectEvent(this, "ShadeOilPhase");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "Overdose");
		Object.UnregisterEffectEvent(this, "ShadeOilPhase");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ShadeOilPhase")
		{
			if (base.Object.OnWorldMap())
			{
				if (base.Object.IsPlayer())
				{
					Popup.ShowFail("You cannot do that on the world map.");
				}
			}
			else
			{
				base.Object.RemoveActivatedAbility(ref ActivatedAbilityID);
				base.Object.ApplyEffect(new Phased(Stat.Random(7, 9)));
			}
		}
		else if (E.ID == "TonicAutoApplied")
		{
			if (base.Duration > 0 && !base.Object.HasEffect("Phased"))
			{
				if (base.Object.IsTrueKin())
				{
					StringBuilder stringBuilder = Event.NewStringBuilder();
					GameObject gameObjectParameter = E.GetGameObjectParameter("By");
					stringBuilder.Append(ColorUtility.CapitalizeExceptFormatting(GetDescription())).Append(" has been applied");
					if (gameObjectParameter != null)
					{
						stringBuilder.Append(" by ");
						if (!gameObjectParameter.HasProperName)
						{
							if (gameObjectParameter.GetObjectContext() == base.Object)
							{
								stringBuilder.Append("your ");
							}
							else
							{
								stringBuilder.Append(gameObjectParameter.a);
							}
						}
						stringBuilder.Append(gameObjectParameter.ShortDisplayName);
					}
					stringBuilder.Append(". Do you wish to phase out immediately?");
					if (Popup.ShowYesNo(stringBuilder.ToString(), AllowEscape: false) == DialogResult.Yes)
					{
						FireEvent(Event.New("ShadeOilPhase"));
					}
				}
				else if (25.in100())
				{
					base.Object.ApplyEffect(new Phased(Stat.Random(1, 3)));
				}
			}
		}
		else if (E.ID == "Overdose")
		{
			if (base.Duration > 0 && !base.Object.HasProperty("EvilTwin"))
			{
				base.Duration = 0;
				ApplyOverdose(base.Object);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
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
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You flicker through spacetime uncontrollably.");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You flicker through spacetime uncontrollably.");
			}
		}
		int i = 0;
		for (int num = Stat.Random(1, 3); i < num; i++)
		{
			EvilTwin.CreateEvilTwin(Object, "Shadow");
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 0 && num < 10)
		{
			E.Tile = null;
			E.RenderString = "@";
			E.ColorString = "&K";
		}
		return true;
	}
}
