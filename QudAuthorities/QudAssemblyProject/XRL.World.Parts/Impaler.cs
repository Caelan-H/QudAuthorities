using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Impaler : IPart
{
	public string ClusterSize = "1";

	public string Damage = "1d10";

	public string BleedDamage = "1d2";

	public string AlternateBlueprint = "Luminous Hoarshroom";

	public string Message = "{{R|=subject.A= =verb:strike==subject.directionIfAny=!}}";

	public string DamageAttributes = "Stabbing";

	public string DamageMessage = "from %t impalement.";

	public string FloatMessage = "*impaled*";

	public int AlternateBlueprintChance = 15;

	public int BleedSave = 20;

	public bool RequiresHostility = true;

	public bool NotIfAllied = true;

	public bool DestroyOnStrike;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetNavigationWeightEvent.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (E.Smart && !ParentObject.HasPart("Hidden"))
		{
			E.Uncacheable = true;
			if (E.Actor.IsCombatObject() && (ParentObject.IsHostileTowards(E.Actor) || (!RequiresHostility && (!NotIfAllied || !ParentObject.IsAlliedTowards(E.Actor)))) && ParentObject.PhaseAndFlightMatches(E.Actor))
			{
				E.MinWeight(99);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (HasOtherImpaler(E.Cell) && !ParentObject.HasPart("Combat"))
		{
			ParentObject.Obliterate();
			return false;
		}
		if (ClusterSize != "1")
		{
			List<Cell> list = new List<Cell>(ParentObject.CurrentCell.GetLocalEmptyAdjacentCells()).ShuffleInPlace();
			int num = ClusterSize.RollCached();
			for (int i = 0; i < num && i < list.Count; i++)
			{
				Cell cell = list[i];
				if (HasOtherImpaler(cell) || (!string.IsNullOrEmpty(AlternateBlueprint) && cell.HasObject(AlternateBlueprint)))
				{
					num++;
					continue;
				}
				if (AlternateBlueprintChance.in100())
				{
					cell.AddObject(AlternateBlueprint);
					continue;
				}
				GameObject gameObject = GameObject.create(ParentObject.Blueprint);
				gameObject.TakeOnAttitudesOf(ParentObject, CopyLeader: true);
				(gameObject.GetPart("Impaler") as Impaler).ClusterSize = "1";
				cell.AddObject(gameObject);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.IsCombatObject())
		{
			if (ParentObject.IsHostileTowards(E.Object) || (!RequiresHostility && (!NotIfAllied || !ParentObject.IsAlliedTowards(E.Object))))
			{
				if (ParentObject.PhaseAndFlightMatches(E.Object))
				{
					Hidden hidden = ParentObject.GetPart("Hidden") as Hidden;
					if (hidden == null || !hidden.Found)
					{
						if (E.Object.IsVisible())
						{
							string text = GameText.VariableReplace(Message, ParentObject, null, ExplicitSubjectPlural: false, E.Object);
							if (!string.IsNullOrEmpty(text))
							{
								IComponent<GameObject>.AddPlayerMessage(text);
							}
							string text2 = GameText.VariableReplace(FloatMessage, ParentObject, null, ExplicitSubjectPlural: false, E.Object);
							if (!string.IsNullOrEmpty(text2))
							{
								E.Object.ParticleText(text2, IComponent<GameObject>.ConsequentialColorChar(null, E.Object));
							}
							hidden?.Reveal(Silent: true);
						}
						if (E.Object.TakeDamage(Damage.RollCached(), Attributes: DamageAttributes, Owner: ParentObject, Message: DamageMessage) && E.Object.IsValid() && !E.Object.IsInGraveyard())
						{
							E.Object.Bloodsplatter();
							E.Object.ApplyEffect(new Bleeding(BleedDamage, BleedSave, ParentObject, Stack: false));
						}
						if (DestroyOnStrike)
						{
							ParentObject.Destroy();
						}
					}
				}
			}
			else if (E.Object.IsPlayer())
			{
				Reveal();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool HasOtherImpaler(Cell C)
	{
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			if (C.Objects[i] != ParentObject && C.Objects[i].HasPart("Impaler"))
			{
				return true;
			}
		}
		return false;
	}
}
