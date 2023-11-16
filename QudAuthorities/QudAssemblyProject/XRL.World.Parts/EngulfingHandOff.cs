using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingHandOff : IPart
{
	public string SaveStat = "Strength";

	public string SaveDifficultyStat = "Strength";

	public int SaveTarget = 30;

	public string BleedingDamageBonus;

	public int BleedingSavePenalty;

	public override bool SameAs(IPart Part)
	{
		return false;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == BeginTakeActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (ParentObject.GetPart("Engulfing") is Engulfing engulfing)
		{
			if (engulfing.Engulfed == null)
			{
				return true;
			}
			if (!engulfing.CheckEngulfed())
			{
				return true;
			}
			if (!engulfing.Engulfed.CanBeInvoluntarilyMoved())
			{
				return true;
			}
			List<Engulfing> adjacentReceivers = GetAdjacentReceivers();
			if (adjacentReceivers.Count > 0)
			{
				AttemptHandOff(engulfing, adjacentReceivers.GetRandomElement(), engulfing.Engulfed);
			}
		}
		return base.HandleEvent(E);
	}

	public List<Engulfing> GetAdjacentReceivers()
	{
		List<Engulfing> list = new List<Engulfing>();
		ParentObject.CurrentCell?.ForeachAdjacentCell(delegate(Cell Cell)
		{
			foreach (GameObject @object in Cell.Objects)
			{
				if (@object.GetPart("Engulfing") is Engulfing engulfing)
				{
					if (engulfing.Engulfed == null)
					{
						list.Add(engulfing);
					}
					break;
				}
			}
		});
		return list;
	}

	public bool HandOffSave(GameObject Engulfed)
	{
		return Engulfed.MakeSave(SaveStat, SaveTarget, ParentObject, SaveDifficultyStat, "Engulfment Move");
	}

	public void AttemptHandOff(Engulfing Giver, Engulfing Receiver, GameObject Engulfed)
	{
		ParentObject.UseEnergy(1000, "HandOff");
		if (!HandOffSave(Engulfed) && Engulfed.GetEffect("Engulfed") is Engulfed engulfed)
		{
			Giver.Engulfed = null;
			engulfed.EngulfedBy = Receiver.ParentObject;
			Engulfed.ApplyEffect(new AnemoneEffect(BleedingDamageBonus, BleedingSavePenalty));
			Engulfed.CurrentCell.RemoveObject(Engulfed);
			Receiver.ParentObject.CurrentCell.AddObject(Engulfed);
			Receiver.Engulfed = Engulfed;
			Receiver.ParentObject.UseEnergy(1000, "HandOff");
			if (ParentObject.IsPlayer() || Engulfed.IsPlayer())
			{
				DidXToY("hand", Engulfed, "off", null, null, null, Engulfed);
			}
		}
	}
}
