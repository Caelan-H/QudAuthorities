using System;

namespace XRL.World.Parts;

[Serializable]
public class EncounterEntry
{
	public string Text = "You notice some strange ruins nearby. Do you want to investigate?";

	public string Zone = "";

	public string secretID = "";

	public string ReplacementText = "";

	public bool Optional = true;

	public bool Enabled = true;

	public EncounterEntry()
	{
	}

	public EncounterEntry(string _Text, string _Zone, string _Replacement, string secret, bool _Optional)
	{
		Text = _Text;
		Zone = _Zone;
		ReplacementText = _Replacement;
		Optional = _Optional;
		secretID = secret;
	}
}
