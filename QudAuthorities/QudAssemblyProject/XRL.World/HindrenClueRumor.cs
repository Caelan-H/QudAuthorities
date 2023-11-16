using System;
using Qud.API;

namespace XRL.World;

[Serializable]
public class HindrenClueRumor
{
	public string villagerCategory;

	public string text;

	public string secret;

	public void trigger()
	{
		JournalAPI.RevealObservation(secret, onlyIfNotRevealed: true);
	}
}
