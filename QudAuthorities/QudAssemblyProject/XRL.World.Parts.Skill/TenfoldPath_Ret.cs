using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Skill;

[Serializable]
public class TenfoldPath_Ret : BaseSkill
{
	public const int SHAKE_OFF = 25;

	public const int MENTAL_ACTION_REDUCTION = 50;

	private static List<Effect> targetEffects = new List<Effect>();

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ApplyEffectEvent.ID && ID != EndTurnEvent.ID)
		{
			return ID == GetEnergyCostEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetEnergyCostEvent E)
	{
		if (E.Type != null && E.Type.Contains("Mental"))
		{
			E.PercentageReduction += 50;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (AffectEffect(E.Effect) && 25.in100() && ParentObject.IsPlayer())
		{
			if (E.Effect.ClassName == E.Effect.DisplayName)
			{
				IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off the effect!", 'g');
			}
			else
			{
				IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off being " + E.Effect.DisplayName + "!", 'g');
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ParentObject.Effects != null && 25.in100())
		{
			targetEffects.Clear();
			int i = 0;
			for (int count = ParentObject.Effects.Count; i < count; i++)
			{
				if (AffectEffect(ParentObject.Effects[i]))
				{
					targetEffects.Add(ParentObject.Effects[i]);
				}
			}
			if (targetEffects.Count > 0)
			{
				Effect randomElement = targetEffects.GetRandomElement();
				if (randomElement != null)
				{
					if (ParentObject.IsPlayer())
					{
						if (randomElement.DisplayName == randomElement.ClassName)
						{
							IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off a mental state!", 'g');
						}
						else
						{
							IComponent<GameObject>.AddPlayerMessage("A supernal force helps you shake off being " + randomElement.DisplayName + "!", 'g');
						}
					}
					ParentObject.RemoveEffect(randomElement);
				}
			}
			targetEffects.Clear();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool AffectEffect(Effect FX)
	{
		if (FX.IsOfTypes(100663298))
		{
			return !FX.IsOfType(134217728);
		}
		return false;
	}
}
