using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.World.Encounters.EncounterObjectBuilders;

namespace XRL.World.Parts;

[Serializable]
public class DromadCaravan : IPart
{
	public bool Seen;

	public bool Created;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == GetPointsOfInterestEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (Seen && IsWorkingCaravan() && E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Created)
		{
			try
			{
				Created = true;
				string value = "DromadMerchant_" + ParentObject.CurrentZone.ZoneID;
				ParentObject.SetStringProperty("nosecret", value);
				List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells(4);
				List<Cell> list = new List<Cell>();
				foreach (Cell item in adjacentCells)
				{
					if (item.IsEmpty())
					{
						list.Add(item);
					}
				}
				List<string> list2 = new List<string>();
				int num = Stat.Random(1, 3);
				for (int i = 0; i < num; i++)
				{
					list2.Add("Great Saltback");
				}
				int num2 = Stat.Random(2, 4);
				for (int j = 0; j < num2; j++)
				{
					list2.Add("Caravan Guard");
				}
				Tier1HumanoidEquipment tier1HumanoidEquipment = new Tier1HumanoidEquipment();
				for (int k = 0; k < list2.Count; k++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObject.create(list2[k]);
					tier1HumanoidEquipment.BuildObject(gameObject);
					gameObject.PartyLeader = ParentObject;
					Cell randomElement = list.GetRandomElement();
					randomElement.AddObject(gameObject);
					gameObject.MakeActive();
					list.Remove(randomElement);
				}
			}
			catch
			{
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (!Seen)
		{
			Seen = true;
			if (IsWorkingCaravan())
			{
				string secretId = "DromadMerchant_" + ParentObject.CurrentZone.ZoneID;
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (mapNote != null)
				{
					if (!mapNote.revealed)
					{
						IBaseJournalEntry.DisplayMessage("You spot a {{w|dromad}} caravan.");
						JournalAPI.RevealMapNote(mapNote);
					}
				}
				else
				{
					IBaseJournalEntry.DisplayMessage("You spot a {{w|dromad}} caravan.");
					JournalAPI.AddMapNote(ParentObject.CurrentZone.ZoneID, "A dromad caravan", "Merchants", new string[2] { "dromad", "merchant" }, secretId, revealed: true, sold: false, -1L);
				}
			}
		}
		return base.Render(E);
	}

	public bool IsWorkingCaravan()
	{
		if (ParentObject.HasPart("PsychicThrall"))
		{
			return false;
		}
		if (ParentObject.HasPart("DomesticatedSlave"))
		{
			return false;
		}
		if (ParentObject.IsHostileTowards(The.Player))
		{
			return false;
		}
		if (ParentObject.IsPlayerControlled())
		{
			return false;
		}
		return true;
	}
}
