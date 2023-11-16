using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class LootOnStep : IPart
{
	public string SuccessMessage = "=subject.T==subject.directionIfAny= =verb:flex= and =verb:splinter= apart, revealing =object.a=.";

	public string FailMessage = "=subject.T==subject.directionIfAny= =verb:flex= and =verb:splinter= apart.";

	public int MaxWeight = 150;

	public int Chance = 100;

	public bool RequiresAllied = true;

	public bool NotIfHostile = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (E.Object.IsPlayerControlled() && (ParentObject.IsAlliedTowards(E.Object) || (!RequiresAllied && (!NotIfHostile || !ParentObject.IsHostileTowards(E.Object)))) && ParentObject.PhaseAndFlightMatches(E.Object))
		{
			if (Chance.in100())
			{
				GameObject anItem = EncountersAPI.GetAnItem(IsSuitableLoot);
				if (anItem != null)
				{
					ParentObject.EmitMessage(SuccessMessage, anItem, null, UsePopup: true);
					SteppedOn(E.Object, E.Forced || E.Dragging != null);
					E.Cell.AddObject(anItem);
				}
			}
			else
			{
				ParentObject.EmitMessage(FailMessage, E.Object);
				SteppedOn(E.Object, E.Forced || E.Dragging != null);
			}
		}
		return base.HandleEvent(E);
	}

	public void SteppedOn(GameObject By, bool Accidental = false)
	{
		ParentObject.Die(By, null, "You were stepped on.", ParentObject.It + ParentObject.GetVerb("were") + " @@stepped on.", Accidental, null, null, Force: false, "");
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool IsSuitableLoot(GameObjectBlueprint BP)
	{
		if (MaxWeight > 0)
		{
			string partParameter = BP.GetPartParameter("Physics", "Weight");
			if (string.IsNullOrEmpty(partParameter) || !int.TryParse(partParameter, out var result) || result > MaxWeight)
			{
				return false;
			}
		}
		return true;
	}
}
