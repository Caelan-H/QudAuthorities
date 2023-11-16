using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Rusted : Effect
{
	public Rusted()
	{
		base.DisplayName = "{{r|rusted}}";
	}

	public Rusted(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117441536;
	}

	public override string GetDetails()
	{
		if (base.Object.IsCreature)
		{
			return "-70 Quickness\nSome capabilities may not function properly.";
		}
		return "Doesn't function properly and can't be equipped.\nWill erode into dust if rusted again.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.HasPart("Metal"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyRusted"))
		{
			return false;
		}
		if (Object.HasEffect("Rusted"))
		{
			if (Object.IsCreature)
			{
				return false;
			}
			GameObject gameObject = Object.Equipped ?? Object.InInventory ?? Object;
			if (IComponent<GameObject>.Visible(gameObject))
			{
				gameObject.ParticleText("*" + Object.DisplayNameOnlyStripped + " reduced to dust*", 'r');
			}
			DidX("are", "reduced to dust", "!", null, null, gameObject);
			Object.Destroy();
		}
		else
		{
			GameObject equipped = Object.Equipped;
			GameObject gameObject2 = equipped ?? Object.InInventory ?? Object;
			if (IComponent<GameObject>.Visible(gameObject2))
			{
				gameObject2.ParticleText("*" + Object.DisplayNameOnlyStripped + " rusted*", 'r');
			}
			DidX("rust", null, null, null, null, gameObject2);
			equipped?.FireEvent(Event.New("CommandUnequipObject", "BodyPart", equipped.FindEquippedObject(Object), "SemiForced", 1));
			ApplyStats();
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		UnapplyStats();
		base.Remove(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != GetDisplayNameEvent.ID && ID != IsRepairableEvent.ID)
		{
			return ID == RepairedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddTag("[{{r|rusted}}]", 20);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.01);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRepairableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(RepairedEvent E)
	{
		base.Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.RegisterEffectEvent(this, "BeginBeingEquipped");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
		Object.UnregisterEffectEvent(this, "BeginBeingEquipped");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			string text = "You can't equip " + base.Object.the + base.Object.ShortDisplayName + ", " + base.Object.itis + " badly rusted!";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.Show(text);
			}
			return false;
		}
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyStats();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyStats();
		}
		return base.FireEvent(E);
	}

	private void ApplyStats()
	{
		base.StatShifter.SetStatShift(base.Object, "Speed", -70);
	}

	private void UnapplyStats()
	{
		base.StatShifter.RemoveStatShifts(base.Object);
	}
}
