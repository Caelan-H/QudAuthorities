using System;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class AbstractEmbarkBuilderModuleData
{
	public string version = "1.0.0";
}
