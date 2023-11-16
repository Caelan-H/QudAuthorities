using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ModBlinkEscape : IModification
{
	public ModBlinkEscape()
	{
	}

	public ModBlinkEscape(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnEquipper = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		base.IsTechScannable = true;
		NameForStatus = "EmergencyTeleporter";
	}

	public override void ApplyModification(GameObject Object)
	{
		IncreaseDifficultyAndComplexity(3, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnCarrier)
		{
			E.Postfix.AppendRules("Whenever you're about to take avoidable damage, there's " + Grammar.A(GetActivationChance()) + "% chance you blink away instead.", base.AddStatusSummary);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (IsObjectActivePartSubject(E.Object))
		{
			CheckBlinkEscape(E.Object, E.Actor, E.Damage);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "BeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "BeforeApplyDamage");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public static int GetActivationChance(int Tier)
	{
		return 5 + Tier;
	}

	public int GetActivationChance()
	{
		return GetActivationChance(Tier);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage")
		{
			Damage damage = E.GetParameter("Damage") as Damage;
			GameObject gameObjectParameter = E.GetGameObjectParameter("Owner");
			CheckBlinkEscape(gameObjectParameter, damage);
		}
		return base.FireEvent(E);
	}

	public int CheckBlinkEscape(GameObject source, Damage damage)
	{
		int num = 0;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (CheckBlinkEscape(activePartSubject, source, damage))
				{
					num++;
				}
			}
			return num;
		}
		if (CheckBlinkEscape(GetActivePartFirstSubject(), source, damage))
		{
			num++;
		}
		return num;
	}

	public bool CheckBlinkEscape(GameObject who, GameObject source, Damage damage)
	{
		if (who == null)
		{
			return false;
		}
		if (who.OnWorldMap())
		{
			return false;
		}
		if (damage.HasAttribute("Unavoidable"))
		{
			return false;
		}
		if (!who.FireEvent("CheckRealityDistortionUsability"))
		{
			return false;
		}
		if (!GetActivationChance().in100() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		if (source != null && source.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("Fate intervenes and you deal no damage to " + ParentObject.the + ParentObject.ShortDisplayName + ".");
		}
		damage.Amount = 0;
		who.RandomTeleport(Swirl: true);
		return true;
	}
}
