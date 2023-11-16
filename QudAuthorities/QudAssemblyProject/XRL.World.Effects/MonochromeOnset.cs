using System;

namespace XRL.World.Effects;

[Serializable]
public class MonochromeOnset : Effect
{
	public int Stage;

	public int Bonus;

	public int Days;

	public int Count;

	public bool SawSore;

	public bool Mature;

	public MonochromeOnset()
	{
		base.DisplayName = "blurry vision";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Vision is blurred.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Monochrome") || Object.HasEffect("MonochromeOnset"))
		{
			return false;
		}
		if (Object.FireEvent("ApplyDiseaseOnset") && ApplyEffectEvent.Check(Object, "DiseaseOnset", this) && Object.FireEvent("ApplyMonochrome") && ApplyEffectEvent.Check(Object, "Monochrome", this))
		{
			base.Duration = 1;
			return true;
		}
		return false;
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override string GetDescription()
	{
		return "blurry vision";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ModifyDefendingSaveEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ModifyDefendingSaveEvent E)
	{
		if (Bonus != 0 && E.Vs == "Monochrome Disease Onset")
		{
			E.Roll += Bonus;
			if (E.Actual)
			{
				Bonus = 0;
			}
		}
		return base.HandleEvent(E);
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
			Count++;
			if (Count >= 1200)
			{
				Count = 0;
				Days++;
				if (base.Object.MakeSave("Toughness", 13, null, null, "Monochrome Disease Onset"))
				{
					Stage--;
					if (SawSore && Stage > -2 && base.Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You feel a bit better.");
					}
				}
				else
				{
					Stage++;
					if (Stage < 3)
					{
						if (base.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("Your vision blurs.");
						}
						SawSore = true;
					}
				}
				if (Stage <= -2)
				{
					if (SawSore && base.Object.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("Your vision clears up.");
					}
					base.Duration = 0;
					return true;
				}
				if (Stage >= 3 || Days >= 5)
				{
					base.Duration = 0;
					base.Object.ApplyEffect(new Monochrome());
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			if (!E.GetGameObjectParameter("Container").LiquidVolume.ComponentLiquids.ContainsKey("honey") && Bonus < 2)
			{
				Bonus = 2;
			}
		}
		else if (E.ID == "Eating")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Food");
			if (gameObjectParameter == null)
			{
				return true;
			}
			if (Bonus > 0)
			{
				return true;
			}
			if (gameObjectParameter.Blueprint.Contains("Yuckwheat") && Bonus < 2)
			{
				Bonus = 2;
			}
		}
		return base.FireEvent(E);
	}
}
