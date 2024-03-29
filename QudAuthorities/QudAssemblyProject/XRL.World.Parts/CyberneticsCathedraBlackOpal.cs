using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraBlackOpal : CyberneticsCathedra
{
	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		ActivatedAbilityID = Object.AddActivatedAbility("Wormhole", "CommandActivateCathedra", "Cybernetics", null, "\u0015", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
	}

	public override void Activate(GameObject Actor)
	{
		Cell destinationCellFor = SpaceTimeVortex.GetDestinationCellFor(SpaceTimeVortex.GetRandomDestinationZoneID(Actor.CurrentZone?.GetZoneWorld()), Actor);
		if (SpaceTimeVortex.Teleport(Actor, destinationCellFor, ParentObject))
		{
			IComponent<GameObject>.XDidY(Actor, "leap", "through a wormhole", "!");
		}
		base.Activate(Actor);
	}
}
