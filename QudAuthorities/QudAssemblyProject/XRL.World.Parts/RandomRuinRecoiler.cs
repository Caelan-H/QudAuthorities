using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RandomRuinRecoiler : IPart
{
	public string additionalDescription;

	[NonSerialized]
	private int NameCacheTick;

	[NonSerialized]
	private string NameCache;

	public override bool SameAs(IPart p)
	{
		if ((p as RandomRuinRecoiler).additionalDescription != additionalDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetDisplayNameEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			if (The.ZoneManager != null && (NameCache == null || The.ZoneManager.NameUpdateTick > NameCacheTick))
			{
				Teleporter part = ParentObject.GetPart<Teleporter>();
				if (part != null)
				{
					string destinationZone = part.DestinationZone;
					if (!string.IsNullOrEmpty(destinationZone))
					{
						string text = The.ZoneManager.GetZoneReferenceDisplayName(destinationZone);
						if (!string.IsNullOrEmpty(text))
						{
							text += " ";
						}
						NameCache = text;
					}
				}
				NameCacheTick = The.ZoneManager.NameUpdateTick;
			}
			if (!string.IsNullOrEmpty(NameCache))
			{
				string text2 = E.DB.PrimaryBase;
				int num = 10;
				if (text2 == null)
				{
					text2 = "recoiler";
				}
				else
				{
					num = E.DB[text2];
					E.DB.Remove(text2);
				}
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(NameCache);
				if (text2.StartsWith("random-point "))
				{
					stringBuilder.Append(text2.Substring(13));
				}
				else
				{
					stringBuilder.Append(text2);
				}
				E.AddBase(stringBuilder.ToString(), num - 10);
			}
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(additionalDescription))
		{
			E.Postfix.Append(additionalDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (E.Context != "Sample" && E.Context != "Initialization" && The.Game != null)
		{
			List<JournalMapNote> mapNotes = JournalAPI.GetMapNotes((JournalMapNote note) => note.Has("ruins") || note.Has("historic"));
			if (mapNotes.Count > 0)
			{
				JournalMapNote randomElement = mapNotes.GetRandomElement();
				Teleporter teleporter = ParentObject.RequirePart<Teleporter>();
				teleporter.DestinationZone = randomElement.zoneid;
				teleporter.DestinationX = -1;
				teleporter.DestinationY = -1;
			}
		}
		return base.HandleEvent(E);
	}
}
