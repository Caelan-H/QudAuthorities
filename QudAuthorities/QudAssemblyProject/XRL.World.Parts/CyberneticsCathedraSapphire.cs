using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCathedraSapphire : CyberneticsCathedra
{
	public override void OnImplanted(GameObject Object)
	{
		base.OnImplanted(Object);
		ActivatedAbilityID = Object.AddActivatedAbility("Stunning Force", "CommandActivateCathedra", "Cybernetics", null, "#");
	}

	public override void Activate(GameObject Actor)
	{
		IComponent<GameObject>.XDidY(Actor, "invoke", "a wave of concussive force", "!");
		StunningForce.Concussion(Actor.CurrentCell, Actor, GetLevel(Actor), 3, Actor.GetPhase());
		base.Activate(Actor);
	}
}
