using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SecretRevealer : IPart
{
	public string id;

	public string text;

	public string message;

	public string adjectives;

	public string category;

	public string extraprepopup;

	public bool revealed;

	public bool location = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (!revealed)
		{
			revealed = true;
			JournalMapNote note = JournalAPI.GetMapNote(id);
			if (note != null)
			{
				if (!note.revealed)
				{
					Popup.Show(message);
					if (location)
					{
						The.Game.Systems.ForEach(delegate(IGameSystem s)
						{
							s.LocationDiscovered(note.text);
						});
					}
					JournalAPI.RevealMapNote(note);
				}
			}
			else
			{
				Popup.Show(message);
				JournalAPI.AddMapNote(ParentObject.GetCurrentCell().ParentZone.ZoneID, text, category, adjectives.Split(','), id, revealed: true, sold: false, -1L);
				if (location)
				{
					The.Game.Systems.ForEach(delegate(IGameSystem s)
					{
						s.LocationDiscovered(note.text);
					});
				}
			}
		}
		return base.HandleEvent(E);
	}
}
