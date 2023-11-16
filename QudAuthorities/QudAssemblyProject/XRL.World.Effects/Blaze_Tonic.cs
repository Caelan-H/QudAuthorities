using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Blaze_Tonic : ITonicEffect
{
	public int x;

	public int y;

	public string zone;

	public Blaze_Tonic()
	{
	}

	public Blaze_Tonic(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		if (base.Object.IsTrueKin())
		{
			return "+100 Heat Resistance\nCan't be frozen.\n+25% heat damage dealt.\n-50 Cold Resistance\n+20 Quickness\nTemperature can't be increased by external heat sources.\nMust move around to avoid combusting and losing the effects of this tonic.";
		}
		return "+100 Heat Resistance.\nCan't be frozen.\n+25% heat damage dealt.\n-50 Cold Resistance\n+10 Quickness\nTemperature can't be increased by external heat sources.\nMust move around to avoid combusting and losing the effects of this tonic.";
	}

	public override string GetDescription()
	{
		return "{{blaze|blaze}} tonic";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsPlayer())
		{
			Popup.Show("Your stomach swells with a burning sensation.");
		}
		if (Object.pPhysics.Temperature <= Object.pPhysics.FreezeTemperature)
		{
			Object.pPhysics.Temperature = Object.pPhysics.FreezeTemperature + 1;
		}
		ApplyStats();
		if (Object.GetLongProperty("Overdosing") == 1)
		{
			FireEvent(Event.New("Overdose"));
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift("Speed", base.Object.IsTrueKin() ? 20 : 10);
		base.StatShifter.SetStatShift("ColdResistance", -50);
		base.StatShifter.SetStatShift("HeatResistance", 100);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeginTakeActionEvent.ID)
		{
			return ID == EndActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (base.Object.pPhysics.Temperature <= base.Object.pPhysics.FreezeTemperature)
		{
			base.Object.pPhysics.Temperature = base.Object.pPhysics.FreezeTemperature + 1;
		}
		if (base.Object.pPhysics.Temperature >= base.Object.pPhysics.FlameTemperature)
		{
			base.Object.RemoveEffect(this);
		}
		else if (base.Duration > 0)
		{
			if (base.Duration != 9999)
			{
				base.Duration--;
			}
			if (base.Duration <= 0)
			{
				if (base.Object.IsPlayer())
				{
					Popup.Show("You start to cool off.");
				}
				base.Object.RemoveEffect(this);
			}
			else if (base.Object.CurrentCell != null)
			{
				x = base.Object.CurrentCell.X;
				y = base.Object.CurrentCell.Y;
				zone = base.Object.CurrentZone?.ZoneID;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndActionEvent E)
	{
		if (base.Duration > 0)
		{
			Cell cell = base.Object.CurrentCell;
			if (cell != null)
			{
				if (x == cell.X && y == cell.Y && zone == cell.ParentZone.ZoneID)
				{
					base.Object.pPhysics.Temperature += 120;
				}
				else if (base.Object.pPhysics.Temperature > 45)
				{
					base.Object.pPhysics.Temperature -= 20;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "AttackerDealingDamage");
		Object.RegisterEffectEvent(this, "BeforeApplyDamage");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeTemperatureChange");
		Object.RegisterEffectEvent(this, "Overdose");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "AttackerDealingDamage");
		Object.UnregisterEffectEvent(this, "BeforeApplyDamage");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeTemperatureChange");
		Object.UnregisterEffectEvent(this, "Overdose");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerDealingDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			if (damage.HasAttribute("Heat") || damage.HasAttribute("Fire"))
			{
				damage.Amount = (int)Math.Ceiling((float)damage.Amount * 1.25f);
			}
		}
		else if (E.ID == "BeforeApplyDamage" || E.ID == "TonicAutoApplied")
		{
			if (E.GetParameter("Damage") is Damage damage2 && damage2.IsHeatDamage())
			{
				damage2.Amount = 0;
				return false;
			}
		}
		else if (E.ID == "BeforeTemperatureChange")
		{
			if (E.GetIntParameter("Amount") > 0)
			{
				return false;
			}
		}
		else if (E.ID == "Overdose")
		{
			if (base.Duration > 0)
			{
				base.Duration = 0;
				ApplyOverdose(base.Object);
			}
		}
		else if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
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
				Popup.Show("Your mutant physiology reacts adversely to the tonic. You erupt into flames!");
			}
			else
			{
				Popup.Show("The tonics you ingested react adversely to each other. You erupt into flames!");
			}
		}
		if (Object.pPhysics != null)
		{
			int num = Object.pPhysics.FlameTemperature + 200;
			if (Object.pPhysics.Temperature < num)
			{
				Object.pPhysics.Temperature = num;
			}
		}
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 60;
		if (base.Duration > 0 && num > 35 && num < 45)
		{
			E.Tile = null;
			E.RenderString = "!";
			switch (Stat.RandomCosmetic(1, 3))
			{
			case 1:
				E.ColorString = "&R";
				break;
			case 2:
				E.ColorString = "&W";
				break;
			case 3:
				E.ColorString = "&r";
				break;
			}
		}
		return true;
	}
}
