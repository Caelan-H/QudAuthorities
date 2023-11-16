using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class BeyLahSurface : IPart
{
	public string RegionName = "Bey Lah";

	public string RevealString;

	public string RevealSecret = "$beylah";

	public string RevealKey = "BeyLah_LocationKnown";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && !XRLCore.Core.Game.HasIntGameState(RevealKey))
		{
			XRLCore.Core.Game.SetIntGameState(RevealKey, 1);
			ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("BeyLahReveal");
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.revealed)
			{
				JournalAPI.AddAccomplishment("You discovered the hidden village of Bey Lah.", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered Bey Lah, once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		return base.FireEvent(E);
	}
}
