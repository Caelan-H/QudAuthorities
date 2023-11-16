using Qud.API;
using XRL.Core;
using XRL.UI;
using XRL.World.Encounters;

namespace XRL.World.Capabilities;

public static class PsychicGlimmer
{
	public static bool Perceptible(int amount)
	{
		return amount >= PsychicManager.GLIMMER_FLOOR;
	}

	public static bool Perceptible(GameObject who)
	{
		return Perceptible(who.GetPsychicGlimmer());
	}

	public static void Update(GameObject who)
	{
		if (!who.IsPlayer() || who.HasEffect("Dominated"))
		{
			return;
		}
		int psychicGlimmer = who.GetPsychicGlimmer();
		if (psychicGlimmer >= PsychicManager.GLIMMER_FLOOR && who.GetIntProperty("LastGlimmer") < PsychicManager.GLIMMER_FLOOR)
		{
			Popup.Show("{{K|You are being watched.\n\nIt's a familiar feeling. When someone has watched you in the past, when it's light that's betrayed your presence, you made a friend of the darkness. You pulled your hat brim low over your eyes. You stepped behind the cover of a thatched wall. But those who watch you now watch in spite of such simple obstructions. Their sight isn't mediated by the rays of a gleaming star or torch but by something much older. If there are ways to conceal " + who.itself + " from these seeing eyes, if there are new kinds of darknesses to befriend, you know nothing of them.}}");
			if (!XRLCore.Core.Game.HasIntGameState("ExceededGlimmerFloor") || XRLCore.Core.Game.GetIntGameState("ExceededGlimmerFloor") != 1)
			{
				JournalAPI.AddAccomplishment("You had the feeling of being watched, and learned that there's a sight older than sight.", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= was gifted with a divine sight older than sight.", "general", JournalAccomplishment.MuralCategory.LearnsSecret, JournalAccomplishment.MuralWeight.High, null, -1L);
				AchievementManager.SetAchievement("ACH_20_GLIMMER");
			}
			if (who.IsTrueKin())
			{
				AchievementManager.SetAchievement("ACH_TRUE_GLIMMER");
			}
			XRLCore.Core.Game.SetIntGameState("ExceededGlimmerFloor", 1);
		}
		if (psychicGlimmer < PsychicManager.GLIMMER_FLOOR && who.GetIntProperty("LastGlimmer") >= PsychicManager.GLIMMER_FLOOR)
		{
			Popup.Show("{{K|You've discovered a way to conceal " + who.itself + ". For now.}}");
		}
		if (psychicGlimmer >= PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR && who.GetIntProperty("LastGlimmer") < PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			Popup.Show("{{K|What you understood to be the psychic sea was only a pond. There are other watchers now, countless in number, beyond the gulf of materiality. Points of light glimmer in all directions, but what are directions on a space that cannot be ordered? All you know now is of an aether vaster than the very mathematics that describe it. And you are not nor will you ever be again alone.}}");
			if (!XRLCore.Core.Game.HasIntGameState("ExceededGlimmerExtraFloor") || XRLCore.Core.Game.GetIntGameState("ExceededGlimmerExtraFloor") != 1)
			{
				AchievementManager.SetAchievement("ACH_40_GLIMMER");
				JournalAPI.AddAccomplishment("You learned a cosmic truth, early among truths, about the locality of space and time as you knew them and an aether vaster than both.", "In the month of " + Calendar.getMonth() + " of " + Calendar.getYear() + ", =name= saw the psychic aether for what it was and became an extradimensional being of note.", "general", JournalAccomplishment.MuralCategory.LearnsSecret, JournalAccomplishment.MuralWeight.High, null, -1L);
			}
			XRLCore.Core.Game.SetIntGameState("ExceededGlimmerExtraFloor", 1);
		}
		if (psychicGlimmer < PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR && who.GetIntProperty("LastGlimmer") >= PsychicManager.GLIMMER_EXTRADIMENSIONAL_FLOOR)
		{
			Popup.Show("{{K|You've discovered a way to conceal " + who.itself + " from extradimensional watchers. For now.}}");
		}
		if (psychicGlimmer >= 100)
		{
			AchievementManager.SetAchievement("ACH_100_GLIMMER");
		}
		if (psychicGlimmer >= 200)
		{
			AchievementManager.SetAchievement("ACH_200_GLIMMER");
		}
		who.SetIntProperty("LastGlimmer", psychicGlimmer);
	}
}
