using System;
using System.Collections.Generic;
using Wintellect.PowerCollections;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DelegateSpawner : IPart
{
	public string Faction = "";

	public DelegateSpawner()
	{
	}

	public DelegateSpawner(string _faction)
	{
		Faction = _faction;
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			Cell cell = ParentObject.pPhysics.CurrentCell;
			List<GameObjectBlueprint> factionMembers = GameObjectFactory.Factory.GetFactionMembers(Faction);
			if (factionMembers.Count == 0)
			{
				foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
				{
					if (blueprint.Name == "Diplomacy Droid")
					{
						factionMembers.Add(blueprint);
					}
				}
			}
			Algorithms.RandomShuffleInPlace(factionMembers, Stat.Rand);
			GameObject gameObject = GameObject.create(factionMembers[0].Name);
			gameObject.RequirePart<SocialRoles>().RequireRole("delegate for " + XRL.World.Faction.getFormattedName(Faction));
			gameObject.pBrain.FactionMembership.Clear();
			gameObject.pBrain.FactionMembership.Add(Faction, 100);
			gameObject.pBrain.Wanders = false;
			gameObject.pBrain.WandersRandomly = false;
			gameObject.SetIntProperty("IsDelegate", 1);
			XRLCore.Core.Game.ActionManager.AddActiveObject(gameObject);
			cell.AddObject(gameObject);
			ParentObject.Destroy();
		}
		return true;
	}
}
