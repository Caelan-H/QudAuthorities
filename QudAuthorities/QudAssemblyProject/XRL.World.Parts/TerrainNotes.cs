using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class TerrainNotes : IPartWithPrefabImposter
{
	public bool tracked;

	public bool shown;

	[NonSerialized]
	private string descriptionCache;

	[NonSerialized]
	public List<JournalMapNote> notes;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		descriptionCache = null;
		notes = null;
		tracked = false;
		InitNotes();
		shown = false;
		foreach (JournalMapNote note in notes)
		{
			if (JournalAPI.GetCategoryMapNoteToggle(note.category))
			{
				shown = true;
				break;
			}
		}
		if (notes.Count > 0 && shown)
		{
			string value = "B";
			if (notes.Any((JournalMapNote item) => item.tracked))
			{
				tracked = true;
			}
			if (notes.Any((JournalMapNote item) => item.category == "lairs"))
			{
				value = "M";
			}
			else if (notes.Any((JournalMapNote item) => item.category == "oddities"))
			{
				value = "G";
			}
			else if (notes.Any((JournalMapNote item) => item.category == "settlements"))
			{
				value = "W";
			}
			ParentObject.SetStringProperty("OverlayDetailColor", value);
			ParentObject.SetStringProperty("OverlayTile", "assets_content_textures_text_42.bmp");
			ParentObject.SetStringProperty("OverlayRenderString", "*");
			prefabID = "Prefabs/Imposters/NoteMarker";
		}
		else
		{
			ParentObject.DeleteStringProperty("OverlayDetailColor");
			ParentObject.DeleteStringProperty("OverlayTile");
			ParentObject.DeleteStringProperty("OverlayRenderString");
			prefabID = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (descriptionCache == null)
		{
			InitNotes();
			StringBuilder stringBuilder = Event.NewStringBuilder();
			if (notes.Count > 0)
			{
				stringBuilder.AppendLine(" ");
				stringBuilder.AppendLine("&CNotes:&y");
			}
			foreach (JournalMapNote note in notes)
			{
				stringBuilder.AppendLine(note.text);
			}
			descriptionCache = stringBuilder.ToString();
		}
		if (!string.IsNullOrEmpty(descriptionCache))
		{
			E.Postfix.Append(descriptionCache);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void InitNotes()
	{
		if (notes == null)
		{
			notes = JournalAPI.GetRevealedMapNotesForWorldMapCell(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (tracked)
		{
			if (DateTime.Now.Millisecond % 500 <= 250)
			{
				E.ColorString += "&g^G";
				E.DetailColor = "G";
			}
			else
			{
				E.ColorString += "&g^g";
				E.DetailColor = "g";
			}
		}
		return base.Render(E);
	}
}
