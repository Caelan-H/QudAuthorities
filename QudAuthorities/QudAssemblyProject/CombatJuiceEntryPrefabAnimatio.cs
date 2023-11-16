using UnityEngine;

public class CombatJuiceEntryPrefabAnimation : CombatJuiceEntry
{
	public Vector3 location;

	public string animation;

	public CombatJuiceEntryPrefabAnimation(Vector3 location, string animation)
	{
		this.location = location;
		this.animation = animation;
	}

	public override void start()
	{
		CombatJuice._playPrefabAnimation(location, animation);
	}
}
