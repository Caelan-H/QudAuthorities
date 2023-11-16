using System;
using Qud.API;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class HydroponSurface : IPart
{
	public string RegionName = "Hydropon";

	public string RevealString;

	public string RevealSecret = "$hydropon";

	public string RevealKey = "Hydropon_LocationKnown";

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
			ZoneManager.instance.GetZone("JoppaWorld").BroadcastEvent("HydroponReveal");
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.revealed)
			{
				JournalAPI.AddAccomplishment("You discovered the Hydropon.", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered the Hydropon.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				JournalAPI.RevealMapNote(mapNote);
			}
		}
		return base.FireEvent(E);
	}
}
