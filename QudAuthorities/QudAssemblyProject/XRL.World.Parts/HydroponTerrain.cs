using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class HydroponTerrain : IPart
{
	public string secretId = "$hydropon";

	public bool revealed;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Object.RegisterPartEvent(this, "HydroponReveal");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "HydroponReveal")
		{
			ParentObject.pRender.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.pRender.ColorString = "&B";
			ParentObject.pRender.DisplayName = "Hydropon";
			ParentObject.pRender.DetailColor = "r";
			ParentObject.pRender.RenderString = "#";
			ParentObject.HasProperName = true;
			ParentObject.GetPart<Description>().Short = "It's the hydropon.";
			ParentObject.GetPart<TerrainTravel>()?.Encounters.Clear();
			ParentObject.SetStringProperty("OverlayColor", "&B");
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
