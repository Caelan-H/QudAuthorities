using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class Broken : Effect
{
	public bool FromDamage;

	public bool FromExamine;

	public bool FromOverload;

	[FieldSaveVersion(251)]
	public bool FromModding;

	public Broken()
	{
		base.DisplayName = "{{r|broken}}";
		base.Duration = 1;
	}

	public Broken(bool FromDamage = false, bool FromExamine = false, bool FromOverload = false, bool FromModding = false)
		: this()
	{
		this.FromDamage = FromDamage;
		this.FromExamine = FromExamine;
		this.FromOverload = FromOverload;
		this.FromModding = FromModding;
	}

	public override int GetEffectType()
	{
		return 100664320;
	}

	public override string GetDetails()
	{
		return "Can't be equipped or used.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.IsBroken())
		{
			return false;
		}
		if (!Object.HasTagOrProperty("Breakable"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyBroken"))
		{
			return false;
		}
		Object.pPhysics?.PlayWorldSound("breakage");
		GameObject gameObject = Object.Equipped ?? Object.InInventory;
		if (gameObject != null)
		{
			gameObject.ParticleText("*" + Object.ShortDisplayNameStripped + " broken*", IComponent<GameObject>.ConsequentialColorChar(null, gameObject));
			Event @event = Event.New("CommandUnequipObject");
			@event.SetParameter("BodyPart", gameObject.FindEquippedObject(Object));
			@event.SetFlag("SemiForced", State: true);
			gameObject.FireEvent(@event);
		}
		else
		{
			Object.ParticleText("*" + Object.ShortDisplayNameStripped + " broken*", 'R');
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != BeginTakeActionEvent.ID && ID != GetDisplayNameEvent.ID && ID != IsRepairableEvent.ID)
		{
			return ID == RepairedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		E.AddTag("[{{r|broken}}]", 20);
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

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == base.Object)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginBeingEquipped");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginBeingEquipped");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginBeingEquipped")
		{
			string text = "You can't equip " + base.Object.t() + ", " + base.Object.itis + " broken!";
			if (E.GetIntParameter("AutoEquipTry") > 0)
			{
				E.SetParameter("FailureMessage", text);
			}
			else if (E.GetGameObjectParameter("Equipper").IsPlayer())
			{
				Popup.ShowFail(text);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
