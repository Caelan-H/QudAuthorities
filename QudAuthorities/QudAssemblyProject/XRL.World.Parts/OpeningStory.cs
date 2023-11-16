using System;
using Qud.API;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class OpeningStory : IPart
{
	public bool Triggered;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeTakeActionEvent.ID)
		{
			return ID == ReplicaCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (!Triggered)
		{
			bool num = The.Game.GetStringGameState("embark", "Joppa").EqualsNoCase("Joppa");
			Triggered = true;
			string text = (num ? Data.GetText("OpeningStoryJoppa") : Data.GetText("OpeningStoryAlternate"));
			if (CapabilityManager.AllowKeyboardHotkeys)
			{
				text += "\n\n<Press space, then press F1 for help.>";
			}
			string displayName = ParentObject.GetCurrentCell().ParentZone.DisplayName;
			if (!num)
			{
				The.Game.SetStringGameState("villageZeroName", displayName);
			}
			text = text.Replace("$day", Calendar.getDay());
			text = text.Replace("$month", Calendar.getMonth());
			text = text.Replace("$village", displayName);
			Popup.Show(text);
			JournalAPI.AddAccomplishment("On the " + Calendar.getDay() + " of " + Calendar.getMonth() + ", you arrived at " + displayName + ".", "On the auspicious " + Calendar.getDay() + " of " + Calendar.getMonth() + ", =name= arrived in " + displayName + " and began " + The.Player.GetPronounProvider().PossessiveAdjective + " prodigious odyssey through Qud.", "general", JournalAccomplishment.MuralCategory.IsBorn, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplicaCreatedEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.WantToRemove(this);
		}
		return base.HandleEvent(E);
	}
}
