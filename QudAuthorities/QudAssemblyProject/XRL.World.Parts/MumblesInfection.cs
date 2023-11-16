using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class MumblesInfection : IPart
{
	public int Chance = 5;

	[NonSerialized]
	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public override void LoadData(SerializationReader Reader)
	{
		Visited = Reader.ReadDictionary<string, bool>();
		base.LoadData(Reader);
	}

	public override void SaveData(SerializationWriter Writer)
	{
		Writer.Write(Visited);
		base.SaveData(Writer);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "EnteredCell");
		Object.RegisterPartEvent(this, "Equipped");
		Object.RegisterPartEvent(this, "Unequipped");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (ParentObject.pPhysics.Equipped != null && ParentObject.pPhysics.Equipped.IsPlayer())
			{
				string zoneID = IComponent<GameObject>.ThePlayer.pPhysics.CurrentCell.ParentZone.ZoneID;
				if (!IComponent<GameObject>.ThePlayer.pPhysics.CurrentCell.ParentZone.IsWorldMap())
				{
					if (Visited.ContainsKey(zoneID))
					{
						return true;
					}
					Visited.Add(zoneID, value: true);
					if (Stat.Random(0, 100) <= Chance)
					{
						IBaseJournalEntry randomUnrevealedNote = JournalAPI.GetRandomUnrevealedNote();
						JournalMapNote obj = randomUnrevealedNote as JournalMapNote;
						string text = "";
						text = ((obj == null) ? randomUnrevealedNote.text : ("The location of " + Grammar.InitLowerIfArticle(randomUnrevealedNote.text)));
						Popup.Show("The mouths on your skin begin to mumble coherently, revealing the wisdom of a trillion microbes:\n\n" + text);
						randomUnrevealedNote.Reveal();
						AchievementManager.SetAchievement("ACH_LEARN_SECRET_FROM_MUMBLEMOUTH");
					}
				}
			}
		}
		else if (E.ID == "Equipped")
		{
			E.GetGameObjectParameter("EquippingObject").RegisterPartEvent(this, "EnteredCell");
		}
		else if (E.ID == "Unequipped")
		{
			E.GetGameObjectParameter("UnequippingObject").UnregisterPartEvent(this, "EnteredCell");
		}
		return base.FireEvent(E);
	}
}
