using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SecretObjectLook : IPart
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
		Object.RegisterPartEvent(this, "AfterLookedAt");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (!revealed && E.ID == "AfterLookedAt")
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
				Popup.Show(message);
				JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, text, category, adjectives.Split(','), id, revealed: true, sold: false, -1L);
			}
		}
		return base.FireEvent(E);
	}
}
