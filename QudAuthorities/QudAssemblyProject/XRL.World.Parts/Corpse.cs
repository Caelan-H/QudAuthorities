using System;
using System.Collections.Generic;
using XRL.Names;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Corpse : IPart
{
	[Obsolete("save compat")]
	public GameObject Placeholder;

	public int CorpseChance;

	public string CorpseBlueprint;

	public string CorpseRequiresBodyPart;

	public int BurntCorpseChance;

	public string BurntCorpseBlueprint;

	public string BurntCorpseRequiresBodyPart;

	public int VaporizedCorpseChance;

	public string VaporizedCorpseBlueprint;

	public string VaporizedCorpseRequiresBodyPart;

	[FieldSaveVersion(240)]
	public int BuildCorpseChance = 100;

	public override bool SameAs(IPart p)
	{
		Corpse corpse = p as Corpse;
		if (corpse.CorpseChance != CorpseChance)
		{
			return false;
		}
		if (corpse.CorpseBlueprint != CorpseBlueprint)
		{
			return false;
		}
		if (corpse.CorpseRequiresBodyPart != CorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.BurntCorpseChance != BurntCorpseChance)
		{
			return false;
		}
		if (corpse.BurntCorpseBlueprint != BurntCorpseBlueprint)
		{
			return false;
		}
		if (corpse.BurntCorpseRequiresBodyPart != BurntCorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.VaporizedCorpseChance != VaporizedCorpseChance)
		{
			return false;
		}
		if (corpse.VaporizedCorpseBlueprint != VaporizedCorpseBlueprint)
		{
			return false;
		}
		if (corpse.VaporizedCorpseRequiresBodyPart != VaporizedCorpseRequiresBodyPart)
		{
			return false;
		}
		if (corpse.BuildCorpseChance != BuildCorpseChance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (ParentObject.GetIntProperty("SuppressCorpseDrops") <= 0)
		{
			ProcessCorpseDrop(E.ThirdPersonReason);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ProcessCorpseDrop(string deathReason = null)
	{
		GameObject gameObject = ParentObject.InInventory;
		if (gameObject != null && gameObject.IsCreature)
		{
			gameObject = null;
		}
		Cell cell = ((gameObject == null) ? ParentObject.GetDropCell() : null);
		if ((cell != null && !cell.ParentZone.Built && !BuildCorpseChance.in100()) || (gameObject == null && cell == null))
		{
			return;
		}
		Body body = ParentObject.Body;
		GameObject gameObject2 = null;
		if (ParentObject.pPhysics.LastDamagedByType == "Fire" || ParentObject.pPhysics.LastDamagedByType == "Light")
		{
			if (BurntCorpseChance > 0 && (string.IsNullOrEmpty(BurntCorpseRequiresBodyPart) || (body != null && body.GetFirstPart(BurntCorpseRequiresBodyPart) != null)) && BurntCorpseChance.in100())
			{
				gameObject2 = GameObject.create(BurntCorpseBlueprint);
			}
		}
		else if (ParentObject.pPhysics.LastDamagedByType == "Vaporized")
		{
			if (VaporizedCorpseChance > 0 && (string.IsNullOrEmpty(VaporizedCorpseRequiresBodyPart) || (body != null && body.GetFirstPart(VaporizedCorpseRequiresBodyPart) != null)) && VaporizedCorpseChance.in100())
			{
				gameObject2 = GameObject.create(VaporizedCorpseBlueprint);
			}
		}
		else if (CorpseChance > 0 && (string.IsNullOrEmpty(CorpseRequiresBodyPart) || (body != null && body.GetFirstPart(CorpseRequiresBodyPart) != null)) && CorpseChance.in100())
		{
			gameObject2 = GameObject.create(CorpseBlueprint);
		}
		if (gameObject2 == null)
		{
			return;
		}
		Temporary.CarryOver(ParentObject, gameObject2);
		Phase.carryOver(ParentObject, gameObject2);
		if (ParentObject.HasProperName)
		{
			gameObject2.SetStringProperty("CreatureName", ParentObject.BaseDisplayName);
		}
		else
		{
			string text = NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, null, null, FailureOkay: true);
			if (text != null)
			{
				gameObject2.SetStringProperty("CreatureName", text);
			}
		}
		if (!string.IsNullOrEmpty(deathReason))
		{
			gameObject2.SetStringProperty("DeathReason", deathReason);
		}
		if (ParentObject.HasProperty("StoredByPlayer") || ParentObject.HasProperty("FromStoredByPlayer"))
		{
			gameObject2.SetIntProperty("FromStoredByPlayer", 1);
		}
		if (gameObject != null)
		{
			gameObject.ReceiveObject(gameObject2);
		}
		else
		{
			cell.AddObject(gameObject2);
		}
		string genotype = ParentObject.GetGenotype();
		if (!string.IsNullOrEmpty(genotype))
		{
			gameObject2.SetStringProperty("FromGenotype", genotype);
		}
		if (body == null)
		{
			return;
		}
		List<GameObject> list = null;
		foreach (BodyPart part in body.GetParts())
		{
			if (part.Cybernetics != null)
			{
				if (list == null)
				{
					list = Event.NewGameObjectList();
				}
				list.Add(part.Cybernetics);
				UnimplantedEvent.Send(ParentObject, part.Cybernetics, part);
			}
		}
		if (list != null)
		{
			gameObject2.AddPart(new CyberneticsButcherableCybernetic(list));
			gameObject2.RemovePart("Food");
		}
	}
}
