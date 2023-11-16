using System;
using XRL.UI;

namespace XRL.World.Effects;

[Serializable]
public class AmbientRealityStabilized : Effect
{
	public int Strength;

	public int Visibility = 2;

	public bool Projective;

	public GameObject Owner;

	public AmbientRealityStabilized()
	{
		base.Duration = 1;
		base.DisplayName = "{{Y|astral friction}}";
	}

	public AmbientRealityStabilized(int Strength)
		: this()
	{
		this.Strength = Strength;
	}

	public AmbientRealityStabilized(int Strength, GameObject Owner)
		: this(Strength)
	{
		this.Owner = Owner;
	}

	public override int GetEffectType()
	{
		return 16777280;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EndTurnEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Zone currentZone = base.Object.CurrentZone;
		if (currentZone != null && !currentZone.GetCell(0, 0).HasObject("AmbientStabilization") && base.Duration > 0)
		{
			if (base.Object.IsPlayer())
			{
				Popup.Show("The astral friction diffuses.");
			}
			RealityStabilized realityStabilized = base.Object.GetEffectByClassName("RealityStabilized") as RealityStabilized;
			base.Duration = 0;
			base.Object.RemoveEffect(this);
			realityStabilized?.Maintain();
			return false;
		}
		ApplyRealityStabilization();
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect("AmbientRealityStabilized"))
		{
			return false;
		}
		if (!Object.FireEvent("ApplyAmbientRealityStabilized"))
		{
			return false;
		}
		return ApplyRealityStabilization(Initial: true);
	}

	public bool ApplyRealityStabilization(bool Initial = false)
	{
		if (!(base.Object.GetEffectByClassName("RealityStabilized") is RealityStabilized realityStabilized))
		{
			RealityStabilized e = new RealityStabilized(Strength, Owner);
			if (!base.Object.ForceApplyEffect(e))
			{
				return false;
			}
		}
		else if (Initial)
		{
			realityStabilized.SynchronizeEffect();
		}
		return true;
	}
}
