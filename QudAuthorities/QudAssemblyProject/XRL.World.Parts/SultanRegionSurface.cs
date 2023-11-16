using System;
using Qud.API;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class SultanRegionSurface : IPart
{
	public string RegionName;

	public string RevealString;

	public string RevealSecret;

	public string RevealKey;

	public Vector2i RevealLocation;

	public int region = -1;

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
		if (E.ID == "EnteredCell")
		{
			if (XRLCore.Core.Game.HasQuest("Visit " + RegionName))
			{
				XRLCore.Core.Game.CompleteQuest("Visit " + RegionName);
				if (XRLCore.Core.Game.GetIntGameState("Visited_" + RegionName) != 1)
				{
					JournalAPI.AddAccomplishment("You visited the historic site of " + RegionName + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + region + ", once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
				}
				XRLCore.Core.Game.SetIntGameState("Visited_" + RegionName, 1);
			}
			else if (XRLCore.Core.Game.GetIntGameState("Visited_" + RegionName) != 1)
			{
				int newTier = ParentObject.CurrentZone.NewTier;
				if (XRLCore.Core.Game.Player.Body != null)
				{
					XRLCore.Core.Game.Player.Body.AwardXP(250 * newTier);
				}
				XRLCore.Core.Game.SetIntGameState("Visited_" + RegionName, 1);
				JournalAPI.AddAccomplishment("You visited the historic site of " + RegionName + ".", "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + region + ", once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			}
			if (XRLCore.Core.Game.HasIntGameState(RevealKey))
			{
				return true;
			}
			XRLCore.Core.Game.SetIntGameState(RevealKey, 1);
			if (RevealLocation != null && XRLCore.Core.Game.ZoneManager.GetZone("JoppaWorld").GetCell(RevealLocation.x, RevealLocation.y).FireEvent("SultanReveal"))
			{
				if (RevealString != null)
				{
					Popup.Show(RevealString);
				}
				The.Game.Systems.ForEach(delegate(IGameSystem s)
				{
					s.LocationDiscovered(RegionName);
				});
			}
			JournalMapNote mapNote = JournalAPI.GetMapNote(RevealSecret);
			if (mapNote != null && !mapNote.revealed)
			{
				JournalAPI.RevealMapNote(mapNote);
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
