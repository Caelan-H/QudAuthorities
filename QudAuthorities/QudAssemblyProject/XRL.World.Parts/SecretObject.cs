using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class SecretObject : IPart
{
	public string id = "";

	public string text;

	public string message;

	public string adjectives;

	public string category;

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		if (id == "")
		{
			id = Guid.NewGuid().ToString();
		}
		base.Register(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (!revealed && IComponent<GameObject>.ThePlayer.GetConfusion() <= 0)
		{
			revealed = true;
			JournalMapNote mapNote = JournalAPI.GetMapNote(id);
			if (mapNote != null)
			{
				if (mapNote.revealed)
				{
					return true;
				}
				JournalAPI.RevealMapNote(mapNote);
			}
			else
			{
				IBaseJournalEntry.DisplayMessage(message);
				JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, text, category, adjectives.Split(','), id, revealed: true, sold: false, -1L);
			}
		}
		return base.Render(E);
	}
}
