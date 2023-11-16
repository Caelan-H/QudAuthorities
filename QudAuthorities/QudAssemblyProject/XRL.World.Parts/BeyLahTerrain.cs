using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class BeyLahTerrain : IPart
{
	public string secretId = "$beylah";

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Object.RegisterPartEvent(this, "BeyLahReveal");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeyLahReveal")
		{
			ParentObject.pRender.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.pRender.ColorString = "&W";
			ParentObject.pRender.DisplayName = "Bey Lah";
			ParentObject.pRender.DetailColor = "r";
			ParentObject.pRender.RenderString = "#";
			ParentObject.HasProperName = true;
			ParentObject.GetPart<Description>().Short = "At the center of a particularly thick copse, the vegetation clears. Flower-bedecked huts huddle in the clearing within, surrounded by phalanxes of tidy watervine rows and carefully-tended lah.";
			ParentObject.GetPart<TerrainTravel>()?.Encounters.Clear();
			ParentObject.SetStringProperty("OverlayColor", "&W");
			if (secretId != null)
			{
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (mapNote != null && !mapNote.revealed)
				{
					mapNote.Reveal();
				}
			}
		}
		return base.FireEvent(E);
	}
}
