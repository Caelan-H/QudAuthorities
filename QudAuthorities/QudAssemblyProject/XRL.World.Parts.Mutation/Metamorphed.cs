using System;
using XRL.Core;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Metamorphed : Effect
{
	public Guid AAID;

	public int XPAwarded;

	public GameObject OriginalBody;

	public Metamorphed()
	{
		base.DisplayName = "metamorphed";
	}

	public Metamorphed(GameObject OriginalBody, int Duration)
		: this()
	{
		this.OriginalBody = OriginalBody;
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 1024;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AwardedXPEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardedXPEvent E)
	{
		XPAwarded += E.Amount;
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Assuming another creature's form.";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "CommandEndMetamorphosis");
		Object.RegisterEffectEvent(this, "EndAction");
		AAID = AddMyActivatedAbility("End Metamorphosis", "CommandEndMetamorphosis", "Physical Mutation", null, "\u0002");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "CommandEndMetamorphosis");
		Object.UnregisterEffectEvent(this, "EndAction");
		RemoveMyActivatedAbility(ref AAID);
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandEndMetamorphosis")
		{
			Cell cell = base.Object.pPhysics.CurrentCell;
			cell.RemoveObject(base.Object);
			cell.AddObject(OriginalBody);
			XRLCore.Core.Game.Player.Body = OriginalBody;
			Metamorphosis.TransferInventory(base.Object, OriginalBody, bTagLastEquipped: false);
			base.Object.MakeInactive();
			OriginalBody.MakeActive();
			OriginalBody.AwardXP(XPAwarded);
		}
		else if (E.ID == "EndAction" && base.Duration <= 0)
		{
			base.Object.FireEvent("CommandEndMetamorphosis");
		}
		return base.FireEvent(E);
	}
}
