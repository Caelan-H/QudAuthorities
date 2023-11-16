using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AmbientStabilization : IPart
{
	public int Strength = 40;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EndTurnEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Stabilize();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Stabilize();
		return base.HandleEvent(E);
	}

	public void Stabilize()
	{
		if (ParentObject.InSameZone(The.Player) && !The.Player.HasEffect("AmbientRealityStabilized"))
		{
			Popup.Show("You feel some ambient astral friction here.");
			The.Player.ApplyEffect(new AmbientRealityStabilized(Strength));
		}
	}
}
