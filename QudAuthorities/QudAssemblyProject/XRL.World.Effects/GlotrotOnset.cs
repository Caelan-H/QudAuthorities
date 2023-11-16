using System;
using XRL.Core;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class GlotrotOnset : Effect
{
	public int Stage;

	public int Bonus;

	public int Days;

	public int Count;

	public bool SawSore;

	public GlotrotOnset()
	{
		base.DisplayName = "{{r|sore throat}}";
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
		return "Throat is sore and tongue is swollen.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("Glotrot") || Object.HasEffect("GlotrotOnset"))
		{
			return false;
		}
		if (Object.FireEvent("ApplyDiseaseOnset") && ApplyEffectEvent.Check(Object, "DiseaseOnset", this) && Object.FireEvent("ApplyGlotrot") && ApplyEffectEvent.Check(Object, "Glotrot", this))
		{
			base.Duration = 1;
			return true;
		}
		return false;
	}

	public override string GetDescription()
	{
		return "{{r|sore throat}}";
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
		if (Bonus != 0 && E.Vs == "Glotrot Disease Onset")
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
		Object.RegisterEffectEvent(this, "Eating");
		Object.RegisterEffectEvent(this, "EndTurn");
		Object.RegisterEffectEvent(this, "DrinkingFrom");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "Eating");
		Object.UnregisterEffectEvent(this, "EndTurn");
		Object.UnregisterEffectEvent(this, "DrinkingFrom");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			Count++;
			if (Count >= 1200 && base.Duration > 0)
			{
				Count = 0;
				Days++;
				if (base.Object.MakeSave("Toughness", 13, null, null, "Glotrot Disease Onset"))
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
							IComponent<GameObject>.AddPlayerMessage("Your throat feels sore.");
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
				}
				else if (Stage >= 3 || Days >= 5)
				{
					base.Duration = 0;
					base.Object.ApplyEffect(new Glotrot());
				}
			}
		}
		else if (E.ID == "DrinkingFrom")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Container");
			LiquidVolume liquidVolume = gameObjectParameter.LiquidVolume;
			bool flag = false;
			if (gameObjectParameter.IsAflame())
			{
				flag = true;
				for (int i = 1; i <= 3; i++)
				{
					string stringGameState = XRLCore.Core.Game.GetStringGameState("GlotrotCure" + i);
					if (!liquidVolume.ComponentLiquids.ContainsKey(stringGameState))
					{
						flag = false;
						break;
					}
					if (i == 3 && liquidVolume.GetPrimaryLiquidID() != stringGameState)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				base.Duration = 0;
				if (base.Object.IsPlayer())
				{
					Popup.Show("It tastes even worse than you had imagined -- like a dead turtle boiled in phlegm.");
				}
			}
			else if (!liquidVolume.ComponentLiquids.ContainsKey("honey") && Bonus < 3)
			{
				Bonus = 3;
			}
		}
		else if (E.ID == "Eating")
		{
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Food");
			if (gameObjectParameter2 == null)
			{
				return true;
			}
			if (Bonus > 0)
			{
				return true;
			}
			if (gameObjectParameter2.Blueprint.Contains("Yuckwheat") && Bonus < 3)
			{
				Bonus = 3;
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
