using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class IronshankOnset : Effect
{
	public int Stage;

	public int Bonus;

	public int Days;

	public int Count;

	public bool SawSore;

	public bool Mature;

	public IronshankOnset()
	{
		base.DisplayName = "stiff legs";
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 100679700;
	}

	public override string GetDetails()
	{
		return "Legs ache at the joints.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasStat("MoveSpeed"))
		{
			return false;
		}
		if (!Ironshank.IsInfectable(Object))
		{
			return false;
		}
		if (Object.HasEffect("Ironshank") || Object.HasEffect("IronshankOnset"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyDiseaseOnset"))
		{
			return false;
		}
		if (!ApplyEffectEvent.Check(Object, "DiseaseOnset", this))
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
		return true;
	}

	public override string GetDescription()
	{
		return "stiff legs";
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
		if (Bonus != 0 && E.Vs == "Ironshank Disease Onset")
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
			if (!Ironshank.IsInfectable(base.Object))
			{
				base.Duration = 0;
			}
			else
			{
				Count++;
				if (Count >= 1200)
				{
					Count = 0;
					Days++;
					if (base.Object.MakeSave("Toughness", 13, null, null, "Ironshank Disease Onset"))
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
								IComponent<GameObject>.AddPlayerMessage("Your legs ache at the joints.");
							}
							SawSore = true;
						}
					}
					if (Stage <= -2)
					{
						if (SawSore && base.Object.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("You feel better.");
						}
						base.Duration = 0;
						return true;
					}
					if (Stage >= 3 || Days >= 5)
					{
						base.Duration = 0;
						base.Object.ApplyEffect(new Ironshank());
					}
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			LiquidVolume liquidVolume = E.GetGameObjectParameter("Container").LiquidVolume;
			if ((liquidVolume.ComponentLiquids.ContainsKey("honey") || liquidVolume.ComponentLiquids.ContainsKey("gel")) && Bonus < 2)
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
