using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they can't acquire new diseases for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to disease.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect("CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect"))
		{
			go.ApplyEffect(new CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect());
		}
	}
}
[Serializable]
public class CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public CookingDomainMedicinal_DiseaseImmunity_ProceduralCookingTriggeredActionEffect()
	{
		base.Duration = 300;
	}

	public override string GetDetails()
	{
		return "Immune to disease onset.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyDiseaseOnset");
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Object.UnregisterEffectEvent(this, "ApplyDiseaseOnset");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyDiseaseOnset")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
[Serializable]
public class CookingDomainMedicinal_DiseaseResistUnit : ProceduralCookingEffectUnit
{
	public override void Init(GameObject target)
	{
	}

	public override string GetDescription()
	{
		return "+3 to saves vs. disease";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "ModifyDefendingSave");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "ModifyDefendingSave" && E.GetStringParameter("Vs").Contains("Disease"))
		{
			E.SetParameter("Roll", E.GetIntParameter("Roll") + 3);
		}
	}
}
