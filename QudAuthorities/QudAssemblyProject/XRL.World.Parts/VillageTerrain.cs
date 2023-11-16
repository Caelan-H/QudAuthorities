using System;
using HistoryKit;
using Qud.API;
using XRL.Core;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class VillageTerrain : IPart
{
	public string secretId;

	public bool revealed;

	public HistoricEntity village;

	public VillageTerrain()
	{
	}

	public VillageTerrain(HistoricEntity village)
	{
		this.village = village;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.SetIntProperty("ForceMutableSave", 1);
		Object.RegisterPartEvent(this, "VillageReveal");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageReveal")
		{
			if (revealed)
			{
				return false;
			}
			revealed = true;
			History sultanHistory = XRLCore.Core.Game.sultanHistory;
			HistoricEntitySnapshot currentSnapshot = village.GetCurrentSnapshot();
			string tag = ParentObject.GetTag("AlternateTerrainName", ParentObject.GetTag("Terrain"));
			string text = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
			string text2;
			do
			{
				text2 = HistoricStringExpander.ExpandString("<spice.history.regions.terrain." + tag + ".over.!random>", currentSnapshot, sultanHistory);
			}
			while (text.Equals(text2) && !string.IsNullOrEmpty(text));
			if (If.d100(30))
			{
				text = text + " and " + text2;
			}
			string newValue = ((currentSnapshot.GetList("sacredThings").Count > 0) ? currentSnapshot.GetList("sacredThings").GetRandomElement() : currentSnapshot.GetProperty("defaultSacredThing"));
			string newValue2 = ((currentSnapshot.GetList("profaneThings").Count > 0) ? currentSnapshot.GetList("profaneThings").GetRandomElement() : currentSnapshot.GetProperty("defaultProfaneThing"));
			ParentObject.GetPart<Description>().Short = Grammar.InitCap(HistoricStringExpander.ExpandString("<spice.villages.description.!random>.").Replace("*terrainFragment*", text).Replace("*sacredThing*", newValue)
				.Replace("*profaneThing*", newValue2)
				.Replace("*faction*", Faction.getFormattedName(currentSnapshot.GetProperty("baseFaction"))));
			ParentObject.pRender.Tile = "Terrain/sw_joppa.bmp";
			ParentObject.pRender.ColorString = "&" + Crayons.GetRandomColorAll();
			ParentObject.pRender.DisplayName = Grammar.MakeTitleCase(currentSnapshot.GetProperty("name"));
			ParentObject.HasProperName = true;
			ParentObject.SetStringProperty("IndefiniteArticle", "");
			ParentObject.SetStringProperty("DefiniteArticle", "");
			ParentObject.SetStringProperty("OverrideIArticle", "");
			ParentObject.SetStringProperty("Gender", "nonspecific");
			do
			{
				ParentObject.pRender.DetailColor = Crayons.GetRandomColorAll();
			}
			while (ParentObject.pRender.DetailColor == ParentObject.pRender.ColorString.Substring(1));
			ParentObject.pRender.RenderString = "#";
			ParentObject.GetPart<TerrainTravel>()?.Encounters.Clear();
			ParentObject.SetStringProperty("OverlayColor", "&W");
			if (secretId != null)
			{
				JournalMapNote mapNote = JournalAPI.GetMapNote(secretId);
				if (!mapNote.revealed)
				{
					mapNote.Reveal();
				}
			}
			return true;
		}
		return base.FireEvent(E);
	}
}
