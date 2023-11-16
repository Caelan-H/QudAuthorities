using System;
using System.Collections.Generic;
using Genkit;

namespace XRL.World;

[Serializable]
public class DynamicQuestDeliveryTarget
{
	public string type;

	public string zoneId;

	public Location2D location;

	public string displayName;

	public string secretId;

	public List<string> attributes;

	public List<string> factions;
}
