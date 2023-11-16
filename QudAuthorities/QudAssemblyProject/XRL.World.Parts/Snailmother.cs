using System;

namespace XRL.World.Parts;

[Serializable]
public class Snailmother : IPart
{
	public string TrailBlueprint = "SmallSlimePuddle";

	public string SpawnBlueprint = "SnailmotherEgg";

	public string SpawnCheckBlueprint = "Ickslug";

	public string SpawnVerb = "lay";

	public int SpawnChance = 5;

	public int SpawnCheckLimit = 50;

	public bool PassAttitudes;

	public bool VillagePassAttitudes = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!string.IsNullOrEmpty(TrailBlueprint))
		{
			ParentObject.CurrentCell.AddObject(TrailBlueprint);
		}
		if (!E.Forced && !E.System && SpawnChance.in100() && ParentObject.CurrentZone.CountObjects(SpawnCheckBlueprint) < SpawnCheckLimit)
		{
			GameObject gameObject = GameObject.create(SpawnBlueprint);
			if (gameObject.GetPart("SnailmotherEgg") is SnailmotherEgg snailmotherEgg)
			{
				snailmotherEgg.SpawnedBy = ParentObject;
				if (PassAttitudes)
				{
					snailmotherEgg.AdjustAttitude = true;
				}
			}
			ParentObject.CurrentCell.AddObject(gameObject);
			DidXToY(SpawnVerb, gameObject, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "VillageInit");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit" && VillagePassAttitudes)
		{
			PassAttitudes = true;
		}
		return base.FireEvent(E);
	}
}
