using System;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class LocationFinder : IPart
{
	public string ID;

	public string Text;

	public int Value;

	public string Trigger;

	public LocationFinder()
	{
		Trigger = "Created";
	}

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		if (Trigger == "OnScreen")
		{
			Object.RegisterPartEvent(this, "EndTurn");
		}
		if (Trigger == "Taken")
		{
			Object.RegisterPartEvent(this, "GotNewQuest");
			Object.RegisterPartEvent(this, "Taken");
		}
		base.Register(Object);
	}

	public void TriggerFind()
	{
		if (XRLCore.Core.Game.StringGameState.ContainsKey("LairFound_" + ID))
		{
			return;
		}
		JournalMapNote mapNote = JournalAPI.GetMapNote(ID);
		XRLCore.Core.Game.StringGameState.Add("LairFound_" + ID, "1");
		XRLCore.Core.Game.IncrementIntGameState("LairsFound", 1);
		string locName;
		if (mapNote.category == "Ruins")
		{
			locName = ((mapNote.text == "some forgotten ruins") ? Grammar.InitLower(mapNote.text) : Grammar.MakeTitleCaseWithArticle(mapNote.text));
		}
		else if (mapNote.category == "Lairs")
		{
			locName = Grammar.InitLower(mapNote.text);
		}
		else
		{
			_ = mapNote.category == "Settlements";
			locName = Grammar.InitLowerIfArticle(mapNote.text);
		}
		if (!mapNote.revealed)
		{
			string text = "You discover " + locName + "!";
			Popup.ShowSpace(text);
			The.Game.Systems.ForEach(delegate(IGameSystem s)
			{
				s.LocationDiscovered(locName);
			});
			JournalAPI.AddAccomplishment(text.Replace('!', '.').Replace("discover", "discovered"), "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + locName + ", once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
			if (Value > 0)
			{
				XRLCore.Core.Game.Player.Body.AwardXP(Value);
			}
			JournalAPI.RevealMapNote(mapNote);
		}
		else if (Value > 0)
		{
			string text2 = Text.Replace("You discover", "You traveled to");
			Popup.Show(text2);
			if (Value > 0)
			{
				XRLCore.Core.Game.Player.Body.AwardXP(Value);
			}
			JournalAPI.AddAccomplishment(text2, "<spice.commonPhrases.intrepid.!random.capitalize> =name= discovered " + locName + ", once thought lost to the sands of time.", "general", JournalAccomplishment.MuralCategory.VisitsLocation, JournalAccomplishment.MuralWeight.Low, null, -1L);
		}
		The.Game.Systems.ForEach(delegate(IGameSystem s)
		{
			s.LocationDiscovered(locName);
		});
	}

	public override bool Render(RenderEvent E)
	{
		if (Trigger == "Seen" && ParentObject?.CurrentZone?.ZoneID == The.Player?.CurrentZone?.ZoneID && ParentObject?.CurrentZone?.ZoneID != null)
		{
			TriggerFind();
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == BeforeRenderEvent.ID)
			{
				return Trigger == "Created";
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (Trigger == "Created" && ParentObject?.CurrentZone?.ZoneID == The.Player?.CurrentZone?.ZoneID && ParentObject?.CurrentZone?.ZoneID != null)
		{
			TriggerFind();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (Trigger == "Taken" && (E.ID == "Taken" || E.ID == "GotNewQuest"))
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("TakingObject");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer())
			{
				TriggerFind();
			}
		}
		return base.FireEvent(E);
	}
}
